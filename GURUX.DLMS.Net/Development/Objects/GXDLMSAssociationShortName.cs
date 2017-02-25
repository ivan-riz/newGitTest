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
using System.Xml.Serialization;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Internal;
using Gurux.DLMS.Secure;
using Gurux.DLMS.Enums;

namespace Gurux.DLMS.Objects
{
public class GXDLMSAssociationShortName : GXDLMSObject, IGXDLMSBase
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public GXDLMSAssociationShortName()
    : this("0.0.40.0.0.255", 0xFA00)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="ln">Logical Name of the object.</param>
    /// <param name="sn">Short Name of the object.</param>
    public GXDLMSAssociationShortName(string ln, ushort sn)
    : base(ObjectType.AssociationShortName, ln, sn)
    {
        ObjectList = new GXDLMSObjectCollection();
        //Default shared secred.
        Secret = System.Text.ASCIIEncoding.ASCII.GetBytes("Gurux");
        HlsSecret = System.Text.ASCIIEncoding.ASCII.GetBytes("Gurux");
    }

    /// <summary>
    /// Secret used in LLS Authentication
    /// </summary>
    [XmlIgnore()]
    public byte[] Secret
    {
        get;
        set;
    }

    /// <summary>
    /// Secret used in HLS Authentication
    /// </summary>
    [XmlIgnore()]
    public byte[] HlsSecret
    {
        get;
        set;
    }

    /// <summary>
    /// List of available objects in short name referencing.
    /// </summary>
    [XmlIgnore()]
    public GXDLMSObjectCollection ObjectList
    {
        get;
        set;
    }

    /// <summary>
    /// List of access rights.
    /// </summary>
    [XmlIgnore()]
    public object AccessRightsList
    {
        get;
        set;
    }

    /// <summary>
    /// Security setup reference.
    /// </summary>
    [XmlIgnore()]
    public string SecuritySetupReference
    {
        get;
        set;
    }

    /// <inheritdoc cref="GXDLMSObject.GetValues"/>
    public override object[] GetValues()
    {
        return new object[] { LogicalName, ObjectList, AccessRightsList, SecuritySetupReference };
    }

    #region IGXDLMSBase Members

    byte[] IGXDLMSBase.Invoke(GXDLMSSettings settings, ValueEventArgs e)
    {
        //Check reply_to_HLS_authentication
        if (e.Index == 8)
        {
            UInt32 ic = 0;
            byte[] secret;
            if (settings.Authentication == Authentication.HighGMAC)
            {
                secret = settings.SourceSystemTitle;
                GXByteBuffer bb = new GXByteBuffer(e.Parameters as byte[]);
                bb.GetUInt8();
                ic = bb.GetUInt32();
            }
            else
            {
                secret = HlsSecret;
            }
            byte[] serverChallenge = GXSecure.Secure(settings, settings.Cipher, ic, settings.StoCChallenge, secret);
            byte[] clientChallenge = (byte[])e.Parameters;
            if (GXCommon.Compare(serverChallenge, clientChallenge))
            {
                if (settings.Authentication == Authentication.HighGMAC)
                {
                    secret = settings.Cipher.SystemTitle;
                    ic = settings.Cipher.FrameCounter;
                }
                else
                {
                    secret = HlsSecret;
                }
                settings.Connected = true;
                return GXSecure.Secure(settings, settings.Cipher, ic, settings.CtoSChallenge, secret);
            }
            else
            {
                // If the password does not match.
                settings.Connected = false;
                return null;
            }

        }
        else
        {
            e.Error = ErrorCode.ReadWriteDenied;
            return null;
        }
    }

