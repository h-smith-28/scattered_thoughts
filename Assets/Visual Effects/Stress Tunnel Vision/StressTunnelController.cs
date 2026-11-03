
using UnityEngine;
using UnityEngine.UI;

public class StressTunnelController : MonoBehaviour
{
    public s_GameStates gameStates;

    [Header("Stress Range")]
    public float startStress = 6f;
    public float maxStress = 50f;

    [Header("Vision Radius")]
    public float maxRadius = 0.71f;
    public float minRadius = 0.28f;

    [Header("Transition Speed")]
    public float shrinkSpeed = 1.2f;
    public float recoverSpeed = 15f;

    private RawImage overlay;
    private Material materialInstance;
    private float currentRadius;

    void Start()
    {
        overlay = GetComponent<RawImage>();

        if (gameStates == null)
            gameStates = FindFirstObjectByType<s_GameStates>();

        if (overlay == null || gameStates == null ||
            overlay.material == null)
        {
            Debug.LogError("Stress Tunnel: Missing reference!");
            enabled = false;
            return;
        }

        materialInstance = new Material(overlay.material);
        overlay.material = materialInstance;

        currentRadius = maxRadius;
        materialInstance.SetFloat("_Radius", currentRadius);
    }

    void Update()
    {
        float stressValue = gameStates.stressLevel;

        float progress = Mathf.InverseLerp(
            startStress,
            maxStress,
            stressValue
        );

        // Make the black border visible earlier
        // while keeping the overall transition gradual
        progress = Mathf.Pow(progress, 0.5f);

        float targetRadius = Mathf.Lerp(
            maxRadius,
            minRadius,
            progress
        );

        // Slow shrinking, fast recovery
        float speed = targetRadius > currentRadius
            ? recoverSpeed
            : shrinkSpeed;

        currentRadius = Mathf.Lerp(
            currentRadius,
            targetRadius,
            1f - Mathf.Exp(-speed * Time.deltaTime)
        );

        currentRadius = Mathf.Clamp(
            currentRadius,
            minRadius,
            maxRadius
        );

        materialInstance.SetFloat("_Radius", currentRadius);
    }

    void OnDestroy()
    {
        if (materialInstance != null)
            Destroy(materialInstance);
    }
}
