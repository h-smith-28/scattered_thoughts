using System.Collections;
using UnityEngine;

/// <summary>
/// Controls fullscreen eye effects through GLOBAL Shader Graph properties.
/// In BlurredBlink mode, red appears as closing starts, remains visible
/// while the eye opens, then fades near the end of opening. Blur also
/// holds through reopening, fading only near the end. All effects share
/// the same close -> hold -> open timeline (no extra delay).
///
/// Shader Graph property references must match:
///   _EyeFXBlink, _EyeFXRedness, _EyeFXBlurPixels, _EyeFXDarkness.
/// The user's current Graph has Blink, Redness and a three-sample blur.
/// </summary>
[DisallowMultipleComponent]
public sealed class EyeVisionFXController : MonoBehaviour
{
    public static EyeVisionFXController Instance { get; private set; }

    // Keep enum order unchanged so old scene's serialized selection is preserved.
    public enum AutomaticEffect
    {
        Blink,
        BlurredBlink,
        WakeUp,
        DamageFlash,
        Blackout
    }

    [Header("Automatic loop - no keyboard needed")]
    public bool automaticCycle = true;
    public AutomaticEffect automaticEffect = AutomaticEffect.BlurredBlink;
    [Min(0f)] public float firstTriggerDelaySeconds = 5f;
    [Min(0.5f)] public float repeatIntervalSeconds = 10f;

    [Header("Blink timing: 2 + 1 + 2 = 5 seconds")]
    [Min(0.01f)] public float closeSeconds = 2f;
    [Min(0f)] public float closedSeconds = 1f;
    [Min(0.01f)] public float openSeconds = 2f;

    [Header("Pause support (optional)")]
    [Tooltip("Assign the pause panel that is only ACTIVE while the game is paused. Leave empty if your pause system sets Time.timeScale = 0.")]
    public GameObject pauseMenuRoot;

    [Header("Synchronized red + blink")]
    [Tooltip("Redness starts as the eyelids close and stays strong during most of the opening, then reaches zero at the end.")]
    [Range(0f, 1f)] public float loopRedness = 0.65f;
    [Tooltip("Red quickly becomes visible after closing starts. Both effects still start together, with no delay.")]
    [Range(0.05f, 1f)] public float redFadeInSeconds = 0.25f;
    [Tooltip("During opening, hold maximum redness until near the end, then fade it to zero during the final N seconds. Set 0.5 for a 2-second opening to keep red visible for the first 1.5 seconds.")]
    [Min(0.05f)] public float redFadeOutSeconds = 0.5f;
    [Tooltip("Maximum blur strength during the blink (requires _EyeFXBlurPixels in Shader Graph).")]
    [Range(0f, 15f)] public float loopBlurPixels = 6f;
    [Tooltip("Blur reaches full strength during the first N seconds of closing; separate from eyelid progress.")]
    [Min(0.05f)] public float blurFadeInSeconds = 0.25f;
    [Tooltip("Blur stays at full strength while the eyes reopen, then fades only during the FINAL N seconds of opening; reaches zero exactly when the eyelids disappear.")]
    [Min(0.05f)] public float blurFadeOutSeconds = 0.35f;

    // Retained for compatibility with previous component data or callers.
    // The synchronized 5-second BlurredBlink does NOT add this time after opening.
    [HideInInspector][Min(0f)] public float loopRecoverySeconds = 1.5f;

    [Header("Other effects (optional)")]
    [Range(0f, 1f)] public float damageRedness = 0.90f;
    [Range(0f, 15f)] public float damageBlurPixels = 5f;
    [Range(0f, 15f)] public float wakeBlurPixels = 9f;
    [Min(0f)] public float wakeBlackHoldSeconds = 0.35f;
    [Min(0f)] public float blackoutHoldSeconds = 1f;

    public float BlinkAmount { get; private set; }
    public float Redness { get; private set; }
    public float BlurPixels { get; private set; }
    public float Darkness { get; private set; }

    private Coroutine runningEffect;
    private float nextAutoTime;
    private bool manualPaused;
    private bool wasPaused;

