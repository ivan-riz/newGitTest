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
using System.Linq;
using System.Text;
using Gurux.DLMS.Enums;
using System.ComponentModel;
using Gurux.DLMS.Internal;
using System.Diagnostics;
using Gurux.DLMS.Objects;

namespace Gurux.DLMS
{
    /// <summary>
    /// this class is used to handle LN commands.
    /// </summary>
    internal sealed class GXDLMSLNCommandHandler
    {
        public static void HandleGetRequest(GXDLMSSettings settings, GXDLMSServer server, GXByteBuffer data, GXByteBuffer replyData, GXDLMSTranslatorStructure xml)
        {
            //Return error if connection is not established.
            if (xml == null && !settings.Connected)
            {
                replyData.Set(GXDLMSServer.GenerateConfirmedServiceError(ConfirmedServiceError.InitiateError,
                              ServiceError.Service, (byte)Service.Unsupported));
                return;
            }

            GetCommandType type = (GetCommandType)data.GetUInt8();
            // Get invoke ID and priority.
            byte invokeID = data.GetUInt8();
            if (xml != null)
            {
                xml.AppendStartTag(Command.GetRequest);
                xml.AppendStartTag(Command.GetRequest, type);
                xml.AppendLine(TranslatorTags.InvokeId, "Value", xml.IntegerToHex(invokeID, 2));
            }

            // GetRequest normal
            if (type == GetCommandType.Normal)
            {
                GetRequestNormal(settings, server, data, replyData, xml);
            }
            else if (type == GetCommandType.NextDataBlock)
            {
                // Get request for next data block
                GetRequestNextDataBlock(settings, server, data, replyData, xml);
            }
            else if (type == GetCommandType.WithList)
            {
                // Get request with a list.
                GetRequestWithList(settings, server, data, replyData, xml);
            }
            else
            {
                Debug.WriteLine("HandleGetRequest failed. Invalid command type.");
                settings.ResetBlockIndex();
                GXByteBuffer bb = new GXByteBuffer();
                // Access Error : Device reports a hardware fault.
                bb.SetUInt8((byte)ErrorCode.HardwareFault);
                GXDLMS.GetLNPdu(new GXDLMSLNParameters(settings, Command.GetResponse, (byte)type, null, bb, (byte)ErrorCode.Ok), replyData);
            }
            if (xml != null)
            {
                xml.AppendEndTag(Command.GetRequest, type);
                xml.AppendEndTag(Command.GetRequest);
            }
        }

        ///<summary>
        ///Handle set request.
        ///</summary>
        ///<returns>
        ///Reply to the client.
        ///</returns>
        public static void HandleSetRequest(GXDLMSSettings settings, GXDLMSServer server, GXByteBuffer data, GXByteBuffer replyData, GXDLMSTranslatorStructure xml)
        {
            //Return error if connection is not established.
            if (xml == null && !settings.Connected)
            {
                replyData.Set(GXDLMSServer.GenerateConfirmedServiceError(ConfirmedServiceError.InitiateError,
                              ServiceError.Service, (byte)Service.Unsupported));
                return;
            }
            // Get type.
            SetRequestType type = (SetRequestType)data.GetUInt8();
            // Get invoke ID and priority.
            byte invoke = data.GetUInt8();
            // SetRequest normal or Set Request With First Data Block
            GXDLMSLNParameters p = new GXDLMSLNParameters(settings, Command.SetResponse, (byte)type, null, null, 0);
            if (xml != null)
            {
                xml.AppendStartTag(Command.SetRequest);
                xml.AppendStartTag(Command.SetRequest, type);
                //InvokeIdAndPriority
                xml.AppendLine(TranslatorTags.InvokeId, "Value", xml.IntegerToHex(invoke, 2));
            }
            switch (type)
            {
                case SetRequestType.Normal:
                case SetRequestType.FirstDataBlock:
                    HandleSetRequestNormal(settings, server, data, (byte)type, p, replyData, xml);
                    break;
                case SetRequestType.WithDataBlock:
                    HanleSetRequestWithDataBlock(settings, server, data, p, replyData, xml);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("HandleSetRequest failed. Unknown command.");
                    settings.ResetBlockIndex();
                    p.status = (byte)ErrorCode.HardwareFault;
                    break;
            }
            if (xml != null)
            {
                xml.AppendEndTag(Command.SetRequest, type);
                xml.AppendEndTag(Command.SetRequest);
                return;
            }
            GXDLMS.GetLNPdu(p, replyData);
        }

