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

namespace Gurux.DLMS.Internal
{
    /// <summary>
    /// HDLC frame types.
    /// </summary>
    enum HdlcFrameType : byte
    {
        /// <summary>
        /// I-Frame. Information frames are used to transport user data. 
        /// </summary>
        Iframe = 0x0,
        /// <summary>
        /// S-frame. Supervisory Frames are used for flow and error control. Rejected, RNR and RR.       
        /// </summary>
        Sframe = 0x1,
        /// <summary>
        /// U-frame. Unnumbered frames are used for link management. Example SNRM and UA.
        /// </summary>
        Uframe = 0x3
    }
}
