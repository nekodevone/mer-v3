using AdminToys;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using UnityEngine;
using YamlDotNet.Serialization;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable
{
    public class SerializableElevatorChamber : SerializableObject, IIndicatorDefinition
    {
        public DoorPermissionFlags KeycardPermissions { get; set; }
        [IgnoreToolgunGUI]
        [YamlIgnore]
        public StructurePositionSync StructurePositionSync { get; set; }

        public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
        {
            ElevatorChamber text = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.ElevatorChamber) : instance.GetComponent<ElevatorChamber>();
            Vector3 position = room.GetAbsolutePosition(Position);
            Quaternion rotation = room.GetAbsoluteRotation(Rotation);
            _prevIndex = Index;
            

            StructurePositionSync = text.GetComponent<StructurePositionSync>();

            text.transform.SetPositionAndRotation(position, rotation);
            text.transform.localScale = Scale;

            StructurePositionSync.Network_position = (text.transform.position);
            StructurePositionSync.Network_rotationY = (sbyte)Mathf.RoundToInt(text.transform.eulerAngles.y / 5.625f);

            NetworkServer.UnSpawn(text.gameObject);
            NetworkServer.Spawn(text.gameObject);

            return text.gameObject;
        }
        public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
        {
            PrimitiveObjectToy cube;

            Vector3 position = room.GetAbsolutePosition(Position);
            Quaternion rotation = room.GetAbsoluteRotation(Rotation);

            if (instance == null)
            {
                cube = UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject);
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
    }

}
