using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using ProjectMER.Features;
using Utils.NonAllocLINQ;

namespace ProjectMER.Events;

public class MapOnEventHandlers : CustomEventsHandler
{
	public override void OnServerWaitingForPlayers() => HandleActionList(ProjectMER.Singleton.Config!.OnWaitingForPlayers);
	public override void OnServerRoundStarted() => HandleActionList(ProjectMER.Singleton.Config!.OnRoundStarted);
	public override void OnServerLczDecontaminationStarted() => HandleActionList(ProjectMER.Singleton.Config!.OnLczDecontaminationStarted);
    public override void OnWarheadStarted(WarheadStartedEventArgs ev) => HandleActionList(ProjectMER.Singleton.Config!.OnWarheadStarted);
    public override void OnWarheadStopped(WarheadStoppedEventArgs ev) => HandleActionList(ProjectMER.Singleton.Config!.OnWarheadStopped);
    public override void OnWarheadDetonated(WarheadDetonatedEventArgs ev) => HandleActionList(ProjectMER.Singleton.Config!.OnWarheadDetonated);

	private void HandleActionList(List<string> list)
	{
		foreach (string element in list)
		{
			string[] split = element.Split(':');
			string action = split[0];
			string argument = split[1];

			switch (action)
			{
				case "load":
					LoadMap(argument);
					return;

				case "unload":
					UnloadMap(argument);
					return;

				default:
					Logger.Error($"Unknown action: {action}");
					return;
			}
		}
	}

	private void LoadMap(string mapName)
	{
		/*
		if (mapName == "*")
		{
			MapUtils.LoadedMaps.Keys.ToList().ForEach<string>(x => MapUtils.LoadMap(x));
			return;
		}
		*/

		MapUtils.LoadMap(mapName);
	}

	private void UnloadMap(string mapName)
	{
		if (mapName == "*")
		{
			MapUtils.LoadedMaps.Keys.ToList().ForEach<string>(x => MapUtils.UnloadMap(x));
			return;
		}

		MapUtils.UnloadMap(mapName);
	}
}
