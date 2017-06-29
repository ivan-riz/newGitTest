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
using Gurux.DLMS.Objects;
using Gurux.DLMS;
using Gurux.Net;
using Gurux.DLMS.Enums;
using Gurux.DLMS.Objects.Enums;
using Gurux.DLMS.Secure;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Globalization;

namespace GuruxDLMSServerExample
{
    /// <summary>
    /// All example servers are using same objects.
    /// </summary>
    class GXDLMSBase : GXDLMSSecureServer
    {
        static string dataFile = "data.csv";
        bool trace = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ln">Logical name settings.</param>
        /// <param name="type">Interface type.</param>
        public GXDLMSBase(GXDLMSAssociationLogicalName ln, InterfaceType type)
        : base(ln, type, "GRX", 12345678)
        {
            MaxReceivePDUSize = 1024;
            //Default secreds.
            ln.Secret = ASCIIEncoding.ASCII.GetBytes("Gurux");
            ln.HlsSecret = ln.Secret;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sn">Short name settings.</param>
        /// <param name="type">Interface type.</param>
        public GXDLMSBase(GXDLMSAssociationShortName sn, InterfaceType type)
        : base(sn, type, "GRX", 12345678)
        {
            MaxReceivePDUSize = 1024;
            //Default secreds.
            sn.Secret = ASCIIEncoding.ASCII.GetBytes("Gurux");
            sn.HlsSecret = sn.Secret;
        }

        Gurux.Common.IGXMedia Media = null;

        public void Initialize(string port)
        {
            Media = new Gurux.Serial.GXSerial(port);
            Init();
        }
        /// <summary>
        /// Generic initialize for all servers.
        /// </summary>
        /// <param name="server"></param>
        public void Initialize(int port)
        {
            Media = new GXNet(NetworkType.Tcp, port);
            Init();
        }

        void Init()
        {
            Media.OnReceived += new Gurux.Common.ReceivedEventHandler(OnReceived);
            Media.OnClientConnected += new Gurux.Common.ClientConnectedEventHandler(OnClientConnected);
            Media.OnClientDisconnected += new Gurux.Common.ClientDisconnectedEventHandler(OnClientDisconnected);
            Media.OnError += new Gurux.Common.ErrorEventHandler(OnError);
            Media.Open();
            ///////////////////////////////////////////////////////////////////////
            //Add Logical Device Name. 123456 is meter serial number.
            GXDLMSData ldn = new GXDLMSData("0.0.42.0.0.255");
            ldn.Value = "Gurux123456";
            //Value is get as Octet String.
            ldn.SetDataType(2, DataType.OctetString);
            ldn.SetUIDataType(2, DataType.String);
            Items.Add(ldn);

            //Add firmware version.
            GXDLMSData fw = new GXDLMSData("1.0.0.2.0.255");
            fw.Value = "Gurux FW 0.0.1";
            Items.Add(fw);

            //Add Last average.
            GXDLMSRegister r = new GXDLMSRegister("1.1.21.25.0.255");
            //Set access right. Client can't change average value.
            Items.Add(r);
            //Add default clock. Clock's Logical Name is 0.0.1.0.0.255.
            GXDLMSClock clock = new GXDLMSClock();
            clock.Begin = new GXDateTime(-1, 9, 1, -1, -1, -1, -1);
            clock.End = new GXDateTime(-1, 3, 1, -1, -1, -1, -1);
            clock.Deviation = 0;
            Items.Add(clock);
            //Add Tcp Udp setup. Default Logical Name is 0.0.25.0.0.255.
            GXDLMSTcpUdpSetup tcp = new GXDLMSTcpUdpSetup();
            Items.Add(tcp);
            ///////////////////////////////////////////////////////////////////////
            //Add Load profile.
            GXDLMSProfileGeneric pg = new GXDLMSProfileGeneric("1.0.99.1.0.255");
            //Set capture period to 60 second.
            pg.CapturePeriod = 60;
            //Maximum row count.
            pg.ProfileEntries = 100000;
            pg.SortMethod = SortMethod.FiFo;
            pg.SortObject = clock;
            //Add columns.
            //Set saved attribute index.
            pg.CaptureObjects.Add(new GXKeyValuePair<GXDLMSObject, GXDLMSCaptureObject>(clock, new GXDLMSCaptureObject(2, 0)));
            //Set saved attribute index.
            pg.CaptureObjects.Add(new GXKeyValuePair<GXDLMSObject, GXDLMSCaptureObject>(r, new GXDLMSCaptureObject(2, 0)));
            Items.Add(pg);
            //Add initial rows.
            //Generate Profile Generic data file
            lock (dataFile)
            {
                using (var writer = File.CreateText(dataFile))
                {
                    //Create 10 000 rows for profile generic file.
                    //In example profile generic we have two columns. 
                    //Date time and integer value.
                    int rowCount = 10000;
                    DateTime dt = DateTime.Now;
                    //Reset minutes and seconds to Zero.
                    dt = dt.AddSeconds(-dt.Second);
                    dt = dt.AddMinutes(-dt.Minute);
                    dt = dt.AddHours(-(rowCount - 1));
                    StringBuilder sb = new StringBuilder();
                    for (int pos = 0; pos != rowCount; ++pos)
                    {
                        sb.Append(dt.ToString(CultureInfo.InvariantCulture));
                        sb.Append(';');
                        sb.AppendLine(Convert.ToString(pos + 1));
                        dt = dt.AddHours(1);
                    }
                    sb.Length -= 2;
                    writer.Write(sb.ToString());
                }
            }
            ///////////////////////////////////////////////////////////////////////
            //Add Auto connect object.
            GXDLMSAutoConnect ac = new GXDLMSAutoConnect();
            ac.Mode = AutoConnectMode.AutoDiallingAllowedAnytime;
            ac.Repetitions = 10;
            ac.RepetitionDelay = 60;
            //Calling is allowed between 1am to 6am.
            ac.CallingWindow.Add(new KeyValuePair<GXDateTime, GXDateTime>(new GXDateTime(-1, -1, -1, 1, 0, 0, -1), new GXDateTime(-1, -1, -1, 6, 0, 0, -1)));
            ac.Destinations = new string[] { "www.gurux.org" };
            Items.Add(ac);
            ///////////////////////////////////////////////////////////////////////
            //Add Activity Calendar object.
            GXDLMSActivityCalendar activity = new GXDLMSActivityCalendar();
            activity.CalendarNameActive = "Active";
            activity.SeasonProfileActive = new GXDLMSSeasonProfile[] { new GXDLMSSeasonProfile("Summer time", new GXDateTime(-1, 3, 31, -1, -1, -1, -1), "") };
            activity.WeekProfileTableActive = new GXDLMSWeekProfile[] { new GXDLMSWeekProfile("Monday", 1, 1, 1, 1, 1, 1, 1) };
            activity.DayProfileTableActive = new GXDLMSDayProfile[] { new GXDLMSDayProfile(1, new GXDLMSDayProfileAction[] { new GXDLMSDayProfileAction(new GXTime(DateTime.Now), "0.1.10.1.101.255", 1) }) };
            activity.CalendarNamePassive = "Passive";
            activity.SeasonProfilePassive = new GXDLMSSeasonProfile[] { new GXDLMSSeasonProfile("Winter time", new GXDateTime(-1, 10, 30, -1, -1, -1, -1), "") };
            activity.WeekProfileTablePassive = new GXDLMSWeekProfile[] { new GXDLMSWeekProfile("Tuesday", 1, 1, 1, 1, 1, 1, 1) };
            activity.DayProfileTablePassive = new GXDLMSDayProfile[] { new GXDLMSDayProfile(1, new GXDLMSDayProfileAction[] { new GXDLMSDayProfileAction(new GXTime(DateTime.Now), "0.1.10.1.101.255", 1) }) };
            activity.Time = new GXDateTime(DateTime.Now);
            Items.Add(activity);
            ///////////////////////////////////////////////////////////////////////
            //Add Optical Port Setup object.
            GXDLMSIECOpticalPortSetup optical = new GXDLMSIECOpticalPortSetup();
            optical.DefaultMode = OpticalProtocolMode.Default;
            optical.ProposedBaudrate = BaudRate.Baudrate9600;
            optical.DefaultBaudrate = BaudRate.Baudrate300;
            optical.ResponseTime = LocalPortResponseTime.ms200;
            optical.DeviceAddress = "Gurux";
            optical.Password1 = "Gurux1";
            optical.Password2 = "Gurux2";
            optical.Password5 = "Gurux5";
            Items.Add(optical);
            ///////////////////////////////////////////////////////////////////////
            //Add Demand Register object.
            GXDLMSDemandRegister dr = new GXDLMSDemandRegister();
            dr.LogicalName = "0.0.1.0.0.255";
            dr.CurrentAverageValue = (uint)10;
            dr.LastAverageValue = (uint)20;
            dr.Status = (byte)1;
            dr.StartTimeCurrent = dr.CaptureTime = new GXDateTime(DateTime.Now);
            dr.Period = 10;
            dr.NumberOfPeriods = 1;
            Items.Add(dr);
            ///////////////////////////////////////////////////////////////////////
            //Add Register Monitor object.
            GXDLMSRegisterMonitor rm = new GXDLMSRegisterMonitor();
            rm.LogicalName = "0.0.1.0.0.255";
            rm.Thresholds = new object[] { (int)0x1234, (int)0x5678 };
            GXDLMSActionSet set = new GXDLMSActionSet();
            set.ActionDown.LogicalName = rm.LogicalName;
            set.ActionDown.ScriptSelector = 1;
            set.ActionUp.LogicalName = rm.LogicalName;
            set.ActionUp.ScriptSelector = 2;
            rm.Actions = new GXDLMSActionSet[] {
            set
            };
            rm.MonitoredValue.Update(r, 2);
            Items.Add(rm);
            ///////////////////////////////////////////////////////////////////////
            //Add Activate test mode Script table object.
            GXDLMSScriptTable st = new GXDLMSScriptTable("0.1.10.1.101.255");
            GXDLMSScript s = new GXDLMSScript();
            s.Id = 1;
            GXDLMSScriptAction a = new GXDLMSScriptAction();
            a.Target = null;

            s.Actions.Add(a);
            st.Scripts.Add(s);
            Items.Add(st);
            ///////////////////////////////////////////////////////////////////////
            //Add action schedule object.
            GXDLMSActionSchedule actionS = new GXDLMSActionSchedule();
            actionS.Target = st;
            actionS.ExecutedScriptSelector = 1;
            actionS.Type = SingleActionScheduleType.SingleActionScheduleType1;
            actionS.ExecutionTime = new GXDateTime[] { new GXDateTime(DateTime.Now) };
            Items.Add(actionS);
            ///////////////////////////////////////////////////////////////////////
            //Add SAP Assignment object.
            GXDLMSSapAssignment sap = new GXDLMSSapAssignment();
            sap.SapAssignmentList.Add(new KeyValuePair<UInt16, string>(1, "Gurux"));
            sap.SapAssignmentList.Add(new KeyValuePair<UInt16, string>(16, "Gurux-2"));
            Items.Add(sap);

            ///////////////////////////////////////////////////////////////////////
            //Add Auto Answer object.
            GXDLMSAutoAnswer aa = new GXDLMSAutoAnswer();
            aa.Mode = AutoAnswerMode.Connected;
            aa.ListeningWindow.Add(new KeyValuePair<GXDateTime, GXDateTime>(new GXDateTime(-1, -1, -1, 6, -1, -1, -1), new GXDateTime(-1, -1, -1, 8, -1, -1, -1)));
            aa.Status = AutoAnswerStatus.Inactive;
            aa.NumberOfCalls = 0;
            aa.NumberOfRingsInListeningWindow = 1;
            aa.NumberOfRingsOutListeningWindow = 2;
            Items.Add(aa);
            ///////////////////////////////////////////////////////////////////////
            //Add Modem Configuration object.
            GXDLMSModemConfiguration mc = new GXDLMSModemConfiguration();
            mc.CommunicationSpeed = BaudRate.Baudrate57600;
            GXDLMSModemInitialisation init = new GXDLMSModemInitialisation();
            init.Request = "AT";
            init.Response = "OK";
            init.Delay = 0;
            mc.InitialisationStrings = new GXDLMSModemInitialisation[] { init };
            Items.Add(mc);

            ///////////////////////////////////////////////////////////////////////
            //Add Mac Address Setup object.
            GXDLMSMacAddressSetup mac = new GXDLMSMacAddressSetup();
            mac.MacAddress = "00:11:22:33:44:55:66";
            Items.Add(mac);

            ///////////////////////////////////////////////////////////////////////
            //Add Image transfer object.
            GXDLMSImageTransfer i = new GXDLMSImageTransfer();
            Items.Add(i);
            ///////////////////////////////////////////////////////////////////////
            //Add IP4 Setup object.
            GXDLMSIp4Setup ip4 = new GXDLMSIp4Setup();
            //Get local IP address.
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip4.IPAddress = ip.ToString();
                }
            }
            Items.Add(ip4);

