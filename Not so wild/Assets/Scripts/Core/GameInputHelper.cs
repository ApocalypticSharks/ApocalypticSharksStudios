using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NotSoWild.Core
{
    public static class GameInputHelper
    {
        public static Vector2 MousePosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (Mouse.current != null)
                {
                    return Mouse.current.position.ReadValue();
                }

                return Vector2.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.mousePosition;
#else
                return Vector2.zero;
#endif
            }
        }

        public static bool WasLeftMousePressedThisFrame
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (Mouse.current != null)
                {
                    return Mouse.current.leftButton.wasPressedThisFrame;
                }

                return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetMouseButtonDown(0);
#else
                return false;
#endif
            }
        }

        public static bool WasEscapePressedThisFrame
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    return Keyboard.current.escapeKey.wasPressedThisFrame;
                }

                return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.Escape);
#else
                return false;
#endif
            }
        }
    }
}
