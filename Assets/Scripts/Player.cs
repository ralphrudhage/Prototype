using System;
using System.Collections.Generic;
using DefaultNamespace;
using Managers;
using Model;
using Model.MageCards;
using Model.PriestCards;
using Model.WarriorCards;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject playerCircle;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject hpPos;
    [SerializeField] public GameObject playerTarget;
    [SerializeField] private GameObject bloodPrefab;

    public Vector2Int currentGridPos;

    private const int maxHp = 100;
    private int currentHp;

    private const int maxActionPoints = 3;
    private int currentAP;

    private const int currentRange = 6;

    private TextSpawner textSpawner;
    private PlayerInfo playerInfo;
    private GameObject playerInfoObject;
    public PlayerClass playerClass;
    public Queue<Card> drawPile = new();
    public List<Card> hand = new();
    public List<Card> discarded = new();
    
    private Vector3 originalPosition;
    private bool isDragging;

    private void OnEnable()
    {
        // test
        currentHp = 50;

        // currentHp = maxHp;
        currentAP = maxActionPoints;

        textSpawner = FindAnyObjectByType<TextSpawner>();

        playerInfoObject = textSpawner.SpawnPlayerInfo();
        playerInfo = playerInfoObject.GetComponent<PlayerInfo>();
        playerInfo.BarColor(GameUtils.lightBlue);

        InitializeDeck();
    }

    void Start()
    {
        currentGridPos = GridManager.Instance.GetGridPositionFromWorld(transform.position);
        transform.position = GridManager.Instance.GetWorldPosition(currentGridPos);
        
        originalPosition = transform.position;

        RefreshPlayerUI();

        Debug.Log($"{playerClass} set at {currentGridPos}");
    }

    private void LateUpdate()
    {
        if (isDragging)
        {
            playerInfoObject.transform.position = Camera.main.WorldToScreenPoint(hpPos.transform.position);
        }
    }

    private void InitializeDeck()
    {
        List<Card> actions = new();

        switch (playerClass)
        {
            case PlayerClass.WARRIOR:
                actions.AddRange(new Card[]
                {
                    new Charge("Charge", 1, 1, 3),
                    new Charge("Charge", 1, 1, 3),
                    new Charge("Charge", 1, 1, 3),
                    new Charge("Charge", 1, 1, 3),
                    new Charge("Charge", 1, 1, 3),
                    new Strike("Strike", 1, 1, 1),
                    new Strike("Strike", 1, 1, 1),
                    new Strike("Strike", 1, 1, 1),
                    new Strike("Strike", 1, 1, 1),
                    new Strike("Strike", 1, 1, 1),
                    new Strike("Strike", 1, 1, 1),
                });
                break;

            case PlayerClass.MAGE:
                actions.AddRange(new Card[]
                {
                    new Fireball("Fireball", 1, 20, 6),
                    new Fireball("Fireball", 1, 20, 6),
                    new Fireball("Fireball", 1, 20, 6),
                    new Fireball("Fireball", 1, 20, 6),
                    new Fireball("Fireball", 1, 20, 6),
                    new Fireball("Fireball", 1, 20, 6),
                    new Curse("Curse", 2, 20, 6, 3),
                    new Curse("Curse", 2, 20, 6, 3),
                    new Curse("Curse", 2, 20, 6, 3),
                    new Curse("Curse", 2, 20, 6, 3),
                    new Curse("Curse", 2, 20, 6, 3),
                    new Curse("Curse", 2, 20, 6, 3)
                });
                break;

            case PlayerClass.PRIEST:
                actions.AddRange(new Card[]
                {
                    new Heal("Heal", 1, 20, 6),
                    new Heal("Heal", 1, 20, 6),
                    new Heal("Heal", 1, 20, 6),
                    new Heal("Heal", 1, 20, 6),
                    new Heal("Heal", 1, 20, 6),
                    new Heal("Heal", 1, 20, 6),
                    new Wand("Wand", 1, 5, 6),
                    new Wand("Wand", 1, 5, 6),
                    new Wand("Wand", 1, 5, 6),
                    new Wand("Wand", 1, 5, 6),
                    new Wand("Wand", 1, 5, 6),
                    new Wand("Wand", 1, 5, 6)
                });
                break;
        }

        actions.Shuffle();
        drawPile = new Queue<Card>(actions);
    }

    private void RefreshPlayerUI()
    {
        PlayerUI.Instance.UpdatePlayerUI(this);

        playerInfoObject.transform.position = Camera.main.WorldToScreenPoint(hpPos.transform.position);
        playerInfo.UpdatePlayerInfo(currentHp, maxHp, currentAP);
    }

    public void TakeDamage(int damageTaken)
    {
        Instantiate(bloodPrefab, playerTarget.transform.position, Quaternion.identity);
        textSpawner.SpawnFloatingText(damageTaken.ToString(), playerTarget.transform.position, GameUtils.lightRed);
        currentHp -= damageTaken;
        RefreshPlayerUI();
        if (currentHp <= 0)
        {
            Debug.Log($"{playerClass} DIED");
        }
    }

    public void SetGridPosition(Vector2Int gridPos)
    {
        currentGridPos = gridPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPos);
        RefreshPlayerUI();

        Debug.Log($"{playerClass} set at {currentGridPos}");
    }

    public void UseAP(int actionPoint)
    {
        currentAP -= actionPoint;
        RefreshPlayerUI();
    }

    public int GetCurrentAP()
    {
        return currentAP;
    }

    public int GetCurrentRange()
    {
        return currentRange;
    }

    public void ResetAP()
    {
        currentAP = 3;
        RefreshPlayerUI();
    }

    public void AttackEnemy(Vector2 enemyPosition, int damage)
    {
        var bullet = Instantiate(projectilePrefab, playerTarget.transform.position, Quaternion.identity);
        bullet.GetComponent<Projectile>().Initialize(enemyPosition, damage, true);
    }

    public void CastSpell(Card card)
    {
        Debug.LogFormat("Player {0} casts spell {1}", playerClass, card.name);
    }

    public void CardEffect(Card card)
    {
        Debug.LogFormat("Player {0} receives card effect {1} with effect {2}", playerClass, card.name, card.effect);
        currentHp += card.effect;
        textSpawner.SpawnFloatingText(card.effect.ToString(), playerTarget.transform.position, GameUtils.lightYellow);
        RefreshPlayerUI();
    }
    
    private void OnMouseDown()
    {
        PartyManager.Instance.SetCurrentPlayer(this);
        Debug.Log($"{playerClass} selected");
        
        if (PartyManager.Instance.currentPlayer.GetCurrentAP() <= 0) return;
        
        isDragging = true;
        originalPosition = transform.position;

        GridManager.Instance.ShowMoveCircles(PartyManager.Instance.currentPlayer.currentGridPos, 3);
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0f;
        transform.position = newPosition;
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        Vector3 droppedWorldPos = transform.position;
        Vector2Int droppedGridPos = GridManager.Instance.GetGridPositionFromWorld(droppedWorldPos);

        if (GridManager.Instance.IsValidMoveTile(droppedGridPos))
        {
            PartyManager.Instance.currentPlayer.SetGridPosition(droppedGridPos);
            PartyManager.Instance.currentPlayer.UseAP(1);
        }
        else
        {
            transform.position = originalPosition;
            RefreshPlayerUI();
        }

        GridManager.Instance.ClearHighlights();
    }

    public void SelectedCircle()
    {
        var circles = GameObject.FindGameObjectsWithTag("circle");
        foreach (var circle in circles) circle.SetActive(false);
        playerCircle.SetActive(true);
    }

    public void Deselect()
    {
        playerCircle.SetActive(false);
    }
}