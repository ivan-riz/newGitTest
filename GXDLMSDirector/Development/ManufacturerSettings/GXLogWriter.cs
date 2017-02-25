//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://mars/Projects/GuruxClub/GXDLMSDirector/Development/ManufacturerSettings/GXLogWriter.cs $
//
// Version:         $Revision: 9233 $,
//                  $Date: 2017-02-20 09:17:34 +0200 (ma, 20 helmi 2017) $
//                  $Author: gurux01 $
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
// More information of Gurux DLMS/COSEM Director: http://www.gurux.org/GXDLMSDirector
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Diagnostics;
using System.Windows.Forms;
using Gurux.Common;

namespace GXDLMS.ManufacturerSettings
{
    class GXLogWriter
    {
        static public string LogPath
        {
            get
            {
                string path = string.Empty;
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    path = System.IO.Path.Combine(path, ".Gurux");
                }
                else
                {
                    //Vista: C:\ProgramData
                    //XP: c:\Program Files\Common Files                
                    //XP = 5.1 & Vista = 6.0
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    }
                    else
                    {
                        path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
                    }
                    path = System.IO.Path.Combine(path, "Gurux");
                }
                path = System.IO.Path.Combine(path, "GXDLMSDirector");
                path = System.IO.Path.Combine(path, "GXDLMSDirector.log");
                return path;
            }
        }

        /// <summary>
        /// Append data to log file.
        /// </summary>
        static public void WriteLog(string data)
        {
            if (data == null)
            {
                return;
            }
            System.Diagnostics.Trace.WriteLine(DateTime.Now.ToLongTimeString() + " " + data.Replace("\r", "<CR>").Replace("\n", "<LF>"));
        }

        static public void WriteLog(string text, byte[] value)
        {
            string str = DateTime.Now.ToLongTimeString() + " " + text;
            if (value != null)
            {
                str += "\r\n" + GXCommon.ToHex(value, true);
            }
            System.Diagnostics.Trace.WriteLine(str);
        }

        /// <summary>
        /// Clear log file.
        /// </summary>
        static public void ClearLog()
        {
            foreach (TraceListener it in System.Diagnostics.Trace.Listeners)
            {
                if (it is TextWriterTraceListener)
                {
                    //Flush and close the output.
                    Trace.Flush();
                    it.Flush();
                    if (((TextWriterTraceListener)it).Writer != null)
                    {
                        ((TextWriterTraceListener)it).Writer.Close();
                    }
                    ((TextWriterTraceListener)it).Writer = new StreamWriter(GXLogWriter.LogPath);
                    GXFileSystemSecurity.UpdateFileSecurity(GXLogWriter.LogPath);
                    break;
                }
            }
            //Get version info
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(asm.Location);
            Debug.WriteLine("GXDLMSDirector " + info.FileVersion);
            Debug.WriteLine("Log created " + DateTime.Now.ToLongTimeString());
        }
    }
}
