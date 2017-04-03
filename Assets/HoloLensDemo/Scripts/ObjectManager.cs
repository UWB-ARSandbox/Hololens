using UnityEngine;
using System.Collections;
using HoloToolkit.Unity.InputModule;
using Photon;
using Random = System.Random;
using System;



namespace UWBNetworkingPackage.HoloLensDemo
{
    /// <summary>
    /// Script that allows a HoloLens to instantiate/move sphere and cube objects
    /// (located in the Photon Resources folder)
    /// Also instantiates and tracks a sphere that acts as the HoloLens avatar
    /// </summary>
    public class ObjectManager : PunBehaviour
    {
        [Tooltip("The cube object to be instantiated (must exist in the Photon Resources folder")]
        public GameObject Cube;

        [Tooltip("The sphere object to be instantiated (must exist in the Photon Resources folder")]
        public GameObject Sphere;

        [Tooltip("The HoloLens head object to be instantiated (must exist in the Photon Resources folder")]
        public GameObject HololensHead;

        [Tooltip("The HoloLens head object to be instantiated (must exist in the Photon Resources folder")]
        public GameObject Camera;

        [Tooltip("The cube object to be instantiated (must exist in the Photon Resources folder")]
        public GameObject Trophy;
        private readonly ArrayList _trophy = new ArrayList();  // Stores all sphere instances
       


        [Tooltip("Distance, in meters, to offset the cursor from the collision point.")]
        public float DistanceFromCollision = 1f;



        private readonly ArrayList _cubes = new ArrayList();    // Stores all cube instances
        private readonly ArrayList _spheres = new ArrayList();  // Stores all sphere instances



        /// <summary>
        /// On joined room, instantiates the HoloLens head avatar object (specified by user)
        /// </summary>
        public override void OnJoinedRoom()
        {
            PhotonNetwork.Instantiate(HololensHead.name, Vector3.zero, Quaternion.identity, 0);
        }

        /// <summary>
        /// Instantiates the cube object in the origin of the game space
        /// </summary>
        public void CreateCube()
        {
            // Place the cursor at the calculated position.
            Vector3 position = GazeManager.Instance.HitPosition + GazeManager.Instance.GazeNormal * DistanceFromCollision;

            
            _cubes.Add(PhotonNetwork.Instantiate(Cube.name, position, Quaternion.identity, 0));
        }

        /// <summary>
        /// Instantiates teh sphere object in the origin of the game space
        /// </summary>
        public void CreateSphere()
        {
            // Place the cursor at the calculated position.
            Vector3 position = GazeManager.Instance.HitPosition + GazeManager.Instance.GazeNormal * DistanceFromCollision;
            _spheres.Add(PhotonNetwork.Instantiate(Sphere.name, position, Quaternion.identity, 0));
        }

        public void CreateTrophy()
        {
            // Place the cursor at the calculated position.
            Vector3 position = GazeManager.Instance.HitPosition + GazeManager.Instance.GazeNormal * DistanceFromCollision;
            _trophy.Add(PhotonNetwork.Instantiate(Trophy.name, position, Quaternion.identity, 0));
        }

        public void SetCamera()
        {
            // Place the cursor at the calculated position.
      
            
                Vector3 position = GazeManager.Instance.HitPosition + GazeManager.Instance.GazeNormal * DistanceFromCollision;
                var headPosition = GameObject.Find("HoloLensCamera").transform.localPosition;
                GameObject spotHolder = new GameObject();
                spotHolder.transform.position = position;
                spotHolder.transform.LookAt(headPosition);
                PhotonNetwork.Instantiate(Camera.name, position, spotHolder.transform.rotation, 0);
           
            
           
        }

     
        
        public void ResetCamera()
        {
            Debug.Log("RESET!");
            GameObject virtualcam = GameObject.Find("VirtualCamera(Clone)");
            virtualcam.GetPhotonView().RequestOwnership();
            PhotonNetwork.Destroy(virtualcam);

        }


   

       
    }
}