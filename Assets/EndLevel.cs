using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// EndLevel:
// سكربت مسؤول عن إنهاء المرحلة عند لمس Mario لعمود العلم (Flag Pole)
// يتحكم بحركة Mario والعلم + الأنيميشن + إيقاف الوقت + تشغيل أصوات الفوز
public class EndLevel : MonoBehaviour
{

    // عدّاد فريمات: نستخدمه لتقسيم أحداث النهاية على مراحل زمنية
    private int flagFrame;

    // هل انتهت المرحلة وبدأت حركة النهاية؟
    private bool finished;

    // مراجع نستخدمها أثناء سيناريو النهاية
    private Animator animator;               // Animator تبع Mario
    private SpriteRenderer spriteRenderer;   // SpriteRenderer تبع Mario
    private bool poweredUp;                  // هل Mario كبير (Mega) ولا صغير؟
    private float topPole;                   // نقطة بداية نزول Mario من العمود
    private Mario mario;                     // مرجع لكائن Mario

    // HitPole:
    // تُستدعى لحظة لمس Mario لعمود العلم
    // نخزّن المراجع ونفعّل وضع النهاية finished = true
    public void HitPole(Animator animator, Mario mario, SpriteRenderer spriteRenderer, bool poweredUp)
    {
        this.animator = animator;
        this.mario = mario;
        this.spriteRenderer = spriteRenderer;
        this.poweredUp = poweredUp;
        finished = true;
    }

    // FixedUpdate:
    // يُستدعى على معدل ثابت (مناسب للحركة الفيزيائية/الخطوات الزمنية الثابتة)
    void FixedUpdate()
    {

        // إذا ما خلصت المرحلة بعد، ما نعمل شي
        if (!finished) return;

        // كل FixedUpdate نزيد عدّاد الفريمات
        flagFrame++;

        // المرحلة 1: أول لحظة بعد لمس العلم
        if (flagFrame == 1)
        {

            // إيقاف عدّاد الوقت (stopClock)
            GameObject.Find("Time").GetComponent<Time>().stopClock = true;

            // تجهيز أنيميشن “الإمساك بالعمود”
            // إطفاء طبقات Mini/Mega وتفعيل طبقة Pole
            animator.SetLayerWeight(animator.GetLayerIndex("Mini"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("Mega"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("Pole"), 1);

            // ضبط حالة هل Mario كبير أم لا
            animator.SetBool("isMega", poweredUp);

            // حفظ نقطة البداية (قبل النزول)
            topPole = mario.transform.position.y;

            // تشغيل صوت لمس العلم
            AudioManager.PlaySound(AudioManager.main.flag, 1);

            // إيقاف موسيقى الخلفية (AudioSource على الكاميرا)
            Camera.main.GetComponent<AudioSource>().Stop();
        }

        // المرحلة 2: (من فريم 2 إلى 69)
        // Mario والعلم ينزلوا تدريجياً باستخدام Lerp
        else if (flagFrame < 70)
        {

            // تحريك Mario نزولاً من topPole إلى -3f خلال 70 فريم
            mario.transform.position = new Vector2(
                195,
                Mathf.Lerp(topPole, -3f, flagFrame / 70f)
            );

            // تحريك العلم نزولاً أيضاً
            transform.position = new Vector2(
                195,
                Mathf.Lerp(4.5f, -3.5f, flagFrame / 70f)
            );
        }

        // المرحلة 3: بعد النزول، Mario يلتفت (flipX) لفترة قصيرة
        else if (flagFrame < 100)
        {
            spriteRenderer.flipX = true;
            mario.transform.position = new Vector2(196, -3f);
        }

        // المرحلة 4: عند فريم 100
        // إعادة الأنيميشن للوضع الطبيعي وتشغيل صوت الفوز
        else if (flagFrame == 100)
        {

            spriteRenderer.flipX = false;

            // إطفاء طبقة Pole وإرجاع Mini/Mega حسب حالة Mario
            animator.SetLayerWeight(animator.GetLayerIndex("Mini"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("Mega"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("Pole"), 0);

            // جعل Mario كأنه يمشي للأمام
            animator.SetFloat("xvel", 1);
            animator.SetBool("skidding", false);
            animator.SetBool("jumping", false);

            // تفعيل طبقة الشخصية حسب poweredUp
            if (poweredUp)
            {
                animator.SetLayerWeight(animator.GetLayerIndex("Mega"), 1);
            }
            else
            {
                animator.SetLayerWeight(animator.GetLayerIndex("Mini"), 1);
            }

            animator.SetBool("isMega", poweredUp);

            // تشغيل صوت الفوز
            AudioManager.PlaySound(AudioManager.main.win, 1);
        }

        // المرحلة 5: من 100 إلى 199
        // Mario يمشي باتجاه القلعة/النهاية (من x=196 إلى x=200)
        else if (flagFrame < 200)
        {

            // ضبط Y بناءً على أبعاد Mario حتى يكون على الأرض بشكل صحيح
            mario.transform.position = new Vector2(
                Mathf.Lerp(196, 200, (flagFrame - 100) / 100f),
                -5 + mario.dimensions.y / 2
            );
        }

        // المرحلة 6: عند فريم 200
        // إخفاء Sprite الخاص بMario وبدء تحويل الوقت لنقاط
        else if (flagFrame == 200)
        {

            // إخفاء Mario
            spriteRenderer.enabled = false;

            // تفعيل finishlevel لبدء تحويل الوقت إلى Score في Time.cs
            GameObject.Find("Time").GetComponent<Time>().finishlevel = true;
        }
    }
}
