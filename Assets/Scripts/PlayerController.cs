using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Animación patada")]
    public float kickRotationSpeed = 15f;
    public float kickMoveDistance = 0.3f;
    private bool isKicking = false;
    private float kickAngle = 0f;
    private Vector3 shoeDefaultLocalPos;

    [Header("Escala visual")]
    public Vector3 headScale = new Vector3(160f, 160f, 160f);
    public Vector3 shoeScale = new Vector3(1f, 1f, 1f);

    [Header("Personaje visual")]
    public Transform headAttachPoint;
    public Transform shoeAttachPoint;
    private GameObject headInstance;
    private GameObject shoeInstance;

    [Header("Configuración")]
    public float maxPossessionTime = 2f;
    private float possessionTimer = 0f;
    public float moveSpeed = 6f;
    public float kickForce = 12f;
    public float pickupRadius = 1.0f;
    public float pickupCooldown = 0.4f;

    [Header("Qué mapa de controles usa este jugador")]
    public string actionMapName = "Player1";

    private Rigidbody rb;
    private BallController ball;
    private Vector3 moveDirection;
    private float pickupTimer = 0f;

    private InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction kickAction;
    private PlayerControls controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        controls = new PlayerControls();
        actionMap = controls.asset.FindActionMap(actionMapName);
        actionMap.Enable();

        moveAction = actionMap.FindAction("Move");
        kickAction = actionMap.FindAction("Kick");
    }

    void Start()
    {
        ball = FindObjectOfType<BallController>();

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
        if (pickupTimer > 0f)
            pickupTimer -= Time.deltaTime;

        Vector2 input = moveAction.ReadValue<Vector2>();
        moveDirection = new Vector3(input.x, 0, input.y).normalized;

        if (moveDirection != Vector3.zero)
            transform.forward = moveDirection;

        if (ball.carrier == this)
        {
            possessionTimer += Time.deltaTime;
            if (possessionTimer >= maxPossessionTime)
            {
                TriggerKickAnimation();
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

        if (kickAction.WasPressedThisFrame() && ball.carrier == this)
        {
            TriggerKickAnimation();
            ball.Kick(transform.forward, kickForce);
            pickupTimer = pickupCooldown;
            return;
        }

        if (!ball.isCarried && pickupTimer <= 0f)
        {
            float dist = Vector3.Distance(transform.position, ball.transform.position);
            if (dist <= pickupRadius)
                ball.Pickup(this);
        }
        AnimateShoe();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        if (headInstance != null)
        {
            Vector3 targetDir;

            if (ball.carrier == this)
                targetDir = ball.transform.position - headAttachPoint.position;
            else
                targetDir = moveDirection;

            targetDir.y = 0;

            if (targetDir != Vector3.zero)
            {
                // Rotar el headAttachPoint hacia la dirección
                Quaternion targetRotation = Quaternion.LookRotation(targetDir);
                headAttachPoint.rotation = Quaternion.Lerp(
                    headAttachPoint.rotation,
                    targetRotation,
                    10f * Time.fixedDeltaTime
                );

                // El zapato sigue exactamente la misma rotación
                if (shoeAttachPoint != null)
                    shoeAttachPoint.rotation = headAttachPoint.rotation;
            }
        }

    }

    void OnDestroy()
    {
        actionMap?.Disable();
        controls?.asset.FindActionMap(actionMapName).Disable();
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

    public void LoadCharacter(string characterName)
    {
        // Limpiar instancias anteriores
        if (headInstance != null) Destroy(headInstance);
        if (shoeInstance != null) Destroy(shoeInstance);

        // Cargar cabeza
        GameObject prefab = Resources.Load<GameObject>("Characters/" + characterName);
        if (prefab == null)
        {
            Debug.LogError("No se encontró el personaje: " + characterName);
            return;
        }

        headInstance = Instantiate(prefab, headAttachPoint.position, Quaternion.identity);
        headInstance.transform.SetParent(headAttachPoint);
        headInstance.transform.localPosition = Vector3.zero;
        headInstance.transform.localScale = headScale;
        headInstance.transform.localRotation = Quaternion.Euler(270, 0, 90);

        // Cargar zapato
        GameObject shoePrefab = Resources.Load<GameObject>("Shoe");
        if (shoePrefab == null)
        {
            Debug.LogWarning("No se encontró el prefab Shoe en Resources/");
            return;
        }

        shoeInstance = Instantiate(shoePrefab, shoeAttachPoint.position, Quaternion.identity);
        shoeInstance.transform.SetParent(shoeAttachPoint);
        shoeInstance.transform.localPosition = Vector3.zero;
        shoeDefaultLocalPos = Vector3.zero;
        shoeInstance.transform.localScale = shoeScale;
        shoeInstance.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    void AnimateShoe()
    {
        if (shoeAttachPoint == null || shoeInstance == null) return;

        if (isKicking)
        {
            kickAngle = Mathf.MoveTowards(kickAngle, 90f, kickRotationSpeed * 100f * Time.deltaTime);
            if (kickAngle >= 90f) isKicking = false;
        }
        else
        {
            kickAngle = Mathf.MoveTowards(kickAngle, 0f, kickRotationSpeed * 60f * Time.deltaTime);
        }

        // Rotar el zapato en X para simular la patada
        Quaternion baseRot = Quaternion.Euler(0, 0, 0);
        shoeInstance.transform.localRotation = baseRot * Quaternion.Euler(-kickAngle, 0, 0);

        // Mover ligeramente hacia adelante
        float forward = (kickAngle / 90f) * kickMoveDistance;
        shoeInstance.transform.localPosition = shoeDefaultLocalPos + new Vector3(0, 0, forward);
    }

    void TriggerKickAnimation()
    {
        isKicking = true;
        kickAngle = 0f;
    }
}