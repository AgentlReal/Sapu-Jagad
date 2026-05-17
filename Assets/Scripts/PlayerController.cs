using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

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

        [Header("Trash Picking")]
        public float pickingDuration = 3.0f;
        private bool isPickingTrash = false;
        public GameObject pickingCanvas;
        public Image pickingProgressBar;

        [Header("Status Text")]
        private TextMeshPro statusText;
        private float statusAlpha = 0f;
        private bool statusFadeIn = true;

        private bool wasMoving = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            anim = GetComponent<Animator>();

            if (pickingCanvas != null)
            {
                pickingCanvas.SetActive(false);
            }

            // Create floating status text as child
            CreateStatusText();
        }

        private void CreateStatusText()
        {
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = new Vector3(0, 1.2f, 0);

            statusText = textObj.AddComponent<TextMeshPro>();
            statusText.text = "";
            statusText.fontSize = 4f;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.sortingOrder = 100;
            statusText.enabled = false;

            // Make it render in front of sprites
            var renderer = statusText.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 100;
            }
        }

        private void Start()
        {
            // Apply level config if available
            if (GameManager.Instance != null && GameManager.Instance.currentLevelConfig != null)
            {
                pickingDuration = GameManager.Instance.currentLevelConfig.trashPickingDuration;
            }
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

            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            if (statusText == null) return;

            if (stunTimer > 0)
            {
                statusText.enabled = true;
                statusText.text = "Stunned";
                statusText.color = GetPulsingColor(new Color(1f, 0.2f, 0.2f)); // Red
            }
            else if (slowTimer > 0)
            {
                statusText.enabled = true;
                statusText.text = "Slowed";
                statusText.color = GetPulsingColor(new Color(1f, 0.9f, 0.2f)); // Yellow
            }
            else
            {
                statusText.enabled = false;
                statusText.text = "";
                statusAlpha = 0f;
                statusFadeIn = true;
            }
        }

        private Color GetPulsingColor(Color baseColor)
        {
            // Pulse alpha between 0.3 and 1.0
            float pulseSpeed = 3f;
            if (statusFadeIn)
            {
                statusAlpha += Time.deltaTime * pulseSpeed;
                if (statusAlpha >= 1f)
                {
                    statusAlpha = 1f;
                    statusFadeIn = false;
                }
            }
            else
            {
                statusAlpha -= Time.deltaTime * pulseSpeed;
                if (statusAlpha <= 0.3f)
                {
                    statusAlpha = 0.3f;
                    statusFadeIn = true;
                }
            }

            baseColor.a = statusAlpha;
            return baseColor;
        }

        private void FixedUpdate()
        {
            // Block movement if interacting, game over, or picking trash
            if (GameManager.Instance != null && (GameManager.Instance.isInteracting || GameManager.Instance.isGameOver) || isPickingTrash)
            {
                rb.linearVelocity = Vector2.zero;
                UpdateAnimations(Vector2.zero);
                HandleFootstepSFX(false);
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

            bool isMoving = moveInput.magnitude > 0.01f && currentSpeed > 0;
            HandleFootstepSFX(isMoving);
        }

        private void HandleFootstepSFX(bool isMoving)
        {
            if (SFXManager.Instance == null) return;

            if (isMoving && !wasMoving)
            {
                SFXManager.Instance.PlayFootstep();
            }
            else if (!isMoving && wasMoving)
            {
                SFXManager.Instance.StopFootstep();
            }
            wasMoving = isMoving;
        }

        private void UpdateAnimations(Vector2 move)
        {
            if (anim == null) return;
            
            float speed = move.magnitude;
            anim.SetFloat("Speed", speed);
            
            if (speed > 0.01f)
            {
                anim.SetFloat("Horizontal", move.x);
                anim.SetFloat("Vertical", move.y);
            }
        }

        private void HandleInteraction()
        {
            if (GameManager.Instance != null && (GameManager.Instance.isInteracting || GameManager.Instance.isGameOver) || isPickingTrash) 
            {
                Debug.Log("Interaction blocked by state");
                return;
            }

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);
            Debug.Log("Interaction Check. Found: " + colliders.Length);

            foreach (var col in colliders)
            {
                if (col.gameObject == gameObject) continue;

                if (col.CompareTag("Trash"))
                {
                    StartCoroutine(PickTrashCoroutine(col.gameObject));
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

        private IEnumerator PickTrashCoroutine(GameObject trashObject)
        {
            if (trashObject == null) yield break;

            isPickingTrash = true;
            
            if (pickingCanvas != null) pickingCanvas.SetActive(true);
            if (pickingProgressBar != null) pickingProgressBar.fillAmount = 0f;

            // Start picking SFX
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlayPickingProgress();

            float timer = 0f;
            while (timer < pickingDuration)
            {
                // If trash gets destroyed by something else during picking
                if (trashObject == null)
                {
                    CancelPicking();
                    yield break;
                }

                timer += Time.deltaTime;
                if (pickingProgressBar != null)
                {
                    pickingProgressBar.fillAmount = timer / pickingDuration;
                }
                yield return null;
            }

            // Finished picking
            if (trashObject != null)
            {
                Destroy(trashObject);
                if (GameManager.Instance != null) GameManager.Instance.PickTrash();
                Debug.Log("Trash Picked!");

                // Play success SFX
                if (SFXManager.Instance != null)
                    SFXManager.Instance.PlayTrashPicked();
            }

            CancelPicking();
        }

        private void CancelPicking()
        {
            isPickingTrash = false;
            if (pickingCanvas != null) pickingCanvas.SetActive(false);

            // Stop picking SFX
            if (SFXManager.Instance != null)
                SFXManager.Instance.StopPickingProgress();
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