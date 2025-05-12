using UnityEngine;

namespace DefaultNamespace
{
    public class Projectile : MonoBehaviour
    {
        private int damage;
        private const float speed = 10f;
        private const float lifetime = 3f;
        private Vector2 direction;
        private bool isEnemyTarget;
        private Player playerTarget;

        public void Initialize(Vector2 targetPosition, int targetDamage, bool isEnemy)
        {
            direction = (targetPosition - (Vector2)transform.position).normalized;
            damage = targetDamage;
            isEnemyTarget = isEnemy;
            Destroy(gameObject, lifetime);
        }
        
        public void Initialize(Player player, Vector2 targetPosition, int targetDamage, bool isEnemy)
        {
            playerTarget = player;
            direction = (targetPosition - (Vector2)transform.position).normalized;
            damage = targetDamage;
            isEnemyTarget = isEnemy;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isEnemyTarget)
            {
                if (other.CompareTag("Enemy"))
                {
                    other.gameObject.GetComponent<Enemy>().TakeDamage(damage);
                    Destroy(gameObject, 0.1f);
                }
            }
            else
            {
                if (other.CompareTag("Player"))
                {
                    var player = other.gameObject.GetComponent<Player>();
                    if (player == playerTarget)
                    {
                        player.TakeDamage(damage);
                        Destroy(gameObject, 0.1f);
                    }
                }
            }
        }
    }
}