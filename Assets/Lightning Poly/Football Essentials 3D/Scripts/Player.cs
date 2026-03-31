namespace LightningPoly.FootballEssentials3D
{
    using UnityEngine;

    public class Player : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float acceleration = 10f;
        public float deceleration = 5f;
        public float jumpForce = 7f;
        public float rotationSpeed = 10f;
        public float kickForce = 10f;
        public float kickUpwardForce = 2f;

        private Rigidbody rb;
        private bool isGrounded;
        private Transform cameraTransform;
        private Vector3 moveDirection;

        public GameObject[] decorations, eyes, mouths, hairs, all;

        private void OnGUI()
        {
            if (GUILayout.Button("Change Character Appearance"))
            {
                ChangeCloth();
            }
        }
        [ContextMenu(nameof(ChangeCloth))]
        public void ChangeCloth()
        {
            foreach (var item in all)
            {
                item.SetActive(false);
            }
            decorations[Random.Range(0, decorations.Length)].SetActive(true);
            eyes[Random.Range(0, eyes.Length)].SetActive(true);
            mouths[Random.Range(0, mouths.Length)].SetActive(true);
            hairs[Random.Range(0, hairs.Length)].SetActive(true);
        }


        void Start()
        {
            rb = GetComponent<Rigidbody>();
            cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            ProcessInput();
            Jump();
            RotateCharacter();
        }

        void FixedUpdate()
        {
            Move();
        }

        void ProcessInput()
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * moveZ + right * moveX).normalized;
        }

        void Move()
        {
            Vector3 targetVelocity = moveDirection * moveSpeed;
            if (!isGrounded)
            {
                targetVelocity.y = rb.linearVelocity.y;
            }

            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, (isGrounded ? acceleration : deceleration) * Time.fixedDeltaTime);
        }

        void RotateCharacter()
        {
            if (moveDirection.magnitude > 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        void Jump()
        {
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
        }

        void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out Ground ground))
            {
                isGrounded = true;
            }
        }

        void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out Ground ground))
            {
                isGrounded = false;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out Ball ball))
            {
                Rigidbody ballRb = collision.gameObject.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    Vector3 kickDirection = (collision.transform.position - transform.position).normalized;
                    kickDirection.y += kickUpwardForce;

                    ballRb.AddForce(kickDirection * kickForce, ForceMode.Impulse);
                    ballRb.AddTorque(Vector3.up * kickForce * 0.2f, ForceMode.Impulse);
                }
            }
        }
    }

}

