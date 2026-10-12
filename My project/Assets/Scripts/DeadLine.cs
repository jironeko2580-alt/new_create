using System.Collections.Generic;
using UnityEngine;

/// <summary>トリガー内にボールが一定時間とどまったらゲームオーバー</summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DeadLine : MonoBehaviour
{
    [SerializeField] float limitSeconds = 2.5f;
    [Tooltip("警告表示用のライン（任意）")]
    [SerializeField] SpriteRenderer lineRenderer;
    [SerializeField] Color normalColor = new Color(1f, 0.3f, 0.3f, 0.5f);
    [SerializeField] Color warningColor = new Color(1f, 0f, 0f, 1f);

    readonly Dictionary<Ball, float> stayTimes = new();
    readonly List<Ball> buffer = new();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Ball ball)) stayTimes[ball] = 0f;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out Ball ball)) stayTimes.Remove(ball);
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.IsGameOver) return;

        float longest = 0f;
        buffer.Clear();
        buffer.AddRange(stayTimes.Keys);
        foreach (var ball in buffer)
        {
            if (ball == null || ball.IsMerging)
            {
                stayTimes.Remove(ball);
                continue;
            }
            float t = stayTimes[ball] + Time.deltaTime;
            stayTimes[ball] = t;
            longest = Mathf.Max(longest, t);
        }

        if (lineRenderer != null)
        {
            // 超過中はラインを点滅させて警告
            lineRenderer.color = longest > 0.5f
                ? Color.Lerp(normalColor, warningColor, Mathf.PingPong(Time.time * 4f, 1f))
                : normalColor;
        }

        if (longest >= limitSeconds) gm.GameOver();
    }
}
