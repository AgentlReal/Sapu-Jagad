using UnityEngine;
using System.Collections.Generic;

public class NPCBehavior : MonoBehaviour
{
    public NPCData data;
    private bool interacted = false;
    private float trashTimer = 0f;

    private static GameObject _trashPrefab;
    private SpriteRenderer sr;
    private Animator anim;
    private Spawner _spawner;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
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

    private void Update()
    {
        // 1. Check Game State
        if (GameManager.Instance == null || GameManager.Instance.isInteracting || GameManager.Instance.isGameOver || interacted)
        {
            if (anim != null) anim.SetFloat("Horizontal", 0);
            return;
        }

        // 2. Handle Movement
        float moveDirRaw = Mathf.Sin(Time.time * 0.5f);
        float moveAmount = data.moveSpeed * Time.deltaTime * moveDirRaw;
        transform.Translate(Vector2.right * moveAmount);

        if (anim != null)
        {
            // Use discrete values (1 or -1) to trigger transitions reliably
            float horizontalValue = moveDirRaw > 0 ? 1f : -1f;
            anim.SetFloat("Horizontal", horizontalValue);
        }

        // 3. Handle Trash Spawning
        trashTimer -= Time.deltaTime;
        if (trashTimer <= 0)
        {
            DropTrash();
            trashTimer = data.trashDropRate;
        }
    }

    private void DropTrash()
    {
        if (_trashPrefab != null)
        {
            Instantiate(_trashPrefab, transform.position, Quaternion.identity);
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
                    Instantiate(_trashPrefab, player.transform.position + offset, Quaternion.identity);
                }
                break;
            case NPCType.OknumOrmas:
                player.ApplySlow(10f);
                break;
        }
    }
}