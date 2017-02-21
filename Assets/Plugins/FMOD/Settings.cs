using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FMODUnity
{


    [Serializable]
    public enum FMODPlatform
    {
        None,
        PlayInEditor,
        Default,
        Desktop,
        Mobile,
        MobileHigh,
        MobileLow,
        Console,
        Windows,
        Mac,
        Linux,
        iOS,
        Android,
        WindowsPhone,
        XboxOne,
        PS4,
        WiiU,
        PSVita,
		AppleTV,
        UWP,
        Count,
    }
    
    public class PlatformSettingBase
    {
        public FMODPlatform Platform;
    }
    
    public class PlatformSetting<T> : PlatformSettingBase
    {
        public T Value;
    }
    
    [Serializable]
    public class PlatformIntSetting : PlatformSetting<int>
    {
    }

    [Serializable]
    public class PlatformStringSetting : PlatformSetting<string>
    {
    }

    [Serializable]
    public class PlatformBoolSetting : PlatformSetting<bool>
    {
    }
    
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public class Settings : ScriptableObject
    {
        const string SettingsAssetName = "FMODStudioSettings";

        private static Settings instance = null;

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load(SettingsAssetName) as Settings;
                    if (instance == null)
                    {
                        UnityEngine.Debug.Log("FMOD Studio: cannot find integration settings, creating default settings");
                        instance = CreateInstance<Settings>();
                        instance.name = "FMOD Studio Integration Settings";

                        #if UNITY_EDITOR
                        if (!System.IO.Directory.Exists("Assets/Resources"))
                        {
                            AssetDatabase.CreateFolder("Assets", "Resources");
                        }
                        AssetDatabase.CreateAsset(instance, "Assets/Resources/" + SettingsAssetName + ".asset");
                        #endif
                    }
                }
                return instance;
            }
        }

            
	    #if UNITY_EDITOR
        [MenuItem("FMOD/Edit Settings", priority = 0)]
        public static void EditSettings()
	    {
	        Selection.activeObject = Instance;
            EditorApplication.ExecuteMenuItem("Window/Inspector");
        }
        #endif


        [SerializeField]
        public bool HasSourceProject = true;

        [SerializeField]
        public bool HasPlatforms = true;
        
        [SerializeField]
        public string SourceProjectPath;

        [SerializeField]
        public string SourceBankPath;
        
        [SerializeField]
        public bool AutomaticEventLoading;

        [SerializeField]
        public bool AutomaticSampleLoading;


        [SerializeField]
        public List<PlatformIntSetting> SpeakerModeSettings;

        [SerializeField]
        public List<PlatformIntSetting> SampleRateSettings;

        [SerializeField]
        public List<PlatformBoolSetting> LiveUpdateSettings;

        [SerializeField]
        public List<PlatformBoolSetting> OverlaySettings;

        [SerializeField]
        public List<PlatformBoolSetting> LoggingSettings;

        [SerializeField]
        public List<PlatformStringSetting> BankDirectorySettings;

        [SerializeField]
        public List<PlatformIntSetting> VirtualChannelSettings;

        [SerializeField]
        public List<PlatformIntSetting> RealChannelSettings;

        [SerializeField]
        public List<string> Plugins = new List<string>();

        [SerializeField]
        public string MasterBank;

        [SerializeField]
        public List<string> Banks;

        public static FMODPlatform GetParent(FMODPlatform platform)
        {
            switch (platform)
            {
                case FMODPlatform.Windows:
                case FMODPlatform.Linux:
                case FMODPlatform.Mac:
                case FMODPlatform.UWP:
                    return FMODPlatform.Desktop;
                case FMODPlatform.MobileHigh:
                case FMODPlatform.MobileLow:
                case FMODPlatform.iOS:
                case FMODPlatform.Android:
                case FMODPlatform.WindowsPhone:
				case FMODPlatform.PSVita:
			    case FMODPlatform.AppleTV:
                    return FMODPlatform.Mobile;
                case FMODPlatform.XboxOne:
                case FMODPlatform.PS4:
                case FMODPlatform.WiiU:
                    return FMODPlatform.Console;
                case FMODPlatform.Desktop:
                case FMODPlatform.Console:
                case FMODPlatform.Mobile:
                    return FMODPlatform.Default;
                case FMODPlatform.PlayInEditor:
                    return FMODPlatform.Default;
                case FMODPlatform.Default:
                default:
                    return FMODPlatform.None;
            }
        }

        public static bool HasSetting<T>(List<T> list, FMODPlatform platform) where T : PlatformSettingBase
        {
            return list.Exists((x) => x.Platform == platform);
        }

        public static U GetSetting<T, U>(List<T> list, FMODPlatform platform, U def) where T : PlatformSetting<U>
        {
            T t = list.Find((x) => x.Platform == platform);
            if (t == null)
            {
                FMODPlatform parent = GetParent(platform);
                if (parent != FMODPlatform.None)
                {
                    return GetSetting(list, parent, def);
                }
                else
                {
                    return def;
                }
            }
            return t.Value;
        }

        public static void SetSetting<T, U>(List<T> list, FMODPlatform platform, U value) where T : PlatformSetting<U>, new()
        {
            T setting = list.Find((x) => x.Platform == platform);
            if (setting == null)
            {
                setting = new T();
                setting.Platform = platform;
                list.Add(setting);
            }
            setting.Value = value;
        }

        public static void RemoveSetting<T>(List<T> list, FMODPlatform platform) where T : PlatformSettingBase
        {
            list.RemoveAll((x) => x.Platform == platform);
        }

        // --------   Live Update ----------------------
        public bool IsLiveUpdateEnabled(FMODPlatform platform)
        {
            return GetSetting(LiveUpdateSettings, platform, false);
        }

        // --------   Overlay Update ----------------------
        public bool IsOverlayEnabled(FMODPlatform platform)
        {
            return GetSetting(OverlaySettings, platform, false);
        }

        // --------   Logging ----------------------
        public bool IsLoggingEnabled(FMODPlatform platform)
        {
            return GetSetting(LoggingSettings, platform, false);
        }

        // --------   Real channels ----------------------
        public int GetRealChannels(FMODPlatform platform)
        {
            return GetSetting(RealChannelSettings, platform, 64);
        }

        // --------   Virtual channels ----------------------
        public int GetVirtualChannels(FMODPlatform platform)
        {
            return GetSetting(VirtualChannelSettings, platform, 128);
        }

        // --------   Speaker Mode ----------------------
        public int GetSpeakerMode(FMODPlatform platform)
        {
            return GetSetting(SpeakerModeSettings, platform, (int)FMOD.SPEAKERMODE.STEREO);
        }
        // --------   Sample Rate ----------------------
        public int GetSampleRate(FMODPlatform platform)
        {
            return GetSetting(SampleRateSettings, platform, 48000);
        }

        // --------   Bank Platform ----------------------
        public string GetBankPlatform(FMODPlatform platform)
        {
            return HasPlatforms ? GetSetting(BankDirectorySettings, platform, "Desktop") : "";
        }
 
        private Settings()
        {
            Banks = new List<string>();
            RealChannelSettings = new List<PlatformIntSetting>();
            VirtualChannelSettings = new List<PlatformIntSetting>();
            LoggingSettings = new List<PlatformBoolSetting>();
            LiveUpdateSettings = new List<PlatformBoolSetting>();
            OverlaySettings = new List<PlatformBoolSetting>();
            SampleRateSettings = new List<PlatformIntSetting>();
            SpeakerModeSettings = new List<PlatformIntSetting>();
            BankDirectorySettings = new List<PlatformStringSetting>();
            
            // Default play in editor settings
            SetSetting(LoggingSettings, FMODPlatform.PlayInEditor, true);
            SetSetting(LiveUpdateSettings, FMODPlatform.PlayInEditor, true);
            SetSetting(OverlaySettings, FMODPlatform.PlayInEditor, true);
            SetSetting(SpeakerModeSettings, FMODPlatform.PlayInEditor, (int)FMOD.SPEAKERMODE.STEREO);
            // These are not editable, set them high
            SetSetting(RealChannelSettings, FMODPlatform.PlayInEditor, 256);
            SetSetting(VirtualChannelSettings, FMODPlatform.PlayInEditor, 1024);

            // Default runtime settings
            SetSetting(LoggingSettings, FMODPlatform.Default, false);
            SetSetting(LiveUpdateSettings, FMODPlatform.Default, false);
            SetSetting(OverlaySettings, FMODPlatform.Default, false);

            SetSetting(RealChannelSettings, FMODPlatform.Default, 32); // Match the default in the low level
            SetSetting(VirtualChannelSettings, FMODPlatform.Default, 128);
            SetSetting(SampleRateSettings, FMODPlatform.Default, 0);
            SetSetting(SpeakerModeSettings, FMODPlatform.Default, (int) FMOD.SPEAKERMODE.STEREO);

            AutomaticEventLoading = true;
            AutomaticSampleLoading = false;
        }
    }

}