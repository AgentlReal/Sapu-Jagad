using UnityEngine;
using System.Collections.Generic;

public class NPCBehavior : MonoBehaviour
{
    public NPCData data;
    private bool interacted = false;
    private float trashTimer = 0f;

    private static GameObject _trashPrefab;

    private void Start()
    {
        if (data != null)
        {
            trashTimer = data.trashDropRate;
        }

        if (_trashPrefab == null)
        {
            var spawner = GameObject.FindObjectsByType<Spawner>(FindObjectsSortMode.None)[0];
            _trashPrefab = spawner.trashPrefab;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.isInteracting || GameManager.Instance.isGameOver || interacted) return;

        // Simple random movement
        transform.Translate(Vector2.right * data.moveSpeed * Time.deltaTime * Mathf.Sin(Time.time * 0.5f));

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
        UIManager.Instance.StartMiniGame(selected);
        
        interacted = true; 
    }

    public void ApplyPenalty(SapuJagad.PlayerController player)
    {
        switch (data.type)
        {
            case NPCType.IbuIbu:
                player.ApplyStun(7f);
                break;
            case NPCType.AnakKecil:
                // Spawn 5 trash around player
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