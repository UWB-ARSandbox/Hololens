//----------------------------------------------------------------------------
// File:        WorldCursor.cs
// Authors:     Original script by HolotoolKit from Microsoft
//              which was heavily modified by Koukeng Yang (Keng)
// DateCreated: July 2016
//
// Notes:       This script is attached to cursor object that the user of the
//              hololens sees during program runtime.
//
//----------------------------------------------------------------------------
using UnityEngine;

namespace UWBNetworkingPackage.HoloLensDemo
{
    public class WorldCursor : MonoBehaviour
    {
        private MeshRenderer meshRenderer;

        // Use this for initialization
        void Start()
        {
            // Grab the mesh renderer that's on the same object as this script.
            meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            // Do a raycast into the world based on the user's
            // head position and orientation.
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
            {
                // If the raycast hit a hologram...

                // Display the cursor mesh.
                meshRenderer.enabled = true;
                // Move the cursor to the point where the raycast hit.
                this.transform.position = hitInfo.point;
                // Rotate the cursor to hug the surface of the hologram.
                this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
            else
            {
                // If the raycast did not hit a hologram, hide the cursor mesh.
                meshRenderer.enabled = false;
            }
        }
    }
}


