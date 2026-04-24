using UnityEngine;

public class Trash : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddTrash();
        }
    }
}