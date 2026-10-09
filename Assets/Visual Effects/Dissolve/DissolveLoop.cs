using UnityEngine;
using System.Collections;

public class DissolveLoop : MonoBehaviour
{
    public Renderer targetRenderer;

    [Range(0f, 1f)]
    public float maxDissolve = 1f;

   
    public float normalHoldTime = 3f;

   
    public float dissolveDuration = 20f;

   
    public float disappearedHoldTime = 5f;

    
    public float restoreDuration = 20f;

    private Material material;

    void Start()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        material = targetRenderer.material;

        material.SetFloat("_DissolveAmount", 0f);

        StartCoroutine(DissolveCycle());
    }

    IEnumerator DissolveCycle()
    {
        while (true)
        {
            
            material.SetFloat("_DissolveAmount", 0f);
            yield return new WaitForSeconds(normalHoldTime);

            yield return AnimateDissolve(
                0f,
                maxDissolve,
                dissolveDuration
            );

            material.SetFloat("_DissolveAmount", maxDissolve);
            yield return new WaitForSeconds(disappearedHoldTime);

            yield return AnimateDissolve(
                maxDissolve,
                0f,
                restoreDuration
            );
        }
    }

    IEnumerator AnimateDissolve(float start, float end, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);

           
            t = Mathf.SmoothStep(0f, 1f, t);

            float value = Mathf.Lerp(start, end, t);

            material.SetFloat("_DissolveAmount", value);

            yield return null;
        }

        material.SetFloat("_DissolveAmount", end);
    }
}