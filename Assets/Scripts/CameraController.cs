using System.Collections;
using UnityEngine;

/// <summary>
/// - Sigue el punto medio entre los dos jugadores
/// - Siempre mira hacia ese punto medio (no se desorienta)
/// - Zoom dinámico según la distancia entre ellos
/// - Efecto de gol: zoom in suave (FOV) + screen shake
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Jugadores a seguir")]
    public Transform player1;
    public Transform player2;

    [Header("Seguimiento suave")]
    [Tooltip("Qué tan rápido sigue la cámara. Más bajo = más suave.")]
    public float followSmoothTime = 0.15f;

    [Tooltip("Offset respecto al punto medio (Y = altura, Z = distancia atrás)")]
    public Vector3 positionOffset = new Vector3(0f, 12f, -16f);

    [Tooltip("Qué tan rápido rota la cámara para mirar al punto medio")]
    public float rotationSmoothSpeed = 5f;

    [Header("Zoom dinámico")]
    public float baseFOV      = 55f;
    public float maxFOV       = 72f;
    public float maxDistance  = 22f;
    public float fovSmoothSpeed = 4f;

    [Header("Efecto de gol")]
    public float goalZoomFOV  = 38f;
    public float goalZoomIn   = 0.8f;
    public float goalHoldTime = 0.8f;
    public float goalZoomOut  = 1.0f;

    [Header("Screen Shake")]
    public float shakeMagnitude = 0.25f;
    public float shakeDuration  = 0.45f;

    // ─── Internos ─────────────────────────────────────────────────────────────
    private Camera  cam;
    private Vector3 velocity    = Vector3.zero;
    private Vector3 shakeOffset = Vector3.zero;
    private float   currentFOV;
    private bool    goalEffectRunning = false;

    void Awake()
    {
        cam        = GetComponent<Camera>();
        currentFOV = baseFOV;
    }

    void OnEnable()  { GameManager.OnGoalScored += OnGoalScored; }
    void OnDisable() { GameManager.OnGoalScored -= OnGoalScored; }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        Vector3 midpoint = (player1.position + player2.position) * 0.5f;

        if (!goalEffectRunning)
        {
            // ── Posición ────────────────────────────────────────────────────
            Vector3 targetPos = midpoint + positionOffset + shakeOffset;
            transform.position = Vector3.SmoothDamp(
                transform.position, targetPos, ref velocity, followSmoothTime);

            // ── Siempre mirar al punto medio ────────────────────────────────
            Vector3 dir = midpoint - transform.position;
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation  = Quaternion.Slerp(
                    transform.rotation, targetRot,
                    Time.deltaTime * rotationSmoothSpeed);
            }

            // ── Zoom dinámico ────────────────────────────────────────────────
            float dist      = Vector3.Distance(player1.position, player2.position);
            float t         = Mathf.Clamp01(dist / maxDistance);
            float targetFOV = Mathf.Lerp(baseFOV, maxFOV, t);
            currentFOV      = Mathf.Lerp(currentFOV, targetFOV,
                                          Time.deltaTime * fovSmoothSpeed);
        }

        cam.fieldOfView = currentFOV;
    }

    // ─── Efecto de gol ────────────────────────────────────────────────────────

    void OnGoalScored(int scoringPlayer)
    {
        if (!goalEffectRunning)
            StartCoroutine(GoalEffectCoroutine());
    }

    IEnumerator GoalEffectCoroutine()
    {
        goalEffectRunning = true;

        float startFOV = currentFOV;

        // Shake
        StartCoroutine(ScreenShake());

        // Fase 1: Zoom in
        float elapsed = 0f;
        while (elapsed < goalZoomIn)
        {
            elapsed    += Time.deltaTime;
            currentFOV  = Mathf.Lerp(startFOV, goalZoomFOV, elapsed / goalZoomIn);

            // Seguir mirando al punto medio incluso durante el efecto
            Vector3 mid = (player1.position + player2.position) * 0.5f;
            Vector3 dir = mid - transform.position;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * rotationSmoothSpeed);

            yield return null;
        }

        // Fase 2: Hold
        yield return new WaitForSeconds(goalHoldTime);

        // Fase 3: Zoom out
        float zoomedFOV = currentFOV;
        elapsed = 0f;
        while (elapsed < goalZoomOut)
        {
            elapsed    += Time.deltaTime;
            currentFOV  = Mathf.Lerp(zoomedFOV, baseFOV, elapsed / goalZoomOut);
            yield return null;
        }

        goalEffectRunning = false;
    }

    IEnumerator ScreenShake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed     += Time.deltaTime;
            float str    = Mathf.Lerp(shakeMagnitude, 0f, elapsed / shakeDuration);
            shakeOffset  = Random.insideUnitSphere * str;
            shakeOffset.z = 0f;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
