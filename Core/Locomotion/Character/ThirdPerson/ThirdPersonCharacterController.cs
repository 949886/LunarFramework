﻿using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using Luna.Extensions;
using Luna.Extensions.Unity;
using Luna.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Random = UnityEngine.Random;

namespace Luna.Core.Locomotion.Character
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayableDirector))]
    public class ThirdPersonCharacterController : MonoBehaviour, ThirdPersonInput.IGameplayActions
    {
        public float moveSpeed = 5f;
        public Transform moveTarget;
        
        public float burstSpeed;
        
        public GameObject weapon;
        public GameObject projectile;

        protected Animator _animator;
        protected ThirdPersonCharacterAttackBehaviour _attackBehaviour;
        
        private ThirdPersonInput inputs;
        private bool isCharging;
        private int lastDirection;
        
        private Rigidbody _rigidbody;
        private CapsuleCollider _capsuleCollider;
        
        private PlayableDirector _playableDirector;
        private TimelineAsset _timelineAsset;
        private IMarker[] _markers = new IMarker[] {};
        
        private Vector3 _deltaPosition;

        
        public bool IsIdle => _animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle");

        
        protected virtual void Awake()
        {
            inputs = new ThirdPersonInput();
            inputs.Gameplay.AddCallbacks(this);
            
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
            _capsuleCollider = GetComponent<CapsuleCollider>();
            
            _animator = GetComponent<Animator>(); 
            _attackBehaviour = _animator.GetBehaviour<ThirdPersonCharacterAttackBehaviour>();
            _attackBehaviour.weapon = weapon;
            
            _playableDirector = GetComponent<PlayableDirector>();
            _timelineAsset = _playableDirector.playableAsset as TimelineAsset;
            if (_timelineAsset) _markers = _timelineAsset.markerTrack.GetMarkers().ToArray();
        }

        protected virtual void OnEnable()
        {
            if (moveTarget == null)
            {
                moveTarget = transform;
            }
            inputs.Enable();
        }
        
        protected virtual void Start()
        {
#if UNITY_2022_3_OR_NEWER
            Physics.simulationMode = SimulationMode.Script;
#endif
        }

        protected virtual  void OnDisable()
        {
            inputs.Disable();
        }

        protected virtual  void Update()
        {
            var move = inputs.Gameplay.Move.ReadValue<Vector2>();
            var look = inputs.Gameplay.Look.ReadValue<Vector2>();
            var sprint = inputs.Gameplay.Sprint.ReadValue<float>();

            if (sprint > 0.5) move *= 2;

            // Update orientation first, then move.
            // Otherwise move orientation will lag behind by one frame.
            // Look(look);
            // SetDirection(move);
            Move(move); 
            Rotate(move);
            
            UpdateState();
        }

        private void UpdateState()
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsTag("Attack"))
            {
                
            }
            else
            {
                // _attackBehaviour.attackIndex = 0;
                // animator.SetInteger("Attack Index", 0);
            }
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
            
            Physics.Simulate(Time.deltaTime);
            moveTarget.position += _deltaPosition;
            _deltaPosition = Vector3.zero;
        }

        private void OnAnimatorMove()
        {
            // Debug.Log("onAnimatorMove");
            // Vector3 newPosition = transform.position;
            // newPosition.x += animator.deltaPosition.x;
            // newPosition.z += animator.deltaPosition.z;
            // transform.position = newPosition;
            
            // Debug.Log($"onAnimatorMove {moveTarget}");
            // _deltaPosition = animator.deltaPosition;
            moveTarget.position += _animator.deltaPosition; 
        }
        
        #region Behavious

        private void Move(Vector2 direction)
        {
            _animator.SetFloat("Speed", direction.magnitude * moveSpeed);
            if (direction.sqrMagnitude < 0.01)
                return;
            var scaledMoveSpeed = moveSpeed * Time.deltaTime;
            // Debug.Log("direction: " + direction + " scaledMoveSpeed: " + scaledMoveSpeed + " position: " + transform.position);
            // transform.position += new Vector3(direction.x, 0, direction.y) * scaledMoveSpeed;
        }

        private void Rotate(Vector2 direction)
        {
            if (_attackBehaviour.attackIndex > 0)
                return;
            
            // World space rotation:
            // var rotateDirection = direction.sqrMagnitude < 0.01 ? transform.forward : new Vector3(direction.x, 0, direction.y);
            // var rotateAngle = Vector3.SignedAngle(transform.forward, rotateDirection, Vector3.up);
            // var rotateRadians = rotateAngle * Mathf.Deg2Rad;
            
            // Free rotation 1:
            var rotateDirection = Vector3.ProjectOnPlane(
                Camera.main.transform.rotation * new Vector3(direction.x, 0, direction.y), 
                Vector3.up
                ).normalized;
            var rotateAngle = Vector3.SignedAngle(transform.forward, rotateDirection, Vector3.up);
            var rotateRadians = rotateAngle * Mathf.Deg2Rad;
            
            // Free rotation 2:
            // var rotateAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(direction.x, 0, direction.y), Vector3.up);
            // var cameraAngle = Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up), Vector3.up);
            // Debug.Log("rotateAngle: " + rotateAngle + " cameraAngle: " + cameraAngle);
            // Debug.Log("rotation: " + transform.rotation.eulerAngles.y + " angle: " + ((cameraAngle + rotateAngle - 180f).Mod(360f) - 180));
            // var angle = transform.rotation.eulerAngles.y > 180f ? transform.rotation.eulerAngles.y - 360f : transform.rotation.eulerAngles.y;
            // rotateAngle = -angle + ((cameraAngle + rotateAngle + 180f).Mod(360f) - 180f); 
            // Debug.Log("rotateAngle: " + rotateAngle);
            // var rotateRadians = rotateAngle * Mathf.Deg2Rad;
            
            _animator.SetFloat("MovingTurn", rotateRadians);
            
            // transform.forward = Vector3.Slerp(transform.forward, rotateDirection, Time.deltaTime * 15f);
            // Or:
            // var rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));
            // transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            
            // Rotate and tilt the player based on the turning speed.
            var eulerAngles = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(
                eulerAngles.x, 
                Mathf.Lerp(eulerAngles.y, eulerAngles.y + rotateAngle, Time.deltaTime * 10f), // Rotate
                Mathf.Lerp(eulerAngles.z > 180f ? eulerAngles.z - 360f : eulerAngles.z, -rotateRadians * 6f, Time.deltaTime * 10f)); // Tilt
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

        public void Attack()
        {
            _animator.SetBool("Attack", true);
        }
        
        public void HeavyAttack()
        {
            _animator.SetBool("Extra Attack", true);
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
            // var input = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Debug.Log(@"Sprinting..." + context);
                _animator.SetTrigger("Sprint");
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            
        }

        public async void OnAttack(InputAction.CallbackContext context)
        {
            // Debug.Log(@"Attacking..." + context);
            
            if (context.interaction is SlowTapInteraction)
            {
                if (context.started)
                {
                    HeavyAttack();
                }

                if (context.performed)
                {
                    
                }

                if (context.canceled)
                {
                    // _animator.SetBool("Extra Attack", false);
                }
            }
            else
            {
                if (context.started)
                {

                }

                if (context.performed)
                {
                    // var state = _animator.GetCurrentAnimatorStateInfo(0);
                    // var stateName = _animator.GetCurrentStateName(0);
                    Attack();
                    
                    // _playableDirector.Play();
                    // _playableDirector.time = _markers[0].time;
                    // await UniTask.WaitForSeconds(0.1f); 
                    // await UniTask.NextFrame(); 
                    
                    // await UniTask.DelayFrame(5);
                    // animator.SetBool("Attack", false);
                    // Time.timeScale = 0.1f;
                    // await UniTask.WaitForSeconds(1f); 
                    // Time.timeScale = 1f;
                }

                if (context.canceled)
                {
                    // animator.SetBool("Attack", false);
                }
            }
        }

        public void OnExtraAttack(InputAction.CallbackContext context)
        {
            // Debug.Log("Extra Attack");
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            
        }

        public void OnPickup(InputAction.CallbackContext context)
        {
            
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            
        }

        public void OnMouse(InputAction.CallbackContext context)
        {
            
        }

        public void OnSkill1(InputAction.CallbackContext context)
        {
            
        }

        public void OnSkill2(InputAction.CallbackContext context)
        {
            
        }

        public void OnSkill3(InputAction.CallbackContext context)
        {
            
        }

        public void OnSlot1(InputAction.CallbackContext context)
        {
            
        }

        public void OnSlot2(InputAction.CallbackContext context)
        {
            
        }

        public void OnSlot3(InputAction.CallbackContext context)
        {
            
        }

        public void OnSlot4(InputAction.CallbackContext context)
        {
            
        }

        public void OnSlot5(InputAction.CallbackContext context)
        {
            
        }

        public void OnSlot6(InputAction.CallbackContext context)
        {
            
        }

        public void OnSlot7(InputAction.CallbackContext context)
        {
            
        }

        public void OnSlot8(InputAction.CallbackContext context)
        {
            
        }

        public void OnSlot9(InputAction.CallbackContext context)
        {
            
        }

        public void OnSlot0(InputAction.CallbackContext context)
        {
            
        }

        #endregion

        #region Collision Events
        
        private void OnCollisionEnter(Collision other)
        {
            // Debug.Log("OnCollisionEnter");
        }
        
        private void OnCollisionStay(Collision other)
        {
            // Debug.Log("OnCollisionStay");
        }
        
        private void OnCollisionExit(Collision other)
        {
            // Debug.Log("OnCollisionExit");
        }

        #endregion
        
        #region Methods

        private void SetDirection(Vector2 direction)
        {
            if (direction.magnitude > .01f)
                lastDirection = DirectionToIndex(direction);

            _animator.SetFloat("Direction", lastDirection);
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

        public void SyncEffectPosition()
        {
            // swordSlashEffect.SetTransform(transform);
        }

        #endregion

        
    }
}