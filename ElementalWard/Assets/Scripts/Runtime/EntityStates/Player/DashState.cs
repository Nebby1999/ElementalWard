using UnityEngine;
using ElementalWard;

namespace EntityStates.Player
{
    public class DashState : BaseGroundedCharacterMain
    {
        public static float duration;
        public static float baseDashSpeed;
        public static float maxDashSpeedCoefficient;
        public static Vector3 onAirMaxSpeed;
        public static float onAirDashSpeedCoefficient;


        private Vector3 _forwardDirection;
        private float _dashSpeed;
        private float _currentXVelocity;
        private float _currentZVelocity;
        public override void OnEnter()
        {
            base.OnEnter();

            if(HasCharacterInputBank)
            {
                var quaternion = Quaternion.Euler(0, CharacterInputBank.LookRotation.eulerAngles.y, 0);
                _forwardDirection = quaternion * CharacterInputBank.moveVector;
            }
            else
            {
                _forwardDirection = Transform.forward;
            }
            _forwardDirection = _forwardDirection.normalized;

            CalculateDashSpeed();

            if(GroundedCharacterMovementController)
            {
                var controllerVelocity = GroundedCharacterMovementController.characterVelocity;
                GroundedCharacterMovementController.Motor.ForceUnground();
                Vector3 newVelocityXZ = _forwardDirection * _dashSpeed;
                GroundedCharacterMovementController.characterVelocity = new Vector3(newVelocityXZ.x, controllerVelocity.y, newVelocityXZ.z);
            }
        }

        private void CalculateDashSpeed()
        {
            var baseDashSpeed = movementSpeedStat;
            var baseSprintingSpeed = CharacterBody.MovementSpeed * CharacterBody.SprintSpeedMultiplier;
            var maxDashSpeed = baseSprintingSpeed * maxDashSpeedCoefficient;

            if(baseDashSpeed > maxDashSpeed)
            {
                baseDashSpeed = maxDashSpeed;
            }

            var speed = baseDashSpeed * DashState.baseDashSpeed;
            _dashSpeed = IsGrounded ? speed : speed * onAirDashSpeedCoefficient;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!IsGrounded && HasGroundedCharacterMovementController)
            {
                Vector3 currentVelocity = GroundedCharacterMovementController.characterVelocity;
                bool xNegative = currentVelocity.x < 0;
                bool zNegative = currentVelocity.z < 0;
                float x = Mathf.Abs(currentVelocity.x);
                float z = Mathf.Abs(currentVelocity.z);
                if (x > onAirMaxSpeed.x)
                {
                    float newX = Mathf.SmoothDamp(x, onAirMaxSpeed.x, ref _currentXVelocity, 0.1f);
                    currentVelocity.x = xNegative ? -newX : newX;
                }
                if (z > onAirMaxSpeed.z)
                {
                    float newZ = Mathf.SmoothDamp(z, onAirMaxSpeed.z, ref _currentZVelocity, 0.1f);
                    currentVelocity.z = zNegative ? -newZ : newZ;
                }
                GroundedCharacterMovementController.characterVelocity = currentVelocity;
            }
            if (FixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}