using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SensorEmitterServer;

public class SensorReceive : MonoBehaviour {

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



    private Quaternion referanceRotation = Quaternion.identity;

    float newInputqX;
    float newInputqY;
    float newInputqZ;
    float newInputqW ;

    float newInputX;
    float newInputY;
    float newInputZ;
    float velocityx ;
    float distanceX ;

    float velocityy ;
    float distanceY;

    float velocityz ;
    float distanceZ;

    float accelerationx ;
    float accelerationy ;
    float accelerationz ;





    // Use this for initialization
    void Start () {
        Debug.Log("ServerStarted");
        // put this e.g. at your application start:
        obj = GameObject.Find("cardboardBox_02");
        script = obj.GetComponent<Spawnable>();
        
        var server = new SensorServer<SensorEmitterReading>();
        server.ExceptionOccured += (s, e) => { Debug.Log("Something wrong!"); };
        server.ValuesReceived += (s, e) => { MoveObject(e.SensorReading); };
        server.Start();
        UpdateCalibration(true);
        RecalculateReferenceRotation();
    }

    void Update()
    {
        float dtime = Time.deltaTime;
        quaternion = new Quaternion(newInputqX, newInputqY, newInputqZ, newInputqW);
        obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation,
            cameraBase * (ConvertRotation(referanceRotation * quaternion) * GetRotFix()), lowPassFilterFactor);


        accelerationx = newInputX;
        accelerationy = newInputY;
        accelerationz = newInputZ;
        if (accelerationx < 0.02 && accelerationx > -0.02)
            accelerationx = 0;
        if (accelerationy < 0.02 && accelerationy > -0.02)
            accelerationy = 0;
        if (accelerationz < 0.02 && accelerationz > -0.02)
            accelerationz = 0;



        //first X integration:
        velocityx = accelerationx * dtime * 10;
        //second X integration:
        distanceX = velocityx * dtime + (accelerationx * (Mathf.Pow(dtime, 2))) / 2;
        //first Y integration:
        velocityy = accelerationy * dtime * 10;
        //second Y integration:
        distanceY = velocityy * dtime + (accelerationy * (Mathf.Pow(dtime, 2))) / 2;

        //first Z integration:
        velocityz = accelerationz * dtime * 10;
        //second Z integration:
        distanceZ = velocityz * dtime + (accelerationz * (Mathf.Pow(dtime, 2))) / 2;


        position = new Vector3(obj.transform.position.x+distanceX, obj.transform.position.y+distanceY , obj.transform.position.z+distanceZ );
        //obj.transform.position = position;


        if (!script.calibrate)
            return;
        Debug.Log("calibrated");
        UpdateCalibration(true);
        RecalculateReferenceRotation();
        script.calibrate = false;




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
