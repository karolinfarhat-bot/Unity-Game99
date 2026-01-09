using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Mario.cs:
// هذا السكربت هو “قلب” اللعبة.
// مسؤول عن:
// 1) حركة ماريو يمين/يسار + السرعة + الانزلاق (Skid)
// 2) القفز + تحسين الاستجابة (Coyote Time + Jump Buffer)
// 3) الجاذبية وسقوط ماريو
// 4) التصادمات مع البلوكات والأعداء (Goomba) والـ Flag
// 5) النقاط والـ Coins والـ GameOver والرجوع للـ MainMenu
public class Mario : MonoBehaviour
{
    // مكونات مهمة على ماريو (رسم/أنيميشن)
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // UI (نقاط + عملات)
    private Score score;
    private Coins coins;

    // Singleton بسيط حتى باقي السكربتات توصل لماريو بسهولة
    public static Mario mario;

    // أبعاد جسم ماريو (تُستخدم في التصادم)
    public Vector2 dimensions;

    // سرعات الحركة
    private float xvel, yvel;

    // نوع القفزة (بطيئة/متوسطة/سريعة حسب السرعة قبل القفز)
    private JumpState jump;

    // هل ماريو كان عم يركض بسرعة بالهواء (تأثير على strafe)
    private bool fastAirStraff;

    // grounded: هل ماريو واقف على أرض؟
    // jumping: هل ماريو بحالة قفز؟
    private bool grounded, jumping;

    // ✅ Coyote + Jump Buffer:
    // lastGroundedTime: آخر لحظة كان فيها ماريو على الأرض
    // lastJumpPressedTime: آخر لحظة ضغطتي فيها Space
    private float lastGroundedTime;
    private float lastJumpPressedTime;

    // coyoteTime: يسمح بالقفز “بعد ما يترك الحافة” بوقت بسيط
    private const float coyoteTime = 0.15f;

    // jumpBufferTime: إذا ضغطتي Space قبل ما يلمس الأرض بلحظات، القفزة تنحسب فور ما يلمس
    private const float jumpBufferTime = 0.15f;

    // عناصر مرتبطة بالمرحلة
    public GameObject flag;       // راية النهاية
    public GameObject breakTile;  // قطع الطوبة لما تنكسر
    public GameObject gameover;   // واجهة Game Over
    public GameObject floatText;  // نص النقاط اللي بيطلع

    // ثوابت فيزيائية/حركية (مبنية على نظام اللعبة الأصلي تقريباً)
    private const float conversion = 65536; // تحويل للقيم العشرية
    private const float maxRunX = 10496 / conversion;
    private const float maxWalkX = 6400 / conversion;
    private const float walkAcc = 152 / conversion;
    private const float runAcc = 228 / conversion;

    private const float skidPower = 416 / conversion;
    private const float releaseDeAcc = 208 / conversion;

    // ✅ قوة القفز (كلما زدتيها بيصير أعلى)
    private const float fastJumpPower = 22000 / conversion;
    private const float jumpPower = 19000 / conversion;

    // شروط نوع القفزة حسب سرعة xvel
    private const float fastJumpReq = 9472 / conversion;
    private const float modJumpReq = 4096 / conversion;

    // تناقص سرعة Y (الجاذبية/الهبوط) حسب نوع القفز وهل عم تضلي ضاغطة Space أو لا
    private const float fastJumpDecay = 2304 / conversion;
    private const float fastJumpDecayUp = 640 / conversion;
    private const float modJumpDecay = 1536 / conversion;
    private const float modJumpDecayUp = 460 / conversion;
    private const float slowJumpDecay = 1792 / conversion;
    private const float slowJumpDecayUp = 490 / conversion;

    private const float airStrafeBorder = 6400 / conversion;
    private const float airStrafeFast = 7424 / conversion;

    // قوة القفز عند الدعس على Goomba
    private const float goombaJump = 18432 / conversion;

    // هل ماريو قوي (Mega) أو صغير (Mini)
    private bool poweredUp;

