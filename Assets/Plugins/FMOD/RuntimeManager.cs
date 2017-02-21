using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FMODUnity
{
    [AddComponentMenu("")]
    public class RuntimeManager : MonoBehaviour
    {

        static SystemNotInitializedException initException = null;
        static RuntimeManager instance;
        static bool isQuitting = false;
        [SerializeField]
        FMODPlatform fmodPlatform;
        static RuntimeManager Instance
        {
            get
            {
                if (initException != null)
                {
                    throw initException;
                }
                if (isQuitting)
                {
                    throw new Exception("FMOD Studio attempted access by script to RuntimeManager while application is quitting");
                }

                if (instance == null)
                {
                    var existing = FindObjectOfType(typeof(RuntimeManager)) as RuntimeManager;
                    if (existing != null)
                    {
                        // Older versions of the integration may have leaked the runtime manager game object into the scene,
                        // which was then serialized. It won't have valid pointers so don't use it.
                        if (existing.cachedPointers[0] != 0)
                        {
                            instance = existing;
                            instance.studioSystem = new FMOD.Studio.System((IntPtr)instance.cachedPointers[0]);
                            instance.lowlevelSystem = new FMOD.System((IntPtr)instance.cachedPointers[1]);
                            instance.mixerHead = new FMOD.DSP((IntPtr)instance.cachedPointers[2]);
                            return instance;
                        }
                    }                    

                    var gameObject = new GameObject("FMOD.UnityItegration.RuntimeManager");
                    instance = gameObject.AddComponent<RuntimeManager>();
                    DontDestroyOnLoad(gameObject);
                    gameObject.hideFlags = HideFlags.HideInHierarchy;

                    try
                    {
                        #if UNITY_ANDROID && !UNITY_EDITOR
            
                        // First, obtain the current activity context
                        AndroidJavaObject activity = null;
                        using (var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                        {
                            activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                        }

                        using (var fmodJava = new AndroidJavaClass("org.fmod.FMOD"))
                        {
                            if (fmodJava != null)
                            {
                                fmodJava.CallStatic("init", activity);
                            }
                            else
                            {
                                UnityEngine.Debug.LogWarning("FMOD Studio: Cannot initialiaze Java wrapper");
                            }
                        }
                        
                        #endif

                        RuntimeUtils.EnforceLibraryOrder();
                        instance.Initialiase(false);
                    }
                    catch (Exception e)
                    {
                        initException = e as SystemNotInitializedException;
                        if (initException == null)
                        {
                            initException = new SystemNotInitializedException(e);
                        }
                        throw initException;
                    }
                }

                return instance;
            }
       
        }

        public static FMOD.Studio.System StudioSystem
        {
            get { return Instance.studioSystem; }
        }


        public static FMOD.System LowlevelSystem
        {
            get { return Instance.lowlevelSystem; }
        }

        FMOD.Studio.System studioSystem;
        FMOD.System lowlevelSystem;
        FMOD.DSP mixerHead;

        [SerializeField]
        private long[] cachedPointers = new long[3];
        
        struct LoadedBank
        {
            public FMOD.Studio.Bank Bank;
            public int RefCount;
        }
        
        Dictionary<string, LoadedBank> loadedBanks = new Dictionary<string, LoadedBank>();
        Dictionary<string, uint> loadedPlugins = new Dictionary<string, uint>();

        // Explicit comparer to avoid issues on platforms that don't support JIT compilation
        class GuidComparer : IEqualityComparer<Guid>
        {
            bool IEqualityComparer<Guid>.Equals(Guid x, Guid y)
            {
                return x.Equals(y);
            }

            int IEqualityComparer<Guid>.GetHashCode(Guid obj)
            {
                return obj.GetHashCode();
            }
        }
        Dictionary<Guid, FMOD.Studio.EventDescription> cachedDescriptions = new Dictionary<Guid, FMOD.Studio.EventDescription>(new GuidComparer());

        void CheckInitResult(FMOD.RESULT result, string cause)
        {
            if (result != FMOD.RESULT.OK)
            {
                if (studioSystem != null)
                {
                    studioSystem.release();
                    studioSystem = null;
                }
                throw new SystemNotInitializedException(result, cause);
            }
        }

        void Initialiase(bool forceNoNetwork)
        {
            UnityEngine.Debug.Log("FMOD Studio: Creating runtime system instance");

            FMOD.RESULT result;
            result = FMOD.Studio.System.create(out studioSystem);
            CheckInitResult(result, "Creating System Object");
            studioSystem.getLowLevelSystem(out lowlevelSystem);

            Settings fmodSettings = Settings.Instance;
            fmodPlatform = RuntimeUtils.GetCurrentPlatform();            

            #if UNITY_EDITOR || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX) && DEVELOPMENT_BUILD)
                result = FMOD.Debug.Initialize(FMOD.DEBUG_FLAGS.LOG, FMOD.DEBUG_MODE.FILE, null, RuntimeUtils.LogFileName);
                if (result == FMOD.RESULT.ERR_FILE_NOTFOUND)
                {
                    UnityEngine.Debug.LogWarningFormat("FMOD Studio: Cannot open FMOD debug log file '{0}', logs will be missing for this session.", System.IO.Path.Combine(Application.dataPath, RuntimeUtils.LogFileName));
                }
                else
                {
                    CheckInitResult(result, "Applying debug settings");
                }
            #endif

            int realChannels = fmodSettings.GetRealChannels(fmodPlatform);
            result = lowlevelSystem.setSoftwareChannels(realChannels);
            CheckInitResult(result, "Set software channels");
            result = lowlevelSystem.setSoftwareFormat(
                fmodSettings.GetSampleRate(fmodPlatform),
                (FMOD.SPEAKERMODE)fmodSettings.GetSpeakerMode(fmodPlatform),
                0 // raw not supported
                );
            CheckInitResult(result, "Set software format");

            // Setup up the platforms recommended codec to match the real channel count
            FMOD.ADVANCEDSETTINGS advancedsettings = new FMOD.ADVANCEDSETTINGS();
            #if UNITY_EDITOR || UNITY_STANDALONE
            advancedsettings.maxVorbisCodecs = realChannels;
            #elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8_1 || UNITY_PSP2 || UNITY_WII
            advancedsettings.maxFADPCMCodecs = realChannels;
            #elif UNITY_XBOXONE
            advancedsettings.maxXMACodecs = realChannels;
            #elif UNITY_PS4
            advancedsettings.maxAT9Codecs = realChannels;
            #endif

            #if UNITY_5_0 || UNITY_5_1
            if (fmodSettings.IsLiveUpdateEnabled(fmodPlatform) && !forceNoNetwork)
            {
                UnityEngine.Debug.LogWarning("FMOD Studio: Detected Unity 5, running on port 9265");
                advancedsettings.profilePort = 9265;
            }
            #endif

            advancedsettings.randomSeed = (uint) DateTime.Now.Ticks;
            result = lowlevelSystem.setAdvancedSettings(ref advancedsettings);
            CheckInitResult(result, "Set advanced settings");

            FMOD.INITFLAGS lowlevelInitFlags = FMOD.INITFLAGS.NORMAL;
            FMOD.Studio.INITFLAGS studioInitFlags = FMOD.Studio.INITFLAGS.NORMAL | FMOD.Studio.INITFLAGS.DEFERRED_CALLBACKS;

            if (fmodSettings.IsLiveUpdateEnabled(fmodPlatform) && !forceNoNetwork)
            {
                studioInitFlags |= FMOD.Studio.INITFLAGS.LIVEUPDATE;
            }                       

            FMOD.RESULT initResult = studioSystem.initialize(
                fmodSettings.GetVirtualChannels(fmodPlatform),
                studioInitFlags,
                lowlevelInitFlags,
                IntPtr.Zero
                );

            CheckInitResult(initResult, "Calling initialize");

            // Dummy flush and update to get network state
            studioSystem.flushCommands();
            FMOD.RESULT updateResult = studioSystem.update();

            // Restart without liveupdate if there was a socket error
            if (updateResult == FMOD.RESULT.ERR_NET_SOCKET_ERROR)
            {
                studioSystem.release();
                UnityEngine.Debug.LogWarning("FMOD Studio: Cannot open network port for Live Update, restarting with Live Update disabled. Check for other applications that are running FMOD Studio");
                Initialiase(true);
            }
            else
            {
                // Load plugins (before banks)
                foreach (var pluginName in fmodSettings.Plugins)
                {
                    string pluginPath = RuntimeUtils.GetPluginPath(pluginName);
                    uint handle;
                    result = lowlevelSystem.loadPlugin(pluginPath, out handle);
                    #if UNITY_64 || UNITY_EDITOR_64
                    // Add a "64" suffix and try again
                    if (result == FMOD.RESULT.ERR_FILE_BAD || result == FMOD.RESULT.ERR_FILE_NOTFOUND)
                    {
                        pluginPath = RuntimeUtils.GetPluginPath(pluginName + "64");
                        result = lowlevelSystem.loadPlugin(pluginPath, out handle);
                    }
                    #endif
                    CheckInitResult(result, String.Format("Loading plugin '{0}' from '{1}'", pluginName, pluginPath));
                    loadedPlugins.Add(pluginName, handle);
                }

                // Always load strings bank
                try
                { 
                    LoadBank(fmodSettings.MasterBank + ".strings", fmodSettings.AutomaticSampleLoading);
                }
                catch (BankLoadException e)
                {
                    UnityEngine.Debug.LogException(e);
                }

                if (fmodSettings.AutomaticEventLoading)
                {
                    try
                    {
                        LoadBank(fmodSettings.MasterBank, fmodSettings.AutomaticSampleLoading);
                    }
                    catch (BankLoadException e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }

                    foreach (var bank in fmodSettings.Banks)
                    {
                        try
                        {
                            LoadBank(bank, fmodSettings.AutomaticSampleLoading);
                        }
                        catch (BankLoadException e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            };

            FMOD.ChannelGroup master;
            lowlevelSystem.getMasterChannelGroup(out master);
            master.getDSP(0, out mixerHead);
            mixerHead.setMeteringEnabled(false, true);
        }
        
        class AttachedInstance            
        {
            public FMOD.Studio.EventInstance instance;
            public Transform transform;
            public Rigidbody rigidBody;
        }

        List<AttachedInstance> attachedInstances = new List<AttachedInstance>(128);

        bool listenerWarningIssued = false;
        void Update()
        {
            if (studioSystem != null)
            {
                studioSystem.update();
                if (!hasListener && !listenerWarningIssued)
                {
                    listenerWarningIssued = true;
                    UnityEngine.Debug.LogWarning("FMOD Studio Integration: Please add an 'FMOD Studio Listener' component to your a camera in the scene for correct 3D positioning of sounds");
                }

                for (int i = 0; i < attachedInstances.Count; i++)
                {
                    FMOD.Studio.PLAYBACK_STATE playbackState = FMOD.Studio.PLAYBACK_STATE.STOPPED;
                    attachedInstances[i].instance.getPlaybackState(out playbackState);
                    if (!attachedInstances[i].instance.isValid() || 
                        playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED ||
                        attachedInstances[i].transform == null // destroyed game object
                        )
                    {
                        attachedInstances.RemoveAt(i);
                        i--;
                        continue;
                    }
                    attachedInstances[i].instance.set3DAttributes(RuntimeUtils.To3DAttributes(attachedInstances[i].transform, attachedInstances[i].rigidBody));
                }
            }
        }

        public static void AttachInstanceToGameObject(FMOD.Studio.EventInstance instance, Transform transform, Rigidbody rigidBody)
        {
            var attachedInstance = new AttachedInstance();
            attachedInstance.transform = transform;
            attachedInstance.instance = instance;
            attachedInstance.rigidBody = rigidBody;
            Instance.attachedInstances.Add(attachedInstance);
        }

        public static void DetachInstanceFromGameObject(FMOD.Studio.EventInstance instance)
        {
            var manager = Instance;
            for (int i = 0; i < manager.attachedInstances.Count; i++)
            {
                if (manager.attachedInstances[i].instance == instance)
                {
                    manager.attachedInstances.RemoveAt(i);
                    return;
                }
            }
        }

        //Rect windowRect = new Rect(10, 10, 300, 100);
        /*void OnGUI()
        {
            if (studioSystem != null && Settings.Instance.IsOverlayEnabled(fmodPlatform))
            {
                windowRect = GUI.Window(0, windowRect, DrawDebugOverlay, "FMOD Studio Debug");
            }
        }*/

        string lastDebugText;
        float lastDebugUpdate = 0;
        void DrawDebugOverlay(int windowID)
        {
            if (lastDebugUpdate + 0.25f < Time.unscaledTime)
            {
                if (initException != null)
                {
                    lastDebugText = initException.Message;
                }
                else
                {

                    StringBuilder debug = new StringBuilder();

                    FMOD.Studio.CPU_USAGE cpuUsage;
                    studioSystem.getCPUUsage(out cpuUsage);
                    debug.AppendFormat("CPU: dsp = {0:F1}%, studio = {1:F1}%\n", cpuUsage.dspUsage, cpuUsage.studioUsage);

                    int currentAlloc, maxAlloc;
                    FMOD.Memory.GetStats(out currentAlloc, out maxAlloc);
                    debug.AppendFormat("MEMORY: cur = {0}MB, max = {1}MB\n", currentAlloc >> 20, maxAlloc >> 20);

                    int realchannels, channels;
                    lowlevelSystem.getChannelsPlaying(out channels, out realchannels);
                    debug.AppendFormat("CHANNELS: real = {0}, total = {1}\n", realchannels, channels);

                    FMOD.DSP_METERING_INFO metering = new FMOD.DSP_METERING_INFO();
                    mixerHead.getMeteringInfo(null, metering);
                    float rms = 0;
                    for (int i = 0; i < metering.numchannels; i++)
                    {
                        rms += metering.rmslevel[i] * metering.rmslevel[i];
                    }
                    rms = Mathf.Sqrt(rms / (float)metering.numchannels);

                    float db = rms > 0 ? 20.0f * Mathf.Log10(rms * Mathf.Sqrt(2.0f)) : -80.0f;
                    if (db > 10.0f) db = 10.0f;

                    debug.AppendFormat("VOLUME: RMS = {0:f2}db\n", db);
                    lastDebugText = debug.ToString();
                    lastDebugUpdate = Time.unscaledTime;
                }
            }

            GUI.Label(new Rect(10, 20, 290, 100), lastDebugText);
            GUI.DragWindow();
        }        
        
        void OnDisable()
        {
            // If we're being torn down for a script reload - cache the native pointers in something unity can serialize
            cachedPointers[0] = (long)studioSystem.getRaw();
            cachedPointers[1] = (long)lowlevelSystem.getRaw();
            cachedPointers[2] = (long)mixerHead.getRaw();
        }

        void OnDestroy()
        {
            if (studioSystem != null)
            {
                UnityEngine.Debug.Log("FMOD Studio: Destroying runtime system instance");
                studioSystem.release();
                studioSystem = null;
            }
            initException = null;
            instance = null;
            isQuitting = true;
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (studioSystem != null && studioSystem.isValid())
			{
				if (pauseStatus)
				{
					lowlevelSystem.mixerSuspend();
				}
				else
				{
					lowlevelSystem.mixerResume();
				}
			}
        }

        public static void LoadBank(string bankName, bool loadSamples = false)
        {
            if (Instance.loadedBanks.ContainsKey(bankName))
            {
                LoadedBank loadedBank = Instance.loadedBanks[bankName];
                loadedBank.RefCount++;

                if (loadSamples)
                {
                    loadedBank.Bank.loadSampleData();
                }
            }
            else
            {
                LoadedBank loadedBank = new LoadedBank();
                string bankPath = RuntimeUtils.GetBankPath(bankName);
                FMOD.RESULT loadResult;
                #if UNITY_ANDROID && !UNITY_EDITOR
                if (!bankPath.StartsWith("file:///android_asset"))
                {
                    using (var www = new WWW(bankPath))
                    {
                        while (!www.isDone) { }
                        if (!String.IsNullOrEmpty(www.error))
                        {
                            throw new BankLoadException(bankPath, www.error);  
                        }
                        else
                        {
                            loadResult = Instance.studioSystem.loadBankMemory(www.bytes, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);
                        }
                    }
                }
                else
                #endif
                {
                    loadResult = Instance.studioSystem.loadBankFile(bankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);
                }

                if (loadResult == FMOD.RESULT.OK)
                {
                    loadedBank.RefCount = 1;
                    Instance.loadedBanks.Add(bankName, loadedBank);

                    if (loadSamples)
                    {
                        loadedBank.Bank.loadSampleData();
                    }
                }
                else if (loadResult == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
                {
                    // someone loaded this bank directly using the studio API
                    // TODO: will the null bank handle be an issue
                    loadedBank.RefCount = 2;
                    Instance.loadedBanks.Add(bankName, loadedBank);                    
                }
                else
                {
                    throw new BankLoadException(bankPath, loadResult);
                }
            }
        }

        public static void UnloadBank(string bankName)
        {
            LoadedBank loadedBank;
            if (Instance.loadedBanks.TryGetValue(bankName, out loadedBank))
            {
                loadedBank.RefCount--;
                if (loadedBank.RefCount == 0)
                {
                    loadedBank.Bank.unload();
                    Instance.loadedBanks.Remove(bankName);
                }
            }
        }

        public static Guid PathToGUID(string path)
        {
            Guid guid = Guid.Empty;
            if (path.StartsWith("{"))
            {
                FMOD.Studio.Util.ParseID(path, out guid);
            }
            else
            {
                var result = Instance.studioSystem.lookupID(path, out guid);
                if (result == FMOD.RESULT.ERR_EVENT_NOTFOUND)
                {
                    throw new EventNotFoundException(path);
                }
            }
            return guid;
        }
        
        public static FMOD.Studio.EventInstance CreateInstance(string path) 
        {
            try
            {
                return CreateInstance(PathToGUID(path)); 
            }
            catch(EventNotFoundException)
            {
                // Switch from exception with GUID to exception with path
                //throw new EventNotFoundException(path);
                return null;
            }
        }

        public static FMOD.Studio.EventInstance CreateInstance(Guid guid)
        {
            FMOD.Studio.EventDescription eventDesc = GetEventDescription(guid);
            FMOD.Studio.EventInstance newInstance;
            eventDesc.createInstance(out newInstance);
            return newInstance;
        }
        
        public static void PlayOneShot(string path, Vector3 position = new Vector3())
        {
            try
            {
                PlayOneShot(PathToGUID(path), position);
            }
            catch (EventNotFoundException)
            {
                // Switch from exception with GUID to exception with path
                //throw new EventNotFoundException(path);
            }
        }

        public static void PlayOneShot(Guid guid, Vector3 position = new Vector3())
        {
            var instance = CreateInstance(guid);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            instance.start();
            instance.release();
        }

        public static void PlayOneShotAttached(string path, GameObject gameObject)
        {
            try
            {
                PlayOneShotAttached(PathToGUID(path), gameObject);
            }
            catch (EventNotFoundException)
            {
                // Switch from exception with GUID to exception with path
                //throw new EventNotFoundException(path);
            }
        }

        public static void PlayOneShotAttached(Guid guid, GameObject gameObject)
        {
            var instance = CreateInstance(guid);
            AttachInstanceToGameObject(instance, gameObject.transform, gameObject.GetComponent<Rigidbody>());
            instance.start();
        }

        public static FMOD.Studio.EventDescription GetEventDescription(string path)
        {
            try
            {
                return GetEventDescription(PathToGUID(path));
            }
            catch (EventNotFoundException)
            {
                return null;
                //throw new EventNotFoundException(path);
            }
        }

        public static FMOD.Studio.EventDescription GetEventDescription(Guid guid)
        {
            FMOD.Studio.EventDescription eventDesc = null;
            if (Instance.cachedDescriptions.ContainsKey(guid) && Instance.cachedDescriptions[guid].isValid())
            {
                eventDesc = Instance.cachedDescriptions[guid];
            }
            else
            {
                var result = Instance.studioSystem.getEventByID(guid, out eventDesc);

                if (result != FMOD.RESULT.OK)
                {
                    throw new EventNotFoundException(guid);
                }

                if (eventDesc != null && eventDesc.isValid())
                {
                    Instance.cachedDescriptions[guid] = eventDesc;
                }
            }
            return eventDesc;
        }

        static bool hasListener;
        public static bool HasListener
        {
            set { hasListener = value; }
            get { return hasListener;  }
        }

        public static void SetListenerLocation(GameObject gameObject, Rigidbody rigidBody = null)
        {
            Instance.studioSystem.setListenerAttributes(0, RuntimeUtils.To3DAttributes(gameObject, rigidBody));
        }

        public static void SetListenerLocation(Transform transform)
        {
            Instance.studioSystem.setListenerAttributes(0, transform.To3DAttributes());
        }

        public static FMOD.Studio.Bus GetBus(String path)
        {
            FMOD.RESULT result;
            FMOD.Studio.Bus bus;
            result = StudioSystem.getBus(path, out bus);
            if (result != FMOD.RESULT.OK)
            {

            }
            return bus;
        }

        public static FMOD.Studio.VCA GetVCA(String path)
        {
            FMOD.RESULT result;
            FMOD.Studio.VCA vca;
            result = StudioSystem.getVCA(path, out vca);
            if (result != FMOD.RESULT.OK)
            {

            }
            return vca;
        }
    }
}
