using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Nombres de personajes")]
    public TMP_Text player1CharName;
    public TMP_Text player2CharName;

    [Header("Marcador - arrastra aquí los textos del marcador")]
    public TMP_Text scorePlayer1;
    public TMP_Text scorePlayer2;

    private int goalsPlayer1 = 0;
    private int goalsPlayer2 = 0;

    void Start()
    {
        string name1 = PlayerPrefs.GetString("Player1Character", "Jugador 1");
        string name2 = PlayerPrefs.GetString("Player2Character", "Jugador 2");

        if (player1CharName) player1CharName.text = name1;
        if (player2CharName) player2CharName.text = name2;
    }

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