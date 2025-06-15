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
        
        Player? player = Player.Get(sender)!;

        if (!TryGetTarget(arguments, sender, out var target))
        {
            response = "Введены некорректные данные";
            return false;
        }

        if (!ToolGunHandler.TryGetSelectedMapObject(player, out MapEditorObject mapEditorObject))
        {
            response = "You haven't selected any object!";
            return false;
        }

        if (AttachedSchematic.ContainsKey(player))
        {
            response = "На игроке уже весит схемат!";
            return true;
        }

        var schematic = mapEditorObject.Map.Schematics[mapEditorObject.Id];

        if (schematic == null || schematic.SchematicObject == null)
        {
            response = "Не получилось получить схемат!";
            return false;
        }

        AttachSchematic(target, schematic.SchematicObject, arguments.At(1));
        response = "Схемат был привязан к игроку!!";
        return true;
    }

    private static void AttachSchematic(Player player, SchematicObject schematicObject, string slug)
    {
        if (AttachedSchematic.ContainsKey(player))
        {
            return;
        }

        if (!slug.IsEmpty())
        {
            schematicObject.gameObject.transform.parent = player.GameObject.transform;
            AttachedSchematic[player] = schematicObject;
            return;
        }

        if (!player.IsSCP)
        {
            var hitboxIdentity = player.ReferenceHub.GetModel().Hitboxes.FirstOrDefault(hb => hb.name == slug);

            if (hitboxIdentity is null)
            {
                return;
            }

            schematicObject.gameObject.transform.parent = hitboxIdentity.transform;
            AttachedSchematic[player] = schematicObject;
            return;
        }

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (player.Role)
        {
            case RoleTypeId.Scp3114:
            {
                var newSlug = Scp3114HitboxesSlug[slug];

                var hitboxIdentity = player.ReferenceHub.GetModel().Hitboxes.FirstOrDefault(hb => hb.name == newSlug);

                if (hitboxIdentity is null)
                {
                    return;
                }

                schematicObject.gameObject.transform.parent = hitboxIdentity.transform;
                break;
            }
            case RoleTypeId.Scp0492:
            {
                var newSlug = Scp0492HitboxesSlug[slug];

                var hitboxIdentity = player.ReferenceHub.GetModel().Hitboxes.FirstOrDefault(hb => hb.name == newSlug);

                if (hitboxIdentity is null)
                {
                    return;
                }

                schematicObject.gameObject.transform.parent = hitboxIdentity.transform;
                break;
            }
            default:
                schematicObject.gameObject.transform.parent = player.GameObject.transform;
                break;
        }

        AttachedSchematic[player] = schematicObject;
    }

    /// <summary>
    /// Получаем игрока
    /// </summary>
    private bool TryGetTarget(ArraySegment<string> arguments, ICommandSender sender, out Player? player)
    {
        if (!arguments.Any() && Player.TryGet(sender, out player))
        {
            return true;
        }

        if (int.TryParse(arguments.At(0), out var id) && Player.TryGet(id, out player))
        {
            return true;
        }

        player = null;
        return false;
    }
}