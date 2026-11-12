using System;
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

    void Awake()
    {
        foreach (Node n in nodes)
        {
            Node node = n;   
            node.button.onClick.AddListener(() => Play(node.level));
        }
    }

    void OnEnable()
    {
        var pm = ProfileManager.Instance;
        for (int i = 0; i < nodes.Length; i++)
        {            
            bool open = i == 0 || (pm != null && pm.IsCompleted(nodes[i - 1].level));

            nodes[i].button.interactable = open;
            if (nodes[i].lockIcon != null) nodes[i].lockIcon.SetActive(!open);
        }
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