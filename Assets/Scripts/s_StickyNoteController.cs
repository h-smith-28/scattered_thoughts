/**
* Attached to sticky notes to give provided behaviour
*/
using UnityEngine;

public class s_StickyNoteController : MonoBehaviour
{
    // Collider for the trigger on back of sticky note
    // Will face into the wall and tell sticky note when it is "stuck" to something
    [SerializeField]
    private Collider stickyCollider;

    [SerializeField]
    private Rigidbody rb;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
        {
            //Debug.Log("sticking");
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
            //Debug.Log("Unsticking");
            rb.constraints = RigidbodyConstraints.None;
            //rb.useGravity = true;
        }
    }
}
