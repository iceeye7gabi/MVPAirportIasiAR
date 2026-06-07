using UnityEngine;

namespace AirportAR.AR
{
    /// <summary>
    /// Keeps a world-space label facing the active AR / main camera.
    /// </summary>
    public class FaceCamera : MonoBehaviour
    {
        Camera targetCamera;

        void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                return;
            }

            Vector3 direction = transform.position - targetCamera.transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }
    }
}