        ///<summary>
        /// Handle action request.
        ///</summary>
        ///<param name="Reply">
        /// Received data from the client.
        ///</param>
        ///<returns>
        ///Reply.
        ///</returns>
        public static void HandleMethodRequest(GXDLMSSettings settings, GXDLMSServer server, GXByteBuffer data, GXDLMSConnectionEventArgs connectionInfo, GXByteBuffer replyData, GXDLMSTranslatorStructure xml)
        {
            ErrorCode error = ErrorCode.Ok;
            GXByteBuffer bb = new GXByteBuffer();
            // Get type.
            ActionRequestType type = (ActionRequestType)data.GetUInt8();
            // Get invoke ID and priority.
            byte invokeId = data.GetUInt8();
            // CI
            ObjectType ci = (ObjectType)data.GetUInt16();
            byte[] ln = new byte[6];
            data.Get(ln);
            // Attribute Id
            byte id = data.GetUInt8();
            // Get parameters.
            object parameters = null;
            byte selection = data.GetUInt8();
            if (xml != null)
            {
                xml.AppendStartTag(Command.MethodRequest);
                if (type == ActionRequestType.Normal)
                {
                    xml.AppendStartTag(Command.MethodRequest, ActionRequestType.Normal);
                    xml.AppendLine(TranslatorTags.InvokeId, "Value", xml.IntegerToHex(invokeId, 2));
                    AppendMethodDescriptor(xml, (int)ci, ln, id);
                    if (selection != 0)
                    {
                        //MethodInvocationParameters
                        xml.AppendStartTag(TranslatorTags.MethodInvocationParameters);
                        GXDataInfo di = new GXDataInfo();
                        di.xml = xml;
                        GXCommon.GetData(settings, data, di);
                        xml.AppendEndTag(TranslatorTags.MethodInvocationParameters);
                    }
                    xml.AppendEndTag(Command.MethodRequest, ActionRequestType.Normal);
                }
                xml.AppendEndTag(Command.MethodRequest);
                return;
            }
            if (selection != 0)
            {
                GXDataInfo info = new GXDataInfo();
                parameters = GXCommon.GetData(settings, data, info);
            }

            GXDLMSObject obj = settings.Objects.FindByLN(ci, GXDLMSObject.ToLogicalName(ln));
            if (!settings.Connected && (ci != ObjectType.AssociationLogicalName || id != 1))
            {
                replyData.Set(GXDLMSServer.GenerateConfirmedServiceError(ConfirmedServiceError.InitiateError,
                              ServiceError.Service, (byte)Service.Unsupported));
                return;
            }

            if (obj == null)
            {
                obj = server.NotifyFindObject(ci, 0, GXDLMSObject.ToLogicalName(ln));
            }
            if (obj == null)
            {
                // Device reports a undefined object.
                error = ErrorCode.UndefinedObject;
            }
            else
            {
                if (obj.GetMethodAccess(id) == MethodAccessMode.NoAccess)
                {
                    error = ErrorCode.ReadWriteDenied;
                }
                else
                {
                    ValueEventArgs e = new ValueEventArgs(settings, obj, id, 0, parameters);
                    server.NotifyAction(new ValueEventArgs[] { e });
                    byte[] actionReply;
                    if (e.Handled)
                    {
                        actionReply = (byte[])e.Value;
                    }
                    else
                    {
                        actionReply = (obj as IGXDLMSBase).Invoke(settings, e);
                    }
                    //Set default action reply if not given.
                    if (actionReply != null && e.Error == 0)
                    {
                        //Add return parameters
                        bb.SetUInt8(1);
                        //Add parameters error code.
                        bb.SetUInt8(0);
                        GXCommon.SetData(settings, bb, GXCommon.GetValueType(actionReply), actionReply);
                    }
                    else
                    {
                        error = e.Error;
                        //Add return parameters
                        bb.SetUInt8(0);
                    }
                }
            }

            GXDLMSLNParameters p = new GXDLMSLNParameters(settings, Command.MethodResponse, 1, null, bb, (byte)error);
            GXDLMS.GetLNPdu(p, replyData);
            //If High level authentication fails.
            if (!settings.Connected && obj is GXDLMSAssociationLogicalName && id == 1)
            {
                server.NotifyInvalidConnection(connectionInfo);
            }
        }

