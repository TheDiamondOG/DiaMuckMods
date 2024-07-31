using HarmonyLib;

namespace DiaMuckMods.Patches
{
    public class GameOvered
    {
        [HarmonyPatch(typeof(GameManager))]
        [HarmonyPatch("GameOver")]
        public class GameManager_GameOver_Patch
        {
            static void Postfix(GameManager __instance)
            {
                __instance.Invoke("ShowEndScreen", 4f);
            }
        }
    }
}
