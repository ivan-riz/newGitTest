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
    public class GXDLMSPushSetup : GXDLMSObject, IGXDLMSBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public GXDLMSPushSetup()
        : this("0.7.25.9.0.255")
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ln">Logical Name of the object.</param>
        public GXDLMSPushSetup(string ln)
        : this(ln, 0)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ln">Logical Name of the object.</param>
        /// <param name="sn">Short Name of the object.</param>
        public GXDLMSPushSetup(string ln, ushort sn)
        : base(ObjectType.PushSetup, ln, sn)
        {
            CommunicationWindow = new List<KeyValuePair<GXDateTime, GXDateTime>>();
            PushObjectList = new List<KeyValuePair<GXDLMSObject, GXDLMSCaptureObject>>();
        }

        public ServiceType Service
        {
            get;
            set;
        }
        public string Destination
        {
            get;
            set;
        }
        public MessageType Message
        {
            get;
            set;
        }

        /// <summary>
        /// Defines the list of attributes or objects to be pushed.
        /// Upon a call of the push (data) method the selected attributes are sent to the desti-nation
        /// defined in send_destination_and_method.
        /// </summary>
        [XmlIgnore()]
        public List<KeyValuePair<GXDLMSObject, GXDLMSCaptureObject>> PushObjectList
        {
            get;
            set;
        }



        /// <summary>
        /// Contains the start and end date/time
        /// stamp when the communication window(s) for the push become active
        /// (for the start instant), or inac-tive (for the end instant).
        /// </summary>
        [XmlIgnore()]
        public List<KeyValuePair<GXDateTime, GXDateTime>> CommunicationWindow
        {
            get;
            set;
        }

        /// <summary>
        /// To avoid simultaneous network connections of a lot of devices at ex-actly
        /// the same point in time, a randomisation interval in seconds can be defined.
        /// This means that the push operation is not started imme-diately at the
        /// beginning of the first communication window but started randomly delayed.
        /// </summary>
        [XmlIgnore()]
        public ushort RandomisationStartInterval
        {
            get;
            set;
        }
        /// <summary>
        /// The maximum number of retrials in case of unsuccessful push at-tempts. After a successful push no further push attempts are made until the push setup is triggered again.
        /// A value of 0 means no repetitions, i.e. only the initial connection at-tempt is made.
        /// </summary>
        [XmlIgnore()]
        public byte NumberOfRetries
        {
            get;
            set;
        }

        [XmlIgnore()]
        public ushort RepetitionDelay
        {
            get;
            set;
        }

        /// <inheritdoc cref="GXDLMSObject.GetValues"/>
        public override object[] GetValues()
        {
            return new object[] { LogicalName, PushObjectList, Service + " " + Destination + " " + Message,
                              CommunicationWindow, RandomisationStartInterval, NumberOfRetries, RepetitionDelay
                            };
        }

        #region IGXDLMSBase Members

        byte[] IGXDLMSBase.Invoke(GXDLMSSettings settings, ValueEventArgs e)
        {
            if (e.Index == 1)
            {
                //Only TCP/IP push is allowed at the moment.
                if (Service != ServiceType.Tcp || Message != MessageType.CosemApdu ||
                        PushObjectList.Count == 0)
                {
                    e.Error = ErrorCode.HardwareFault;
                    return null;
                }
                return null;
            }
            e.Error = ErrorCode.ReadWriteDenied;
            return null;
        }

        /// <summary>
        /// Activates the push process.
        /// </summary>
        /// <returns></returns>
        public byte[][] Activate(GXDLMSClient client)
        {
            return client.Method(this, 1, (sbyte)0);
        }

        int[] IGXDLMSBase.GetAttributeIndexToRead()
        {
            List<int> attributes = new List<int>();
            //LN is static and read only once.
            if (string.IsNullOrEmpty(LogicalName))
            {
                attributes.Add(1);
            }
            //PushObjectList
            if (CanRead(2))
            {
                attributes.Add(2);
            }
            //SendDestinationAndMethod
            if (CanRead(3))
            {
                attributes.Add(3);
            }
            //CommunicationWindow
            if (CanRead(4))
            {
                attributes.Add(4);
            }
            //RandomisationStartInterval
            if (CanRead(5))
            {
                attributes.Add(5);
            }
            //NumberOfRetries
            if (CanRead(6))
            {
                attributes.Add(6);
            }
            //RepetitionDelay
            if (CanRead(7))
            {
                attributes.Add(7);
            }
            return attributes.ToArray();
        }

        /// <inheritdoc cref="IGXDLMSBase.GetNames"/>
        string[] IGXDLMSBase.GetNames()
        {
            return new string[] { Internal.GXCommon.GetLogicalNameString(), "Push Object List",
                              "Send Destination And Method", "Communication Window", "Randomisation Start Interval", "Number Of Retries", "Repetition Delay"
                            };
        }

        int IGXDLMSBase.GetAttributeCount()
        {
            return 7;
        }

        int IGXDLMSBase.GetMethodCount()
        {
            return 1;
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
                return DataType.Array;
            }
            if (index == 3)
            {
                return DataType.Structure;
            }
            if (index == 4)
            {
                return DataType.Array;
            }
            if (index == 5)
            {
                return DataType.UInt16;
            }
            if (index == 6)
            {
                return DataType.UInt8;
            }
            if (index == 7)
            {
                return DataType.UInt16;
            }
            throw new ArgumentException("GetDataType failed. Invalid attribute index.");
        }

        object IGXDLMSBase.GetValue(GXDLMSSettings settings, ValueEventArgs e)
        {
            if (e.Index == 1)
            {
                return GXCommon.LogicalNameToBytes(LogicalName);
            }
            GXByteBuffer buff = new GXByteBuffer();
            if (e.Index == 2)
            {
                buff.SetUInt8(DataType.Array);
                GXCommon.SetObjectCount(PushObjectList.Count, buff);
                foreach (KeyValuePair<GXDLMSObject, GXDLMSCaptureObject> it in PushObjectList)
                {
                    buff.SetUInt8(DataType.Structure);
                    buff.SetUInt8(4);
                    GXCommon.SetData(settings, buff, DataType.UInt16, it.Key.ObjectType);
                    GXCommon.SetData(settings, buff, DataType.OctetString, GXCommon.LogicalNameToBytes(it.Key.LogicalName));
                    GXCommon.SetData(settings, buff, DataType.Int8, it.Value.AttributeIndex);
                    GXCommon.SetData(settings, buff, DataType.UInt16, it.Value.DataIndex);
                }
                return buff.Array();
            }
            if (e.Index == 3)
            {
                buff.SetUInt8(DataType.Structure);
                buff.SetUInt8(3);
                GXCommon.SetData(settings, buff, DataType.UInt8, Service);
                if (Destination != null)
                {
                    GXCommon.SetData(settings, buff, DataType.OctetString, ASCIIEncoding.ASCII.GetBytes(Destination));
                }
                else
                {
                    GXCommon.SetData(settings, buff, DataType.OctetString, null);
                }
                GXCommon.SetData(settings, buff, DataType.UInt8, Message);
                return buff.Array();
            }
            if (e.Index == 4)
            {
                buff.SetUInt8(DataType.Array);
                GXCommon.SetObjectCount(CommunicationWindow.Count, buff);
                foreach (KeyValuePair<GXDateTime, GXDateTime> it in CommunicationWindow)
                {
                    buff.SetUInt8(DataType.Structure);
                    buff.SetUInt8(2);
                    GXCommon.SetData(settings, buff, DataType.OctetString, it.Key);
                    GXCommon.SetData(settings, buff, DataType.OctetString, it.Value);
                }
                return buff.Array();
            }
            if (e.Index == 5)
            {
                return RandomisationStartInterval;
            }
            if (e.Index == 6)
            {
                return NumberOfRetries;
            }
            if (e.Index == 7)
            {
                return RepetitionDelay;
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
                PushObjectList.Clear();
                if (e.Value is object[])
                {
                    foreach (object it in e.Value as object[])
                    {
                        object[] tmp = it as object[];
                        ObjectType type = (ObjectType)Convert.ToUInt16(tmp[0]);
                        string ln = GXCommon.ToLogicalName(tmp[1]);
                        GXDLMSObject obj = settings.Objects.FindByLN(type, ln);
                        if (obj == null)
                        {
                            obj = GXDLMSClient.CreateObject(type);
                            obj.LogicalName = ln;
                        }
                        GXDLMSCaptureObject co = new GXDLMSCaptureObject(Convert.ToInt32(tmp[2]), Convert.ToInt32(tmp[3]));
                        PushObjectList.Add(new KeyValuePair<GXDLMSObject, GXDLMSCaptureObject>(obj, co));
                    }
                }
            }
            else if (e.Index == 3)
            {
                if (e.Value is object[] tmp)
                {
                    Service = (ServiceType)Convert.ToInt32(tmp[0]);
                    //LN can be used with HDLC 
                    if (((byte[])tmp[1]).Length == 6 && ((byte[])tmp[1])[5] == 0xFF)
                    {
                        Destination = GXCommon.ToLogicalName((byte[])tmp[1]);
                    }
                    else
                    {
                        Destination = (string)GXDLMSClient.ChangeType((byte[])tmp[1], DataType.String, settings.UseUtc2NormalTime);
                    }
                    Message = (MessageType)Convert.ToInt32(tmp[2]);
                }
            }
            else if (e.Index == 4)
            {
                CommunicationWindow.Clear();
                if (e.Value is object[])
                {
                    foreach (object it in e.Value as object[])
                    {
                        object[] tmp = it as object[];
                        GXDateTime start = GXDLMSClient.ChangeType((byte[])tmp[0], DataType.DateTime, settings.UseUtc2NormalTime) as GXDateTime;
                        GXDateTime end = GXDLMSClient.ChangeType((byte[])tmp[1], DataType.DateTime, settings.UseUtc2NormalTime) as GXDateTime;
                        CommunicationWindow.Add(new KeyValuePair<GXDateTime, GXDateTime>(start, end));
                    }
                }
            }
            else if (e.Index == 5)
            {
                RandomisationStartInterval = (ushort)e.Value;
            }
            else if (e.Index == 6)
            {
                NumberOfRetries = (byte)e.Value;
            }
            else if (e.Index == 7)
            {
                RepetitionDelay = (ushort)e.Value;
            }
            else
            {
                e.Error = ErrorCode.ReadWriteDenied;
            }
        }

        void IGXDLMSBase.Load(GXXmlReader reader)
        {
            PushObjectList.Clear();
            if (reader.IsStartElement("ObjectList", true))
            {
                while (reader.IsStartElement("Item", true))
                {
                    ObjectType ot = (ObjectType)reader.ReadElementContentAsInt("ObjectType");
                    string ln = reader.ReadElementContentAsString("LN");
                    int ai = reader.ReadElementContentAsInt("AI");
                    int di = reader.ReadElementContentAsInt("DI");
                    reader.ReadEndElement("ObjectList");
                    GXDLMSCaptureObject co = new GXDLMSCaptureObject(ai, di);
                    GXDLMSObject obj = reader.Objects.FindByLN(ot, ln);
                    PushObjectList.Add(new KeyValuePair<Objects.GXDLMSObject, Objects.GXDLMSCaptureObject>(obj, co));
                }
                reader.ReadEndElement("ObjectList");
            }

            Service = (ServiceType)reader.ReadElementContentAsInt("Service");
            Destination = reader.ReadElementContentAsString("Destination");
            Message = (MessageType)reader.ReadElementContentAsInt("Message");
            CommunicationWindow.Clear();
            if (reader.IsStartElement("CommunicationWindow", true))
            {
                while (reader.IsStartElement("Item", true))
                {
                    GXDateTime start = new GXDateTime(reader.ReadElementContentAsString("Start"));
                    GXDateTime end = new GXDateTime(reader.ReadElementContentAsString("End"));
                    CommunicationWindow.Add(new KeyValuePair<DLMS.GXDateTime, DLMS.GXDateTime>(start, end));
                }
                reader.ReadEndElement("CommunicationWindow");
            }
            RandomisationStartInterval = (ushort)reader.ReadElementContentAsInt("RandomisationStartInterval");
            NumberOfRetries = (byte)reader.ReadElementContentAsInt("NumberOfRetries");
            RepetitionDelay = (ushort)reader.ReadElementContentAsInt("RepetitionDelay");
        }

        void IGXDLMSBase.Save(GXXmlWriter writer)
        {
            if (PushObjectList != null)
            {
                writer.WriteStartElement("ObjectList");
                foreach (KeyValuePair<GXDLMSObject, GXDLMSCaptureObject> it in PushObjectList)
                {
                    writer.WriteStartElement("Item");
                    writer.WriteElementString("ObjectType", (int)it.Key.ObjectType);
                    writer.WriteElementString("LN", it.Key.LogicalName);
                    writer.WriteElementString("AI", it.Value.AttributeIndex);
                    writer.WriteElementString("DI", it.Value.DataIndex);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteElementString("Service", (int)Service);
            writer.WriteElementString("Destination", Destination);
            writer.WriteElementString("Message", (int)Message);
            if (CommunicationWindow != null)
            {
                writer.WriteStartElement("CommunicationWindow");
                foreach (KeyValuePair<GXDateTime, GXDateTime> it in CommunicationWindow)
                {
                    writer.WriteStartElement("Item");
                    writer.WriteElementString("Start", it.Key.ToFormatString());
                    writer.WriteElementString("End", it.Value.ToFormatString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteElementString("RandomisationStartInterval", RandomisationStartInterval);
            writer.WriteElementString("NumberOfRetries", NumberOfRetries);
            writer.WriteElementString("RepetitionDelay", RepetitionDelay);
        }

        void IGXDLMSBase.PostLoad(GXXmlReader reader)
        {
        }

        #endregion
    }
}
