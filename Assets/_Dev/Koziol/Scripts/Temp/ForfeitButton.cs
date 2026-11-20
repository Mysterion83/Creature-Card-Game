using System;
using UnityEngine;
using UnityEngine.SceneManagement;


[Obsolete("Refactor whole forfeit Button")]
public class ForfeitButton : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
