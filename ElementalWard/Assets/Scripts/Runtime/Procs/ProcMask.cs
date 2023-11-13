using Nebula;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElementalWard
{
    public struct ProcMask
    {
        private HashSet<ProcType> _mask;

        public bool HasProc(ProcAsset procAsset)
        {
            if (procAsset.ProcType == ProcType.None)
                return false;

            return HasProc(procAsset.ProcType);
        }
        public bool HasProc(ProcType procType)
        {
            EnsureHashSet();
            return _mask?.Contains(procType) ?? false;
        }

        public void AddProc(ProcAsset procAsset)
        {
            if (procAsset.ProcType == ProcType.None)
                return;

            AddProc(procAsset.ProcType);
        }

        public void AddProc(ProcType procType)
        {
            EnsureHashSet();
            if (procType == ProcType.None || _mask.Contains(procType))
                return;

            if (_mask.Contains(ProcType.None))
                _mask.Remove(ProcType.None);

            _mask.Add(procType);
        }

        public void RemoveProc(ProcAsset procAsset)
        {
            if (procAsset.ProcType == ProcType.None)
                return;

            RemoveProc(procAsset.ProcType);
        }

        public void RemoveProc(ProcType procType)
        {
            EnsureHashSet();
            _mask.Remove(procType);

            if(_mask.Count == 0)
                _mask.Add(ProcType.None);
        }

        private void EnsureHashSet()
        {
            _mask ??= new HashSet<ProcType>();
        }

        public ProcMask(ProcMask other)
        {
            _mask = new HashSet<ProcType>(other._mask.AsEnumerable());
        }
    }
}