        private static void AppendAttributeDescriptor(GXDLMSTranslatorStructure xml, int ci, byte[] ln, byte attributeIndex)
        {
            xml.AppendStartTag(TranslatorTags.AttributeDescriptor);
            xml.AppendLine(TranslatorTags.ClassId, "Value", xml.IntegerToHex((int)ci, 4));
            xml.AppendLine(TranslatorTags.InstanceId, "Value", GXCommon.ToHex(ln, false));
            xml.AppendLine(TranslatorTags.AttributeId, "Value", xml.IntegerToHex(attributeIndex, 2));
            xml.AppendEndTag(TranslatorTags.AttributeDescriptor);
        }

        private static void AppendMethodDescriptor(GXDLMSTranslatorStructure xml, int ci, byte[] ln, byte attributeIndex)
        {
            xml.AppendStartTag(TranslatorTags.MethodDescriptor);
            xml.AppendLine(TranslatorTags.ClassId, "Value", xml.IntegerToHex((int)ci, 4));
            xml.AppendLine(TranslatorTags.InstanceId, "Value", GXCommon.ToHex(ln, false));
            xml.AppendLine(TranslatorTags.MethodId, "Value", xml.IntegerToHex(attributeIndex, 2));
            xml.AppendEndTag(TranslatorTags.MethodDescriptor);
        }

