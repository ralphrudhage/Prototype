using System.Collections;
using System.Collections.Generic;
using Model;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class PartyManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI turn;
        [SerializeField] private List<Player> playerUnits;
        private int currentPlayerIndex;
        private int currentTurn;

        public static PartyManager Instance { get; private set; }

        public Player currentPlayer => playerUnits[currentPlayerIndex];

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            StartPlayerTurn();
        }

        private void StartPlayerTurn()
        {
            currentTurn++;
            turn.text = currentTurn.ToString();
            
            foreach (var player in playerUnits)
            {
                player.ResetAP();
                player.hand.Clear();
                // DrawInitialHandForPlayer(player);
            }

            currentPlayerIndex = -1;
            // currentPlayerIndex = 0;
            // ActionManager.Instance.ShowHandForPlayer(currentPlayer);
        }
        
        public IEnumerator EnemyTurnThenDraw()
        {
            yield return EnemyManager.Instance.TakeEnemyTurn();
            StartPlayerTurn();
        }
        
        public void SetCurrentPlayer(Player player)
        {
            int index = playerUnits.IndexOf(player);
            if (index == -1) return;

            currentPlayerIndex = index;
            
            if (player.hand.Count == 0)
            {
                DrawHand(player);
                PlayerUI.Instance.UpdatePlayerUI(player);
            }

            ActionManager.Instance.ShowHandForPlayer(player);
        }
        
        private void DrawHand(Player player)
        {
            for (int i = 0; i < 5; i++)
            {
                if (player.drawPile.Count == 0 && player.discarded.Count > 0)
                {
                    player.discarded.Shuffle();
                    player.drawPile = new Queue<GameAction>(player.discarded);
                    player.discarded.Clear();
                }

                if (player.drawPile.Count > 0)
                {
                    var action = player.drawPile.Dequeue();
                    player.hand.Add(action);
                }
            }
        }
        
        public List<Player> GetAllPlayers()
        {
            return playerUnits;
        }
    }
}
