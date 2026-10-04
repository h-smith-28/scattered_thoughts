
using UnityEngine;
using System.Collections;

public class FlickerAndFadeLamp : MonoBehaviour
{
    public Light pointLight;
    public Material lampMaterial;

    public Color lampColor = Color.magenta;

    public float normalIntensity = 2f;
    public float normalEmission = 5f;

    public float flickerIntensity = 0.2f;
    public float flickerEmission = 0.3f;

    public float flickerDelay = 0.12f;
    public float fadeDuration = 3f;

    void Start()
    {
        StartCoroutine(LampSequence());
    }

    IEnumerator LampSequence()
    {
        SetLamp(normalIntensity, normalEmission);

        yield return new WaitForSeconds(2f);

        // 第一次闪
        SetLamp(flickerIntensity, flickerEmission);
        yield return new WaitForSeconds(flickerDelay);

        SetLamp(normalIntensity, normalEmission);
        yield return new WaitForSeconds(flickerDelay);

        // 第二次闪
        SetLamp(flickerIntensity, flickerEmission);
        yield return new WaitForSeconds(flickerDelay);

        SetLamp(normalIntensity, normalEmission);
        yield return new WaitForSeconds(0.3f);

        // 慢慢熄灭
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            float currentLight = Mathf.Lerp(normalIntensity, 0f, t);
            float currentEmission = Mathf.Lerp(normalEmission, 0f, t);

            SetLamp(currentLight, currentEmission);

            yield return null;
        }

        SetLamp(0f, 0f);
    }

    void SetLamp(float lightIntensity, float emissionStrength)
    {
        pointLight.intensity = lightIntensity;

        lampMaterial.SetColor(
            "_EmissionColor",
            lampColor * emissionStrength
        );
    }
}