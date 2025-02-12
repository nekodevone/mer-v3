using CommandSystem;
using ProjectMER.Features;

namespace ProjectMER.Commands.Map;

public class Load : ICommand
{
	public string Command => "load";

	public string[] Aliases => ["l"];

	public string Description => "Loads a map";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (arguments.Count == 0)
		{
			response = "You need to provide a map name!";
			return false;
		}

		if (MapUtils.LoadMap(arguments.At(0)))
		{
			response = $"{arguments.At(0)} map has been loaded!";
			return true;
		}
		else
		{
			response = $"{arguments.At(0)} map could not be loaded. Please check server console.";
			return false;
		}
	}
}
