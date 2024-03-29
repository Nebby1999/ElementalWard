﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Nebula.Editor
{
    public abstract class IMGUIObjectEditingWindow<TObject> : IMGUIEditorWindow where TObject : UnityEngine.Object
    {
        protected TObject TargetType { get; private set; }
        protected SerializedObject WindowSerializedObject { get; private set; }

        public static TEditorWindow OpenEditorWindow<TEditorWindow>(string windowName = null) where TEditorWindow : ExtendedEditorWindow
        {
            throw new NotImplementedException($"Do not use \"OpenEditorWindow<TEditorWindow>(string)\" for opening \"UIElementsObjectEditingWindow\", use \"OpenEditorWindow<TEditorWindow>(Object, string)\" instead");
        }

        public static TEditorWindow OpenEditorWindow<TEditorWindow>(Object obj, string windowName = null) where TEditorWindow : IMGUIObjectEditingWindow<TObject>
        {
            if (!obj)
            {
                throw new NullReferenceException();
            }

            TEditorWindow window = GetWindow<TEditorWindow>(windowName ?? ObjectNames.NicifyVariableName(typeof(TEditorWindow).Name));
            window.SerializedObject = new SerializedObject(obj);
            window.TargetType = window.SerializedObject.targetObject as TObject;
            window.WindowSerializedObject = new SerializedObject(window);
            return window;
        }
    }
}
