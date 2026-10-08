using UnityEngine;

public class s_GameStates : MonoBehaviour
{
    public float stressLevel = 0.0f;
    s_GameMenus gameMenus;

    void Start()
    {
        gameMenus = FindAnyObjectByType<s_GameMenus>();
    }

    void Update()
    {
        stressLevel += Time.deltaTime;
        if(stressLevel >= 60)
        {
            EndGameState();
        }
    }

    void EndGameState()
    {
        gameMenus?.ShowLose();
    }
}
