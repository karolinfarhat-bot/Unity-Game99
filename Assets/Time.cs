using UnityEngine;
using UnityEngine.UI;

// Time.cs
// هذا السكربت مسؤول عن عدّاد الوقت في اللعبة (Timer)
public class Time : MonoBehaviour
{

    // مرجع لنص الـ UI الذي يعرض الوقت
    private Text text;

    // قيمة الوقت الحالية
    private float clock;

    // true عند نهاية المرحلة (لتحويل الوقت إلى نقاط)
    public bool finishlevel;

    // true لإيقاف العداد مؤقتاً (مثل عند مسك العلم)
    public bool stopClock;

    // مرجع لسكربت النقاط لإضافة Score
    private Score score;

    // يُستدعى مرة واحدة عند بداية اللعبة
    void Start()
    {

        // جلب مكون Text من نفس GameObject
        text = GetComponent<Text>();

        // جلب سكربت Score من المشهد
        score = GameObject.Find("Score").GetComponent<Score>();

        // تعيين الوقت الابتدائي (400 ثانية)
        clock = 400;
    }

    // يُستدعى كل FixedUpdate (مناسب للحسابات الزمنية)
    void FixedUpdate()
    {

        // إذا انتهت المرحلة
        if (finishlevel)
        {

            // تقريب الوقت للأعلى
            clock = Mathf.Ceil(clock);

            // طالما الوقت أكبر من صفر
            if (clock > 0)
            {

                // إنقاص الوقت
                clock--;

                // إضافة نقاط مقابل كل ثانية متبقية
                score.AddScore(50);

                // تشغيل صوت العدّ (beep)
                if ((int)clock % 1 == 0)
                    AudioManager.PlaySound(AudioManager.main.beep, 0.6f);
            }

            // تحديث نص الوقت على الشاشة
            text.text = "Time\n" + Mathf.Ceil(clock);
        }

        // إذا لم تنتهِ المرحلة ولم يكن الوقت متوقف
        else if (!stopClock)
        {

            // إنقاص الوقت تدريجياً (سرعة مشابهة لماريو الأصلي)
            clock -= (1 / 60f) * 2.408f;

            // تحديث النص
            text.text = "Time\n" + Mathf.Ceil(clock);
        }
    }
}
