//
// --------------------------------------------------------------------------
//  Gurux Ltd
//
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// More information of Gurux products: http://www.gurux.org
//
// This code is licensed under the GNU General Public License v2.
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Gurux.DLMS;
using System.ComponentModel;
using System.Xml.Serialization;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Internal;
using Gurux.DLMS.Enums;
using Gurux.DLMS.Objects.Enums;
using System.Xml;

namespace Gurux.DLMS.Objects
{
    public class GXDLMSIp4Setup : GXDLMSObject, IGXDLMSBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public GXDLMSIp4Setup()
        : this("0.0.25.1.0.255")
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ln">Logical Name of the object.</param>
        public GXDLMSIp4Setup(string ln)
        : this(ln, 0)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ln">Logical Name of the object.</param>
        /// <param name="sn">Short Name of the object.</param>
        public GXDLMSIp4Setup(string ln, ushort sn)
        : base(ObjectType.Ip4Setup, ln, sn)
        {
        }

        [XmlIgnore()]
        public string DataLinkLayerReference
        {
            get;
            set;
        }

        [XmlIgnore()]
        public string IPAddress
        {
            get;
            set;
        }

        [XmlIgnore()]
        public uint[] MulticastIPAddress
        {
            get;
            set;
        }

        [XmlIgnore()]
        public GXDLMSIp4SetupIpOption[] IPOptions
        {
            get;
            set;
        }

        [XmlIgnore()]
        public ulong SubnetMask
        {
            get;
            set;
        }

        [XmlIgnore()]
        public ulong GatewayIPAddress
        {
            get;
            set;
        }

        [XmlIgnore()]
        public bool UseDHCP
        {
            get;
            set;
        }

        [XmlIgnore()]
        public ulong PrimaryDNSAddress
        {
            get;
            set;
        }

        [XmlIgnore()]
        public ulong SecondaryDNSAddress
        {
            get;
            set;
        }

        /// <inheritdoc cref="GXDLMSObject.GetValues"/>
        public override object[] GetValues()
        {
            return new object[] { LogicalName, DataLinkLayerReference, IPAddress,
                              MulticastIPAddress, IPOptions, SubnetMask, GatewayIPAddress,
                              UseDHCP, PrimaryDNSAddress, SecondaryDNSAddress
                            };
        }
        #region IGXDLMSBase Members


        byte[] IGXDLMSBase.Invoke(GXDLMSSettings settings, ValueEventArgs e)
        {
            e.Error = ErrorCode.ReadWriteDenied;
            return null;
        }

        int[] IGXDLMSBase.GetAttributeIndexToRead()
        {
            List<int> attributes = new List<int>();
            //LN is static and read only once.
            if (string.IsNullOrEmpty(LogicalName))
            {
                attributes.Add(1);
            }
            //DataLinkLayerReference
            if (!base.IsRead(2))
            {
                attributes.Add(2);
            }
            //IPAddress
            if (CanRead(3))
            {
                attributes.Add(3);
            }
            //MulticastIPAddress
            if (CanRead(4))
            {
                attributes.Add(4);
            }
            //IPOptions
            if (CanRead(5))
            {
                attributes.Add(5);
            }
            //SubnetMask
            if (CanRead(6))
            {
                attributes.Add(6);
            }
            //GatewayIPAddress
            if (CanRead(7))
            {
                attributes.Add(7);
            }
            //UseDHCP
            if (!base.IsRead(8))
            {
                attributes.Add(8);
            }
            //PrimaryDNSAddress
            if (CanRead(9))
            {
                attributes.Add(9);
            }
            //SecondaryDNSAddress
            if (CanRead(10))
            {
                attributes.Add(10);
            }
            return attributes.ToArray();
        }

        /// <inheritdoc cref="IGXDLMSBase.GetNames"/>
        string[] IGXDLMSBase.GetNames()
        {
            return new string[] {Internal.GXCommon.GetLogicalNameString(),
                             "Data LinkLayer Reference", "IP Address", "Multicast IP Address",
                             "IP Options", "Subnet Mask", "Gateway IP Address", "Use DHCP",
                             "Primary DNS Address", "Secondary DNS Address"
                            };

        }

        int IGXDLMSBase.GetAttributeCount()
        {
            return 10;
        }

        int IGXDLMSBase.GetMethodCount()
        {
            return 3;
        }

