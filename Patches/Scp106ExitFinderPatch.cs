using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using PlayerRoles.PlayableScps.Scp106;
using ProjectMER.Features.Objects;

namespace ProjectMER.Patches;

[HarmonyPatch(typeof(Scp106PocketExitFinder), nameof(Scp106PocketExitFinder.ValidateDoor))]
public class Scp106ExitFinderPatch
{
    private static void Postfix(DoorVariant dv, ref bool __result)
    {
        __result = __result && dv.gameObject.GetComponent<MapEditorObject>() is null;
    }
}