using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using ProjectMER.Features;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.Utility;

public class Lock : ICommand
{
    public string Command => "lock";
 
    public string[] Aliases { get; }

    public string Description => "Блокирует удаление выделенного схемата.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission($"mpr.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        var player = Player.Get(sender);

        if (!ToolGunHandler.TryGetSelectedMapObject(player!, out var mapEditorObject))
        {
            response = "You haven't selected any object!";
            return false;
        }

        if (MapUtils.LockedObjects.Contains(mapEditorObject))
        {
            MapUtils.LockedObjects.Remove(mapEditorObject);
            response = "Объект был разблокирован!";
            return true;
        }

        MapUtils.LockedObjects.Add(mapEditorObject);
        response = "Объект был заблокирован!";
        return true;
    }
}