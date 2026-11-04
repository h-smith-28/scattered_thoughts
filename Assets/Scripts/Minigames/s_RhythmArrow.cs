using UnityEngine;
using UnityEngine.UI;

public enum ArrowDirection
{
    Up,
    Down,
    Left,
    Right
}

public class s_RhythmArrow : MonoBehaviour
{
    public ArrowDirection direction;

    public float moveDuration = 2f;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Vector2 targetPosition;

    public float Progress { get; private set; }

    public bool IsFinished { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(
        ArrowDirection dir,
        Vector2 start,
        Vector2 target,
        float duration)
    {
        direction = dir;
        startPosition = start;
        targetPosition = target;
        moveDuration = duration;

        rectTransform.anchoredPosition = startPosition;
        Progress = 0f;

        switch (direction)
        {
            case ArrowDirection.Up:
                rectTransform.localRotation = Quaternion.identity;
                break;

            case ArrowDirection.Down:
                rectTransform.localRotation = Quaternion.Euler(0, 0, 180);
                break;

            case ArrowDirection.Left:
                rectTransform.localRotation = Quaternion.Euler(0, 0, 90);
                break;

            case ArrowDirection.Right:
                rectTransform.localRotation = Quaternion.Euler(0, 0, -90);
                break;
        }
    }

    private void Update()
    {
        if (IsFinished)
            return;

        Progress += Time.deltaTime / moveDuration;

        rectTransform.anchoredPosition =
            Vector2.Lerp(startPosition, targetPosition, Progress);

        if (Progress >= 1f)
        {
            IsFinished = true;

            s_RhythmMinigame.Instance.MissArrow(this);
        }
    }

    public void Hit()
    {
        IsFinished = true;
        Destroy(gameObject);
    }

    public void Miss()
    {
        IsFinished = true;
        Destroy(gameObject);
    }
}