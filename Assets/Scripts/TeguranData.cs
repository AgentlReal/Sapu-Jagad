using UnityEngine;

[CreateAssetMenu(fileName = "TeguranData", menuName = "SapuJagad/TeguranData")]
public class TeguranData : ScriptableObject
{
    public NPCType targetType;
    [TextArea]
    public string fullSentence;
}