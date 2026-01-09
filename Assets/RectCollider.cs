using UnityEditorInternal;
using UnityEngine;
using Time = UnityEngine.Time;

// RectCollider.cs
// هذا السكربت يعمل Collider مستطيل (Rectangle Collider) مخصص للعبة.
// كل بلوك/عدو/عنصر مهم بالمرحلة عنده RectCollider.
// Mario و Goomba يستدعوا Actor.Collide() ليعرفوا إذا صار:
// - اصطدام من فوق/تحت/يمين/يسار
public class RectCollider : MonoBehaviour
{

    // أبعاد المستطيل (عرض/ارتفاع) للتصادم
    public float width = 1;
    public float height = 1;

    // نوع الشيء: بلوك صلب؟ قابل للكسر؟ coin block؟ goomba؟ flag؟
    public BlockType blockType = BlockType.solid;

    // (اختياري) نوع الـ pickup (مثلاً coin)
    public Pickup pickup = Pickup.coin;

    // "تصادم فارغ" نرجّعه إذا ما صار أي تصادم
    private static CollisionInfo noCollision;

    // هل الكوليدر مسجل داخل Actor.colliders
    private bool registered;

    // تأخير عشوائي بسيط لتوزيع فحص التسجيل على فريمات مختلفة (لتخفيف الضغط)
    private int delay;

    // Awake: أول ما ينولد الـ GameObject
    private void Awake()
    {

        // delay عشوائي بين 0 و 30 فريم
        delay = Random.Range(0, 30);

        // تسجيل هذا الكوليدر داخل Actor حتى يتم استخدامه في التصادم
        Actor.RegisterCollider(this);
        registered = true;

        // تجهيز قيمة noCollision (كل الجهات false)
        noCollision = new CollisionInfo(false, false, false, false, this);
    }

    // FixedUpdate: فحص دوري + Bounce effect
    public void FixedUpdate()
    {

        // إذا البلوك عم يعمل Bounce (يهتز لفوق وتحت)
        if (bouncing) Bounce();

        // فحص التسجيل مرة كل فترة (تقريباً كل 30 فريم) مع delay
        // الهدف: ما نعمل Register/Delete لكل الكوليدرز بنفس الفريم
        if (UnityEngine.Time.frameCount + delay % 30 != 0) return;

        // إذا Mario بعيد عن هذا الكوليدر أكثر من 15 وحدة:
        // نزيله من قائمة Actor.colliders لتخفيف الحسابات
        if (Mathf.Abs(Mario.mario.transform.position.x - transform.position.x) > 15)
        {
            if (registered)
            {
                Actor.DeleteCollider(this);
                registered = false;
            }
        }
        // إذا Mario قريب:
        // نعيد تسجيله (إذا كان غير مسجل)
        else
        {
            if (!registered)
            {
                Actor.RegisterCollider(this);
                registered = true;
            }
        }
    }

    // متغيرات خاصة بحركة الـ Bounce
    private bool bouncing;
    private int bounceFrame;

    // Collide:
    // هذه أهم دالة: تفحص هل الجسم (Mario أو Goomba) تصادم مع هذا RectCollider
    // وتحدد اتجاه التصادم: فوق/تحت/يمين/يسار
    public CollisionInfo Collide(Vector2 dimensions, Vector2 currentPosition, Vector2 newPosition, float shorten)
    {

        // موقع هذا الكوليدر
        Vector2 myPosition = transform.position;

        // حساب مسافة التصادم (نصف عرض/ارتفاع هذا + نصف عرض/ارتفاع الجسم)
        float widthCol = width / 2 + dimensions.x / 2 - shorten;
        float heightCol = height / 2 + dimensions.y / 2 - shorten;

        // هل الجسم ضمن مجال X و Y قبل التصادم؟
        bool withinX = myPosition.x <= currentPosition.x + widthCol && myPosition.x >= currentPosition.x - widthCol;
        bool withinY = myPosition.y <= currentPosition.y + heightCol && myPosition.y >= currentPosition.y - heightCol;

        // إذا ما كان قريب لا على X ولا على Y → لا يوجد تصادم
        if (!withinX && !withinY) return noCollision;

        // collisions array:
        // 0 = hitRight
        // 1 = hitTop
        // 2 = hitLeft
        // 3 = hitBottom
        bool[] collisions = new bool[4];

        // فحص التصادم العمودي:
        // إذا الجسم كان فوق هذا الكوليدر ونزل لتحت → hitTop (يعني وقف عليه)
        if (withinX && currentPosition.y >= myPosition.y + heightCol)
        {
            if (newPosition.y <= myPosition.y + heightCol)
            {
                collisions[1] = true;
            }
        }
        // إذا الجسم كان تحت وطلع لفوق → hitBottom (يعني ضربه من تحت)
        else if (withinX && currentPosition.y <= myPosition.y - heightCol)
        {
            if (newPosition.y >= myPosition.y - heightCol)
            {
                collisions[3] = true;
            }
        }

        // فحص التصادم الأفقي:
        // إذا الجسم كان يمين هذا الكوليدر وتحرك يسار → hitRight
        if (withinY && currentPosition.x >= myPosition.x + widthCol)
        {
            if (newPosition.x <= myPosition.x + widthCol)
            {
                collisions[0] = true;
            }
        }
        // إذا الجسم كان يسار وتحرك يمين → hitLeft
        else if (withinY && currentPosition.x <= myPosition.x - widthCol)
        {
            if (newPosition.x >= myPosition.x - widthCol)
            {
                collisions[2] = true;
            }
        }

        // نرجع CollisionInfo مبني على النتائج
        return new CollisionInfo(collisions[3], collisions[1], collisions[0], collisions[2], this);
    }

