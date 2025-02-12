using LabApi.Events.CustomHandlers;
using ProjectMER.Features;

namespace ProjectMER.Events;

public class EventsHandler : CustomEventsHandler
{
	public override void OnServerWaitingForPlayers()
	{
		PrefabManager.RegisterPrefabs();
	}
}
