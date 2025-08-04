using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.Utility;

public class Find : ICommand
{
    public string Command => "find";
 
    public string[] Aliases { get; }

    public string Description => "Телепортирует к объекту по его id. Используется если вы потеряли объект.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission($"mpr.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        if (arguments.Count < 1)
        {
            response = "Введите значение ID!";
            return true;
        }

        var slug = arguments.At(1);
        if (ToolGunHandler.TryGetObjectById(slug, out MapEditorObject idObject))
        {
            Player.Get(sender)!.Position = idObject.Room.GetAbsolutePosition(idObject.transform.position);
            response = "Вы были телепортированы!";
            return true;
        }

        response = $"Не удалось найти объект с ID {slug}!";
        return false;
    }
}