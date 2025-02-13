using LabApi.Features.Wrappers;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;
using UnityEngine;

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

	public Dictionary<string, SerializableSchematic> Schematics { get; set; } = [];

	public List<MapEditorObject> SpawnedObjects = [];

	public void Reload()
	{
		foreach (MapEditorObject mapEditorObject in SpawnedObjects)
			mapEditorObject.Destroy();

		SpawnedObjects.Clear();

		foreach (KeyValuePair<string, SerializablePrimitive> kVP in Primitives)
		{
			SpawnObject(kVP.Key, kVP.Value);
		}

		foreach (KeyValuePair<string, SerializableLight> kVP in Lights)
		{
			SpawnObject(kVP.Key, kVP.Value);
		}

		foreach (KeyValuePair<string, SerializableSchematic> kVP in Schematics)
		{
			SpawnObject(kVP.Key, kVP.Value);
		}
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
				SpawnedObjects.Add(gameObject.AddComponent<SchematicObject>().Init(serializableSchematic, data, Name, id, room));
			}

			return;
		}

		foreach (Room room in serializableObject.GetRooms())
		{
			GameObject gameObject = serializableObject.SpawnOrUpdateObject(room);
			SpawnedObjects.Add(gameObject.AddComponent<MapEditorObject>().Init(serializableObject, Name, id, room));
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
		if (serializableObject is SerializablePrimitive primitive)
		{
			if (Primitives.ContainsKey(id))
				return false;

			Primitives.Add(id, primitive);
			return true;
		}

		if (serializableObject is SerializableLight light)
		{
			if (Lights.ContainsKey(id))
				return false;

			Lights.Add(id, light);
			return true;
		}

		if (serializableObject is SerializableSchematic schematic)
		{
			if (Schematics.ContainsKey(id))
				return false;

			Schematics.Add(id, schematic);
			return true;
		}

		return false;
	}

	public bool TryRemoveElement(string id)
	{
		if (Primitives.Remove(id))
			return true;

		if (Lights.Remove(id))
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
