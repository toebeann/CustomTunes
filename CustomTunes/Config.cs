using SMLHelper.V2.Json;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.CustomTunes
{
    internal class Config : ConfigFile
    {
        public bool IncludeOST { get; set; } = true;
        public bool ReloadOnFileChange { get; set; } = true;
        public KeyCode PlayPauseKey { get; set; } = KeyCode.None;
        public KeyCode NextTrackKey { get; set; } = KeyCode.None;
        public KeyCode PreviousTrackKey { get; set; } = KeyCode.None;
        public KeyCode StopKey { get; set; } = KeyCode.None;
        public int MinimumDelay { get; set; } = 1;
        public int MaximumDelay { get; set; } = 6;
    }
}
