using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Cam:
// سكربت مسؤول عن تحريك الكاميرا وتتبع اللاعب (Mario)
// الكاميرا تتحرك فقط للأمام (يمين) مثل لعبة Mario الأصلية
public class Cam : MonoBehaviour
{

    // مرجع لتحويل (Transform) اللاعب Mario
    private Transform mario;

    // وضع debug:
    // إذا كان true → الكاميرا تتحرك حتى لو رجع ماريو لورا
    // مفيد للاختبار أثناء التطوير
    public bool debug;

    // Start:
    // يُستدعى مرة واحدة عند بداية المشهد
    void Start()
    {

        // تحديد دقة الشاشة:
        // 256x240 هي دقة جهاز NES الأصلي
        // true = Fullscreen
        // 60 = معدل الإطارات
        Screen.SetResolution(256, 240, true, 60);

        // إيجاد GameObject باسم "Mario" وأخذ Transform تبعه
        mario = GameObject.Find("Mario").transform;
    }

    // Update:
    // يُستدعى كل Frame
    void Update()
    {

        // إذا Mario تقدّم للأمام (يمين)
        // أو إذا وضع debug مفعّل
        if (mario.position.x > transform.position.x || debug)
        {

            // نحرك الكاميرا أفقياً لتساوي موقع Mario
            // Y ثابت (ما نتحرك عمودياً)
            // Z = -10 حتى تبقى الكاميرا أمام المشهد
            transform.position = new Vector3(
                mario.position.x,
                transform.position.y,
                -10
            );
        }
    }
}
