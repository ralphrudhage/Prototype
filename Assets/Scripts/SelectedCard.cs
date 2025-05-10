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
            
            case CardType.DOT:
            case CardType.MELEE:
            case CardType.RANGED:
                PartyManager.Instance.currentPlayer.AttackEnemy(
                    CardManager.Instance.GetCurrentEnemy().targetPos,
                    card.effect
                );
                break;
            case CardType.PARTY:
                PartyManager.Instance.currentPlayer.CastSpell(card);
                CardManager.Instance.GetCurrentPartyPlayer().CardEffect(card);
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

        switch (card.type)
        {
            case CardType.MOVE:
            {
                var droppedGridPos = GridManager.Instance.GetGridPositionFromWorld(droppedWorldPos);

                var canMove = GridManager.Instance.IsValidMoveTile(droppedGridPos);
                if (canMove)
                {
                    PartyManager.Instance.currentPlayer.SetGridPosition(droppedGridPos);
                    return true;
                }

                return false;
            }
            // damage attacks
            case CardType.RANGED or CardType.DOT or CardType.MELEE:
            {
                var hit = Physics2D.OverlapPoint(droppedWorldPos);
                if (hit != null && hit.TryGetComponent<Enemy>(out var enemy))
                {
                    var enemyPos = enemy.currentGridPos;
                    var playerPos = PartyManager.Instance.currentPlayer.currentGridPos;

                    var dx = Mathf.Abs(enemyPos.x - playerPos.x);
                    var dy = Mathf.Abs(enemyPos.y - playerPos.y);
                    var chebyshevDistance = Mathf.Max(dx, dy);

                    if (chebyshevDistance <= card.range)
                    {
                        CardManager.Instance.SetCurrentEnemy(enemy); // Set this so ConsumeCard can use it
                        return true;
                    }


                    Debug.Log("Out of range");
                }

                break;
            }
            // target a party member for i.e. healing spells
            case CardType.PARTY:
            {
                var hit = Physics2D.OverlapPoint(droppedWorldPos);
                if (hit != null && hit.TryGetComponent<Player>(out var partyPlayer))
                {
                    var enemyPos = partyPlayer.currentGridPos;
                    var playerPos = PartyManager.Instance.currentPlayer.currentGridPos;

                    var dx = Mathf.Abs(enemyPos.x - playerPos.x);
                    var dy = Mathf.Abs(enemyPos.y - playerPos.y);
                    var chebyshevDistance = Mathf.Max(dx, dy);

                    if (chebyshevDistance <= card.range)
                    {
                        CardManager.Instance.SetCurrentPartyPlayer(partyPlayer); // Set this so ConsumeCard can use it
                        return true;
                    }

                    Debug.Log("Out of range");
                }

                break;
            }
        }
        
        return false;
    }
}