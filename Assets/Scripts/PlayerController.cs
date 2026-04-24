using UnityEngine;
using UnityEngine.InputSystem;

namespace SapuJagad
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        private Vector2 moveInput;
        private Rigidbody2D rb;

        public float interactRadius = 1.5f;
        public LayerMask interactLayer;

        private float stunTimer = 0f;
        private float slowTimer = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                HandleInteraction();
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.isInteracting || GameManager.Instance.isGameOver))
            {
                moveInput = Vector2.zero;
                return;
            }

            if (stunTimer > 0)
            {
                stunTimer -= Time.deltaTime;
                moveInput = Vector2.zero;
            }

            if (slowTimer > 0)
            {
                slowTimer -= Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            float currentSpeed = moveSpeed;
            if (stunTimer > 0) currentSpeed = 0;
            else if (slowTimer > 0) currentSpeed *= 0.5f;

            rb.linearVelocity = moveInput * currentSpeed;
        }

        private void HandleInteraction()
        {
            if (GameManager.Instance != null && (GameManager.Instance.isInteracting || GameManager.Instance.isGameOver)) return;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);
            foreach (var col in colliders)
            {
                if (col.CompareTag("Trash"))
                {
                    Destroy(col.gameObject);
                    if (GameManager.Instance != null) GameManager.Instance.PickTrash();
                    return;
                }
                if (col.CompareTag("NPC"))
                {
                    NPCBehavior npc = col.GetComponent<NPCBehavior>();
                    if (npc != null) npc.Interact();
                    return;
                }
            }
        }

        public void ApplyStun(float duration) => stunTimer = duration;
        public void ApplySlow(float duration) => slowTimer = duration;
    }
}