using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class AccelerometerControllerType : BaseControllerType
    {
        //An accelerometer is a simple control with 3 floats for the accleration along each axis
        public float ACCELERATION_X;
        public float ACCELERATION_Y;
        public float ACCELERATION_Z;

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
                        ACCELERATION_X = (float) Convert.ToDecimal(splitMessage[0]);
                        ACCELERATION_Y = (float) Convert.ToDecimal(splitMessage[1]);
                        ACCELERATION_Z = (float) Convert.ToDecimal(splitMessage[2]);
                    }
                }
            }


        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            //double and float if converted straight to string can have e notation in it which causes issues on when trying
            //to convert on the the other end so decimal is used "over the wire"
            message += Convert.ToDecimal(ACCELERATION_X).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(ACCELERATION_Y).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(ACCELERATION_Z).ToString() + EasyWiFiConstants.SPLITMESSAGE_NEWLINE;

            return message;

        }


    }

}