        /// <inheritdoc cref="IGXDLMSBase.GetDataType"/>
        public override DataType GetDataType(int index)
        {
            if (index == 1)
            {
                return DataType.OctetString;
            }
            if (index == 2)
            {
                return DataType.OctetString;
            }
            if (index == 3)
            {
                return DataType.UInt32;
            }
            if (index == 4)
            {
                return DataType.Array;
            }
            if (index == 5)
            {
                return DataType.Array;
            }
            if (index == 6)
            {
                return DataType.UInt32;
            }
            if (index == 7)
            {
                return DataType.UInt32;
            }
            if (index == 8)
            {
                return DataType.Boolean;
            }
            if (index == 9)
            {
                return DataType.UInt32;
            }
            if (index == 10)
            {
                return DataType.UInt32;
            }
            throw new ArgumentException("GetDataType failed. Invalid attribute index.");
        }

        object IGXDLMSBase.GetValue(GXDLMSSettings settings, ValueEventArgs e)
        {
            if (e.Index == 1)
            {
                return GXCommon.LogicalNameToBytes(LogicalName);
            }
            if (e.Index == 2)
            {
                return GXCommon.LogicalNameToBytes(DataLinkLayerReference);
            }
            if (e.Index == 3)
            {
                //If IP address is not given.
                if (IPAddress == null || IPAddress.Trim().Length == 0)
                {
                    return 0;
                }
                return BitConverter.ToUInt32(System.Net.IPAddress.Parse(IPAddress).GetAddressBytes(), 0);
            }
            if (e.Index == 4)
            {
                GXByteBuffer data = new GXByteBuffer();
                data.SetUInt8((byte)DataType.Array);
                if (MulticastIPAddress == null)
                {
                    //Object count is zero.
                    data.SetUInt8(0);
                }
                else
                {
                    GXCommon.SetObjectCount(MulticastIPAddress.Length, data);
                    foreach (ushort it in MulticastIPAddress)
                    {
                        GXCommon.SetData(settings, data, DataType.UInt16, it);
                    }
                }
                return data.Array();
            }
            if (e.Index == 5)
            {
                GXByteBuffer data = new GXByteBuffer();
                data.SetUInt8((byte)DataType.Array);
                if (IPOptions == null)
                {
                    //Object count is zero.
                    data.SetUInt8(0);
                }
                else
                {
                    GXCommon.SetObjectCount(IPOptions.Length, data);
                    foreach (GXDLMSIp4SetupIpOption it in IPOptions)
                    {
                        data.SetUInt8((byte)DataType.Structure);
                        data.SetUInt8(3);
                        GXCommon.SetData(settings, data, DataType.UInt8, it.Type);
                        GXCommon.SetData(settings, data, DataType.UInt8, it.Length);
                        GXCommon.SetData(settings, data, DataType.OctetString, it.Data);
                    }
                }
                return data.Array();
            }
            if (e.Index == 6)
            {
                return this.SubnetMask;
            }
            if (e.Index == 7)
            {
                return this.GatewayIPAddress;
            }
            if (e.Index == 8)
            {
                return this.UseDHCP;
            }
            if (e.Index == 9)
            {
                return this.PrimaryDNSAddress;
            }
            if (e.Index == 10)
            {
                return this.SecondaryDNSAddress;
            }
            e.Error = ErrorCode.ReadWriteDenied;
            return null;
        }

        void IGXDLMSBase.SetValue(GXDLMSSettings settings, ValueEventArgs e)
        {
            if (e.Index == 1)
            {
                LogicalName = GXCommon.ToLogicalName(e.Value);
            }
            else if (e.Index == 2)
            {
                DataLinkLayerReference = GXCommon.ToLogicalName(e.Value);
            }
            else if (e.Index == 3)
            {
                IPAddress = new System.Net.IPAddress(BitConverter.GetBytes(Convert.ToUInt32(e.Value))).ToString();
            }
            else if (e.Index == 4)
            {
                List<uint> data = new List<uint>();
                if (e.Value != null)
                {
                    if (e.Value is object[])
                    {
                        foreach (object it in (object[])e.Value)
                        {
                            data.Add(Convert.ToUInt16(it));
                        }
                    }
                    else if (e.Value is ushort[])
                    {
                        //Some meters are returning wrong data here.
                        foreach (ushort it in (ushort[])e.Value)
                        {
                            data.Add(it);
                        }
                    }
                }
                MulticastIPAddress = data.ToArray();
            }
            else if (e.Index == 5)
            {
                List<GXDLMSIp4SetupIpOption> data = new List<GXDLMSIp4SetupIpOption>();
                if (e.Value != null)
                {
                    foreach (object[] it in (object[])e.Value)
                    {
                        GXDLMSIp4SetupIpOption item = new GXDLMSIp4SetupIpOption();
                        item.Type = (Ip4SetupIpOptionType)Convert.ToInt32(it[0]);
                        item.Length = Convert.ToByte(it[1]);
                        item.Data = (byte[])it[2];
                        data.Add(item);
                    }
                }
                IPOptions = data.ToArray();
            }
            else if (e.Index == 6)
            {
                SubnetMask = Convert.ToUInt32(e.Value);
            }
            else if (e.Index == 7)
            {
                GatewayIPAddress = Convert.ToUInt32(e.Value);
            }
            else if (e.Index == 8)
            {
                UseDHCP = Convert.ToBoolean(e.Value);
            }
            else if (e.Index == 9)
            {
                PrimaryDNSAddress = Convert.ToUInt32(e.Value);
            }
            else if (e.Index == 10)
            {
                SecondaryDNSAddress = Convert.ToUInt32(e.Value);
            }
            else
            {
                e.Error = ErrorCode.ReadWriteDenied;
            }
        }

