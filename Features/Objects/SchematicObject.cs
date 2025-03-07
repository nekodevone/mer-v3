using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Serializable.Schematics;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;
using Object = UnityEngine.Object;


namespace ProjectMER.Features.Objects;

public class SchematicObject : MapEditorObject
{
	public SchematicObjectDataList SchematicData { get; private set; }

	/// <summary>
	/// Gets a schematic directory path.
	/// </summary>
	public string DirectoryPath { get; private set; }

	internal Dictionary<int, Transform> ObjectFromId = new();

	public SchematicObject Init(SerializableSchematic serializableSchematic, SchematicObjectDataList data, string mapName, string id, Room room)
	{
		Base = serializableSchematic;
		SchematicData = data;
		DirectoryPath = data.Path;
		MapName = mapName;
		Id = id;
		Room = room;

		ObjectFromId = new Dictionary<int, Transform>(SchematicData.Blocks.Count + 1)
		{
			{ data.RootObjectId, transform },
		};

		CreateRecursiveFromID(data.RootObjectId, data.Blocks, transform);

		bool isAnimated = AddAnimators();

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

		GameObject gameObject = null!;
		RuntimeAnimatorController animatorController;

		switch (block.BlockType)
		{
			case BlockType.Empty:
				{
					gameObject = new GameObject(block.Name)
					{
						transform =
						{
							parent = parentTransform,
							localPosition = block.Position,
							localEulerAngles = block.Rotation,
							localScale = Vector3.one,
						},
					};

					break;
				}

			case BlockType.Primitive:
				{
					PrimitiveObjectToy primitive = Instantiate(PrefabManager.PrimitiveObjectPrefab, parentTransform);

					primitive.name = block.Name;
					primitive.transform.localPosition = block.Position;
					primitive.transform.localEulerAngles = block.Rotation;
					primitive.transform.localScale = block.Scale;

					primitive.NetworkPrimitiveType = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), block.Properties["PrimitiveType"].ToString());

					if (ColorUtility.TryParseHtmlString("#" + block.Properties["Color"].ToString(), out Color color))
						primitive.NetworkMaterialColor = color;

					PrimitiveFlags primitiveFlags;
					if (block.Properties.TryGetValue("PrimitiveFlags", out object flags))
					{
						primitiveFlags = (PrimitiveFlags)Enum.Parse(typeof(PrimitiveFlags), flags.ToString());
					}
					else
					{
						// Backward compatibility
						primitiveFlags = PrimitiveFlags.Visible;
						if (block.Scale.x >= 0f)
							primitiveFlags |= PrimitiveFlags.Collidable;
					}

					primitive.NetworkPrimitiveFlags = primitiveFlags;
					NetworkServer.Spawn(primitive.gameObject);

					gameObject = primitive.gameObject;
					break;
				}

			case BlockType.Light:
				{
					LightSourceToy light = Instantiate(PrefabManager.LightSourcePrefab, parentTransform);

					light.name = block.Name;
					light.transform.localPosition = block.Position;
					light.transform.localEulerAngles = block.Rotation;
					light.transform.localScale = block.Scale;

					if (ColorUtility.TryParseHtmlString("#" + block.Properties["Color"].ToString(), out Color color))
						light.NetworkLightColor = color;

					light.NetworkLightIntensity = float.Parse(block.Properties["Intensity"].ToString());
					light.NetworkLightRange = float.Parse(block.Properties["Range"].ToString());
					light.NetworkShadowType = bool.Parse(block.Properties["Shadows"].ToString()) ? LightShadows.Soft : LightShadows.None;

					NetworkServer.Spawn(light.gameObject);

					if (TryGetAnimatorController(block.AnimatorName, out animatorController))
						_animators.Add(light.gameObject, animatorController);

					gameObject = light.gameObject;
					break;

					/*
					if (Instantiate(PrefabManager.LightSourcePrefab, parentTransform).TryGetComponent(out LightSourceToy lightSourceToy))
					{
						gameObject = lightSourceToy.gameObject.AddComponent<LightSourceObject>().Init(block).gameObject;


					}
					*/

					break;
				}

