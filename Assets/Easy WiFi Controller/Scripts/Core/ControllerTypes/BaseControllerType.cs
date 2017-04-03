using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
// the different controller types (some maybe a few ints, a few floats, others quaternions, can be anything really)
namespace EasyWiFi.Core
{

    public abstract class BaseControllerType
    {
        //locally controls have a different key than server because server can have many clients with their
        //own controls we need to keep track of the key to the server dictionary
        public string clientKey;
        public string serverKey;
        public string clientIP;
        public string controllerType;

        //used serverside only
        public int lastPacketNumber;
        public bool justReconnected = false;
        public int logicalPlayerNumber; //this is a logical player number to be used in game (can be changed at will when you come and go)
        public int previousConnectionPlayerNumber;
        public DateTime lastReceivedPacketTime;


        //for our network traffic on both ends we are essentially having strings (converted in/out for byte[] on the send/recieve
        //these methods essentially will be one line in the string (remember a client will have more than one controller type)




        //this method is called when a packet is received on a connection that is marked as disconnected due to a timeout
        //this can occur in game usecases when the player suspends the app and restarts and expects the controller to still work
        //because the app wasn't executing having the server sending a message stating hey you timed out is useless
        //instead because our design is player based simply check to see if another controller has been assigned this player number
        //if not then just change the player number back and proceed as normal
        //if another controller has then get the next available player number and then notify the callback of a changed connection
        public void reuseOrGetAnotherConnection(int previousPlayerNumber)
        {
            if (EasyWiFiUtilities.isPlayerNumberOccupied(previousPlayerNumber,clientKey))
            {
                logicalPlayerNumber = EasyWiFiController.getNewPlayerNumber(clientKey);
            }
            else
            {
                logicalPlayerNumber = previousConnectionPlayerNumber;
            }
            lastReceivedPacketTime = DateTime.UtcNow;
            justReconnected = true;

            //take this time to reactivate all of the controls from this IP address so callback only get fired once
            BaseControllerType temp;
            string[] splitter = { clientKey };
            String[] clientIP = serverKey.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            if (EasyWiFiController.controllerDataDictionary != null)
            {
                foreach (string key in EasyWiFiController.controllerDataDictionary.Keys)
                {
                    temp = EasyWiFiController.controllerDataDictionary[key];
                    if (temp.serverKey.Contains(clientIP[0]))
                    {
                        temp.justReconnected = true;
                        temp.lastReceivedPacketTime = DateTime.UtcNow;
                        temp.logicalPlayerNumber = logicalPlayerNumber;
                    }
                }
            }

            //send the callback for a connection
            EasyWiFiController.isConnect = true;
            EasyWiFiController.lastConnectedPlayerNumber = logicalPlayerNumber;
            EasyWiFiController.forceConnectionRefresh();
        }



        //All our packets are UDP (meaning no acks but even on a network they can still come in out of order
        //this method detects that so that we can disregard this info
        //packets are getting sent a maximum of every update so can come pretty rapid fire but not super fast
        //for this reason we roll over at 10000 
        public bool isNewPacket(int packetNumber)
        {
            //if we've just reconnected we may have gotten out of sync with the packet numbers
            //however obviously its a fresh connection and is a new packet we'll give a two second allowance time
            if (justReconnected)
            {
                justReconnected = false;
                return true;
            }

            if (lastPacketNumber > EasyWiFiConstants.CHECK_FOR_ROLLOVER_HIGH)
            {
                //if we are within a specified range also check to see if we've rolled over
                if (packetNumber < EasyWiFiConstants.CHECK_FOR_ROLLOVER_LOW)
                {
                    //this is a new packet set the time
                    this.lastReceivedPacketTime = DateTime.UtcNow;

                    return true;
                }
            }
            else if (lastPacketNumber < EasyWiFiConstants.CHECK_FOR_ROLLOVER_LOW)
            {
                //if were at the low end of course 9999 > 1 but because we rolled over need to make sure we don't roll back
                if (packetNumber > EasyWiFiConstants.CHECK_FOR_ROLLOVER_HIGH)
                {
                    return false;
                }
            }

            if (packetNumber > lastPacketNumber)
            {
                //conventional check for every other case

                //this is a new packet set the time
                this.lastReceivedPacketTime = DateTime.UtcNow;

                return true;
            }

            //if no cases are met then we don't have a new packet it came out of order discard
            return false;
        }

        //all of the data types will be of the format serverkey#;value,value,value,value (endline)
        public abstract void mapNetworkDataToStructure(int packetNumber, string line);
        public abstract string mapStructureToNetworkData();

    }
}
