using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace ProjectMER.Commands.Modifying.Position.SubCommands;

/// <summary>
/// Grabs a specific <see cref="MapEditorObject"/>.
/// </summary>
public class Grab : ICommand
{
	public string Command => "grab";

	public string[] Aliases => [];

	public string Description => "Grabs an object.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		Player? player = Player.Get(sender);
		if (player is null)
		{
			response = "This command can't be run from the server console.";
			return false;
		}

		if (!ToolGun.TryGetSelectedMapObject(player, out MapEditorObject mapEditorObject))
		{
			response = "You need to select an object first!";
			return false;
		}

		if (GrabbingPlayers.ContainsKey(player))
		{
			Timing.KillCoroutines(GrabbingPlayers[player]);
			GrabbingPlayers.Remove(player);

			mapEditorObject.Base.Position = mapEditorObject.Room.Transform.InverseTransformPoint(mapEditorObject.transform.position).ToString("G");
			mapEditorObject.UpdateObjectAndCopies();

			response = "Ungrabbed";
			return true;
		}

		GrabbingPlayers.Add(player, Timing.RunCoroutine(GrabbingCoroutine(player, mapEditorObject)));

		response = "Grabbed";
		return true;
	}

	private IEnumerator<float> GrabbingCoroutine(Player player, MapEditorObject mapEditorObject)
	{
		Vector3 position = player.Camera.position;
		float multiplier = Vector3.Distance(position, mapEditorObject.transform.position);
		Vector3 prevPos = position + (player.Camera.forward * multiplier);

		while (true)
		{
			yield return Timing.WaitForOneFrame;

			if (mapEditorObject == null || !ToolGun.TryGetSelectedMapObject(player, out _))
				break;

			Vector3 newPos = mapEditorObject.transform.position = player.Camera.position + (player.Camera.forward * multiplier);

			if (prevPos == newPos)
				continue;

			prevPos = newPos;
			mapEditorObject.transform.position = prevPos;
		}

		GrabbingPlayers.Remove(player);
		if (mapEditorObject != null)
		{
			mapEditorObject.Base.Position = mapEditorObject.Room.Transform.InverseTransformPoint(mapEditorObject.transform.position).ToString("G");
			mapEditorObject.UpdateObjectAndCopies();
		}
	}

	/// <summary>
	/// The <see cref="Dictionary{TKey, TValue}"/> which contains all <see cref="Player"/> and <see cref="CoroutineHandle"/> pairs.
	/// </summary>
	private static readonly Dictionary<Player, CoroutineHandle> GrabbingPlayers = [];
}
