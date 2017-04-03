using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SensorEmitterServer;
using HoloToolkit.Unity.InputModule;

public class SensorReceive : MonoBehaviour, IInputClickHandler
{

    Quaternion quaternion;
    Vector3 position;
    GameObject obj;
    Spawnable script;
    private const float lowPassFilterFactor = 0.2f;

    private readonly Quaternion baseIdentity = Quaternion.Euler(90, 0, 0);
    private readonly Quaternion landscapeRight = Quaternion.Euler(0, 0, 90);
    private readonly Quaternion landscapeLeft = Quaternion.Euler(0, 0, -90);
    private readonly Quaternion upsideDown = Quaternion.Euler(0, 0, 180);

    private Quaternion cameraBase = Quaternion.identity;
    private Quaternion calibration = Quaternion.identity;
    private Quaternion baseOrientation = Quaternion.Euler(90, 0, 0);
    private Quaternion baseOrientationRotationFix = Quaternion.identity;

    AHRS.MadgwickAHRS algorithm = new AHRS.MadgwickAHRS(1/256);

    private Quaternion referanceRotation = Quaternion.identity;

    float newInputqX;
    float newInputqY;
    float newInputqZ;
    float newInputqW ;

    float newInputX;
    float newInputY;
    float newInputZ;
    float velocityx1 =0;
    float velocityx2=0;
    float posX1 =0;
    float posX2=0;


    float velocityy1=0 ;
    float velocityy2=0;
    float posY1=0;
    float posY2=0;

    float velocityz1=0 ;
    float velocityz2=0;
    float posZ1=0;
    float posZ2=0;

    float accelerationx ;
    float accelerationy ;
    float accelerationz ;

    float dtime = 0;

    Shader originalshader;



    // Use this for initialization
    void Start () {
        Debug.Log("ServerStarted");
        // put this e.g. at your application start:
        obj = GameObject.Find("cardboardBox_02");
        posX1 = obj.transform.position.x;
        posY1 = obj.transform.position.y;
        posZ1 = obj.transform.position.z;
        script = obj.GetComponent<Spawnable>();
        UpdateCalibration(true);
        RecalculateReferenceRotation();
        var server = new SensorServer<SensorEmitterReading>();
        server.ExceptionOccured += (s, e) => { Debug.Log("Something wrong!"); };
        server.ValuesReceived += (s, e) => { MoveObject(e.SensorReading); };
        server.Start();
      

        
    }

    public void OnInputClicked(InputEventData eventData)
    {
        if (GazeManager.Instance.HitObject != null)
        {
            obj = GazeManager.Instance.HitObject.gameObject;
            script = obj.GetComponent<Spawnable>();
            if (script.selected == false)
            {

                posX1 = obj.transform.position.x;
                posY1 = obj.transform.position.y;
                posZ1 = obj.transform.position.z;
                originalshader = obj.GetComponent<Renderer>().material.shader;
                obj.GetComponent<Renderer>().material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
                UpdateCalibration(true);
                RecalculateReferenceRotation();
                script.selected = true;

            }
            else if (script.selected == true)
            {
                obj.GetComponent<Renderer>().material.shader = originalshader;
                obj = null;
                script.selected = false;
            }
        }

       
    }

    

    void Update()
    {
        
        if(obj==null)
        { return; }
         dtime = Time.deltaTime;
        quaternion = new Quaternion(newInputqX, newInputqY, newInputqZ, newInputqW);
        obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation,
            cameraBase * (ConvertRotation(referanceRotation * quaternion) * GetRotFix()), lowPassFilterFactor);


        accelerationx = newInputX;
        accelerationy = newInputY;
        accelerationz = newInputZ;

        //stationary treshold
        if (accelerationx < 0.05 && accelerationx > -0.05)
        {
            accelerationx = 0;
            velocityx1 = 0;
            velocityx2 = 0;
        }
        if (accelerationy < 0.05 && accelerationy > -0.05)
        {
            accelerationy = 0;
            velocityy1 = 0;
            velocityy2 = 0;
        }
        if (accelerationz < 0.05 && accelerationz > -0.05)
        {
            accelerationz = 0;
            velocityz1 = 0;
            velocityz2 = 0;
        }

       

        //first X integration:
        velocityx2 = velocityx1 + accelerationx * dtime*100;
        //second X integration:
        posX2 =posX1 + velocityx2 * dtime ;


