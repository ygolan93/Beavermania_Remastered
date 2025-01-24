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
        [SerializeField] Animator _animatior;

        [Header("Movement Settings")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float rotationSpeed = 0.3f;
        private bool isSprint = false;
        private float currentSpeed;

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

        // Add these cached values at class level
        private static readonly Vector3 ZeroVector = Vector3.zero;
        private static readonly Vector3 VerticalOffset = new Vector3(0, -2f, 0);

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

        private void HandleAnimations()
        {
            // Check if there is movement input
            if (_moveDirection == Vector2.zero)
            {
                currentSpeed = 0; // Reset current speed if no input
                _animatior.SetFloat("Move", currentSpeed); // Reset animator float to 0
            }
            else
            {
                // Calculate current speed based on whether the player is sprinting or walking
                currentSpeed = isSprint ? speed * sprintMultiplier : speed; // Adjust speed based on sprinting
                _animatior.SetFloat("Move", currentSpeed); // Update animator float
            }

            // Update other animation parameters
            _animatior.SetFloat("Yvelocity", _velocity.y);
            _animatior.SetBool("Jump", !characterController.isGrounded);
        }

        private void Update()
        {
            UpdateTimers();
            ApplyGravity();
            HandleMovement();
            HandleAnimations();
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
            characterController.Move(Vector3.up * (_velocity.y * Time.deltaTime));
        }

        private void HandleMovement()
        {
            if (_moveDirection == Vector2.zero) return;

            // Get camera forward direction - optimize by doing this calculation only when needed
            Vector3 camForward = _mainCameraTransform.forward;
            camForward.y = 0;
            camForward.Normalize(); // Normalize the forward vector for consistent movement

            // Calculate movement in relation to the camera's forward vector
            Vector3 moveDirection = camForward * _moveDirection.y + _mainCameraTransform.right * _moveDirection.x;
            moveDirection.y = _velocity.y;

            // Only calculate rotation if actually moving
            Vector3 horizontalMove = new Vector3(moveDirection.x, 0, moveDirection.z);
            if (horizontalMove != ZeroVector)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Slerp(transform.rotation, 
                                                    Quaternion.Euler(0, targetAngle, 0), 
                                                    rotationSpeed);
            }
            
            // Optimize movement calculation
            currentSpeed = isSprint ? speed * sprintMultiplier : speed;
            horizontalMove = Vector3.Scale(horizontalMove.normalized, new Vector3(currentSpeed, 0, currentSpeed)) * Time.deltaTime;
            characterController.Move(horizontalMove + Vector3.up * (moveDirection.y * Time.deltaTime));
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
