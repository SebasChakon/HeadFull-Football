using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    [Header("¿Quién mete gol cuando el balón entra aquí?")]
    public int playerWhoScores = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            GameManager.Instance?.GoalScored(playerWhoScores);
        }
    }
}