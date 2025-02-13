using System.Reflection;
using System.Text;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using MapGeneration;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using ProjectMER.Features.Serializable.Schematics;
using UnityEngine;
using UserSettings.ServerSpecific;
using Logger = LabApi.Features.Console.Logger;

namespace ProjectMER.Features;

public class ToolGun
{
	public static readonly string[] List =
	{
		nameof(SerializablePrimitive),
		nameof(SerializableLight),
		nameof(SerializableSchematic),
	};

	public static Dictionary<ushort, ToolGun> Dictionary { get; private set; } = [];
	// public static Dictionary<Player, MapEditorObject> SelectedObject = [];

	private readonly Firearm Firearm;
	private readonly IAdsModule AdsModule;
	private readonly StringBuilder sb = new();

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

	// public Player Player => Player.Get(Firearm.Owner);
	// public bool Equiped => Player.CurrentItem != null && Player.CurrentItem.Serial == Firearm.ItemSerial;
	public MapEditorObject? SelectedObject { get; private set; }

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
		toolgun.ApplyAttachmentsCode(390, true);
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
		Vector3 forward = player.Camera.forward;
		if (!Physics.Raycast(player.Camera.position + forward, forward, out RaycastHit hit, 100f))
			return false;

		if (!hit.transform.TryGetComponentInParent(out mapEditorObject))
			return false;

		return mapEditorObject;
	}

	public void Shot(Player player)
	{
		if (CreateMode)
		{
			Create(player);
			return;
		}

		if (!TryGetMapObject(player, out MapEditorObject mapEditorObject))
			return;

		if (DeleteMode)
		{
			Delete(mapEditorObject);
			return;
		}

		if (SelectMode)
		{
			Select(mapEditorObject);
			return;
		}
	}

	private void Create(Player player)
	{
		Vector3 forward = player.Camera.forward;
		if (!Physics.Raycast(player.Camera.position + forward, forward, out RaycastHit hit, 100f))
			return;

		if (!Room.TryGetRoomAtPosition(hit.point, out Room? room))
			room = Room.List.First(x => x.Name == RoomName.Outside);

		string position = room.Name == RoomName.Outside ? hit.point.ToString("G") : room.Transform.InverseTransformPoint(hit.point).ToString("G");
		string roomId = room.GetRoomStringId();

		MapSchematic map = MapUtils.UntitledMap;
		switch (List[ObjectToSpawnIndex])
		{
			case nameof(SerializablePrimitive):
				SerializablePrimitive serializablePrimitive = new() { Position = position, Room = roomId };
				KeyValuePair<string, SerializablePrimitive> kvp = new(Guid.NewGuid().ToString(), serializablePrimitive);
				if (map.TryAddElement(kvp.Key, kvp.Value))
					map.SpawnObject(kvp.Key, kvp.Value);
				return;

			case nameof(SerializableLight):
				SerializableLight serializableLight = new() { Position = position, Room = roomId };
				KeyValuePair<string, SerializableLight> kvp2 = new(Guid.NewGuid().ToString(), serializableLight);
				if (map.TryAddElement(kvp2.Key, kvp2.Value))
					map.SpawnObject(kvp2.Key, kvp2.Value);
				return;

			default:
				if (!ServerSpecificSettingsSync.TryGetSettingOfUser(player.ReferenceHub, 0, out SSDropdownSetting dropdownSetting) ||
				 	!dropdownSetting.TryGetSyncSelectionText(out string schematicName))
					return;

				SerializableSchematic serializableSchematic = new() { Position = position, Room = roomId, SchematicName = schematicName };
				KeyValuePair<string, SerializableSchematic> kvp3 = new(Guid.NewGuid().ToString(), serializableSchematic);
				if (map.TryAddElement(kvp3.Key, kvp3.Value))
					map.SpawnObject(kvp3.Key, kvp3.Value);
				return;
		}
	}

	private void Delete(MapEditorObject mapEditorObject)
	{
		MapSchematic map = MapUtils.LoadedMaps[mapEditorObject.MapName];
		if (map.TryRemoveElement(mapEditorObject.Id))
			map.DestroyObject(mapEditorObject.Id);
	}

	private void Select(MapEditorObject mapEditorObject)
	{
		SelectedObject = mapEditorObject;
	}

	public string GetHintHUD(Player player)
	{
		int offset = 0;
		object instance = null!;
		List<PropertyInfo> properties = [];
		if (SelectedObject != null)
		{
			instance = SelectedObject.GetType().GetField("Base").GetValue(SelectedObject);
			properties = instance.GetType().GetProperties().ToList();
			offset = properties.Count;
		}

		sb.Clear();

		for (int i = 0; i < 34 - offset; i++)
		{
			sb.Append("<size=50%> </size>");
			sb.AppendLine();
		}

		foreach (PropertyInfo property in properties)
		{
			sb.Append($"<size=50%>");
			sb.Append($"{property.Name}: {property.GetValue(instance)}");
			sb.Append("</size>");
			sb.AppendLine();
		}

		/*
		sb.Append("<size=75%>OTHER POS");
		for (int i = 0; i < 30; i++)
			sb.Append(" ");
		sb.Append("PLAYER POS");
		for (int i = 0; i < 30; i++)
			sb.Append(" ");
		sb.Append("OTHER POS</size>");
		*/

		sb.Append($"<size=50%>");
		sb.Append(GetToolGunModeString(player));
		sb.Append("</size>");

		sb.AppendLine();


		sb.Append($"<size=50%>");
		/*
		sb.Append("(900.000, 1000.000, -700.000)");
		for (int i = 0; i < 30; i++)
			sb.Append(" ");
			*/
		sb.Append($"{player.Position:F3}");
		/*
		for (int i = 0; i < 30; i++)
			sb.Append(" ");
		sb.Append("(900.000, 1000.000, -700.000)");
		*/
		sb.Append("</size>");

		sb.AppendLine();

		sb.Append($"<size=50%>");
		sb.Append(Room.TryGetRoomAtPosition(player.Camera.transform.position, out Room? room) ? $"{room.Zone}_{room.Shape}_{room.Name}" : "Unknown");
		sb.Append("</size>");

		return sb.ToString();
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

		Vector3 forward = player.Camera.forward;
		string name = " ";
		if (Physics.Raycast(player.Camera.position + forward, forward, out RaycastHit hit, 100f))
		{
			if (hit.transform.TryGetComponentInParent(out MapEditorObject mapEditorObject))
			{
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
}
