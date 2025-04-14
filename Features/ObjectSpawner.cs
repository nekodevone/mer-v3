using AdminToys;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using ProjectMER.Features.Serializable.Schematics;
using UnityEngine;

namespace ProjectMER.Features;

public static class ObjectSpawner
{
	public static PrimitiveObjectToy SpawnPrimitive(SerializablePrimitive serializablePrimitive)
	{
		GameObject gameObject = serializablePrimitive.SpawnOrUpdateObject();
		return gameObject.GetComponent<PrimitiveObjectToy>();
	}

	public static SchematicObject SpawnSchematic(string schematicName, Vector3 position, Vector3 rotation) =>
		SpawnSchematic(new() { SchematicName = schematicName, Position = position, Rotation = rotation });

	public static SchematicObject SpawnSchematic(SerializableSchematic serializableSchematic)
	{
		GameObject? gameObject = serializableSchematic.SpawnOrUpdateObject();
		if (gameObject == null)
			return null!;

		return gameObject.GetComponent<SchematicObject>();
	}

	public static bool TrySpawnSchematic(string schematicName, Vector3 position, Vector3 rotation, out SchematicObject? schematic)
	{
		try
		{
			schematic = SpawnSchematic(schematicName, position, rotation);
			return schematic != null;
		}
		catch (Exception)
		{
			schematic = null;
			return false;
		}
	}
}
