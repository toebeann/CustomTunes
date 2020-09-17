using HarmonyLib;
using UnityEngine.SceneManagement;
using UWE;

namespace Straitjacket.Subnautica.Mods.CustomTunes.Patches
{
    internal static class FMOD_CustomEmitterPatch
    {
        #region FMOD_CustomEmitter.Play
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Play))]
        [HarmonyPrefix]
        static bool PlayPrefix(FMOD_CustomEmitter __instance)
        {
            if (__instance?.asset?.path != null && __instance.asset.path.Contains(@"event:/env/music/") && SceneManager.GetActiveScene().name == "Main")
            {
                CustomTunes.Main.Play(__instance.asset.path);
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region FMOD_CustomEmitter.Stop
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Stop))]
        [HarmonyPrefix]
        static bool StopPrefix(FMOD_CustomEmitter __instance)
        {
            if (__instance?.asset?.path != null && __instance.asset.path.Contains(@"event:/env/music/"))
            {
                CoroutineHost.StartCoroutine(CustomTunes.Main.Stop(__instance.asset.path));
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region FMOD_CustomEmitter.ReleaseEvent
        [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.ReleaseEvent))]
        [HarmonyPrefix]
        static bool ReleaseEventPrefix(FMOD_CustomEmitter __instance)
        {
            if (__instance?.asset?.path != null && __instance.asset.path.Contains(@"event:/env/music/"))
            {
                __instance.Stop();
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}
