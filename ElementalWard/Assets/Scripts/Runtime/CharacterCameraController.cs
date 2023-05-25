using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ElementalWard
{
    public class CharacterCameraController : MonoBehaviour
    {
        public Vector3 cameraPositionOffset;
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
            VirtualCamera.Follow = transform;
            var currentComponent = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if(currentComponent is not CinemachineHardLockToTarget)
            {
                VirtualCamera.AddCinemachineComponent<CinemachineHardLockToTarget>();
            }
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
