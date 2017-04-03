using UnityEngine;
using System.Collections;
using System;

namespace UWB_MixedRealityNetworking_HoloLensDemo
{
    public class HeadTracking : MonoBehaviour
    {

        public bool IsHololens;

        private GameObject _camera;

        // Use this for initialization
        void Start()
        {
            try
            {
                _camera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            catch (Exception e)
            {
                // Catch null reference. 
                Debug.LogError("Need a \"MainCamera\" object for HoloLens: " + e.StackTrace);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_camera == null || !IsHololens) return;
            this.transform.position = _camera.transform.position;
            this.transform.rotation = _camera.transform.rotation;
        }
    }
}
