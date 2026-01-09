using UnityEngine;
using UnityEngine.SceneManagement;

// هذا السكربت مسؤول عن التحكم بالـ Main Menu
// (زر Play – زر Exit – اختصارات الكيبورد)

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("1-1");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // يظهر فقط داخل Unity
    }

    void Update()
    {
        // ESC يرجّع للمينيو
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }

        // Q يطلع من اللعبة
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitGame();
        }
    }
}
