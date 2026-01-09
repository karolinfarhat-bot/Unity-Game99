using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// AudioManager:
// مدير الأصوات الرئيسي باللعبة
// مسؤول عن تشغيل كل الأصوات (قفز، عملة، موت، فوز...)
// معمول بنظام Singleton (عن طريق المتغير static main)
public class AudioManager : MonoBehaviour
{

    // مرجع static لنسخة AudioManager الموجودة بالمشهد
    // بيسمح لأي سكربت يوصل عليه بدون ما نعمل Find كل مرة
    public static AudioManager main;

    // Prefab فيه AudioSource
    // نستخدمه لإنشاء AudioSource مؤقت لكل صوت
    public GameObject prefab;

    // أصوات العملات
    public AudioClip coin;

    // صوت قتل الـ Goomba
    public AudioClip goomba;

    // صوت كسر البلوك
    public AudioClip breakBlock;

    // صوت الدخول بالأنبوب
    public AudioClip pipe;

    // صوت القفزة العادية
    public AudioClip jump;

    // صوت القفزة القوية (Mega Jump)
    public AudioClip megaJump;

    // صوت الموت
    public AudioClip death;

    // صوت لمس العلم (نهاية المرحلة)
    public AudioClip flag;

    // صوت الفوز
    public AudioClip win;

    // صوت عدّ الوقت بالنهاية
    public AudioClip beep;

    // صوت اصطدام الرأس بالبلوك
    public AudioClip bump;

    // Awake:
    // يُستدعى مرة وحدة عند بداية اللعبة
    // نربط النسخة الحالية بالمتغير static main
    private void Awake()
    {
        main = this;
    }

    // PlaySound:
    // دالة static يعني ممكن تنادى من أي سكربت مباشرة
    // clip: الصوت المراد تشغيله
    // volume: مستوى الصوت (من 0 إلى 1)
    public static void PlaySound(AudioClip clip, float volume)
    {

        // إنشاء AudioSource جديد من prefab
        AudioSource source = Instantiate(main.prefab).GetComponent<AudioSource>();

        // تعيين الصوت
        source.clip = clip;

        // تشغيل الصوت
        source.Play();

        // تحديد مستوى الصوت
        source.volume = volume;

        // حذف AudioSource بعد انتهاء الصوت
        // (حتى ما تتراكم كائنات غير مستخدمة بالمشهد)
        Destroy(source.gameObject, source.clip.length);
    }
}
