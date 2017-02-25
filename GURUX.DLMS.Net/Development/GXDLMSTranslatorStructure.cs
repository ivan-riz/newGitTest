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
using Gurux.DLMS.Internal;

namespace Gurux.DLMS
{
    /// <summary>
    /// This class is used internally in GXDLMSTranslator to save generated xml.
    /// </summary>
    class GXDLMSTranslatorStructure
    {
        public TranslatorOutputType OutputType
        {
            get;
            private set;
        }
        /// <summary>
        /// Amount of spaces.
        /// </summary>
        private int offset;

        /// <summary>
        /// Amount of spaces.
        /// </summary>
        public int Offset
        {
            get
            {
                return offset;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("offset");
                }
                offset = value;
            }
        }
        internal StringBuilder sb = new StringBuilder();
        private SortedList<int, string> tags;

        public String GetDataType(DataType type)
        {
            return tags[GXDLMS.DATA_TYPE_OFFSET + (int)type];
        }

        /**
         * Are numeric values shows as hex.
         */
        private bool showNumericsAsHex;

        public bool ShowStringAsHex
        {
            get;
            set;
        }

        /// <summary>
        /// Are comments added.
        /// </summary>
        public bool Comments
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="list">List of tags.</param>
        public GXDLMSTranslatorStructure(TranslatorOutputType type, bool numericsAshex, bool hex, bool comments, SortedList<int, string> list)
        {
            OutputType = type;
            showNumericsAsHex = numericsAshex;
            ShowStringAsHex = hex;
            Comments = comments;
            tags = list;
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public void AppendLine(string str)
        {
            sb.Append(' ', 2 * offset);
            sb.AppendLine(str);
        }

        public void AppendLine(Enum tag, string name, object value)
        {
            AppendLine(Convert.ToInt32(tag), name, value);
        }

        public void AppendLine(int tag, string name, object value)
        {
            AppendLine(tags[tag], name, value);
        }

        public void AppendLine(string tag, string name, object value)
        {
            sb.Append(' ', 2 * offset);
            sb.Append('<');
            sb.Append(tag);
            if (OutputType == TranslatorOutputType.SimpleXml)
            {
                sb.Append(' ');
                if (name == null)
                {
                    sb.Append("Value");
                }
                else
                {
                    sb.Append(name);
                }
                sb.Append("=\"");
            }
            else
            {
                sb.Append('>');
            }
            if (value is byte)
            {
                sb.Append(IntegerToHex((byte)value, 2));
            }
            else if (value is sbyte)
            {
                sb.Append(IntegerToHex((sbyte)value, 2));
            }
            else if (value is UInt16)
            {
                sb.Append(IntegerToHex((UInt16)value, 4));
            }
            else if (value is Int16)
            {
                sb.Append(IntegerToHex((Int16)value, 4));
            }
            else if (value is UInt32)
            {
                sb.Append(IntegerToHex((UInt32)value, 8));
            }
            else if (value is Int32)
            {
                sb.Append(IntegerToHex((Int32)value, 8));
            }
            else if (value is UInt64)
            {
                sb.Append(IntegerToHex((UInt64)value));
            }
            else if (value is Int64)
            {
                sb.Append(IntegerToHex((Int64)value, 16));
            }
            else
            {
                sb.Append(Convert.ToString(value));
            }
            if (OutputType == TranslatorOutputType.SimpleXml)
            {
                sb.Append("\" />");
            }
            else
            {
                sb.Append("</");
                sb.Append(tag);
                sb.Append('>');
            }
            sb.Append('\r');
            sb.Append('\n');
        }

        /// <summary>
        /// Append comment.
        /// </summary>
        /// <param name="comment">Comment to add.</param>
        public void AppendComment(string comment)
        {
            if (Comments)
            {
                sb.Append(' ', 2 * offset);
                sb.Append("<!--");
                sb.Append(comment);
                sb.Append("-->");
                sb.Append('\r');
                sb.Append('\n');
            }
        }

        public void Append(String value)
        {
            sb.Append(value);
        }

        public void Append(int tag, bool start)
        {
            if (start)
            {
                sb.Append('<');
            }
            else
            {
                sb.Append("</");
            }
            sb.Append(tags[tag]);
            sb.Append('>');
        }

        public void AppendStartTag(Enum tag, string name, string value)
        {
            AppendStartTag(Convert.ToInt32(tag), name, value);
        }

        public void AppendStartTag(int tag, string name, string value)
        {
            sb.Append(' ', 2 * offset);
            sb.Append('<');
            sb.Append(tags[tag]);
            if (OutputType == TranslatorOutputType.SimpleXml && name != null)
            {
                sb.Append(' ');
                sb.Append(name);
                sb.Append("=\"");
                sb.Append(value);
                sb.AppendLine("\" >");
            }
            else
            {
                sb.AppendLine(">");
            }
            ++offset;
        }

        public void AppendStartTag(Enum cmd)
        {
            sb.Append(' ', 2 * offset);
            sb.Append("<");
            sb.Append(tags[Convert.ToInt32(cmd)]);
            sb.AppendLine(">");
            ++offset;
        }

        public void AppendStartTag(Command cmd, Enum type)
        {
            sb.Append(' ', 2 * offset);
            sb.Append("<");
            sb.Append(tags[(int)cmd << 8 | Convert.ToByte(type)]);
            sb.AppendLine(">");
            ++offset;
        }

        public void AppendEndTag(Enum cmd)
        {
            AppendEndTag(Convert.ToInt32(cmd));
        }

        public void AppendEndTag(Command cmd, Enum type)
        {
            AppendEndTag((int)cmd << 8 | Convert.ToByte(type));
        }

        public void AppendEndTag(int tag)
        {
            --Offset;
            sb.Append(' ', 2 * offset);
            sb.Append("</");
            sb.Append(tags[tag]);
            sb.AppendLine(">");
        }

        /// <summary>
        /// Convert integer to string.
        /// </summary>
        /// <param name="value">Conveted value.</param>
        /// <param name="desimals">Desimal count.</param>
        /// <returns>Integer value as a string.</returns>
        public string IntegerToHex(long value, int desimals)
        {
            if (showNumericsAsHex
                    && OutputType == TranslatorOutputType.SimpleXml)
            {
                return value.ToString("X" + desimals.ToString());
            }
            return value.ToString();
        }

        /// <summary>
        /// Convert integer to string.
        /// </summary>
        /// <param name="value">Conveted value.</param>
        /// <param name="desimals">Desimal count.</param>
        /// <returns>Integer value as a string.</returns>
        public string IntegerToHex(UInt64 value)
        {
            if (showNumericsAsHex
                    && OutputType == TranslatorOutputType.SimpleXml)
            {
                return value.ToString("X16");
            }
            return value.ToString();
        }
    }
}
