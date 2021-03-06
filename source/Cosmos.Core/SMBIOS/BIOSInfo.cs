﻿using System;
using System.Collections.Generic;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.SMBIOS
{
    public unsafe class BIOSInfo : SMBIOSTable
    {
        private byte VendorID = 0xff;
        private byte VersionID = 0xff;
        private byte ReleaseDateID = 0xff;
        private EntryPointTable EntryPointTable;

        public string Vendor{ get; set; }
        public string Version { get; set; }
        public ushort StartingAddressSegment { get; set; }
        public string ReleaseDate { get; set; }
        //TODO: do the calculation explained in the reference
        public byte ROMSize { get; set; }
        //Since its qword its a int64
        public ulong Characteristics { get; set; }
        public byte[] OptionalCharacteristics { get; set; }
        public byte SystemBiosMajorRelease { get; set; }
        public byte SystemBiosMinorRelease { get; set; }
        public byte EmbeddedControllerFirmwareMajorRelease { get; set; }
        public byte EmbeddedControllerFirmwareMinorRelease { get; set; }
        public ushort ExtendedBiosROMSize { get; set; }

        //We asume that the smbios is always greater than 2.0
        //TODO: independice the hardcoded numbers with a variable (which we will use to move through memory)
        public BIOSInfo(EntryPointTable entryPointTable, byte* BeginningAddress) : base(BeginningAddress)
        {
            this.BeginningAddress = BeginningAddress;
            this.EntryPointTable = entryPointTable;
        }

        //We go byte by byte MANUALLY to parse the table.
        //The field that is assigned is autodocumented
        //We use the BitConverter for words and qwords (2 bytes and 8 bytes, respectively);
        public override byte* Parse()
        {
            byte* newAddress =  BeginningAddress;
            int i;
            int j;

            this.Type = BeginningAddress[0];

            this.Length = BeginningAddress[1];

            byte[] tmp = new byte[2];
            tmp[0] = BeginningAddress[2];
            tmp[1] = BeginningAddress[3];
            this.Handle = BitConverter.ToUInt16(tmp, 0);

            VendorID = BeginningAddress[4];
            VersionID = BeginningAddress[5];

            tmp[0] = BeginningAddress[6];
            tmp[1] = BeginningAddress[7];
            StartingAddressSegment = BitConverter.ToUInt16(tmp, 0);

            ReleaseDateID = BeginningAddress[8];
            ROMSize = BeginningAddress[9];

            tmp = new byte[8];
            for (int k = 0; k < 8; k++)
            {
                //Since we left in 10...
                tmp[k] = BeginningAddress[k + 10];
            }
            Characteristics = BitConverter.ToUInt64(tmp, 0);

            newAddress = BeginningAddress + 18;

            if (EntryPointTable.IsVersionGreaterThan(2, 4))
            {
                //Begin to parse the optional characteristics
                //Since it is an optional field, we need to calculate its size first
                //Formula: Length - 12h == Length - 18
                //var size = Length - 18;

                //I dont know if the specification is incorrect but i count 22 bytes, not 18 (you must 
                // count the system bios bytes and firmware (since they are 22 bytes).
                // Fucking engineers. Need discrete math 101 to learn to count.
                var size = Length - 22;
                //If there is no optional characteristic, skip
                if (size > 0)
                {
                    OptionalCharacteristics = new byte[size];
                    //We start whre we left (18)
                    for (int k = 0; k < size; k++)
                    {
                        OptionalCharacteristics[k] = BeginningAddress[k + 18];
                    }
                }

                SystemBiosMajorRelease = BeginningAddress[size + 18];
                SystemBiosMinorRelease = BeginningAddress[size + 19];
                EmbeddedControllerFirmwareMajorRelease = BeginningAddress[size + 20];
                EmbeddedControllerFirmwareMinorRelease = BeginningAddress[size + 21];

                //This will not work in bochs since its version is 2.4
                if (EntryPointTable.IsVersionGreaterThan(3, 1))
                {
                    size += 2;
                    tmp = new byte[2];
                    tmp[0] = BeginningAddress[size + 22];
                    tmp[1] = BeginningAddress[size + 23];
                    ExtendedBiosROMSize = BitConverter.ToUInt16(tmp, 0);
                }
                
                //We have finished parsing the formatted area
                //We start now the unformatted area
                newAddress = BeginningAddress + size + 22;
            }

            //Parse the first string
            string tmpString = "";
            int[] tmpArray = new int[3];
            tmpArray[0] = VendorID;
            tmpArray[1] = ReleaseDateID;
            tmpArray[2] = VersionID;
            //TODO: method for this
            for (int q = 0; q < 3; q++)
            {
                for (int w = 1; w < 3 - q; w++)
                {
                    if (tmpArray[w - 1] > tmpArray[w])
                    {
                        var tmp2 =  tmpArray[w - 1];
                        tmpArray[w - 1] = tmpArray[w];
                        tmpArray[w] = tmp2;
                    }
                } 
            }
            foreach (int t in tmpArray)
            {
                if (t == 255 | t == 0)
                    continue;
                newAddress = Core.SMBIOS.SMBIOS.ParseString(newAddress, out tmpString);
                if(t == VendorID)
                    Vendor = tmpString;
                else if (t == ReleaseDateID)
                    ReleaseDate = tmpString;
                else if (t == VersionID)
                    Version = tmpString;
                else
                    continue;
            }

            //ParseString leaves the pointer after the first \0
            //So we need to skip the double null
            return newAddress + 1;
        }
    }
}

