using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "SapuJagad/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public string levelName = "Level 1";
    public int levelNumber = 1;
    public Sprite mapSprite;
    
    [Header("Difficulty Settings")]
    public float gameDuration = 300f;
    public int initialTrashCount = 20;
    public float npcSpawnIntervalMin = 30f;
    public float npcSpawnIntervalMax = 60f;
    public int maxActiveNPCs = 3;
    public float trashPickingDuration = 3f;
    
    [Header("NPC Data")]
    public List<NPCData> npcTypes;
    
    [Header("Win Conditions")]
    public float cleanlinessThreshold = 70f;
    public float empathyThreshold = 80f;
    
    [Header("Map Bounds")]
    public Vector2 mapMin = new Vector2(-17.5f, -17.5f);
    public Vector2 mapMax = new Vector2(17.5f, 17.5f);
    public Rect spawnArea = new Rect(-10, -10, 20, 20);

    [Header("Wall Layout")]
    [Tooltip("Prefab containing all NPCWall colliders for this level")]
    public GameObject wallLayoutPrefab;
}
