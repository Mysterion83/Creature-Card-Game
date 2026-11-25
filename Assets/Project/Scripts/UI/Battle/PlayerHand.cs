using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerHand : MonoBehaviour
{
    [SerializeField] private Transform _handContent;
    [SerializeField] private Color _highlightColour = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Vector2 _highlightSize = new Vector2(6f, 6f);

    [Header("Hover")]
    [SerializeField] private float _hoverScale = 1.2f;
    [SerializeField] private float _hoverLift = 40f;
    [SerializeField] private float _hoverSpeed = 12f;

    private class HandCard
    {
        public CreatureData Creature;
        public GameObject Slot;
        public RectTransform Rect;
        public HandCardHover Hover;
        public Outline Highlight;
        public bool WasHovered;
    }

    public static event Action OnCannotAffordCard;
    public static event Action<CreatureData> OnCardHovered;
    public static event Action<CreatureData> OnCardSelected;
    public static event Action<CreatureData> OnCardDeselected;
    public static event Action<int> OnHandPopulated;

    private readonly List<HandCard> _cards = new();
    private MatchController _match;
    private BoardController _boardController;
    private Player _localPlayer;
    private HandCard _picked;
    private Tile _hoveredTile;
    private readonly List<RaycastResult> _uiHits = new();

    private void Awake()
    {
        _match = FindAnyObjectByType<MatchController>();
        _boardController = FindAnyObjectByType<BoardController>();

        if (_handContent == null)
        {
            Card card = FindAnyObjectByType<Card>(FindObjectsInactive.Include);
            if (card != null) _handContent = card.transform.parent;
        }
    }

    private void OnEnable()
    {
        if (_boardController != null) _boardController.OnTileSelected += HandleTileHovered;
    }

    private void OnDisable()
    {
        if (_boardController != null) _boardController.OnTileSelected -= HandleTileHovered;
    }

    private void Start()
    {
        _localPlayer = FindLocalPlayer();
        BuildHand();
    }

    private void Update()
    {
        AnimateCards();

        if (_picked == null) return;

        if (!CanSummonNow() || Mouse.current == null)
        {
            Unpick();
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame) Unpick();
        else if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverButton()) TryPlace();
    }

    private void BuildHand()
    {
        if (_handContent == null || _localPlayer == null || !_localPlayer.TryGetComponent(out PlayerDeck deck)) return;

        Card template = _handContent.GetComponentInChildren<Card>(true);
        if (template == null) return;

        Vector2 cardSize = ((RectTransform)template.transform).sizeDelta;

        var newObjects = new List<GameObject>();
        foreach (CreatureData creature in deck.CreaturesToPlace)
        {
            var slot = new GameObject("CardSlot", typeof(RectTransform), typeof(LayoutElement));
            slot.transform.SetParent(_handContent, false);
            ((RectTransform)slot.transform).sizeDelta = cardSize;
            LayoutElement layout = slot.GetComponent<LayoutElement>();
            layout.preferredWidth = cardSize.x;
            layout.preferredHeight = cardSize.y;

            Card card = Instantiate(template, slot.transform);
            card.gameObject.SetActive(true);
            card.SetCardData(creature);

            var rect = (RectTransform)card.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = cardSize;
            rect.localScale = Vector3.one;

            var handCard = new HandCard
            {
                Creature = creature,
                Slot = slot,
                Rect = rect,
                Hover = card.gameObject.AddComponent<HandCardHover>()
            };

            Image image = card.GetComponent<Image>();
            Button button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => Pick(handCard));

            handCard.Highlight = card.gameObject.AddComponent<Outline>();
            handCard.Highlight.effectColor = _highlightColour;
            handCard.Highlight.effectDistance = _highlightSize;
            handCard.Highlight.enabled = false;

            _cards.Add(handCard);
            newObjects.Add(slot);
        }

        foreach (Transform child in _handContent)
            if (!newObjects.Contains(child.gameObject)) Destroy(child.gameObject);

        OnHandPopulated?.Invoke(_cards.Count);
    }

    private void Pick(HandCard card)
    {
        if (!CanSummonNow()) return;

        if (_localPlayer.EnergySystem.Energy.CurrentAmount < card.Creature.EnergyCost)
        {
            OnCannotAffordCard?.Invoke();
            return;
        }

        if (_picked == card)
        {
            Unpick();
            return;
        }

        Unpick();
        _picked = card;
        _picked.Highlight.enabled = true;
        OnCardSelected?.Invoke(card.Creature);
    }

    private void Unpick()
    {
        if (_picked == null) return;

        _picked.Highlight.enabled = false;
        OnCardDeselected?.Invoke(_picked.Creature);
        _picked = null;
    }

    private void TryPlace()
    {
        if (_hoveredTile == null || _boardController == null) return;

        Creature placed = _boardController.PlaceCreature(_picked.Creature);
        if (placed == null) return;

        HandCard card = _picked;
        card.Highlight.enabled = false;
        _picked = null;
        card.Slot.SetActive(false);
    }

    private void AnimateCards()
    {
        bool localTurn = IsLocalTurn();
        float t = 1f - Mathf.Exp(-_hoverSpeed * Time.deltaTime);

        foreach (HandCard card in _cards)
        {
            bool hovered = localTurn && card.Hover.IsHovered;
            if (hovered && !card.WasHovered) OnCardHovered?.Invoke(card.Creature);
            card.WasHovered = hovered;

            bool raised = card == _picked || hovered;
            card.Rect.localScale = Vector3.Lerp(card.Rect.localScale, Vector3.one * (raised ? _hoverScale : 1f), t);
            card.Rect.anchoredPosition = Vector2.Lerp(card.Rect.anchoredPosition, new Vector2(0f, raised ? _hoverLift : 0f), t);
        }
    }

    private bool IsLocalTurn() =>
        _match != null && _localPlayer != null && _match.IsMatchActive && _match.CurrentPlayer == _localPlayer;

    private bool CanSummonNow() =>
        _match != null && _localPlayer != null && _match.IsMatchActive &&
        _match.CurrentState == MatchState.TurnSummon && _match.CurrentPlayer == _localPlayer;

    private void HandleTileHovered(Tile tile, Tile selectedTile) => _hoveredTile = tile;

    private bool IsPointerOverButton()
    {
        if (EventSystem.current == null || Mouse.current == null) return false;

        var pointer = new PointerEventData(EventSystem.current) { position = Mouse.current.position.ReadValue() };
        _uiHits.Clear();
        EventSystem.current.RaycastAll(pointer, _uiHits);

        foreach (RaycastResult hit in _uiHits)
            if (hit.gameObject.GetComponentInParent<Selectable>() != null || hit.gameObject.GetComponentInParent<ScrollRect>() != null) return true;
        return false;
    }

    private Player FindLocalPlayer()
    {
        foreach (Player player in PlayerRegistry.MasterList)
            if (player != null && !player.Data.IsAI) return player;
        return null;
    }
}
