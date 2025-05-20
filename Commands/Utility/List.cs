using System.Text;
using CommandSystem;
using LabApi.Features.Permissions;
using NorthwoodLib.Pools;
using ProjectMER.Features;

namespace ProjectMER.Commands.Utility;

/// <summary>
/// Command used for listing all saved maps and schematics.
/// </summary>
public class List : ICommand
{
	/// <inheritdoc/>
	public string Command => "list";

	/// <inheritdoc/>
	public string[] Aliases { get; } = ["li", "ls"];

	/// <inheritdoc/>
	public string Description => "Shows the list of all available maps.";

	/// <inheritdoc/>
	public bool SanitizeResponse => false;

	/// <inheritdoc/>
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission($"mpr.{Command}"))
		{
			response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
			return false;
		}

		StringBuilder builder = StringBuilderPool.Shared.Rent();

		builder.AppendLine();
		builder.AppendLine();
		builder.Append("<color=green><b>List of maps:</b></color>");

		foreach (string filePath in Directory.GetFiles(ProjectMER.MapsDir))
		{
			builder.AppendLine();
			builder.Append($"- <color=yellow>{Path.GetFileNameWithoutExtension(filePath)}</color>");
		}

		builder.AppendLine();
		builder.AppendLine();
		builder.Append("<color=orange><b>List of schematics:</b></color>");

		foreach (string schematicName in MapUtils.GetAvailableSchematicNames())
		{
			builder.AppendLine();
			builder.Append($"- <color=yellow>{schematicName}</color>");
		}

		response = StringBuilderPool.Shared.ToStringReturn(builder);
		return true;
	}
}