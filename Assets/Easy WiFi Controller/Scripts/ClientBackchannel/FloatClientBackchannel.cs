using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ClientBackchannels
{

    [AddComponentMenu("EasyWiFiController/Client/Backchannels/Float Backchannel")]
    public class FloatClientBackchannel : MonoBehaviour, IClientBackchannel
    {

        public string controlName = "Float1";
        public string notifyMethod = "yourMethod";

        //runtime variables
        FloatBackchannelType floatBackchannel = new FloatBackchannelType();
        string backchannelKey;

        void Awake()
        {
            backchannelKey = EasyWiFiController.registerControl(EasyWiFiConstants.BACKCHANNELTYPE_FLOAT, controlName);
            floatBackchannel = (FloatBackchannelType)EasyWiFiController.controllerDataDictionary[backchannelKey];
        }

        // Update is called once per frame
        void Update()
        {
            //if we have a populated server key then we know where to look in the data structure
            if (floatBackchannel.serverKey != null)
            {
                mapDataStructureToMethod();
            }
        }


        public void mapDataStructureToMethod()
        {
            SendMessage(notifyMethod, floatBackchannel, SendMessageOptions.DontRequireReceiver);
        }
    }

}
