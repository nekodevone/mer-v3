using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
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
			yield return Timing.WaitForSeconds(0.1f);

			foreach (Player player in Player.List)
			{
				if (!player.CurrentItem.IsToolGun(out ToolGun toolGun))
					continue;

				string hud;
				try
				{
					hud = toolGun.GetHintHUD(player);
				}
				catch (Exception e)
				{
					Logger.Error(e);
					hud = "ERROR: Check server console";
				}

				player.SendHint(hud, 0.25f);
			}
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
