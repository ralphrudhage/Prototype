using System.Collections;
using Model;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class CardManager : MonoBehaviour
    {
        [SerializeField] private GameObject actionsParent;
        [SerializeField] private GameObject actionPrefab;

        private Player currentPlayer;
        private SelectedCard currentCard;
        private Enemy currentEnemy;
        private Player currentPartyPlayer;
        private int currentTurn;
        
        public static CardManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            actionsParent.transform.DestroyTagRecursively("action");
        }
        
        public void EndTurn()
        {
            DestroyAllCardsFromUI();

            foreach (var player in PartyManager.Instance.GetAllPlayers())
            {
                foreach (var action in player.hand)
                {
                    player.discarded.Add(action);
                    Debug.Log($"{player.playerClass} added {action} to discard pile {player.discarded.Count}");
                }

                player.Deselect();
                player.hand.Clear();
            }

            StartCoroutine(PartyManager.Instance.EnemyTurnThenDraw());
        }
        
        public void DiscardSelectedCard(Card action)
        {
            var player = PartyManager.Instance.currentPlayer;
            player.discarded.Add(action);
            Debug.Log($"{player.playerClass} added {action} to discard pile {player.discarded.Count}");
        }
        
        public void SetCurrentAction(SelectedCard card)
        {
            currentCard = card;
        }

        public void SetCurrentEnemy(Enemy enemy)
        {
            currentEnemy = enemy;
        }
        
        public void SetCurrentPartyPlayer(Player player)
        {
            currentPartyPlayer = player;
        }
        
        public Player GetCurrentPartyPlayer()
        {
            return currentPartyPlayer;
        }

        public Enemy GetCurrentEnemy()
        {
            return currentEnemy;
        }

        public bool PerformAction()
        {
            if (currentCard == null) return false;

            var actionLogic = currentCard.card;
            currentCard.ConsumeCard();

            currentPlayer = PartyManager.Instance.currentPlayer;
            currentPlayer.hand.Remove(actionLogic);

            Destroy(currentCard.gameObject);
            currentCard = null;

            return true;
        }
        
        private void DestroyAllCardsFromUI()
        {
            var unselectedActions = GameObject.FindGameObjectsWithTag("action");
            foreach (var action in unselectedActions)
            {
                Destroy(action);
            }
        }
        
        public void ShowHandForPlayer(Player player)
        {
            Debug.Log($"Showing hand for {player.playerClass} with size {player.hand.Count}");
            
            player.SelectedCircle();
            currentPlayer = player;
            DestroyAllCardsFromUI();
            StartCoroutine(ShowHandWithDelay(player));
        }
        
        private IEnumerator ShowHandWithDelay(Player player)
        {
            foreach (var action in player.hand)
            {
                var go = Instantiate(actionPrefab, actionsParent.transform);
                go.GetComponent<SelectedCard>().SetUpCard(action);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}