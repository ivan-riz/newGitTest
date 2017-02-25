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
using Gurux.DLMS.Enums;

namespace Gurux.DLMS.Objects
{
    public class GXDLMSSchedule : GXDLMSObject, IGXDLMSBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public GXDLMSSchedule()
        : base(ObjectType.Schedule)
        {
            Entries = new List<GXScheduleEntry>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ln">Logical Name of the object.</param>
        public GXDLMSSchedule(string ln)
        : base(ObjectType.Schedule, ln, 0)
        {
            Entries = new List<GXScheduleEntry>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ln">Logical Name of the object.</param>
        /// <param name="sn">Short Name of the object.</param>
        public GXDLMSSchedule(string ln, ushort sn)
        : base(ObjectType.Schedule, ln, sn)
        {
            Entries = new List<GXScheduleEntry>();
        }

        /// <summary>
        /// Specifies the scripts to be executed at given times.
        /// </summary>
        [XmlIgnore()]
        public List<GXScheduleEntry> Entries
        {
            get;
            set;
        }

        /// <inheritdoc cref="GXDLMSObject.GetValues"/>
        public override object[] GetValues()
        {
            return new object[] { LogicalName, Entries };
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
            //Entries
            if (CanRead(2))
            {
                attributes.Add(2);
            }
            return attributes.ToArray();
        }

        /// <inheritdoc cref="IGXDLMSBase.GetNames"/>
        string[] IGXDLMSBase.GetNames()
        {
            return new string[] { Gurux.DLMS.Properties.Resources.LogicalNameTxt, "Entries" };
        }

        int IGXDLMSBase.GetAttributeCount()
        {
            return 2;
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
                return DataType.Array;
            }
            throw new ArgumentException("GetDataType failed. Invalid attribute index.");
        }

        object IGXDLMSBase.GetValue(GXDLMSSettings settings, ValueEventArgs e)
        {
            if (e.Index == 1)
            {
                return this.LogicalName;
            }
            if (e.Index == 2)
            {
                GXByteBuffer data = new GXByteBuffer();
                data.SetUInt8((byte)DataType.Array);
                data.SetUInt8((byte)Entries.Count);
                /*
                foreach (GXScheduleEntry it in Entries)
                {
                    data.SetUInt8((byte)DataType.Structure);
                    data.SetUInt8(10);
                    //Add index.
                    data.SetUInt8((byte)DataType.UInt8);
                    data.SetUInt8(it.Index);
                    //Add enable.
                    data.SetUInt8((byte)DataType.Boolean);
                    data.SetUInt8((byte) (it.Enable ? 1 : 0));

                    //Add logical Name.
                    data.SetUInt8((byte)DataType.OctetString);
                    data.SetUInt8((byte) it.LogicalName.Length);
                    //TODO: data.SetUInt8((byte)it.LogicalName.Length);

                    //Add script selector.
                    data.SetUInt8((byte)DataType.UInt8);
                    data.SetUInt8(it.ScriptSelector);

                    //Add switch time.
                    ret = var_setDateTime(&tmp, &se->switchTime);
                    if (ret != 0)
                    {
                        var_clear(&tmp);
                        break;
                    }
                    ret = var_getBytes(&tmp, &value->byteArr);
                    var_clear(&tmp);
                    if (ret != 0)
                    {
                        break;
                    }
                    //Add validity window.
                    data.SetUInt8((byte)DataType.UInt8);
                    data.SetUInt8(it.ValidityWindow);

                    //Add exec week days.
                    ba_setUInt8(&value->byteArr, DLMS_DATA_TYPE_BIT_STRING);
                    setObjectCount(se->execWeekdays.size, &value->byteArr);
                    ba_addRange(&value->byteArr, se->execWeekdays.data, bit_getByteCount(se->execWeekdays.size));

                    //Add exec spec days.
                    ba_setUInt8(&value->byteArr, DLMS_DATA_TYPE_BIT_STRING);
                    setObjectCount(se->execSpecDays.size, &value->byteArr);
                    ba_addRange(&value->byteArr, se->execSpecDays.data, bit_getByteCount(se->execSpecDays.size));

                    //Add begin date.
                    ret = var_setDateTime(&tmp, &se->beginDate);
                    if (ret != 0)
                    {
                        var_clear(&tmp);
                        break;
                    }
                    ret = var_getBytes(&tmp, &value->byteArr);
                    var_clear(&tmp);
                    if (ret != 0)
                    {
                        break;
                    }
                    //Add end date.
                    ret = var_setDateTime(&tmp, &se->endDate);
                    if (ret != 0)
                    {
                        var_clear(&tmp);
                        break;
                    }
                    ret = var_getBytes(&tmp, &value->byteArr);
                    var_clear(&tmp);
                    if (ret != 0)
                    {
                        break;
                    }
                }
                 * */
                return data.Array();
            }
            e.Error = ErrorCode.ReadWriteDenied;
            return null;
        }

        void IGXDLMSBase.SetValue(GXDLMSSettings settings, ValueEventArgs e)
        {
            if (e.Index == 1)
            {
                if (e.Value is string)
                {
                    LogicalName = e.Value.ToString();
                }
                else
                {
                    LogicalName = GXDLMSClient.ChangeType((byte[])e.Value, DataType.OctetString).ToString();
                }
            }
            else if (e.Index == 2)
            {
                Entries.Clear();
                Object[] arr = (Object[])e.Value;
                foreach (var it in arr)
                {
                    GXScheduleEntry item = new GXScheduleEntry();
                    Object[] tmp = (Object[])it;
                    item.Index = Convert.ToByte(tmp[0]);
                    item.Enable = (bool)tmp[1];
                    item.LogicalName = GXDLMSClient.ChangeType((byte[])tmp[2], DataType.OctetString).ToString();
                    item.ScriptSelector = Convert.ToByte(tmp[3]);
                    item.SwitchTime = (GXDateTime)GXDLMSClient.ChangeType((byte[])tmp[4], DataType.DateTime);
                    item.ValidityWindow = Convert.ToByte(tmp[5]);
                    item.ExecWeekdays = (string)tmp[6];
                    item.ExecSpecDays = (string)tmp[7];
                    item.BeginDate = (GXDateTime)GXDLMSClient.ChangeType((byte[])tmp[8], DataType.DateTime);
                    item.EndDate = (GXDateTime)GXDLMSClient.ChangeType((byte[])tmp[9], DataType.DateTime);
                    Entries.Add(item);
                }
            }
            else
            {
                e.Error = ErrorCode.ReadWriteDenied;
            }
        }
        #endregion
    }
}
