using UnityEngine;

namespace SapuJagad
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float zoomValue = 5f;
        public float smoothSpeed = 0.125f;
        public Vector3 offset = new Vector3(0, 0, -10);

        [Header("Map Bounds")]
        public Vector2 mapMin = new Vector2(-17.5f, -17.5f);
        public Vector2 mapMax = new Vector2(17.5f, 17.5f);

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

            // Apply Zoom
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = zoomValue;
            }

            // Follow position
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Clamp camera to map bounds
            if (cam != null && cam.orthographic)
            {
                float camHalfHeight = cam.orthographicSize;
                float camHalfWidth = camHalfHeight * cam.aspect;

                float clampedX = Mathf.Clamp(smoothedPosition.x, mapMin.x + camHalfWidth, mapMax.x - camHalfWidth);
                float clampedY = Mathf.Clamp(smoothedPosition.y, mapMin.y + camHalfHeight, mapMax.y - camHalfHeight);

                smoothedPosition = new Vector3(clampedX, clampedY, smoothedPosition.z);
            }

            transform.position = smoothedPosition;
        }

        /// <summary>
        /// Updates the camera bounds from a LevelConfig.
        /// Called by GameManager when a level loads.
        /// </summary>
        public void SetBounds(Vector2 min, Vector2 max)
        {
            mapMin = min;
            mapMax = max;
        }
    }
}