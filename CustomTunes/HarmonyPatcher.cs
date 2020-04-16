using System.Reflection;
using Harmony;
using QModManager.API.ModLoading;

namespace Straitjacket.Subnautica.Mods.CustomTunes
{
    [QModCore]
    public class HarmonyPatcher
    {
        [QModPatch]
        public static void ApplyPatches()
        {
            var harmony = HarmonyInstance.Create("com.tobeyblaber.straitjacket.subnautica.customtunes.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            CustomTunes.Initialise();
        }
    }
}
