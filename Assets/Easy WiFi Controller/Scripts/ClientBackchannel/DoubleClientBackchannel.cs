using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ClientBackchannels
{

    [AddComponentMenu("EasyWiFiController/Client/Backchannels/Double Backchannel")]
    public class DoubleClientBackchannel : MonoBehaviour, IClientBackchannel
    {

        public string controlName = "Double1";
        public string notifyMethod = "yourMethod";

        //runtime variables
        DoubleBackchannelType doubleBackchannel = new DoubleBackchannelType();
        string backchannelKey;

        void Awake()
        {
            backchannelKey = EasyWiFiController.registerControl(EasyWiFiConstants.BACKCHANNELTYPE_DOUBLE, controlName);
            doubleBackchannel = (DoubleBackchannelType)EasyWiFiController.controllerDataDictionary[backchannelKey];
        }

        // Update is called once per frame
        void Update()
        {
            //if we have a populated server key then we know where to look in the data structure
            if (doubleBackchannel.serverKey != null)
            {
                mapDataStructureToMethod();
            }
        }


        public void mapDataStructureToMethod()
        {
            SendMessage(notifyMethod, doubleBackchannel, SendMessageOptions.DontRequireReceiver);
        }
    }

}
