using CommandSystem;
using LabApi.Features.Wrappers;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;
using static ProjectMER.Features.Extensions.VectorExtensions;

namespace ProjectMER.Commands.Modifying.Position.SubCommands;

/// <summary>
/// Modifies object's position by setting it to a certain value.
/// </summary>
public class Set : ICommand
{
	/// <inheritdoc/>
	public string Command => "set";

	/// <inheritdoc/>
	public string[] Aliases { get; } = Array.Empty<string>();

	/// <inheritdoc/>
	public string Description => string.Empty;

	/// <inheritdoc/>
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

		if (arguments.Count >= 3 && TryGetVector(arguments.At(0), arguments.At(1), arguments.At(2), out Vector3 newPosition))
		{
			mapEditorObject.Base.Position = newPosition.ToString("G");
			mapEditorObject.UpdateObjectAndCopies();

			response = mapEditorObject.Base.Position;
			return true;
		}

		response = "Invalid values.";
		return false;
	}
}
