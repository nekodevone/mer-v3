using System.Reflection;
using System.Text;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using MapGeneration;
using NorthwoodLib.Pools;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using ProjectMER.Features.Serializable.Schematics;
using UnityEngine;
using UserSettings.ServerSpecific;
using YamlDotNet.Serialization;
using Logger = LabApi.Features.Console.Logger;

namespace ProjectMER.Features;

public class ToolGun
{
	public static readonly string[] List =
	{
		nameof(SerializablePrimitive),
		nameof(SerializableLight),
		nameof(SerializableDoor),
		nameof(SerializablePlayerSpawnpoint),
		nameof(SerializableSchematic),
	};

	public static Dictionary<ushort, ToolGun> Dictionary { get; private set; } = [];
	public static Dictionary<Player, MapEditorObject> PlayerSelectedObjectDict = [];

	private readonly Firearm Firearm;
	private readonly IAdsModule AdsModule;

	public bool CreateMode => Firearm.IsEmittingLight && !AdsModule.AdsTarget;
	public bool DeleteMode => !Firearm.IsEmittingLight && !AdsModule.AdsTarget;
	public bool SelectMode => Firearm.IsEmittingLight && AdsModule.AdsTarget;

	private int _selectedItemIndex;
	public int ObjectToSpawnIndex
	{
		get => _selectedItemIndex;
		set
		{
			_selectedItemIndex = value;
			if (List.Length <= _selectedItemIndex)
			{
				_selectedItemIndex = 0;
				return;
			}

			if (_selectedItemIndex < 0)
			{
				_selectedItemIndex = List.Length - 1;
				return;
			}
		}
	}

	private ToolGun(Firearm firearm)
	{
		Firearm = firearm;
		if (!firearm.TryGetModule(out AdsModule))
		{
			throw new Exception("Module not found. This error should never occur.");
		}
	}

	public static bool TryAdd(Player player)
	{
		Item? item = player.AddItem(ItemType.GunCOM18);
		if (item == null)
			return false;

		Firearm toolgun = (Firearm)item.Base;
		toolgun.ApplyAttachmentsCode(454, false);
		if (!toolgun.TryGetModules(out MagazineModule magazineModule, out AutomaticActionModule automaticActionModule))
		{
			Logger.Error("Modules not found. This error should never occur.");
			return false;
		}

		magazineModule.AmmoStored = 0;
		magazineModule.ServerResyncData();

		automaticActionModule.Cocked = true;
		automaticActionModule.ServerResync();

		player.AddAmmo(ItemType.Ammo9x19, 1);

		Dictionary.Add(toolgun.ItemSerial, new ToolGun(toolgun));

		ServerSpecificSettingsSync.SendOnJoinFilter = (_) => false; // Prevent all users from receiving the tools after joining the server.
		ServerSpecificSettingsSync.DefinedSettings =
		[
			new SSGroupHeader("MapEditorReborn"),
			new SSDropdownSetting(0, "Schematic Name", MapUtils.GetAvailableSchematicNames())
		];

		ServerSpecificSettingsSync.SendToPlayersConditionally(x => x.inventory.UserInventory.Items.Values.Any(x => x.IsToolGun(out ToolGun _)));

		return true;
	}

	public static bool Remove(Player player)
	{
		foreach (ItemBase itemBase in player.Inventory.UserInventory.Items.Values)
		{
			if (Dictionary.ContainsKey(itemBase.ItemSerial))
			{
				Dictionary.Remove(itemBase.ItemSerial);
				player.RemoveItem(itemBase);
				return true;
			}
		}

		return false;
	}

	public static bool TryGetMapObject(Player player, out MapEditorObject mapEditorObject)
	{
		mapEditorObject = null!;
		if (!Raycast(player, out RaycastHit hit))
			return false;

		if (!hit.transform.TryGetComponentInParent(out mapEditorObject))
			return false;

		if (mapEditorObject is IndicatorObject indicatorObject)
			mapEditorObject = IndicatorObject.Dictionary[indicatorObject];

		return mapEditorObject;
	}

	public static bool TryGetSelectedMapObject(Player player, out MapEditorObject mapEditorObject)
	{
		if (!PlayerSelectedObjectDict.ContainsKey(player))
		{
			mapEditorObject = null!;
			return false;
		}

		return PlayerSelectedObjectDict.TryGetValue(player, out mapEditorObject) && mapEditorObject != null;
	}