            //Add Push Setup. (On Connectivity)
            GXDLMSPushSetup push = new GXDLMSPushSetup("0.0.25.9.0.255");
            //Send Push messages to this address as default.
            push.Destination = ip4.IPAddress + ":7000";
            Items.Add(push);
            //Add push object itself. This is needed to tell structure of data to the Push listener.
            push.PushObjectList.Add(new KeyValuePair<GXDLMSObject, GXDLMSCaptureObject>(push, new GXDLMSCaptureObject(2, 0)));
            //Add logical device name.
            push.PushObjectList.Add(new KeyValuePair<GXDLMSObject, GXDLMSCaptureObject>(ldn, new GXDLMSCaptureObject(2, 0)));
            //Add .0.0.25.1.0.255 Ch. 0 IPv4 setup IP address.
            push.PushObjectList.Add(new KeyValuePair<GXDLMSObject, GXDLMSCaptureObject>(ip4, new GXDLMSCaptureObject(3, 0)));

            Items.Add(new GXDLMSSpecialDaysTable());

            //Add  S-FSK objects
            Items.Add(new GXDLMSSFSKPhyMacSetUp());
            Items.Add(new GXDLMSSFSKActiveInitiator());
            Items.Add(new GXDLMSSFSKMacCounters());
            Items.Add(new GXDLMSSFSKMacSynchronizationTimeouts());
            //Add IEC14908 (OSGB) objects.
            Items.Add(new GXDLMSIEC14908Diagnostic());
            Items.Add(new GXDLMSIEC14908Identification());
            Items.Add(new GXDLMSIEC14908PhysicalSetup());
            Items.Add(new GXDLMSIEC14908PhysicalStatus());

