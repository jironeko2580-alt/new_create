using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Ball")]
    [SerializeField] Ball ballPrefab;
    [SerializeField] BallLevel[] levels =
    {
        new BallLevel("Cherry",    0.5f, 10,  new Color(0.90f, 0.20f, 0.25f)),
        new BallLevel("Orange",    0.8f, 20,  new Color(1.00f, 0.60f, 0.15f)),
        new BallLevel("Muscat",    1.2f, 40,  new Color(0.35f, 0.75f, 0.95f)),
        new BallLevel("Pineapple", 1.7f, 80,  new Color(1.00f, 0.85f, 0.20f)),
        new BallLevel("Watermelon",2.3f, 160, new Color(0.30f, 0.75f, 0.30f)),
    };

    [Header("Drop")]
    [Range(0f, 1f)]
    [SerializeField] float level1Chance = 0.7f; // Lv.1: 70% / Lv.2: 30%

    [Header("UI")]
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text finalScoreText;
    [SerializeField] Button retryButton;

    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        retryButton.onClick.AddListener(Retry);
        UpdateScoreText();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool IsMaxLevel(int level) => level >= levels.Length - 1;

    public int RollDropLevel() => Random.value < level1Chance ? 0 : 1;

    public Ball SpawnBall(int level, Vector2 position, bool simulated)
    {
        var ball = Instantiate(ballPrefab, position, Quaternion.identity);
        ball.Setup(level, levels[level], simulated);
        return ball;
    }

    public void Merge(Ball a, Ball b)
    {
        // 「片方の位置」に生成：下にある方を採用
        Vector2 pos = a.transform.position.y <= b.transform.position.y
            ? a.transform.position
            : b.transform.position;
        int next = a.Level + 1;

        Destroy(a.gameObject);
        Destroy(b.gameObject);
        SpawnBall(next, pos, true);

        AddScore(levels[next].score);
    }

    void AddScore(int amount)
    {
        Score += amount;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = $"SCORE: {Score:D4}";
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f;
        finalScoreText.text = $"SCORE: {Score:D4}";
        gameOverPanel.SetActive(true);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
