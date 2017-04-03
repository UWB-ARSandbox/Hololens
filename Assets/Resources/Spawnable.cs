using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class Spawnable : MonoBehaviour, IInputClickHandler
{
    static int sphereCount;
    int sphereNumber;
    Rigidbody rigidBody;
    public bool calibrate=false;
    public bool selected = false;
    void Start()
    {
        this.gameObject.GetComponent<HandDraggable>().StartedDragging += OnStartedDragging;
        this.gameObject.GetComponent<HandDraggable>().StoppedDragging += OnStoppedDragging;
        rigidBody = this.gameObject.GetComponent<Rigidbody>();
        this.sphereNumber = ++sphereCount;
    }
    
    void OnMouseDown()
    {
        Debug.Log("CLick");
        calibrate = true;
    }

    public void OnInputClicked(InputEventData eventData)
    {
        if(selected)
            calibrate = true;
    }

    void OnStartedDragging()
    {
        // When we start dragging we clear out any existing world anchor for this
        //  Spawnable.
        WorldAnchorManager.Instance.RemoveAnchor(this.gameObject);
        Debug.Log(this.gameObject.GetPhotonView().isMine);
       
        if (!this.gameObject.GetPhotonView().isMine)
            this.gameObject.GetPhotonView().RequestOwnership();
        // But we don't want it to fall on the floor on its own.
       
    }
    void OnStoppedDragging()
    {
       

        // But we don't want it to fall on the floor on its own.
     
        this.gameObject.GetPhotonView().TransferOwnership(1);
        // Now remember where the Spawnable was.
        WorldAnchorManager.Instance.AttachAnchor(this.gameObject, this.sphereNumber.ToString());

    }
    void OnCollisionEnter(Collision collision)
    {
        // If we do get a collision then we (a bit rudley) just stop the
        // drag operation.
        //this.gameObject.GetComponent<HandDraggable>().StopDragging();
    }
}