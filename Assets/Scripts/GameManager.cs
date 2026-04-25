using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float gameDuration = 300f; // 5 minutes
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
            currentTime = gameDuration;
        }
        else 
        {
            Destroy(gameObject);
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
            UIManager.Instance.ShowEvaluation(GetCleanlinessPercentage(), empathyScore);
        }
    }
}