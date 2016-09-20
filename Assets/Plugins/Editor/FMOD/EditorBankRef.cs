using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
namespace FMODUnity
{
    public class EditorBankRef : ScriptableObject
    {
        [Serializable]
        public class NameValuePair
        {
            public string Name;
            public long Value;

            public NameValuePair(string name, long value)
            {
                Name = name;
                Value = value;
            }
        }

        [SerializeField]
        public string Path;
        public string Name
        {
            get { return global::System.IO.Path.GetFileNameWithoutExtension(Path); }
        }

        [SerializeField]
        Int64 lastModified;
        public DateTime LastModified
        {
            get { return new DateTime(lastModified); }
            set { lastModified = value.Ticks; }
        }
        
        [SerializeField]
        public FMOD.RESULT LoadResult;

        [SerializeField]        
        public List<NameValuePair> FileSizes;

        public bool Exists;
    }
}
