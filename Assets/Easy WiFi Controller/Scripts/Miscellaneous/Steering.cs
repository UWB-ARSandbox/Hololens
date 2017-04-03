using UnityEngine;
using System;
using System.Collections;
using EasyWiFi.Core;
using EasyWiFi.ServerBackchannels;

[AddComponentMenu("EasyWiFiController/Miscellaneous/Steering")]
public class Steering : MonoBehaviour {

    public FloatServerBackchannel floatBackchannel;

    Rigidbody myRigidbody;
    Vector3 accel;
    float horizontal, vertical;
    float normalizeDegrees = 90f;
    float sensitivity = 5f;
    Vector3 actionVectorPosition;
    Vector3 computerVector;

    void Start()
    {
        myRigidbody = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        floatBackchannel.setValue(myRigidbody.velocity.magnitude);
    }

    //human example scene methods (gaspedal, brakepedal, and steerball)
    public void gasPedal(ButtonControllerType gasButton)
    {
        if (gasButton.BUTTON_STATE_IS_PRESSED)
        {
            if (myRigidbody.velocity.magnitude > 1f)
                myRigidbody.AddForce(myRigidbody.velocity * sensitivity * .5f);
            else
                myRigidbody.AddForce((this.transform.position - Camera.main.transform.position) * sensitivity * 5f);
        }
    }

    public void brakePedal (ButtonControllerType brakeButton)
    {
        if (brakeButton.BUTTON_STATE_IS_PRESSED)
        {
            if (myRigidbody.velocity.magnitude > 1f)
                myRigidbody.AddForce(myRigidbody.velocity * -myRigidbody.velocity.magnitude);                
            else
                myRigidbody.AddForce(myRigidbody.velocity * -myRigidbody.velocity.magnitude * 10f); 
        }
    }

    public void steerBall(AccelerometerControllerType accelerometer)
    {
        //data
        accel.x = accelerometer.ACCELERATION_X;
        accel.y = accelerometer.ACCELERATION_Y;
        accel.z = accelerometer.ACCELERATION_Z;

        //accelerometers due to gravity can really only sense 2 axis (can't filter out gravity)
        //here we convert those 2 axis into horizontal and vertical and normalize
        horizontal = EasyWiFiUtilities.relativeAngleInAxis(Vector3.up, -accel, Vector3.forward) / normalizeDegrees;
        vertical = EasyWiFiUtilities.relativeAngleInAxis(Vector3.up, -accel, Vector3.right) / normalizeDegrees;

        horizontal *= -sensitivity;
        vertical *= -sensitivity;

        actionVectorPosition.x = horizontal;
        actionVectorPosition.y = 0f;
        actionVectorPosition.z = vertical;

        myRigidbody.AddForce(actionVectorPosition);

    }

    //AI steering methods (moveX and moveZ)
    public void moveX(FloatBackchannelType xValue)
    {
        computerVector.x = xValue.FLOAT_VALUE;
        computerVector.y = 0f;
        computerVector.z = 0f;
        myRigidbody.AddForce(computerVector * sensitivity);
    }

    public void moveZ(FloatBackchannelType zValue)
    {
        computerVector.x = 0f;
        computerVector.y = 0f;
        computerVector.z = zValue.FLOAT_VALUE;
        myRigidbody.AddForce(computerVector * sensitivity);
    }

}
