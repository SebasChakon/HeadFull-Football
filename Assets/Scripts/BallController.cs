using UnityEngine;

public class BallController : MonoBehaviour
{
    public float maxSpeed = 14f;

    [HideInInspector] public bool isCarried = false;
    [HideInInspector] public PlayerController carrier = null;

    private Rigidbody rb;
    private Vector3 startPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (isCarried)
        {
            // El balón sigue al jugador, se pega enfrente de él
            Vector3 targetPos = carrier.transform.position
                              + carrier.transform.forward * 0.8f;
            targetPos.y = 0.5f;
            rb.MovePosition(targetPos);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        // Mantener en el plano
        Vector3 pos = rb.position;
        pos.y = 0.5f;
        rb.MovePosition(pos);

        // Limitar velocidad
        Vector3 vel = rb.linearVelocity;
        vel.y = 0;
        if (vel.magnitude > maxSpeed)
            vel = vel.normalized * maxSpeed;
        rb.linearVelocity = vel;
    }

    public void Pickup(PlayerController player)
    {
        isCarried = true;
        carrier = player;
        rb.isKinematic = true;
    }

    public void Kick(Vector3 direction, float force)
    {
        isCarried = false;
        carrier = null;
        rb.isKinematic = false;

        direction.y = 0;
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }

    public void ResetBall()
    {
        isCarried = false;
        carrier = null;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
    }
}