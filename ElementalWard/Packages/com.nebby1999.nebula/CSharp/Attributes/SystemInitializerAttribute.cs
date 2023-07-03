using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace Nebula
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SystemInitializerAttribute : SearchableAttribute
    {
        public static bool HasExecuted { get; private set; }

        public readonly Type[] dependencies = Array.Empty<Type>();

        public MethodInfo MethodInfo => Target as MethodInfo;

        private Type associatedType;

        public SystemInitializerAttribute(params Type[] dependencies)
        {
            if (dependencies != null)
                this.dependencies = dependencies;
        }

        public static void Execute()
        {
            if (HasExecuted)
                return;
            HasExecuted = true;

            Queue<SystemInitializerAttribute> queue = new Queue<SystemInitializerAttribute>();
            List<SystemInitializerAttribute> attributes = new List<SystemInitializerAttribute>();
            if(!TryGetInstances(attributes))
            {
                return;
            }
            foreach(SystemInitializerAttribute attribute in attributes)
            {
                try
                {
                    AddToQueue(queue, attribute);
                }
                catch(Exception ex)
                {
                    Debug.LogError($"Failed to add {attribute} to queue.\n{ex}");
                }
            }

            ProcessQueue(queue);
        }

        private static void AddToQueue(Queue<SystemInitializerAttribute> queue,  SystemInitializerAttribute instance)
        {
            var methodInfo = instance.MethodInfo;
            if(methodInfo != null && methodInfo.IsStatic)
            {
                queue.Enqueue(instance);
                instance.associatedType = methodInfo.DeclaringType;
                return;
            }
            Debug.LogError($"Seachable Attribute cannot be added to queue as it's attached to a MemberInfo that's not a method. Member Info: {instance.Target.DeclaringType.FullName}.{instance.Target.Name}");
        }

        private static void ProcessQueue(Queue<SystemInitializerAttribute> queue)
        {
            HashSet<Type> initializedTypes = new HashSet<Type>();
            int num = 0;
            while(queue.Count > 0)
            {
                SystemInitializerAttribute instance = queue.Dequeue();
                if(!InitializerDependenciesMet(instance))
                {
                    queue.Enqueue(instance);
                    num++;
                    if(num >= queue.Count)
                    {
                        Debug.LogError($"SystemInitializerAttribute: Infinite Loop Detected. (Current Method:{instance.associatedType.FullName}.{instance.MethodInfo.Name}())");
                        Debug.LogError($"Initializer Dependencies:\n" + string.Join('\n', (IEnumerable<Type>)instance.dependencies));
                        Debug.LogError($"Initialized Types:\n" + string.Join('\n', initializedTypes));
                        break;
                    }
                    continue;
                }
                try
                {
                    instance.MethodInfo.Invoke(null, Array.Empty<object>());
                    initializedTypes.Add(instance.associatedType);
                }
                catch(Exception ex)
                {
                    Debug.LogError($"Error during execution of SystemInitializer attached to {instance.associatedType.FullName}.{instance.MethodInfo.Name}().\n{ex}");
                }
            }

            bool InitializerDependenciesMet(SystemInitializerAttribute instance)
            {
                Type[] dependencies = instance.dependencies;
                foreach(Type type in dependencies)
                {
                    if (!initializedTypes.Contains(type))
                        return false;
                }
                return true;
            }
        }
    }
}