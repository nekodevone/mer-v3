using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using ProjectMER.Features.Serializable.Schematics;
using Serialization;
using Utf8Json;
using YamlDotNet.Core;
using Logger = LabApi.Features.Console.Logger;

namespace ProjectMER.Features;

public static class MapUtils
{
	public const string UntitledMapName = "Untitled";

	public static MapSchematic UntitledMap => LoadedMaps.GetOrAdd(UntitledMapName, () => new(UntitledMapName));

	public static Dictionary<string, MapSchematic> LoadedMaps { get; private set; } = [];

	public static void SaveMap(string mapName)
	{
		if (!LoadedMaps.TryGetValue(mapName, out MapSchematic map))
			map = LoadedMaps.GetOrAdd(UntitledMapName, () => new MapSchematic(UntitledMapName));

		string path = Path.Combine(ProjectMER.MapsDir, $"{mapName}.yml");
		File.WriteAllText(path, YamlParser.Serializer.Serialize(map));
	}

	public static bool LoadMap(string mapName)
	{
		if (!TryGetMapData(mapName, out MapSchematic map))
			return false;

		UnloadMap(mapName);
		map.Reload();

		LoadedMaps.Add(mapName, map);
		return true;
	}

	public static bool UnloadMap(string mapName)
	{
		if (!LoadedMaps.ContainsKey(mapName))
			return false;

		foreach (MapEditorObject mapEditorObject in LoadedMaps[mapName].SpawnedObjects)
			mapEditorObject.Destroy();

		LoadedMaps.Remove(mapName);
		return true;
	}

	public static bool TryGetMapData(string mapName, out MapSchematic map)
	{
		map = null!;
		string path = Path.Combine(ProjectMER.MapsDir, $"{mapName}.yml");

		if (!File.Exists(path))
		{
			Logger.Error($"Failed to load map data: File {mapName}.yml does not exist!");
			return false;
		}

		try
		{
			map = YamlParser.Deserializer.Deserialize<MapSchematic>(File.ReadAllText(path));
			map.Name = mapName;
		}
		catch (YamlException e)
		{
			Logger.Error($"Failed to load map data: File {mapName}.yml has YAML errors!\n{e.ToString().Split('\n')[0]}");
			return false;
		}

		return true;
	}

	public static SchematicObjectDataList GetSchematicDataByName(string schematicName)
	{
		string dirPath = Path.Combine(ProjectMER.SchematicsDir, schematicName);
		if (!Directory.Exists(dirPath))
			return null!;

		string schematicPath = Path.Combine(dirPath, $"{schematicName}.json");
		if (!File.Exists(schematicPath))
			return null!;

		SchematicObjectDataList data = JsonSerializer.Deserialize<SchematicObjectDataList>(File.ReadAllText(schematicPath));
		data.Path = dirPath;

		return data;
	}

    public static string[] GetAvailableSchematicNames() => Directory.GetDirectories(ProjectMER.SchematicsDir).Select(Path.GetFileName).ToArray();
}
