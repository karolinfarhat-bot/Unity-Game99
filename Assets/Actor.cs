using System.Collections.Generic;
using UnityEngine;

// Actor: كلاس Static يعني ما بينحط على GameObject
// وظيفته: إدارة كل RectCollider الموجودة باللعبة + تنفيذ فحص التصادمات (Collision)
// أي جسم متحرك (Mario, Goomba...) بيستدعي Actor.Collide ليعرف إذا رح يصطدم أثناء الحركة
public static class Actor
{

    // قائمة بكل الـ RectCollider المسجّلة حالياً (البلوكات/الأعداء/الأعلام... حسب استخدام RectCollider)
    // RectCollider.RegisterCollider رح يضيف نفسه لهون بوقت Awake غالباً
    private static List<RectCollider> colliders;

    // Collide:
    // - curPos: مكان الجسم الحالي
    // - attemptPos: المكان اللي بده يروح عليه بعد الحركة
    // - dimensions: أبعاد الجسم اللي عم يتحرك (عرض/طول)
    // - shorten: قيمة صغيرة لتخفيف "التصاق" الكوليدر (حتى ما يصير في sticky collision)
    // ترجع: لائحة CollisionInfo (كل الاصطدامات اللي صارت مع كوليدرز مختلفة)
    public static CollisionInfo[] Collide(Vector2 curPos, Vector2 attemptPos, Vector2 dimensions, float shorten)
    {

        // هون منجمع كل الاصطدامات اللي صارت
        List<CollisionInfo> collisions = new List<CollisionInfo>();

        // نمرّ على كل RectCollider مسجّل باللعبة
        foreach (RectCollider collider in colliders)
        {

            // نطلب من كل RectCollider يفحص إذا في اصطدام بين الجسم المتحرك وهذا الكوليدر
            // ويرجع CollisionInfo فيه: hitTop/hitBottom/hitRight/hitLeft + مرجع للكوليدر نفسه
            CollisionInfo collision = collider.Collide(dimensions, curPos, attemptPos, shorten);

            // إذا صار اصطدام من أي جهة → منضيفه للنتائج
            if (collision.hitBottom || collision.hitTop || collision.hitRight || collision.hitLeft)
            {
                collisions.Add(collision);
            }
        }

        // نحولها لمصفوفة Array لأنها أنسب للاستخدام السريع بباقي الكود
        return collisions.ToArray();
    }

    // RegisterCollider:
    // أي RectCollider باللعبة لازم يسجّل نفسه هون حتى ينحسب ضمن التصادمات
    public static void RegisterCollider(RectCollider collider)
    {

        // أول مرة بنعمل list إذا كانت null
        if (colliders == null) colliders = new List<RectCollider>();

        // نضيف الكوليدر للقائمة
        colliders.Add(collider);
    }

    // DeleteCollider:
    // لما RectCollider ينحذف (Destroy) أو يطلع برا مجال اللعب
    // لازم ينشال من اللائحة حتى ما يضل ينحسب بالتصادمات
    public static void DeleteCollider(RectCollider collider)
    {
        colliders.Remove(collider);
    }
}
