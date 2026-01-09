using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TileBreak.cs
// هذا السكربت يتحكم بحركة قطع البلوك بعد كسره
// (تطير للأعلى ثم تسقط بسبب الجاذبية وتختفي)
public class TileBreak : MonoBehaviour
{

    // التسارع (يمثل الجاذبية)
    private Vector2 acceleration;

    // السرعة الحالية للقطعة
    private Vector2 velocity;

    // يُستدعى كل FixedUpdate (مناسب للحركة الفيزيائية)
    void FixedUpdate()
    {

        // حفظ الموقع الحالي
        Vector2 position = transform.position;

        // تحديث الموقع حسب السرعة
        position += velocity;

        // تحديث السرعة حسب التسارع (gravity)
        velocity += acceleration;

        // تطبيق الموقع الجديد
        transform.position = position;
    }

    // دالة يتم استدعاؤها عند إنشاء القطعة
    // لتحديد السرعة والتسارع الأوليين
    public void SetData(Vector2 acceleration, Vector2 velocity)
    {

        // تعيين السرعة الابتدائية
        this.velocity = velocity;

        // تعيين التسارع (الجاذبية)
        this.acceleration = acceleration;

        // حذف القطعة بعد ثانيتين (حتى لا تبقى للأبد)
        Destroy(gameObject, 2.0f);
    }
}
