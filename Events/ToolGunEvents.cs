using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Extensions;

namespace ProjectMER.Events;

public class ToolGunEvents : CustomEventsHandler
{
	public override void OnServerRoundStarted()
	{
		Timing.RunCoroutine(ToolGunGUI());
	}

	private static IEnumerator<float> ToolGunGUI()
	{
		while (true)
		{
			// string output = File.ReadAllText(Path.Combine(ProjectMER.SchematicsDir, "hint.txt"));

			yield return Timing.WaitForSeconds(0.1f);
			foreach(ToolGun toolGun in ToolGun.Dictionary.Values)
			{
				if (!toolGun.Equiped)
					continue;

				toolGun.Player.SendHint(toolGun.GetHintHUD(), 0.25f);
			}
			// foreach (Player player in Player.List)
			// {
				// if (!player.CurrentItem.IsToolGun(out ToolGun toolGun))
					//continue;

				// if (!Room.TryGetRoomAtPosition(player.Camera.transform.position, out Room? room))
					// continue;

				//player.SendHint(toolGun.GetHintHUD(), 0.25f);
				// player.SendHint(output, 1f);
				// player.SendHint($"{(ToolGun.List[toolGun.ObjectToSpawnIndex].Replace("Serializable", ""))}\n{room.Zone}_{room.Shape}_{room.Name}", 0.25f);
			// }
		}
	}

	// public override void OnPlayerShootingWeapon(PlayerShootingWeaponEventArgs ev)
	public override void OnPlayerDryFiringWeapon(PlayerDryFiringWeaponEventArgs ev)
	{
		if (!ev.Weapon.IsToolGun(out ToolGun toolGun))
			return;

		ev.IsAllowed = false;
		toolGun.Shot(ev.Player);
	}

	public override void OnPlayerReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
	{
		if (!ev.Weapon.IsToolGun(out ToolGun toolGun))
			return;

		ev.IsAllowed = false;
		toolGun.ObjectToSpawnIndex--;
	}

    public override void OnPlayerDroppingItem(PlayerDroppingItemEventArgs ev)
	{
		if (!ev.Item.IsToolGun(out ToolGun toolGun))
			return;

		ev.IsAllowed = false;
		toolGun.ObjectToSpawnIndex++;
	}
}
