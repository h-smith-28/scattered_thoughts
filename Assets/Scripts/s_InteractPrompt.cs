using UnityEngine;

public class s_InteractPrompt : MonoBehaviour
{
    [SerializeField] private GameObject prompt;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 4f;

    void Update()
    {
        prompt.SetActive(LookingAtFreeStation());
    }

    bool LookingAtFreeStation()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange))
            return false;

        s_ComputerStation station = hit.collider.GetComponentInParent<s_ComputerStation>();
        if(station != null && !station.IsBusy)
            return true;

        s_MinigameObject minigameStation = hit.collider.GetComponentInParent<s_MinigameObject>();
        if(minigameStation != null && !minigameStation.IsBusy)
            return true;
        
        return false;
    }
}