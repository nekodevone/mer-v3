using AdminToys;
using Interactables.Interobjects.DoorUtils;
using Mirror;
using UnityEngine;

namespace ProjectMER.Features;

public static class PrefabManager
{
	public static PrimitiveObjectToy PrimitiveObjectPrefab { get; private set; }

	public static LightSourceToy LightSourcePrefab { get; private set; }

	public static DoorVariant LczDoorPrefab { get; private set; }
	public static DoorVariant HczDoorPrefab { get; private set; }
	public static DoorVariant EzDoorPrefab { get; private set; }
	public static DoorVariant BulkDoorPrefab { get; private set; }

	public static void RegisterPrefabs()
	{
		foreach (GameObject gameObject in NetworkClient.prefabs.Values)
		{
			if (gameObject.TryGetComponent(out PrimitiveObjectToy primitiveObjectToy))
			{
				PrimitiveObjectPrefab = primitiveObjectToy;
				continue;
			}

			if (gameObject.TryGetComponent(out LightSourceToy lightSourceToy))
			{
				LightSourcePrefab = lightSourceToy;
				continue;
			}

			if (gameObject.TryGetComponent(out DoorVariant doorVariant))
			{
				switch (gameObject.name)
				{
					case "LCZ BreakableDoor":
						LczDoorPrefab = doorVariant;
						continue;
					case "HCZ BreakableDoor":
						HczDoorPrefab = doorVariant;
						continue;
					case "EZ BreakableDoor":
						EzDoorPrefab = doorVariant;
						continue;
					case "HCZ BulkDoor":
						BulkDoorPrefab = doorVariant;
						continue;
				}
			}
		}
	}
}
