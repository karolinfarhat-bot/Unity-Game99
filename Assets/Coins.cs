using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Coins:
// سكربت مسؤول عن عدّ العملات (Coins) وعرضها على واجهة المستخدم
// مرتبط بعنصر Text داخل الـ Canvas
public class Coins : MonoBehaviour
{

    // مرجع لعنصر النص (Text) اللي بيعرض عدد العملات
    private Text text;

    // عدد العملات الحالية
    private int coins;

    // Start:
    // يُستدعى مرة واحدة عند بداية المشهد
    void Start()
    {

        // الحصول على مكوّن Text الموجود على نفس GameObject
        text = GetComponent<Text>();
    }

    // AddCoins:
    // تُستدعى عند جمع عملة جديدة
    // coins: عدد العملات المضافة (غالباً 1)
    public void AddCoins(int coins)
    {

        // زيادة عدد العملات الحالي
        this.coins += coins;

        // تحديث النص الظاهر على الشاشة
        // PadLeft(2, '0') → يضمن أن الرقم يظهر من خانتين (مثال: 01, 05, 12)
        // الشكل النهائي: *05
        text.text = "*" + this.coins.ToString().PadLeft(2, '0');
    }
}
