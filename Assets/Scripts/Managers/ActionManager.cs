using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private TextMeshProUGUI turn;
        [SerializeField] private GameObject actionPrefab;

        private SelectedAction currentAction;

        private int currentTurn;
        private int discardedSize;
        private const int handSize = 5;
        private int currentSize;
        private PlayerController player;

        private Queue<GameAction> queuedActions;
        private readonly List<GameAction> discardedActions = new();

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
            player = FindAnyObjectByType<PlayerController>();
            actionsParent.transform.DestroyTagRecursively("action");
        }

        private void Start()
        {
            var startActions = new List<GameAction>(new GameAction[]
            {
                new MoveAction(100, 1),
                new MoveAction(100, 1),
                new MoveAction(100, 1),
                new MoveAction(100, 1),
                new MoveAction(100, 1),
                new MoveAction(100, 1),
                new AttackAction(100, 20),
                new AttackAction(100, 20),
                new AttackAction(100, 20),
                new AttackAction(100, 20),
                new AttackAction(100, 20),
                new AttackAction(100, 20)
            });

            startActions.Shuffle();

            queuedActions = new Queue<GameAction>(startActions);

            actionsSizeAmount.text = queuedActions.Count.ToString();
            turn.text = "";

            StartCoroutine(SpawnNewActions());
        }

        private IEnumerator SpawnNewActions()
        {
            yield return new WaitForSeconds(1f);
            currentTurn++;
            turn.text = currentTurn.ToString();
            player.ResetEnergy();

            for (int i = 0; i < handSize; i++)
            {
                if (queuedActions.Count == 0 && discardedActions.Count > 0)
                {
                    discardedActions.Shuffle();
                    queuedActions = new Queue<GameAction>(discardedActions);
                    discardedActions.Clear();
                    discardedSize = 0;
                    discardedAmount.text = "0";
                }

                if (queuedActions.Count > 0)
                {
                    SpawnAction();
                    actionsSizeAmount.text = queuedActions.Count.ToString();
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }

        private void SpawnAction()
        {
            var action = Instantiate(actionPrefab, actionsParent.transform);
            action.GetComponent<SelectedAction>().SetUpAction(queuedActions.Dequeue());
        }


        // when an action is performed it should be discarded
        public void DiscardSelectedAction(GameAction action)
        {
            discardedActions.Add(action);
            discardedSize = discardedActions.Count;
            discardedAmount.text = discardedSize.ToString();
        }

        // called from UI when player wants to end the turn, discard unselected actions
        public void EndTurn()
        {
            var unselectedActions = GameObject.FindGameObjectsWithTag("action");
            foreach (var action in unselectedActions)
            {
                DiscardSelectedAction(action.GetComponent<SelectedAction>().gameAction);
                Destroy(action);
            }

            StartCoroutine(SpawnNewActions());
        }

        // called from UI when action is selected 
        public void SetCurrentAction(SelectedAction action)
        {
            currentAction = action;
        }

        public bool PerformAction()
        {
            if (currentAction == null) return false;

            currentAction.ConsumeAction();
            currentAction = null;

            return true;
        }
    }
}