using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private Vector2Int? markedTilePos = null; // Nullable, so null if no tile is marked
    private bool willExplodeNextTurn = false;


    public Vector2Int currentGridPos;

    private const int maxHp = 100;
    private int currentHp;

    private const int maxActionPoints = 3;
    private int currentAP;

    private const int rangedAttack = 100;
    private const int attackApCost = 2;
    private const int damage = 10;

    private const float actionDelay = 0.5f;

    private TextSpawner textSpawner;
    private GameObject healthBar;
    private GameObject infoText;
    private readonly List<Player> currentPlayerTargets = new();

    private Vector3 originalPosition;

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

        originalPosition = transform.position;

        RefreshEnemyUI();
    }

    public void TakeDamage(int damageTaken)
    {
        Instantiate(bloodPrefab, bloodParent.transform.position, Quaternion.identity);
        textSpawner.SpawnFloatingText(damageTaken.ToString(), bloodParent.transform.position, GameUtils.lightRed);
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

        infoText.transform.position = infoPos.transform.position;
        infoText.GetComponent<TextMeshProUGUI>().text = $"AP: {currentAP}";

        healthBar.transform.position = hpPos.transform.position;
        healthBar.GetComponent<HealthBar>().UpdateHp(currentHp, maxHp);
    }

    private void OnMouseDown()
    {
        if (!IsTargetableByPlayer()) return;

        CardManager.Instance.SetCurrentEnemy(this);
        CardManager.Instance.PerformAction();
    }

    private bool IsTargetableByPlayer()
    {
        return IsInRange(PartyManager.Instance.currentPlayer.currentGridPos,
                   PartyManager.Instance.currentPlayer.GetCurrentRange()) &&
               HasStrictLineOfSight(PartyManager.Instance.currentPlayer.currentGridPos, currentGridPos);
    }

    private void FindPlayerTargets()
    {
        currentPlayerTargets.Clear();

        var players = GameObject.FindGameObjectsWithTag("Player")
            .Select(go => go.GetComponent<Player>())
            .Where(p => p != null)
            .ToList();

        Debug.LogFormat("found {0} players", players.Count);

        foreach (var player in players)
        {
            if (IsInRange(player.currentGridPos, rangedAttack) &&
                HasStrictLineOfSight(currentGridPos, player.currentGridPos) &&
                currentAP >= attackApCost)
            {
                currentPlayerTargets.Add(player);
                Debug.LogFormat("added {0} to targetList", player.playerClass);
            }
        }
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
                Debug.LogWarning(
                    $"LOS blocked immediately due to corner clipping between {origin} and {target} via {corner1} & {corner2}");
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
                Vector2Int check1 = new Vector2Int(previousTile.x + dx, previousTile.y); // Horizontal neighbor
                Vector2Int check2 = new Vector2Int(previousTile.x, previousTile.y + dy); // Vertical neighbor

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

        if (willExplodeNextTurn && markedTilePos.HasValue)
        {
            ExplodeMarkedTile();
            willExplodeNextTurn = false;
            markedTilePos = null;
            yield return new WaitForSeconds(actionDelay);
        }

        while (currentAP > 0)
        {
            yield return new WaitForSeconds(actionDelay);

            bool attacked = AttackPlayer();

            if (currentAP <= 0)
                break;

            if (attacked)
            {
                // Successful attack
                if (currentAP >= 1)
                {
                    Debug.Log($"Enemy had AP left after attack, marking tile for explosion.");
                    MarkTileNearPlayer();
                    willExplodeNextTurn = true;
                    currentAP = 0; // After marking, end turn
                }

                break; // End after attack
            }

            // Failed to attack ➔ try moving
            Debug.Log($"Enemy failed to attack, trying to move.");
            yield return TryMoveTowardPlayer();

            if (currentAP <= 0)
                break;

            yield return new WaitForSeconds(actionDelay);

            // Try attack again after moving
            attacked = AttackPlayer();

            if (currentAP <= 0)
                break;

            if (!attacked)
            {
                Debug.Log($"Enemy failed attack after move, marking tile for explosion.");
                MarkTileNearPlayer();
                willExplodeNextTurn = true;
                currentAP = 0; // After marking, end turn
            }

            break; // Whether second attack worked or not, end after
        }

        Debug.Log($"Enemy at {currentGridPos} ends turn with {currentAP} AP remaining.");
    }

    private void MarkTileNearPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player")
            .Select(go => go.GetComponent<Player>())
            .Where(p => p != null)
            .ToList();

        foreach (var player in players)
        {
            int dx = Mathf.Abs(currentGridPos.x - player.currentGridPos.x);
            int dy = Mathf.Abs(currentGridPos.y - player.currentGridPos.y);
            bool adjacentToEnemy = Mathf.Max(dx, dy) <= 1;

            if (adjacentToEnemy)
                continue; // skip players adjacent to enemy (want ranged ones)

            // Pick one of the player's adjacent tiles
            List<Vector2Int> adjacentTiles = GridManager.Instance.GetNeighbors(player.currentGridPos);

            foreach (var tilePos in adjacentTiles)
            {
                if (!GridManager.Instance.IsWalkable(tilePos))
                    continue;

                if (!IsOccupied(tilePos))
                {
                    markedTilePos = tilePos;
                    Debug.Log($"Enemy marks tile at {tilePos}!");

                    if (GridManager.Instance.TryGetDynamicTile(tilePos, out var tile))
                    {
                        tile.MarkTileAsTarget();
                    }

                    return;
                }
            }
        }

        Debug.Log("Couldn't find tile to mark.");
    }

    private void ExplodeMarkedTile()
    {
        if (!markedTilePos.HasValue)
            return;

        Vector2Int pos = markedTilePos.Value;

        Debug.Log($"Exploding tile at {pos}!");

        // Check if a player is standing there
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(pos);
        var hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null && hit.GetComponent<Player>() != null)
        {
            Player player = hit.GetComponent<Player>();
            player.TakeDamage(15); // Arbitrary explosion damage
            Debug.Log($"Player {player.playerClass} hit by explosion!");
        }

        // Optionally reset the tile visual
        if (GridManager.Instance.TryGetDynamicTile(pos, out var tile))
        {
            tile.ResetTile();
        }

        // Tell FxManager to explode at that center
        FindAnyObjectByType<FxManager>().ExplodeAt(pos);
    }


    private Player FindClosestPlayerTarget()
    {
        var players = GameObject.FindGameObjectsWithTag("Player")
            .Select(go => go.GetComponent<Player>())
            .Where(p => p != null)
            .ToList();

        if (players.Count == 0)
        {
            Debug.LogWarning("No players found!");
            return null;
        }

        Player closest = players
            .OrderBy(p => Vector2Int.Distance(currentGridPos, p.currentGridPos))
            .First();

        Debug.Log($"Enemy selected target {closest.playerClass} at {closest.currentGridPos}");

        return closest;
    }

    private IEnumerator TryMoveTowardPlayer()
    {
        if (currentAP <= 0) yield break;

        Player closestPlayer = FindClosestPlayerTarget();
        if (closestPlayer == null) yield break;

        Vector2Int playerPos = closestPlayer.currentGridPos;

        Vector2Int? bestMove = null;
        float bestDistance = float.MaxValue;

        foreach (Vector2Int neighbor in GridManager.Instance.GetNeighbors(currentGridPos))
        {
            if (!GridManager.Instance.IsWalkable(neighbor))
                break;

            if (IsOccupied(neighbor))
                break;

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
            if (!GridManager.Instance.IsWalkable(path[i]) || IsOccupied(path[i]))
            {
                Debug.Log($"Blocked at {path[i]}, stopping pathing");
                break;
            }

            yield return MoveTo(path[i]);
        }

        RefreshEnemyUI();
    }


    private IEnumerator MoveTo(Vector2Int pos)
    {
        currentGridPos = pos;
        transform.position = GridManager.Instance.GetWorldPosition(pos);
        originalPosition = transform.position;
        currentAP -= 1;
        RefreshEnemyUI();

        yield return new WaitForSeconds(actionDelay);
    }


    private bool AttackPlayer()
    {
        Debug.Log($"Enemy attacking players...");

        FindPlayerTargets();

        if (currentPlayerTargets.Count == 0)
        {
            Debug.Log("No targets found.");
            return false; // No attack performed
        }

        // Check for any adjacent player first
        var adjacentPlayer = currentPlayerTargets.FirstOrDefault(player =>
        {
            int dx = Mathf.Abs(currentGridPos.x - player.currentGridPos.x);
            int dy = Mathf.Abs(currentGridPos.y - player.currentGridPos.y);
            return Mathf.Max(dx, dy) == 1;
        });

        if (adjacentPlayer != null)
        {
            Debug.Log($"Melee attacking {adjacentPlayer.playerClass}!");
            StartCoroutine(PerformMeleeAttack(adjacentPlayer));
            return true; // Attack performed
        }

        // No adjacent players ➔ shoot all ranged targets
        foreach (var player in currentPlayerTargets)
        {
            Debug.Log($"Ranged attacking {player.playerClass}!");
            var bullet = Instantiate(projectilePrefab, bloodParent.transform.position, Quaternion.identity);
            bullet.GetComponent<Projectile>()
                .Initialize(player, player.playerTarget.transform.position, damage, false);
        }

        currentAP -= attackApCost;
        RefreshEnemyUI();
        return true; // Attack performed
    }

    private IEnumerator PerformMeleeAttack(Player target)
    {
        yield return MeleeAttackAnimation(target.transform.position);

        target.TakeDamage(damage);

        currentAP -= attackApCost;
        RefreshEnemyUI();
    }

    private IEnumerator MeleeAttackAnimation(Vector2 enemyPosition)
    {
        Vector3 attackDirection = (enemyPosition - (Vector2)originalPosition).normalized;
        Vector3 attackPosition = originalPosition + attackDirection * 0.4f; // 0.3 units forward

        float moveDuration = 0.1f; // fast lunge
        float elapsedTime = 0f;

        // Move forward
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, attackPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = attackPosition;

        elapsedTime = 0f;

        // Move back
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(attackPosition, originalPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    private bool IsOccupied(Vector2Int pos)
    {
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(pos);
        var hit = Physics2D.OverlapPoint(worldPos);
        return hit != null && (hit.GetComponent<Enemy>() != null || hit.GetComponent<Player>() != null);
    }
}