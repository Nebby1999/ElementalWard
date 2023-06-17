using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard
{
    public interface IGravityProvider
    {
        public Vector3 GravityDirection { get; }
    }
}
