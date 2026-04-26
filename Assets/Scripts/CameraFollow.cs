using UnityEngine;

namespace SapuJagad
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float zoomValue = 5f;
        public float smoothSpeed = 0.125f;
        public Vector3 offset = new Vector3(0, 0, -10);

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            // Try to find player if not assigned
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Follow position
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Apply Zoom
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = zoomValue;
            }
        }
    }
}