using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;
using UnityEngine;
using UnityEngine.UI;
using UWE;

namespace Straitjacket.Subnautica.Mods.CustomTunes
{
    [Menu("CustomTunes ♫", LoadOn = MenuAttribute.LoadEvents.MenuOpened | MenuAttribute.LoadEvents.MenuRegistered)]
    internal class Config : ConfigFile
    {
        [Toggle("Include OST"), OnChange(nameof(OnIncludeOSTChanged))]
        public bool IncludeOST = true;
        private void OnIncludeOSTChanged(ToggleChangedEventArgs e)
        {
            if (!e.Value)
                CoroutineHost.StartCoroutine(CustomTunes.UnloadOST());
        }

        [Toggle("Reload after adding music files")]
        public bool ReloadOnFileChange = true;

        [Keybind("Play/Pause")]
        public KeyCode PlayPauseKey = KeyCode.None;

        [Keybind("Next track")]
        public KeyCode NextTrackKey = KeyCode.None;

        [Keybind("Previous track")]
        public KeyCode PreviousTrackKey = KeyCode.None;

        [Keybind("Stop")]
        public KeyCode StopKey = KeyCode.None;

        [Slider("Minimum delay between tracks", 0, 120, DefaultValue = 1, Format = "{0}s")]
        [OnChange(nameof(OnMinimumDelayChanged))]
        [OnGameObjectCreated(nameof(OnMinimumDelayGameObjectCreated))]
        public int MinimumDelay = 1;
        private void OnMinimumDelayChanged(SliderChangedEventArgs e)
        {
            if (maximumDelaySlider != null)
            {
                maximumDelaySlider.minValue = e.IntegerValue;
                UpdateDefaultValue(maximumDelaySlider);
            }
        }
        private Slider minimumDelaySlider;
        private void OnMinimumDelayGameObjectCreated(GameObjectCreatedEventArgs e)
            => minimumDelaySlider = e.GameObject.GetComponentInChildren<Slider>();

        [Slider("Maximum delay between tracks", 0, 120, DefaultValue = 6, Format = "{0}s")]
        [OnChange(nameof(OnMaximumDelayChanged))]
        [OnGameObjectCreated(nameof(OnMaximumDelayGameObjectCreated))]
        public int MaximumDelay = 6;
        private void OnMaximumDelayChanged(SliderChangedEventArgs e)
        {
            if (minimumDelaySlider != null)
            {
                minimumDelaySlider.maxValue = e.IntegerValue;
                UpdateDefaultValue(minimumDelaySlider);
            }
        }
        private Slider maximumDelaySlider;
        private void OnMaximumDelayGameObjectCreated(GameObjectCreatedEventArgs e)
            => maximumDelaySlider = e.GameObject.GetComponentInChildren<Slider>();

        private static void UpdateDefaultValue(Slider slider)
        {
            if (slider != null &&
                slider.GetComponentInChildren<uGUI_SnappingSlider>() is uGUI_SnappingSlider snappingSlider &&
                snappingSlider.defaultValue >= slider.minValue && snappingSlider.defaultValue <= slider.maxValue)
            {
                snappingSlider.UpdateDefaultValueRect();
            }
        }
    }
}
