using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ElementalWard.Editor
{
    [CustomEditor(typeof(ElementalWard.EntityStateMachine))]
    public class EntityStateMachineInspector : Nebula.Editor.Inspectors.EntityStateMachineInspectorBase<ElementalWard.EntityStateMachine>
    {

    }
}