        //first Y integration:
        velocityy2 = velocityy1 + accelerationy * dtime * 100;
        //second Y integration:
        posY2 = posY1 + velocityy2 * dtime;

        //first Z integration:
        velocityz2 = velocityz1+ accelerationz * dtime *100;
        //second Z integration:
        posZ2 = posZ1 + velocityz2 * dtime;
    

        //transform
        position = new Vector3(posX2, posY2, posZ2);
        obj.transform.position = position;


        //update
        velocityx1 = velocityx2;
        velocityy1 = velocityy2;
        velocityz1 = velocityz2;

        posX1 = posX2;
        posY1 = posY2;
        posZ1 = posZ2;


  

        if (!script.calibrate)
            return;
        Debug.Log("calibrated");
        UpdateCalibration(true);
        RecalculateReferenceRotation();
        script.calibrate = false;
        

        Debug.Log(quaternion.x);
        obj.transform.rotation = quaternion;



    }
	
	void MoveObject( SensorEmitterReading reading)
    {
        
        newInputqX = reading.QuaternionX;
        newInputqY = reading.QuaternionY;
        newInputqZ = reading.QuaternionZ;
        newInputqW = reading.QuaternionW;

        newInputX = reading.LinearAccelerationX;
        newInputY = reading.LinearAccelerationY;
        newInputZ = reading.LinearAccelerationZ;
        

      // quaternion= algorithm.Update(reading.QuaternionX, reading.QuaternionY, reading.QuaternionZ, reading.RawAccelerationX, reading.RawAccelerationY, reading.RawAccelerationZ,reading.MagnetometerX,reading.MagnetometerY,reading.MagnetometerZ);
          

    }


    #region [Private methods]

    /// <summary>
    /// Update the gyro calibration.
    /// </summary>
    private void UpdateCalibration(bool onlyHorizontal)
    {
        if (onlyHorizontal)
        {
            var fw = (quaternion) * (-Vector3.forward);
            fw.z = 0;
            if (fw == Vector3.zero)
            {
                calibration = Quaternion.identity;
            }
            else
            {
                calibration = (Quaternion.FromToRotation(baseOrientationRotationFix * Vector3.up, fw));
            }
        }
        else
        {
            calibration = quaternion;
        }
    }

    /// <summary>
    /// Update the camera base rotation.
    /// </summary>
    /// <param name='onlyHorizontal'>
    /// Only y rotation.
    /// </param>
    private void UpdateCameraBaseRotation(bool onlyHorizontal)
    {
        if (onlyHorizontal)
        {
            var fw = transform.forward;
            fw.y = 0;
            if (fw == Vector3.zero)
            {
                cameraBase = Quaternion.identity;
            }
            else
            {
                cameraBase = Quaternion.FromToRotation(Vector3.forward, fw);
            }
        }
        else
        {
            cameraBase = transform.rotation;
        }
    }

    /// <summary>
    /// Converts the rotation from right handed to left handed.
    /// </summary>
    /// <returns>
    /// The result rotation.
    /// </returns>
    /// <param name='q'>
    /// The rotation to convert.
    /// </param>
    private static Quaternion ConvertRotation(Quaternion q)
    {

        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    /// <summary>
    /// Gets the rot fix for different orientations.
    /// </summary>
    /// <returns>
    /// The rot fix.
    /// </returns>
    private Quaternion GetRotFix()
    {
#if UNITY_3_5
		if (Screen.orientation == ScreenOrientation.Portrait)
			return Quaternion.identity;
		
		if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.Landscape)
			return landscapeLeft;
				
		if (Screen.orientation == ScreenOrientation.LandscapeRight)
			return landscapeRight;
				
		if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
			return upsideDown;
		return Quaternion.identity;
#else
        return Quaternion.identity;
#endif
    }

    /// <summary>
    /// Recalculates reference system.
    /// </summary>
    private void ResetBaseOrientation()
    {
        baseOrientationRotationFix = GetRotFix();
        baseOrientation = baseOrientationRotationFix * baseIdentity;
    }

    /// <summary>
    /// Recalculates reference rotation.
    /// </summary>
    private void RecalculateReferenceRotation()
    {
        referanceRotation = Quaternion.Inverse(baseOrientation) * Quaternion.Inverse(calibration);
    }

    #endregion
}
