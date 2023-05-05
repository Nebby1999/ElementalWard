using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nebula
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute
    {

    }
}
