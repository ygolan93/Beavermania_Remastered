using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beavermania.Core.Input
{
    [CreateAssetMenu(menuName = "InputReader")]
    public class InputReader : ScriptableObject, BeaverInputSystem.IGameplayActions, BeaverInputSystem.IUIActions
    {
        private BeaverInputSystem _playerInput;

        private void OnEnable()
        {
            if (_playerInput == null)
            {
                _playerInput = new BeaverInputSystem();
                _playerInput.Gameplay.SetCallbacks(this);
                _playerInput.UI.SetCallbacks(this);

                SetGameplay();
            }
        }
        private void OnDisable()
        {
            _playerInput.Gameplay.SetCallbacks(this);
            _playerInput.UI.SetCallbacks(this);
        }
        public void SetGameplay()
        {
            _playerInput.Gameplay.Enable();
            _playerInput.UI.Disable();
        }

        public void SetUI()
        {
            _playerInput.Gameplay.Disable();
            _playerInput.UI.Enable();
        }


        public event Action<Vector2> MoveEvent;
        public event Action JumpEvent;
        public event Action JumpCancelledEvent;
        public event Action PauseEvent;
        public event Action ResumeEvent;
        public event Action<InputAction.CallbackContext> SprintEvent;
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                JumpEvent?.Invoke();
            }
            if (context.phase==InputActionPhase.Canceled)
            {
                JumpCancelledEvent?.Invoke();
            }
        }

        public void OnMouse(InputAction.CallbackContext context)
        {

        }

        public void OnMove(InputAction.CallbackContext context)
        {
            //Debug.Log($"Phase: {context.phase}, Value: {context.ReadValue<Vector2>()}");
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.phase== InputActionPhase.Performed)
            {
                PauseEvent?.Invoke();
                SetUI();
            }
        }

        public void OnResume(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
            {
                ResumeEvent?.Invoke();
                SetGameplay();
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            SprintEvent?.Invoke(context);
        }
    }
}

