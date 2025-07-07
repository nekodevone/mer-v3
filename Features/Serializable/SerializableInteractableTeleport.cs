using AdminToys;
using LabApi.Features.Wrappers;
using MapGeneration;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable
{
    public class SerializableInteractableTeleport : SerializableObject, IIndicatorDefinition
    {
        public string ToTeleportID { get; set; } = "PutID";
        public string ThisTeleportID { get; set; } = "PutID";
        public float InteractionDuration { get; set; } = 1;

        public Room RoomToy { get; set; }
        public bool IsLocked { get; set; } = false;
        public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
        {
            InvisibleInteractableToy interactabletoy = instance == null ? UnityEngine.Object.Instantiate(TargetPrefab) : instance.GetComponent<InvisibleInteractableToy>();
            Vector3 position = room.GetAbsolutePosition(Position);
            Quaternion rotation = room.GetAbsoluteRotation(Rotation);

            interactabletoy.transform.SetPositionAndRotation(position, rotation);
            interactabletoy.transform.localScale = Scale;

            InteractableToy.Get(interactabletoy);

            interactabletoy.NetworkInteractionDuration = InteractionDuration;
            interactabletoy.NetworkIsLocked = IsLocked;
            interactabletoy.NetworkScale = Scale;
            interactabletoy.NetworkPosition = interactabletoy.transform.position;
            interactabletoy.NetworkRotation = interactabletoy.transform.rotation;
            if (room is not null)
            {
                RoomToy = room;
            }
            else
            {
                if (interactabletoy.gameObject.transform.position.TryGetRoom(out var roomToy))
                {
                    RoomToy = LabApi.Features.Wrappers.Room.Get(roomToy);
                }
            }

            if (instance == null)
                NetworkServer.Spawn(interactabletoy.gameObject);

            return interactabletoy.gameObject;
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
            }
            else
            {
                cube = instance.GetComponent<PrimitiveObjectToy>();
            }

            cube.transform.SetPositionAndRotation(position, rotation);
            cube.transform.localScale = Scale;
            cube.NetworkScale = Scale;

            return cube.gameObject;
        }
        private InvisibleInteractableToy TargetPrefab => PrefabManager.InteractableToyPrefab;
    }
}