	public void Shot(Player player)
	{
		if (CreateMode)
		{
			Create(player);
			return;
		}

		if (TryGetMapObject(player, out MapEditorObject mapEditorObject) && DeleteMode)
		{
			Delete(mapEditorObject);
			return;
		}

		if (SelectMode)
			Select(player, mapEditorObject);
	}

	private void Create(Player player)
	{
		if (!Raycast(player, out RaycastHit hit))
			return;

		if (!Room.TryGetRoomAtPosition(hit.point, out Room? room))
			room = Room.List.First(x => x.Name == RoomName.Outside);

		string position = room.Name == RoomName.Outside ? hit.point.ToString("F3") : room.Transform.InverseTransformPoint(hit.point).ToString("F3");
		string roomId = room.GetRoomStringId();

		MapSchematic map = MapUtils.UntitledMap;
		string id = Guid.NewGuid().ToString();

		switch (List[ObjectToSpawnIndex])
		{
			case nameof(SerializablePrimitive):
				{
					SerializablePrimitive serializablePrimitive = new() { Position = position, Room = roomId, Index = room.GetRoomIndex() };
					KeyValuePair<string, SerializablePrimitive> kvp = new(id, serializablePrimitive);
					if (map.TryAddElement(kvp.Key, kvp.Value))
						map.SpawnObject(kvp.Key, kvp.Value);
					break;
				}

			case nameof(SerializableLight):
				{
					SerializableLight serializableLight = new() { Position = position, Room = roomId, Index = room.GetRoomIndex() };
					KeyValuePair<string, SerializableLight> kvp = new(id, serializableLight);
					if (map.TryAddElement(kvp.Key, kvp.Value))
						map.SpawnObject(kvp.Key, kvp.Value);
					break;
				}

			case nameof(SerializableDoor):
				{
					SerializableDoor serializableDoor = new() { Position = position, Room = roomId, Index = room.GetRoomIndex() };
					KeyValuePair<string, SerializableDoor> kvp = new(id, serializableDoor);
					if (map.TryAddElement(kvp.Key, kvp.Value))
						map.SpawnObject(kvp.Key, kvp.Value);
					break;
				}

			case nameof(SerializablePlayerSpawnpoint):
				{
					SerializablePlayerSpawnpoint serializablePlayerSpawnpoint = new() { Position = (position.ToVector3() + Vector3.up * 0.01f).ToString("F3"), Room = roomId, Index = room.GetRoomIndex() };
					KeyValuePair<string, SerializablePlayerSpawnpoint> kvp = new(id, serializablePlayerSpawnpoint);
					if (map.TryAddElement(kvp.Key, kvp.Value))
						map.SpawnObject(kvp.Key, kvp.Value);
					break;
				}

			case nameof(SerializableSchematic):
				{
					if (!ServerSpecificSettingsSync.TryGetSettingOfUser(player.ReferenceHub, 0, out SSDropdownSetting dropdownSetting) ||
						 !dropdownSetting.TryGetSyncSelectionText(out string schematicName))
						return;

					SerializableSchematic serializableSchematic = new() { Position = position, Room = roomId, SchematicName = schematicName, Index = room.GetRoomIndex() };
					KeyValuePair<string, SerializableSchematic> kvp = new(id, serializableSchematic);
					if (map.TryAddElement(kvp.Key, kvp.Value))
						map.SpawnObject(kvp.Key, kvp.Value);
					break;
				}
		}

		foreach (MapEditorObject mapEditorObject in map.SpawnedObjects)
		{
			if (mapEditorObject.Id != id)
				continue;

			IndicatorObject.TrySpawnOrUpdateIndicator(mapEditorObject);
		}
	}

	public static void Delete(MapEditorObject mapEditorObject)
	{
		IndicatorObject.TryDestroyIndicator(mapEditorObject);

		MapSchematic map = MapUtils.LoadedMaps[mapEditorObject.MapName];
		if (map.TryRemoveElement(mapEditorObject.Id))
			map.DestroyObject(mapEditorObject.Id);
	}

	public static void Select(Player player, MapEditorObject mapEditorObject)
	{
		if (!PlayerSelectedObjectDict.ContainsKey(player))
		{
			PlayerSelectedObjectDict.Add(player, mapEditorObject);
			return;
		}

		PlayerSelectedObjectDict[player] = mapEditorObject;
	}

