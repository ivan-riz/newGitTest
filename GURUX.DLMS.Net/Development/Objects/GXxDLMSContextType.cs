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

namespace Gurux.DLMS.Objects
{
    public class GXxDLMSContextType
    {
        public string Conformance
        {
            get;
            set;
        }
        public UInt16 MaxReceivePduSize
        {
            get;
            set;
        }
        public UInt16 MaxSendPpuSize
        {
            get;
            set;
        }
        public Byte DlmsVersionNumber
        {
            get;
            set;
        }
        public sbyte QualityOfService
        {
            get;
            set;
        }
        public byte[] CypheringInfo
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Conformance + " " + MaxReceivePduSize.ToString()  + " " + MaxSendPpuSize.ToString()
                 + " " + DlmsVersionNumber.ToString()  + " " + QualityOfService.ToString() + " " +
                 Gurux.DLMS.Internal.GXCommon.ToHex(CypheringInfo, true);
        }
    };
}
