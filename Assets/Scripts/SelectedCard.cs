using Managers;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectedCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private TextMeshProUGUI apText;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI rangeValue;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI effectValue;

    public Card card;
    private Vector3 originalPosition;
    private bool isDragging;
    private Image cardImage;

    private void Start()
    {
        cardImage = GetComponent<Image>();
    }

    public void SetUpCard(Card selectedAction)
    {
        card = selectedAction;

        cardName.text = card.name;
        apText.text = card.cost.ToString();

        rangeValue.text = card.range.ToString();

        switch (card.type)
        {
            case CardType.MELEE:
            case CardType.RANGED:
            case CardType.DOT:
                effectText.text = "Damage:";
                effectValue.text = card.effect.ToString();
                break;
            case CardType.MOVE:
                effectText.text = "";
                effectValue.text = "";
                break;
            case CardType.PARTY:
                effectText.text = "Amount:";
                effectValue.text = card.effect.ToString();
                break;
        }
    }

    public void ConsumeCard()
    {
        switch (card.type)
        {
            case CardType.MOVE:
                GridManager.Instance.ClearHighlights();
                break;

            case CardType.RANGED:
                PartyManager.Instance.currentPlayer.AttackEnemy(
                    CardManager.Instance.GetCurrentEnemy().targetPos,
                    card.effect
                );
                // cursorManager.SetCrossHair();
                break;
        }

        PartyManager.Instance.currentPlayer.UseAP(card.cost);
        CardManager.Instance.DiscardSelectedCard(card);
        Destroy(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PartyManager.Instance.currentPlayer.GetCurrentAP() < card.cost) return;

        originalPosition = transform.position;
        isDragging = true;

        CardManager.Instance.SetCurrentAction(this);

        cardImage.color = new Color(cardImage.color.r, cardImage.color.g, cardImage.color.b, 0.5f);

        switch (card)
        {
            case MoveCard move:
                GridManager.Instance.ShowMoveCircles(PartyManager.Instance.currentPlayer.currentGridPos, move.range);
                break;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0f;
        transform.position = newPosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        if (CanConsume())
        {
            ConsumeCard();
        }
        else
        {
            cardImage.color = new Color(cardImage.color.r, cardImage.color.g, cardImage.color.b, 1f);
            GridManager.Instance.ClearHighlights();
            transform.position = originalPosition;
        }
    }

    private bool CanConsume()
    {
        Vector3 droppedWorldPos = transform.position;

        if (card is MoveCard)
        {
            Vector2Int droppedGridPos = GridManager.Instance.GetGridPositionFromWorld(droppedWorldPos);

            bool canMove = GridManager.Instance.IsValidMoveTile(droppedGridPos);
            if (canMove)
            {
                PartyManager.Instance.currentPlayer.SetGridPosition(droppedGridPos);
                return true;
            }

            return false;
        }

        if (card.type == CardType.RANGED)
        {
            Collider2D hit = Physics2D.OverlapPoint(droppedWorldPos);
            if (hit != null && hit.TryGetComponent<Enemy>(out var enemy))
            {
                Vector2Int enemyPos = enemy.currentGridPos;
                Vector2Int playerPos = PartyManager.Instance.currentPlayer.currentGridPos;
                int range = PartyManager.Instance.currentPlayer.GetCurrentRange();

                int dx = Mathf.Abs(enemyPos.x - playerPos.x);
                int dy = Mathf.Abs(enemyPos.y - playerPos.y);
                int manhattanDistance = dx + dy;

                if (manhattanDistance <= range)
                {
                    CardManager.Instance.SetCurrentEnemy(enemy); // Set this so ConsumeCard can use it
                    return true;
                }
            }
        }

        // Add future checks for AttackCard, etc.
        return false;
    }
}