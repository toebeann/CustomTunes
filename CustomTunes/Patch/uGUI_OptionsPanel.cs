using Harmony;
using UnityEngine.SceneManagement;

namespace Straitjacket.Subnautica.Mods.CustomTunes.Patch
{
    [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.OnMasterVolumeChanged))]
    internal static class uGUI_OptionsPanel_OnMasterVolumeChanged
    {
        static void Postfix()
        {
            if (SceneManager.GetActiveScene().name == "Main")
            {
                CustomTunes.Main.CalculateVolume();
            }
        }
    }

    [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.OnMusicVolumeChanged))]
    internal static class uGUI_OptionsPanel_OnMusicVolumeChanged
    {
        static void Postfix()
        {
            if (SceneManager.GetActiveScene().name == "Main")
            {
                CustomTunes.Main.CalculateVolume();
            }
        }
    }
}
