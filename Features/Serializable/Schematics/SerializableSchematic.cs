using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable.Schematics;

public class SerializableSchematic : SerializableObject
{
	public string SchematicName { get; set; } = "None";

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		PrimitiveObjectToy primitive = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObjectPrefab) : instance.GetComponent<PrimitiveObjectToy>();
		primitive.NetworkPrimitiveFlags = PrimitiveFlags.None;

		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);
		_prevIndex = Index;

		if (instance == null)
		{
			if (!MapUtils.TryGetSchematicDataByName(SchematicName, out SchematicObjectDataList? data))
				return null;

			primitive.name = $"CustomSchematic-{SchematicName}";
			primitive.transform.SetPositionAndRotation(position, rotation);
			NetworkServer.Spawn(primitive.gameObject);

			primitive.gameObject.AddComponent<SchematicObject>().Init(data);
		}
		else
		{
			primitive = instance.GetComponent<PrimitiveObjectToy>();
			primitive.transform.SetPositionAndRotation(position, rotation);
		}

		return primitive.gameObject;
	}
}
