using CommandSystem;
using LabApi.Features.Wrappers;
using ProjectMER.Commands.Modifying.Rotation.SubCommands;

namespace ProjectMER.Commands.Modifying.Rotation;

/// <summary>
/// Command used for modifing object's rotation.
/// </summary>
public class Rotation : ParentCommand
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Rotation"/> class.
	/// </summary>
	public Rotation() => LoadGeneratedCommands();

	/// <inheritdoc/>
	public override string Command => "rotation";

	/// <inheritdoc/>
	public override string[] Aliases { get; } = { "rot" };

	/// <inheritdoc/>
	public override string Description => "Modifies object's rotation.";

	/// <inheritdoc/>
	public override void LoadGeneratedCommands()
	{
		RegisterCommand(new Add());
		RegisterCommand(new Set());
		// RegisterCommand(new Rotate());
	}

	/// <inheritdoc/>
	protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		Player? player = Player.Get(sender);
		if (player is null)
		{
			response = "This command can't be run from the server console.";
			return false;
		}

		/*
		Player player = Player.Get(sender);
		if (player.TryGetSessionVariable(SelectedObjectSessionVarName, out MapEditorObject mapObject) && mapObject != null)
		{
			response = $"Object current rotation: {mapObject.RelativeRotation}\n";
			return true;
		}
		*/

		response = "\nUsage:";
		response += "\nmp rotation set (x) (y) (z)";
		response += "\nmp rotation add (x) (y) (z)";
		return false;
	}
}