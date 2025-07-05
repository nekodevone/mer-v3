using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using PlayerRoles;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Structs;
using ProjectMER.Features.ToolGun;
using Utils.NonAllocLINQ;

namespace ProjectMER.Commands.Utility;

public class Attach : ICommand
{
    public string Command => "attachschematics";

    public string[] Aliases => ["as"];

    public string Description => "Привязывает к игроку схемат";

    public static List<AttachedSchematic> AttachedSchematic = new ();

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission($"mpr.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        if (!TryGetTarget(arguments, sender, out var target) || target == null)
        {
            response = "Введены некорректные данные";
            return false;
        }

        if (AttachedSchematic.TryGetFirst(schem => schem.Player == target, out var schematicTarget))
        {
            SchematicUnfollow(schematicTarget);
            response = "Схемат снят с игрока!";
            return true;
        }

        if (!ToolGunHandler.TryGetSelectedMapObject(Player.Get(sender)!, out var mapEditorObject))
        {
            response = "You haven't selected any object!";
            return false;
        }

        var schematic = mapEditorObject.Map.Schematics[mapEditorObject.Id];

        if (schematic == null || schematic.SchematicObject == null)
        {
            response = "Не получилось получить схемат!";
            return false;
        }

        AttachSchematic(target, schematic.SchematicObject);
        response = "Схемат был привязан к игроку!!";
        return true;
    }

    private static void AttachSchematic(Player player, SchematicObject schematicObject)
    {
        var attachedSchematic = new AttachedSchematic()
        {
            Player = player,
            Schematic = schematicObject,
            OriginalTransform = schematicObject.transform
        };

        schematicObject.gameObject.transform.position = player.Position;
        schematicObject.gameObject.transform.rotation = player.Rotation;
        schematicObject.gameObject.transform.parent = player.GameObject.transform;
        AttachedSchematic.Add(attachedSchematic);
    }

    /// <summary>
    /// Отвязывает схемат от игрок
    /// </summary>
    /// <param name="schem">Схемат.</param>
    private static void SchematicUnfollow(AttachedSchematic attachedSchematic)
    {
        attachedSchematic.Schematic.AttachedPlayer = null;
        attachedSchematic.Schematic.transform.parent = null;
        AttachedSchematic.Remove(attachedSchematic);
    }

    /// <summary>
    /// Получаем игрока
    /// </summary>
    private static bool TryGetTarget(ArraySegment<string> arguments, ICommandSender sender, out Player? player)
    {
        if (!arguments.Any() && Player.TryGet(sender, out player))
        {
            return true;
        }

        player = null;
        return false;
    }
}