        /// <summary>
        /// Handle get request normal command.
        /// </summary>
        /// <param name="data">Received data.</param>
        private static void GetRequestNormal(GXDLMSSettings settings, GXDLMSServer server, GXByteBuffer data, GXByteBuffer replyData, GXDLMSTranslatorStructure xml)
        {
            ValueEventArgs e = null;
            GXByteBuffer bb = new GXByteBuffer();
            // Get type.
            ErrorCode status = ErrorCode.Ok;

            settings.Count = settings.Index = 0;
            settings.ResetBlockIndex();
            // CI
            ObjectType ci = (ObjectType)data.GetUInt16();
            byte[] ln = new byte[6];
            data.Get(ln);
            // Attribute Id
            byte attributeIndex = data.GetUInt8();

            // AccessSelection
            byte selection = data.GetUInt8();
            byte selector = 0;
            object parameters = null;
            GXDataInfo info = new GXDataInfo();
            if (selection != 0)
            {
                selector = data.GetUInt8();
            }
            if (xml != null)
            {
                AppendAttributeDescriptor(xml, (int)ci, ln, attributeIndex);
                if (selection != 0)
                {
                    info.xml = xml;
                    xml.AppendStartTag(TranslatorTags.AccessSelection);
                    xml.AppendLine(TranslatorTags.AccessSelector, "Value", xml.IntegerToHex(selector, 2));
                    xml.AppendStartTag(TranslatorTags.AccessParameters);
                    GXCommon.GetData(settings, data, info);
                    xml.AppendEndTag(TranslatorTags.AccessParameters);
                    xml.AppendEndTag(TranslatorTags.AccessSelection);
                }
                return;
            }
            if (selection != 0)
            {
                parameters = GXCommon.GetData(settings, data, info);
            }

            GXDLMSObject obj = settings.Objects.FindByLN(ci, GXDLMSObject.ToLogicalName(ln));
            if (obj == null)
            {
                obj = server.NotifyFindObject(ci, 0, GXDLMSObject.ToLogicalName(ln));
            }
            if (obj == null)
            {
                // "Access Error : Device reports a undefined object."
                status = ErrorCode.UndefinedObject;
            }
            else
            {
                if (obj.GetAccess(attributeIndex) == AccessMode.NoAccess)
                {
                    //Read Write denied.
                    status = ErrorCode.ReadWriteDenied;
                }
                else
                {
                    e = new ValueEventArgs(settings, obj, attributeIndex, selector, parameters);
                    server.NotifyRead(new ValueEventArgs[] { e });
                    object value;
                    if (e.Handled)
                    {
                        value = e.Value;
                    }
                    else
                    {
                        value = (obj as IGXDLMSBase).GetValue(settings, e);
                    }
                    if (e.ByteArray)
                    {
                        bb.Set((byte[])value);
                    }
                    else
                    {
                        GXDLMS.AppendData(settings, obj, attributeIndex, bb, value);
                    }
                    status = e.Error;
                }
            }
            GXDLMS.GetLNPdu(new GXDLMSLNParameters(settings, Command.GetResponse, 1, null, bb, (byte)status), replyData);
            if (settings.Count != settings.Index || bb.Size != bb.Position)
            {
                server.transaction = new GXDLMSLongTransaction(new ValueEventArgs[] { e }, Command.GetRequest, bb);
            }
        }

        /// <summary>
        /// Handle get request next data block command.
        /// </summary>
        /// <param name="data">Received data.</param>
        private static void GetRequestNextDataBlock(GXDLMSSettings settings, GXDLMSServer server, GXByteBuffer data, GXByteBuffer replyData, GXDLMSTranslatorStructure xml)
        {
            GXByteBuffer bb = new GXByteBuffer();
            UInt32 index;
            // Get block index.
            index = data.GetUInt32();
            if (xml != null)
            {
                xml.AppendLine(TranslatorTags.BlockNumber, null, xml.IntegerToHex(index, 8));
                return;
            }
            if (index != settings.BlockIndex)
            {
                Debug.WriteLine("handleGetRequest failed. Invalid block number. " + settings.BlockIndex + "/" + index);
                GXDLMS.GetLNPdu(new GXDLMSLNParameters(settings, Command.GetResponse, 2, null, bb, (byte)ErrorCode.DataBlockNumberInvalid), replyData);
            }
            else
            {
                settings.IncreaseBlockIndex();
                GXDLMSLNParameters p = new GXDLMSLNParameters(settings, Command.GetResponse, 2, null, bb, (byte)ErrorCode.Ok);
                //If transaction is not in progress.
                if (server.transaction == null)
                {
                    p.status = (byte)ErrorCode.NoLongGetOrReadInProgress;
                }
                else
                {
                    bb.Set(server.transaction.data);
                    bool moreData = settings.Index != settings.Count;
                    if (moreData)
                    {
                        //If there is multiple blocks on the buffer.
                        //This might happen when Max PDU size is very small.
                        if (bb.Size < settings.MaxPduSize)
                        {
                            foreach (ValueEventArgs arg in server.transaction.targets)
                            {
                                object value;
                                if (arg.Handled)
                                {
                                    server.NotifyRead(new ValueEventArgs[] { arg });
                                    value = arg.Value;
                                }
                                else
                                {
                                    value = (arg.Target as IGXDLMSBase).GetValue(settings, arg);
                                }
                                //Add data.
                                if (arg.ByteArray)
                                {
                                    bb.Set((byte[])value);
                                }
                                else
                                {
                                    GXDLMS.AppendData(settings, arg.Target, arg.Index, bb, value);
                                }
                            }
                            moreData = settings.Index != settings.Count;
                        }
                    }
                    p.multipleBlocks = true;
                    GXDLMS.GetLNPdu(p, replyData);
                    if (moreData || bb.Size - bb.Position != 0)
                    {
                        server.transaction.data = bb;
                    }
                    else
                    {
                        server.transaction = null;
                    }
                }
            }
        }

