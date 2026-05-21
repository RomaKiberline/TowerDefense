using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { WaitingToStart, Preparation, Battle, RoundEnd }
    public GameState currentState;

    [Header("Параметри гравця")]
    public int gold = 300;
    public int baseHealth = 20;

    [Header("Таймер підготовки")]
    public float preparationTime = 10f;
    private float currentTimer;

    [Header("UI Елементи")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI stateText;

    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject startWaveButton;
    public GameObject shopPanel;

    private bool waveStarted = false;

    void Awake()
    {
        instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        UpdateUI();
        ChangeState(GameState.WaitingToStart);
    }

    void Update()
    {
        if (currentState == GameState.Preparation)
        {
            currentTimer -= Time.deltaTime;

            if (stateText != null)
            {
                stateText.text = "Next wave in: " + Mathf.Ceil(currentTimer) + "s";
            }

            if (currentTimer <= 0f && !waveStarted)
            {
                waveStarted = true;

                Spawner spawner = FindAnyObjectByType<Spawner>();
                if (spawner != null)
                {
                    spawner.StartNextWave();
                }
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        if (newState == GameState.Preparation)
        {
            currentTimer = preparationTime;
            waveStarted = false;
        }

        UpdateUI();

        switch (currentState)
        {
            case GameState.WaitingToStart:
                if (stateText != null)
                    stateText.text = "Click Start to Begin";

                if (startWaveButton != null)
                    startWaveButton.SetActive(true);

                if (shopPanel != null)
                    shopPanel.SetActive(true);
                break;

            case GameState.Preparation:
                if (startWaveButton != null)
                    startWaveButton.SetActive(false);

                if (shopPanel != null)
                    shopPanel.SetActive(true);
                break;

            case GameState.Battle:
                if (stateText != null)
                    stateText.text = "Wave started!";

                if (startWaveButton != null)
                    startWaveButton.SetActive(false);

                if (shopPanel != null)
                    shopPanel.SetActive(false);
                break;

            case GameState.RoundEnd:
                if (startWaveButton != null)
                    startWaveButton.SetActive(false);

                if (shopPanel != null)
                    shopPanel.SetActive(false);

                HandleRoundEnd();
                break;
        }
    }

    public void OnStartButtonClicked()
    {
        if (currentState == GameState.WaitingToStart)
        {
            ChangeState(GameState.Preparation);
        }
    }

    void HandleRoundEnd()
    {
        AddGold(100);
        ChangeState(GameState.Preparation);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI();
    }

    public bool SpendGold(int amount)
    {
        if (currentState == GameState.RoundEnd || currentState == GameState.Battle)
            return false;

        if (gold >= amount)
        {
            gold -= amount;
            UpdateUI();
            return true;
        }

        return false;
    }

    public void TakeDamage(int damage)
    {
        baseHealth -= damage;
        UpdateUI();

        if (baseHealth <= 0)
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }

    public void WinGame()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    void UpdateUI()
    {
        if (goldText != null)
            goldText.text = " " + gold;

        if (healthText != null)
            healthText.text = " " + baseHealth;
    }
}