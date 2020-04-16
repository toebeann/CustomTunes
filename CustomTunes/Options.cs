using SMLHelper.V2.Options;
using UWE;

namespace Straitjacket.Subnautica.Mods.CustomTunes
{
    internal class Options : ModOptions
    {
        public override void BuildModOptions()
        {
            AddToggleOption("includeOST", "Include OST", CustomTunes.Config.IncludeOST);
            AddToggleOption("reloadOnFileChange", "Reload on add music files", CustomTunes.Config.ReloadOnFileChange);
            AddKeybindOption("playPause", "Play/Pause", GameInput.GetPrimaryDevice(), CustomTunes.Config.PlayPauseKey);
            AddKeybindOption("nextTrack", "Next track", GameInput.GetPrimaryDevice(), CustomTunes.Config.NextTrackKey);
            AddKeybindOption("prevTrack", "Previous track", GameInput.GetPrimaryDevice(), CustomTunes.Config.PreviousTrackKey);
            AddKeybindOption("stop", "Stop", GameInput.GetPrimaryDevice(), CustomTunes.Config.StopKey);
        }

        public Options() : base("CustomTunes")
        {
            ToggleChanged += Options_ToggleChanged;
            KeybindChanged += Options_KeybindChanged;
        }

        private void Options_ToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            switch (e.Id)
            {
                case "includeOST":
                    CustomTunes.Config.IncludeOST = e.Value;
                    if (!CustomTunes.Config.IncludeOST)
                    {
                        CoroutineHost.StartCoroutine(CustomTunes.UnloadOST());
                    }
                    break;
                case "reloadOnFileChange":
                    CustomTunes.Config.ReloadOnFileChange = e.Value;
                    break;
            }
            CustomTunes.Config.Save();
        }

        private void Options_KeybindChanged(object sender, KeybindChangedEventArgs e)
        {
            switch (e.Id)
            {
                case "playPause":
                    CustomTunes.Config.PlayPauseKey = e.Key;
                    break;
                case "nextTrack":
                    CustomTunes.Config.NextTrackKey = e.Key;
                    break;
                case "prevTrack":
                    CustomTunes.Config.PreviousTrackKey = e.Key;
                    break;
                case "stop":
                    CustomTunes.Config.StopKey = e.Key;
                    break;
            }
            CustomTunes.Config.Save();
        }
    }
}
