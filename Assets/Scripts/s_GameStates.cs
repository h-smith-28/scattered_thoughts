using UnityEngine;
using UnityEngine.UI;

public class s_GameStates : MonoBehaviour
{
    public float stressLevel = 0.0f;
    s_GameMenus gameMenus;
    public Text stressNum;

    void Start()
    {
        gameMenus = FindAnyObjectByType<s_GameMenus>();
    }

    void Update()
    {
        stressLevel += Time.deltaTime;
        stressNum.text = Mathf.Round(stressLevel).ToString();
        if (stressLevel >= 60)
        {
            EndGameState();
        }
    }

    void EndGameState()
    {
        gameMenus?.ShowLose();
    }
}
