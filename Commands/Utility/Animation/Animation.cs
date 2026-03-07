using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using ProjectMER.Commands.Modifying.Scale;
using ProjectMER.Commands.Utility.Animation.SubCommands;

namespace ProjectMER.Commands.Utility.Animation;

public class Animation : ParentCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Scale"/> class.
    /// </summary>
    public Animation() => LoadGeneratedCommands();
    
    public override void LoadGeneratedCommands()
    {
        RegisterCommand(new Play());
        RegisterCommand(new Stop());
        RegisterCommand(new GetAnimator());
    }

    public override string Command => "animation";

    public override string[] Aliases => ["anim"];

    public override string Description => "Команда управления анимацией";

    public override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        Player? player = Player.Get(sender);
        if (player is null)
        {
            response = "This command can't be run from the server console.";
            return false;
        }

        response = "\nUsage:\n";
        response += "mp animation stop\n";
        response += "mp animation play {animation name} {animator name}";
        response += "mp animation getanimator";
        return false;
    }
}