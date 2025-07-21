using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using Mirror;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using LabApi.Features.Wrappers;
using UnityEngine;
using YamlDotNet.Serialization;
using LockerChamber = MapGeneration.Distributors.LockerChamber;

namespace ProjectMER.Features.Serializable
{
    public class SerializablePedestalScp : SerializableObject
    {
        public PedestalType PedestalType { get; set; } = PedestalType.Scp207;

        public ItemType ItemContainer { get; set; } = ItemType.SCP207;

        public bool ShuffleChambers { get; set; } = true;

        public DoorPermissionFlags KeycardPermissions { get; set; } = DoorPermissionFlags.None;
        [IgnoreToolgunGUI]
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

            if (instance == null)
            {
                lockerVariant = (GameObject.Instantiate<MapGeneration.Distributors.Locker>(PedestalPrefab));
            }
            else
            {
                lockerVariant = (instance.gameObject.GetComponent<MapGeneration.Distributors.Locker>());
            }

            Pedestal = lockerVariant;

            SetupLocker(lockerVariant);

            StructurePositionSync = lockerVariant.GetComponent<StructurePositionSync>();

            lockerVariant.transform.SetPositionAndRotation(position, rotation);
            lockerVariant.transform.localScale = Scale;

            StructurePositionSync.Network_position = (lockerVariant.transform.position);
            StructurePositionSync.Network_rotationY = (sbyte)Mathf.RoundToInt(lockerVariant.transform.eulerAngles.y / 5.625f);

            _prevType = PedestalType;

            NetworkServer.UnSpawn(lockerVariant.gameObject);
            NetworkServer.Spawn(lockerVariant.gameObject);

            return lockerVariant.gameObject;
        }


        public void SetupLocker(MapGeneration.Distributors.Locker locker)
        {
            Pedestal.Loot = Array.Empty<LockerLoot>();
            HandleItems();
            IsSpawnedLoot = true;
        }
        private void HandleItems()
        {
            foreach (LockerChamber lockerChamber in Pedestal.Chambers)
                lockerChamber.RequiredPermissions = (DoorPermissionFlags)KeycardPermissions;

            for (int i = 0; i < Pedestal.Chambers.Length; i++)
            {
                Pedestal.Chambers.ElementAt(i).SpawnItem(ItemContainer, 1);
                break;
            }
        }


        private MapGeneration.Distributors.Locker Pedestal;

        private MapGeneration.Distributors.Locker PedestalPrefab
        {
            get
            {
                MapGeneration.Distributors.Locker prefab = PedestalType switch
                {
                    PedestalType.Scp018 => PrefabManager.PedestalScp018,
                    PedestalType.Scp1853 => PrefabManager.PedestalScp1853,
                    PedestalType.Scp244 => PrefabManager.PedestalScp244,
                    PedestalType.Scp268 => PrefabManager.PedestalScp268,
                    PedestalType.Scp2176 => PrefabManager.PedestalScp2176,
                    PedestalType.Scp1576 => PrefabManager.PedestalScp1576,
                    PedestalType.Scp500 => PrefabManager.PedestalScp500,
                    PedestalType.Scp1344 => PrefabManager.PedestalScp1344,
                    PedestalType.AntiScp207 => PrefabManager.PedestalAntiScp207,
                    PedestalType.Scp207 => PrefabManager.PedestalAntiScp207,
                    _ => throw new InvalidOperationException(),
                };

                return prefab;
            }
        }

        public override bool RequiresReloading => PedestalType != _prevType || base.RequiresReloading;

        internal PedestalType _prevType;
    }
}