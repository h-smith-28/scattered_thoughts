using UnityEngine;

public class FlickerAndFadeLamp : MonoBehaviour
{
    [Header("Light")]
    public Light pointLight;

    [Header("Breathing Settings")]
    public float minIntensity = 60f;
    public float maxIntensity = 200f;

    [Tooltip("How many seconds one full breathing cycle takes.")]
    public float breathingDuration = 3f;

    void Update()
    {
        if (pointLight == null)
            return;
 
        float t =
            (Mathf.Sin(Time.time * Mathf.PI * 2f / breathingDuration) + 1f) / 2f;

 
        pointLight.intensity =
            Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}