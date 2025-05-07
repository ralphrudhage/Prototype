using System.Collections;
using Model;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class ActionManager : MonoBehaviour
    {
        [SerializeField] private GameObject actionsParent;
        [SerializeField] private TextMeshProUGUI actionsSizeAmount;
        [SerializeField] private TextMeshProUGUI discardedAmount;
        [SerializeField] private GameObject actionPrefab;

        private Player currentPlayer;
        private SelectedAction currentAction;
        private Enemy currentEnemy;
        private int currentTurn;
        
        public static ActionManager Instance { get; private set; }

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
            DestroyAllActionsFromUI();

            foreach (var player in PartyManager.Instance.GetAllPlayers())
            {
                foreach (var action in player.hand)
                {
                    player.discarded.Add(action);
                    Debug.Log($"{player.playerClass} added {action} to discard pile {player.discarded.Count}");
                }

                player.hand.Clear();
            }

            StartCoroutine(PartyManager.Instance.EnemyTurnThenDraw());
        }
        
        // when an action is performed it should be discarded
        public void DiscardSelectedAction(GameAction action)
        {
            var player = PartyManager.Instance.currentPlayer;
            player.discarded.Add(action);
            Debug.Log($"{player.playerClass} added {action} to discard pile {player.discarded.Count}");
        }
        
        // called from UI when action is selected 
        public void SetCurrentAction(SelectedAction action)
        {
            currentAction = action;
        }

        public void SetCurrentEnemy(Enemy enemy)
        {
            currentEnemy = enemy;
        }

        public Enemy GetCurrentEnemy()
        {
            return currentEnemy;
        }

        public bool PerformAction()
        {
            if (currentAction == null) return false;

            var actionLogic = currentAction.gameAction;
            currentAction.ConsumeAction();

            currentPlayer = PartyManager.Instance.currentPlayer;
            currentPlayer.hand.Remove(actionLogic);
            
            discardedAmount.text = currentPlayer.discarded.Count.ToString();

            Destroy(currentAction.gameObject);
            currentAction = null;

            return true;
        }
        
        private void DestroyAllActionsFromUI()
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
            DestroyAllActionsFromUI();
            StartCoroutine(ShowHandWithDelay(player));
        }
        
        private IEnumerator ShowHandWithDelay(Player player)
        {
            foreach (var action in player.hand)
            {
                var go = Instantiate(actionPrefab, actionsParent.transform);
                go.GetComponent<SelectedAction>().SetUpAction(action);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}