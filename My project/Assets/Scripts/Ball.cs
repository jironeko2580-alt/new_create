using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Ball : MonoBehaviour
{
    /// <summary>0始まりのレベル番号（0 = Lv.1 チェリー）</summary>
    public int Level { get; private set; }

    /// <summary>合体処理中（二重合体を防ぐ）</summary>
    public bool IsMerging { get; private set; }

    /// <summary>見た目上の半径（スプライトは直径1ユニット）</summary>
    public float Radius => transform.localScale.x * 0.5f;

    Rigidbody2D rb;
    SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Setup(int level, BallLevel data, bool simulated)
    {
        Level = level;
        transform.localScale = Vector3.one * data.scale;
        sr.color = data.color;
        sr.sortingOrder = 1;
        rb.simulated = simulated;
        gameObject.name = $"Ball_Lv{level + 1}_{data.name}";
    }

    /// <summary>投下：物理挙動を有効化する</summary>
    public void Release()
    {
        rb.simulated = true;
    }

    void OnCollisionEnter2D(Collision2D collision) => TryMerge(collision);

    // 既に接触している同レベル同士（合体直後など）も拾うため Stay でも判定
    void OnCollisionStay2D(Collision2D collision) => TryMerge(collision);

    void TryMerge(Collision2D collision)
    {
        var gm = GameManager.Instance;
        if (IsMerging || gm == null || gm.IsGameOver) return;
        if (!collision.gameObject.TryGetComponent(out Ball other)) return;
        if (other.IsMerging || other.Level != Level) return;
        if (gm.IsMaxLevel(Level)) return;

        IsMerging = true;
        other.IsMerging = true;
        gm.Merge(this, other);
    }
}
