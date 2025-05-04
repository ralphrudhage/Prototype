using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class EnemyManager: MonoBehaviour
    {
        private List<Enemy> allEnemies = new();

        public static EnemyManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterEnemy(Enemy enemy)
        {
            allEnemies.Add(enemy);
        }

        public void UnregisterEnemy(Enemy enemy)
        {
            allEnemies.Remove(enemy);
        }

        public IEnumerator TakeEnemyTurn()
        {
            foreach (var enemy in allEnemies)
            {
                yield return enemy.TakeTurn();
            }
        }
    }
}