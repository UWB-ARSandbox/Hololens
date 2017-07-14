using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;
using HoloToolkit.Unity.SpatialMapping;

namespace EasyWiFi.ServerControls
{

    [AddComponentMenu("EasyWiFiController/Server/UserControls/Match Orientation Gyro")]
    public class MatchOrientationGyroServerController : MonoBehaviour, IServerController
    {
        private const float lowPassFilterFactor = 0.2f;
        Quaternion quaternion;
        private readonly Quaternion baseIdentity = Quaternion.Euler(90, 0, 0);
        private readonly Quaternion landscapeRight = Quaternion.Euler(0, 0, 90);
        private readonly Quaternion landscapeLeft = Quaternion.Euler(0, 0, -90);
        private readonly Quaternion upsideDown = Quaternion.Euler(0, 0, 90);

        private Quaternion cameraBase = Quaternion.identity;
        private Quaternion calibration = Quaternion.identity;
        private Quaternion baseOrientation = Quaternion.Euler(90, 0, 0);
        private Quaternion baseOrientationRotationFix = Quaternion.identity;
        private bool firsttime = true;
        float dtime = 0;

        private Quaternion referanceRotation = Quaternion.identity;
        Spawnable script;
        Controller script1;
        public string control = "Gyro";
        public EasyWiFiConstants.PLAYER_NUMBER player = EasyWiFiConstants.PLAYER_NUMBER.Player1;

        //runtime variables
        GyroControllerType[] gyro = new GyroControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int currentNumberControllers = 0;
        Quaternion orientation;
        private void Start()
        {
            script = this.GetComponent<Spawnable>();
            if (script == null)
                script1 = this.GetComponent<Controller>();
        }
        void OnEnable()
        {
            EasyWiFiController.On_ConnectionsChanged += checkForNewConnections;

            //do one check at the beginning just in case we're being spawned after startup and after the callbacks
            //have already been called
            if (gyro[0] == null && EasyWiFiController.lastConnectedPlayerNumber >= 0)
            {
                EasyWiFiUtilities.checkForClient(control, (int)player, ref gyro, ref currentNumberControllers);
            }
        }

        void OnDestroy()
        {
            EasyWiFiController.On_ConnectionsChanged -= checkForNewConnections;
        }

        // Update is called once per frame
        void Update()
        {
            //iterate over the current number of connected controllers
            for (int i = 0; i < currentNumberControllers; i++)
            {
                if (gyro[i] != null && gyro[i].serverKey != null && gyro[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                {
                    mapDataStructureToAction(i);
                }
            }
        }


        public void mapDataStructureToAction(int index)
        {
            orientation.w = gyro[index].GYRO_W;
            orientation.x = gyro[index].GYRO_X;
            orientation.y = gyro[index].GYRO_Y;
            orientation.z = gyro[index].GYRO_Z;
            dtime = Time.deltaTime;
            quaternion = new Quaternion(orientation.x, orientation.y, orientation.z, orientation.w);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, cameraBase * (ConvertRotation(referanceRotation * quaternion) * GetRotFix()), lowPassFilterFactor);

            if (firsttime)
            {
                UpdateCalibration(true);
                RecalculateReferenceRotation();
                firsttime = false;
            }

            if(script != null && !script.calibrate)
            {
                return;
            }
            if(script1 != null && !script1.calibrate)
            {
                return;
            }

            UpdateCalibration(true);
            RecalculateReferenceRotation();
            if (script != null)
                script.calibrate = false;
            if (script1 != null)
                script1.calibrate = false;
            //transform.localRotation = orientation;
        }

        public void checkForNewConnections(bool isConnect, int playerNumber)
        {
            EasyWiFiUtilities.checkForClient(control, (int)player, ref gyro, ref currentNumberControllers);
        }

        #region [Private methods]

        /// <summary>
        /// Update the gyro calibration.
        /// </summary>
        /// <param name="onlyHorizontal"></param>
        private void UpdateCalibration(bool onlyHorizontal)
        {
            if (onlyHorizontal)
            {
                var fw = (quaternion) * (Vector3.up);
                fw.z = 0;
                if(fw == Vector3.zero)
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
        /// <param name="onlyHorizontal">
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
        /// <param name="q">
        /// The rotation to convert.
        /// </param>
        private static Quaternion ConvertRotation(Quaternion q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }

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
    
}
