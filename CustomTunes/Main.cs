using QModManager.API.ModLoading;

namespace Straitjacket.Subnautica.Mods.CustomTunes
{
    [QModCore]
    public class Main
    {
        [QModPatch]
        public static void Patch() => CustomTunes.Initialise();
    }
}
