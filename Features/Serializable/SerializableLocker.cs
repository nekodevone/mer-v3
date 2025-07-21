using Interactables.Interobjects.DoorUtils;
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
        public Dictionary<int, List<SerializableLockerItem>?> Chambers { get; set; } = new()
        {
            { 0, new () { new () } },
        };

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


        public void SetupLocker(MapGeneration.Distributors.Locker locker)
        {
            Locker.Loot = Array.Empty<LockerLoot>();
            HandleItems();
            IsSpawnedLoot = true;
        }
        private void HandleItems()
        {
            foreach (LockerChamber lockerChamber in Locker.Chambers)
                lockerChamber.RequiredPermissions = KeycardPermissions;

            Dictionary<int, List<SerializableLockerItem>> chambersCopy = null;
            if (ShuffleChambers)
            {
                chambersCopy = new(Chambers.Count);
                List<List<SerializableLockerItem>> chambersRandomValues = Chambers.Values.OrderBy(x => UnityEngine.Random.value).ToList();
                for (int i = 0; i < Chambers.Count; i++)
                {
                    chambersCopy.Add(i, chambersRandomValues[i]);
                }
            }

            for (int i = 0; i < Locker.Chambers.Length; i++)
            {
                if (i == Chambers.Count)
                    break;

                SerializableLockerItem chosenLoot = Choose(ShuffleChambers ? chambersCopy?[i] : Chambers[i]);

                Locker.Chambers.ElementAt(i).SpawnItem(chosenLoot.Item, (int)chosenLoot.Count);
            }

            Locker.OpenedChambers = OpenedChambers;
        }

        private static SerializableLockerItem Choose(List<SerializableLockerItem>? chambers)
        {
            if (chambers == null || chambers.Count == 0)
                return null;

            float total = 0;

            foreach (SerializableLockerItem elem in chambers)
            {
                total += elem.Chance;
            }

            float randomPoint = UnityEngine.Random.value * total;

            for (int i = 0; i < chambers.Count; i++)
            {
                if (randomPoint < chambers[i].Chance)
                {
                    return chambers[i];
                }

                randomPoint -= chambers[i].Chance;
            }

            return chambers[chambers.Count - 1];
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