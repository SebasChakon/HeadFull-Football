using System.Collections;
using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [Header("Duración de la ronda en segundos")]
    public float roundDuration = 60f;

    [Header("UI - Timer de la ronda")]
    public TMP_Text timerText;

    [Header("UI - Countdown 3-2-1 (texto grande del centro)")]
    public TMP_Text countdownText;

    // ─── Estado interno ───────────────────────────────────────────────────────
    private float timeLeft;
    private bool roundActive  = false;
    private bool roundEnded   = false;

    // ─── Referencias ─────────────────────────────────────────────────────────
    private Vector3   ballStartPos;
    private Vector3[] playerStartPositions;

    private BallController     ball;
    private PlayerController[] players;

    // ─────────────────────────────────────────────────────────────────────────

    void OnEnable()
    {
        GameManager.OnGoalScored += HandleGoalScored;
    }

    void OnDisable()
    {
        GameManager.OnGoalScored -= HandleGoalScored;
    }

    void Start()
    {
        ball    = FindObjectOfType<BallController>();
        players = FindObjectsOfType<PlayerController>();

        // Ordenar para que Player1 sea siempre [0]
        System.Array.Sort(players, (a, b) =>
            string.Compare(a.actionMapName, b.actionMapName));

        // Guardar posiciones iniciales
        ballStartPos          = ball.transform.position;
        playerStartPositions  = new Vector3[players.Length];
        for (int i = 0; i < players.Length; i++)
            playerStartPositions[i] = players[i].transform.position;

        timeLeft = roundDuration;
        UpdateTimerText();

        if (countdownText) countdownText.gameObject.SetActive(false);

        StartCoroutine(PreRoundCountdown());
    }

    void Update()
    {
        if (!roundActive || roundEnded) return;

        timeLeft -= Time.deltaTime;
        UpdateTimerText();

        if (timeLeft <= 0f)
            StartCoroutine(EndRound());
    }

    // ─── Countdown inicial ────────────────────────────────────────────────────

    IEnumerator PreRoundCountdown()
    {
        SetPlayersEnabled(false);
        ShowCountdown(true);

        for (int i = 3; i >= 1; i--)
        {
            SetCountdownText(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        SetCountdownText("¡GO!");
        yield return new WaitForSeconds(0.6f);

        ShowCountdown(false);
        SetPlayersEnabled(true);
        roundActive = true;
    }

    // ─── Gol anotado ─────────────────────────────────────────────────────────

    void HandleGoalScored(int player)
    {
        if (GameManager.Instance.IsGameOver) return;
        if (roundEnded) return;
        if (!roundActive) return; // ← esto evita el doble gol

        StartCoroutine(PostGoalSequence());
    }

    IEnumerator PostGoalSequence()
    {
        roundActive = false;
        SetPlayersEnabled(false);
        ball.ResetBall(); // ← resetear pelota INMEDIATAMENTE

        // Esperar que el efecto de cámara haga lo suyo
        yield return new WaitForSeconds(2.2f);

        // Resetear posiciones de jugadores
        for (int i = 0; i < players.Length; i++)
        {
            var rb = players[i].GetComponent<Rigidbody>();
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            players[i].transform.position = playerStartPositions[i];
        }

        ShowCountdown(true);
        for (int i = 3; i >= 1; i--)
        {
            SetCountdownText(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        SetCountdownText("¡GO!");
        yield return new WaitForSeconds(0.6f);

        ShowCountdown(false);
        SetPlayersEnabled(true);
        roundActive = true;
    }

    // ─── Fin de ronda por tiempo ──────────────────────────────────────────────

    IEnumerator EndRound()
    {
        roundEnded  = true;
        roundActive = false;
        SetPlayersEnabled(false);

        // Mostrar "¡Fin!" brevemente antes del game over
        ShowCountdown(true);
        SetCountdownText("¡Fin!");
        yield return new WaitForSeconds(1.5f);
        ShowCountdown(false);

        GameManager.Instance.ShowGameOver();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    void ResetPositions()
    {
        ball.ResetBall();
        for (int i = 0; i < players.Length; i++)
        {
            var rb = players[i].GetComponent<Rigidbody>();
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            players[i].transform.position = playerStartPositions[i];
        }
    }

    void SetPlayersEnabled(bool enabled)
    {
        foreach (var p in players)
            p.enabled = enabled;
    }

    void UpdateTimerText()
    {
        if (timerText == null) return;
        int seconds = Mathf.CeilToInt(Mathf.Max(timeLeft, 0f));
        timerText.text = seconds.ToString();
    }

    void ShowCountdown(bool show)
    {
        if (countdownText) countdownText.gameObject.SetActive(show);
    }

    void SetCountdownText(string text)
    {
        if (countdownText) countdownText.text = text;
    }
}