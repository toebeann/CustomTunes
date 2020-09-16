using SMLHelper.V2.Options;
using UWE;

namespace Straitjacket.Subnautica.Mods.CustomTunes
{
    internal class Options : ModOptions
    {
        private const string INCLUDE_OST_ID = "includeOST";
        private const string RELOAD_ON_FILE_CHANGE_ID = "reloadOnFileChange";
        private const string PLAY_PAUSE_ID = "playPause";
        private const string NEXT_TRACK_ID = "nextTrack";
        private const string PREV_TRACK_ID = "prevTrack";
        private const string STOP_TRACK_ID = "stopTrack";
        private const string MIN_DELAY_ID = "minDelay";
        private const string MAX_DELAY_ID = "maxDelay";

        public override void BuildModOptions()
        {
            AddToggleOption(INCLUDE_OST_ID, "Include OST", CustomTunes.Config.IncludeOST);
            AddToggleOption(RELOAD_ON_FILE_CHANGE_ID, "Reload after adding music files", CustomTunes.Config.ReloadOnFileChange);
            AddKeybindOption(PLAY_PAUSE_ID, "Play/Pause", GameInput.GetPrimaryDevice(), CustomTunes.Config.PlayPauseKey);
            AddKeybindOption(NEXT_TRACK_ID, "Next track", GameInput.GetPrimaryDevice(), CustomTunes.Config.NextTrackKey);
            AddKeybindOption(PREV_TRACK_ID, "Previous track", GameInput.GetPrimaryDevice(), CustomTunes.Config.PreviousTrackKey);
            AddKeybindOption(STOP_TRACK_ID, "Stop", GameInput.GetPrimaryDevice(), CustomTunes.Config.StopKey);
            AddSliderOption(MIN_DELAY_ID, "Minimum delay between tracks (seconds)", 0, 120, CustomTunes.Config.MinimumDelay);
            AddSliderOption(MAX_DELAY_ID, "Maximum delay between tracks (seconds)", 0, 120, CustomTunes.Config.MaximumDelay);
        }

        public Options() : base("CustomTunes ♫")
        {
            ToggleChanged += Options_ToggleChanged;
            KeybindChanged += Options_KeybindChanged;
            SliderChanged += Options_SliderChanged;
        }

        private void Options_ToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            switch (e.Id)
            {
                case INCLUDE_OST_ID:
                    CustomTunes.Config.IncludeOST = e.Value;
                    if (!CustomTunes.Config.IncludeOST)
                    {
                        CoroutineHost.StartCoroutine(CustomTunes.UnloadOST());
                    }
                    break;
                case RELOAD_ON_FILE_CHANGE_ID:
                    CustomTunes.Config.ReloadOnFileChange = e.Value;
                    break;
            }
            CustomTunes.Config.Save();
        }

        private void Options_KeybindChanged(object sender, KeybindChangedEventArgs e)
        {
            switch (e.Id)
            {
                case PLAY_PAUSE_ID:
                    CustomTunes.Config.PlayPauseKey = e.Key;
                    break;
                case NEXT_TRACK_ID:
                    CustomTunes.Config.NextTrackKey = e.Key;
                    break;
                case PREV_TRACK_ID:
                    CustomTunes.Config.PreviousTrackKey = e.Key;
                    break;
                case STOP_TRACK_ID:
                    CustomTunes.Config.StopKey = e.Key;
                    break;
            }
            CustomTunes.Config.Save();
        }

        private void Options_SliderChanged(object sender, SliderChangedEventArgs e)
        {
            switch (e.Id)
            {
                case MIN_DELAY_ID:
                    CustomTunes.Config.MinimumDelay = System.Math.Min((int)e.Value, CustomTunes.Config.MaximumDelay);
                    break;
                case MAX_DELAY_ID:
                    CustomTunes.Config.MaximumDelay = System.Math.Max((int)e.Value, CustomTunes.Config.MinimumDelay);
                    break;
            }
            CustomTunes.Config.Save();
        }
    }
}
