using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// سكربت Goomba مسؤول عن حركة العدو (Goomba) وسقوطه وموته
// يفعّل العدو فقط لما يقترب Mario (تحسين أداء)
// يتعامل مع الاصطدامات (أرض، جدران، Mario)
// ينفّذ حركة السقوط والموت عند القتل
public class Goomba : MonoBehaviour
{

    // اتجاه الحركة:
    // -1 = يمشي لليسار
    //  1 = يمشي لليمين
    private float right;

    // أبعاد الـ Goomba (تُستخدم في التصادم)
    public Vector2 dimensions;

    // هل الـ Goomba نشط (قريب من Mario)
    private bool active;

    // هل الـ Goomba ميت
    private bool dead;

    // تسارع السقوط (الجاذبية)
    private float downAcc;

    // يتم استدعاؤها مرة واحدة عند بداية اللعبة
    void Start()
    {
        // يبدأ Goomba بالمشي لليسار
        right = -1;

        // في البداية يكون حي
        dead = false;
    }

    // يتم استدعاؤها بشكل ثابت (مناسبة للحركة والفيزياء)
    void FixedUpdate()
    {

        // إذا كان الـ Goomba ميت → ينفذ حركة السقوط فقط
        if (dead)
        {
            Fall();
            return;
        }

        // تفعيل Goomba فقط إذا كان قريب من Mario (لتوفير الأداء)
        active = Mathf.Abs(transform.position.x - Mario.mario.transform.position.x) <= 14;
        if (!active) return;

        // إذا ابتعد كثيراً عن Mario يتم حذفه
        if (transform.position.x - Mario.mario.transform.position.x >= 15)
            Destroy(gameObject);

        // حركة Goomba الأفقية
        float rightmove = 0.035f * right;

        // متجه الحركة (أفقي + عمودي)
        Vector2 move = new Vector2(rightmove, downAcc);

        // تطبيق الجاذبية
        downAcc -= 0.0184f;

        // فحص التصادم مع العناصر الأخرى
        CollisionInfo[] collisions = Actor.Collide(
            transform.position,
            new Vector2(transform.position.x, transform.position.y) + move,
            dimensions,
            0.01f
        );

        // معالجة كل تصادم
        foreach (CollisionInfo collision in collisions)
        {

            // تصادم من الأعلى (الوقوف على أرض)
            if (collision.hitTop)
            {
                transform.position = new Vector2(
                    transform.position.x,
                    collision.obj.GetPosition().y + dimensions.y / 2 + collision.obj.height / 2
                );
                move.y = 0;
                downAcc = 0;
            }

            // تصادم من الأسفل (ضرب من تحت)
            else if (collision.hitBottom)
            {
                transform.position = new Vector2(
                    transform.position.x,
                    collision.obj.GetPosition().y - dimensions.y / 2 - collision.obj.height / 2
                );
                move.y = 0;
                downAcc = 0;
            }

            // تصادم من اليمين → تغيير الاتجاه لليمين
            if (collision.hitRight)
            {
                print(collision.obj.name);
                transform.position = new Vector2(
                    collision.obj.GetPosition().x + dimensions.x / 2 + collision.obj.width / 2,
                    transform.position.y
                );
                move.x *= -1;
                right = 1;
            }

            // تصادم من اليسار → تغيير الاتجاه لليسار
            else if (collision.hitLeft)
            {
                transform.position = new Vector2(
                    collision.obj.GetPosition().x - dimensions.x / 2 - collision.obj.width / 2,
                    transform.position.y
                );
                move.x *= -1;
                right = -1;
            }
        }

        // تطبيق الحركة النهائية على موقع Goomba
        transform.position += new Vector3(move.x, move.y, 0f);
    }

    // دالة سقوط Goomba بعد موته
    void Fall()
    {
        Vector2 position = transform.position;

        // حركة أفقية بسيطة أثناء السقوط
        position.x += 0.035f;

        // حركة عمودية بسبب الجاذبية
        position.y += downAcc;
        transform.position = position;

        // زيادة تأثير الجاذبية
        downAcc -= 0.01f;

        // إذا خرج من الشاشة يتم حذفه
        if (position.y < -8)
        {
            Destroy(gameObject);
        }
    }

    // دالة قتل Goomba (عند القفز عليه)
    public void Kill()
    {

        // تعيينه كميت
        dead = true;

        // إزالة المصادم
        Destroy(GetComponent<RectCollider>());

        // قلب الـ Sprite رأساً على عقب
        GetComponent<SpriteRenderer>().flipY = true;

        // إعطاؤه دفعة للأعلى عند الموت
        downAcc = 0.1f;
    }
}
