using AdminToys;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration.Distributors;
using Mirror;
using UnityEngine;
using LightSourceToy = AdminToys.LightSourceToy;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features;

public static class PrefabManager
{
	public static PrimitiveObjectToy PrimitiveObjectPrefab { get; private set; }

	public static LightSourceToy LightSourcePrefab { get; private set; }

	public static DoorVariant LczDoorPrefab { get; private set; }
	public static DoorVariant HczDoorPrefab { get; private set; }
	public static DoorVariant EzDoorPrefab { get; private set; }
	public static DoorVariant BulkDoorPrefab { get; private set; }

	public static WorkstationController WorkstationPrefab { get; private set; }

	public static CapybaraToy CapybaraPrefab { get; private set; }

	public static TextToy TextPrefab { get; private set; }

	public static Scp079CameraToy LczCameraToy { get; private set; }
	public static Scp079CameraToy HczCameraToy { get; private set; }
	public static Scp079CameraToy SzCameraToy { get; private set; }
	public static Scp079CameraToy EzArmCameraToy { get; private set; }
	public static Scp079CameraToy EzCameraToy { get; private set; }

	public static ShootingTarget ShootingTargetSport { get; private set; }
	public static ShootingTarget ShootingTargetDBoy { get; private set; }
	public static ShootingTarget ShootingTargetBinary { get; private set; }

	public static Locker Scp018PedestalPrefab { get; private set; }
	public static Locker Scp207PedstalPrefab { get; private set; }
	public static Locker Scp244PedestalPrefab { get; private set; }
	public static Locker Scp268PedestalPrefab { get; private set; }
	public static Locker LargeGunLockerPrefab { get; private set; }
	public static Locker RifleRackLockerPrefab { get; private set; }
	public static Locker MiscLockerPrefab { get; private set; }
	public static Locker RegularMedkitLockerPrefab { get; private set; }
	public static Locker AdrenalineMedkitLockerPrefab { get; private set; }
	public static Locker Scp500PedestalPrefab { get; private set; }
	public static Locker Scp1853PedstalPrefab { get; private set; }
	public static Locker Scp2176PedestalPrefab { get; private set; }
	public static Locker Scp1576PedestalPrefab { get; private set; }
	public static Locker AntiScp207PedestalPrefab { get; private set; }
	public static Locker Scp1344PedestalPrefab { get; private set; }
	public static Locker ExperimentalWeaponLockerPrefab { get; private set; }

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

			if (gameObject.TryGetComponent(out ShootingTarget shootingTarget))
			{
				switch (gameObject.name)
				{
					case "sportTargetPrefab":
						ShootingTargetSport = shootingTarget;
						continue;
					case "dboyTargetPrefab":
						ShootingTargetDBoy = shootingTarget;
						continue;
					case "binaryTargetPrefab":
						ShootingTargetBinary = shootingTarget;
						continue;
				}
			}

			if (gameObject.TryGetComponent(out WorkstationController workstationController))
			{
				WorkstationPrefab = workstationController;
				continue;
			}

			if (gameObject.TryGetComponent(out CapybaraToy capybaraToy))
			{
				CapybaraPrefab = capybaraToy;
				continue;
			}

			if (gameObject.TryGetComponent(out Scp079CameraToy cameraToy))
			{
				switch (gameObject.name)
				{
					case "LczCameraToy":
						LczCameraToy = cameraToy;
						continue;
					case "HczCameraToy":
						HczCameraToy = cameraToy;
						continue;
					case "SzCameraToy":
						SzCameraToy = cameraToy;
						continue;
					case "EzArmCameraToy":
						EzArmCameraToy = cameraToy;
						continue;
					case "EzCameraToy":
						EzCameraToy = cameraToy;
						continue;
				}
			}

			if (gameObject.TryGetComponent(out TextToy textToy))
			{
				TextPrefab = textToy;
				continue;
			}

			if (gameObject.TryGetComponent(out Locker locker))
			{
				switch (gameObject.name)
				{
					case "Scp018PedestalStructure Variant":
						Scp018PedestalPrefab = locker;
						continue;
					case "Scp207PedestalStructure Variant":
						Scp207PedstalPrefab = locker;
						continue;
					case "Scp244PedestalStructure Variant":
						Scp244PedestalPrefab = locker;
						continue;
					case "Scp268PedestalStructure Variant":
						Scp268PedestalPrefab = locker;
						continue;
					case "LargeGunLockerStructure":
						LargeGunLockerPrefab = locker;
						continue;
					case "RifleRackStructure":
						RifleRackLockerPrefab = locker;
						continue;
					case "MiscLocker":
						MiscLockerPrefab = locker;
						continue;
					case "RegularMedkitStructure":
						RegularMedkitLockerPrefab = locker;
						continue;
					case "AdrenalineMedkitStructure":
						AdrenalineMedkitLockerPrefab = locker;
						continue;
					case "Scp500PedestalStructure Variant":
						Scp500PedestalPrefab = locker;
						continue;
					case "Scp1853PedestalStructure Variant":
						Scp1853PedstalPrefab = locker;
						continue;
					case "Scp2176PedestalStructure Variant":
						Scp2176PedestalPrefab = locker;
						continue;
					case "Scp1576PedestalStructure Variant":
						Scp1576PedestalPrefab = locker;
						continue;
					case "AntiScp207PedestalStructure Variant":
						AntiScp207PedestalPrefab = locker;
						continue;
					case "Scp1344PedestalStructure Variant":
						Scp1344PedestalPrefab = locker;
						continue;
					case "Experimental Weapon Locker":
						ExperimentalWeaponLockerPrefab = locker;
						continue;
				}
			}
		}
	}
}
