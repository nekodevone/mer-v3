using LabApi.Features.Wrappers;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace ProjectMER.Features.Serializable.Schematics;

public class SerializableSchematic : SerializableObject
{
	public string SchematicName { get; set; } = "None";

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		GameObject gameObject;

		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);
		_prevIndex = Index;

		if (instance == null)
		{
			if (!MapUtils.TryGetSchematicDataByName(SchematicName, out SchematicObjectDataList? data))
				return null;

			gameObject = new($"CustomSchematic-{SchematicName}")
			{
				transform =
				{
					position = position,
					rotation = rotation
				},
			};

			gameObject.AddComponent<SchematicObject>().Init(data);
		}
		else
		{
			gameObject = instance;
			gameObject.transform.SetPositionAndRotation(position, rotation);
		}

		return gameObject;
	}
}
