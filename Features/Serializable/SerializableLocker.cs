using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration.Distributors;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using LabApi.Features.Wrappers;
using UnityEngine;
using YamlDotNet.Serialization;
using LockerChamber = MapGeneration.Distributors.LockerChamber;
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
            MapGeneration.Distributors.Locker lockerVariant;
            Vector3 position = room.GetAbsolutePosition(Position);
            Quaternion rotation = room.GetAbsoluteRotation(Rotation);
            _prevIndex = Index;

            lockerVariant = instance == null ? Object.Instantiate(LockerPrefab) : instance.gameObject.GetComponent<MapGeneration.Distributors.Locker>();

            Locker = lockerVariant;

            SetupLocker(lockerVariant);

            StructurePositionSync = lockerVariant.GetComponent<StructurePositionSync>();

            lockerVariant.transform.SetPositionAndRotation(position, rotation);
            lockerVariant.transform.localScale = Scale;

            StructurePositionSync.Network_position = (lockerVariant.transform.position);
            StructurePositionSync.Network_rotationY = (sbyte)Mathf.RoundToInt(lockerVariant.transform.eulerAngles.y / 5.625f);


            _prevType = LockerType;

            NetworkServer.UnSpawn(lockerVariant.gameObject);
            NetworkServer.Spawn(lockerVariant.gameObject);

            return lockerVariant.gameObject;
        }

        private void SetupLocker(MapGeneration.Distributors.Locker locker)
        {
            Locker.Loot = Array.Empty<LockerLoot>();
            HandleItems();
            IsSpawnedLoot = true;
        }
        
        private void HandleItems()
        {
            if (Locker == null)
            {
                Logger.Error("Locker is null in HandleItems()");
                return;
            }

            if (Locker.Chambers == null || Locker.Chambers.Length == 0)
            {
                Logger.Error("Locker.Chambers is null or empty");
                return;
            }

            Locker.Loot = new LockerLoot[Locker.Chambers.Length];

            for (int i = 0; i < Locker.Chambers.Length; i++)
            {
                var lockerChamber = Locker.Chambers[i];
                if (lockerChamber == null)
                {
                    Logger.Warn($"Locker chamber {i} is null in prefab");
                    continue;
                }

                lockerChamber.RequiredPermissions = KeycardPermissions;

                if (!Chambers.TryGetValue(i, out var chamberItems) || chamberItems == null)
                    chamberItems = new List<SerializableLockerItem>();

                var chosenLoot = Choose(chamberItems);

                if (chosenLoot == null)
                {
                    Logger.Warn($"No loot chosen for chamber {i}");
                    continue;
                }

                Locker.Loot[i].TargetItem = chosenLoot.Item;
            }

            Locker.OpenedChambers = OpenedChambers;
        }

        private static SerializableLockerItem Choose(List<SerializableLockerItem>? chambers)
        {
            if (chambers == null || chambers.Count == 0)
            {
                return new SerializableLockerItem(ItemType.Coin, 0, new List<AttachmentName>(), 0);
            }

            // Фильтруем предметы без типа
            var validChambers = chambers.Where(c => c.Item != ItemType.None && c.Chance > 0).ToList();
            if (validChambers.Count == 0)
            {
                return new SerializableLockerItem(ItemType.Coin, 0, new List<AttachmentName>(), 0);
            }

            var total = validChambers.Sum(elem => elem.Chance);
            var randomPoint = UnityEngine.Random.value * total;

            foreach (var t in validChambers)
            {
                if (randomPoint < t.Chance)
                    return t;

                randomPoint -= t.Chance;
            }

            return validChambers[validChambers.Count-1];
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