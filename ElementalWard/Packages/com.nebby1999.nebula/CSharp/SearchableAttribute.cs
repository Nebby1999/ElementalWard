using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Nebula
{
    public class SearchableAttribute : Attribute
    {
        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
        public sealed class OptIn : Attribute
        {

        }

        private static Dictionary<Type, SearchableAttribute[]> typeToAttributes = new Dictionary<Type, SearchableAttribute[]>();

        private static void ScanAllAssemblies()
        {
            Assembly[] appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Assembly> optedInAssemblies = new List<Assembly>();
            foreach(Assembly assembly in appDomainAssemblies)
            {
                if(assembly.GetCustomAttribute<OptIn>() != null)
                {
                    optedInAssemblies.Add(assembly);
                }
            }

            foreach(Assembly optedInAssembly in optedInAssemblies)
            {

            }
        }

        static SearchableAttribute()
        {
            try
            {
                ScanAllAssemblies();
            }
            catch(Exception e)
            {
                Debug.LogError($"Error while scanning assemblies: {e}");
            }
        }
    }
}