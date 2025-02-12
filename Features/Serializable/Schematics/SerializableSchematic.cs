using LabApi.Features.Wrappers;
using ProjectMER.Features.Extensions;
using UnityEngine;

namespace ProjectMER.Features.Serializable.Schematics;

public class SerializableSchematic : SerializableObject
{
	public string SchematicName { get; set; } = "None";

	public override GameObject SpawnOrUpdateObject(Room room, GameObject? instance = null)
	{
		GameObject gameObject;

		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);

		if (instance == null)
		{
			gameObject = new($"CustomSchematic-{SchematicName}")
			{
				transform =
				{
					position = position,
					rotation = rotation
				},
			};
		}
		else
		{
			gameObject = instance;
			gameObject.transform.position = position;
			gameObject.transform.rotation = rotation;
		}

		return gameObject;
	}
}
