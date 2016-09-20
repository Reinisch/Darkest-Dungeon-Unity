using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Callbacks;

namespace FMODUnity
{
    [InitializeOnLoad]
    public class EventManager : MonoBehaviour
    {
        const string CacheAssetName = "FMODStudioCache";
        const string CacheAssetFullName = "Assets/" + CacheAssetName + ".asset";
        static EventCache eventCache;

        const string StringBankExtension = "strings.bank";
        const string BankExtension = "bank";

        static int countdownTimer;

        static void ClearCache()
        {
            countdownTimer = 1;
            eventCache.StringsBankWriteTime = DateTime.MinValue;
            eventCache.EditorBanks.Clear();
            eventCache.EditorEvents.Clear();
            OnCacheChange();
        }
        
        static public void UpdateCache()
        {
            // Deserialize the cache from the unity resources
            if (eventCache == null)
            {
                eventCache = AssetDatabase.LoadAssetAtPath(CacheAssetFullName, typeof(EventCache)) as EventCache;
                if (eventCache == null || eventCache.cacheVersion != EventCache.CurrentCacheVersion)
                {
                    UnityEngine.Debug.Log("FMOD Studio: Cannot find serialized event cache or cache in old format, creating new instance");
                    eventCache = ScriptableObject.CreateInstance<EventCache>();
                    eventCache.cacheVersion = EventCache.CurrentCacheVersion;

                    AssetDatabase.CreateAsset(eventCache, CacheAssetFullName);
                }
            }

            if (EditorUtils.GetBankDirectory() == null)
            {
                ClearCache();
                return;
            }

            string defaultBankFolder = null;
            
            if (!Settings.Instance.HasPlatforms)
            {
                defaultBankFolder = EditorUtils.GetBankDirectory();
            }
            else
            {
                FMODPlatform platform = EditorUtils.GetFMODPlatform();
                if (platform == FMODPlatform.None)
                {
                    platform = FMODPlatform.PlayInEditor;
                }

                defaultBankFolder = Path.Combine(EditorUtils.GetBankDirectory(), Settings.Instance.GetBankPlatform(platform));
            }

            string[] bankPlatforms = EditorUtils.GetBankPlatforms();
            string[] bankFolders = new string[bankPlatforms.Length];            
            for (int i = 0; i < bankPlatforms.Length; i++)
            {
                bankFolders[i] = Path.Combine(EditorUtils.GetBankDirectory(), bankPlatforms[i]);
            }

            List<String> stringBanks = new List<string>(0);
            try
            {
                var files = Directory.GetFiles(defaultBankFolder, "*." + StringBankExtension);
                stringBanks = new List<string>(files);
            }
            catch
            {
            }

            // Strip out OSX resource-fork files that appear on FAT32
            stringBanks.RemoveAll((x) => Path.GetFileName(x).StartsWith("._"));

            if (stringBanks.Count == 0)
            {
                bool wasValid = eventCache.StringsBankWriteTime != DateTime.MinValue;
                ClearCache();
                if (wasValid)
                {
                    UnityEngine.Debug.LogError(String.Format("FMOD Studio: Directory {0} doesn't contain any banks. Build from the tool or check the path in the settings", defaultBankFolder));
                }
                return;
            }

            // If we have multiple .strings.bank files find the most recent
            stringBanks.Sort((a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));
            string stringBankPath = stringBanks[0];

            // Use the string bank timestamp as a marker for the most recent build of any bank because it gets exported every time
            if (File.GetLastWriteTime(stringBankPath) == eventCache.StringsBankWriteTime)
            {
                countdownTimer = 1;
                return;
            }            

            if (EditorUtils.IsFileOpenByStudio(stringBankPath))
            {
                countdownTimer = 1;
                return;
            }
            

            FMOD.Studio.Bank stringBank = null;
            EditorUtils.CheckResult(EditorUtils.System.loadBankFile(stringBankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out stringBank));
            if (stringBank == null)
            {
                countdownTimer = 1;
                return;
            }

            // Iterate every string in the strings bank and look for any that identify banks
            int stringCount;
            stringBank.getStringCount(out stringCount);
            List<string> bankFileNames = new List<string>();
            for (int stringIndex = 0; stringIndex < stringCount; stringIndex++)
            {
                string currentString;
                Guid currentGuid;
                stringBank.getStringInfo(stringIndex, out currentGuid, out currentString);
                const string BankPrefix = "bank:/";
                int BankPrefixLength = BankPrefix.Length;
                if (currentString.StartsWith(BankPrefix))
                {
                    string bankFileName = currentString.Substring(BankPrefixLength) + "." + BankExtension;
                    if (!bankFileName.Contains(StringBankExtension)) // filter out the strings bank
                    {
                        bankFileNames.Add(bankFileName);
                    }
                }
            }

            // Unload the strings bank
            stringBank.unload();

            // Check if any of the files are still being written by studio
            foreach (string bankFileName in bankFileNames)
            {
                string bankPath = Path.Combine(defaultBankFolder, bankFileName);

                if (!File.Exists(bankPath))
                {
                    countdownTimer = 1;
                    return;
                }                

                EditorBankRef bankRef = eventCache.EditorBanks.Find((x) => bankPath == x.Path);
                if (bankRef == null)
                {
                    if (EditorUtils.IsFileOpenByStudio(bankPath))
                    {
                        countdownTimer = 1;
                        return;
                    }
                    continue;
                }

                if (bankRef.LastModified != File.GetLastWriteTime(bankPath))
                {
                    if (EditorUtils.IsFileOpenByStudio(bankPath))
                    {
                        countdownTimer = 1;
                        return;
                    }
                }
            }

            // Do one extra loop through the in-use check in case we catch studio exactly in-between updating two files.
            if (countdownTimer-- > 0)
            {
                return;
            }

            // All files are finished being modified by studio so update the cache
            // Reload the strings bank
            EditorUtils.CheckResult(EditorUtils.System.loadBankFile(stringBankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out stringBank));
            if (stringBank == null)
            {
                ClearCache();
                return;
            }
            eventCache.StringsBankWriteTime = File.GetLastWriteTime(stringBankPath);
            string masterBankFileName = Path.GetFileName(stringBankPath).Replace(StringBankExtension, BankExtension);

            AssetDatabase.StartAssetEditing();

            eventCache.EditorBanks.ForEach((x) => x.Exists = false);

            foreach (string bankFileName in bankFileNames)
            {
                string bankPath = Path.Combine(defaultBankFolder, bankFileName);
                
                EditorBankRef bankRef = eventCache.EditorBanks.Find((x) => bankPath == x.Path);

                // New bank we've never seen before
                if (bankRef == null)
                {
                    bankRef = ScriptableObject.CreateInstance<EditorBankRef>();
                    AssetDatabase.AddObjectToAsset(bankRef, eventCache);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(bankRef));
                    bankRef.Path = bankPath;
                    bankRef.LastModified = DateTime.MinValue;
                    bankRef.FileSizes = new List<EditorBankRef.NameValuePair>();
                    eventCache.EditorBanks.Add(bankRef);
                }

                bankRef.Exists = true;
                
                // Timestamp check - if it doesn't match update events from that bank
                if (bankRef.LastModified != File.GetLastWriteTime(bankPath))
                {
                    bankRef.LastModified = File.GetLastWriteTime(bankPath);                    
                    UpdateCacheBank(bankRef);
                }

                // Update file sizes
                bankRef.FileSizes.Clear();      
                for (int i = 0; i < bankPlatforms.Length; i++)
                {		
                    string platformBankPath = Path.Combine(bankFolders[i], bankFileName);
                    var fileInfo = new FileInfo(platformBankPath);
                    if (fileInfo.Exists)
                    {
                        bankRef.FileSizes.Add(new EditorBankRef.NameValuePair(bankPlatforms[i], fileInfo.Length));
                    }
                }

                if (bankFileName == masterBankFileName)
                {
                    eventCache.MasterBankRef = bankRef;
                }
            }


