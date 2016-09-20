using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text;
using System.Net.Sockets;

namespace FMODUnity
{

    public enum PreviewState
    {
        Stopped,
        Playing,
        Paused,
    }

    [InitializeOnLoad]
    class EditorUtils : MonoBehaviour
    {
        public static void CheckResult(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                UnityEngine.Debug.LogError(string.Format("FMOD Studio: Encounterd Error: {0} {1}", result, FMOD.Error.String(result)));
            }
        }

        const string BuildFolder = "Build";

        public static string GetBankDirectory()
        {
            if (Settings.Instance.HasSourceProject && !String.IsNullOrEmpty(Settings.Instance.SourceProjectPath))
            {
                string projectPath = Settings.Instance.SourceProjectPath;
                string projectFolder = Path.GetDirectoryName(projectPath);
                return Path.Combine(projectFolder, BuildFolder);            
            }
            else if (!String.IsNullOrEmpty(Settings.Instance.SourceBankPath))
            {
                return Settings.Instance.SourceBankPath;
            }
            return null;
        }

        public static void ValidateSource(out bool valid, out string reason)
        {
            valid = true;
            reason = "";
            var settings = Settings.Instance;
            if (settings.HasSourceProject)
            {
                if (String.IsNullOrEmpty(settings.SourceProjectPath))
                {
                    valid = false;
                    reason = "FMOD Studio Project path not set";
                    return;
                }
                if (!File.Exists(settings.SourceProjectPath))
                {
                    valid = false;
                    reason = "FMOD Studio Project not found";
                    return;
                }

                string projectPath = settings.SourceProjectPath;
                string projectFolder = Path.GetDirectoryName(projectPath);
                string buildFolder = Path.Combine(projectFolder, BuildFolder);
                if (!Directory.Exists(buildFolder) ||
                    Directory.GetDirectories(buildFolder).Length == 0 ||
                    Directory.GetFiles(Directory.GetDirectories(buildFolder)[0], "*.bank").Length == 0
                    )
                {
                    valid = false;
                    reason = "FMOD Studio Project does not contain any built data. Please build your project in FMOD Studio.";
                    return;
                }
            }
            else
            {
                if (String.IsNullOrEmpty(settings.SourceBankPath))
                {
                    valid = false;
                    reason = "Build path not set";
                    return;
                }
                if (!Directory.Exists(settings.SourceBankPath))
                {
                    valid = false;
                    reason = "Build path doesn't exist";
                    return;
                }

                if (settings.HasPlatforms)
                {
                    if (Directory.GetDirectories(settings.SourceBankPath).Length == 0)
                    {
                        valid = false;
                        reason = "Build path doesn't contain any platform folders";
                        return;
                    }
                }
                else
                {
                    if (Directory.GetFiles(settings.SourceBankPath, "*.strings.bank").Length == 0)
                    {
                        valid = false;
                        reason = "Build path doesn't contain the contents of an FMOD Studio Build";
                        return;
                    }
                }
            }
        }

        public static string[] GetBankPlatforms()
        {
            string buildFolder = GetBankDirectory();
            try
            {
                if (Directory.GetFiles(buildFolder, "*.bank").Length == 0)
                {                
                    string[] buildDirectories = Directory.GetDirectories(buildFolder);
                    string[] buildNames = new string[buildDirectories.Length];
                    for (int i = 0; i < buildDirectories.Length; i++)
                    {
                        buildNames[i] = Path.GetFileNameWithoutExtension(buildDirectories[i]);
                    }
                    return buildNames;
                }
            }
            catch
            {
            }
            return new string[0];
        }

