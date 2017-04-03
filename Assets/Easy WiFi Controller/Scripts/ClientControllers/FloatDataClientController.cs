using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/DataControls/Float")]
    public class FloatDataClientController : MonoBehaviour, IClientController
    {

        public string controlName = "FloatData1";

        //we reuse the backchannel data types even though this is a forward channel
        FloatBackchannelType floatData;
        string floatKey;

        //variable other script will modify via setValue to be sent across the network
        float value;

        // Use this for initialization
        void Awake()
        {
            floatKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_FLOAT, controlName);
            floatData = (FloatBackchannelType)EasyWiFiController.controllerDataDictionary[floatKey];
        }

        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void setValue(float newValue)
        {
            value = newValue;
        }

        public void mapInputToDataStream()
        {
            //for properties DO NOT reset to default values becasue there isn't a default
            floatData.FLOAT_VALUE = value;
        }

    }

}