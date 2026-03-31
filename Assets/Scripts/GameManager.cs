using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Marcador - arrastra aquí los textos del marcador")]
    public TMP_Text scorePlayer1;
    public TMP_Text scorePlayer2;

    private int goalsPlayer1 = 0;
    private int goalsPlayer2 = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GoalScored(int player)
    {
        if (player == 1) goalsPlayer1++;
        else             goalsPlayer2++;

        UpdateScoreboard();
        FindObjectOfType<BallController>()?.ResetBall();
    }

    void UpdateScoreboard()
    {
        if (scorePlayer1) scorePlayer1.text = goalsPlayer1.ToString();
        if (scorePlayer2) scorePlayer2.text = goalsPlayer2.ToString();
    }

    public void ResetScore()
    {
        goalsPlayer1 = 0;
        goalsPlayer2 = 0;
        UpdateScoreboard();
    }
}