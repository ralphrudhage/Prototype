using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Managers;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject hpPos;
    [SerializeField] private GameObject infoPos;
    [SerializeField] private GameObject bloodParent;
    [SerializeField] private GameObject bloodPrefab;

    private int currentHp;
    private const int maxHp = 100;
    private const int maxActionPoints = 3;
    private const int rangedAttack = 6;
    private const int attackApCost = 2;
    private const int damage = 10;
    private int currentAP;

    private const float actionDelay = 0.25f;

    public Vector2Int currentGridPos;
    private TextSpawner textSpawner;
    private GameObject healthBar;
    private GameObject infoText;
    private Player player;
    
    public Vector2 targetPos;

    void Start()
    {
        EnemyManager.Instance.RegisterEnemy(this);
        
        currentHp = maxHp;
        currentAP = maxActionPoints;

        player = FindAnyObjectByType<Player>();
        textSpawner = FindAnyObjectByType<TextSpawner>();
        currentGridPos = GridManager.Instance.GetGridPositionFromWorld(transform.position);
        transform.position = GridManager.Instance.GetWorldPosition(currentGridPos);
        healthBar = textSpawner.SpawnHealthBar();
        infoText = textSpawner.SpawnInfoText("AP: + " + currentAP, infoPos.transform.position);
        
        targetPos = bloodParent.transform.position;
        
        RefreshEnemyUI();
    }

    public void TakeDamage(int damageTaken)
    {
        Instantiate(bloodPrefab, bloodParent.transform.position, Quaternion.identity);
        textSpawner.SpawnFloatingText(damage.ToString(), bloodParent.transform.position);
        currentHp -= damageTaken;
        RefreshEnemyUI();
        if (currentHp <= 0)
        {
            Debug.LogFormat("ENEMY DIED");
        }
    }

    private void RefreshEnemyUI()
    {
        infoText.transform.position = Camera.main.WorldToScreenPoint(infoPos.transform.position);
        infoText.GetComponent<TextMeshProUGUI>().text = $"AP: {currentAP}";
        
        healthBar.transform.position = Camera.main.WorldToScreenPoint(hpPos.transform.position);
        healthBar.GetComponent<HealthBar>().UpdateHp(currentHp, maxHp);
    }

    private void OnMouseDown()
    {
        ActionManager.Instance.SetCurrentEnemy(this);
        ActionManager.Instance.PerformAction();
    }

    private void OnDestroy()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.UnregisterEnemy(this);
    }

    public IEnumerator TakeTurn()
    {
        Debug.Log($"Enemy at {currentGridPos} starts turn...");
        currentAP = maxActionPoints;
        RefreshEnemyUI();
        
        while (currentAP > 0)
        {
            yield return new WaitForSeconds(actionDelay);

            if (CanAttackPlayer())
            {
                AttackPlayer();
                currentAP = 0;
                break;
            }

            if (currentAP >= 1)
            {
                yield return TryMoveTowardPlayer();
            }
            else
            {
                break;
            }
        }

        Debug.Log($"Enemy at {currentGridPos} ends turn.");
    }
    
    private bool CanAttackPlayer()
    {
        Vector2Int playerPos = player.currentGridPos;

        var dx = Mathf.Abs(currentGridPos.x - playerPos.x);
        var dy = Mathf.Abs(currentGridPos.y - playerPos.y);
        var manhattanDistance = dx + dy;

        var inRange = manhattanDistance <= rangedAttack;
        var hasLineOfSight = GridManager.Instance.HasLineOfSight(currentGridPos, playerPos);

        return inRange && hasLineOfSight && currentAP >= attackApCost;
    }

    private IEnumerator TryMoveTowardPlayer()
    {
        List<Vector2Int> path = GridManager.Instance.FindPath(currentGridPos, player.currentGridPos);

        if (path.Count > 0)
        {
            Vector2Int nextStep = path[0];
            yield return MoveTo(nextStep);
        }
        else
        {
            Debug.Log($"Enemy at {currentGridPos} can't find path to player");
            currentAP = 0;
            RefreshEnemyUI();
        }
    }
    
    private IEnumerator MoveTo(Vector2Int pos)
    {
        currentGridPos = pos;
        transform.position = GridManager.Instance.GetWorldPosition(pos);
        currentAP -= 1;
        RefreshEnemyUI();
        
        yield return new WaitForSeconds(actionDelay);
    }


    private void AttackPlayer()
    {
        var bullet = Instantiate(projectilePrefab, bloodParent.transform.position, Quaternion.identity);
        bullet.GetComponent<Projectile>().Initialize(Player.Instance.playerTarget.transform.position, damage);
        
        currentAP -= attackApCost;
        RefreshEnemyUI();
    }
}