            ///Add G3-PLC objects.
            Items.Add(new GXDLMSG3Plc6LoWPan());
            Items.Add(new GXDLMSG3PlcMacLayerCounters());
            Items.Add(new GXDLMSG3PlcMacSetup());
            //Add security setup object
            Items.Add(new GXDLMSSecuritySetup());

            ///////////////////////////////////////////////////////////////////////
            //Server must initialize after all objects are added.
            Initialize();
        }

        public override void Close()
        {
            base.Close();
            Media.Close();
        }

        void OnError(object sender, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

        /// <summary>
        /// Return data using start and end indexes.
        /// </summary>
        /// <param name="p">ProfileGeneric</param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns>Add data Rows</returns>
        void GetProfileGenericDataByEntry(GXDLMSProfileGeneric p, UInt32 index, UInt32 count)
        {
            //Clear old data. It's already serialized.
            p.Buffer.Clear();
            if (count != 0)
            {
                lock (dataFile)
                {
                    using (var fs = File.OpenRead(dataFile))
                    {
                        using (var reader = new StreamReader(fs))
                        {
                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();
                                if (line.Length != 0)
                                {
                                    //Skip row
                                    if (index > 0)
                                    {
                                        --index;
                                    }
                                    else
                                    {
                                        string[] values = line.Split(';');
                                        p.Buffer.Add(new object[] { DateTime.Parse(values[0], CultureInfo.InvariantCulture), int.Parse(values[1]) });
                                    }
                                    if (p.Buffer.Count == count)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find start index and row count using start and end date time.
        /// </summary>
        /// <param name="start">Start time.</param>
        /// <param name="end">End time</param>
        /// <param name="index">Start index.</param>
        /// <param name="count">Item count.</param>
        void GetProfileGenericDataByRange(ValueEventArgs e)
        {
            GXDateTime start = (GXDateTime)GXDLMSClient.ChangeType((byte[])((object[])e.Parameters)[1], DataType.DateTime);
            GXDateTime end = (GXDateTime)GXDLMSClient.ChangeType((byte[])((object[])e.Parameters)[2], DataType.DateTime);
            lock (dataFile)
            {
                using (var fs = File.OpenRead(dataFile))
                {
                    using (var reader = new StreamReader(fs))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (line.Length != 0)
                            {
                                string[] values = line.Split(';');
                                DateTime tm = DateTime.Parse(values[0], CultureInfo.InvariantCulture);
                                if (tm > end)
                                {
                                    //If all data is read.
                                    break;
                                }
                                if (tm < start)
                                {
                                    //If we have not find first item.
                                    ++e.RowBeginIndex;
                                }
                                ++e.RowEndIndex;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get row count.
        /// </summary>
        /// <returns></returns>
        UInt16 GetProfileGenericDataCount()
        {
            UInt16 rows = 0;
            lock (dataFile)
            {
                using (var fs = File.OpenRead(dataFile))
                {
                    using (var reader = new StreamReader(fs))
                    {
                        while (!reader.EndOfStream)
                        {
                            if (reader.ReadLine().Length != 0)
                            {
                                ++rows;
                            }
                        }
                    }
                }
            }
            return rows;
        }

        void HandleClock(ValueEventArgs e)
        {
            GXDLMSClock c = e.Target as GXDLMSClock;
            if (e.Index == 2)
            {
                c.Time = c.Now();
            }
        }

        /// <summary>
        /// Generic read handle for all servers.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="e"></param>
        protected override void PreRead(ValueEventArgs[] args)
        {
            foreach (ValueEventArgs e in args)
            {
                //Framework will handle Association objects automatically.
                if (e.Target is GXDLMSAssociationLogicalName ||
                        e.Target is GXDLMSAssociationShortName)
                {
                    continue;
                }
                //Framework will handle profile generic automatically.
                if (e.Target is GXDLMSProfileGeneric)
                {
                    //If buffer is read and we want to save memory.
                    if (e.Index == 6)
                    {
                        //If client wants to know EntriesInUse.
                        GXDLMSProfileGeneric p = (GXDLMSProfileGeneric)e.Target;
                        p.EntriesInUse = GetProfileGenericDataCount();
                    }
                    if (e.Index == 2)
                    {
                        //Client reads buffer.
                        GXDLMSProfileGeneric p = (GXDLMSProfileGeneric)e.Target;
                        //If reading first time.
                        if (e.RowEndIndex == 0)
                        {
                            if (e.Selector == 0)
                            {
                                e.RowEndIndex = GetProfileGenericDataCount();
                            }
                            else if (e.Selector == 1)
                            {
                                //Read by entry.
                                GetProfileGenericDataByRange(e);
                            }
                            else if (e.Selector == 2)
                            {
                                //Read by range.
                                e.RowBeginIndex = (UInt32)((object[])e.Parameters)[0];
                                e.RowEndIndex = e.RowBeginIndex + (UInt32)((object[])e.Parameters)[1];
                                //If client wants to read more data what we have.
                                UInt16 cnt = GetProfileGenericDataCount();
                                if (e.RowEndIndex - e.RowBeginIndex > cnt - e.RowBeginIndex)
                                {
                                    e.RowEndIndex = cnt - e.RowBeginIndex;
                                    if (e.RowEndIndex < 0)
                                    {
                                        e.RowEndIndex = 0;
                                    }
                                }
                            }
                        }
                        UInt32 count = e.RowEndIndex - e.RowBeginIndex;
                        //Read only rows that can fit to one PDU.
                        if (e.RowToPdu != 0 && e.RowEndIndex - e.RowBeginIndex > e.RowToPdu)
                        {
                            count = e.RowToPdu;
                        }
                        GetProfileGenericDataByEntry(p, e.RowBeginIndex, count);
                    }
                    continue;
                }

                Console.WriteLine(string.Format("Client Read value from {0} attribute: {1}.", e.Target.Name, e.Index));
                if (e.Target is GXDLMSClock)
                {
                    HandleClock(e);
                }
                else if (e.Target.GetUIDataType(e.Index) == DataType.DateTime ||
                         e.Target.GetDataType(e.Index) == DataType.DateTime)
                {
                    e.Value = DateTime.Now;
                    e.Handled = true;
                }

                else if (e.Target is GXDLMSRegisterMonitor && e.Index == 2)
                {
                    //Update Register Monitor Thresholds values.
                    e.Value = new object[] { new Random().Next(0, 1000) };
                    e.Handled = true;
                }
                else
                {
                    //If data is not assigned and value type is unknown return number.
                    object[] values = e.Target.GetValues();
                    if (e.Index <= values.Length)
                    {
                        if (values[e.Index - 1] == null)
                        {
                            DataType tp = e.Target.GetDataType(e.Index);
                            if (tp == DataType.None || tp == DataType.Int8 || tp == DataType.Int16 ||
                                    tp == DataType.Int32 || tp == DataType.Int64 || tp == DataType.UInt8 || tp == DataType.UInt16 ||
                                    tp == DataType.UInt32 || tp == DataType.UInt64)
                            {
                                e.Value = new Random().Next(0, 1000);
                                e.Handled = true;
                            }
                            if (tp == DataType.String)
                            {
                                e.Value = "Gurux";
                                e.Handled = true;
                            }
                        }
                    }
                }
            }
        }

        protected override void PostRead(ValueEventArgs[] args)
        {

        }

        protected override void PreWrite(ValueEventArgs[] args)
        {
            foreach (ValueEventArgs it in args)
            {
                System.Diagnostics.Debug.WriteLine("Writing " + it.Target.LogicalName);
            }
        }

        protected override void PostWrite(ValueEventArgs[] args)
        {
            foreach (ValueEventArgs it in args)
            {
                System.Diagnostics.Debug.WriteLine("Writing " + it.Target.LogicalName);
            }
        }

        protected override void InvalidConnection(GXDLMSConnectionEventArgs e)
        {
        }

        void SendPush(GXDLMSPushSetup target)
        {
            int pos = target.Destination.IndexOf(':');
            if (pos == -1)
            {
                throw new ArgumentException("Invalid destination.");
            }
            GXDLMSNotify notify = new GXDLMSNotify(true, 1, 1, InterfaceType.WRAPPER);
            byte[][] data = notify.GeneratePushSetupMessages(DateTime.MinValue, target);
            string host = target.Destination.Substring(0, pos);
            int port = int.Parse(target.Destination.Substring(pos + 1));
            GXNet net = new GXNet(NetworkType.Tcp, host, port);
            try
            {
                net.Open();
                foreach (byte[] it in data)
                {
                    net.Send(it, null);
                }
            }
            finally
            {
                net.Close();
            }
        }

        protected override void PreAction(ValueEventArgs[] args)
        {
            foreach (ValueEventArgs it in args)
            {
                System.Diagnostics.Debug.WriteLine("Action " + it.Target.LogicalName);
                if (it.Target is GXDLMSPushSetup && it.Index == 1)
                {
                    SendPush(it.Target as GXDLMSPushSetup);
                    it.Handled = true;
                }
            }
        }

        private void HandleProfileGenericActions(ValueEventArgs it)
        {
            GXDLMSProfileGeneric pg = (GXDLMSProfileGeneric)it.Target;
            if (it.Index == 1)
            {
                //Profile generic clear is called. Clear data.
                using (var fs = File.CreateText(dataFile))
                {
                }
            }
            else if (it.Index == 2)
            {
                //Profile generic Capture is called.
                using (var writer = File.AppendText(dataFile))
                {
                    StringBuilder sb = new StringBuilder();
                    for (int pos = pg.Buffer.Count - 1; pos != pg.Buffer.Count; ++pos)
                    {
                        for (int c = 0; c != pg.CaptureObjects.Count; ++c)
                        {
                            if (c != 0)
                            {
                                sb.Append(';');
                            }
                            object col = pg.Buffer[pos][c];
                            if (col is DateTime)
                            {
                                sb.Append(((DateTime)col).ToString(CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                sb.Append(Convert.ToString(col));
                            }
                        }
                        sb.AppendLine("");
                    }
                    writer.Write(sb.ToString());
                }
            }
        }

        protected override void PostAction(ValueEventArgs[] args)
        {
            foreach (ValueEventArgs it in args)
            {
                if (it.Target is GXDLMSProfileGeneric)
                {
                    lock (dataFile)
                    {
                        HandleProfileGenericActions(it);
                    }
                }
            }
        }

        /// <summary>
        /// Our example server accept all connections.
        /// </summary>
        protected override bool IsTarget(int serverAddress, int clientAddress)
        {
            return true;
        }

        protected override AccessMode GetAttributeAccess(ValueEventArgs arg)
        {
            //Only read is allowed for register.
            if (arg.Target is GXDLMSRegister)
            {
                return AccessMode.Read;
            }
            //Only read is allowed
            if (arg.Settings.Authentication == Authentication.None)
            {
                return AccessMode.Read;
            }
            //Only clock write is allowed.
            if (arg.Settings.Authentication == Authentication.Low)
            {
                if (arg.Target is GXDLMSClock)
                {
                    return AccessMode.ReadWrite;
                }
                return AccessMode.Read;
            }
            //All write are allowed.
            return AccessMode.ReadWrite;
        }

        /// <summary>
        /// Get method access mode.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns>Method access mode</returns>
        protected override MethodAccessMode GetMethodAccess(ValueEventArgs arg)
        {
            //Methods are not allowed.
            if (arg.Settings.Authentication == Authentication.None)
            {
                return MethodAccessMode.NoAccess;
            }
            //Only clock methods are allowed.
            if (arg.Settings.Authentication == Authentication.Low)
            {
                if (arg.Target is GXDLMSClock)
                {
                    return MethodAccessMode.Access;
                }
                return MethodAccessMode.NoAccess;
            }
            return MethodAccessMode.Access;
        }


        /// <summary>
        /// Our example server accept all authentications.
        /// </summary>
        protected override SourceDiagnostic ValidateAuthentication(Authentication authentication, byte[] password)
        {
            //If low authentication fails.
            if (authentication == Authentication.Low)
            {
                byte[] expected;
                if (UseLogicalNameReferencing)
                {
                    GXDLMSAssociationLogicalName ln = (GXDLMSAssociationLogicalName)Items.FindByLN(ObjectType.AssociationLogicalName, "0.0.40.0.0.255");
                    expected = ln.Secret;
                }
                else
                {
                    GXDLMSAssociationShortName sn = (GXDLMSAssociationShortName)Items.FindByLN(ObjectType.AssociationShortName, "0.0.40.0.0.255");
                    expected = sn.Secret;
                }
                if (Gurux.Common.GXCommon.EqualBytes(expected, password))
                {
                    return SourceDiagnostic.None;
                }
                return SourceDiagnostic.AuthenticationFailure;
            }
            //Other authentication levels are check later.
            return SourceDiagnostic.None;
        }

        /// <summary>
        /// All objects are static in our example.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="sn"></param>
        /// <param name="ln"></param>
        /// <returns></returns>
        protected override GXDLMSObject FindObject(ObjectType objectType, int sn, string ln)
        {
            return null;
        }


        /// <summary>
        /// Client has close connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnClientDisconnected(object sender, Gurux.Common.ConnectionEventArgs e)
        {
            //Reset server settings when connection closed.
            this.Reset();
            Console.WriteLine("Client Disconnected.");
        }

        /// <summary>
        /// Client has made connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnClientConnected(object sender, Gurux.Common.ConnectionEventArgs e)
        {
            Console.WriteLine("Client Connected.");
        }

        /// <summary>
        /// Client has send data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnReceived(object sender, Gurux.Common.ReceiveEventArgs e)
        {
            try
            {
                lock (this)
                {
                    if (trace)
                    {
                        Console.WriteLine("<- " + Gurux.Common.GXCommon.ToHex((byte[])e.Data, true));
                    }
                    byte[] reply = HandleRequest((byte[])e.Data);
                    //Reply is null if we do not want to send any data to the client.
                    //This is done if client try to make connection with wrong device ID.
                    if (reply != null)
                    {
                        if (trace)
                        {
                            Console.WriteLine("-> " + Gurux.Common.GXCommon.ToHex(reply, true));
                        }
                        Media.Send(reply, e.SenderInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Generate random value for profile generic.
        /// </summary>
        /// <param name="args"></param>
        public override void PreGet(ValueEventArgs[] args)
        {
            foreach (ValueEventArgs it in args)
            {
                if (it.Target is GXDLMSProfileGeneric)
                {
                    GXDLMSProfileGeneric pg = (GXDLMSProfileGeneric)it.Target;
                    pg.Buffer.Clear();
                    int cnt = GetProfileGenericDataCount() + 1;
                    //Update last average value.
                    pg.Buffer.Add(new object[] { DateTime.Now, cnt });
                    it.Handled = true;
                }
            }
        }

        public override void PostGet(ValueEventArgs[] args)
        {
        }

        protected override void Connected(GXDLMSConnectionEventArgs e)
        {
            Console.WriteLine("Connected.");
        }

        protected override void Disconnected(GXDLMSConnectionEventArgs e)
        {
            Console.WriteLine("Disconnected");
        }
    }
}