    int[] IGXDLMSBase.GetAttributeIndexToRead()
    {
        List<int> attributes = new List<int>();
        //LN is static and read only once.
        if (string.IsNullOrEmpty(LogicalName))
        {
            attributes.Add(1);
        }
        //ObjectList is static and read only once.
        if (!base.IsRead(2))
        {
            attributes.Add(2);
        }
        //AccessRightsList is static and read only once.
        if (!base.IsRead(3))
        {
            attributes.Add(3);
        }
        //SecuritySetupReference is static and read only once.
        if (!base.IsRead(4))
        {
            attributes.Add(4);
        }
        return attributes.ToArray();
    }

    /// <inheritdoc cref="IGXDLMSBase.GetNames"/>
    string[] IGXDLMSBase.GetNames()
    {
        return new string[] {Gurux.DLMS.Properties.Resources.LogicalNameTxt,
                             "Object List",
                             "Access Rights List",
                             "Security Setup Reference"
                            };
    }

    int IGXDLMSBase.GetAttributeCount()
    {
        return 4;
    }

    int IGXDLMSBase.GetMethodCount()
    {
        return 8;
    }

    private void GetAccessRights(GXDLMSSettings settings, GXDLMSObject item, GXByteBuffer data)
    {
        data.SetUInt8((byte)DataType.Structure);
        data.SetUInt8((byte)3);
        GXCommon.SetData(settings, data, DataType.UInt16, item.ShortName);
        data.SetUInt8((byte)DataType.Array);
        data.SetUInt8((byte)item.Attributes.Count);
        foreach (GXDLMSAttributeSettings att in item.Attributes)
        {
            data.SetUInt8((byte)DataType.Structure); //attribute_access_item
            data.SetUInt8((byte)3);
            GXCommon.SetData(settings, data, DataType.Int8, att.Index);
            GXCommon.SetData(settings, data, DataType.Enum, att.Access);
            GXCommon.SetData(settings, data, DataType.None, null);
        }
        data.SetUInt8((byte)DataType.Array);
        data.SetUInt8((byte)item.MethodAttributes.Count);
        foreach (GXDLMSAttributeSettings it in item.MethodAttributes)
        {
            data.SetUInt8((byte)DataType.Structure); //attribute_access_item
            data.SetUInt8((byte)2);
            GXCommon.SetData(settings, data, DataType.Int8, it.Index);
            GXCommon.SetData(settings, data, DataType.Enum, it.MethodAccess);
        }
    }

    /// <inheritdoc cref="IGXDLMSBase.GetDataType"/>
    public override DataType GetDataType(int index)
    {
        if (index == 1)
        {
            return DataType.OctetString;
        }
        else if (index == 2)
        {
            return DataType.Array;
        }
        else if (index == 3)
        {
            return DataType.Array;
        }
        else if (index == 4)
        {
            return DataType.OctetString;
        }
        throw new ArgumentException("GetDataType failed. Invalid attribute index.");
    }

    /// <summary>
    /// Returns Association View.
    /// </summary>
    private GXByteBuffer GetObjects(GXDLMSSettings settings, ValueEventArgs e)
    {
        int cnt = ObjectList.Count;
        GXByteBuffer data = new GXByteBuffer();
        //Add count only for first time.
        if (settings.Index == 0)
        {
            settings.Count = (UInt16)cnt;
            data.SetUInt8((byte)DataType.Array);
            GXCommon.SetObjectCount(cnt, data);
        }
        ushort pos = 0;
        foreach (GXDLMSObject it in ObjectList)
        {
            ++pos;
            if (!(pos <= settings.Index))
            {
                data.SetUInt8((byte)DataType.Structure);
                //Count
                data.SetUInt8((byte)4);
                //base address.
                GXCommon.SetData(settings, data, DataType.Int16, it.ShortName);
                //ClassID
                GXCommon.SetData(settings, data, DataType.UInt16, it.ObjectType);
                //Version
                GXCommon.SetData(settings, data, DataType.UInt8, 0);
                //LN
                GXCommon.SetData(settings, data, DataType.OctetString, it.LogicalName);
                ++settings.Index;
                //If PDU is full.
                if (!e.SkipMaxPduSize && data.Size >= settings.MaxPduSize)
                {
                    break;
                }
            }
        }
        return data;
    }

