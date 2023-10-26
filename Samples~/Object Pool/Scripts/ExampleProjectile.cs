using System.Collections;
using UnityEngine;

namespace Luna.Core.Pool
{
    // A projectile used for the object pool elements.
    public class ExampleProjectile : MonoBehaviour
    {
        // deactivate after delay
        [SerializeField] private float timeoutDelay = 3f;
        

        private void Awake()
        {

        }

        public void Deactivate()
        {
            StartCoroutine(DeactivateRoutine(timeoutDelay));
        }

        IEnumerator DeactivateRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            // reset the moving Rigidbody
            Rigidbody rBody = GetComponent<Rigidbody>();
            rBody.velocity = new Vector3(0f, 0f, 0f);
            rBody.angularVelocity = new Vector3(0f, 0f, 0f);

            // set inactive and return to pool
            // pooledObject?.Release();
            gameObject.SetActive(false);
        }
    }
}
