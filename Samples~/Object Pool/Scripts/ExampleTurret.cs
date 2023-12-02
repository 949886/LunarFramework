using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Luna.Core.Pool
{
    // This is an example client that uses our simple object pool.
    public class ExampleTurret : MonoBehaviour
    {
        [Tooltip("Prefab to shoot")]
        [SerializeField] private GameObject projectile;
        [Tooltip("Prefab to shoot")]
        [SerializeField] private GameObject projectile2;
        [Tooltip("Projectile force")]
        [SerializeField] float muzzleVelocity = 700f;
        [Tooltip("End point of gun where shots appear")]
        [SerializeField] private Transform muzzlePosition;
        [Tooltip("Time between shots / smaller = higher rate of fire")]
        [SerializeField] float cooldownWindow = 0.1f;
        // [Tooltip("Reference to Object Pool")]
        // [SerializeField] ObjectPool objectPool;

        private float nextTimeToShoot;

        private Stopwatch stopwatch = new();
        private int count = 0;

        private void FixedUpdate()
        {
            // shoot if we have exceeded delay
            if (Input.GetButton("Fire1") && Time.time > nextTimeToShoot)
            {
                count++;

                // get a pooled object instead of instantiating

                stopwatch.Start();
                GameObject bulletObject = Luna.Core.Pool.ObjectPool.Get(count % 2 == 0 ? projectile : projectile2);
                stopwatch.Stop();

                if (bulletObject == null)
                    return;

                bulletObject.SetActive(true);

                // align to gun barrel/muzzle position
                bulletObject.transform.SetPositionAndRotation(muzzlePosition.position, muzzlePosition.rotation);

                // move projectile forward
                var rigidbody = bulletObject.GetComponent<Rigidbody>();
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.AddForce(bulletObject.transform.forward * muzzleVelocity, ForceMode.Acceleration);

                // turn off after a few seconds
                ExampleProjectile p = bulletObject.GetComponent<ExampleProjectile>();
                p?.Deactivate();

                // set cooldown delay
                nextTimeToShoot = Time.time + cooldownWindow;

            }

            if (Input.GetButton("Fire2"))
            {
                Stopwatch s = Stopwatch.StartNew();
                Random random = new Random();
                for (int i = 0; i < 1_000_000; i++)
                {
                    // Luna.Unity.Extensions.ObjectPool.Get(projectile);
                    random.Next();
                }

                s.Stop();
                Debug.Log(s.ElapsedMilliseconds);

                // 250-270ms for 1,000,000 loops, 5ms for the first 100 loops
                // 1.7ms for 1,000,000 empty loops.
            }

            if (Input.GetButton("Fire3"))
            {
                Debug.Log($"Shoot {count} for {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
}