    void Start()
    {
        poweredUp = true;

        // جلب مكونات ماريو
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // حفظ مرجع ماريو
        mario = this;

        // تهيئة السرعات والحالات
        xvel = 0;
        yvel = 0;
        jump = JumpState.SlowJump;
        jumping = false;
        grounded = true;

        // جلب الـ UI
        score = GameObject.Find("Score").GetComponent<Score>();
        coins = GameObject.Find("Coins").GetComponent<Coins>();

        // قيم بعيدة حتى بالبداية ما تعتبر “ضمن وقت الـ buffer”
        lastGroundedTime = -10f;
        lastJumpPressedTime = -10f;
    }

    // متغيرات مفاتيح (نقرأها بـ Update لنضمن استجابة أحسن)
    private bool keySpace, keyD, keyA, keyShift, keySpaceDown;

    void Update()
    {
        // قراءة القفز
        keySpaceDown = Input.GetKeyDown(KeyCode.Space);
        keySpace = Input.GetKey(KeyCode.Space);

        // قراءة الحركة (D أو سهم يمين) و (A أو سهم يسار)
        keyD = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        keyA = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);

        // Shift للركض
        keyShift = Input.GetKey(KeyCode.LeftShift);

        // ✅ Jump Buffer: نحفظ وقت ضغط Space
        if (keySpaceDown)
        {
            lastJumpPressedTime = UnityEngine.Time.time;
        }
    }

    void FixedUpdate()
    {
        // إذا كان ماريو على الراية، لا نطبق حركة عادية
        if (onFlag)
        {
            FlagPole();
            return;
        }

        // ✅ تحقق من Jump Buffer + Coyote:
        bool canUseBufferedJump = (UnityEngine.Time.time - lastJumpPressedTime <= jumpBufferTime);
        bool canUseCoyote = grounded || (UnityEngine.Time.time - lastGroundedTime <= coyoteTime);

        // ====== القفز ======
        if (canUseBufferedJump && canUseCoyote)
        {
            // استهلكنا الضغطة (حتى ما تتكرر بالقفزة التالية)
            lastJumpPressedTime = -10f;

            jumping = true;

            // اختيار نوع القفز حسب سرعة ماريو الحالية
            if (Mathf.Abs(xvel) > fastJumpReq)
            {
                AudioManager.PlaySound(AudioManager.main.megaJump, 1);
                jump = JumpState.FastJump;
                yvel = fastJumpPower;
            }
            else if (Mathf.Abs(xvel) > modJumpReq)
            {
                AudioManager.PlaySound(AudioManager.main.jump, 1);
                jump = JumpState.ModerateJump;
                yvel = jumpPower;
            }
            else
            {
                AudioManager.PlaySound(AudioManager.main.jump, 1);
                jump = JumpState.SlowJump;
                yvel = jumpPower;
            }

            // هل كان سريع بالهواء
            fastAirStraff = Mathf.Abs(xvel) > airStrafeFast;
        }

        // ====== حركة يمين/يسار ======
        bool moving = false;
        bool skidding = false;

        // حركة لليمين
        if (keyD)
        {
            if (!grounded)
            {
                // حركة بالهواء
                if (xvel >= 0)
                {
                    if (xvel >= airStrafeBorder) xvel += runAcc;
                    else xvel += walkAcc;
                }
                else
                {
                    // كان رايح يسار وبده يغيّر اتجاهه بالهواء
                    if (-xvel >= airStrafeBorder) xvel += runAcc;
                    else
                    {
                        if (fastAirStraff) xvel += releaseDeAcc;
                        xvel += walkAcc;
                    }
                }
            }
            else
            {
                // حركة على الأرض
                moving = true;
                if (xvel >= 0)
                {
                    if (keyShift) xvel += runAcc;
                    else xvel += walkAcc;
                }
                else
                {
                    // كان ماشي عكس الاتجاه -> skid
                    xvel += skidPower;
                    skidding = true;
                }
            }
        }

        // حركة لليسار
        if (keyA)
        {
            if (!grounded)
            {
                if (xvel <= 0)
                {
                    if (-xvel >= airStrafeBorder) xvel -= runAcc;
                    else xvel -= walkAcc;
                }
                else
                {
                    if (xvel >= airStrafeBorder) xvel -= runAcc;
                    else
                    {
                        if (fastAirStraff) xvel -= releaseDeAcc;
                        xvel -= walkAcc;
                    }
                }
            }
            else
            {
                moving = true;
                if (xvel <= 0)
                {
                    if (keyShift) xvel -= runAcc;
                    else xvel -= walkAcc;
                }
                else
                {
                    xvel -= skidPower;
                    skidding = true;
                }
            }
        }

        // ====== تخفيف السرعة إذا ما عم نضغط حركة ======
        if (!moving && grounded)
        {
            if (xvel > 0)
            {
                xvel -= releaseDeAcc;
                if (xvel < 0) xvel = 0;
            }
            else
            {
                xvel += releaseDeAcc;
                if (xvel > 0) xvel = 0;
            }
        }

        // ====== حد أقصى للسرعة ======
        float maxSpeed = keyShift ? maxRunX : maxWalkX;
        if (xvel > maxSpeed) xvel = maxSpeed;
        else if (xvel < -maxSpeed) xvel = -maxSpeed;

        // ====== الجاذبية/تأثير الضغط المستمر على Space ======
        if (keySpace)
        {
            // إذا ضاغطة Space: الهبوط يكون أبطأ (قفزة أعلى)
            switch (jump)
            {
                case JumpState.FastJump: yvel -= fastJumpDecayUp; break;
                case JumpState.ModerateJump: yvel -= modJumpDecayUp; break;
                case JumpState.SlowJump: yvel -= slowJumpDecayUp; break;
            }
        }
        else
        {
            // إذا تركتي Space: الهبوط أسرع (قفزة أقصر)
            switch (jump)
            {
                case JumpState.FastJump: yvel -= fastJumpDecay; break;
                case JumpState.ModerateJump: yvel -= modJumpDecay; break;
                case JumpState.SlowJump: yvel -= slowJumpDecay; break;
            }
        }

        // ====== قلب السبرايت حسب الاتجاه + skid ======
        if (xvel > 0) spriteRenderer.flipX = skidding;
        else if (xvel < 0) spriteRenderer.flipX = !skidding;

        // نعمل reset قبل Move لأن التصادم هو اللي رح يرجعها true
        grounded = false;

        // ✅ تحريك ماريو مرة واحدة
        Move(new Vector2(xvel, yvel));

        // ✅ إذا صار grounded بعد التصادم، خزّني وقت الأرض
        if (grounded) lastGroundedTime = UnityEngine.Time.time;

        // إذا ماريو على الأرض فـ jumping = false
        if (jumping) jumping = !grounded;

        // سقوط تحت الخريطة -> موت
        if (transform.position.y < -8)
        {
            StartCoroutine(GetHurt(true));
        }

        // ====== تحديث الأنيميشن ======
        animator.SetFloat("xvel", Mathf.Abs(xvel));
        animator.SetBool("skidding", skidding);
        animator.SetBool("jumping", jumping);
    }

    // تحريك ماريو + حساب التصادم
    private void Move(Vector2 move)
    {
        Vector2 curPos = transform.position;
        Vector2 attemptPos = curPos + move;

        // نجلب كل التصادمات المحتملة
        CollisionInfo[] collisions = Actor.Collide(curPos, attemptPos, dimensions, 0);

        // إذا في تصادمات، نعالجها ونعدل الحركة
        if (collisions.Length > 0) move = HandleCollisions(move, collisions);

        // تطبيق الحركة الفعلية
        transform.position += new Vector3(move.x, move.y, 0);
    }

    // معالجة التصادمات (أرض/سقف/يمين/يسار + أعداء + بلوكات)
    Vector2 HandleCollisions(Vector2 move, CollisionInfo[] collisions)
    {
        bool hitTop = false;
        CollisionInfo closestTopBlock = collisions[0];
        grounded = false;

        foreach (CollisionInfo collision in collisions)
        {
            // لمس الراية -> نهاية المستوى
            if (collision.obj.blockType == BlockType.flag)
            {
                FlagPole();
                return move;
            }

            // اصطدام من الأعلى (أي ماريو نازل على شي)
            if (collision.hitTop)
            {
                move.y = 0;
                yvel = 0;
                grounded = true;

                // إذا نزل على Goomba -> يقتلها ويعمل قفزة صغيرة
                if (collision.obj.blockType == BlockType.goomba)
                {
                    AudioManager.PlaySound(AudioManager.main.goomba, 1);
                    collision.obj.GetComponent<Goomba>().Kill();
                    yvel = goombaJump;
                    move.y = goombaJump;
                    grounded = false;
                    AddPoints(100, true, collision.obj.transform.position);
                }
            }

            // اصطدام من الأسفل (رأس ماريو ضرب بلوك)
            if (collision.hitBottom)
            {
                if (collision.obj.blockType == BlockType.goomba) StartCoroutine(GetHurt(false));

                // نختار أقرب بلوك ضربه ماريو من تحت
                if (!hitTop)
                {
                    closestTopBlock = collision;
                    hitTop = true;
                }
                else
                {
                    if (Mathf.Abs(transform.position.x - collision.obj.GetPosition().x) <
                        Mathf.Abs(transform.position.x - closestTopBlock.obj.GetPosition().x))
                    {
                        closestTopBlock = collision;
                    }
                }

                move.y = 0;
                yvel = 0;
            }

            // اصطدام يمين
            if (collision.hitRight)
            {
                if (collision.obj.blockType == BlockType.goomba) StartCoroutine(GetHurt(false));
                move.x = 0;
                xvel = 0;
            }

            // اصطدام يسار
            if (collision.hitLeft)
            {
                if (collision.obj.blockType == BlockType.goomba) StartCoroutine(GetHurt(false));
                move.x = 0;
                xvel = 0;
            }
        }

        // إذا ضرب بلوك من تحت -> نطبق تأثير البلوك (كسر/عملة/نطة)
        if (hitTop) HitTopBlock(closestTopBlock);

        return move;
    }

    // ====== نهاية المستوى (الراية) ======
    private bool onFlag;
    private int flagFrame;
    private float topPole;

    void FlagPole()
    {
        onFlag = true;
        flag.GetComponent<EndLevel>().HitPole(animator, this, spriteRenderer, poweredUp);
    }

    // ====== ضرر/موت ======
    private IEnumerator GetHurt(bool kill)
    {
        float waitTime = 0.2f;

        // إذا ماريو صغير أو وقع -> موت
        if (!poweredUp || kill)
        {
            UnityEngine.Time.timeScale = 0;
            Camera.main.GetComponent<AudioSource>().Stop();

            AudioManager.PlaySound(AudioManager.main.death, 1);

            // أنيميشن الموت
            animator.SetLayerWeight(animator.GetLayerIndex("Mini"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("Mega"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("Death"), 1);

            yield return new WaitForSecondsRealtime(1f);

            // إظهار شاشة Game Over
            gameover.SetActive(true);

            yield return new WaitForSecondsRealtime(4f);

            UnityEngine.Time.timeScale = 1;

            // ✅ رجوع للـ MainMenu بعد الموت
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // إذا ماريو قوي -> يخسر القوة فقط (يصغر)
            dimensions = new Vector2(1, 1);
            UnityEngine.Time.timeScale = 0;
            poweredUp = false;

            AudioManager.PlaySound(AudioManager.main.pipe, 1);

            // تأثير "وميض" بين Mega و Mini
            Vector2 oldPosition = transform.position;
            Vector2 newPosition = transform.position;
            newPosition.y -= 0.4f;

            animator.SetLayerWeight(animator.GetLayerIndex("Mini"), 1);
            animator.SetLayerWeight(animator.GetLayerIndex("Mega"), 0);
            transform.position = newPosition;

            yield return new WaitForSecondsRealtime(waitTime);

            animator.SetLayerWeight(animator.GetLayerIndex("Mini"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("Mega"), 1);
            transform.position = oldPosition;

            yield return new WaitForSecondsRealtime(waitTime);

            animator.SetLayerWeight(animator.GetLayerIndex("Mini"), 1);
            animator.SetLayerWeight(animator.GetLayerIndex("Mega"), 0);
            transform.position = newPosition;

            yield return new WaitForSecondsRealtime(waitTime);

            animator.SetLayerWeight(animator.GetLayerIndex("Mini"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("Mega"), 1);
            transform.position = oldPosition;

            yield return new WaitForSecondsRealtime(waitTime);

            animator.SetLayerWeight(animator.GetLayerIndex("Mini"), 1);
            animator.SetLayerWeight(animator.GetLayerIndex("Mega"), 0);
            transform.position = newPosition;

            UnityEngine.Time.timeScale = 1;
        }
    }

    // التعامل مع البلوك عند ضربه من تحت
    void HitTopBlock(CollisionInfo collision)
    {
        if (collision.obj.blockType == BlockType.breakable)
        {
            // بلوك قابل للكسر (ينكسر فقط إذا ماريو Mega)
            if (poweredUp)
            {
                AudioManager.PlaySound(AudioManager.main.breakBlock, 1);
                BreakTile(collision.obj.transform.position);
                Destroy(collision.obj.gameObject);
            }
            else
            {
                // إذا صغير -> فقط Bounce
                collision.obj.StartBounce();
            }
        }
        else if (collision.obj.blockType == BlockType.coinblock)
        {
            // بلوك عملات (مرة واحدة)
            Animator animator = collision.obj.gameObject.GetComponent<Animator>();
            if (!animator.GetBool("used"))
            {
                AudioManager.PlaySound(AudioManager.main.coin, 1);
                coins.AddCoins(1);
                AddPoints(200, true, collision.obj.transform.position);

                animator.SetBool("used", true);
                collision.obj.StartBounce();
            }
        }
        else
        {
            // أي بلوك ثاني -> Bounce
            collision.obj.StartBounce();
        }
    }

    // إضافة نقاط + إظهار Text فوق المكان
    public static void AddPoints(int points, bool drawText, Vector2 position)
    {
        mario.score.AddScore(points);
        if (!drawText) return;

        Vector2 screenCoords = Camera.main.WorldToScreenPoint(new Vector2(position.x, position.y + 0.5f));
        screenCoords.x = (int)screenCoords.x;
        screenCoords.y = (int)screenCoords.y;

        Instantiate(mario.floatText, screenCoords, Quaternion.identity)
            .GetComponent<PointText>().SetPoints(points);
    }

    // كسر الطوبة إلى 4 قطع (تأثير بصري)
    void BreakTile(Vector2 position)
    {
        const float gravity = -0.01f;
        const float hor = 0.01f;

        Instantiate(breakTile, position, Quaternion.identity).GetComponent<TileBreak>().SetData(new Vector2(0, gravity),
            new Vector2(-hor, 0.3f));
        Instantiate(breakTile, position, Quaternion.identity).GetComponent<TileBreak>().SetData(new Vector2(0, gravity),
            new Vector2(-hor, 0.2f));
        Instantiate(breakTile, position, Quaternion.identity).GetComponent<TileBreak>().SetData(new Vector2(0, gravity),
            new Vector2(hor, 0.3f));
        Instantiate(breakTile, position, Quaternion.identity).GetComponent<TileBreak>().SetData(new Vector2(0, gravity),
            new Vector2(hor, 0.2f));
    }
}

// أنواع القفز
internal enum JumpState
{
    SlowJump,
    ModerateJump,
    FastJump
}
