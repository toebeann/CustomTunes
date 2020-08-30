using System.Reflection;
using HarmonyLib;
using QModManager.API.ModLoading;

namespace Straitjacket.Subnautica.Mods.CustomTunes
{
    [QModCore]
    public class HarmonyPatcher
    {
        [QModPatch]
        public static void ApplyPatches()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.tobeyblaber.straitjacket.subnautica.customtunes.mod");
            CustomTunes.Initialise();
        }
    }
}
