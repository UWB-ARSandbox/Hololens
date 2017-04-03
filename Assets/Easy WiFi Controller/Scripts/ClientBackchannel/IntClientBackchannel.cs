using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ClientBackchannels
{

    [AddComponentMenu("EasyWiFiController/Client/Backchannels/Int Backchannel")]
    public class IntClientBackchannel : MonoBehaviour, IClientBackchannel
    {

        public string controlName = "Int1";
        public string notifyMethod = "yourMethod";

        //runtime variables
        IntBackchannelType intBackchannel = new IntBackchannelType();
        string backchannelKey;

        void Awake()
        {
            backchannelKey = EasyWiFiController.registerControl(EasyWiFiConstants.BACKCHANNELTYPE_INT, controlName);
            intBackchannel = (IntBackchannelType)EasyWiFiController.controllerDataDictionary[backchannelKey];
        }

        // Update is called once per frame
        void Update()
        {
            //if we have a populated server key then we know where to look in the data structure
            if (intBackchannel.serverKey != null)
            {
                mapDataStructureToMethod();
            }
        }


        public void mapDataStructureToMethod()
        {
            SendMessage(notifyMethod, intBackchannel, SendMessageOptions.DontRequireReceiver);
        }
    }

}
