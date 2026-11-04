
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class s_RhythmMinigame : MonoBehaviour
{
    [Header("Arrow Settings")]
    public s_RhythmArrow arrowPrefab;
    public RectTransform arrowParent;
    public RectTransform centerTarget;

    [Header("Spawn Settings")]
    public float spawnDistance = 600f;
    public float spawnInterval = 1f;
    public float arrowTravelTime = 2f;

    // Final percentage of travel during which an arrow can be hit.
    // 0.1 = final 10% of travel.
    public float hitWindow = 0.1f;

    [Header("Gameplay")]
    public int requiredSuccesses = 10;

    private List<s_RhythmArrow> activeArrows =
        new List<s_RhythmArrow>();

    private float spawnTimer;
    private int successes;

    private bool gameStarted;

    private s_MinigameObject owner;

    public GameObject canvas;

    public static s_RhythmMinigame Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartGame(s_MinigameObject ownerObj)
    {
        canvas.SetActive(true);

        owner = ownerObj;

        successes = 0;
        spawnTimer = 0f;

        activeArrows.Clear();

        gameStarted = true;
    }

    private void Update()
    {
        if (gameStarted)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnRandomArrow();
            }

            CheckInput();
        }
    }

    private void SpawnRandomArrow()
    {
        ArrowDirection direction =
            (ArrowDirection)Random.Range(0, 4);

        s_RhythmArrow arrow = Instantiate(
            arrowPrefab,
            arrowParent
        );

        Vector2 spawnPosition = GetSpawnPosition(direction);
        Vector2 targetPosition = centerTarget.anchoredPosition;

        arrow.Initialize(
            direction,
            spawnPosition,
            targetPosition,
            arrowTravelTime
        );

        activeArrows.Add(arrow);
    }

    private Vector2 GetSpawnPosition(ArrowDirection direction)
    {
        switch (direction)
        {
            case ArrowDirection.Up:
                return centerTarget.anchoredPosition
                    + Vector2.up * spawnDistance;

            case ArrowDirection.Down:
                return centerTarget.anchoredPosition
                    + Vector2.down * spawnDistance;

            case ArrowDirection.Left:
                return centerTarget.anchoredPosition
                    + Vector2.left * spawnDistance;

            case ArrowDirection.Right:
                return centerTarget.anchoredPosition
                    + Vector2.right * spawnDistance;
        }

        return centerTarget.anchoredPosition;
    }

    private void CheckInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            TryHitArrow(ArrowDirection.Up);

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            TryHitArrow(ArrowDirection.Down);

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            TryHitArrow(ArrowDirection.Left);

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            TryHitArrow(ArrowDirection.Right);
    }

    private void TryHitArrow(ArrowDirection direction)
    {
        s_RhythmArrow closestArrow = null;
        float closestProgress = -1f;

        foreach (s_RhythmArrow arrow in activeArrows)
        {
            if (arrow == null || arrow.IsFinished)
                continue;

            if (arrow.direction != direction)
                continue;

            if (arrow.Progress >= 1f - hitWindow &&
                arrow.Progress <= 1f)
            {
                if (arrow.Progress > closestProgress)
                {
                    closestProgress = arrow.Progress;
                    closestArrow = arrow;
                }
            }
        }

        if (closestArrow != null)
        {
            closestArrow.Hit();
            activeArrows.Remove(closestArrow);

            successes++;

            Debug.Log("Successful rhythm hit: " + successes);

            if (successes >= requiredSuccesses)
            {
                EndGame(true);
            }
        }
    }

    public void MissArrow(s_RhythmArrow arrow)
    {
        activeArrows.Remove(arrow);

        arrow.Miss();

        if (gameStarted)
        {
            EndGame(false);
        }
    }

    public void EndGame(bool success)
    {
        if (!gameStarted)
            return;

        if (success)
        {
            Debug.Log("Succeeded rhythm minigame");
        }
        else
        {
            Debug.Log("Failed rhythm minigame");
        }

        gameStarted = false;

        foreach (s_RhythmArrow arrow in activeArrows)
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }

        activeArrows.Clear();

        canvas.SetActive(false);
        owner.Exit();
    }
}