				/*
				case BlockType.Pickup:
					{
						Pickup pickup = null;

						if (block.Properties.TryGetValue("Chance", out object property) && Random.Range(0, 101) > float.Parse(property.ToString()))
						{
							gameObject = new("Empty Pickup")
							{
								transform = { parent = parentTransform, localPosition = block.Position, localEulerAngles = block.Rotation, localScale = block.Scale },
							};
							break;
						}

						if (block.Properties.TryGetValue("CustomItem", out property) && !string.IsNullOrEmpty(property.ToString()))
						{
							string customItemName = property.ToString();

							if (!CustomItem.TryGet(customItemName, out CustomItem customItem))
							{
								Log.Error($"CustomItem with the name {customItemName} does not exist!");
								gameObject = new("Invalid Pickup")
								{
									transform = { parent = parentTransform, localPosition = block.Position, localEulerAngles = block.Rotation, localScale = block.Scale },
								};

								AttachedBlocks.Add(gameObject);
								ObjectFromId.Add(block.ObjectId, gameObject.transform);
							}
							else
							{
								pickup = customItem.Spawn(Vector3.zero);
							}
						}
						else
						{
							Item item = Item.Create((ItemType)Enum.Parse(typeof(ItemType), block.Properties["ItemType"].ToString()));

							if (item is Firearm firearm && block.Properties.TryGetValue("Attachements", out property))
								firearm.AddAttachment(property as List<AttachmentName>);

							pickup = item.CreatePickup(Vector3.zero);
						}

						gameObject = pickup.Base.gameObject;
						gameObject.name = block.Name;

						NetworkServer.UnSpawn(gameObject);

						gameObject.transform.parent = parentTransform;
						gameObject.transform.localPosition = block.Position;
						gameObject.transform.localEulerAngles = block.Rotation;
						gameObject.transform.localScale = block.Scale;

						NetworkServer.Spawn(gameObject);

						if (block.Properties.ContainsKey("Locked"))
							API.PickupsLocked.Add(pickup.Serial);

						if (block.Properties.TryGetValue("Uses", out property))
							API.PickupsUsesLeft.Add(pickup.Serial, int.Parse(property.ToString()));

						break;
					}

				case BlockType.Workstation:
					{
						if (Instantiate(ObjectType.WorkStation.GetObjectByMode(), parentTransform).TryGetComponent(out WorkstationController workstation))
						{
							gameObject = workstation.gameObject.AddComponent<WorkstationObject>().Init(block).gameObject;

							gameObject.transform.parent = null;
							NetworkServer.Spawn(gameObject);

							_transformProperties.Add(gameObject.transform.GetInstanceID(), block.ObjectId);
						}

						break;
					}

				case BlockType.Locker:
					{
						if (block.Properties.TryGetValue("Chance", out object property) && Random.Range(0, 101) > float.Parse(property.ToString()))
						{
							gameObject = new("Empty Locker")
							{
								transform = { localPosition = block.Position, localEulerAngles = block.Rotation, localScale = block.Scale },
							};
						}
						else
						{
							LockerType lockerType = (LockerType)Enum.Parse(typeof(LockerType), block.Properties["LockerType"].ToString());
							gameObject = Instantiate(lockerType.GetLockerObjectByType(), parentTransform).AddComponent<LockerObject>().Init(block).gameObject;
						}

						break;
					}

				case BlockType.Schematic:
					{
						string schematicName = block.Properties["SchematicName"].ToString();

						gameObject = ObjectSpawner.SpawnSchematic(schematicName, transform.position + block.Position, Quaternion.Euler(transform.eulerAngles + block.Rotation), null, null, true).gameObject;
						gameObject.transform.parent = parentTransform;

						gameObject.name = schematicName;

						break;
					}
					*/
		}

		// AttachedBlocks.Add(gameObject);
		ObjectFromId.Add(block.ObjectId, gameObject.transform);

		if (block.BlockType != BlockType.Light && TryGetAnimatorController(block.AnimatorName, out animatorController))
			_animators.Add(gameObject, animatorController);

		return gameObject.transform;
	}

	private bool TryGetAnimatorController(string animatorName, out RuntimeAnimatorController animatorController)
	{
		animatorController = null!;

		if (string.IsNullOrEmpty(animatorName))
			return false;

		Object animatorObject = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(x => x.mainAsset.name == animatorName)?.LoadAllAssets().First(x => x is RuntimeAnimatorController);

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

		_animators = null;
		AssetBundle.UnloadAllAssetBundles(false);
		return isAnimated;
	}

	private Dictionary<GameObject, RuntimeAnimatorController> _animators = new();
}
