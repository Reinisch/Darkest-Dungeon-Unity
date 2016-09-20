using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FMODUnity
{
    class EventCache : ScriptableObject
    {
        public static int CurrentCacheVersion = 1;

        [SerializeField]
        public List<EditorBankRef> EditorBanks;
        [SerializeField]
        public List<EditorEventRef> EditorEvents;
        [SerializeField]
        public EditorBankRef MasterBankRef;
        [SerializeField]
        Int64 stringsBankWriteTime;
        [SerializeField]
        public int cacheVersion;

        public DateTime StringsBankWriteTime
        {
            get { return new DateTime(stringsBankWriteTime); }
            set { stringsBankWriteTime = value.Ticks; }
        }

        public EventCache()
        {
            EditorBanks = new List<EditorBankRef>();
            EditorEvents = new List<EditorEventRef>();
            MasterBankRef = null;
            stringsBankWriteTime = 0;
        }
    }
}
