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
using System.ComponentModel;
using Gurux.DLMS;
using System.Xml.Serialization;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Internal;
using Gurux.DLMS.Enums;

namespace Gurux.DLMS.Objects
{
    public class GXDLMSRegister : GXDLMSObject, IGXDLMSBase
    {
        protected int scaler;
        /// <summary>
        /// Constructor.
        /// </summary>
        public GXDLMSRegister()
        : base(ObjectType.Register)
        {
        }

        internal GXDLMSRegister(ObjectType type, string ln, ushort sn)
        : base(type, ln, sn)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ln">Logical Name of the object.</param>
        public GXDLMSRegister(string ln)
        : this(ObjectType.Register, ln, 0)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ln">Logical Name of the object.</param>
        /// <param name="sn">Short Name of the object.</param>
        public GXDLMSRegister(string ln, ushort sn)
        : this(ObjectType.Register, ln, sn)
        {
        }

        /// <summary>
        /// Scaler of COSEM Register object.
        /// </summary>
        [DefaultValue(1.0)]
        public double Scaler
        {
            get
            {
                return Math.Pow(10, scaler);
            }
            set
            {
                scaler = (int)Math.Log10(value);
            }
        }

        /// <summary>
        /// Unit of COSEM Register object.
        /// </summary>
        [DefaultValue(Unit.NoUnit)]
        public Unit Unit
        {
            get;
            set;
        }

        /// <summary>
        /// Value of COSEM Register object.
        /// </summary>
        /// <remarks>
        /// Register value is not serialized because XML serializer can't handle all cases.
        /// </remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [System.Xml.Serialization.XmlIgnore()]
        public object Value
        {
            get;
            set;
        }

        /// <summary>
        /// Reset value.
        /// </summary>
        /// <returns></returns>
        public byte[][] Reset(GXDLMSClient client)
        {
            return client.Method(this, 1, (byte)0);
        }

        /// <inheritdoc cref="GXDLMSObject.GetValues"/>
        public override object[] GetValues()
        {
            return new object[] { LogicalName, Value, "Scaler: " + Scaler + " Unit: " + Unit };
        }

        #region IGXDLMSBase Members


        byte[] IGXDLMSBase.Invoke(GXDLMSSettings settings, ValueEventArgs e)
        {
            // Resets the value to the default value.
            // The default value is an instance specific constant.
            if (e.Index == 1)
            {
                Value = null;
            }
            else
            {
                e.Error = ErrorCode.ReadWriteDenied;
            }
            return null;
        }

        public override bool IsRead(int index)
        {
            if (index == 3)
            {
                return this.Unit != Unit.None;
            }
            return base.IsRead(index);
        }

        int[] IGXDLMSBase.GetAttributeIndexToRead()
        {
            List<int> attributes = new List<int>();
            //LN is static and read only once.
            if (string.IsNullOrEmpty(LogicalName))
            {
                attributes.Add(1);
            }
            //ScalerUnit
            if (!IsRead(3))
            {
                attributes.Add(3);
            }
            //Value
            if (CanRead(2))
            {
                attributes.Add(2);
            }
            return attributes.ToArray();
        }

        /// <inheritdoc cref="IGXDLMSBase.GetNames"/>
        string[] IGXDLMSBase.GetNames()
        {
            return new string[] { Gurux.DLMS.Properties.Resources.LogicalNameTxt, "Scaler and Unit", "Value" };
        }

        int IGXDLMSBase.GetAttributeCount()
        {
            return 3;
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
                return base.GetDataType(index);
            }
            if (index == 3)
            {
                return DataType.Array;
            }
            if (index == 4 && this is GXDLMSExtendedRegister)
            {
                return base.GetDataType(index);
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
                return Value;
            }
            if (e.Index == 3)
            {
                GXByteBuffer data = new GXByteBuffer();
                data.SetUInt8((byte)DataType.Structure);
                data.SetUInt8(2);
                GXCommon.SetData(settings, data, DataType.Int8, scaler);
                GXCommon.SetData(settings, data, DataType.Enum, Unit);
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
                else if (e.Value == null)
                {
                    LogicalName = null;
                }
                else
                {
                    LogicalName = GXDLMSClient.ChangeType((byte[])e.Value, DataType.OctetString).ToString();
                }
            }
            else if (e.Index == 2)
            {
                if (Scaler != 1)
                {
                    try
                    {
                        Value = Convert.ToDouble(e.Value) * Scaler;
                    }
                    catch (Exception)
                    {
                        //Sometimes scaler is set for wrong Object type.
                        Value = e.Value;
                    }
                }
                else
                {
                    Value = e.Value;
                }
            }
            else if (e.Index == 3)
            {
                if (e.Value == null)
                {
                    Scaler = 1;
                    Unit = Unit.None;
                }
                else
                {
                    object[] arr = (object[])e.Value;
                    if (arr.Length != 2)
                    {
                        throw new Exception("setValue failed. Invalid scaler unit value.");
                    }
                    scaler = Convert.ToInt32(arr[0]);
                    Unit = (Unit)(Convert.ToInt32(arr[1]) & 0xFF);
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