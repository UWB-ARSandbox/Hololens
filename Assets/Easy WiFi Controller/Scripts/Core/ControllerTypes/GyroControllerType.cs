using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class GyroControllerType : BaseControllerType
    {
        //A gyro is a simple control with 4 floats for the quaternion from the gyro
        public float GYRO_W;
        public float GYRO_X;
        public float GYRO_Y;
        public float GYRO_Z;

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
                    if (splitMessage.Length > 3)
                    {
                        GYRO_W = (float) Convert.ToDecimal(splitMessage[0]);
                        GYRO_X = (float) Convert.ToDecimal(splitMessage[1]);
                        GYRO_Y = (float) Convert.ToDecimal(splitMessage[2]);
                        GYRO_Z = (float) Convert.ToDecimal(splitMessage[3]);
                    }
                }
            }

        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            //double and float if converted straight to string can have e notation in it which causes issues on when trying
            //to convert on the the other end so decimal is used "over the wire"
            message += Convert.ToDecimal(GYRO_W).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(GYRO_X).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(GYRO_Y).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(GYRO_Z).ToString() + EasyWiFiConstants.SPLITMESSAGE_NEWLINE;

            return message;

        }


    }

}