            // Unload the strings bank
            stringBank.unload();

            // Remove any stale entries from bank and event lists
            eventCache.EditorBanks.FindAll((x) => !x.Exists).ForEach(RemoveCacheBank);
            eventCache.EditorBanks.RemoveAll((x) => !x.Exists);
            eventCache.EditorEvents.RemoveAll((x) => x.Banks.Count == 0);

            OnCacheChange();
            AssetDatabase.StopAssetEditing();
        }

        static void UpdateCacheBank(EditorBankRef bankRef)
        {
            // Clear out any cached events from this bank
            eventCache.EditorEvents.ForEach((x) => x.Banks.Remove(bankRef));

            FMOD.Studio.Bank bank;
            bankRef.LoadResult = EditorUtils.System.loadBankFile(bankRef.Path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);

            if (bankRef.LoadResult == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
            {
                EditorUtils.System.getBank(bankRef.Name, out bank);
                bank.unload();
                bankRef.LoadResult = EditorUtils.System.loadBankFile(bankRef.Path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);
            }

            if (bankRef.LoadResult == FMOD.RESULT.OK)
            {
                // Iterate all events in the bank and cache them
                FMOD.Studio.EventDescription[] eventList;
                var result = bank.getEventList(out eventList);
                if (result == FMOD.RESULT.OK)
                {
                    foreach (var eventDesc in eventList)
                    {
                        string path;
                        eventDesc.getPath(out path);
                        EditorEventRef eventRef = eventCache.EditorEvents.Find((x) => x.Path == path);
                        if (eventRef == null)
                        {
                            eventRef = ScriptableObject.CreateInstance<EditorEventRef>();
                            AssetDatabase.AddObjectToAsset(eventRef, eventCache);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(eventRef));
                            eventRef.Banks = new List<EditorBankRef>();
                            eventCache.EditorEvents.Add(eventRef);
                        }

                        eventRef.Banks.Add(bankRef);
                        Guid guid;
                        eventDesc.getID(out guid);
                        eventRef.Guid = guid;
                        eventRef.Path = path;
                        eventDesc.is3D(out eventRef.Is3D);
                        eventDesc.isOneshot(out eventRef.IsOneShot);
                        eventDesc.isStream(out eventRef.IsStream);
                        eventDesc.getMaximumDistance(out eventRef.MaxDistance);
                        eventDesc.getMinimumDistance(out eventRef.MinDistance);
                        int paramCount = 0;
                        eventDesc.getParameterCount(out paramCount);
                        eventRef.Parameters = new List<EditorParamRef>(paramCount);
                        for (int paramIndex = 0; paramIndex < paramCount; paramIndex++)
                        {
                            EditorParamRef paramRef = ScriptableObject.CreateInstance<EditorParamRef>();
                            AssetDatabase.AddObjectToAsset(paramRef, eventCache);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(paramRef));
                            FMOD.Studio.PARAMETER_DESCRIPTION param;
                            eventDesc.getParameterByIndex(paramIndex, out param);
                            paramRef.Name = param.name;
                            paramRef.Min = param.minimum;
                            paramRef.Max = param.maximum;
                            paramRef.Default = param.defaultValue;
                            eventRef.Parameters.Add(paramRef);
                        }
                    }
                }

                bank.unload();
            }
            else
            {
                eventCache.StringsBankWriteTime = DateTime.MinValue;
            }
        }

        static void RemoveCacheBank(EditorBankRef bankRef)
        {
            eventCache.EditorEvents.ForEach((x) => x.Banks.Remove(bankRef));
        }


        static EventManager()
	    {
            countdownTimer = 1;
            EditorUserBuildSettings.activeBuildTargetChanged += BuildTargetChanged;
            EditorApplication.update += Update;
        }               

        public static void CopyToStreamingAssets()
        {
            FMODPlatform platform = EditorUtils.GetFMODPlatform();
            if (platform == FMODPlatform.None)
            {
                UnityEngine.Debug.LogWarning(String.Format("FMOD Studio: copy banks for platform {0} : Unsupported platform", EditorUserBuildSettings.activeBuildTarget.ToString()));
                return;
            }

            string bankTargetFolder = Application.dataPath + "/StreamingAssets";
            Directory.CreateDirectory(bankTargetFolder);

            string bankSourceFolder = EditorUtils.GetBankDirectory() + "/" + Settings.Instance.GetBankPlatform(platform);

            if (Path.GetFullPath(bankTargetFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant() ==
                Path.GetFullPath(bankSourceFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant())
            {
                return;
            }

            bool madeChanges = !true;

            try
            {

                // Clean out any stale .bank files
                string[] currentBankFiles = Directory.GetFiles(bankTargetFolder, "*.bank");
                foreach (var bankFileName in currentBankFiles)
                {
                    string bankName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(bankFileName));
                    if (!eventCache.EditorBanks.Exists((x) => bankName == x.Name))
                    {
                        File.Delete(bankFileName);
                        madeChanges = true;
                    }
                }

                // Copy over any files that don't match timestamp or size or don't exist
                foreach (var bankRef in eventCache.EditorBanks)
                {
                    string sourcePath = bankSourceFolder + "/" + bankRef.Name + ".bank";
                    string targetPath = bankTargetFolder + "/" + bankRef.Name + ".bank";

                    FileInfo sourceInfo = new FileInfo(sourcePath);
                    FileInfo targetInfo = new FileInfo(targetPath);

                    if (!targetInfo.Exists ||
                        sourceInfo.Length != targetInfo.Length ||
                        sourceInfo.LastWriteTime != targetInfo.LastWriteTime)
                    {
                        File.Copy(sourcePath, targetPath, true);

                        if (bankRef == eventCache.MasterBankRef)
                        {
                            sourcePath = bankSourceFolder + "/" + bankRef.Name + ".strings.bank";
                            targetPath = bankTargetFolder + "/" + bankRef.Name + ".strings.bank";
                            File.Copy(sourcePath, targetPath, true);
                        }

                        madeChanges = true;
                    }
                }
            }
            catch(Exception exception)
            {
                UnityEngine.Debug.LogError(String.Format("FMOD Studio: copy banks for platform {0} : copying banks from {1} to {2}", platform.ToString(), bankSourceFolder, bankTargetFolder));
                UnityEngine.Debug.LogException(exception);
                return;
            }            

            if (madeChanges)
            {
                UnityEngine.Debug.Log(String.Format("FMOD Studio: copy banks for platform {0} : copying banks from {1} to {2} succeeded", platform.ToString(), bankSourceFolder, bankTargetFolder));
            }
        }

        private static void BuildTargetChanged()
        {
            UpdateCache();

            // Copy over assets for the new platform
            CopyToStreamingAssets();
        }   

        static void OnCacheChange()
        {
            if (eventCache.MasterBankRef)
            {
                Settings.Instance.MasterBank = eventCache.MasterBankRef.Name;
            }
            else
            {
                Settings.Instance.MasterBank = null;
            }

            Settings.Instance.Banks.Clear();
            foreach (var bankRef in eventCache.EditorBanks)
            {
                if (bankRef != eventCache.MasterBankRef)
                {
                    Settings.Instance.Banks.Add(bankRef.Name);
                }
            }
            EditorUtility.SetDirty(Settings.Instance);
            EditorUtility.SetDirty(eventCache);
			
            CopyToStreamingAssets();

            EventBrowser.RepaintEventBrowser();
        }

        static bool firstUpdate = true;
        static float lastCheckTime;
        static void Update()
        {
            if (firstUpdate)
            {
                UpdateCache();
                CopyToStreamingAssets();
                bool isValid;
                string validateMessage;
                EditorUtils.ValidateSource(out isValid, out validateMessage);
                if (!isValid)
                {
                    Debug.LogError("FMOD Studio: " + validateMessage);
                }
                firstUpdate = false;
                lastCheckTime = Time.realtimeSinceStartup;
            }

            if (lastCheckTime + 1 < Time.realtimeSinceStartup)
            {
                UpdateCache();
                lastCheckTime = Time.realtimeSinceStartup;
            }
        }

        public static List<EditorEventRef> Events
        {
            get
            {
                UpdateCache();
                return eventCache.EditorEvents;
            }
        }

        public static List<EditorBankRef> Banks
        {
            get
            {
                UpdateCache();
                return eventCache.EditorBanks;
            }
        }

        public static EditorBankRef MasterBank
        { 
            get
            {
                UpdateCache();
                return eventCache.MasterBankRef;
            }
        }

        public static bool IsLoaded
        {
            get
            {
                return EditorUtils.GetBankDirectory() != null;
            }
        }

        public static bool IsValid
        {
            get
            {
                UpdateCache();
                return eventCache.StringsBankWriteTime != DateTime.MinValue;
            }
        }

        public static EditorEventRef EventFromPath(string path)
        {
            UpdateCache();
            return eventCache.EditorEvents.Find((x) => x.Path.Equals(path, StringComparison.CurrentCultureIgnoreCase));
        }

        public static EditorEventRef EventFromGUID(Guid guid)
        {
            UpdateCache();
            return eventCache.EditorEvents.Find((x) => x.Guid == guid);
        }
    }

}
