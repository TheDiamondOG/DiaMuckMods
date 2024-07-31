using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiaMuckMods.Patches
{
    public class CostChangeChest
    {
        [HarmonyPatch(typeof(GameManager), "ChestPriceMultiplier")]
        private static float ChestPriceMultiplier_Patch(float price)
        {
            return 0f * price;
        }
    }
}
