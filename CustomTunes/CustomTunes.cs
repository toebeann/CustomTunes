using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using MimeTypes;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using NAudio.Wave;
using UnityEngine;
using UnityEngine.SceneManagement;
using UWE;
using AudioClipPath = System.Collections.Generic.KeyValuePair<string, UnityEngine.AudioClip>;

namespace Straitjacket.Subnautica.Mods.CustomTunes
{
    internal class CustomTunes : MonoBehaviour
    {
        private static CustomTunes instance = null;
        public static CustomTunes Main => instance = instance ?? new GameObject("CustomTunes").AddComponent<CustomTunes>();

        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        private static extern int FindMimeFromData(IntPtr pBC,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I1, SizeParamIndex=3)]
            byte[] pBuffer,
            int cbSize,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
            int dwMimeFlags,
            out IntPtr ppwzMimeOut,
            int dwReserved);

        private static bool urlmonFailed = false;
        private static string GetMimeType(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename + " not found");

            if (!urlmonFailed)
            {
                int MaxContent = (int)new FileInfo(filename).Length;
                if (MaxContent > 4096) MaxContent = 4096;
                FileStream fs = File.OpenRead(filename);


                byte[] buf = new byte[MaxContent];
                fs.Read(buf, 0, MaxContent);
                fs.Close();

                try
                {
                    int result = FindMimeFromData(IntPtr.Zero, filename, buf, MaxContent, null, 0, out var mimeout, 0);

                    if (result != 0)
                        throw Marshal.GetExceptionForHR(result);
                    string mime = Marshal.PtrToStringUni(mimeout);
                    Marshal.FreeCoTaskMem(mimeout);
                    return mime;
                }
                catch (EntryPointNotFoundException e)
                {
                    urlmonFailed = true;
                    Console.WriteLine("[CustomTunes] Urlmon.dll could not be loaded:");
                    Console.WriteLine($"[CustomTunes] {e.Message}");
                    Console.WriteLine("[CustomTunes] Resorting to mime type mapping via file extension rather than file signature.");

                    return MimeTypeMap.GetMimeType(Path.GetExtension(filename));
                }
            }
            else
            {
                return MimeTypeMap.GetMimeType(Path.GetExtension(filename));
            }
        }

        private static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.Default.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        private static string GetWaveFilenameFromMp3(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(filename + " not found");
            }

            string md5 = CreateMD5(File.ReadAllText(filename));
            return Path.Combine(TempPath, $"{md5}.wav"); ;
        }

        private static string GetWaveFromMp3(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(filename + " not found");
            }

            var waveFilename = GetWaveFilenameFromMp3(filename);

            if (!File.Exists(waveFilename))
            {
                var fileInfo = new FileInfo(waveFilename);
                fileInfo.Directory.Create(); // If the directory already exists, this method does nothing.

                try
                {
                    using (var reader = new Mp3FileReader(filename))
                    {
                        WaveFileWriter.CreateWaveFile(waveFilename, reader);
                    }
                }
                catch (Exception e)
                {
                    var error = $"Failed to load {Path.GetFileName(filename)}: {e.Message}";

                    Console.WriteLine($"[CustomTunes] {error}");
                    return null;
                }
            }

            return waveFilename;
        }

        public static Config Config = new Config();
        public static void Initialise()
        {
            Config.Load();
            OptionsPanelHandler.RegisterModOptions(new Options());
        }

        private static string tempPath = null;
        private static string TempPath => tempPath = tempPath ?? Path.Combine(Path.GetTempPath(), @"Straitjacket\Subnautica\Mods\CustomTunes\");

        private static string ostPath = null;
        private static string OSTPath => ostPath = ostPath ?? Path.Combine(Directory.GetCurrentDirectory(), @"OST\");
        private static void LoadOST()
        {
            if (Directory.Exists(OSTPath))
            {
                foreach (var filename in Directory.GetFiles(OSTPath, "*", SearchOption.AllDirectories))
                {
                    if (OST.ContainsKey(filename) || failedPaths.Contains(filename) || !ValidAudioFile(filename))
                    {
                        continue;
                    }

                    OST.Add(filename, null);
                }
            }
        }

        private static string musicPath = null;
        private static string MusicPath => musicPath = musicPath ?? Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(CustomTunes)).Location), @"Music\");
        private static void LoadCustomMusic()
        {
            var fileInfo = new FileInfo(MusicPath);
            fileInfo.Directory.Create(); // If the directory already exists, this method does nothing.

            foreach (var filename in Directory.GetFiles(MusicPath, "*", SearchOption.AllDirectories))
            {
                if (CustomMusic.ContainsKey(filename) || failedPaths.Contains(filename) || !ValidAudioFile(filename))
                {
                    continue;
                }

                CustomMusic.Add(filename, null);
            }
        }

        private static readonly List<Wildcard> acceptedMimeTypes = new List<Wildcard>() {
            new Wildcard(@"audio/*wav*", RegexOptions.IgnoreCase),
            new Wildcard(@"audio/ogg", RegexOptions.IgnoreCase),
            new Wildcard(@"audio/mpeg", RegexOptions.IgnoreCase)
        };
        private static bool ValidAudioFile(string filename)
        {
            var preliminaryMimeType = MimeTypeMap.GetMimeType(Path.GetExtension(filename));
            if (!acceptedMimeTypes.Any(acceptedMimeType => acceptedMimeType.IsMatch(preliminaryMimeType)))
            {
                return false;
            }

            if (!urlmonFailed)
            {
                var mimeType = GetMimeType(filename);
                if (!acceptedMimeTypes.Any(acceptedMimeType => acceptedMimeType.IsMatch(mimeType)))
                {
                    return false;
                }
            }

            return true;
        }

        private static Dictionary<string, string> Mp3WavePaths = new Dictionary<string, string>();
        public static Dictionary<string, AudioClip> OST = new Dictionary<string, AudioClip>();
        public static Dictionary<string, AudioClip> CustomMusic = new Dictionary<string, AudioClip>();

        private static void RegisterAudioClip(string filename, AudioClip audioClip)
        {
            if (OST.ContainsKey(filename))
            {
                OST[filename] = audioClip;
            }
            else if (CustomMusic.ContainsKey(filename))
            {
                CustomMusic[filename] = audioClip;
            }
        }

        private float timeOfLastMusic;
        private AudioSource musicSource;
        private float currentSilenceLength => UnityEngine.Random.Range(Config.MinimumDelay, Config.MaximumDelay);
        private bool stopped = false;
        private bool paused = false;
        private bool generatingAudioClip = false;
        private static List<AudioClipPath> playlist = new List<AudioClipPath>();

        private void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(this);
            }
            else
            {
                CoroutineHost.StartCoroutine(LoadMusic());
            }
        }

        public void CalculateVolume()
        {
            volume = SoundSystem.masterVolume * SoundSystem.musicVolume;
        }

        private float volume = 1;
        private IEnumerator Start()
        {
            CalculateVolume();
            yield return new WaitUntil(() => Player.main != null);
            musicSource = gameObject.AddComponent<AudioSource>();
            timeOfLastMusic = Time.time;
        }

        private void Update()
        {
            if (musicSource != null)
            {
                musicSource.volume = volume;
            }

            if (VirtualKey.GetKeyDown(VK.VK_MEDIA_STOP) || (Config.StopKey != KeyCode.None && KeyCodeUtils.GetKeyDown(Config.StopKey)))
            {
                stopped = true;
                paused = false;
                Stop();
            }
            if (VirtualKey.GetKeyDown(VK.VK_MEDIA_PLAY_PAUSE) || (Config.PlayPauseKey != KeyCode.None && KeyCodeUtils.GetKeyDown(Config.PlayPauseKey)))
            {
                if (!stopped)
                {
                    Pause();
                }
                else
                {
                    stopped = false;
                    paused = false;
                    timeOfLastMusic = Time.time;
                    var filename = OST.Concat(CustomMusic).First(x => x.Value == musicSource.clip).Key;
                    musicSource.Play();
                }
            }
            if (VirtualKey.GetKeyDown(VK.VK_MEDIA_NEXT_TRACK) || (Config.NextTrackKey != KeyCode.None && KeyCodeUtils.GetKeyDown(Config.NextTrackKey)))
            {
                NextTrack();
            }
            if (VirtualKey.GetKeyDown(VK.VK_MEDIA_PREV_TRACK) || (Config.PreviousTrackKey != KeyCode.None && KeyCodeUtils.GetKeyDown(Config.PreviousTrackKey)))
            {
                if (CurrentTrackIndex > 0 && Time.time - timeOfLastMusic <= 1)
                {
                    PreviousTrack();
                }
                else
                {
                    Stop();
                    timeOfLastMusic = Time.time;
                    var filename = OST.Concat(CustomMusic).First(x => x.Value == musicSource.clip).Key;
                    musicSource.Play();
                }
            }
        }

        public void Pause()
        {
            if (!stopped)
            {
                if (musicSource.isPlaying)
                {
                    paused = true;
                    musicSource.Pause();
                }
                else
                {
                    CoroutineHost.StartCoroutine(Unpause());
                }
            }
        }

        public IEnumerator Unpause()
        {
            if (paused)
            {
                yield return new WaitForFixedUpdate();
                yield return new WaitUntil(() => !FreezeTime.freezers.Any());
                paused = false;
                if (!musicSource.isPlaying)
                {
                    musicSource.Play();
                }
            }
        }

        public void NextTrack()
        {
            Stop();
            musicSource.clip = null;
            CoroutineHost.StartCoroutine(Play());
        }

        public void PreviousTrack()
        {
            Stop();
            musicSource.clip = null;
            CurrentTrackIndex -= 2;
            CoroutineHost.StartCoroutine(Play());
        }

        private void FixedUpdate()
        {
            if (Config.ReloadOnFileChange)
            {
                CoroutineHost.StartCoroutine(LoadMusic(true));
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                //Destroy(musicSource);
                //Mp3WavePaths.Clear();
                // TODO: Tidy up loaded music
            }
        }

        private static Coroutine OSTPreload = null;
        private static Coroutine CustomMusicPreload = null;
        internal static IEnumerator LoadMusic(bool force = false)
        {
            if (Config.IncludeOST)
            {
                LoadOST();
            }
            LoadCustomMusic();

            var failed = failedPaths.Count();

            yield return new WaitWhile(() => WaitScreen.main == null);

            if (OSTPreload == null && OST.Any(x => x.Value == null))
            {
                OSTPreload = CoroutineHost.StartCoroutine(PreloadOST());
            }
            if (CustomMusicPreload == null && CustomMusic.Any(x => x.Value == null))
            {
                CustomMusicPreload = CoroutineHost.StartCoroutine(PreloadCustomMusic());
            }

            if ((OSTPreload != null || CustomMusicPreload != null) && force)
            {
                FreezeTime.Begin("CustomTunesPreload");
                WaitScreen.ShowImmediately();
            }

            yield return OSTPreload;
            yield return CustomMusicPreload;
            GeneratePlaylist();

            yield return new WaitWhile(() => WaitScreen.main.isShown);
            FreezeTime.End("CustomTunesPreload");

            if (failedPaths.Count() > failed)
            {
                ErrorMessage.AddError("Some tracks failed to load, see error log for details.");
            }
        }

        private static IEnumerator PreloadOST()
        {
            WaitScreen.ManualWaitItem waitItemOST = null;
            yield return new WaitUntil(() => WaitScreen.main != null);

            waitItemOST = WaitScreen.Add("Preloading OST ♫");

            foreach (var audioClipPath in OST.ToList())
            {
                if (audioClipPath.Value == null)
                {
                    var loadTrackTask = LoadTrack(audioClipPath.Key);
                    yield return loadTrackTask;
                    yield return new WaitWhile(() => loadTrackTask.GetResult() == null);
                    var audioClip = loadTrackTask.GetResult();

                    if (audioClip.AudioClip == null)
                    {
                        OST.Remove(audioClipPath.Key);
                        failedPaths.Add(audioClipPath.Key);
                    }
                    else
                    {
                        yield return new WaitWhile(() => audioClip.AudioClip.loadState == AudioDataLoadState.Loading);
                        RegisterAudioClip(audioClipPath.Key, audioClip.AudioClip);
                    }

                    waitItemOST.SetProgress(OST.Count(x => x.Value != null), OST.Count());
                }
            }

            WaitScreen.Remove(waitItemOST);
            OSTPreload = null;
        }

        private static IEnumerator PreloadCustomMusic()
        {
            WaitScreen.ManualWaitItem waitItemCustomMusic = null;
            yield return new WaitUntil(() => WaitScreen.main != null);

            waitItemCustomMusic = WaitScreen.Add("Preloading custom music ♫");

            foreach (var audioClipPath in CustomMusic.ToList())
            {
                if (audioClipPath.Value == null)
                {
                    var loadTrackTask = LoadTrack(audioClipPath.Key);
                    yield return loadTrackTask;
                    yield return new WaitWhile(() => loadTrackTask.GetResult() == null);
                    var audioClip = loadTrackTask.GetResult();

                    if (audioClip.AudioClip == null)
                    {
                        CustomMusic.Remove(audioClipPath.Key);
                        failedPaths.Add(audioClipPath.Key);
                    }
                    else
                    {
                        yield return new WaitWhile(() => audioClip.AudioClip.loadState == AudioDataLoadState.Loading);
                        RegisterAudioClip(audioClipPath.Key, audioClip.AudioClip);
                    }

                    waitItemCustomMusic.SetProgress(CustomMusic.Count(x => x.Value != null), CustomMusic.Count());
                }
            }

            WaitScreen.Remove(waitItemCustomMusic);
            CustomMusicPreload = null;
        }

        private static readonly HashSet<string> failedPaths = new HashSet<string>();

        private readonly HashSet<string> biomes = new HashSet<string>();
        private readonly HashSet<string> eventPaths = new HashSet<string>();
        public void Play(string eventPath)
        {
            if (!eventPaths.Contains(eventPath))
            {
                eventPaths.Add(eventPath);
            }

            if (!stopped && !paused && !musicSource.isPlaying && !generatingAudioClip)
            {
                if (eventPath.Contains("background_music"))
                {
                    var biome = eventPath.Replace("event:/env/music/", "").Replace("_background_music", "");
                    if (!biomes.Contains(biome))
                    {
                        biomes.Add(biome);
                    }
                    CoroutineHost.StartCoroutine(Play());
                }
            }
        }

        private static int CurrentTrackIndex = -1;
        private static void GeneratePlaylist()
        {
            if (!playlist.Any())
            {
                if (Config.IncludeOST)
                {
                    playlist = OST.ToList();
                }
                playlist = playlist.Concat(CustomMusic.ToList()).OrderBy(x => Guid.NewGuid()).ToList();
            }
        }
        public static IEnumerator UnloadOST()
        {
            if (SceneManager.GetActiveScene().name == "Main")
            {
                yield return new WaitUntil(() => !FreezeTime.freezers.Any());
                Main.Stop();
                Main.musicSource.clip = null;
                playlist.Clear();
            }
        }

        private static CoroutineTask<AudioClipResult> LoadTrack(string filename)
        {
            var result = new TaskResult<AudioClipResult>();
            return new CoroutineTask<AudioClipResult>(LoadTrack(filename, result), result);
        }

        private class AudioClipResult
        {
            public AudioClip AudioClip = null;
            public bool Error = false;
        }
        private static IEnumerator LoadTrack(string filename, IOut<AudioClipResult> result)
        {
            if (GetMimeType(filename).ToLowerInvariant().Contains("mpeg"))
            {
                if (!Mp3WavePaths.TryGetValue(filename, out var waveFilename))
                {
                    waveFilename = GetWaveFromMp3(filename);
                    Mp3WavePaths.Add(filename, waveFilename);
                    filename = waveFilename;
                }
            }

            var www = new WWW($"file://{filename}");
            var audioClip = www.GetAudioClip();
            result.Set(new AudioClipResult { AudioClip = audioClip });
            if (audioClip == null)
            {
                yield break;
            }
            yield return new WaitWhile(() => audioClip.loadState == AudioDataLoadState.Loading);
        }

        private static void IteratePlaylist()
        {
            CurrentTrackIndex++;
            if (CurrentTrackIndex >= playlist.Count())
            {
                CurrentTrackIndex = 0;
                playlist.Clear();
            }
        }
        private IEnumerator Play()
        {
            if (!musicSource.isPlaying)
            {
                IteratePlaylist();
                GeneratePlaylist();

                var audioClipPath = playlist.ElementAt(CurrentTrackIndex);
                var audioClip = audioClipPath.Value;
                if (audioClip == null || audioClip.loadState == AudioDataLoadState.Loading)
                {
                    generatingAudioClip = true;

                    if (audioClip == null)
                    {
                        CoroutineTask<AudioClipResult> loadTrackTask = LoadTrack(audioClipPath.Key);
                        yield return loadTrackTask;
                        yield return new WaitWhile(() => loadTrackTask.GetResult() == null);
                        audioClip = loadTrackTask.GetResult().AudioClip;
                    }

                    if (audioClip == null)
                    {
                        ErrorMessage.AddError($"Failed to load {Path.GetFileName(audioClipPath.Key)}, see error log for details.");
                        yield break;
                    }

                    yield return new WaitWhile(() => audioClip.loadState == AudioDataLoadState.Loading);
                    RegisterAudioClip(audioClipPath.Key, audioClip);

                    generatingAudioClip = false;
                }

                if (audioClip.loadState != AudioDataLoadState.Loaded)
                {
                    ErrorMessage.AddError($"Failed to load {Path.GetFileName(audioClipPath.Key)}.");
                    yield break;
                }

                if (musicSource.clip != audioClipPath.Value)
                {
                    var playDelayed = musicSource.clip != null;
                    musicSource.clip = audioClip;
                    if (playDelayed)
                    {
                        var delay = currentSilenceLength;
                        musicSource.PlayDelayed(delay);
                        yield return new WaitForSecondsRealtime(delay);
                    }
                    else
                    {
                        musicSource.Play();
                    }
                    if (musicSource.isPlaying)
                    {
                        timeOfLastMusic = Time.time;
                    }
                }
            }
        }

        public void Stop()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public IEnumerator Stop(string eventPath)
        {
            if (musicSource != null && musicSource.isPlaying && eventPaths.Contains(eventPath))
            {
                eventPaths.Remove(eventPath);
                yield return new WaitForFixedUpdate();
                if (!eventPaths.Any())
                {
                    Stop();
                }
            }
        }
    }
}