    object IGXDLMSBase.GetValue(GXDLMSSettings settings, ValueEventArgs e)
    {
        if (ObjectList == null)
        {
            ObjectList = new GXDLMSObjectCollection();
        }
        if (e.Index == 1)
        {
            return this.LogicalName;
        }
        else if (e.Index == 2)
        {
            return GetObjects(settings, e).Array();
        }
        else if (e.Index == 3)
        {
            bool lnExists = ObjectList.FindBySN(this.ShortName) != null;
            //Add count
            int cnt = ObjectList.Count;
            if (!lnExists)
            {
                ++cnt;
            }
            GXByteBuffer data = new GXByteBuffer();
            data.SetUInt8((byte)DataType.Array);
            GXCommon.SetObjectCount(cnt, data);
            foreach (GXDLMSObject it in ObjectList)
            {
                GetAccessRights(settings, it, data);
            }
            if (!lnExists)
            {
                GetAccessRights(settings, this, data);
            }
            return data.Array();
        }
        else if (e.Index == 4)
        {
            GXByteBuffer data = new GXByteBuffer();
            GXCommon.SetData(settings, data, DataType.OctetString, SecuritySetupReference);
            return data.Array();
        }
        e.Error = ErrorCode.ReadWriteDenied;
        return null;
    }

    void UpdateAccessRights(Object[] buff)
    {
        foreach (Object[] access in buff)
        {
            ushort sn = Convert.ToUInt16(access[0]);
            GXDLMSObject obj = ObjectList.FindBySN(sn);
            if (obj != null)
            {
                foreach (Object[] attributeAccess in (Object[])access[1])
                {
                    int id = Convert.ToInt32(attributeAccess[0]);
                    int mode = Convert.ToInt32(attributeAccess[1]);
                    obj.SetAccess(id, (AccessMode)mode);
                }
                foreach (Object[] methodAccess in (Object[])access[2])
                {
                    int id = Convert.ToInt32(methodAccess[0]);
                    int mode = Convert.ToInt32(methodAccess[1]);
                    obj.SetMethodAccess(id, (MethodAccessMode)mode);
                }
            }
        }
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
            ObjectList.Clear();
            if (e.Value != null)
            {
                foreach (Object[] item in (Object[])e.Value)
                {
                    ushort sn = (ushort)(Convert.ToInt32(item[0]) & 0xFFFF);
                    ObjectType type = (ObjectType)Convert.ToInt32(item[1]);
                    int version = Convert.ToInt32(item[2]);
                    String ln = GXDLMSObject.ToLogicalName((byte[])item[3]);
                    GXDLMSObject obj = null;
                    if (settings.Objects != null)
                    {
                        obj = settings.Objects.FindBySN(sn);
                    }
                    if (obj == null)
                    {
                        obj = Gurux.DLMS.GXDLMSClient.CreateObject(type);
                        if (obj != null)
                        {
                            obj.LogicalName = ln;
                            obj.ShortName = sn;
                            obj.Version = version;
                        }
                    }
                    //Unknown objects are not shown.
                    if (obj is IGXDLMSBase)
                    {
                        ObjectList.Add(obj);
                    }
                }
            }
        }
        else if (e.Index == 3)
        {
            if (e.Value == null)
            {
                foreach (GXDLMSObject it in ObjectList)
                {
                    for (int pos = 1; pos != (it as IGXDLMSBase).GetAttributeCount(); ++pos)
                    {
                        it.SetAccess(pos, AccessMode.NoAccess);
                    }
                }
            }
            else
            {
                UpdateAccessRights((Object[])e.Value);
            }
        }
        else if (e.Index == 4)
        {
            if (e.Value is string)
            {
                SecuritySetupReference = e.Value.ToString();
            }
            else
            {
                SecuritySetupReference = GXDLMSClient.ChangeType(e.Value as byte[], DataType.OctetString).ToString();
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