    // حفظ موقع البلوك قبل الـ Bounce
    private Vector2 originalposition;

    // إزاحات جاهزة للـ Bounce animation (حركة لفوق وبعدين رجعة لتحت)
    private static readonly float[] yoffsets = {
        0.04f, 0.08f, 0.15f, 0.23f, 0.3f, 0.4f, 0.45f, 0.48f, 0.5f
    };

    // تنفيذ حركة الـ Bounce خطوة خطوة حسب bounceFrame
    private void Bounce()
    {

        // الجزء الأول: يطلع لفوق
        if (bounceFrame < yoffsets.Length)
        {
            transform.position = new Vector2(originalposition.x, originalposition.y + yoffsets[bounceFrame]);
        }
        // آخر خطوة: يرجع للمكان الأصلي وينهي الحركة
        else if (bounceFrame == yoffsets.Length * 2 - 2)
        {
            transform.position = originalposition;
            bouncing = false;
        }
        // الجزء الثاني: يرجع لتحت تدريجياً (عكس yoffsets)
        else
        {
            transform.position = new Vector2(
                originalposition.x,
                originalposition.y + yoffsets[yoffsets.Length - (bounceFrame - yoffsets.Length) - 1]
            );
        }

        // تقدم خطوة
        bounceFrame++;
    }

    // StartBounce:
    // تستدعى عندما Mario يضرب بلوك من تحت
    public void StartBounce()
    {

        // تشغيل صوت bump
        AudioManager.PlaySound(AudioManager.main.bump, 1);

        // بدء الـ Bounce
        bouncing = true;
        bounceFrame = 0;
        originalposition = transform.position;

        // إذا في Enemy فوق هذا البلوك ضمن مسافة معينة (مثل Goomba)
        // نقتله (يعني البلوك ضربه من تحت)
        Transform enemies = GameObject.Find("Enemies").transform;

        for (int i = 0; i < enemies.childCount; i++)
        {
            Transform enemie = enemies.GetChild(i);

            // شرط: العدو يكون فوق البلوك وبقربه
            if (enemie.position.y > transform.position.y && enemie.position.y < transform.position.y + 1.5f &&
                enemie.position.x > transform.position.x - 1 && enemie.position.x < transform.position.x + 1)
            {

                Goomba goomba = enemie.GetComponent<Goomba>();

                // إذا هو Goomba -> Kill
                if (goomba != null)
                {
                    goomba.Kill();
                }
                // غيره من الأعداء -> Destroy
                else Destroy(enemie.gameObject);
            }
        }
    }

    // ترجع موقع هذا الكوليدر (مفيدة في التصادم)
    public Vector2 GetPosition()
    {
        return transform.position;
    }

    // عند تدمير الكوليدر، لازم نحذفه من قائمة Actor.colliders
    private void OnDestroy()
    {
        Actor.DeleteCollider(this);
    }
}

// CollisionInfo:
// هي نتيجة التصادم: أي جهة انضربت؟ + مرجع للكوليدر المصطدم معه
public struct CollisionInfo
{
    public CollisionInfo(bool hitBottom, bool hitTop, bool hitRight, bool hitLeft, RectCollider obj)
    {
        this.hitBottom = hitBottom;
        this.hitTop = hitTop;
        this.hitRight = hitRight;
        this.hitLeft = hitLeft;
        this.obj = obj;
    }

    // hitBottom: ماريو ضرب من تحت
    // hitTop: ماريو نزل من فوق
    // hitRight: اصطدام من جهة اليمين
    // hitLeft: اصطدام من جهة اليسار
    public readonly bool hitBottom, hitTop, hitRight, hitLeft;

    // المرجع للكائن الذي صُدم معه
    public readonly RectCollider obj;
}

// أنواع البلوكات/الأشياء
[System.Serializable]
public enum BlockType
{
    solid,      // بلوك صلب عادي
    breakable,  // قابل للكسر (إذا ماريو Mega)
    coinblock,  // بلوك عملات
    goomba,     // عدو Goomba
    flag        // راية نهاية المرحلة
}

// أنواع الـ Pickup (حالياً فقط coin)
[System.Serializable]
public enum Pickup
{
    coin
}
