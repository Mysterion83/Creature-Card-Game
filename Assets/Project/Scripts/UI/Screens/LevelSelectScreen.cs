using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectScreen : MonoBehaviour
{
    [Serializable]
    public class Node
    {
        public LevelData level;
        public Button button;
        public GameObject lockIcon;   
    }

    [Tooltip("In play order: the list order IS the unlock order")]
    [SerializeField] Node[] nodes;

    [Header("Selected Level")]
    [SerializeField] TMP_Text levelNameText;
    [SerializeField] Image levelArtImage;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] GameObject coinReward;
    [SerializeField] TMP_Text rewardText;
    [SerializeField] GameObject completedImage;
    [SerializeField] Button startButton;

    [Header("Node highlight")]
    [SerializeField] Color normalColour = Color.white;
    [SerializeField] Color selectedColour = new Color(1f, 0.85f, 0.4f);

    Node selectedNode;

    void Awake()
    {
        foreach (Node n in nodes)
        {
            Node node = n;   
            node.button.onClick.AddListener(() => Select(node));
        }

        if (startButton != null) startButton.onClick.AddListener(StartSelected);
    }

    void OnEnable()
    {
        var pm = ProfileManager.Instance;
        Node newest = null;
        for (int i = 0; i < nodes.Length; i++)
        {            
            bool open = i == 0 || (pm != null && pm.IsCompleted(nodes[i - 1].level));

            nodes[i].button.interactable = open;
            if (nodes[i].lockIcon != null) nodes[i].lockIcon.SetActive(!open);
            if (open) newest = nodes[i];
        }

        Select(newest);
    }

    void Select(Node node)
    {
        if (selectedNode != null) SetHighlight(selectedNode, false);
        selectedNode = node;
        if (selectedNode != null) SetHighlight(selectedNode, true);

        ShowDetails(selectedNode != null ? selectedNode.level : null);
    }

    void SetHighlight(Node node, bool on)
    {
        if (node.button != null && node.button.image != null)
            node.button.image.color = on ? selectedColour : normalColour;
    }

    void ShowDetails(LevelData level)
    {
        bool hasLevel = level != null;
        var pm = ProfileManager.Instance;
        bool completed = hasLevel && pm != null && pm.IsCompleted(level);

        if (levelNameText != null)
            levelNameText.text = !hasLevel ? "" : string.IsNullOrEmpty(level.LevelName) ? level.name : level.LevelName;
        if (descriptionText != null) descriptionText.text = hasLevel ? level.Description : "";

        if (levelArtImage != null)
        {
            levelArtImage.sprite = hasLevel ? level.Preview : null;
            levelArtImage.enabled = hasLevel && level.Preview != null;
        }

        if (rewardText != null) rewardText.text = hasLevel ? $"x{level.CoinReward}" : "";
        if (coinReward != null) coinReward.SetActive(hasLevel && !completed);
        if (completedImage != null) completedImage.SetActive(completed);

        if (startButton != null) startButton.interactable = hasLevel;
    }

    void StartSelected()
    {
        if (selectedNode != null) Play(selectedNode.level);
    }

    void Play(LevelData level)
    {
        if (string.IsNullOrEmpty(level.SceneName))
        {
            Debug.Log($"{level.name} has no Scene Name yet so there's nothing to load.");
            return;
        }
        ProfileManager.Instance.StartLevel(level);
    }
}