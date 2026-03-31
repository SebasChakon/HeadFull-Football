using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [Header("Duración de la ronda en segundos")]
    public float roundDuration = 60f;

    [Header("Texto del timer en escena")]
    public TMP_Text timerText;

    private float timeLeft;
    private bool roundStarted = false;
    private bool roundEnded = false;

    // Posiciones iniciales
    private Vector3 ballStartPos;
    private Vector3 player1StartPos;
    private Vector3 player2StartPos;

    private BallController ball;
    private PlayerController[] players;

    void Start()
    {
        ball = FindObjectOfType<BallController>();
        players = FindObjectsOfType<PlayerController>();

        // Guardar posiciones iniciales
        ballStartPos = ball.transform.position;
        player1StartPos = players[0].transform.position;
        player2StartPos = players[1].transform.position;

        timeLeft = roundDuration;
        UpdateTimerText();
    }

    void Update()
    {
        // Arrancar cuando cualquier jugador presione una tecla
        if (!roundStarted && !roundEnded)
        {
            if (Input.anyKeyDown)
                roundStarted = true;
        }

        if (!roundStarted || roundEnded) return;

        // Contar tiempo
        timeLeft -= Time.deltaTime;
        UpdateTimerText();

        if (timeLeft <= 0f)
            EndRound();
    }

    void UpdateTimerText()
    {
        if (timerText == null) return;
        int seconds = Mathf.CeilToInt(Mathf.Max(timeLeft, 0f));
        timerText.text = seconds.ToString();
    }

    void EndRound()
    {
        roundEnded = true;
        roundStarted = false;

        ball.ResetBall();

        var rb0 = players[0].GetComponent<Rigidbody>();
        rb0.linearVelocity = Vector3.zero;
        players[0].transform.position = player1StartPos;

        var rb1 = players[1].GetComponent<Rigidbody>();
        rb1.linearVelocity = Vector3.zero;
        players[1].transform.position = player2StartPos;

        // Resetear puntos
        GameManager.Instance.ResetScore();

        Invoke(nameof(RestartRound), 1f);
    }

    void RestartRound()
    {
        timeLeft = roundDuration;
        roundEnded = false;
        UpdateTimerText();
        // La ronda vuelve a esperar que alguien presione una tecla
    }
}