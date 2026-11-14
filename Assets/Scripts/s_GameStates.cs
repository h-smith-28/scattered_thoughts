
using UnityEngine;
using UnityEngine.UI;

public class s_GameStates : MonoBehaviour
{
    public float stressLevel = 0.0f;
    s_GameMenus gameMenus;
    public Text stressNum;
    public AudioSource heartbeatSFX;

    void Start()
    {
        gameMenus = FindAnyObjectByType<s_GameMenus>();
    }

    void Update()
    {
        stressLevel += Time.deltaTime;

        if (stressNum != null)
        {
            stressNum.text = Mathf.Round(stressLevel).ToString();
        }

        if (stressLevel >= 80)
        {
            EndGameState();
        }

        if(stressLevel >= 60)
        {
            heartbeatSFX.Play();
        }
    }

    void EndGameState()
    {
        gameMenus?.ShowLose();
    }

    public void AdjustStress(float stressGain)
    {
        stressLevel += stressGain;
        if(stressLevel <= 0)
            stressLevel = 0.0f;
    }
}
