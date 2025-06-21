using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using PlayerRoles;
using ProjectMER.Features.Objects;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.Utility;

public class Attach : ICommand
{
    public string Command => "attachschematics";

    public string[] Aliases => ["as"];

    public string Description => "Привязывает к игроку схемат";

    public static Dictionary<Player, SchematicObject> AttachedSchematic = new();

    private static Dictionary<string, string> Scp3114HitboxesSlug { get; } = new()
    {
        { "Thigh.L", "mixamorig:LeftUpLeg" },
        { "leg.L", "mixamorig:LeftLeg" },
        { "Thigh.R", "mixamorig:RightUpLeg" },
        { "leg.R", "mixamorig:RightLeg" },
        { "SpineMiddle", "mixamorig:Spine" },
        { "Arm.L", "mixamorig:LeftArm" },
        { "Forearm.L", "mixamorig:LeftForeArm" },
        { "Head", "mixamorig:Head" },
        { "Arm.R", "mixamorig:RightArm" },
        { "Forearm.R", "mixamorig:RightForeArm" },
    };

    private static Dictionary<string, string> Scp0492HitboxesSlug { get; } = new()
    {
        { "Thigh.L", "mixamorig:LeftUpLeg" },
        { "leg.L", "mixamorig:LeftLeg" },
        { "Thigh.R", "mixamorig:RightUpLeg" },
        { "leg.R", "mixamorig:RightLeg" },
        { "SpineMiddle", "mixamorig:Spine2" },
        { "Arm.L", "mixamorig:LeftArm" },
        { "Forearm.L", "mixamorig:LeftForeArm" },
        { "Head", "mixamorig:HeadTop_End" },
        { "Arm.R", "mixamorig:RightArm" },
        { "Forearm.R", "mixamorig:RightForeArm" },
    };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasAnyPermission($"mpr.{Command}"))
        {
            response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
            return false;
        }

        Player target;

        target = arguments.At(0) != null ? Player.Get(arguments.At(0))! : Player.Get(sender)!;

        if (AttachedSchematic.ContainsKey(target))
        {
            response = "На игроке уже весит схемат!";
            return true;
        }

        if (!ToolGunHandler.TryGetSelectedMapObject(Player.Get(sender)!, out MapEditorObject mapEditorObject))
        {
            response = "You haven't selected any object!";
            return false;
        }

        if (AttachedSchematic.ContainsKey(target))
        {
            SchematicUnfollow(AttachedSchematic[target]);
            response = "Схемат снят с игрока!";
            return true;
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
        schematicObject.gameObject.transform.position = player.Position;
        schematicObject.gameObject.transform.parent = player.GameObject.transform;
        AttachedSchematic[player] = schematicObject;
    }

    /// <summary>
    /// Отвязывает схемат от игрок
    /// </summary>
    /// <param name="schem">Схемат.</param>
    private static void SchematicUnfollow(SchematicObject schem)
    {
        schem.transform.parent = schem.OriginTransform;
        AttachedSchematic.Remove(schem.AttachedPlayer!);
        schem.AttachedPlayer = null;
    }
}