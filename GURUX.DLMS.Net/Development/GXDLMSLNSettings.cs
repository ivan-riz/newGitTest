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
using Gurux.DLMS.Internal;
using Gurux.DLMS.Enums;

namespace Gurux.DLMS
{
    /// <summary>
    /// GXDLMSLNSettings contains commands for retrieving, and setting, the
    /// Logical Name settings of the server, shortly said LN referencing support.
    /// </summary>
    public class GXDLMSLNSettings
    {
        /// <summary>
        /// Settings.
        /// </summary>
        internal byte[] ConformanceBlock;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GXDLMSLNSettings()
        {
            ConformanceBlock = new byte[3];
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GXDLMSLNSettings(byte[] conformanceBlock)
        {
            if (conformanceBlock == null || conformanceBlock.Length != 3)
            {
                throw new ArgumentException("Invalid conformance block.");
            }
            ConformanceBlock = conformanceBlock;
        }

        /// <summary>
        /// Conformance block.
        /// </summary>
        public Conformance Conformance
        {
            get
            {
                GXByteBuffer bb = new GXByteBuffer(4);
                bb.SetUInt8(0);
                bb.Set(ConformanceBlock);
                return (Conformance)bb.GetUInt32();
            }
            set
            {
                GXByteBuffer bb = new GXByteBuffer(4);
                bb.SetUInt32((UInt32)value);
                bb.Position = 1;
                bb.Get(ConformanceBlock);
            }
        }


        /// <summary>
        /// User can initialize own conformance block.
        /// </summary>
        /// <param name="conformanceBlock"></param> 
        public void SetConformanceBlock(byte[] conformanceBlock)
        {
            if (conformanceBlock == null || conformanceBlock.Length != 3)
            {
                throw new ArgumentException("Invalid conformance block.");
            }
            ConformanceBlock = conformanceBlock;
        }

        /// <summary>
        /// Get conformance block bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] GetConformanceBlock()
        {
            return ConformanceBlock;
        }

        /// <summary>
        /// Clear all bits.
        /// </summary>
        public void Clear()
        {
            ConformanceBlock[0] = 0;
            ConformanceBlock[1] = 0;
            ConformanceBlock[2] = 0;
        }

        public void CopyTo(GXDLMSLNSettings target)
        {
            target.ConformanceBlock = this.ConformanceBlock;
        }

        /// <summary>
        /// Is general protection supported.
        /// </summary>
        public bool GeneralProtection
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[0], 0x40);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[0], 0x40, value);
            }
        }

        /// <summary>
        /// Is general block transfer supported.
        /// </summary>
        public bool GeneralBlockTransfer
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[0], 0x20);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[0], 0x20, value);
            }
        }

        /// <summary>
        /// Is attribute 0 set supported.
        /// </summary>
        public bool Attribute0SetReferencing
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[1], 0x80);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[1], 0x80, value);
            }
        }

        /// <summary>
        /// Is priority management supported.
        /// </summary>
        public bool PriorityManagement
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[1], 0x40);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[1], 0x40, value);
            }
        }

        /// <summary>
        /// Is attribute 0 get supported.
        /// </summary>
        public bool Attribute0GetReferencing
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[1], 0x20);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[1], 0x20, value);
            }
        }

        /// <summary>
        /// Checks, if data from the server can be read in blocks.
        /// </summary>
        public bool GetBlockTransfer
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[1], 0x10);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[1], 0x10, value);
            }
        }

        /// <summary>
        /// Checks, if data to the server can be written in blocks.
        /// </summary>
        public bool SetBlockTransfer
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[1], 0x8);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[1], 0x8, value);
            }

        }

        /// <summary>
        /// Is Action block transfer supported.
        /// </summary>
        public bool ActionBlockTransfer
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[1], 0x4);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[1], 0x4, value);
            }

        }

        /// <summary>
        /// Is it possible to read or write several COSEM objects with one query.
        /// </summary>
        /// <remarks>
        /// If true, ReadList and Write list methods can be used.
        /// </remarks>
        public bool MultipleReferences
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[1], 0x2);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[1], 0x2, value);
            }
        }

        /// <summary>
        /// Is data notification supported.
        /// </summary>
        public bool DataNotification
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[2], 0x80);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[2], 0x80, value);
            }
        }

        /// <summary>
        /// Is access used.
        /// </summary>
        public bool Access
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[2], 0x40);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[2], 0x40, value);
            }
        }

        /// <summary>
        /// Checks, if data can be read from the server.
        /// </summary>
        public bool Get
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[2], 0x10);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[2], 0x10, value);
            }

        }

        /// <summary>
        /// Checks, if data can be written to the server.
        /// </summary>
        public bool Set
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[2], 0x8);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[2], 0x8, value);
            }
        }

        /// <summary>
        /// Is selective access used.
        /// </summary>
        /// <remarks>
        /// Example Profile generic uses selective access if data is read using range or entry.
        /// With Short Name referencing this is called ParameterizedAccess.
        /// </remarks>
        /// <seealso cref="GXDLMSSNSettings.ParameterizedAccess"/>
        public bool SelectiveAccess
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[2], 0x4);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[2], 0x4, value);
            }
        }

        public bool EventNotification
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[2], 0x2);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[2], 0x2, value);
            }

        }

        /// <summary>
        /// Can client call actions.
        /// </summary>
        public bool Action
        {
            get
            {
                return GXCommon.GetBits(ConformanceBlock[2], 0x1);
            }
            set
            {
                GXCommon.SetBits(ref ConformanceBlock[2], 0x1, value);
            }

        }
    }
}
