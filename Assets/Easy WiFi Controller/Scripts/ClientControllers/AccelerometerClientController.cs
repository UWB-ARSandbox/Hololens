using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/UserControls/Accelerometer")]
    public class AccelerometerClientController : MonoBehaviour, IClientController
    {
        public string controlName = "Accelerometer";

        AccelerometerControllerType accelerometer;
        string accelerometerKey;

        // Use this for initialization
        void Awake()
        {
            accelerometerKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_ACCELEROMETER, controlName);
            accelerometer = (AccelerometerControllerType)EasyWiFiController.controllerDataDictionary[accelerometerKey];
        }


        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void mapInputToDataStream()
        {
            Vector3 accel;
            accel.x = 0f; accel.y = 0f; accel.z = 0f;

            //acceleromter sensor
            accel = Input.acceleration;

            accelerometer.ACCELERATION_X = accel.x;
            accelerometer.ACCELERATION_Y = accel.y;
            accelerometer.ACCELERATION_Z = accel.z;
        }

    }

}