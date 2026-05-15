using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Configs")]
    public LevelConfig level1Config;
    public LevelConfig level2Config;

    [HideInInspector]
    public LevelConfig currentLevelConfig;

    // Persists across scene reloads
    public static int currentLevel = 1;

    public float gameDuration = 300f;
    public float currentTime;
    
    public int totalTrashSpawned = 0;
    public int trashPicked = 0;
    public float empathyScore = 100f;

    public bool isInteracting = false;
    public bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            // Force reset state to prevent Inspector-induced freezes
            isGameOver = false;
            isInteracting = false;

            // Select level config
            if (currentLevel == 2 && level2Config != null)
                currentLevelConfig = level2Config;
            else if (level1Config != null)
                currentLevelConfig = level1Config;

            // Apply config
            if (currentLevelConfig != null)
            {
                gameDuration = currentLevelConfig.gameDuration;
            }

            currentTime = gameDuration;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyLevelConfig();
    }

    private void ApplyLevelConfig()
    {
        if (currentLevelConfig == null) return;

        // Swap map sprite
        var mapObj = GameObject.Find("Map 1_0");
        if (mapObj != null && currentLevelConfig.mapSprite != null)
        {
            var sr = mapObj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = currentLevelConfig.mapSprite;
        }

        // Set camera bounds
        var camFollow = Camera.main?.GetComponent<SapuJagad.CameraFollow>();
        if (camFollow != null)
        {
            camFollow.SetBounds(currentLevelConfig.mapMin, currentLevelConfig.mapMax);
        }
    }

    private void Update()
    {
        if (isGameOver || isInteracting) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            EndGame();
        }
    }

    public void AddTrash(int amount = 1)
    {
        totalTrashSpawned += amount;
    }

    public void PickTrash()
    {
        trashPicked++;
        
        // Instant win if cleanliness reaches 100%
        if (totalTrashSpawned > 0 && trashPicked >= totalTrashSpawned)
        {
            EndGame();
        }
    }

    public void ModifyEmpathy(float amount)
    {
        empathyScore = Mathf.Clamp(empathyScore + amount, 0, 100);
    }

    public float GetCleanlinessPercentage()
    {
        if (totalTrashSpawned == 0) return 100f;
        return (float)trashPicked / totalTrashSpawned * 100f;
    }

    private void EndGame()
    {
        if (isGameOver) return; // Prevent double trigger
        
        isGameOver = true;
        Debug.Log("Game Over! Cleanliness: " + GetCleanlinessPercentage() + "%, Empathy: " + empathyScore);
        
        if (UIManager.Instance != null)
        {
            float cleanThreshold = currentLevelConfig != null ? currentLevelConfig.cleanlinessThreshold : 70f;
            float empathyThreshold = currentLevelConfig != null ? currentLevelConfig.empathyThreshold : 80f;
            bool won = GetCleanlinessPercentage() >= cleanThreshold && empathyScore >= empathyThreshold;

            UIManager.Instance.ShowEvaluation(GetCleanlinessPercentage(), empathyScore, currentLevel, won);
        }
    }

    public static void LoadLevel(int level)
    {
        currentLevel = level;
        SceneManager.LoadScene("MainGame");
    }

    public static void ReturnToMainMenu()
    {
        currentLevel = 1;
        SceneManager.LoadScene("MainMenu");
    }
}