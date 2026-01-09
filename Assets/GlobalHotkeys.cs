using UnityEngine;
using UnityEngine.SceneManagement;

// GlobalHotkeys:
// سكربت عام (Global) لالتقاط أزرار الكيبورد من أي مشهد
// يستخدم للرجوع إلى MainMenu عند ضغط ESC أو Q
// يبقى فعّال بين المشاهد باستخدام DontDestroyOnLoad
public class GlobalHotkeys : MonoBehaviour
{
    // Singleton:
    // نضمن وجود نسخة واحدة فقط من هذا السكربت في اللعبة
    private static GlobalHotkeys instance;

    // Awake:
    // يُستدعى قبل Start
    // نتحقق إذا في نسخة ثانية من السكربت
    void Awake()
    {
        // إذا في نسخة موجودة مسبقاً → نحذف النسخة الجديدة
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // تعيين النسخة الحالية
        instance = this;

        // منع تدمير هذا الكائن عند تغيير المشاهد
        DontDestroyOnLoad(gameObject);
    }

    // Update:
    // يُستدعى كل Frame
    // نراقب ضغط أزرار ESC أو Q
    void Update()
    {
        // إذا ضغط المستخدم ESC أو Q
        // نرجعه إلى MainMenu
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
        {
            GoToMainMenu();
        }
    }

    // GoToMainMenu:
    // دالة مسؤولة عن الرجوع إلى مشهد MainMenu
    void GoToMainMenu()
    {
        // إعادة الوقت للوضع الطبيعي
        // (مهم جداً لأن Time.cs قد يوقف الوقت أثناء GameOver أو نهاية المرحلة)
        UnityEngine.Time.timeScale = 1f;

        // إذا كنا أصلاً في مشهد MainMenu
        // لا نعمل شيء
        if (SceneManager.GetActiveScene().name == "MainMenu")
            return;

        // تحميل مشهد MainMenu
        SceneManager.LoadScene("MainMenu");
    }

    // QuitGame:
    // دالة اختيارية تُستخدم مع زر Exit في MainMenu
    // تسكّر اللعبة فقط في النسخة النهائية (Build)
    public void QuitGame()
    {
#if UNITY_EDITOR
        // داخل Unity Editor: فقط نطبع رسالة (ما نسكر Unity)
        Debug.Log("QuitGame called (Editor) - ما رح سكّر Unity");
#else
        // في النسخة النهائية (Build): تسكير اللعبة
        Application.Quit();
#endif
    }
}
