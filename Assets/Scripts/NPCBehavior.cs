using UnityEngine;
using System.Collections.Generic;

public class NPCBehavior : MonoBehaviour
{
    public NPCData data;
    private bool interacted = false;
    private float trashTimer = 0f;
    private float _movePhase = 0f;
    private const float MOVE_FREQUENCY = 0.5f;

    private static GameObject _trashPrefab;
    private SpriteRenderer sr;
    private Animator anim;
    private Spawner _spawner;
    private Rigidbody2D rb;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure NPC is on the NPC layer
        gameObject.layer = LayerMask.NameToLayer("NPC");

        // Force Rigidbody settings for reliable collisions
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Start()
    {
        var spawners = GameObject.FindObjectsByType<Spawner>(FindObjectsSortMode.None);
        if (spawners.Length > 0) _spawner = spawners[0];

        if (data != null)
        {
            ApplyNPCData();
            trashTimer = data.trashDropRate;
        }

        if (_trashPrefab == null && _spawner != null)
        {
            _trashPrefab = _spawner.trashPrefab;
        }

        // Randomize initial phase
        _movePhase = Random.Range(0f, Mathf.PI * 2f);
    }

    public void ApplyNPCData()
    {
        if (data == null) return;
        
        if (sr != null) sr.sprite = data.sprite;
        
        if (anim != null && data.animatorController != null)
        {
            anim.runtimeAnimatorController = data.animatorController;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.isInteracting || GameManager.Instance.isGameOver || interacted)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (anim != null) anim.SetFloat("Horizontal", 0);
            return;
        }

        // 1. Advance Phase
        _movePhase += Time.fixedDeltaTime * MOVE_FREQUENCY;

        // 2. Move via Velocity (Physical)
        float moveDir = Mathf.Sin(_movePhase);
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveDir * data.moveSpeed, 0);
        }

        if (anim != null)
        {
            anim.SetFloat("Horizontal", moveDir);
        }

        // 3. Handle Trash
        trashTimer -= Time.fixedDeltaTime;
        if (trashTimer <= 0)
        {
            DropTrash();
            trashTimer = data.trashDropRate;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Bounce on Wall hit
        if (collision.gameObject.layer == LayerMask.NameToLayer("NPCWall"))
        {
            _movePhase += Mathf.PI;
            Debug.Log(data.npcName + " bounced off " + collision.gameObject.name);
        }
    }

    private void DropTrash()
    {
        if (_trashPrefab != null)
        {
            GameObject trash = Instantiate(_trashPrefab, transform.position, Quaternion.identity);
            // Assign random sprite variant
            if (Spawner.Instance != null)
            {
                Spawner.Instance.AssignRandomTrashSprite(trash);
            }
        }
    }

    public void Interact()
    {
        if (interacted || data == null || data.dialogueOptions == null || data.dialogueOptions.Count == 0) return;

        TeguranData selected = data.dialogueOptions[Random.Range(0, data.dialogueOptions.Count)];
        UIManager.Instance.StartMiniGame(selected, this);
        
        interacted = true; 
    }

    public void OnInteractionEnd(bool success)
    {
        if (!success)
        {
            var player = GameObject.Find("Player")?.GetComponent<SapuJagad.PlayerController>();
            if (player != null) ApplyPenalty(player);
        }

        if (_spawner != null) _spawner.OnNPCRemoved(gameObject);
        Destroy(gameObject);
    }

    private void ApplyPenalty(SapuJagad.PlayerController player)
    {
        if (player == null) return;
        
        switch (data.type)
        {
            case NPCType.IbuIbu:
                player.ApplyStun(7f);
                break;
            case NPCType.AnakKecil:
                for(int i=0; i<5; i++) {
                    Vector3 offset = Random.insideUnitCircle * 2f;
                    GameObject trash = Instantiate(_trashPrefab, player.transform.position + offset, Quaternion.identity);
                    // Assign random sprite variant to penalty trash
                    if (Spawner.Instance != null)
                    {
                        Spawner.Instance.AssignRandomTrashSprite(trash);
                    }
                }
                break;
            case NPCType.OknumOrmas:
                player.ApplySlow(10f);
                break;
        }
    }
}