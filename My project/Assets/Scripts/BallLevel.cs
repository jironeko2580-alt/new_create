using System;
using UnityEngine;

/// <summary>ボール1レベル分の設定（要件定義書 4. ボール仕様一覧）</summary>
[Serializable]
public class BallLevel
{
    public string name;
    public float scale;
    public int score;
    public Color color;

    public BallLevel(string name, float scale, int score, Color color)
    {
        this.name = name;
        this.scale = scale;
        this.score = score;
        this.color = color;
    }
}
