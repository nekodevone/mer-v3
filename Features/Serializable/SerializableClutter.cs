using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable
{
    public class SerializableClutter : SerializableObject, IIndicatorDefinition
    {
        public ClutterType ClutterType { get; set; } = ClutterType.HCZOpenHallway_Clutter_F;

        public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
        {
            GameObject clutterObject;
            Vector3 position = room.GetAbsolutePosition(Position);
            Quaternion rotation = room.GetAbsoluteRotation(Rotation);
            _prevIndex = Index;

            clutterObject = instance == null ? Object.Instantiate(ClutterPrefab) : instance;

            clutterObject.transform.SetPositionAndRotation(position, rotation);
            clutterObject.transform.localScale = Scale;

            _prevType = ClutterType;

            NetworkServer.UnSpawn(clutterObject.gameObject);
            NetworkServer.Spawn(clutterObject.gameObject);

            return clutterObject.gameObject;
        }
        public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
        {
            PrimitiveObjectToy cube;

            Vector3 position = room.GetAbsolutePosition(Position);
            Quaternion rotation = room.GetAbsoluteRotation(Rotation);

            if (instance == null)
            {
                cube = Object.Instantiate(PrefabManager.PrimitiveObject);
                cube.NetworkPrimitiveType = PrimitiveType.Cube;
                cube.NetworkPrimitiveFlags = PrimitiveFlags.Visible;
                cube.NetworkMaterialColor = new Color(1f, 1f, 1f, 0.9f);
                cube.transform.localScale = Vector3.one * 0.25f;
            }
            else
            {
                cube = instance.GetComponent<PrimitiveObjectToy>();
            }

            cube.transform.SetPositionAndRotation(position, rotation);

            return cube.gameObject;
        }
        private GameObject ClutterPrefab
        {
            get
            {
                GameObject prefab = ClutterType switch
                {
                    /*ClutterType.HCZOneSided => PrefabManager.HczOneSidedPrefab.gameObject,
                    ClutterType.HCZTwoSided => PrefabManager.HczTwoSidedPrefab.gameObject,
                    ClutterType.HCZOpenHallway => PrefabManager.HczOpenHallwayPrefab.gameObject,*/
                    ClutterType.HCZOpenHallway_Construct_A => PrefabManager.HczOpenHallwayConstructAPrefab.gameObject,
                    ClutterType.HCZOpenHallway_Clutter_A => PrefabManager.HczOpenHallwayClutterAPrefab.gameObject,
                    ClutterType.HCZOpenHallway_Clutter_B => PrefabManager.HczOpenHallwayClutterBPrefab.gameObject,
                    ClutterType.HCZOpenHallway_Clutter_C => PrefabManager.HczOpenHallwayClutterCPrefab.gameObject,
                    ClutterType.HCZOpenHallway_Clutter_D => PrefabManager.HczOpenHallwayClutterDPrefab.gameObject,
                    ClutterType.HCZOpenHallway_Clutter_E => PrefabManager.HczOpenHallwayClutterEPrefab.gameObject,
                    ClutterType.HCZOpenHallway_Clutter_F => PrefabManager.HczOpenHallwayClutterFPrefab.gameObject,
                    ClutterType.HCZOpenHallway_Clutter_G => PrefabManager.HczOpenHallwayClutterGPrefab.gameObject,
                    _ => throw new InvalidOperationException(),
                };

                return prefab;
            }
        }

        public override bool RequiresReloading => ClutterType != _prevType || base.RequiresReloading;

        internal ClutterType _prevType;
    }

}
