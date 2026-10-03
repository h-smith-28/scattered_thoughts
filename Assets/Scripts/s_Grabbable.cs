using UnityEngine;
using UnityEngine.InputSystem;

public class s_Grabbable : MonoBehaviour
{
    [SerializeField]
    private float lookDistance = 10.0f;
    [SerializeField]
    private float throwForce = 5.0f;

    [SerializeField]
    private Collider collider;
    private Rigidbody rb;

    private bool isGrabbed = false;
    private GameObject grabber;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if(isGrabbed && grabber)
        {
            transform.position = grabber.transform.position + grabber.transform.forward*lookDistance;
            transform.position = new Vector3(transform.position.x, transform.position.y + 3.0f, transform.position.z);
            if (InputSystem.actions.FindAction("Secondary").IsPressed())
            {
                rb.angularVelocity = Vector3.zero;
                transform.Rotate(Mouse.current.delta.ReadValue().y, 0, -Mouse.current.delta.ReadValue().x);
            }
        }
    }

    public void Grab(GameObject owner)
    {
        isGrabbed = true;
        grabber = owner;
        rb.useGravity = false;
        transform.rotation = grabber.transform.rotation * Quaternion.Euler(0,0,0);
        rb.angularVelocity = Vector3.zero;
        collider.enabled = false;
    }

    public void Drop(bool willThrow)
    {
        if (grabber)
        {
            isGrabbed = false;
            if(willThrow)
                rb.AddForce(grabber.GetComponentInChildren<Camera>().transform.forward * throwForce, ForceMode.Impulse);
            rb.useGravity = true;
            grabber = null;
            collider.enabled = true;
        }
    }

    public Collider GetCollider()
    {
        return collider;
    }
}
