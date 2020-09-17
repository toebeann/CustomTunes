using HarmonyLib;
using UnityEngine.SceneManagement;

namespace Straitjacket.Subnautica.Mods.CustomTunes.Patches
{
    internal static class uGUI_OptionsPanelPatch
    {
        #region uGUI_OptionsPanel.OnMasterVolumeChanged
        [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.OnMasterVolumeChanged))]
        [HarmonyPostfix]
        static void OnMasterVolumeChangedPostfix()
        {
            if (SceneManager.GetActiveScene().name == "Main")
            {
                CustomTunes.Main.CalculateVolume();
            }
        }
        #endregion

        #region uGUI_OptionsPanel.OnMusicVolumeChanged
        [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.OnMusicVolumeChanged))]
        [HarmonyPostfix]
        static void OnMusicVolumeChangedPostfix()
        {
            if (SceneManager.GetActiveScene().name == "Main")
            {
                CustomTunes.Main.CalculateVolume();
            }
        }
        #endregion
    }
}