    // Change these names ONLY if your Shader Graph Blackboard uses different references.
    private static readonly int BlinkID = Shader.PropertyToID("_EyeFXBlink");
    private static readonly int RedID = Shader.PropertyToID("_EyeFXRedness");
    private static readonly int BlurID = Shader.PropertyToID("_EyeFXBlurPixels");
    private static readonly int DarkID = Shader.PropertyToID("_EyeFXDarkness");

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("EyeVisionFX: Only one controller should be enabled per scene.", this);
            enabled = false;
            return;
        }
        Instance = this;
        wasPaused = IsGamePaused();
        ApplyVision(0f, 0f, 0f, 0f);
        nextAutoTime = Time.time + Mathf.Max(0f, firstTriggerDelaySeconds);
    }

    private void Update()
    {
        if (IsGamePaused())
        {
            if (!wasPaused) EnterPause();
            return;
        }

        if (wasPaused)
        {
            wasPaused = false;
            // Do not play catch-up blink when the pause menu closes.
            nextAutoTime = Time.time + Mathf.Max(0.5f, repeatIntervalSeconds);
        }

        if (!automaticCycle || runningEffect != null || Time.time < nextAutoTime)
            return;

        switch (automaticEffect)
        {
            case AutomaticEffect.Blink: PlayBlink(); break;
            case AutomaticEffect.BlurredBlink: PlayBlurredBlink(); break;
            case AutomaticEffect.WakeUp: PlayWakeUp(); break;
            case AutomaticEffect.DamageFlash: PlayDamageFlash(); break;
            case AutomaticEffect.Blackout: PlayBlackout(blackoutHoldSeconds); break;
        }
    }

    /// <summary>Call from your PauseManager if the game does not use Time.timeScale = 0.</summary>
    public void SetPaused(bool paused)
    {
        manualPaused = paused;
        if (paused) EnterPause();
        // Update() will reset the loop timer when play resumes.
    }

    private bool IsGamePaused()
    {
        return manualPaused || Time.timeScale <= 0.0001f ||
               (pauseMenuRoot != null && pauseMenuRoot.activeInHierarchy);
    }

    private void EnterPause()
    {
        wasPaused = true;
        CancelEffect();
        // Never draw a black eyelid over the pause UI.
        ApplyVision(0f, 0f, 0f, 0f);
    }

    private void CancelEffect()
    {
        if (runningEffect == null) return;
        StopCoroutine(runningEffect);
        runningEffect = null;
    }

    private void OnDisable()
    {
        if (Instance != this) return;
        CancelEffect();
        ApplyVision(0f, 0f, 0f, 0f);
        Instance = null;
    }

    private void StartEffect(IEnumerator routine)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || IsGamePaused()) return;
        CancelEffect();
        // Interval is measured between effect STARTS, never overlays effects.
        nextAutoTime = Time.time + Mathf.Max(0.5f, repeatIntervalSeconds);
        runningEffect = StartCoroutine(routine);
    }

    [ContextMenu("Preview / Blink (Play mode)")]
    public void PlayBlink() => StartEffect(BlinkSequence());

    [ContextMenu("Preview / Blurred Blink (Play mode)")]
    public void PlayBlurredBlink() => StartEffect(SynchronizedBlurredBlinkSequence());

    [ContextMenu("Preview / Wake Up (Play mode)")]
    public void PlayWakeUp() => StartEffect(WakeSequence());

    [ContextMenu("Preview / Damage Flash (Play mode)")]
    public void PlayDamageFlash() => StartEffect(DamageSequence());

    public void PlayBlackout(float holdSeconds = 1f) =>
        StartEffect(BlackoutSequence(Mathf.Max(0f, holdSeconds)));

    public void SetVision(float blink, float redness, float blurPixels, float darkness)
    {
        if (IsGamePaused()) { EnterPause(); return; }
        CancelEffect();
        ApplyVision(blink, redness, blurPixels, darkness);
        nextAutoTime = Time.time + Mathf.Max(0.5f, repeatIntervalSeconds);
    }

    [ContextMenu("Preview / Reset Vision (Play mode)")]
    public void ResetVision() => SetVision(0f, 0f, 0f, 0f);

    private void ApplyVision(float blink, float red, float blur, float dark)
    {
        BlinkAmount = Mathf.Clamp01(blink);
        Redness = Mathf.Clamp01(red);
        BlurPixels = Mathf.Clamp(blur, 0f, 15f);
        Darkness = Mathf.Clamp01(dark);

        Shader.SetGlobalFloat(BlinkID, BlinkAmount);
        Shader.SetGlobalFloat(RedID, Redness);
        Shader.SetGlobalFloat(BlurID, BlurPixels);
        Shader.SetGlobalFloat(DarkID, Darkness);
    }

    private IEnumerator Transition(float blink, float red, float blur, float dark, float seconds)
    {
        float startBlink = BlinkAmount;
        float startRed = Redness;
        float startBlur = BlurPixels;
        float startDark = Darkness;

        if (seconds <= 0f)
        {
            ApplyVision(blink, red, blur, dark);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime; // Stops when Time.timeScale is 0.
            float t = Mathf.Clamp01(elapsed / seconds);
            t = t * t * (3f - 2f * t); // SmoothStep, same timing for all values.

            ApplyVision(
                Mathf.Lerp(startBlink, blink, t),
                Mathf.Lerp(startRed, red, t),
                Mathf.Lerp(startBlur, blur, t),
                Mathf.Lerp(startDark, dark, t));
            yield return null;
        }
        ApplyVision(blink, red, blur, dark);
    }

    private static IEnumerator WaitGameSeconds(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator BlinkSequence()
    {
        yield return Transition(1f, 0f, 0f, 0f, closeSeconds);
        yield return WaitGameSeconds(closedSeconds);
        yield return Transition(0f, 0f, 0f, 0f, openSeconds);
        runningEffect = null;
    }

    private static float Smooth01(float value)
    {
        float t = Mathf.Clamp01(value);
        return t * t * (3f - 2f * t);
    }

    private IEnumerator SynchronizedBlurredBlinkSequence()
    {
        // One shared timeline: red begins with the first closing frame and
        // stays visible during most of opening, then fades near the end.
        float closing = Mathf.Max(0.01f, closeSeconds);
        float redRamp = Mathf.Min(closing, Mathf.Max(0.01f, redFadeInSeconds));
        float blurRamp = Mathf.Min(closing, Mathf.Max(0.05f, blurFadeInSeconds));
        float elapsed = 0f;
        while (elapsed < closing)
        {
            elapsed += Time.deltaTime;
            float eyeT = Smooth01(elapsed / closing);
            float redT = Smooth01(elapsed / redRamp);
            float blurT = Smooth01(elapsed / blurRamp);
            // Blur becomes strong early while the eye is still open enough to see it.
            ApplyVision(eyeT, loopRedness * redT, loopBlurPixels * blurT, 0f);
            yield return null;
        }

        ApplyVision(1f, loopRedness, loopBlurPixels, 0f);
        yield return WaitGameSeconds(closedSeconds);

        float opening = Mathf.Max(0.01f, openSeconds);
        elapsed = 0f;
        // The eyelids start opening immediately, but red stays strong until the
        // final part of the opening. Otherwise it becomes invisible while the
        // still-closed eyelids hide most of the screen.
        float redFadeDuration = Mathf.Min(opening, Mathf.Max(0.05f, redFadeOutSeconds));
        float redFadeStart = opening - redFadeDuration;
        float blurFadeDuration = Mathf.Min(opening, Mathf.Max(0.05f, blurFadeOutSeconds));
        float blurFadeStart = opening - blurFadeDuration;
        while (elapsed < opening)
        {
            elapsed += Time.deltaTime;
            float t = Smooth01(elapsed / opening);
            float eyelidAmount = 1f - t;

            // No extra wait or second coroutine: synchronized to the SAME 5s timeline.
            float fadeProgress = Smooth01((elapsed - redFadeStart) / redFadeDuration);
            float redAmount = loopRedness * (1f - fadeProgress);

            // IMPORTANT: Do not scale blur by eyelidAmount here. That used to
            // make blur vanish as soon as the eyelids opened enough to see it.
            // Hold full blur through opening, then fade in the final fraction.
            float blurFadeProgress = Smooth01((elapsed - blurFadeStart) / blurFadeDuration);
            float blurAmount = loopBlurPixels * (1f - blurFadeProgress);
            ApplyVision(eyelidAmount, redAmount, blurAmount, 0f);
            yield return null;
        }

        ApplyVision(0f, 0f, 0f, 0f);
        runningEffect = null;
    }

    private IEnumerator DamageSequence()
    {
        yield return Transition(0.26f, damageRedness, damageBlurPixels, 0.24f, 0.07f);
        yield return WaitGameSeconds(0.10f);
        yield return Transition(0f, damageRedness * 0.5f,
                                damageBlurPixels * 0.55f, 0.08f, 0.24f);
        yield return Transition(0f, 0f, 0f, 0f, 0.80f);
        runningEffect = null;
    }

    private IEnumerator WakeSequence()
    {
        ApplyVision(1f, 0.3f, wakeBlurPixels, 0.2f);
        yield return WaitGameSeconds(wakeBlackHoldSeconds);
        yield return Transition(0.23f, 0.4f, wakeBlurPixels, 0.1f, 1.10f);
        yield return Transition(0f, 0.23f, wakeBlurPixels * 0.75f, 0.03f, 0.58f);
        yield return Transition(0f, 0f, 0f, 0f, 1.80f);
        runningEffect = null;
    }

    private IEnumerator BlackoutSequence(float holdSeconds)
    {
        yield return Transition(1f, 0f, 0f, 0f, closeSeconds);
        yield return WaitGameSeconds(holdSeconds);
        yield return Transition(0f, 0f, 0f, 0f, openSeconds);
        runningEffect = null;
    }
}
