using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración")]
    public float maxPossessionTime = 2f;
    private float possessionTimer = 0f;
    public float moveSpeed = 6f;
    public float kickForce = 12f;
    public float pickupRadius = 1.0f;
    public float pickupCooldown = 0.4f; // segundos sin poder recoger tras patear

    [Header("Qué mapa de controles usa este jugador")]
    public string actionMapName = "Player1";

    private Rigidbody rb;
    private BallController ball;
    private Vector3 moveDirection;
    private float pickupTimer = 0f;

    private InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction kickAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        PlayerControls controls = new PlayerControls();
        actionMap = controls.asset.FindActionMap(actionMapName);
        actionMap.Enable();

        moveAction = actionMap.FindAction("Move");
        kickAction = actionMap.FindAction("Kick");
    }

    void Start()
    {
        ball = FindObjectOfType<BallController>();

        // Ignorar colisiones entre el jugador y los triggers de portería
        GoalDetector[] goals = FindObjectsOfType<GoalDetector>();
        foreach (GoalDetector goal in goals)
        {
            Physics.IgnoreCollision(
                GetComponent<Collider>(),
                goal.GetComponent<Collider>()
            );
        }
    }

    void Update()
    {
        // Bajar el timer de cooldown
        if (pickupTimer > 0f)
            pickupTimer -= Time.deltaTime;

        // Movimiento
        Vector2 input = moveAction.ReadValue<Vector2>();
        moveDirection = new Vector3(input.x, 0, input.y).normalized;

        if (moveDirection != Vector3.zero)
            transform.forward = moveDirection;


        // Contar tiempo de posesión
        if (ball.carrier == this)
        {
            possessionTimer += Time.deltaTime;

            // Auto-patear si lleva 2 segundos con el balón
            if (possessionTimer >= maxPossessionTime)
            {
                ball.Kick(transform.forward, kickForce);
                pickupTimer = pickupCooldown;
                possessionTimer = 0f;
                return;
            }
        }
        else
        {
            possessionTimer = 0f;
        }

        // Patear — tiene prioridad, va antes del pickup
        if (kickAction.WasPressedThisFrame() && ball.carrier == this)
        {
            ball.Kick(transform.forward, kickForce);
            pickupTimer = pickupCooldown; // activar cooldown
            return; // salir del Update para no procesar pickup en este frame
        }

        // Recoger balón solo si el cooldown terminó y nadie lo tiene
        if (!ball.isCarried && pickupTimer <= 0f)
        {
            float dist = Vector3.Distance(transform.position, ball.transform.position);
            if (dist <= pickupRadius)
                ball.Pickup(this);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    void OnDestroy()
    {
        actionMap?.Disable();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            var rb = GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            var rb = GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
        }
    }
}