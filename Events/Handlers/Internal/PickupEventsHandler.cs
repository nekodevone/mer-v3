using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using ProjectMER.Features.Objects;

namespace ProjectMER.Events.Handlers.Internal;

public class PickupEventsHandler : CustomEventsHandler
{
	internal static readonly Dictionary<ushort, SchematicObject> ButtonPickups = [];

	public override void OnPlayerSearchingPickup(PlayerSearchingPickupEventArgs ev)
	{
		if (!ButtonPickups.TryGetValue(ev.Pickup.Serial, out SchematicObject schematic))
			return;

		ev.IsAllowed = false;
		Schematic.OnButtonInteracted(new(ev.Pickup, ev.Player, schematic));
	}
}
