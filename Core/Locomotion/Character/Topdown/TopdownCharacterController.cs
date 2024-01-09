using System.Collections;
using Luna.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace Luna.Core.Locomotion.Character
{
    public class TopdownCharacterController : MonoBehaviour
    {
        public float moveSpeed = 2f;
        public float burstSpeed;

        public GameObject projectile;

        private Animator animator;
        private int lastDirection;

        private InputActions inputs;
        private bool isCharging;

        private void Awake()
        {
            inputs = new InputActions();
            // inputs.Gameplay.AddCallbacks(this);
        }

        private void OnEnable()
        {
            inputs.Enable();
        }
        private void Start()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void OnDisable()
        {
            inputs.Disable();
        }

        private void Update()
        {
            var move = inputs.Gameplay.Move.ReadValue<Vector2>();
            var look = inputs.Gameplay.Look.ReadValue<Vector2>();
            var sprint = inputs.Gameplay.Sprint.ReadValue<float>();

            if (sprint > 0.5) move *= 2;

            // Update orientation first, then move.
            // Otherwise move orientation will lag behind by one frame.
            // Look(look);
            SetDirection(move);
            Move(move);
            Rotate(move);
        }

        private void FixedUpdate()
        {
            // float horizontalInput = Input.GetAxis("Horizontal");
            // float verticalInput = Input.GetAxis("Vertical");
            //
            // Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
            // inputVector = Vector2.ClampMagnitude(inputVector, 1);
            // Vector2 movement = inputVector * moveSpeed;
            // Vector2 currentPos = new Vector2(this.transform.position.x, this.transform.position.y);
            // // Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
            //
            // SetDirection(movement);
        }

        #region Behavious

        private void Move(Vector2 direction)
        {
            animator.SetFloat("Speed", direction.magnitude);
            if (direction.sqrMagnitude < 0.01)
                return;
            var scaledMoveSpeed = moveSpeed * Time.deltaTime;
            transform.position += new Vector3(direction.x, 0, direction.y) * scaledMoveSpeed;
        }

        private void Rotate(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.01)
                return;

            var rotateDirection = new Vector3(direction.x, 0, direction.y);

            var rotateAngle = Vector3.SignedAngle(transform.forward, rotateDirection, Vector3.up);
            animator.SetFloat("MovingTurn", rotateAngle * Mathf.Deg2Rad * 0.8f);

            // transform.forward = Vector3.Slerp(transform.forward, rotateDirection, Time.deltaTime * 2f);
            // Or:
            // var rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));
            // transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);

        }   

        private void Fire()
        {
            var transform = this.transform;

            var newProjectile = Instantiate(projectile);
            newProjectile.transform.position = transform.position + transform.forward * 0.6f;
            newProjectile.transform.rotation = transform.rotation;
            const int size = 1;
            newProjectile.transform.localScale *= size;
            newProjectile.GetComponent<Rigidbody>().mass = Mathf.Pow(size, 3);
            newProjectile.GetComponent<Rigidbody>().AddForce(transform.forward * 20f, ForceMode.Impulse);
            newProjectile.GetComponent<MeshRenderer>().material.color =
                new Color(Random.value, Random.value, Random.value, 1.0f);
        }

        private IEnumerator BurstFire(int burstAmount)
        {
            for (var i = 0; i < burstAmount; ++i)
            {
                Fire();
                yield return new WaitForSeconds(0.1f);
            }
        }

        #endregion

        #region Input Events

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (context.interaction is SlowTapInteraction)
                    isCharging = true;
            }

            if (context.performed)
            {
                if (context.interaction is SlowTapInteraction)
                {
                    StartCoroutine(BurstFire((int)(context.duration * burstSpeed)));
                }
                else
                {
                    Fire();
                }
            }

            if (context.canceled)
            {
                isCharging = false;
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Debug.Log(@"Moveing...");
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Methods

        private void SetDirection(Vector2 direction)
        {
            if (direction.magnitude > .01f)
                lastDirection = DirectionToIndex(direction);

            animator.SetFloat("Direction", lastDirection);
        }

        //this function converts a Vector2 direction to an index to a slice around a circle
        //this goes in a counter-clockwise direction.
        private int DirectionToIndex(Vector2 dir, int sliceCount = 8)
        {
            Vector2 normDir = dir.normalized;
            float step = 360f / sliceCount;
            float angle = Vector2.SignedAngle(Vector2.down, normDir);
            if (angle < 0)
                angle += 360;
            float stepCount = angle / step;
            return Mathf.RoundToInt(stepCount);
        }

        #endregion

    }
}