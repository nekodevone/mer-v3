using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.ToolGunLike;

public class Delete : ICommand
{
	/// <inheritdoc/>
	public string Command => "delete";

	/// <inheritdoc/>
	public string[] Aliases { get; } = ["del", "remove", "rm"];

	/// <inheritdoc/>
	public string Description => "Deletes the object which you are looking at.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission($"mpr.{Command}"))
		{
			response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
			return false;
		}

		Player? player = Player.Get(sender);
		if (player is null)
		{
			response = "This command can't be run from the server console.";
			return false;
		}

		if (arguments.Count > 1)
		{
			var slug = arguments.At(1);
			switch (arguments.At(0))
			{
				case "map":
					var map = MapUtils.LoadedMaps[slug];

					if (map is not null)
					{
						MapUtils.UnloadMap(slug);
						response = "Вы успешно удалили объект!";
						return true;
					}

					response = "Подобного объекта не существует!";
					return false;
				case "schematic":

					foreach (var obj in MapUtils.LoadedMaps.Where
						         (obj => obj.Value.Schematics.Any
							         (schematics =>
								         schematics.Value.SchematicName == slug)))
					{
						ToolGunHandler.DeleteSchematicObject(obj.Value);
						response = "Вы успешно удалили объект!";
						return true;
					}

					response = "Объекта не существует!";
					return false;
				case "id":
					if (ToolGunHandler.TryGetObjectById(slug, out MapEditorObject idObject))
					{
						ToolGunHandler.DeleteObject(idObject);
						response = "You've successfully deleted the object!";
						return true;
					}

					response = $"Unable to find object with ID of {slug}!";
					return false;
				default:
					response = "Введены неправильные аргументы!";
					return false;
			}
		}

		if (ToolGunHandler.TryGetMapObject(player, out MapEditorObject mapEditorObject))
		{
			ToolGunHandler.DeleteObject(mapEditorObject);
			response = "You've successfully deleted the object!";

			return true;
		}

		response = "You aren't looking at any Map Editor object!";
		return false;
	}
}
