using UnityEngine;

public class ScreenBarSettings : MonoBehaviour
{
    [SerializeField] TopBar bar;
    [SerializeField] string title;
    [SerializeField] bool showBack = true;
    [SerializeField] bool showTabs = false;

    void OnEnable() { if (bar != null) bar.Configure(title, showBack, showTabs); }
}