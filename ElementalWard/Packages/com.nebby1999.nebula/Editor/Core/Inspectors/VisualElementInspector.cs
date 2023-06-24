using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UObject = UnityEngine.Object;

namespace Nebula.Editor
{
    using static Nebula.Editor.TemplateHelpers;

    public struct ContextMenuData
    {
        public string menuName;

        public Action<DropdownMenuAction> menuAction;

        public Func<DropdownMenuAction, DropdownMenuAction.Status> actionStatusCheck;


        public ContextMenuData(string name, Action<DropdownMenuAction> action)
        {
            menuName = name;
            menuAction = action;
            actionStatusCheck = x => DropdownMenuAction.Status.Normal;
        }

        public ContextMenuData(string name, Action<DropdownMenuAction> action, Func<DropdownMenuAction, DropdownMenuAction.Status> statusCheck)
        {
            menuName = name;
            menuAction = action;
            actionStatusCheck = statusCheck;
        }
    }
}
