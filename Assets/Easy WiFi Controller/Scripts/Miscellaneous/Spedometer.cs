using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using EasyWiFi.Core;

[AddComponentMenu("EasyWiFiController/Miscellaneous/Spedometer")]
public class Spedometer : MonoBehaviour {

    RectTransform myRectTransform;

    float currentSpeed;
    Vector3 myRotation;

    void Start()
    {
        myRectTransform = this.gameObject.GetComponent<RectTransform>();
    }
    

    public void updateSpeed(FloatBackchannelType floatBackchannel)
    {
        //0mph(default) is rotation 10 and 220mph is rotation -190
        //usually is between 0 to 3 if fall off map is almost 10
        //so equation for line would be y= -20x + 10

        currentSpeed = floatBackchannel.FLOAT_VALUE;

        myRotation.z = -20.0f * currentSpeed + 10.0f;
        myRectTransform.localRotation = Quaternion.Euler(myRotation);
    }
}
