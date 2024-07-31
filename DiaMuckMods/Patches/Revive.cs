using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiaMuckMods.Patches
{
    public class Revive
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GraveInteract), "Update")]
        public static void Update(ref GraveInteract __instance, ref int ___id)
        {
            if (GameManager.state == GameManager.GameState.GameOver)
            {
                return;
            }
            if (__instance.timeLeft <= 0f)
            {
                ClientSend.RevivePlayer(__instance.playerId, ___id, true);
            }
        }
    }
}