        public static FMODPlatform GetFMODPlatform()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    return FMODPlatform.Android;
				#if UNITY_4_6 || UNITY_4_7
                case BuildTarget.iPhone:
				#else
				case BuildTarget.iOS:
				#endif
                    return FMODPlatform.iOS;
                case BuildTarget.PS4:
                    return FMODPlatform.PS4;
                case BuildTarget.PSP2:
                    return FMODPlatform.PSVita;
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                    return FMODPlatform.Linux;
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return FMODPlatform.Mac;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return FMODPlatform.Windows;
                case BuildTarget.XboxOne:
                    return FMODPlatform.XboxOne;
				#if UNITY_4_6 || UNITY_4_7
                case BuildTarget.MetroPlayer:
                #else
                case BuildTarget.WSAPlayer:
                #endif
                #if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_0 && !UNITY_5_1
                    if (EditorUserBuildSettings.wsaSDK == WSASDK.UWP)
                    {
                        return FMODPlatform.UWP;
                    }
                #endif
                    if (EditorUserBuildSettings.wsaSDK == WSASDK.PhoneSDK81)
                    { 
                        return FMODPlatform.WindowsPhone;
                    }
                    return FMODPlatform.None;
                #if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
			    case BuildTarget.tvOS:
					return FMODPlatform.AppleTV;
                #endif
                default:
                    return FMODPlatform.None;
            }
        }

        static string VerionNumberToString(uint version)
        {
            uint major = (version & 0x00FF0000) >> 16;
            uint minor = (version & 0x0000FF00) >> 8;
            uint patch = (version & 0x000000FF);

            return major.ToString("X1") + "." + minor.ToString("X2") + "." + patch.ToString("X2");
        }

        static EditorUtils()
	    {
            EditorApplication.update += Update;
		    EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
	    }
        

        static void HandleOnPlayModeChanged()
	    {
            // Ensure we don't leak system handles in the DLL
		    if (EditorApplication.isPlayingOrWillChangePlaymode &&
			    !EditorApplication.isPaused)
		    {
        	    DestroySystem();
		    }
	    }

        static void Update()
        {
            // Ensure we don't leak system handles in the DLL
            if (EditorApplication.isCompiling)
            {
                DestroySystem();
            }

            // Update the editor system
            if (system != null && system.isValid())
            {
                CheckResult(system.update());
            }

            if (previewEventInstance != null)
            {
                FMOD.Studio.PLAYBACK_STATE state;
                previewEventInstance.getPlaybackState(out state);
                if (previewState == PreviewState.Playing && state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    PreviewStop();
                }
            }
        }

        static FMOD.Studio.System system;

        static void DestroySystem()
        {
            if (system != null)
            {
                UnityEngine.Debug.Log("FMOD Studio: Destroying editor system instance");
                system.release();
                system = null;
            }
        }

        static void CreateSystem()
        {
            UnityEngine.Debug.Log("FMOD Studio: Creating editor system instance");
            RuntimeUtils.EnforceLibraryOrder();

            CheckResult(FMOD.Debug.Initialize(FMOD.DEBUG_FLAGS.LOG, FMOD.DEBUG_MODE.FILE, null, "fmod_editor.log"));

            CheckResult(FMOD.Studio.System.create(out system));

            FMOD.System lowlevel;
            CheckResult(system.getLowLevelSystem(out lowlevel));

            // Use play-in-editor speaker mode for event browser preview and metering
            lowlevel.setSoftwareFormat(0, (FMOD.SPEAKERMODE)Settings.Instance.GetSpeakerMode(FMODPlatform.Default),0 );

            CheckResult(system.initialize(256, FMOD.Studio.INITFLAGS.ALLOW_MISSING_PLUGINS | FMOD.Studio.INITFLAGS.SYNCHRONOUS_UPDATE, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));

            FMOD.ChannelGroup master;
            CheckResult(lowlevel.getMasterChannelGroup(out master));
            FMOD.DSP masterHead;
            CheckResult(master.getDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, out masterHead));
            CheckResult(masterHead.setMeteringEnabled(false, true));
        }

        public static void UpdateParamsOnEmitter(SerializedObject serializedObject, string path)
        {
            if (String.IsNullOrEmpty(path) || EventManager.EventFromPath(path) == null)
            {
                return;
            }

            var eventRef = EventManager.EventFromPath(path);
            serializedObject.ApplyModifiedProperties();
            if (serializedObject.isEditingMultipleObjects)
            {
                foreach (var obj in serializedObject.targetObjects)
                {
                    UpdateParamsOnEmitter(obj, eventRef);
                }
            }
            else
            {
                UpdateParamsOnEmitter(serializedObject.targetObject, eventRef);
            }
            serializedObject.Update();
        }
        
        private static void UpdateParamsOnEmitter(UnityEngine.Object obj, EditorEventRef eventRef)
        {
            var emitter = obj as StudioEventEmitter;
            if (emitter == null)
            {
                // Custom game object
                return;
            }

            for (int i = 0; i < emitter.Params.Length; i++)
            {
                if (!eventRef.Parameters.Exists((x) => x.Name == emitter.Params[i].Name))
                {
                    int end = emitter.Params.Length - 1;
                    emitter.Params[i] = emitter.Params[end];
                    Array.Resize<ParamRef>(ref emitter.Params, end);
                    i--;
                }
            }
        }

        public static FMOD.Studio.System System
        {
            get
            {
                if (system == null)
                {
                    CreateSystem();
                }
                return system;
            }
        }

        [MenuItem("FMOD/Help/Integration Manual", priority = 3)]
        static void OnlineManual()
        {
            Application.OpenURL("http://www.fmod.org/documentation/#content/generated/engine_new_unity/overview.html");
        }

        [MenuItem("FMOD/Help/API Documentation", priority = 4)]
        static void OnlineAPIDocs()
        {
            Application.OpenURL("http://www.fmod.org/documentation/#content/generated/studio_api.html");
        }

        [MenuItem("FMOD/Help/Support Forum", priority = 5)]
        static void OnlineQA()
        {
            Application.OpenURL("http://www.fmod.org/questions");
        }

        [MenuItem("FMOD/Help/Revision History", priority = 6)]
        static void OnlineRevisions()
        {
            Application.OpenURL("http://www.fmod.org/documentation/#content/generated/common/revision.html");
        }

        [MenuItem("FMOD/About Integration", priority = 7)]
        public static void About()
        {
            FMOD.System lowlevel;
            CheckResult(System.getLowLevelSystem(out lowlevel));

            uint version;
            CheckResult(lowlevel.getVersion(out version));

            EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "Version: " + VerionNumberToString(version), "OK");
        }

        static FMOD.Studio.Bank masterBank;
        static FMOD.Studio.Bank previewBank;
        static FMOD.Studio.EventDescription previewEventDesc;
        static FMOD.Studio.EventInstance previewEventInstance;
        
        static PreviewState previewState;
        public static PreviewState PreviewState
        {
            get { return previewState; }
        }

        public static void PreviewEvent(EditorEventRef eventRef)
        {
            bool load = true;
            if (previewEventDesc != null)
            {
                Guid guid;
                previewEventDesc.getID(out guid);
                if (guid == eventRef.Guid)
                {
                    load = false;
                }
                else
                {
                    PreviewStop();
                }
            }

            if (load)
            {
                CheckResult(System.loadBankFile(EventManager.MasterBank.Path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out masterBank));
                CheckResult(System.loadBankFile(eventRef.Banks[0].Path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out previewBank));

                CheckResult(System.getEventByID(eventRef.Guid, out previewEventDesc));
                CheckResult(previewEventDesc.createInstance(out previewEventInstance));
            }

            CheckResult(previewEventInstance.start());
            previewState = PreviewState.Playing;
        }

        public static void PreviewUpdateParameter(string paramName, float paramValue)
        {
            if (previewEventInstance != null)
            {
                CheckResult(previewEventInstance.setParameterValue(paramName, paramValue));
            }
        }

        public static void PreviewUpdatePosition(float distance, float orientation)
        {
            if (previewEventInstance != null)
            {
                // Listener at origin
                FMOD.ATTRIBUTES_3D pos = new FMOD.ATTRIBUTES_3D();
                pos.position.x = (float)Math.Sin(orientation) * distance;
                pos.position.y = (float)Math.Cos(orientation) * distance;
                pos.forward.x = 1.0f;
                pos.up.z = 1.0f;
                CheckResult(previewEventInstance.set3DAttributes(pos));
            }
        }

        public static void PreviewPause()
        {
            if (previewEventInstance != null)
            {
                bool paused;
                CheckResult(previewEventInstance.getPaused(out paused));
                CheckResult(previewEventInstance.setPaused(!paused));
                previewState = paused ? PreviewState.Playing : PreviewState.Paused;
            }
        }

        public static void PreviewStop()
        {
            if (previewEventInstance != null)
            {
                previewEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                previewEventInstance.release();
                previewEventInstance = null;
                previewEventDesc = null;
                previewBank.unload();
                masterBank.unload();
                previewState = PreviewState.Stopped;
            }
        }

        public static float[] GetMetering()
        {
            FMOD.System lowlevel;
            CheckResult(System.getLowLevelSystem(out lowlevel));
            FMOD.ChannelGroup master;
            CheckResult(lowlevel.getMasterChannelGroup(out master));
            FMOD.DSP masterHead;
            CheckResult(master.getDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, out masterHead));

            FMOD.DSP_METERING_INFO inputMetering = null;
            FMOD.DSP_METERING_INFO outputMetering = new FMOD.DSP_METERING_INFO();
            CheckResult(masterHead.getMeteringInfo(inputMetering, outputMetering));

            FMOD.SPEAKERMODE mode;
            int rate, raw;
            lowlevel.getSoftwareFormat(out rate, out mode, out raw);
            int channels;
            lowlevel.getSpeakerModeChannels(mode, out channels);

            float[] data = new float[outputMetering.numchannels > 0 ? outputMetering.numchannels : channels];
            if (outputMetering.numchannels > 0)
            {
                Array.Copy(outputMetering.rmslevel, data, outputMetering.numchannels);
            }
            return data;
        }


        const int StudioScriptPort = 3663;
        static NetworkStream networkStream = null;
        static Socket socket = null;
        static IAsyncResult socketConnection = null;

        static NetworkStream ScriptStream
        {
            get
            {
                if (networkStream == null)
                {
                    try
                    {
                        if (socket == null)
                        {
                            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        }

                        if (!socket.Connected)
                        {
                            socketConnection = socket.BeginConnect("127.0.0.1", StudioScriptPort, null, null);
                            socketConnection.AsyncWaitHandle.WaitOne();
                            socket.EndConnect(socketConnection);
                            socketConnection = null;
                        }

                        networkStream = new NetworkStream(socket);

                        byte[] headerBytes = new byte[128];
                        int read = ScriptStream.Read(headerBytes, 0, 128);
                        string header = Encoding.UTF8.GetString(headerBytes, 0, read - 1);
                        if (header.StartsWith("log():"))
                        {
                            UnityEngine.Debug.Log("FMOD Studio: Script Client returned " + header.Substring(6));
                        }    
                    }        
                    catch (Exception e)
                    {
                        UnityEngine.Debug.Log("FMOD Studio: Script Client failed to connect - Check FMOD Studio is running");

                        socketConnection = null;
                        socket = null;
                        networkStream = null;

                        throw e;
                    }
                }
                return networkStream;
            }
        }

        private static void AsyncConnectCallback(IAsyncResult result)
        {
            try
            {
                socket.EndConnect(result);
            }
            catch (Exception)
            {
            }
            finally
            {
                socketConnection = null;
            }
        }

        public static bool IsConnectedToStudio()
        {
            try
            {
                if (socket != null && socket.Connected)
                {
                    if (SendScriptCommand("true"))
                    {
                        return true;
                    }
                }

                if (socketConnection == null)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socketConnection = socket.BeginConnect("127.0.0.1", StudioScriptPort, AsyncConnectCallback, null);
                }

                return false;

            }
            catch(Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public static bool SendScriptCommand(string command)
        {
            byte[] commandBytes = Encoding.UTF8.GetBytes(command);
            try
            {
                ScriptStream.Write(commandBytes, 0, commandBytes.Length);
                byte[] commandReturnBytes = new byte[128];
                int read = ScriptStream.Read(commandReturnBytes, 0, 128);
                string result = Encoding.UTF8.GetString(commandReturnBytes, 0, read - 1);
                return (result.Contains("true"));
            }
            catch (Exception)
            {
                if (networkStream != null)
                {
                    networkStream.Close();
                    networkStream = null;
                }
                //UnityEngine.Debug.Log("FMOD Studio: Script Client failed to connect - Check FMOD Studio is running");
                return false;
            }
        }


        public static string GetScriptOutput(string command)
        {
            byte[] commandBytes = Encoding.UTF8.GetBytes(command);
            try
            {
                ScriptStream.Write(commandBytes, 0, commandBytes.Length);
                byte[] commandReturnBytes = new byte[2048];
                int read = ScriptStream.Read(commandReturnBytes, 0, commandReturnBytes.Length);
                string result = Encoding.UTF8.GetString(commandReturnBytes, 0, read - 1);
                if (result.StartsWith("out():"))
                {
                    return result.Substring(6).Trim();
                }
                return null;
            }
            catch (Exception)
            {
                networkStream.Close();
                networkStream = null;
                //UnityEngine.Debug.Log("FMOD Studio: Script Client failed to connect - Check FMOD Studio is running");
                return null;
            }
        }

        public static bool IsFileOpenByStudio(string path)
        {
            bool open = true;
            try
            {
                using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    open = false;
                }
            }
            catch (Exception)
            {
            }
            return open;
        }
        
        private static string GetMasterBank()
        {
            GetScriptOutput(String.Format("masterBankFolder = studio.project.workspace.masterBankFolder;"));
            string bankCountString = GetScriptOutput(String.Format("masterBankFolder.items.length;"));
            int bankCount = Int32.Parse(bankCountString);
            for (int i = 0; i < bankCount; i++)
            {
                string isMaster = GetScriptOutput(String.Format("masterBankFolder.items[{1}].isOfExactType(\"MasterBank\");", i));
                if (isMaster == "true")
                {
                    string guid = GetScriptOutput(String.Format("masterBankFolder.items[{1}].id;", i));
                    return guid;
                }
            }
            return "";
        }
        
        private static bool CheckForNameConflict(string folderGuid, string eventName)
        {
            GetScriptOutput(String.Format("nameConflict = false;"));
            GetScriptOutput(String.Format("checkFunction = function(val) {{ nameConflict |= val.name == \"{0}\"; }};", eventName));
            GetScriptOutput(String.Format("studio.project.lookup(\"{0}\").items.forEach(checkFunction, this); ", folderGuid));
            string conflictBool = GetScriptOutput(String.Format("nameConflict;"));
            return conflictBool == "1";
        }
        
        public static string CreateStudioEvent(string eventPath, string eventName)
        {
            var folders = eventPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string folderGuid = EditorUtils.GetScriptOutput("studio.project.workspace.masterEventFolder.id;");
            for (int i = 0; i < folders.Length; i++)
            {
                string parentGuid = folderGuid;
                GetScriptOutput(String.Format("guid = \"\";"));
                GetScriptOutput(String.Format("findFunc = function(val) {{ guid = val.isOfType(\"EventFolder\") && val.name == \"{0}\" ? val.id : guid; }};", folders[i]));
                GetScriptOutput(String.Format("studio.project.lookup(\"{0}\").items.forEach(findFunc, this);", folderGuid));
                folderGuid = GetScriptOutput(String.Format("guid;"));
                if (folderGuid == "")
                {
                    GetScriptOutput(String.Format("folder = studio.project.create(\"EventFolder\");"));
                    GetScriptOutput(String.Format("folder.name = \"{0}\"", folders[i]));
                    GetScriptOutput(String.Format("folder.folder = studio.project.lookup(\"{0}\");", parentGuid));
                    folderGuid = GetScriptOutput(String.Format("folder.id;"));
                }
            }

            if (CheckForNameConflict(folderGuid, eventName))
            {
                EditorUtility.DisplayDialog("Name Conflict", String.Format("The event {0} already exists under {1}", eventName, eventPath), "OK");
                return null;
            }

            GetScriptOutput("event = studio.project.create(\"Event\");");
            GetScriptOutput("event.note = \"Placeholder created via Unity\";");
            GetScriptOutput(String.Format("event.name = \"{0}\"", eventName));
            GetScriptOutput(String.Format("event.folder = studio.project.lookup(\"{0}\");", folderGuid));

            // Add a group track
            GetScriptOutput("track = studio.project.create(\"GroupTrack\");");
            GetScriptOutput("track.mixerGroup.output = event.mixer.masterBus;");
            GetScriptOutput("track.mixerGroup.name = \"Audio 1\";");
            GetScriptOutput("event.relationships.groupTracks.add(track);");

            // Add tags
            GetScriptOutput("tag = studio.project.create(\"Tag\");");
            GetScriptOutput("tag.name = \"placeholder\";");
            GetScriptOutput("tag.folder = studio.project.workspace.masterTagFolder;");
            GetScriptOutput("event.relationships.tags.add(tag);");

            string eventGuid = GetScriptOutput(String.Format("event.id;"));
            return eventGuid;
        }
    }
}