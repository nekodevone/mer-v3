using LabApi.Features.Wrappers;
using MonoMod.Utils;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace ProjectMER.Features.Serializable;

public class MapSchematic
{
	public MapSchematic() { }

	public MapSchematic(string mapName)
	{
		Name = mapName;
	}

	public string Name;

	public Dictionary<string, SerializablePrimitive> Primitives { get; set; } = [];

	public Dictionary<string, SerializableLight> Lights { get; set; } = [];

	public Dictionary<string, SerializableDoor> Doors {get; set; } = [];

	public Dictionary<string, SerializablePlayerSpawnpoint> PlayerSpawnpoints { get; set; } = [];

	public Dictionary<string, SerializableSchematic> Schematics { get; set; } = [];

	public List<MapEditorObject> SpawnedObjects = [];

	public MapSchematic Merge(MapSchematic other)
	{
		Primitives.AddRange(other.Primitives);
		Lights.AddRange(other.Lights);
		Doors.AddRange(other.Doors);
		PlayerSpawnpoints.AddRange(other.PlayerSpawnpoints);
		Schematics.AddRange(other.Schematics);

		return this;
	}

	public void Reload()
	{
		foreach (MapEditorObject mapEditorObject in SpawnedObjects)
			mapEditorObject.Destroy();

		SpawnedObjects.Clear();

		Primitives.ForEach(kVP => SpawnObject(kVP.Key, kVP.Value));
		Lights.ForEach(kVP => SpawnObject(kVP.Key, kVP.Value));
		Doors.ForEach(kVP => SpawnObject(kVP.Key, kVP.Value));
		PlayerSpawnpoints.ForEach(kVP => SpawnObject(kVP.Key, kVP.Value));
		Schematics.ForEach(kVP => SpawnObject(kVP.Key, kVP.Value));
	}

	public void SpawnObject<T>(string id, T serializableObject) where T : SerializableObject
	{
		if (serializableObject is SerializableSchematic serializableSchematic)
		{
			if (!MapUtils.TryGetSchematicDataByName(serializableSchematic.SchematicName, out SchematicObjectDataList data))
				return;

			foreach (Room room in serializableObject.GetRooms())
			{
				GameObject gameObject = serializableObject.SpawnOrUpdateObject(room);
                SchematicObject schematicObject = gameObject.AddComponent<SchematicObject>().Init(serializableSchematic, data, Name, id, room);
				SpawnedObjects.Add(schematicObject);
			}

			return;
		}

		foreach (Room room in serializableObject.GetRooms())
		{
			GameObject gameObject = serializableObject.SpawnOrUpdateObject(room);
			MapEditorObject mapEditorObject = gameObject.AddComponent<MapEditorObject>().Init(serializableObject, Name, id, room);
			SpawnedObjects.Add(mapEditorObject);
		}
	}

	public void DestroyObject(string id)
	{
		foreach (MapEditorObject mapEditorObject in SpawnedObjects.ToList())
		{
			if (mapEditorObject.Id != id)
				continue;

			SpawnedObjects.Remove(mapEditorObject);
			mapEditorObject.Destroy();
		}
	}

	public bool TryAddElement<T>(string id, T serializableObject) where T : SerializableObject
	{
		if (Primitives.TryAdd(id, serializableObject))
			return true;

		if (Lights.TryAdd(id, serializableObject))
			return true;

		if (Doors.TryAdd(id, serializableObject))
			return true;

		if (PlayerSpawnpoints.TryAdd(id, serializableObject))
			return true;

		if (Schematics.TryAdd(id, serializableObject))
			return true;

		return false;
	}

	public bool TryRemoveElement(string id)
	{
		if (Primitives.Remove(id))
			return true;

		if (Lights.Remove(id))
			return true;

		if (Doors.Remove(id))
			return true;

		if (PlayerSpawnpoints.Remove(id))
			return true;

		if (Schematics.Remove(id))
			return true;

		return false;
	}

	/*
	public bool TryGetElement<T>(string id, out T serializableObject) where T : SerializableObject
	{
		if (Primitives.TryGetValue(id, out SerializablePrimitive primitive))
		{
			serializableObject = (T)(object)primitive;
			return true;
		}

		if (Lights.TryGetValue(id, out SerializableLight light))
		{
			serializableObject = (T)(object)light;
			return true;
		}

		serializableObject = null!;
		return false;
	}
	*/
}
