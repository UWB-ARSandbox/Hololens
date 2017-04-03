using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/UserControls/Gyro")]
    public class GyroClientController : MonoBehaviour, IClientController
    {
        public string controlName = "Gyro";

        GyroControllerType gyro;
        string gyroKey;

        // Use this for initialization
        void Awake()
        {
            gyroKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_GYRO, controlName);
            gyro = (GyroControllerType)EasyWiFiController.controllerDataDictionary[gyroKey];
            if (SystemInfo.supportsGyroscope)
                Input.gyro.enabled = true;
        }


        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void mapInputToDataStream()
        {
            Quaternion gyroQuat;
            gyroQuat.w = 0f; gyroQuat.x = 0f; gyroQuat.y = 0f; gyroQuat.z = 0f;

            //acceleromter sensor
            gyroQuat = Input.gyro.attitude;

            gyro.GYRO_W = gyroQuat.w;
            gyro.GYRO_X = gyroQuat.x;
            gyro.GYRO_Y = gyroQuat.y;
            gyro.GYRO_Z = gyroQuat.z;
        }

    }

}