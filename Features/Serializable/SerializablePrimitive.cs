
using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Extensions;
using UnityEngine;

namespace ProjectMER.Features.Serializable;

public class SerializablePrimitive : SerializableObject
{
	/// <summary>
	/// Gets or sets the <see cref="UnityEngine.PrimitiveType"/>.
	/// </summary>
	public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Cube;

	/// <summary>
	/// Gets or sets the <see cref="SerializablePrimitive"/>'s color.
	/// </summary>
	public string Color { get; set; } = "#FF0000";

	/// <summary>
	/// Gets or sets the <see cref="SerializablePrimitive"/>'s flags.
	/// </summary>
	public PrimitiveFlags PrimitiveFlags { get; set; } = (PrimitiveFlags)3;

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		PrimitiveObjectToy primitive = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObjectPrefab) : instance.GetComponent<PrimitiveObjectToy>();
		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);
		_prevIndex = Index;

		primitive.transform.SetPositionAndRotation(position, rotation);
		primitive.transform.localScale = Scale;

		primitive.NetworkMaterialColor = Color.GetColorFromString();
		primitive.NetworkPrimitiveType = PrimitiveType;
		primitive.NetworkPrimitiveFlags = PrimitiveFlags;

		if (instance == null)
			NetworkServer.Spawn(primitive.gameObject);

		return primitive.gameObject;
	}
}
