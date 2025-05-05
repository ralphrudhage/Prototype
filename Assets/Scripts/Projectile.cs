using UnityEngine;

namespace DefaultNamespace
{
    public class Projectile : MonoBehaviour
    {
        private const float speed = 5f;
        private const float lifetime = 3f;
        private Vector2 direction;

        public void Initialize(Vector2 targetPosition)
        {
            direction = (targetPosition - (Vector2)transform.position).normalized;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // TODO: Call PlayerController.TakeDamage()
                Destroy(gameObject);
            }
        }
    }
}