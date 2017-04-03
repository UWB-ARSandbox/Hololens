using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/DataControls/String")]
    public class StringDataClientController : MonoBehaviour, IClientController
    {

        public string controlName = "StringData1";

        //we reuse the backchannel data types even though this is a forward channel
        StringBackchannelType stringData;
        string stringKey;

        //variable other script will modify via setValue to be sent across the network
        string value;

        // Use this for initialization
        void Awake()
        {
            stringKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_STRING, controlName);
            stringData = (StringBackchannelType)EasyWiFiController.controllerDataDictionary[stringKey];
        }

        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void setValue(string newValue)
        {
            value = newValue;
        }

        public void mapInputToDataStream()
        {
            //for properties DO NOT reset to default values becasue there isn't a default
            stringData.STRING_VALUE = value;
        }

    }

}