using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using ProjectMER.Features;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;

namespace ProjectMER.Events.Handlers.Internal
{
    public class InvisibleTeleportEventsHandler : CustomEventsHandler
    {
        public override void OnPlayerSearchedToy(PlayerSearchedToyEventArgs ev)
        {
            if (!ev.Interactable.Transform.TryGetComponent<MapEditorObject>(out var merObject)) return;

            if (merObject.Base is not SerializableInteractableTeleport teleport) return;

            if (teleport.IsLocked) return;

            if (teleport.ThisTeleportID == "PutID" || teleport.ToTeleportID == "PutID") return;

            SerializableInteractableTeleport targetTeleport = null;

            foreach (var obj in MapUtils.LoadedMaps)
            {
                foreach (var teleporters in obj.Value.InvisibleTeleports.Where
                             (teleporters => teleporters.Value.ThisTeleportID == teleport.ToTeleportID))
                {
                    targetTeleport = teleporters.Value; break;
                }
            }

            if (targetTeleport is not null)
            {
                ev.Player.Position = targetTeleport.RoomToy.GetAbsolutePosition(targetTeleport.Position);
            }
            else
            {
                ev.Player.SendConsoleMessage($"""

                                              ( FATAL ERROR )
                                              {DateTime.Now.ToString()} - {ev.Player.Nickname}
                                              Teleport ({teleport.ThisTeleportID}) doesn`t found teleporter ({teleport.ToTeleportID})
                                              Обратитесь в Discord в баги!
                                              """, "red");
            }
        } 
    }
}