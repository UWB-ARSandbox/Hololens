using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/DataControls/Decimal")]
    public class DecimalDataClientController : MonoBehaviour, IClientController
    {

        public string controlName = "DecimalData1";

        //we reuse the backchannel data types even though this is a forward channel
        DecimalBackchannelType decimalData;
        string decimalKey;

        //variable other script will modify via setValue to be sent across the network
        decimal value;

        // Use this for initialization
        void Awake()
        {
            decimalKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_DECIMAL, controlName);
            decimalData = (DecimalBackchannelType)EasyWiFiController.controllerDataDictionary[decimalKey];
        }

        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void setValue(decimal newValue)
        {
            value = newValue;
        }

        public void mapInputToDataStream()
        {
            //for properties DO NOT reset to default values becasue there isn't a default
            decimalData.DECIMAL_VALUE = value;
        }

    }

}