using Harmony;
using UWE;

namespace Straitjacket.Subnautica.Mods.CustomTunes.Patch
{
    [HarmonyPatch(typeof(IngameMenu), nameof(IngameMenu.ChangeSubscreen))]
    internal static class IngameMenu_ChangeSubscreen
    {
        static void Prefix(string newScreen)
        {
            if (newScreen == "Main")
            {
                CustomTunes.Main.Pause();
            }
        }
    }

    [HarmonyPatch(typeof(IngameMenu), nameof(IngameMenu.OnDeselect))]
    internal static class IngameMenu_OnDeselect
    {
        static void Prefix()
        {
            CustomTunes.Main.Pause();
        }
    }
}
