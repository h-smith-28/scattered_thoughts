using UnityEngine;

public class s_StickyNoteController : MonoBehaviour
{
    [SerializeField]
    private Collider stickyCollider;

    [SerializeField]
    private Rigidbody rb;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
        {
            Debug.Log("sticking");
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag != "Player")
        {
            Debug.Log("Unsticking");
            rb.constraints = RigidbodyConstraints.None;   
        }
    }
}
