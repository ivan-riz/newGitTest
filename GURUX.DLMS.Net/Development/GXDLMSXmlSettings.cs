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

using Gurux.DLMS.Enums;
using System.Text;
using System.Collections.Generic;
using System;
using Gurux.DLMS.Secure;

namespace Gurux.DLMS
{
    internal class GXDLMSXmlSettings
    {
        public AssociationResult result = AssociationResult.Accepted;
        public SourceDiagnostic diagnostic = SourceDiagnostic.None;
        public ReleaseRequestReason reason = ReleaseRequestReason.Normal;
        public Command command;
        public int count = 0;
        public byte requestType = 0xFF;
        public GXByteBuffer attributeDescriptor = new GXByteBuffer();
        public GXByteBuffer data = new GXByteBuffer();
        public GXDLMSSettings settings = new GXDLMSSettings(true);
        public SortedList<string, int> tags = new SortedList<string, int>();
        public GXDateTime time = DateTime.MinValue;
        /// <summary>
        /// Are numeric values shows as hex.
        /// </summary>
        private bool showNumericsAsHex;
        public bool showStringAsHex = false;

        public TranslatorOutputType OutputType
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="list"></param>
        public GXDLMSXmlSettings(TranslatorOutputType type,
                                 bool numericsAsHex, bool hex, SortedList<string, int> list)
        {
            OutputType = type;
            showNumericsAsHex = numericsAsHex;
            showStringAsHex = hex;
            settings.InterfaceType = InterfaceType.PDU;
            settings.Cipher = new GXCiphering(ASCIIEncoding.ASCII.GetBytes("ABCDEFGH"));
            tags = list;
        }

        public int ParseInt(string value)
        {
            if (showNumericsAsHex)
            {
                return int.Parse(value, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            return int.Parse(value);
        }

        public short ParseShort(string value)
        {
            if (showNumericsAsHex)
            {
                return short.Parse(value, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            return short.Parse(value);
        }

        public long ParseLong(String value)
        {
            if (showNumericsAsHex)
            {
                return long.Parse(value, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            return long.Parse(value);
        }

        public UInt64 ParseULong(String value)
        {
            if (showNumericsAsHex)
            {
                return UInt64.Parse(value, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            return UInt64.Parse(value);
        }
    }
}
