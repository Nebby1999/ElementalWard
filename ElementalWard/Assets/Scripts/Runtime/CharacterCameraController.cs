using Cinemachine;
using Nebula;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ElementalWard
{
    /// <summary>
    /// Takes care of properly updating the virtual camera's transform to match the virtual camera's rotation, which as a results allows us to get the proper aim direction.
    /// </summary>
    public class CharacterCameraController : MonoBehaviour
    {
        public Transform desiredCameraTransform;
        public CinemachineVirtualCamera VirtualCamera
        {
            get => _virtualCamera;
            set
            {
                _virtualCamera = value;
                UpdateVirtualCamera();
            }
        }
        private CinemachineVirtualCamera _virtualCamera;
        private CinemachineBrain _brain;
        private void UpdateVirtualCamera()
        {
            VirtualCamera.Follow = desiredCameraTransform.AsValidOrNull() ?? transform;
            _brain = CinemachineCore.Instance.FindPotentialTargetBrain(VirtualCamera);
        }

        private void Update()
        {
            if(_brain)
            {
                VirtualCamera.transform.localRotation = _brain.transform.localRotation;
            }
        }
    }
}
