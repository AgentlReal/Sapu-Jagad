using UnityEngine;
using UnityEngine.InputSystem;

namespace SapuJagad
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        private Vector2 moveInput;
        private Rigidbody2D rb;
        private Animator anim;

        public float interactRadius = 2.0f;
        public LayerMask interactLayer;

        private float stunTimer = 0f;
        private float slowTimer = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            anim = GetComponent<Animator>();
        }

        // Using SendMessages behavior (OnMove, OnInteract)
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log("OnInteract Message Received");
                HandleInteraction();
            }
        }

        private void Update()
        {
            if (stunTimer > 0)
                stunTimer -= Time.deltaTime;

            if (slowTimer > 0)
                slowTimer -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            // Block movement if interacting or game over
            if (GameManager.Instance != null && (GameManager.Instance.isInteracting || GameManager.Instance.isGameOver))
            {
                rb.linearVelocity = Vector2.zero;
                UpdateAnimations(Vector2.zero);
                return;
            }

            float currentSpeed = moveSpeed;
            if (stunTimer > 0) 
            {
                currentSpeed = 0;
            }
            else if (slowTimer > 0) 
            {
                currentSpeed *= 0.5f;
            }

            rb.linearVelocity = moveInput * currentSpeed;
            UpdateAnimations(moveInput);
        }

        private void UpdateAnimations(Vector2 move)
        {
            if (anim == null) return;
            
            float speed = move.magnitude;
            anim.SetFloat("Speed", speed);
            
            if (speed > 0.01f)
            {
                anim.SetFloat("Horizontal", move.x);
            }
        }

        private void HandleInteraction()
        {
            if (GameManager.Instance != null && (GameManager.Instance.isInteracting || GameManager.Instance.isGameOver)) 
            {
                Debug.Log("Interaction blocked by GameManager state");
                return;
            }

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);
            Debug.Log("Interaction Check. Found: " + colliders.Length);

            foreach (var col in colliders)
            {
                if (col.gameObject == gameObject) continue;

                if (col.CompareTag("Trash"))
                {
                    Destroy(col.gameObject);
                    if (GameManager.Instance != null) GameManager.Instance.PickTrash();
                    Debug.Log("Trash Picked!");
                    return;
                }
                
                if (col.CompareTag("NPC"))
                {
                    NPCBehavior npc = col.GetComponent<NPCBehavior>();
                    if (npc != null) 
                    {
                        npc.Interact();
                    }
                    return;
                }
            }
        }

        public void ApplyStun(float duration) => stunTimer = duration;
        public void ApplySlow(float duration) => slowTimer = duration;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}