using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Score.cs
// هذا السكربت مسؤول عن إدارة وعرض نقاط اللاعب (Score)
public class Score : MonoBehaviour
{

    // مرجع لنص الـ UI الذي يعرض النقاط
    private Text text;

    // المتغير الذي يخزن مجموع النقاط
    private int score;

    // يُستدعى مرة واحدة عند بداية اللعبة
    void Start()
    {

        // جلب مكون Text من نفس GameObject
        text = GetComponent<Text>();
    }

    // دالة لإضافة نقاط جديدة
    // يتم استدعاؤها عند قتل عدو أو جمع نقاط
    public void AddScore(int score)
    {

        // إضافة النقاط الجديدة إلى المجموع الكلي
        this.score += score;

        // تحديث النص المعروض
        // PadLeft(6,'0') تضمن أن الرقم دائماً 6 خانات (مثلاً: 000150)
        text.text = "Mario\n" + this.score.ToString().PadLeft(6, '0');
    }
}