        void IGXDLMSBase.Load(GXXmlReader reader)
        {
            DataLinkLayerReference = reader.ReadElementContentAsString("DataLinkLayerReference");
            IPAddress = reader.ReadElementContentAsString("IPAddress");
            List<uint> list = new List<uint>();
            if (reader.IsStartElement("MulticastIPAddress", true))
            {
                while (reader.IsStartElement("Value", false))
                {
                    list.Add((uint)reader.ReadElementContentAsInt("Value"));
                }
                reader.ReadEndElement("MulticastIPAddress");
            }
            MulticastIPAddress = list.ToArray();

            List<GXDLMSIp4SetupIpOption> ipOptions = new List<GXDLMSIp4SetupIpOption>();
            if (reader.IsStartElement("IPOptions", true))
            {
                while (reader.IsStartElement("IPOptions", true))
                {
                    GXDLMSIp4SetupIpOption it = new GXDLMSIp4SetupIpOption();
                    it.Type = (Ip4SetupIpOptionType)reader.ReadElementContentAsInt("Type");
                    it.Length = (byte)reader.ReadElementContentAsInt("Length");
                    it.Data = GXDLMSTranslator.HexToBytes(reader.ReadElementContentAsString("Data"));
                    ipOptions.Add(it);
                }
                reader.ReadEndElement("IPOptions");
            }
            IPOptions = ipOptions.ToArray();
            SubnetMask = reader.ReadElementContentAsULong("SubnetMask");
            GatewayIPAddress = reader.ReadElementContentAsULong("GatewayIPAddress");
            UseDHCP = reader.ReadElementContentAsInt("UseDHCP") != 0;
            PrimaryDNSAddress = reader.ReadElementContentAsULong("PrimaryDNSAddress");
            SecondaryDNSAddress = reader.ReadElementContentAsULong("SecondaryDNSAddress");
        }

        void IGXDLMSBase.Save(GXXmlWriter writer)
        {
            writer.WriteElementString("DataLinkLayerReference", DataLinkLayerReference);
            writer.WriteElementString("IPAddress", IPAddress);
            if (MulticastIPAddress != null)
            {
                writer.WriteStartElement("MulticastIPAddress");
                foreach (ushort it in MulticastIPAddress)
                {
                    writer.WriteElementString("Value", it);
                }
                writer.WriteEndElement();
            }
            if (IPOptions != null)
            {
                writer.WriteStartElement("IPOptions");
                foreach (GXDLMSIp4SetupIpOption it in IPOptions)
                {
                    writer.WriteStartElement("IPOptions");
                    writer.WriteElementString("Type", (int)it.Type);
                    writer.WriteElementString("Length", it.Length);
                    writer.WriteElementString("Data", GXDLMSTranslator.ToHex(it.Data));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteElementString("SubnetMask", SubnetMask);
            writer.WriteElementString("GatewayIPAddress", GatewayIPAddress);
            writer.WriteElementString("UseDHCP", UseDHCP);
            writer.WriteElementString("PrimaryDNSAddress", PrimaryDNSAddress);
            writer.WriteElementString("SecondaryDNSAddress", SecondaryDNSAddress);
        }

        void IGXDLMSBase.PostLoad(GXXmlReader reader)
        {
        }
        #endregion
    }
}
