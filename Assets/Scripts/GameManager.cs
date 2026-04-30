using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Evento para que la cámara y el RoundManager reaccionen al gol
    public static event System.Action<int> OnGoalScored;

    [Header("Audio")]
    public AudioClip goalSound;
    private AudioSource audioSource;

    [Header("Nombres de personajes")]
    public TMP_Text player1CharName;
    public TMP_Text player2CharName;

    [Header("Marcador")]
    public TMP_Text scorePlayer1;
    public TMP_Text scorePlayer2;

    [Header("Pantalla de fin de juego")]
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    public TMP_Text finalScoreText;

    private int goalsPlayer1 = 0;
    private int goalsPlayer2 = 0;

    public bool IsGameOver { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Crear AudioSource desde código
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        Debug.Log("AudioSource creado: " + audioSource);
    }

    void Start()
    {
        string name1 = PlayerPrefs.GetString("Player1Character", "Jugador 1");
        string name2 = PlayerPrefs.GetString("Player2Character", "Jugador 2");

        if (player1CharName) player1CharName.text = name1;
        if (player2CharName) player2CharName.text = name2;

        if (gameOverPanel) gameOverPanel.SetActive(false);
        IsGameOver = false;
    }

    public void GoalScored(int player)
    {
        Debug.Log("Gol! audioSource: " + audioSource + " | goalSound: " + goalSound);
    
        if (IsGameOver) return;

        if (player == 1) goalsPlayer1++;
        else             goalsPlayer2++;

        if (audioSource && goalSound)
            audioSource.PlayOneShot(goalSound,2f);

        UpdateScoreboard();
        OnGoalScored?.Invoke(player);
        // El RoundManager escucha este evento y hace el countdown post-gol
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

    // RoundManager llama esto cuando el tiempo se acaba
    public void ShowGameOver()
    {
        IsGameOver = true;

        if (gameOverPanel) gameOverPanel.SetActive(true);

        string name1 = PlayerPrefs.GetString("Player1Character", "Jugador 1");
        string name2 = PlayerPrefs.GetString("Player2Character", "Jugador 2");

        if (winnerText)
        {
            if (goalsPlayer1 > goalsPlayer2)
                winnerText.text = $"¡{name1} gana!";
            else if (goalsPlayer2 > goalsPlayer1)
                winnerText.text = $"¡{name2} gana!";
            else
                winnerText.text = "¡Empate!";
        }

        if (finalScoreText)
            finalScoreText.text = $"{goalsPlayer1}  -  {goalsPlayer2}";

        Time.timeScale = 0f;
    }

    // Botón BackToMenuButton
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}