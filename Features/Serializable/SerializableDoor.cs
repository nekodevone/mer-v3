using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using UnityEngine;
using CheckpointDoor = Interactables.Interobjects.CheckpointDoor;
using Logger = LabApi.Features.Console.Logger;

namespace ProjectMER.Features.Serializable;

public class SerializableDoor : SerializableObject
{
	public string Type { get; set; } = "LCZ";
	public bool Open { get; set; } = false;
	public bool Locked { get; set; } = false;

	public override GameObject SpawnOrUpdateObject(Room room, GameObject? instance = null)
	{
		DoorVariant doorVariant;
		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);

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

		doorVariant.transform.position = position;
		doorVariant.transform.rotation = rotation;
		doorVariant.transform.localScale = Scale;

		_prevType = Type;
		doorVariant.NetworkTargetState = Open;
		doorVariant.ServerChangeLock(DoorLockReason.SpecialDoorFeature, Locked);

		if (instance != null)
		{
			NetworkServer.UnSpawn(instance);
			NetworkServer.Spawn(instance);
		}

		return doorVariant.gameObject;
	}

	private DoorVariant DoorPrefab
	{
		get
		{
			DoorVariant prefab = Type.ToUpperInvariant() switch
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

	internal string _prevType;
}