	public static string GetHintHUD(Player player)
	{
		StringBuilder sb = StringBuilderPool.Shared.Rent();

		sb.Append("<font=\"LiberationSans SDF\">");

		int offset = 0;
		object instance = null!;
		List<PropertyInfo> properties = [];
		if (PlayerSelectedObjectDict.TryGetValue(player, out MapEditorObject mapEditorObject) && mapEditorObject != null)
		{
			instance = mapEditorObject.GetType().GetField("Base").GetValue(mapEditorObject);
			properties = instance.GetType().GetProperties().ToList();
			offset = properties.Count - properties.Count(x => Attribute.IsDefined(x, typeof(YamlIgnoreAttribute))) + 2;
		}

		for (int i = 0; i < 36 - offset; i++)
		{
			sb.Append("<size=50%> </size>");
			sb.AppendLine();
		}

		foreach (string property in properties.GetColoredProperties(instance))
		{
			sb.Append($"<size=50%>");
			sb.Append(property);
			sb.Append("</size>");
			sb.AppendLine();
		}

		if (offset > 0)
			sb.AppendLine();

		if (!player.CurrentItem.IsToolGun(out ToolGun toolGun))
			return StringBuilderPool.Shared.ToStringReturn(sb);

		sb.Append($"<size=50%>");
		sb.Append(toolGun.GetToolGunModeString(player));
		sb.Append("</size>");

		sb.AppendLine();


		sb.Append($"<size=50%>");
		sb.Append($"{player.Position:F3}");
		sb.Append("</size>");

		sb.AppendLine();

		sb.Append($"<size=50%>");
		sb.Append(GetRoomString(player));
		sb.Append("</size>");
		sb.Append("</font>");

		return StringBuilderPool.Shared.ToStringReturn(sb);
	}

	private string GetToolGunModeString(Player player)
	{
		if (CreateMode)
		{
			string output = List[ObjectToSpawnIndex].Replace("Serializable", "").ToUpper();
			if (output == "SCHEMATIC")
			{
				if (ServerSpecificSettingsSync.TryGetSettingOfUser(player.ReferenceHub, 0, out SSDropdownSetting dropdownSetting) && dropdownSetting.TryGetSyncSelectionText(out string schematicName))
					output = schematicName.ToUpper();
				else
					output = "Please select schematic in options";
			}

			return $"<color=green>CREATE</color>\n<color=yellow>{output}</color>";
		}

		string name = " ";
		if (Raycast(player, out RaycastHit hit))
		{
			if (hit.transform.TryGetComponentInParent(out MapEditorObject mapEditorObject))
			{
				if (mapEditorObject is IndicatorObject indicatorObject)
					mapEditorObject = IndicatorObject.Dictionary[indicatorObject];

				if (mapEditorObject is SchematicObject schematicObject)
				{
					name = schematicObject.name.Split('-').Last().ToUpper();
				}
				else
				{
					name = mapEditorObject.Base.ToString().Split('.').Last().Replace("Serializable", "").ToUpper();
				}
			}
		}

		if (DeleteMode)
			return $"<color=red>DELETE</color>\n<color=yellow>{name}</color>";

		if (SelectMode)
			return $"<color=yellow>SELECT</color>\n<color=yellow>{name}</color>";

		return "\n ";
	}

	private static string GetRoomString(Player player)
	{
		if (!Room.TryGetRoomAtPosition(player.Camera.transform.position, out Room? room))
			return "Unknown";

        List<Room> list = ListPool<Room>.Shared.Rent(Room.List.Where(x => x.Zone == room.Zone && x.Shape == room.Shape && x.Name == room.Name));

		string roomString;
		if (list.Count == 1)
		{
			roomString = room.GetRoomStringId();
		}
		else
		{
			roomString = $"{room.GetRoomStringId()} ({list.IndexOf(room)}) ({list.Count})";
		}
		
		ListPool<Room>.Shared.Return(list);
		return roomString;
	}

	private static bool Raycast(Player player, out RaycastHit hit) => Raycast(player.Camera.position, player.Camera.forward, out hit);

	private static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit) => Physics.Raycast(origin, direction, out hit, 100f, ToolGunMask.Mask);

	private static readonly CachedLayerMask ToolGunMask = new("Default", "Door");
}
