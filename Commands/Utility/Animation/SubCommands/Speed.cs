using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.Utility.Animation.SubCommands;

public class Speed : ICommand
{
    public string Command => "speed";

    public string[] Aliases => ["speed"];

    public string Description => "Изменить скорость анимации";
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        var player = Player.Get(sender);
        if (player is null)
        {
            response = "This command can't be run from the server console.";
            return false;
        }

        if (arguments.IsEmpty() || arguments.At(0) is null)
        {
            response = "Вы не указали новую скорость (0-1)";
            return false;
        }

        if (!float.TryParse(arguments.At(0), out var speed))
        {
            response = "Вы не правильно указали новую скорость (0-1)";
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

        foreach (var animator in animController.Animators)
        {
            animator.speed = speed;
        }

        response = "Скорость изменена";
        return true;
    }
}