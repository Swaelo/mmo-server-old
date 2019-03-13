﻿// ================================================================================================================================
// File:        PacketReader.cs
// Description: Class for extracting data from network packets received from game clients
// ================================================================================================================================

using System;
using System.Text;
using BEPUutilities;

namespace Server.Networking
{
    public class PacketReader
    {
        byte[] PacketData;  //All of the packet data
        int PacketPosition; //Current position reading through all the data in the packet

        public PacketReader(byte[] PacketData)
        {
            //Copy all the packet data into the class member
            this.PacketData = new byte[PacketData.Length];
            Array.Copy(PacketData, this.PacketData, PacketData.Length);
            //Start reading data at the beginning of the array
            PacketPosition = 0;
        }

        public int ReadInt()
        {
            //Extract the integer value from the packet data
            int Value = BitConverter.ToInt32(PacketData, PacketPosition);
            //Move the position forward for reading the next value
            PacketPosition += 4;
            //Return the extracted value
            return Value;
        }

        public float ReadFloat()
        {
            float Value = BitConverter.ToSingle(PacketData, PacketPosition);
            PacketPosition += 4;
            return Value;
        }

        public string ReadString()
        {
            int StringLength = ReadInt();
            string Value = Encoding.ASCII.GetString(PacketData, PacketPosition, StringLength);
            PacketPosition += StringLength;
            return Value;
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }
    }
}
