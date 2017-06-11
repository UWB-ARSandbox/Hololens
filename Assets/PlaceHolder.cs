using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;

public class PlaceHolder : MonoBehaviour, IInputClickHandler
{
    [Tooltip("The cube object to be instantiated (must exist in the Photon Resources folder")]
    public GameObject Trophy;
    private readonly ArrayList _trophy = new ArrayList();  // Stores all sphere instances
    [Tooltip("Distance, in meters, to offset the cursor from the collision point.")]
    public float DistanceFromCollision = 0f;
    bool loaded;
    private GameObject phone;
    private Controller controller;
    private void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(
          this.gameObject);
        phone = GameObject.Find("phone");
        controller = phone.GetComponent<Controller>();
    }
   
    

    public void OnInputClicked(InputEventData eventData)
    {
        
    }
}