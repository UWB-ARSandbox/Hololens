using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ClientBackchannels
{

    [AddComponentMenu("EasyWiFiController/Client/Backchannels/Bool Backchannel")]
    public class BoolClientBackchannel : MonoBehaviour, IClientBackchannel
    {

        public string controlName = "Bool1";
        public string notifyMethod = "yourMethod";
        [Tooltip("Determines when your Notify Method gets called")]
        public EasyWiFiConstants.CALL_TYPE callType = EasyWiFiConstants.CALL_TYPE.Every_Frame;

        //runtime variables
        BoolBackchannelType boolBackchannel = new BoolBackchannelType();
        string backchannelKey;
        bool lastValue = false;

        void Awake()
        {
            backchannelKey = EasyWiFiController.registerControl(EasyWiFiConstants.BACKCHANNELTYPE_BOOL, controlName);
            boolBackchannel = (BoolBackchannelType)EasyWiFiController.controllerDataDictionary[backchannelKey];
        }

        // Update is called once per frame
        void Update()
        {
            //if we have a populated server key then we know where to look in the data structure
            if (boolBackchannel.serverKey != null)
            {
                mapDataStructureToMethod();
            }
        }


        public void mapDataStructureToMethod()
        {
            if (callType == EasyWiFiConstants.CALL_TYPE.Every_Frame)
                SendMessage(notifyMethod, boolBackchannel, SendMessageOptions.DontRequireReceiver);
            else
            {
                if (lastValue != boolBackchannel.BOOL_VALUE)
                {
                    SendMessage(notifyMethod, boolBackchannel, SendMessageOptions.DontRequireReceiver);
                }
                lastValue = boolBackchannel.BOOL_VALUE;
            }
        }
    }

}
