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

    public Vector2 targetPos;

    void Start()
    {
        EnemyManager.Instance.RegisterEnemy(this);

        currentHp = maxHp;
        currentAP = maxActionPoints;

        textSpawner = FindAnyObjectByType<TextSpawner>();
        currentGridPos = GridManager.Instance.GetGridPositionFromWorld(transform.position);
        transform.position = GridManager.Instance.GetWorldPosition(currentGridPos);
        healthBar = textSpawner.SpawnHealthBar();
        infoText = textSpawner.SpawnInfoText("AP: + " + currentAP, infoPos.transform.position);

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
        targetPos = bloodParent.transform.position;

        infoText.transform.position = Camera.main.WorldToScreenPoint(infoPos.transform.position);
        infoText.GetComponent<TextMeshProUGUI>().text = $"AP: {currentAP}";

        healthBar.transform.position = Camera.main.WorldToScreenPoint(hpPos.transform.position);
        healthBar.GetComponent<HealthBar>().UpdateHp(currentHp, maxHp);
    }

    private void OnMouseDown()
    {
        if (!IsTargetableByPlayer()) return;

        ActionManager.Instance.SetCurrentEnemy(this);
        ActionManager.Instance.PerformAction();
    }

    private bool IsTargetableByPlayer()
    {
        return IsInRange(Player.Instance.currentGridPos, Player.Instance.GetCurrentRange()) &&
               HasStrictLineOfSight(Player.Instance.currentGridPos, currentGridPos);
    }

    private bool CanAttackPlayer()
    {
        return IsInRange(Player.Instance.currentGridPos, rangedAttack) &&
               HasStrictLineOfSight(currentGridPos, Player.Instance.currentGridPos) &&
               currentAP >= attackApCost;
    }

    private bool IsInRange(Vector2Int origin, int range)
    {
        int dx = Mathf.Abs(currentGridPos.x - origin.x);
        int dy = Mathf.Abs(currentGridPos.y - origin.y);
        int manhattanDistance = dx + dy;
        return manhattanDistance <= range;
    }
    
    private bool HasStrictLineOfSight(Vector2Int origin, Vector2Int target)
    {
        Vector3 worldStart = GridManager.Instance.GetWorldPosition(origin);
        Vector3 worldEnd = GridManager.Instance.GetWorldPosition(target);

        Vector3 direction = (worldEnd - worldStart).normalized;
        float stepSize = 0.1f;

        Vector3 current = worldStart;
        Vector2Int previousTile = origin;
        
        // 🔒 Block direct diagonal LOS if adjacent corners are blocked
        int dx0 = target.x - origin.x;
        int dy0 = target.y - origin.y;

        if (Mathf.Abs(dx0) == 1 && Mathf.Abs(dy0) == 1)
        {
            Vector2Int corner1 = new Vector2Int(origin.x + dx0, origin.y);
            Vector2Int corner2 = new Vector2Int(origin.x, origin.y + dy0);

            if (!GridManager.Instance.IsWalkable(corner1) || !GridManager.Instance.IsWalkable(corner2))
            {
                Debug.LogWarning($"LOS blocked immediately due to corner clipping between {origin} and {target} via {corner1} & {corner2}");
                return false;
            }
        }

        while (Vector3.Distance(current, worldEnd) > stepSize)
        {
            current += direction * stepSize;
            Vector2Int tile = GridManager.Instance.GetGridPositionFromWorld(current);

            if (tile == origin || tile == target || tile == previousTile)
                continue;

            if (!GridManager.Instance.IsWithinBounds(tile) || !GridManager.Instance.IsWalkable(tile))
            {
                Debug.Log($"LOS blocked at {tile} — not walkable");
                return false;
            }

            // Detect diagonal transition from previous tile
            int dx = tile.x - previousTile.x;
            int dy = tile.y - previousTile.y;

            if (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) == 1)
            {
                Vector2Int check1 = new Vector2Int(previousTile.x + dx, previousTile.y);     // Horizontal neighbor
                Vector2Int check2 = new Vector2Int(previousTile.x, previousTile.y + dy);     // Vertical neighbor

                if (!GridManager.Instance.IsWalkable(check1) || !GridManager.Instance.IsWalkable(check2))
                {
                    Debug.Log($"LOS blocked at {tile} due to corner clipping: {check1}, {check2}");
                    return false;
                }
            }

            previousTile = tile;
        }

        return true;
    }



    /*
    private bool HasStrictLineOfSight(Vector2Int origin, Vector2Int target)
    {
        Vector2Int delta = target - origin;
        int dx = Mathf.Abs(delta.x);
        int dy = Mathf.Abs(delta.y);
        int sx = delta.x > 0 ? 1 : -1;
        int sy = delta.y > 0 ? 1 : -1;

        int x = origin.x;
        int y = origin.y;
        int err = dx - dy;

        bool firstStep = true;

        while (true)
        {
            if (x == target.x && y == target.y)
                break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }

            var current = new Vector2Int(x, y);
            
            Debug.DrawLine(GridManager.Instance.GetWorldPosition(origin), GridManager.Instance.GetWorldPosition(target), Color.red, 1.0f);
            Debug.Log($"LOS Ray visiting tile: {current}");


            if (!GridManager.Instance.IsWithinBounds(current) || !GridManager.Instance.IsWalkable(current))
            {
                Debug.Log($"LOS blocked directly at {current} — not walkable");
                return false;
            }


            if (!firstStep && Mathf.Abs(x - origin.x) == Mathf.Abs(y - origin.y))
            {
                Vector2Int check1 = new Vector2Int(x, y - sy);
                Vector2Int check2 = new Vector2Int(x - sx, y);
                
                Debug.LogFormat("check1 {0}", check1);
                Debug.LogFormat("check2 {0}", check2);

                if (!GridManager.Instance.IsWalkable(check1) || !GridManager.Instance.IsWalkable(check2))
                {
                    Debug.LogWarning($"LOS blocked at {x},{y} due to BOTH corners blocked:");
                    return false;
                }
            }


            firstStep = false; // 👈 moved here to prevent corner logic on first step
        }

        return true;
    }*/

    
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

    private IEnumerator TryMoveTowardPlayer()
    {
        if (currentAP <= 0) yield break;

        Vector2Int playerPos = Player.Instance.currentGridPos;
        Vector2Int? bestMove = null;
        float bestDistance = float.MaxValue;

        foreach (Vector2Int neighbor in GridManager.Instance.GetNeighbors(currentGridPos))
        {
            if (!GridManager.Instance.IsWalkable(neighbor))
                continue;

            if (HasStrictLineOfSight(neighbor, playerPos))
            {
                float distance = Vector2Int.Distance(neighbor, playerPos);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestMove = neighbor;
                }
            }
        }

        if (bestMove.HasValue)
        {
            Debug.Log($"Moving to {bestMove.Value} for best LOS");
            yield return MoveTo(bestMove.Value);
            yield break;
        }

        // Fallback to normal pathfinding if no LOS found
        List<Vector2Int> path = GridManager.Instance.FindPath(currentGridPos, playerPos);

        if (path.Count == 0)
        {
            Debug.Log($"Enemy at {currentGridPos} can't find path to player");
            currentAP = 0;
            RefreshEnemyUI();
            yield break;
        }

        int maxSteps = Mathf.Min(currentAP, path.Count);

        for (int i = 0; i < maxSteps && currentAP > 0; i++)
        {
            yield return MoveTo(path[i]);
        }

        RefreshEnemyUI();
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
        bullet.GetComponent<Projectile>().Initialize(Player.Instance.playerTarget.transform.position, damage, false);

        currentAP -= attackApCost;
        RefreshEnemyUI();
    }
}