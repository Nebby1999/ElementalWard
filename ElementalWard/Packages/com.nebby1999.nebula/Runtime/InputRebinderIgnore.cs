using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nebula
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class InputRebinderIgnore : InputProcessor
    {
        public override unsafe void Process(void* buffer, int bufferSize, InputControl control)
        {
        }

        public override object ProcessAsObject(object value, InputControl control)
        {
            return value;
        }

#if UNITY_EDITOR
        static InputRebinderIgnore()
        {
            Initialize();
        }
#endif

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            InputSystem.RegisterProcessor<InputRebinderIgnore>();
        }
    }
}
