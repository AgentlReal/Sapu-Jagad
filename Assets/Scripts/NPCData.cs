using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPCData", menuName = "SapuJagad/NPCData")]
public class NPCData : ScriptableObject
{
    public NPCType type;
    public string npcName;
    public Sprite sprite;
    public float moveSpeed = 2f;
    public float trashDropRate = 5f; 
    public float penaltyDuration = 7f;
    public List<TeguranData> dialogueOptions;

    [Header("UI Feedback")]
    public Sprite faceNeutral;
    public Sprite faceHappy;
    public Sprite faceAngry;
}