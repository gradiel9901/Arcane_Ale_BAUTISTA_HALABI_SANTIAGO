using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State state;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer;
    private float elapsedGameTime; // To track total elapsed time
    [SerializeField] private float gamePlayingTimerMax = 60f;
    private bool isGamePaused = false;

    private float timeAccelerationInterval = 10f; // Interval in seconds to decrease the timer
    private float timeSinceLastDecrease = 0f; // Tracks time since the last decrease
    private int deductionMultiplier = 1; // Multiplier for the time deduction (1x = 3 seconds, 2x = 6 seconds, etc.)

    private void Awake()
    {
        Instance = this;
        state = State.WaitingToStart;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state == State.WaitingToStart)
        {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void DeductTimeOnDishSubmit(float timeToDeduct)
    {
        if (state == State.GamePlaying)
        {
            gamePlayingTimer -= timeToDeduct;
            // Ensure the timer doesn't go below zero
            gamePlayingTimer = Mathf.Max(gamePlayingTimer, 0f);
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f)
                {
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    elapsedGameTime = 0f; // Reset elapsed time when the game starts
                    timeSinceLastDecrease = 0f; // Reset the interval tracker
                    deductionMultiplier = 1; // Start the multiplier at 1
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                // Update timers
                gamePlayingTimer -= Time.deltaTime;
                elapsedGameTime += Time.deltaTime;
                timeSinceLastDecrease += Time.deltaTime;

                // Decrease timer by incrementing amounts every 10 seconds
                if (timeSinceLastDecrease >= timeAccelerationInterval)
                {
                    float timeToDeduct = 3f * deductionMultiplier; // Calculate deduction based on multiplier
                    gamePlayingTimer -= timeToDeduct;
                    deductionMultiplier++; // Increase the multiplier for the next interval
                    timeSinceLastDecrease = 0f; // Reset the interval tracker
                }

                // Ensure the timer does not go below zero
                if (gamePlayingTimer < 0f)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameOver:
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (gamePlayingTimer / gamePlayingTimerMax);
    }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;
        if (isGamePaused)
        {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddTimeOnDishSubmit(float timeToAdd)
    {
        if (state == State.GamePlaying)
        {
            gamePlayingTimer += timeToAdd;
            // Ensure the timer does not exceed the max allowed time
            gamePlayingTimer = Mathf.Min(gamePlayingTimer, gamePlayingTimerMax);
        }
    }

    public float GetElapsedTime()
    {
        return elapsedGameTime; // Return the total elapsed time
    }
}
