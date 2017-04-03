using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class PinchZoomTouchpadControllerType : BaseControllerType
    {
        //A Pinch/Zoom Touchpad is a control with 5 floats and an int and supports multitouch within the control
        //The two axis going between 0 and 1 (normalized) and a int for number of touches
        //(0,0) is lower left corner of touchpad

        public int TOUCH_COUNT;
        public float TOUCH1_POSITION_HORIZONTAL;
        public float TOUCH1_POSITION_VERTICAL;
        public float TOUCH2_POSITION_HORIZONTAL;
        public float TOUCH2_POSITION_VERTICAL;
        public float ZOOM_FACTOR;

        public override void mapNetworkDataToStructure(int packetNumber, string line)
        {
            string[] comma = new String[] { EasyWiFiConstants.SPLITMESSAGE_COMMA };

            //if we've received a packet on connection we thought was disconnected take action
            if (logicalPlayerNumber == EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
            {
                reuseOrGetAnotherConnection(previousConnectionPlayerNumber);
            }

            //remember when receiving the data the serverkey has already been parsed out to get the right
            //server instance in the data structure so we're left with datapoint,datapoint,datapoint...etc.
            if (isNewPacket(packetNumber))
            {
                lastPacketNumber = packetNumber;
                string[] splitMessage;

                if (line != null && !line.Equals(string.Empty))
                {
                    splitMessage = line.Split(comma, StringSplitOptions.RemoveEmptyEntries);
                    if (splitMessage.Length > 2)
                    {
                        TOUCH_COUNT = Convert.ToInt32(splitMessage[0]);
                        TOUCH1_POSITION_HORIZONTAL = (float)Convert.ToDecimal(splitMessage[1]);
                        TOUCH1_POSITION_VERTICAL = (float)Convert.ToDecimal(splitMessage[2]);
                        TOUCH2_POSITION_HORIZONTAL = (float)Convert.ToDecimal(splitMessage[3]);
                        TOUCH2_POSITION_VERTICAL = (float)Convert.ToDecimal(splitMessage[4]);
                        ZOOM_FACTOR = (float)Convert.ToDecimal(splitMessage[5]);
                    }
                }
            }

        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            message += TOUCH_COUNT.ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(TOUCH1_POSITION_HORIZONTAL).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(TOUCH1_POSITION_VERTICAL).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(TOUCH2_POSITION_HORIZONTAL).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(TOUCH2_POSITION_VERTICAL).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(ZOOM_FACTOR).ToString() + EasyWiFiConstants.SPLITMESSAGE_NEWLINE;

            return message;

        }


    }

}