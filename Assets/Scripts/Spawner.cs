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

    public Rect spawnArea = new Rect(-10, -10, 20, 20);
    
    private List<GameObject> activeNPCs = new List<GameObject>();

    private void Start()
    {
        SpawnInitialTrash();
        StartCoroutine(SpawnNPCRoutine());
    }

    private void SpawnInitialTrash()
    {
        for (int i = 0; i < initialTrashCount; i++)
        {
            Vector2 spawnPos = new Vector2(
                Random.Range(spawnArea.xMin, spawnArea.xMax),
                Random.Range(spawnArea.yMin, spawnArea.yMax)
            );
            Instantiate(trashPrefab, spawnPos, Quaternion.identity);
        }
    }

    private IEnumerator SpawnNPCRoutine()
    {
        while (GameManager.Instance != null && !GameManager.Instance.isGameOver)
        {
            float waitTime = Random.Range(npcSpawnIntervalMin, npcSpawnIntervalMax);
            yield return new WaitForSeconds(waitTime);

            if (activeNPCs.Count < 3)
            {
                SpawnNPC();
            }
        }
    }

    private void SpawnNPC()
    {
        if (npcTypes == null || npcTypes.Count == 0) return;
        
        Vector2 spawnPos = GetPerimeterSpawnPos();
        GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        NPCBehavior behavior = npc.GetComponent<NPCBehavior>();
        if (behavior != null)
        {
            behavior.data = npcTypes[Random.Range(0, npcTypes.Count)];
        }
        
        activeNPCs.Add(npc);
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