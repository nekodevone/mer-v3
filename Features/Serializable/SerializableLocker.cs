using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;
using YamlDotNet.Serialization;
using LabApiLocker = LabApi.Features.Wrappers.Locker;
using Locker = MapGeneration.Distributors.Locker;
using LockerType = ProjectMER.Features.Enums.LockerType;
using Object = UnityEngine.Object;

namespace ProjectMER.Features.Serializable
{
    public class SerializableLocker : SerializableObject
    {
        public LockerType LockerType { get; set; } = LockerType.Misc;
        [IgnoreToolgunGUI]
        public Dictionary<int, List<SerializableLockerItem>?> Chambers { get; set; } = new();

        public bool ShuffleChambers { get; set; } = true;

        public DoorPermissionFlags KeycardPermissions { get; set; } = DoorPermissionFlags.None;

        public ushort OpenedChambers { get; set; }

        public float Chance { get; set; } = 100f;

        public bool IsSpawnedLoot { get; private set; } = false;
        [IgnoreToolgunGUI]
        [YamlIgnore]
        public StructurePositionSync StructurePositionSync { get; set; }

        public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
        {
            Locker lockerVariant;
            Vector3 position = room.GetAbsolutePosition(Position);
            Quaternion rotation = room.GetAbsoluteRotation(Rotation);
            _prevIndex = Index;

            lockerVariant = instance == null ? Object.Instantiate(LockerPrefab) : instance.gameObject.GetComponent<Locker>();

            Locker = lockerVariant;

            //SetupLocker(lockerVariant);

            StructurePositionSync = lockerVariant.GetComponent<StructurePositionSync>();

            lockerVariant.transform.SetPositionAndRotation(position, rotation);
            lockerVariant.transform.localScale = Scale;
            
            var labApiLocker = LabApiLocker.Get(lockerVariant);

            StructurePositionSync.Network_position = (lockerVariant.transform.position);
            StructurePositionSync.Network_rotationY = (sbyte)Mathf.RoundToInt(lockerVariant.transform.eulerAngles.y / 5.625f);


            _prevType = LockerType;

            labApiLocker.ClearLockerLoot();
            labApiLocker.ClearAllChambers();
            Locker.Chambers.ForEach(chamber => chamber.RequiredPermissions = KeycardPermissions);

            foreach (var chamber in Chambers)
            {
                if (chamber.Value == null) continue;

                for (int i = 0; i < chamber.Value.Count; i++)
                {
                    var loot = chamber.Value[i];
                    var lockerChamber = labApiLocker.Chambers[i];
                    if (loot.Item != ItemType.None)
                    {
                        labApiLocker.AddLockerLoot(loot.Item, 1, loot.Chance, 1, int.Parse($"{loot.Count}"));
                        lockerChamber.AcceptableItems = new[] { loot.Item };
                    }
                    
                    lockerChamber.RequiredPermissions = KeycardPermissions;

                    if (i >= labApiLocker.Chambers.Count)
                        break; // чтобы не выйти за пределы
                }
            }


            NetworkServer.UnSpawn(lockerVariant.gameObject);
            NetworkServer.Spawn(lockerVariant.gameObject);
            
            Timing.CallDelayed(0.25f, () =>
            {
                foreach (ItemPickupBase itemPickupBase in lockerVariant.GetComponentsInChildren<ItemPickupBase>())
                {
                    if (itemPickupBase.TryGetComponent(out Rigidbody rigidbody))
                        rigidbody.isKinematic = false;
                }
            });

            return lockerVariant.gameObject;
        }

        private MapGeneration.Distributors.Locker Locker;

        private MapGeneration.Distributors.Locker LockerPrefab
        {
            get
            {
                MapGeneration.Distributors.Locker prefab = LockerType switch
                {
                    LockerType.RifleRack => PrefabManager.LockerRifleRack,
                    LockerType.LargeGun => PrefabManager.LockerLargeGun,
                    LockerType.Misc => PrefabManager.LockerMisc,
                    LockerType.Medkit => PrefabManager.LockerRegularMedkit,
                    LockerType.Adrenaline => PrefabManager.LockerAdrenalineMedkit,
                    LockerType.ExperimentalWeapon => PrefabManager.LockerExperimentalWeapon,
                    _ => throw new InvalidOperationException(),
                };

                return prefab;
            }
        }

        public override bool RequiresReloading => LockerType != _prevType || base.RequiresReloading;

        internal LockerType _prevType;
    }

}