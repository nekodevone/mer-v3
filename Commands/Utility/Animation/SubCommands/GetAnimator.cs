using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.Utility.Animation.SubCommands;

public class GetAnimator : ICommand
{
    public string Command => "getanimator";

    public string[] Aliases => ["ga"];

    public string Description => "Получение аниматора объекта";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        var player = Player.Get(sender);
        if (player is null)
        {
            response = "This command can't be run from the server console.";
            return false;
        }

        if (!ToolGunHandler.TryGetSelectedMapObject(player, out MapEditorObject mapEditorObject))
        {
            response = "You need to select an object first!";
            return false;
        }

        if (mapEditorObject.Base is not SerializableSchematic schematic)
        {
            response = "Объект не является схематом";
            return false;
        }

        var animController = AnimationController.Get(schematic.SchematicObject);
        if (animController.Animators.IsEmpty())
        {
            response = "У объекта отсутствуют анимации!";
            return false;
        }
        
        var builder = StringBuilderPool.Shared.Rent();
        builder.AppendLine("Список аниматоров:");

        foreach (var animator in animController.Animators)
        {
            builder.AppendLine(animator.name);
        }

        response = StringBuilderPool.Shared.ToStringReturn(builder);
        return true;
    }
}