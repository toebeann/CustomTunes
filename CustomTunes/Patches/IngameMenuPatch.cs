using HarmonyLib;

namespace Straitjacket.Subnautica.Mods.CustomTunes.Patches
{
    internal static class IngameMenuPatch
    {
        #region IngameMenu.ChangeSubscreen
        [HarmonyPatch(typeof(IngameMenu), nameof(IngameMenu.ChangeSubscreen))]
        [HarmonyPrefix]
        static void ChangeSubscreenPrefix(string newScreen)
        {
            if (newScreen == "Main")
            {
                CustomTunes.Main.Pause();
            }
        }
        #endregion

        #region IngameMenu.OnDeselect
        [HarmonyPatch(typeof(IngameMenu), nameof(IngameMenu.OnDeselect))]
        [HarmonyPrefix]
        static void OnDeselectPrefix()
        {
            CustomTunes.Main.Pause();
        }
        #endregion
    }
}
