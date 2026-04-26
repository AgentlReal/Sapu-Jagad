using UnityEngine;
using System.Collections.Generic;

public class NPCBehavior : MonoBehaviour
{
    public NPCData data;
    private bool interacted = false;
    private float trashTimer = 0f;

    private static GameObject _trashPrefab;
    private SpriteRenderer sr;
    private Spawner _spawner;
    private Animator anim;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        // Cache spawner reference
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
        if (data == null || sr == null) return;
        sr.sprite = data.sprite;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.isInteracting || GameManager.Instance.isGameOver || interacted) 
        {
            if (anim != null) anim.SetFloat("Horizontal", 0);
            return;
        }

        // Simple random movement
        float moveStep = Mathf.Sin(Time.time * 0.5f);
        transform.Translate(Vector2.right * data.moveSpeed * Time.deltaTime * moveStep);

        if (anim != null)
        {
            anim.SetFloat("Horizontal", moveStep);
        }

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
            Debug.Log(data.npcName + " dropped trash.");
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
            // Failure: Apply penalty
            var player = GameObject.Find("Player")?.GetComponent<SapuJagad.PlayerController>();
            if (player != null) ApplyPenalty(player);
        }

        // Notify spawner to free up the slot
        if (_spawner != null) _spawner.OnNPCRemoved(gameObject);
        
        // Remove the NPC from the scene
        Destroy(gameObject);
    }

    private void ApplyPenalty(SapuJagad.PlayerController player)
    {
        if (player == null) return;
        
        Debug.Log("Applying penalty from " + data.npcName + " (" + data.type + ")");
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