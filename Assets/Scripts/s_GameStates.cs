using UnityEngine;

public class s_GameStates : MonoBehaviour
{
    public float stressLevel = 0.0f;

    void Update()
    {
        stressLevel += Time.deltaTime;
        if(stressLevel >= 10)
        {
            EndGameState();
        }
    }

    void EndGameState()
    {
        Debug.Log("Unimplemented: end of game");
    }
}
