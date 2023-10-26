using System.Collections;
using System.Collections.Generic;
using Luna.Core.Pool;
using UnityEngine;

namespace Luna.Extensions.VisualScripting
{
    public class ShootingProjectile : MonoBehaviour
    {
        public static void Shooting(GameObject bulletObject, Transform muzzlePosition, float velocity = 700f)
        {
            if (bulletObject == null)
                return;

            bulletObject.SetActive(true);

            // align to gun barrel/muzzle position
            bulletObject.transform.SetPositionAndRotation(muzzlePosition.position, muzzlePosition.rotation);

            // move projectile forward
            var rigidbody = bulletObject.GetComponent<Rigidbody>();
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.AddForce(bulletObject.transform.forward * velocity, ForceMode.Acceleration);

            // turn off after a few seconds
            ExampleProjectile p = bulletObject.GetComponent<ExampleProjectile>();
            p?.Deactivate();
        }
    }

}