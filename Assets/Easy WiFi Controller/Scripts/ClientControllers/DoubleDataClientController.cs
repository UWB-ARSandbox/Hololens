using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/DataControls/Double")]
    public class DoubleDataClientController : MonoBehaviour, IClientController
    {

        public string controlName = "DoubleData1";

        //we reuse the backchannel data types even though this is a forward channel
        DoubleBackchannelType doubleData;
        string doubleKey;

        //variable other script will modify via setValue to be sent across the network
        double value;

        // Use this for initialization
        void Awake()
        {
            doubleKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_DOUBLE, controlName);
            doubleData = (DoubleBackchannelType)EasyWiFiController.controllerDataDictionary[doubleKey];
        }

        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void setValue(double newValue)
        {
            value = newValue;
        }

        public void mapInputToDataStream()
        {
            //for properties DO NOT reset to default values becasue there isn't a default
            doubleData.DOUBLE_VALUE = value;
        }

    }

}