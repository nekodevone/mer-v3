using AdminToys;
using MEC;
using Mirror;
using ProjectMER.Events.Handlers;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Serializable.Schematics;
using UnityEngine;
using Utf8Json;
using Logger = LabApi.Features.Console.Logger;
using Object = UnityEngine.Object;

namespace ProjectMER.Features.Objects;

public class SchematicObject : MonoBehaviour
{
	public SchematicObjectDataList SchematicData { get; private set; }

	/// <summary>
	/// Gets the schematic name.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets a schematic directory path.
	/// </summary>
	public string DirectoryPath { get; private set; }

	public IReadOnlyList<GameObject> AttachedBlocks => _attachedBlocks;

	public IReadOnlyList<NetworkIdentity> NetworkIdentities
	{
		get
		{
			if (_networkIdentities.Count > 0)
				return _networkIdentities;

			foreach (GameObject block in AttachedBlocks)
			{
				if (block.TryGetComponent(out NetworkIdentity networkIdentity))
					_networkIdentities.Add(networkIdentity);
			}

			return _networkIdentities;
		}
	}

	public IReadOnlyList<AdminToyBase> AdminToyBases
	{
		get
		{
			if (_adminToyBases.Count > 0)
				return _adminToyBases;

			foreach (NetworkIdentity netId in NetworkIdentities)
			{
				if (netId.TryGetComponent(out AdminToyBase adminToyBase))
					_adminToyBases.Add(adminToyBase);
			}

			return _adminToyBases;
		}
	}

	public AnimationController AnimationController => AnimationController.Get(this);

	public SchematicObject Init(SchematicObjectDataList data)
	{
		SchematicData = data;
		Name = Path.GetFileNameWithoutExtension(data.Path);
		DirectoryPath = data.Path;

		ObjectFromId = new Dictionary<int, Transform>(SchematicData.Blocks.Count + 1)
		{
			{ data.RootObjectId, transform },
		};

		CreateRecursiveFromID(data.RootObjectId, data.Blocks, transform);

		AddRigidbodies();
		AddAnimators();

		Schematic.OnSchematicSpawned(new(this, Name));

		return this;
	}

	private void CreateRecursiveFromID(int id, List<SchematicBlockData> blocks, Transform parentGameObject)
	{
		Transform childGameObjectTransform = CreateObject(blocks.Find(c => c.ObjectId == id), parentGameObject) ?? transform; // Create the object first before creating children.
		int[] parentSchematics = blocks.Where(bl => bl.BlockType == BlockType.Schematic).Select(bl => bl.ObjectId).ToArray();

		// Gets all the ObjectIds of all the schematic blocks inside "blocks" argument.
		foreach (SchematicBlockData block in blocks.FindAll(c => c.ParentId == id))
		{
			if (parentSchematics.Contains(block.ParentId)) // The block is a child of some schematic inside "parentSchematics" array, therefore it will be skipped to avoid spawning it and its children twice.
				continue;

			CreateRecursiveFromID(block.ObjectId, blocks, childGameObjectTransform); // The child now becomes the parent
		}
	}

	private Transform? CreateObject(SchematicBlockData block, Transform parentTransform)
	{
		if (block == null)
			return null;

		GameObject gameObject = block.Create(this, parentTransform);
		NetworkServer.Spawn(gameObject);

		_attachedBlocks.Add(gameObject);
		ObjectFromId.Add(block.ObjectId, gameObject.transform);

		if (block.BlockType != BlockType.Light && TryGetAnimatorController(block.AnimatorName, out RuntimeAnimatorController animatorController))
			_animators.Add(gameObject, animatorController);

		return gameObject.transform;
	}

	private bool TryGetAnimatorController(string animatorName, out RuntimeAnimatorController animatorController)
	{
		animatorController = null!;

		if (string.IsNullOrEmpty(animatorName))
			return false;

		Object? animatorObject = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(x => x.mainAsset.name == animatorName)?.LoadAllAssets().First(x => x is RuntimeAnimatorController);

		if (animatorObject is null)
		{
			string path = Path.Combine(DirectoryPath, animatorName);

			if (!File.Exists(path))
			{
				Logger.Warn($"{gameObject.name} block of schematic should have a {animatorName} animator attached, but the file does not exist!");
				return false;
			}

			animatorObject = AssetBundle.LoadFromFile(path).LoadAllAssets().First(x => x is RuntimeAnimatorController);
		}

		animatorController = (RuntimeAnimatorController)animatorObject;
		return true;
	}

	private bool AddAnimators()
	{
		bool isAnimated = false;
		if (!_animators.IsEmpty())
		{
			isAnimated = true;
			foreach (KeyValuePair<GameObject, RuntimeAnimatorController> pair in _animators)
				pair.Key.AddComponent<Animator>().runtimeAnimatorController = pair.Value;
		}

		_animators.Clear();
		AssetBundle.UnloadAllAssetBundles(false);
		return isAnimated;
	}

	private bool AddRigidbodies()
	{
		bool hasRigidbodies = false;
		string rigidbodyPath = Path.Combine(DirectoryPath, $"{Name}-Rigidbodies.json");
		if (!File.Exists(rigidbodyPath))
			return false;

		foreach (KeyValuePair<int, SerializableRigidbody> dict in JsonSerializer.Deserialize<Dictionary<int, SerializableRigidbody>>(File.ReadAllText(rigidbodyPath)))
		{
			if (!ObjectFromId.TryGetValue(dict.Key, out Transform transform))
				continue;

			if (!transform.gameObject.TryGetComponent(out Rigidbody rigidbody))
				rigidbody = transform.gameObject.AddComponent<Rigidbody>();

			rigidbody.isKinematic = dict.Value.IsKinematic;
			rigidbody.useGravity = dict.Value.UseGravity;
			rigidbody.constraints = dict.Value.Constraints;
			rigidbody.mass = dict.Value.Mass;

			hasRigidbodies = true;
		}

		return hasRigidbodies;
	}


	public void Destroy() => Destroy(gameObject);

	private void OnDestroy()
	{
		AnimationController.Dictionary.Remove(this);
		NetworkServer.Destroy(gameObject);
		Schematic.OnSchematicDestroyed(new(this, Name));
	}

	internal Dictionary<int, Transform> ObjectFromId = new();

	private List<GameObject> _attachedBlocks = new();
	private List<NetworkIdentity> _networkIdentities = new();
	private List<AdminToyBase> _adminToyBases = new();
	private Dictionary<GameObject, RuntimeAnimatorController> _animators = new();
}
