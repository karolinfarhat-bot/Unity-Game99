using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// PointText.cs
// هذا السكربت مسؤول عن إظهار نقاط مؤقتة فوق المكان
// (مثل 100 أو 200) عند قتل عدو أو كسر بلوك
public class PointText : MonoBehaviour
{

    // RectTransform للنص (لأنه عنصر UI)
    private RectTransform rect;

    // عدّاد فريمات لتحديد مدة بقاء النص
    private int frame;

    // يُستدعى مرة واحدة عند إنشاء النص
    void Start()
    {

        // جلب RectTransform الخاص بالنص
        rect = GetComponent<RectTransform>();

        // ربط النص بالـ Canvas حتى يظهر على الشاشة
        transform.SetParent(GameObject.Find("Canvas").transform, false);
    }

    // ضبط قيمة النقاط التي ستُعرض
    public void SetPoints(int points)
    {

        // وضع رقم النقاط كنص
        GetComponent<Text>().text = points.ToString();
    }

    // يُستدعى كل فريم
    void Update()
    {

        // بعد 60 فريم (حوالي ثانية) يتم حذف النص
        if (frame > 60)
            Destroy(gameObject);

        // تحريك النص للأعلى تدريجياً
        rect.position = new Vector2(rect.position.x, rect.position.y + 1);

        // زيادة عداد الفريمات
        frame++;
    }
}
