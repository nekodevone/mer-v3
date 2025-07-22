using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using UnityEngine;
using YamlDotNet.Serialization;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable.Schematics;

public class SerializableSchematic : SerializableObject
{
	public string SchematicName { get; set; } = "None";
	
	[YamlIgnore]
	public SchematicObject SchematicObject { get; set; }

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		if (Data == null)
			return null;

		PrimitiveObjectToy schematic = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject) : instance.GetComponent<PrimitiveObjectToy>();
		schematic.NetworkPrimitiveFlags = PrimitiveFlags.None;
		schematic.NetworkMovementSmoothing = 60;

		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);
		_prevIndex = Index;

		schematic.name = $"CustomSchematic-{SchematicName}";
		schematic.transform.SetPositionAndRotation(position, rotation);
		schematic.transform.localScale = Scale;

		if (instance == null)
		{
			NetworkServer.Spawn(schematic.gameObject);
			schematic.gameObject.AddComponent<SchematicObject>().Init(Data);
		}

		SchematicObject = schematic.gameObject.GetComponent<SchematicObject>();
		return schematic.gameObject;
	}

	private SchematicObjectDataList? Data => _data ??= MapUtils.TryGetSchematicDataByName(SchematicName, out SchematicObjectDataList data) ? data : null;
	private SchematicObjectDataList? _data;
}
