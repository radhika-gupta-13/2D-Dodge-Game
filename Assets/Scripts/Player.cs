using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Player : MonoBehaviour
{
    public float moveSpeed = 8f;

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 1.5f;
    public float enemyFallSpeed = 3f;

    [Header("UI")]
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Effects")]
    public GameObject explosionEffect;

    private bool gameOver = false;

    public int score = 0;
    private int highScore = 0;

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        // Load high score
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateUI();
    }

    void Update()
    {
        if (!gameOver)
        {
            float move = Input.GetAxis("Horizontal");
            transform.position += Vector3.right * move * moveSpeed * Time.deltaTime;

            UpdateUI();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void SpawnEnemy()
    {
        if (gameOver) return;

        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;

        int count = Random.Range(2, 5);

        for (int i = 0; i < count; i++)
        {
            float randomX = Random.Range(-screenWidth, screenWidth);
            Vector3 spawnPos = new Vector3(randomX, Camera.main.orthographicSize + 1f, 0f);

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.tag = "Enemy";

            if (enemy.GetComponent<Collider2D>() == null)
                enemy.AddComponent<BoxCollider2D>();

            EnemyMover mover = enemy.AddComponent<EnemyMover>();
            mover.speed = enemyFallSpeed;
            mover.player = this;
        }

        // ⚡ Increase difficulty over time
        enemyFallSpeed += 0.2f;
        spawnInterval = Mathf.Max(0.5f, spawnInterval - 0.05f);

        CancelInvoke(nameof(SpawnEnemy));
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    public void AddScore()
    {
        score++;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            gameOver = true;

            // 💥 Explosion
            if (explosionEffect != null)
            {
                GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);

                ParticleSystem ps = fx.GetComponent<ParticleSystem>();
                if (ps != null)
                    ps.Play();
            }
            if (gameOverText != null)
                gameOverText.gameObject.SetActive(true);
        }
    }
}

public class EnemyMover : MonoBehaviour
{
    public float speed = 3f;
    public Player player;

    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;

        if (transform.position.y < -Camera.main.orthographicSize - 2f)
        {
            if (player != null)
                player.AddScore();

            Destroy(gameObject);
        }
    }
}