using UnityEngine;
using Beavermania.Core.Input;
using UnityEngine.InputSystem;

namespace Beavermania.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader input;
        [SerializeField] private CharacterController characterController;

        [Header("Movement Settings")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        private bool isSprint = false;
        [SerializeField] private float rotationSpeed = 0.3f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float gravityMultiplier = 2.0f;
        [SerializeField] private float jumpBufferTime = 0.2f;
        [SerializeField] private float coyoteTime = 0.15f;

        private Vector2 _moveDirection;
        private Vector3 _velocity;
        private Quaternion _camDirection;
        private float _lastGroundedTime;
        private float _lastJumpTime;
        private Transform _mainCameraTransform;

        private void Start()
        {
            _mainCameraTransform = Camera.main.transform;
            SubscribeToInput();
        }

        private void OnEnable() => SubscribeToInput();
        private void OnDisable() => UnsubscribeFromInput();

        private void SubscribeToInput()
        {
            input.MoveEvent += HandleMove;
            input.JumpEvent += HandleJump;
            input.JumpCancelledEvent += HandleCancelledJump;
            input.SprintEvent += HandleSprint;
        }

        private void UnsubscribeFromInput()
        {
            input.MoveEvent -= HandleMove;
            input.JumpEvent -= HandleJump;
            input.JumpCancelledEvent -= HandleCancelledJump;
            input.SprintEvent -= HandleSprint;
        }

        private void Update()
        {
            UpdateTimers();
            ApplyGravity();
            HandleMovement();
        }

        private void UpdateTimers()
        {
            if (characterController.isGrounded)
            {
                _lastGroundedTime = Time.time;
                
                // Check for buffered jump when landing
                if (Time.time - _lastJumpTime <= jumpBufferTime)
                {
                    HandleJump();
                }
            }
        }

        private void ApplyGravity()
        {
            bool isFalling = _velocity.y <= 0f;
            float fallMultiplier = isFalling ? 1.5f : 1.0f;

            if (characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
                return;
            }

            _velocity.y += gravity * gravityMultiplier * fallMultiplier * Time.deltaTime;
            // Only apply vertical movement here
            characterController.Move(new Vector3(0, _velocity.y * Time.deltaTime, 0));
        }

        private void HandleMovement()
        {
            if (_moveDirection == Vector2.zero) return;

            // Get camera forward direction
            Vector3 camForward = _mainCameraTransform.forward;
            camForward.y = 0;
            _camDirection = Quaternion.LookRotation(camForward);

            // Calculate movement
            Vector3 inputVector = new Vector3(_moveDirection.x, 0, _moveDirection.y);
            Vector3 moveDirection = _camDirection * inputVector;
            moveDirection.y = _velocity.y; // Preserve vertical velocity for jumps

            // Apply rotation
            if (new Vector3(moveDirection.x, 0, moveDirection.z) != Vector3.zero)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                Quaternion rotGoal = Quaternion.Euler(0, targetAngle, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotGoal, rotationSpeed);
            }
            
            // Apply horizontal movement
            float currentSpeed = isSprint ? speed * sprintMultiplier : speed;
            Vector3 horizontalMove = new Vector3(moveDirection.x, 0, moveDirection.z) * (currentSpeed * Time.deltaTime);
            Vector3 verticalMove = new Vector3(0, moveDirection.y * Time.deltaTime, 0);
            
            characterController.Move(horizontalMove + verticalMove);
        }

        private void HandleMove(Vector2 dir) => _moveDirection = dir;

        private void HandleJump()
        {
            _lastJumpTime = Time.time;

            bool canJump = Time.time - _lastGroundedTime <= coyoteTime;
            bool jumpRequested = Time.time - _lastJumpTime <= jumpBufferTime;

            if (canJump && jumpRequested)
            {
                _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                _lastJumpTime = 0f; // Reset jump request
            }
        }

        private void HandleCancelledJump()
        {
            if (_velocity.y > 0)
            {
                _velocity.y *= 0.5f;
            }
        }

        private void HandleSprint(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                isSprint = true; // Start sprinting
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                isSprint = false; // Stop sprinting
            }
        }
    }
}