        /// <summary>
        /// Handle get request with list command.
        /// </summary>
        /// <param name="data">Received data.</param>
        private static void GetRequestWithList(GXDLMSSettings settings, GXDLMSServer server, GXByteBuffer data, GXByteBuffer replyData, GXDLMSTranslatorStructure xml)
        {
            ValueEventArgs e;
            GXByteBuffer bb = new GXByteBuffer();
            int pos;
            int cnt = GXCommon.GetObjectCount(data);
            GXCommon.SetObjectCount(cnt, bb);
            List<ValueEventArgs> list = new List<ValueEventArgs>();
            if (xml != null)
            {
                xml.AppendStartTag(TranslatorTags.AttributeDescriptorList, "Qty", xml.IntegerToHex(cnt, 2));
            }
            try
            {
                for (pos = 0; pos != cnt; ++pos)
                {
                    ObjectType ci = (ObjectType)data.GetUInt16();
                    byte[] ln = new byte[6];
                    data.Get(ln);
                    short attributeIndex = data.GetUInt8();
                    // AccessSelection
                    int selection = data.GetUInt8();
                    int selector = 0;
                    object parameters = null;
                    if (selection != 0)
                    {
                        selector = data.GetUInt8();
                        GXDataInfo info = new GXDataInfo();
                        parameters = GXCommon.GetData(settings, data, info);
                    }
                    if (xml != null)
                    {
                        xml.AppendStartTag(TranslatorTags.AttributeDescriptorWithSelection);
                        xml.AppendStartTag(TranslatorTags.AttributeDescriptor);
                        xml.AppendLine(TranslatorTags.ClassId, "Value", xml.IntegerToHex((int)ci, 4));
                        xml.AppendLine(TranslatorTags.InstanceId, "Value", GXCommon.ToHex(ln, false));
                        xml.AppendLine(TranslatorTags.AttributeId, "Value", xml.IntegerToHex(attributeIndex, 2));
                        xml.AppendEndTag(TranslatorTags.AttributeDescriptor);
                        xml.AppendEndTag(TranslatorTags.AttributeDescriptorWithSelection);
                    }
                    else
                    {
                        GXDLMSObject obj = settings.Objects.FindByLN(ci, GXDLMSObject.ToLogicalName(ln));
                        if (obj == null)
                        {
                            obj = server.NotifyFindObject(ci, 0, GXDLMSObject.ToLogicalName(ln));
                        }
                        if (obj == null)
                        {
                            // "Access Error : Device reports a undefined object."
                            e = new ValueEventArgs(settings, obj, attributeIndex, 0, 0);
                            e.Error = ErrorCode.UndefinedObject;
                            list.Add(e);
                        }
                        else
                        {
                            if (obj.GetAccess(attributeIndex) == AccessMode.NoAccess)
                            {
                                //Read Write denied.
                                ValueEventArgs arg = new ValueEventArgs(settings, obj, attributeIndex, 0, null);
                                arg.Error = ErrorCode.ReadWriteDenied;
                                list.Add(arg);
                            }
                            else
                            {
                                ValueEventArgs arg = new ValueEventArgs(settings, obj, attributeIndex, selector, parameters);
                                list.Add(arg);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (xml == null)
                {
                    throw ex;
                }
            }
            if (xml != null)
            {
                xml.AppendEndTag(TranslatorTags.AttributeDescriptorList);
                return;
            }

            server.NotifyRead(list.ToArray());
            object value;
            pos = 0;
            foreach (ValueEventArgs it in list)
            {
                try
                {
                    if (it.Handled)
                    {
                        value = it.Value;
                    }
                    else
                    {
                        value = (it.Target as IGXDLMSBase).GetValue(settings, it);
                    }
                    bb.SetUInt8(it.Error);
                    if (it.ByteArray)
                    {
                        bb.Set((byte[])value);
                    }
                    else
                    {
                        GXDLMS.AppendData(settings, it.Target, it.Index, bb, value);
                    }
                }
                catch (Exception)
                {
                    bb.SetUInt8((byte)ErrorCode.HardwareFault);
                }
                if (settings.Index != settings.Count)
                {
                    server.transaction = new GXDLMSLongTransaction(list.ToArray(), Command.GetRequest, null);
                }
                ++pos;
            }
            GXDLMSLNParameters p = new GXDLMSLNParameters(settings, Command.GetResponse, 3, null, bb, 0xFF);
            GXDLMS.GetLNPdu(p, replyData);
        }


        private static void HandleSetRequestNormal(GXDLMSSettings settings, GXDLMSServer server, GXByteBuffer data, byte type, GXDLMSLNParameters p, GXByteBuffer replyData, GXDLMSTranslatorStructure xml)
        {
            object value = null;
            GXDataInfo reply = new GXDataInfo();
            // CI
            ObjectType ci = (ObjectType)data.GetUInt16();
            byte[] ln = new byte[6];
            data.Get(ln);
            // Attribute index.
            byte index = data.GetUInt8();
            // Get Access Selection.
            data.GetUInt8();
            if (xml != null)
            {
                AppendAttributeDescriptor(xml, (int)ci, ln, index);
                xml.AppendStartTag(TranslatorTags.Value);
                GXDataInfo di = new GXDataInfo();
                di.xml = xml;
                value = GXCommon.GetData(settings, data, di);
                if (!di.Complete)
                {
                    value = GXCommon.ToHex(data.Data, false, data.Position, data.Size - data.Position);
                }
                else if (value is byte[])
                {
                    value = GXCommon.ToHex((byte[])value, false);
                }
                xml.AppendEndTag(TranslatorTags.Value);
                return;
            }
            if (type == 2)
            {
                p.multipleBlocks = data.GetUInt8() == 0;
                UInt32 blockNumber = data.GetUInt32();
                if (blockNumber != settings.BlockIndex)
                {
                    Debug.WriteLine("HandleSetRequest failed. Invalid block number. " + settings.BlockIndex + "/" + blockNumber);
                    p.status = (byte)ErrorCode.DataBlockNumberInvalid;
                    return;
                }
                settings.IncreaseBlockIndex();
                int size = GXCommon.GetObjectCount(data);
                int realSize = data.Size - data.Position;
                if (size != realSize)
                {
                    Debug.WriteLine("HandleSetRequest failed. Invalid block size.");
                    p.status = (byte)ErrorCode.DataBlockUnavailable;
                    return;
                }
            }
            if (!p.multipleBlocks)
            {
                settings.ResetBlockIndex();
                value = GXCommon.GetData(settings, data, reply);
            }

            GXDLMSObject obj = settings.Objects.FindByLN(ci, GXDLMSObject.ToLogicalName(ln));
            if (obj == null)
            {
                obj = server.NotifyFindObject(ci, 0, GXDLMSObject.ToLogicalName(ln));
            }
            // If target is unknown.
            if (obj == null)
            {
                // Device reports a undefined object.
                p.status = (byte)ErrorCode.UndefinedObject;
            }
            else
            {
                AccessMode am = obj.GetAccess(index);
                // If write is denied.
                if (am != AccessMode.Write && am != AccessMode.ReadWrite)
                {
                    //Read Write denied.
                    p.status = (byte)ErrorCode.ReadWriteDenied;
                }
                else
                {
                    try
                    {
                        if (value is byte[])
                        {
                            DataType dt = (obj as IGXDLMSBase).GetDataType(index);
                            if (dt != DataType.None && dt != DataType.OctetString)
                            {
                                value = GXDLMSClient.ChangeType((byte[])value, dt);
                            }
                        }
                        ValueEventArgs e = new ValueEventArgs(settings, obj, index, 0, null);
                        e.Value = value;
                        ValueEventArgs[] list = new ValueEventArgs[] { e };
                        if (p.multipleBlocks)
                        {
                            server.transaction = new GXDLMSLongTransaction(list, Command.GetRequest, data);
                        }
                        server.NotifyWrite(list);
                        if (e.Error != 0)
                        {
                            p.status = (byte)e.Error;
                        }
                        else if (!e.Handled && !p.multipleBlocks)
                        {
                            (obj as IGXDLMSBase).SetValue(settings, e);
                        }
                    }
                    catch (Exception)
                    {
                        p.status = (byte)ErrorCode.HardwareFault;
                    }
                }
            }
        }

        private static void HanleSetRequestWithDataBlock(GXDLMSSettings settings, GXDLMSServer server, GXByteBuffer data, GXDLMSLNParameters p, GXByteBuffer replyData, GXDLMSTranslatorStructure xml)
        {
            GXDataInfo reply = new GXDataInfo();
            p.multipleBlocks = data.GetUInt8() == 0;
            UInt32 blockNumber = data.GetUInt32();
            if (blockNumber != settings.BlockIndex)
            {
                Debug.WriteLine("HanleSetRequestWithDataBlock failed. Invalid block number. " + settings.BlockIndex + "/" + blockNumber);
                p.status = (byte)ErrorCode.DataBlockNumberInvalid;
            }
            else
            {
                int size = GXCommon.GetObjectCount(data);
                int realSize = data.Size - data.Position;
                if (size != realSize)
                {
                    Debug.WriteLine("HanleSetRequestWithDataBlock failed. Invalid block size.");
                    p.status = (byte)ErrorCode.DataBlockUnavailable;
                }
                server.transaction.data.Set(data);
                //If all data is received.
                if (!p.multipleBlocks)
                {
                    try
                    {
                        object value = GXCommon.GetData(settings, server.transaction.data, reply);
                        if (value is byte[])
                        {
                            DataType dt = (server.transaction.targets[0].Target as IGXDLMSBase).GetDataType(server.transaction.targets[0].Index);
                            if (dt != DataType.None && dt != DataType.OctetString)
                            {
                                value = GXDLMSClient.ChangeType((byte[])value, dt);
                            }
                        }
                        server.transaction.targets[0].Value = value;
                        server.NotifyWrite(server.transaction.targets);
                        if (!server.transaction.targets[0].Handled && !p.multipleBlocks)
                        {
                            (server.transaction.targets[0].Target as IGXDLMSBase).SetValue(settings, server.transaction.targets[0]);
                        }
                    }
                    catch (Exception)
                    {
                        p.status = (byte)ErrorCode.HardwareFault;
                    }
                    finally
                    {
                        server.transaction = null;
                    }
                    settings.ResetBlockIndex();
                }
            }
            p.multipleBlocks = true;
        }

        ///<summary>
        /// Handle Access request.
        ///</summary>
        ///<param name="Reply">
        /// Received data from the client.
        /// </param>
        ///<returns>
        /// Reply.
        ///</returns>
        public static void HandleAccessRequest(GXDLMSSettings settings, GXDLMSServer server, GXByteBuffer data,
                                               GXByteBuffer reply, GXDLMSTranslatorStructure xml)
        {
            //Return error if connection is not established.
            if (xml == null && !settings.Connected)
            {
                reply.Set(GXDLMSServer.GenerateConfirmedServiceError(ConfirmedServiceError.InitiateError,
                          ServiceError.Service, (byte)Service.Unsupported));
                return;
            }
            //Get long invoke id and priority.
            UInt32 invokeId = data.GetUInt32();
            int len = GXCommon.GetObjectCount(data);
            byte[] tmp = null;
            // If date time is given.
            if (len != 0)
            {
                tmp = new byte[len];
                data.Get(tmp);
                if (xml == null)
                {
                    DataType dt = DataType.DateTime;
                    if (len == 4)
                    {
                        dt = DataType.Time;
                    }
                    else if (len == 5)
                    {
                        dt = DataType.Date;
                    }
                    GXDataInfo info = new GXDataInfo();
                    info.Type = dt;
                    GXCommon.GetData(settings, new GXByteBuffer(tmp), info);

                }
            }
            // Get object count.
            int cnt = GXCommon.GetObjectCount(data);
            if (xml != null)
            {
                xml.AppendStartTag(Command.AccessRequest);
                xml.AppendLine(TranslatorTags.LongInvokeId, "Value", xml.IntegerToHex(invokeId, 2));
                xml.AppendLine(TranslatorTags.DateTime, "Value", GXCommon.ToHex(tmp, false));
                xml.AppendStartTag(TranslatorTags.AccessRequestBody);
                xml.AppendStartTag(TranslatorTags.ListOfAccessRequestSpecification, "Qty",
                                   xml.IntegerToHex(cnt, 2));
            }
            AccessServiceCommandType type;
            for (int pos = 0; pos != cnt; ++pos)
            {
                type = (AccessServiceCommandType)data.GetUInt8();
                if (!(type == AccessServiceCommandType.Get ||
                        type == AccessServiceCommandType.Set ||
                        type == AccessServiceCommandType.Action))
                {
                    throw new ArgumentException("Invalid access service command type.");
                }
                // CI
                ObjectType ci = (ObjectType)data.GetUInt16();
                byte[] ln = new byte[6];
                data.Get(ln);
                // Attribute Id
                byte attributeIndex = data.GetUInt8();
                if (xml != null)
                {
                    xml.AppendStartTag(TranslatorTags.AccessRequestSpecification);
                    xml.AppendStartTag(Command.AccessRequest, type);
                    AppendAttributeDescriptor(xml, (int)ci, ln, attributeIndex);
                    xml.AppendEndTag(Command.AccessRequest, type);
                    xml.AppendEndTag(TranslatorTags.AccessRequestSpecification);

                }
            }
            if (xml != null)
            {
                xml.AppendEndTag(TranslatorTags.ListOfAccessRequestSpecification);
                xml.AppendStartTag(TranslatorTags.AccessRequestListOfData, "Qty", xml.IntegerToHex(cnt, 2));
            }
            // Get data count.
            cnt = GXCommon.GetObjectCount(data);
            for (int pos = 0; pos != cnt; ++pos)
            {

                GXDataInfo di = new GXDataInfo();
                di.xml = xml;
                if (xml != null && xml.OutputType == TranslatorOutputType.StandardXml)
                {
                    xml.AppendStartTag(Command.WriteRequest, SingleReadResponse.Data);
                }
                object value = GXCommon.GetData(settings, data, di);
                if (!di.Complete)
                {
                    value = GXCommon.ToHex(data.Data, false, data.Position, data.Size - data.Position);
                }
                else if (value is byte[])
                {
                    value = GXCommon.ToHex((byte[])value, false);
                }
                if (xml != null && xml
                        .OutputType == TranslatorOutputType.StandardXml)
                {
                    xml.AppendEndTag(Command.WriteRequest, SingleReadResponse.Data);
                }
            }
            if (xml != null)
            {
                xml.AppendEndTag(TranslatorTags.AccessRequestListOfData);
                xml.AppendEndTag(TranslatorTags.AccessRequestBody);
                xml.AppendEndTag(Command.AccessRequest);
            }
        }
    }
}
