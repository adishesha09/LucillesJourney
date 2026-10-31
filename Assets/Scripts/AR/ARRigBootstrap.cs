#if LJ_AR
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using LucillesJourney.World;

namespace LucillesJourney.AR
{
    /// <summary>
    /// Builds the whole AR rig in code when a gameplay scene loads in AR mode - nothing has to be added to any scene,
    /// and rebuilding the scenes can never remove it. The main menu (no game camera) is never touched.
    ///
    /// Safety net: the AR session is started first. If the device cannot run AR (no ARCore/ARKit, install declined,
    /// no camera permission, timeout) the rig is thrown away before the game camera is touched and the game simply
    /// plays in normal flat mode.
    /// </summary>
    public class ARRigBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!ARMode.Requested) return;
            if (FindFirstObjectByType<ARRigBootstrap>() != null || FindFirstObjectByType<ARTabletop>() != null) return;
            new GameObject("ARRigBootstrap").AddComponent<ARRigBootstrap>();
        }

        private IEnumerator Start()
        {
            // The game camera may be created a few frames into the scene; the menu scene never has one.
            CameraFollow2D follow = null;
            for (int i = 0; i < 120 && follow == null; i++)
            {
                follow = FindFirstObjectByType<CameraFollow2D>();
                if (follow == null) yield return null;
            }
            if (follow == null) { Destroy(gameObject); yield break; }

            // 1. Build the origin first (the XR Simulation loader and some devices need an XROrigin to exist when the
            //    session starts). Its camera stays off for now, so the flat game keeps showing during the check.
            var origin = BuildOrigin(out Camera arCamera);

            var session = new GameObject("AR Session");
            session.AddComponent<ARSession>();
            session.AddComponent<ARInputManager>();

            float waited = 0f;
            while (waited < 40f && IsStillStarting(ARSession.state))
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            if (ARSession.state < ARSessionState.Ready)
            {
                ARMode.FallbackReason = $"AR is not available on this device ({ARSession.state}). Playing in normal mode.";
                Debug.LogWarning(ARMode.FallbackReason);
                Destroy(session);
                Destroy(origin);
                ARMode.Requested = false;
                Destroy(gameObject);
                yield break;
            }

            // 2. AR works: switch the AR camera on and hand over to the tabletop.
            arCamera.enabled = true;
            new GameObject("ARTabletop").AddComponent<ARTabletop>();
            Destroy(gameObject);
        }

        private static bool IsStillStarting(ARSessionState state) =>
            state == ARSessionState.None || state == ARSessionState.CheckingAvailability ||
            state == ARSessionState.NeedsInstall || state == ARSessionState.Installing;

        private static GameObject BuildOrigin(out Camera camera)
        {
            // Built inactive so every reference is in place before XR Origin switches on.
            var originObject = new GameObject("XR Origin (AR)");
            originObject.SetActive(false);

            var offset = new GameObject("Camera Offset").transform;
            offset.SetParent(originObject.transform, false);

            var cameraObject = new GameObject("AR Camera"); // ARTabletop tags it MainCamera once AR is confirmed
            cameraObject.transform.SetParent(offset, false);
            camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 30f;
            cameraObject.AddComponent<ARCameraManager>();
            cameraObject.AddComponent<ARCameraBackground>();

            var driver = cameraObject.AddComponent<TrackedPoseDriver>();
            driver.positionInput = new InputActionProperty(new InputAction("Position", InputActionType.Value, "<XRHMD>/centerEyePosition", expectedControlType: "Vector3"));
            driver.rotationInput = new InputActionProperty(new InputAction("Rotation", InputActionType.Value, "<XRHMD>/centerEyeRotation", expectedControlType: "Quaternion"));

            var origin = originObject.AddComponent<XROrigin>();
            origin.Camera = camera;
            origin.CameraFloorOffsetObject = offset.gameObject;
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;

            originObject.AddComponent<ARPlaneManager>();
            originObject.AddComponent<ARRaycastManager>();
            originObject.SetActive(true);
            return originObject;
        }
    }
}
#endif