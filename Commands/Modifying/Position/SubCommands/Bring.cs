using CommandSystem;
using LabApi.Features.Wrappers;
using ProjectMER.Features;
using ProjectMER.Features.Objects;

namespace ProjectMER.Commands.Modifying.Position.SubCommands;

/// <summary>
/// Modifies object's position by setting it to the sender's current position.
/// </summary>
public class Bring : ICommand
{
	/// <inheritdoc/>
	public string Command => "bring";

	/// <inheritdoc/>
	public string[] Aliases { get; } = [];

	/// <inheritdoc/>
	public string Description => "Brings an object to player's position.";

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

		mapEditorObject.Base.Position = mapEditorObject.Room.Transform.InverseTransformPoint(player.Position).ToString("F3");
		mapEditorObject.UpdateObjectAndCopies();

		response = mapEditorObject.Base.Position;
		return true;
	}
}