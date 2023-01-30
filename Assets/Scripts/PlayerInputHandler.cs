using UnityEngine;
using KinematicCharacterController.Examples;

/* This file handles all inputs and delegates the handling of these inputs into the appropriate components
 * Current sends the inputs to:
 *  - PlayerCharacterController
 *  - Camera
 */

namespace KinematicCharacterController.Walkthrough.PlayerCameraCharacterSetup
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public MainCamera mainCamera;
        public Transform CameraFollowPoint;
        public PlayerCharacterController Character;

        private const string MouseXInput = "Mouse X";
        private const string MouseYInput = "Mouse Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";

        private void Start()
        {
            // Tell camera to always be at this specified position
            mainCamera.SetFollowTransform(CameraFollowPoint);

            // Ignore the character's collider(s) for camera obstruction checks
            mainCamera.IgnoredColliders.Clear();
            mainCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());
        }

        private void Update()
        {
            HandleCharacterInput();
        }

        private void LateUpdate()
        {
            HandleCameraInput();
        }

        private void HandleCameraInput()
        {
            // Input for zooming the camera (disabled in WebGL because it can cause problems)
            float scrollInput = -Input.GetAxis(MouseScrollInput);
#if UNITY_WEBGL
        scrollInput = 0f;
#endif

            // Apply inputs to the camera
            mainCamera.UpdateWithInput(Time.deltaTime, scrollInput);

            // Handle toggling zoom level
            if (Input.GetMouseButtonDown(1))
            {
                mainCamera.TargetDistance = (mainCamera.TargetDistance == 0f) ? mainCamera.DefaultDistance : 0f;
            }
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            // Build the CharacterInputs struct
            characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
            characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);
            characterInputs.CameraRotation = mainCamera.Transform.rotation;

            // Apply inputs to character
            Character.SetInputs(ref characterInputs);
        }
    }
}