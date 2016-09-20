using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FMODUnity
{
    public class EditorParamRef : ScriptableObject
    {
        [SerializeField]
        public string Name;
        [SerializeField]
        public float Min;
        [SerializeField]
        public float Max;
        [SerializeField]
        public float Default;
    }
}
