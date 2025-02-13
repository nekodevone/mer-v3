using CommandSystem;
using LabApi.Features.Wrappers;
using ProjectMER.Features;

namespace ProjectMER.Commands;

public class ToggleToolGun : ICommand
{
	public string Command => "toolgun";

	public string[] Aliases => ["tg"];

	public string Description => "Tool gun for spawning and editing objects.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		Player? player = Player.Get(sender);
		if (player is null)
		{
			response = "This command can't be run from the server console.";
			return false;
		}

		if (ToolGun.Remove(player))
		{
			response = "You no longer have a Tool Gun!";
			return true;
		}

		if (ToolGun.TryAdd(player))
		{
			response = "You now have the Tool Gun!";
			return true;
		}

		response = "You have a full inventory!";
		return false;
	}
}
