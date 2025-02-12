using InventorySystem.Items;
using LabApi.Features.Wrappers;

namespace ProjectMER.Features.Extensions;

public static class ToolGunExtensions
{
    public static bool IsToolGun(this Item item, out ToolGun toolGun)
    {
		if (item == null)
		{
			toolGun = null!;
			return false;
		}

        return IsToolGun(item.Base, out toolGun);
    }

    public static bool IsToolGun(this ItemBase itemBase, out ToolGun toolGun)
    {
		if (itemBase == null)
		{
			toolGun = null!;
			return false;
		}

        return ToolGun.Dictionary.TryGetValue(itemBase.ItemSerial, out toolGun);
    }
}
