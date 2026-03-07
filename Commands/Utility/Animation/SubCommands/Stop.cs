using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.Utility.Animation.SubCommands;

public class Stop : ICommand
{
    public string Command => "stop";

    public string[] Aliases => ["stop"];
    
    public string Description => "Остановить проигрывания анимации";
    
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

        if (arguments.IsEmpty() || arguments.At(0) is null)
        {
            foreach (var animator in animController.Animators)
            {
                animController.Stop(animator.name);
            }
        }
        else
        {
            animController.Stop(arguments.At(0));
        }

        response = "Анимация остановлена";
        return true;
    }
}