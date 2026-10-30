using UnityEngine;

public class ExitConfirm : MonoBehaviour
{
    [SerializeField] MenuController menu;
    [SerializeField] Popup popup;

    public void ReturnToMainMenu()
    {
        popup.Hide();
        if (ProfileManager.Instance != null) ProfileManager.Instance.CloseGame();
        menu.ShowMain();
    }
}