using UnityEngine;
using UnityEngine.InputSystem;

namespace LabTesting
{
    [DisallowMultipleComponent]
    [AddComponentMenu("LabTesting/Player Controller")]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private float verticalVelocity;

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        public Vector3 Velocity { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (controller == null || !controller.enabled)
            {
                return;
            }

            ApplyGravity();
            Move(ReadMoveInput());
            controller.Move(Velocity * Time.deltaTime);
        }

        public void Move(Vector2 input)
        {
            Vector2 clamped = Vector2.ClampMagnitude(input, 1f);
            Vector3 forward = ProjectOnPlane(cameraTransform != null ? cameraTransform.forward : Vector3.forward);
            Vector3 right = ProjectOnPlane(cameraTransform != null ? cameraTransform.right : Vector3.right);
            Vector3 horizontal = (forward * clamped.y + right * clamped.x) * moveSpeed;
            Velocity = new Vector3(horizontal.x, verticalVelocity, horizontal.z);
        }

        public void TakeDamage(Health health, int amount)
        {
            if (health != null)
            {
                health.Damage(amount);
            }
        }

        private void ApplyGravity()
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -3f;
                return;
            }

            verticalVelocity += gravity * Time.deltaTime;
        }

        private static Vector3 ProjectOnPlane(Vector3 direction)
        {
            Vector3 projected = Vector3.ProjectOnPlane(direction, Vector3.up);
            return projected.sqrMagnitude < 0.0001f ? Vector3.forward : projected.normalized;
        }

        private static Vector2 ReadMoveInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float horizontal = Axis(keyboard.dKey.isPressed, keyboard.aKey.isPressed);
            float vertical = Axis(keyboard.wKey.isPressed, keyboard.sKey.isPressed);
            return new Vector2(horizontal, vertical);
        }

        private static float Axis(bool positive, bool negative) => (positive ? 1f : 0f) - (negative ? 1f : 0f);
    }
}
