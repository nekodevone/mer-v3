using AdminToys;
using CommandSystem;
using NorthwoodLib.Pools;
using ProjectMER.Features;

namespace ProjectMER.Commands.Map;

public class SpawnedCount : ICommand
{
    public string Command => "spawnedcount";

    public string[] Aliases { get; } = ["sc"];

    public string Description => "Количество заспавленых объектов";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var sB = StringBuilderPool.Shared.Rent();
        sB.AppendLine($"<color=green><b>Заспавлено объектов всего - {MapUtils.LoadedMaps.Count}</b></color>");

        var countBlock = 0;
        foreach (var mapEditorObject in MapUtils.LoadedMaps.Values.SelectMany(mapEditorObjects => mapEditorObjects.Schematics.Values))
        {
            var count = mapEditorObject.SchematicObject.AdminToyBases.Count(atbase => atbase is PrimitiveObjectToy);
            sB.AppendLine(
                $"{mapEditorObject.SchematicObject.Name} - Количество примитивов: {count}");
            countBlock += count;
        }

        sB.AppendLine($"<color=green><b>Заспавнено примитивов всего - {countBlock}</b></color>");

        response = StringBuilderPool.Shared.ToStringReturn(sB);
        return true;
    }
}