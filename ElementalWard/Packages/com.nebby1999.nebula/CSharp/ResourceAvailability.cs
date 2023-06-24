using System;
using System.Diagnostics;

namespace Nebula
{
    public struct ResourceAvailability
    {
        private event Action _Delegates;

        public bool Available { get; private set; }

        private readonly Type _ownerType;
        public void MakeAvailable(Type callingType)
        {
            if (callingType == _ownerType)
            {
                Available = true;
                var invocationList = _Delegates?.GetInvocationList() ?? Array.Empty<Delegate>();
                if (invocationList.Length == 0)
                    return;

                foreach (Action @delegate in invocationList)
                {
                    try
                    {
                        @delegate.Invoke();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError(ex);
                    }
                }
                _Delegates = null;
                return;
            }
            throw new InvalidOperationException("Only the owner of a ResourceAvailabilyt can make it available.");
        }

        public void CallWhenAvailable(Action action)
        {
            if (Available)
            {
                action();
                return;
            }
            _Delegates += action;
        }
        public ResourceAvailability(Type ownerType)
        {
            _Delegates = default;
            Available = false;
            _ownerType = ownerType;
        }
    }
}