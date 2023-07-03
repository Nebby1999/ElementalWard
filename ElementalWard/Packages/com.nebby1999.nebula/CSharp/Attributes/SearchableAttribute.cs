using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Nebula
{
    public abstract class SearchableAttribute : Attribute
    {
        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
        public sealed class OptInAttribute : Attribute
        {

        }

        private static Dictionary<Type, List<SearchableAttribute>> typeToAttributes = new Dictionary<Type, List<SearchableAttribute>>();
        private static HashSet<Assembly> alreadyScannedAssemblies = new HashSet<Assembly>();
        public MemberInfo Target { get; private set; }

        public static List<SearchableAttribute> GetInstances<TSearchableAttribute>() where TSearchableAttribute : SearchableAttribute
        {
            if(typeToAttributes.TryGetValue(typeof(TSearchableAttribute), out var list))
            {
                return list;
            }
            return null;
        }

        public static bool TryGetInstances<TSearchableAttribute>(List<TSearchableAttribute> dest) where TSearchableAttribute : SearchableAttribute
        {
            List<SearchableAttribute> attributes = GetInstances<TSearchableAttribute>();
            if (attributes == null)
                return false;

            foreach(SearchableAttribute att in attributes)
            {
                dest.Add((TSearchableAttribute)att);
            }
            return true;
        }
        private static void ScanAllAssemblies()
        {
            Assembly[] appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Assembly> optedInAssemblies = new List<Assembly>();
            foreach(Assembly assembly in appDomainAssemblies)
            {
                try
                {
                    if(assembly.GetCustomAttribute<OptInAttribute>() != null)
                    {
                        optedInAssemblies.Add(assembly);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }
            }

            foreach(Assembly optedInAssembly in optedInAssemblies)
            {
                try
                {
                    ScanAssembly(optedInAssembly);
                }
                catch(Exception e)
                {
                    Debug.LogError($"Error scanning assembly {optedInAssembly.FullName}.\n{e}");
                }
            }
        }

        private static void ScanAssembly(Assembly assembly)
        {
            if (alreadyScannedAssemblies.Contains(assembly))
                return;
            
            alreadyScannedAssemblies.Add(assembly);

            Type[] types = assembly.GetTypesSafe();
            foreach(Type t in types)
            {
                try
                {
                    ScanType(t);
                }
                catch(Exception e)
                {
                    Debug.LogError($"Error scanning type {t.AssemblyQualifiedName}.\n{e}");
                }
            }
        }

        private static void ScanType(Type type)
        {
            var attributesInType = type.GetCustomAttributes<SearchableAttribute>().ToArray();
            for(int i = 0; i < attributesInType.Length; i++)
            {
                var att = attributesInType[i];
                try
                {
                    RegisterAttribute(att, type);
                }
                catch(Exception e)
                {
                    Debug.LogError($"Error registering attribute of index {i} that's attached to {type.FullName}, which is from {type.AssemblyQualifiedName}.\n{e}");
                }
            }
            MemberInfo[] memberInfos = type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            foreach(MemberInfo info in memberInfos)
            {
                try
                {
                    SearchableAttribute[] attributes = info.GetCustomAttributes<SearchableAttribute>().ToArray();
                    for(int i = 0; i < attributes.Length; i++)
                    {
                        var attribute = attributes[i];
                        try
                        {
                            RegisterAttribute(attribute, info);
                        }
                        catch(Exception e)
                        {
                            Debug.LogError($"Error on initializing attribute of index {i} on memberinfo {type.FullName}.{info.Name} from assembly {type.Assembly.FullName}.\n{e}");
                        }
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"Error scanning MemberInfo {type.FullName}.{info.Name} from assembly {type.Assembly.FullName}.\n{e}");
                }
            }
        }

        private static void RegisterAttribute(SearchableAttribute attribute, MemberInfo target)
        {
            attribute.Target = target;
            Type attributeType = attribute.GetType();
            while(attributeType != null && typeof(SearchableAttribute).IsAssignableFrom(attributeType))
            {
                if(!typeToAttributes.ContainsKey(attributeType))
                {
                    typeToAttributes[attributeType] = new List<SearchableAttribute>();
                }
                typeToAttributes[attributeType].Add(attribute);
                attributeType = attributeType.BaseType;
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