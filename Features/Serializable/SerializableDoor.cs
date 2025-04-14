using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using UnityEngine;
using YamlDotNet.Serialization;
using CheckpointDoor = Interactables.Interobjects.CheckpointDoor;
using Logger = LabApi.Features.Console.Logger;

namespace ProjectMER.Features.Serializable;

public class SerializableDoor : SerializableObject
{
	public string DoorType { get; set; } = "LCZ";
	public bool IsOpen { get; set; } = false;
	public bool IsLocked { get; set; } = false;

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		DoorVariant doorVariant;
		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);
		_prevIndex = Index;

		if (instance == null)
		{
			doorVariant = GameObject.Instantiate(DoorPrefab);
			if (doorVariant.TryGetComponent(out DoorRandomInitialStateExtension doorRandomInitialStateExtension))
				GameObject.Destroy(doorRandomInitialStateExtension);
		}
		else
		{
			doorVariant = instance.GetComponent<DoorVariant>();
		}

		doorVariant.transform.SetPositionAndRotation(position, rotation);
		doorVariant.transform.localScale = Scale;

		_prevType = DoorType;
		doorVariant.NetworkTargetState = IsOpen;
		doorVariant.ServerChangeLock(DoorLockReason.SpecialDoorFeature, IsLocked);

		NetworkServer.UnSpawn(doorVariant.gameObject);
		NetworkServer.Spawn(doorVariant.gameObject);

		return doorVariant.gameObject;
	}

	private DoorVariant DoorPrefab
	{
		get
		{
			DoorVariant prefab = DoorType.ToUpperInvariant() switch
			{
				"LCZ" => PrefabManager.LczDoorPrefab,
				"HCZ" => PrefabManager.HczDoorPrefab,
				"EZ" => PrefabManager.EzDoorPrefab,
				"BULKDOOR" => PrefabManager.BulkDoorPrefab,
				_ => throw new InvalidOperationException(),
			};

			return prefab;
		}
	}

	public override bool RequiresReloading => DoorType != _prevType || base.RequiresReloading;

	internal string _prevType;
}
