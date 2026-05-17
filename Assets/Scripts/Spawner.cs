using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public GameObject trashPrefab;
    public List<NPCData> npcTypes;
    public GameObject npcPrefab;

    public int initialTrashCount = 20;
    public float npcSpawnIntervalMin = 30f;
    public float npcSpawnIntervalMax = 60f;
    public int maxActiveNPCs = 3;

    public Rect spawnArea = new Rect(-10, -10, 20, 20);

    [Header("Trash Sprite Variants")]
    public Sprite[] trashSpriteVariants;
    
    private List<GameObject> activeNPCs = new List<GameObject>();
    private int npcLayerMask;

    // Singleton-like access for NPCBehavior to get sprites
    public static Spawner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        npcLayerMask = LayerMask.GetMask("NPCWall");

        // Apply level config if available
        if (GameManager.Instance != null && GameManager.Instance.currentLevelConfig != null)
        {
            var config = GameManager.Instance.currentLevelConfig;
            initialTrashCount = config.initialTrashCount;
            npcSpawnIntervalMin = config.npcSpawnIntervalMin;
            npcSpawnIntervalMax = config.npcSpawnIntervalMax;
            maxActiveNPCs = config.maxActiveNPCs;
            spawnArea = config.spawnArea;

            if (config.npcTypes != null && config.npcTypes.Count > 0)
                npcTypes = config.npcTypes;
        }

        SpawnInitialTrash();
        StartCoroutine(SpawnNPCRoutine());
    }

    private void SpawnInitialTrash()
    {
        for (int i = 0; i < initialTrashCount; i++)
        {
            Vector2 spawnPos = GetValidSpawnPos();
            GameObject trash = Instantiate(trashPrefab, spawnPos, Quaternion.identity);
            AssignRandomTrashSprite(trash);
        }
    }

    /// <summary>
    /// Assigns a random sprite variant to a trash GameObject.
    /// </summary>
    public void AssignRandomTrashSprite(GameObject trashObj)
    {
        if (trashSpriteVariants == null || trashSpriteVariants.Length == 0) return;
        if (trashObj == null) return;

        var sr = trashObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = trashSpriteVariants[Random.Range(0, trashSpriteVariants.Length)];
        }
    }

    private Vector2 GetValidSpawnPos()
    {
        Vector2 pos;
        int attempts = 0;
        do
        {
            pos = new Vector2(
                Random.Range(spawnArea.xMin, spawnArea.xMax),
                Random.Range(spawnArea.yMin, spawnArea.yMax)
            );
            attempts++;
        } while (Physics2D.OverlapPoint(pos, npcLayerMask) != null && attempts < 10);
        
        // If still on a wall after attempts, nudge to nearest valid position
        if (Physics2D.OverlapPoint(pos, npcLayerMask) != null)
        {
            pos = NudgeToValidPosition(pos);
        }
        
        return pos;
    }

    private IEnumerator SpawnNPCRoutine()
    {
        while (GameManager.Instance != null && !GameManager.Instance.isGameOver)
        {
            float waitTime = Random.Range(npcSpawnIntervalMin, npcSpawnIntervalMax);
            yield return new WaitForSeconds(waitTime);

            // Clean null entries
            activeNPCs.RemoveAll(n => n == null);

            if (activeNPCs.Count < maxActiveNPCs)
            {
                SpawnNPC();
            }
        }
    }

    private void SpawnNPC()
    {
        if (npcTypes == null || npcTypes.Count == 0) return;
        
        Vector2 spawnPos = GetPerimeterSpawnPos();
        
        // If spawn pos is on a wall, nudge instead of blocking
        if (Physics2D.OverlapPoint(spawnPos, npcLayerMask) != null)
        {
            spawnPos = NudgeToValidPosition(spawnPos);
            
            // Final safety: make sure we're still inside the map
            spawnPos.x = Mathf.Clamp(spawnPos.x, spawnArea.xMin, spawnArea.xMax);
            spawnPos.y = Mathf.Clamp(spawnPos.y, spawnArea.yMin, spawnArea.yMax);
            
            // If still on a wall after nudging, skip this spawn
            if (Physics2D.OverlapPoint(spawnPos, npcLayerMask) != null)
            {
                Debug.LogWarning("NPC Spawn blocked by NPCWall at " + spawnPos + " even after nudge.");
                return;
            }
        }

        GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        NPCBehavior behavior = npc.GetComponent<NPCBehavior>();
        if (behavior != null)
        {
            behavior.data = npcTypes[Random.Range(0, npcTypes.Count)];
        }
        
        activeNPCs.Add(npc);
    }

    /// <summary>
    /// Tries to nudge a position out of a wall by testing nearby offsets.
    /// Searches in expanding rings around the original point, staying within the spawn area.
    /// </summary>
    private Vector2 NudgeToValidPosition(Vector2 pos)
    {
        float[] offsets = { 0.5f, 1f, 1.5f, 2f, 3f, 4f };
        Vector2[] directions = {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
            new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
        };

        foreach (float offset in offsets)
        {
            foreach (Vector2 dir in directions)
            {
                Vector2 testPos = pos + dir * offset;
                
                // Must be inside the spawn area
                if (testPos.x < spawnArea.xMin || testPos.x > spawnArea.xMax ||
                    testPos.y < spawnArea.yMin || testPos.y > spawnArea.yMax)
                    continue;
                
                if (Physics2D.OverlapPoint(testPos, npcLayerMask) == null)
                {
                    return testPos;
                }
            }
        }
        
        // Fallback: return original position
        return pos;
    }

    private Vector2 GetPerimeterSpawnPos()
    {
        int edge = Random.Range(0, 4);
        float x = 0, y = 0;
        switch (edge)
        {
            case 0: x = spawnArea.xMin; y = Random.Range(spawnArea.yMin, spawnArea.yMax); break; // Left
            case 1: x = spawnArea.xMax; y = Random.Range(spawnArea.yMin, spawnArea.yMax); break; // Right
            case 2: y = spawnArea.yMin; x = Random.Range(spawnArea.xMin, spawnArea.xMax); break; // Bottom
            case 3: y = spawnArea.yMax; x = Random.Range(spawnArea.xMin, spawnArea.xMax); break; // Top
        }
        return new Vector2(x, y);
    }

    public void OnNPCRemoved(GameObject npc)
    {
        activeNPCs.Remove(npc);
    }
}