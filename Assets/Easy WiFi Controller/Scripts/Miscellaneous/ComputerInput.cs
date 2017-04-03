using UnityEngine;
using System.Collections;
using EasyWiFi.ClientControls;

[AddComponentMenu("EasyWiFiController/Miscellaneous/ComputerInput")]
public class ComputerInput : MonoBehaviour {

    public FloatDataClientController xSpeedNotification;
    public FloatDataClientController zSpeedNotification;

    //making a cirular motion so start out of phase
    float xSpeed = 0f;
    float zSpeed = 5f;

    bool xAscending = true;
    bool zAscending = true; //this one will get flipped to false before first execution

	// Use this for initialization
	void Start () { 
	
	}
	
	// Update is called once per frame
	void Update () {
        //this function demonstrates sending regular data over the network

        newSpeed();

        xSpeedNotification.setValue(xSpeed);
        zSpeedNotification.setValue(zSpeed);
	
	}

    void newSpeed()
    {
        //this is a very simple function values will oscilate by a fixed value between -10 and 10
        if (xSpeed > 4.9f || xSpeed < -4.9f)
            xAscending = !xAscending;

        if (zSpeed > 4.9f || zSpeed < -4.9f)
            zAscending = !zAscending;
        
        
        if (xAscending)
            xSpeed += .1f;
        else
            xSpeed -= .1f;

        if (zAscending)
            zSpeed += .1f;
        else
            zSpeed -= .1f;
    }
}
