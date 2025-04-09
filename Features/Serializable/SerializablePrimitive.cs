
using AdminToys;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Extensions;
using UnityEngine;

namespace ProjectMER.Features.Serializable;

public class SerializablePrimitive : SerializableObject
{
	/// <summary>
	/// Gets or sets the <see cref="PrimitiveType"/>.
	/// </summary>
	public PrimitiveType Type { get; set; } = PrimitiveType.Cube;

	/// <summary>
	/// Gets or sets the <see cref="SerializablePrimitive"/>'s color.
	/// </summary>
	public string Color { get; set; } = "#FF0000";

	/// <summary>
	/// Gets or sets the <see cref="SerializablePrimitive"/>'s flags.
	/// </summary>
	public PrimitiveFlags Flags { get; set; } = (PrimitiveFlags)3;

	public override GameObject SpawnOrUpdateObject(Room room, GameObject? instance = null)
	{
		PrimitiveObjectToy primitive;
		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);

		if (instance == null)
			primitive = UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObjectPrefab);
		else
		{
			primitive = instance.GetComponent<PrimitiveObjectToy>();
		}

		primitive.transform.position = position;
		primitive.transform.rotation = rotation;
		primitive.transform.localScale = Scale;

		primitive.NetworkMaterialColor = Color.GetColorFromString();

		primitive.NetworkPrimitiveType = Type;
		primitive.NetworkPrimitiveFlags = Flags;

		return primitive.gameObject;
	}
}
