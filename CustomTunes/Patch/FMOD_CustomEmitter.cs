using HarmonyLib;
using UnityEngine.SceneManagement;
using UWE;

namespace Straitjacket.Subnautica.Mods.CustomTunes.Patch
{
    [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Play))]
    internal static class FMOD_CustomEmitter_Play
    {
        static bool Prefix(FMOD_CustomEmitter __instance)
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
    }

    [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.Stop))]
    internal static class FMOD_CustomEmitter_Stop
    {
        static bool Prefix(FMOD_CustomEmitter __instance)
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
    }

    [HarmonyPatch(typeof(FMOD_CustomEmitter), nameof(FMOD_CustomEmitter.ReleaseEvent))]
    internal static class FMOD_CustomEmitter_ReleaseEvent
    {
        static bool Prefix(FMOD_CustomEmitter __instance)
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
    }
}
