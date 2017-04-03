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
    private void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(
          this.gameObject);
    }
   
    

    public void OnInputClicked(InputEventData eventData)
    {
        Debug.Log("CLICK!");
        // Place the cursor at the calculated position.
        //Vector3 position = GazeManager.Instance.HitPosition + GazeManager.Instance.GazeNormal * DistanceFromCollision;


       // _trophy.Add(PhotonNetwork.Instantiate(Trophy.name, position, Quaternion.identity, 0));
    }
}