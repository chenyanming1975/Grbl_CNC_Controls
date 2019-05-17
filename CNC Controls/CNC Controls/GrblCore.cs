﻿/*
 * GrblCore.cs - part of CNC Controls library
 *
 * v0.01 / 2019-05-15 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2018-2019, Io Engineering (Terje Io)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

· Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

· Redistributions in binary form must reproduce the above copyright notice, this
list of conditions and the following disclaimer in the documentation and/or
other materials provided with the distribution.

· Neither the name of the copyright holder nor the names of its contributors may
be used to endorse or promote products derived from this software without
specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Data;
using System.Diagnostics;

namespace CNC_Controls
{
    public delegate void GCodePushHandler(String gcode, GCode.Action action);

    public class GrblConstants
    {
        public const byte
            CMD_EXIT = 0x03, // ctrl-C
            CMD_RESET = 0x18, // ctrl-X
            CMD_STOP = 0x19, // ctrl-Y
            CMD_STATUS_REPORT = 0x80,
            CMD_CYCLE_START = 0x81,
            CMD_FEED_HOLD = 0x82,
            CMD_GCODE_REPORT = 0x83,
            CMD_SAFETY_DOOR = 0x84,
            CMD_JOG_CANCEL = 0x85,
            CMD_STATUS_REPORT_ALL = 0x87,
            CMD_FEED_OVR_RESET = 0x90,
            CMD_FEED_OVR_COARSE_PLUS = 0x91,
            CMD_FEED_OVR_COARSE_MINUS = 0x92,
            CMD_FEED_OVR_FINE_PLUS = 0x93,
            CMD_FEED_OVR_FINE_MINUS = 0x94,
            CMD_RAPID_OVR_RESET = 0x95,
            CMD_RAPID_OVR_MEDIUM = 0x96,
            CMD_RAPID_OVR_LOW = 0x97,
            CMD_SPINDLE_OVR_RESET = 0x99,
            CMD_SPINDLE_OVR_COARSE_PLUS = 0x9A,
            CMD_SPINDLE_OVR_COARSE_MINUS = 0x9B,
            CMD_SPINDLE_OVR_FINE_PLUS = 0x9C,
            CMD_SPINDLE_OVR_FINE_MINUS = 0x9D,
            CMD_SPINDLE_OVR_STOP = 0x9E,
            CMD_COOLANT_FLOOD_OVR_TOGGLE = 0xA0,
            CMD_COOLANT_MIST_OVR_TOGGLE = 0xA1,
            CMD_PID_REPORT = 0xA2,
            CMD_TOOL_ACK = 0xA3;

        public const string
            CMD_STATUS_REPORT_LEGACY = "?",
            CMD_CYCLE_START_LEGACY = "~",
            CMD_FEED_HOLD_LEGACY = "!",
            CMD_UNLOCK = "$X",
            CMD_HOMING = "$H",
            CMD_CHECK = "$C",
            CMD_GETSETTINGS = "$$",
            CMD_GETPARSERSTATE = "$G",
            CMD_GETINFO = "$I",
            CMD_GETNGCPARAMETERS = "$#",
            CMD_PROGRAM_DEMARCATION = "%",
            CMD_SDCARD_MOUNT = "$FM",
            CMD_SDCARD_DIR = "$F",
            CMD_SDCARD_RUN = "$F=",
            FORMAT_METRIC = "###0.0##",
            FORMAT_IMPERIAL = "##0.0###";

        public const int
            X_AXIS = 0,
            Y_AXIS = 1,
            Z_AXIS = 2,
            A_AXIS = 3,
            B_AXIS = 4,
            C_AXIS = 5;
    }

    public enum GrblStates
    {
        Unknown = 0,
        Idle,
        Run,
        Tool,
        Hold,
        Home,
        Check,
        Jog,
        Alarm,
        Door
    }

    public enum GrblSetting
    {
        PulseMicroseconds = 0,
        StepperIdleLockTime = 1,
        StepInvertMask = 2,
        DirInvertMask = 3,
        InvertStepperEnable = 4,
        LimitPinsInvertMask = 5,
        InvertProbePin = 6,
        StatusReportMask = 10,
        JunctionDeviation = 11,
        ArcTolerance = 12,
        ReportInches = 13,
        ControlInvertMask = 14,
        CoolantInvertMask = 15,
        SpindleInvertMask = 16,
        ControlPullUpDisableMask = 17,
        LimitPullUpDisableMask = 18,
        ProbePullUpDisable = 19,
        SoftLimitsEnable = 20,
        HardLimitsEnable = 21,
        HomingEnable = 22,
        HomingDirMask = 23,
        HomingFeedRate = 24,
        HomingSeekRate = 25,
        HomingDebounceDelay = 26,
        HomingPulloff = 27,
        G73Retract = 28,
        PulseDelayMicroseconds = 29,
        RpmMax = 30,
        RpmMin = 31,
        LaserMode = 32,
        PWMFreq = 33,
        PWMOffValue = 34,
        PWMMinValue = 35,
        PWMMaxValue = 36,
        StepperDeenergizeMask = 37,
        SpindlePPR  = 38,
        EnableLegacyRTCommands = 39,
        HomingLocateCycles = 43,
        HomingCycle_1  = 44,
        HomingCycle_2  = 45,
        HomingCycle_3  = 46,
        HomingCycle_4  = 47,
        HomingCycle_5  = 48,
        HomingCycle_6  = 49,
        JogStepSpeed = 50,
        JogSlowSpeed = 51,
        JogFastSpeed = 52,
        JogStepDistance = 53,
        JogSlowDistance = 54,
        JogFastDistance = 55,
        AxisSetting_XMaxRate = 110,
        AxisSetting_XAcceleration = 120,
        AxisSetting_YMaxRate = 111,
        AxisSetting_YAcceleration = 121,
        AxisSetting_ZMaxRate = 112,
        AxisSetting_ZAcceleration = 122,
    }

    public enum StreamingState
    {
        NoFile = 0,
        Idle,
        Send,
        SendMDI,
        Home,
        Halted,
        FeedHold,
        ToolChange,
        Stop,
        Reset,
        AwaitResetAck,
        Jogging,
        Disabled,
        Error
    }

    public enum SpindleState
    {
        Off,
        CW,
        CCW
    }

    public enum ThreadTaper
    {
        None,
        Entry,
        Exit,
        Both
    }

    public enum LatheMode
    {
        Diameter = 1, // Do not change
        Radius = 2    // Do not change
    }

    public struct GrblState
    {
        public GrblStates State;
        public int Substate;
        public System.Drawing.Color Color;
    }

    public class GrblLanguage
    {
        public static string language;

        static GrblLanguage()
        {
            GrblLanguage.language = "en_US";
        }
    }

    public class GrblStatusParameters
    {
        public string MPos {get; private set; }
        public string WPos { get; private set; }
        public string WCO { get; private set; }
        public string A { get; private set; }
        public string FS { get; private set; }
        public string MPG { get; private set; }
        public string Ov { get; private set; }
        public string Pn { get; private set; }
        public string Sc { get; private set; }
        public string SD { get; private set; }
        public string Ex { get; private set; }
        public string H { get; private set; }
        public string D { get; private set; }
        public string T { get; private set; }

        public GrblStatusParameters()
        {
            Clear();
        }

        public void Clear()
        {
            MPos = WPos = WCO = A = FS = MPG = Ov = Sc = Pn = SD = Ex = H = D = T = "";
        }

        public bool Set(string parameter, string value)
        {
            bool changed = false;

            switch (parameter)
            {
                case "MPos":
                    if ((changed = this.MPos != value))
                        this.MPos = value;
                    break;

                case "WPos":
                    if ((changed = this.WPos != value))
                        this.WPos = value;
                    break;

                case "A":
                    if ((changed = this.A != value))
                        this.A = value;
                    break;

                case "WCO":
                    if ((changed = this.WCO != value))
                        this.WCO = value;
                    break;

                case "FS":
                    if ((changed = this.FS != value))
                        this.FS = value;
                    break;

                case "Pn":
                    if ((changed = this.Pn != value))
                        this.Pn = value;
                    break;

                case "Ov":
                    if ((changed = this.Ov!= value))
                        this.Ov = value;
                    break;

                case "Sc":
                    if ((changed = this.Sc != value))
                        this.Sc = value;
                    break;

                case "Ex":
                    if ((changed = this.Ex != value))
                        this.Ex = value;
                    break;

                case "SD":
                    if ((changed = this.SD != value))
                        this.SD = value;
                    break;

                case "T":
                    if ((changed = this.T != value))
                    {
                        this.T = value;
                        GrblInfo.Tool = int.Parse(value);
                    }
                    break;

                case "MPG":
                    this.MPG = value;
                    GrblInfo.MPGMode = value == "1";
                    changed = true;
                    break;

                case "H":
                    this.H = value;
                    changed = true;
                    break;

                case "D":
                    this.D = value;
                    GrblInfo.LatheXMode = value == "0" ? LatheMode.Radius : LatheMode.Diameter;
                    changed = true;
                    break;
            }

            return changed;
        }
    }

    public class CoordinateSystem
    {
        private double[] pos = new double[6];
        public int id = 0;

        public CoordinateSystem(string code, string data)
        {
            for (int i = 0; i < pos.Length; i++)
                pos[i] = 0.0;

            this.code = code;
            double[] values = Parse.Decimals(data); 

            for (int i = 0; i < values.Length; i++)
                pos[i] = values[i];

            if(code.StartsWith("G5"))
            {
                double id = Math.Round(double.Parse(code.Substring(2), CultureInfo.InvariantCulture) - 3.0d, 1);

                this.id = (int)Math.Floor(id) + (int)Math.Round((id - Math.Floor(id)) * 10.0d, 0); 
            }

        }

        #region Attributes
        public string code {get; set; }
        public double x { get { return pos[0]; } set { pos[0] = value; } }
        public double y { get { return pos[1]; } set { pos[1] = value; } }
        public double z { get { return pos[2]; } set { pos[2] = value; } }
        public double a { get { return pos[3]; } set { pos[3] = value; } }
        public double b { get { return pos[4]; } set { pos[4] = value; } }
        public double c { get { return pos[5]; } set { pos[5] = value; } }
        #endregion
    }

    public class Tool
    {
        public string code {get; set;}

        public double R { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        
        public string r {
            get { return R.ToString(CultureInfo.InvariantCulture); }
            set { R = GetValue(value); }
        }

        public string x
        {
            get { return X.ToString(CultureInfo.InvariantCulture); }
            set { X = GetValue(value); }
        }

        public string y
        {
            get { return Y.ToString(CultureInfo.InvariantCulture); }
            set { Y = GetValue(value); }
        }

        public string z
        {
            get { return Z.ToString(CultureInfo.InvariantCulture); }
            set { Z = GetValue(value); }
        }

        private double GetValue(string value)
        {
            double v = 0.0d;
            double.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out v);
            return v;
        }
    }

    public static class GrblInfo
    {
        #region Attributes
        public static string AxisLetters { get; private set; }
        public static string Version { get; private set; }
        public static string Options { get; private set; }
        public static int SerialBufferSize { get; private set; }
        public static int NumAxes { get; private set; }
        public static int Tool { get; set; }
        public static bool HasATC { get; private set; }
        public static bool HasSDCard { get; private set; }
        public static bool HasPIDLog { get; private set; }
        public static bool MPGMode { get; set; }
        public static bool LatheMode { get; set; }
        public static LatheMode LatheXMode { get; set; }
        public static bool UseLegacyRTCommands { get; set; }
        public static string IniName { get; set; }
        #endregion

        static GrblInfo()
        {
            GrblInfo.IniName = "App.config";
            GrblInfo.AxisLetters = "XYZABC";
            GrblInfo.Version = "";
            GrblInfo.Options = "";
            GrblInfo.SerialBufferSize = 128;
            GrblInfo.NumAxes = 3;
            GrblInfo.Tool = 0;
            GrblInfo.HasATC = false;
            GrblInfo.HasSDCard = false;
            GrblInfo.HasPIDLog = false;
            GrblInfo.LatheMode = false;
            GrblInfo.MPGMode = false;
            GrblInfo.LatheXMode = CNC_Controls.LatheMode.Radius;
            GrblInfo.UseLegacyRTCommands = true;
        }

        public static void Get()
        {
            GrblInfo.NumAxes = 3;
            GrblInfo.SerialBufferSize = 128;
            GrblInfo.HasATC = false;

            Comms.com.DataReceived += new DataReceivedHandler(GrblInfo.Process);

            Comms.com.PurgeQueue();
            Comms.com.WriteCommand(GrblConstants.CMD_GETINFO);

            while (Comms.com.CommandState == Comms.State.DataReceived || Comms.com.CommandState == Comms.State.AwaitAck)
                Application.DoEvents();

            Comms.com.DataReceived -= GrblInfo.Process;
        }

        private static void Process(string data)
        {
            if(data.StartsWith("[")) {

                string[] valuepair = data.Substring(1).TrimEnd(']').Split(':');

                switch(valuepair[0]) {

                    case "VER":
                        Version = valuepair[1];
                        break;

                    case "OPT":
                        GrblInfo.Options = valuepair[1];
                        string[] s = GrblInfo.Options.Split(',');
                        if (s.Length > 2)
                            GrblInfo.SerialBufferSize = int.Parse(s[2], CultureInfo.InvariantCulture);
                        if (s.Length > 3)
                            GrblInfo.NumAxes = int.Parse(s[3], CultureInfo.InvariantCulture);
                        break;

                    case "NEWOPT":
                        string[] s2 = valuepair[1].Split(',');
                        foreach (string value in s2)
                        {
                            if (value.StartsWith("ATC"))
                                GrblInfo.HasATC = true;
                            else switch (value)
                            {
                                case "ETH":
                                    break;

                                case "SD":
                                    GrblInfo.HasSDCard = true;
                                    break;

                                case "PID":
                                    GrblInfo.HasPIDLog = true;
                                    break;

                                case "LATHE":
                                    GrblInfo.LatheMode = true;
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }


    public static class GrblParserState
    {
        private static Dictionary<string, string> state = new Dictionary<string, string>();

        public static void Get()
        {
            Comms.com.DataReceived += new DataReceivedHandler(GrblParserState.Process);

            Comms.com.PurgeQueue();
            Comms.com.WriteCommand(GrblConstants.CMD_GETPARSERSTATE);

            while (Comms.com.CommandState == Comms.State.DataReceived || Comms.com.CommandState == Comms.State.AwaitAck)
                Application.DoEvents();

            Comms.com.DataReceived -= GrblParserState.Process;

            GrblInfo.LatheXMode = GrblParserState.IsActive("G7") == null ? LatheMode.Radius : LatheMode.Diameter;
        }

        public static void Process(string data)
        {
            if (data.StartsWith("[GC:"))
            {
                GrblParserState.state.Clear();
                string[] s = data.Substring(4).Split(' ');
                foreach (string val in s)
                {
                    if(val.StartsWith("G51"))
                        GrblParserState.state.Add(val.Substring(0, 3), val.Substring(4));
                    else if ("FST".Contains(val.Substring(0, 1)))
                    {
                        GrblParserState.state.Add(val.Substring(0, 1), val.Substring(1));
                        if (val.Substring(0, 1) == "T")
                            GrblInfo.Tool = int.Parse(val.Substring(1));
                    }
                    else
                        GrblParserState.state.Add(val, "");
                } 
            }
        }

        public static string IsActive(string key) // returns null if not active, "" or parsed value if not
        {
            string value = null;

            GrblParserState.state.TryGetValue(key, out value);

            return value;
        }
    }

    public class GrblWorkParameters
    {
        public static CoordinateSystem toolLengtOffset = null;
        public static CoordinateSystem probePosition = null;
        public static List<CoordinateSystem> offset = new List<CoordinateSystem>();
        public static List<Tool> tool = new List<Tool>();

        public static void Load()
        {
            offset.Clear();
            tool.Clear();

            Comms.com.PurgeQueue();
            Comms.com.DataReceived += new DataReceivedHandler(process);
            Comms.com.WriteCommand(GrblConstants.CMD_GETNGCPARAMETERS);

            while (Comms.com.CommandState == Comms.State.DataReceived || Comms.com.CommandState == Comms.State.AwaitAck)
                Application.DoEvents();

            Comms.com.DataReceived -= process;
        }

        private static string extractValues(string data, out string parameters)
        {
            int sep = data.IndexOf(":");
            parameters = data.Substring(sep + 1, data.IndexOf("]") - sep - 1);
            return data.Substring(1, sep - 1);
        }

        private static void AddTool(string gCode, string data)
        {
            Tool tool = new Tool();
            string[] s1 = data.Split('|');
            string[] s2 = s1[1].Split(',');
            tool.code = s1[0];
            tool.x = s2[0];
            tool.y = s2[1];
            tool.z = s2[2];
            if (s1.Length > 2)
            {
                s2 = s1[2].Split(',');
                tool.r = s2[0];
            }
            GrblWorkParameters.tool.Add(tool);
        }

        private static void process(string data)
        {
            if (data.StartsWith("["))
            {
                string parameters, gCode = extractValues(data, out parameters);
                switch (gCode)
                {
                    case "G28":
                    case "G30":
                    case "G54":
                    case "G55":
                    case "G56":
                    case "G57":
                    case "G58":
                    case "G59":
                    case "G59.1":
                    case "G59.2":
                    case "G59.3":
                    case "G92":
                        offset.Add(new CoordinateSystem(gCode, parameters));
                        break;

                    case "T":
                        AddTool(gCode, parameters);
                        break;

                    case "TLO":
                        toolLengtOffset = new CoordinateSystem(gCode, parameters);
                        break;

                    case "PRB":
                        probePosition = new CoordinateSystem(gCode, parameters.Substring(0, parameters.IndexOf(":") - 1));
                        break;
                }
            }
        }
    }

    public class GrblErrors
    {
        private static Dictionary<string, string> messages = null;

        static GrblErrors()
        {
            try
            {
                StreamReader file = new StreamReader(string.Format("{0}\\error_codes_{1}.csv", Application.StartupPath, GrblLanguage.language));

                if (file != null)
                {
                    messages = new Dictionary<string, string>();

                    string line = file.ReadLine();

                    line = file.ReadLine(); // Skip header  

                    while (line != null)
                    {
                        string[] columns = line.Split(',');

                        if (columns.Length == 3)
                            messages.Add(columns[0], columns[1] + ": " + columns[2]);

                        line = file.ReadLine();
                    }
                }

                file.Close();
            }
            catch
            {
            }
        }

        public static string GetMessage(string key)
        {
            string message = null;

            if (messages != null)
                messages.TryGetValue(key, out message);

            return message == null ? string.Format("Error {0}", key) : message;
        }
    }

    public class GrblAlarms
    {
        private static Dictionary<string, string> messages = null;

        static GrblAlarms()
        {
            try
            {
                StreamReader file = new StreamReader(string.Format("{0}\\alarm_codes_{1}.csv", Application.StartupPath, GrblLanguage.language));

                if (file != null)
                {
                    messages = new Dictionary<string, string>();

                    string line = file.ReadLine();

                    line = file.ReadLine(); // Skip header  

                    while (line != null)
                    {
                        string[] columns = line.Split(',');

                        if (columns.Length == 3)
                            messages.Add(columns[0], columns[1] + ": " + columns[2]);

                        line = file.ReadLine();
                    }
                }

                file.Close();
            }
            catch
            {
            }
        }

        public static string GetMessage(string key)
        {
            string message = "";

            if (messages != null)
                messages.TryGetValue(key, out message);

            return message == "" ? string.Format("Alarm {0}", key) : message;
        }
    }


    public static class GrblSettings
    {
        public static DataTable data;

        static GrblSettings()
        {
            GrblSettings.data = new DataTable("Setting");

            GrblSettings.data.Columns.Add("Id", typeof(int));
            GrblSettings.data.Columns.Add("Name", typeof(string));
            GrblSettings.data.Columns.Add("Value", typeof(string));
            GrblSettings.data.Columns.Add("Unit", typeof(string));
            GrblSettings.data.Columns.Add("Description", typeof(string));
            GrblSettings.data.Columns.Add("DataType", typeof(string));
            GrblSettings.data.Columns.Add("DataFormat", typeof(string));
            GrblSettings.data.Columns.Add("Min", typeof(double));
            GrblSettings.data.Columns.Add("Max", typeof(double));
            GrblSettings.data.PrimaryKey = new DataColumn[] { GrblSettings.data.Columns["Id"] };
        }

        public static bool Loaded { get { return GrblSettings.data.Rows.Count > 0; } }

        public static double parseDouble(string value)
        {
            double result = double.NaN;

            if (value != null)
            {
                value = value.Trim();

                if (value.Length == 0 || !double.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result))
                    result = double.NaN;
            }

            return result;
        }

        public static string GetString(GrblSetting key)
        {
            DataRow[] rows = GrblSettings.data.Select("Id = " + ((int)key).ToString());

            return rows.Count() == 1 ? (string)rows[0]["Value"] : null;
        }

        public static double GetDouble(GrblSetting key)
        {
            return GrblSettings.parseDouble(GrblSettings.GetString(key));
        }

        public static void Load()
        {
            GrblSettings.data.Clear();

            Comms.com.DataReceived += new DataReceivedHandler(GrblSettings.Process);

            Comms.com.PurgeQueue();
            Comms.com.WriteCommand(GrblConstants.CMD_GETSETTINGS);

            while (Comms.com.CommandState == Comms.State.DataReceived || Comms.com.CommandState == Comms.State.AwaitAck)
                Application.DoEvents();

            Comms.com.DataReceived -= GrblSettings.Process;

            try
            {
                StreamReader file = new StreamReader(string.Format("{0}\\setting_codes_{1}.txt", Application.StartupPath, GrblLanguage.language));

                if (file != null)
                {
                    string line = file.ReadLine();

                    line = file.ReadLine(); // Skip header  

                    while (line != null)
                    {
                        string[] columns = line.Split('\t');

                        if (columns.Length >= 6)
                        {
                            DataRow[] rows = GrblSettings.data.Select("Id=" + columns[0]);
                            if (rows.Count() == 1)
                            {
                                rows[0]["Name"] = columns[1];
                                rows[0]["Unit"] = columns[2];
                                rows[0]["DataType"] = columns[3];
                                rows[0]["DataFormat"] = columns[4];
                                rows[0]["Description"] = columns[5];
                                if (columns.Length >= 7)
                                    rows[0]["Min"] = parseDouble(columns[6]);
                                if (columns.Length >= 8)
                                    rows[0]["Max"] = parseDouble(columns[7]);
                                if ((string)rows[0]["DataType"] == "float")
                                    rows[0]["Value"] = GrblSettings.FormatFloat((string)rows[0]["Value"], (string)rows[0]["DataFormat"]);
                            }
                        }
                        line = file.ReadLine();
                    }
                }
                file.Close();
            }
            catch
            {
            }
            GrblSettings.data.AcceptChanges();
            double legacy = GetDouble(GrblSetting.EnableLegacyRTCommands);
            if(!double.IsNaN(legacy))
                GrblInfo.UseLegacyRTCommands = legacy == 1.0d;
        }

        public static void Save()
        {
            DataTable Settings = GrblSettings.data.GetChanges();
            if (Settings != null)
            {
                foreach (DataRow Setting in Settings.Rows)
                {
                    Comms.com.WriteCommand(string.Format("${0}={1}", (int)Setting["Id"], (string)Setting["Value"]));
                    while (Comms.com.CommandState == Comms.State.AwaitAck)
                        Application.DoEvents();
                }
                GrblSettings.data.AcceptChanges();
            }
        }

        public static void Backup(string filename)
        {
            if (GrblSettings.data != null) try
                {
                    StreamWriter file = new StreamWriter(filename);
                    if (file != null)
                    {
                        file.WriteLine('%');
                        foreach (DataRow Setting in GrblSettings.data.Rows)
                        {
                            file.WriteLine(string.Format("${0}={1}", (int)Setting["Id"], (string)Setting["Value"]));
                        }
                        file.WriteLine('%');
                        file.Close();
                    }
                }
                catch
                {
                }
        }

        public static string FormatFloat(string value, string format)
        {
            float fval;
            if (float.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out fval))
                value = fval.ToString(format, CultureInfo.InvariantCulture);
            return value;
        }

        private static void Process(string data)
        {
            if (data != "ok")
            {
                string[] valuepair = data.Split('=');
                if (valuepair.Length == 2 && valuepair[1] != "")
                    GrblSettings.data.Rows.Add(new object[] { valuepair[0].Substring(1), "", valuepair[1], "", "", "", "", double.NaN, double.NaN });
            }
        }

    }

    public class PollGrbl
    {
        System.Timers.Timer pollTimer = null;

        public void run()
        {
            this.pollTimer = new System.Timers.Timer();
            this.pollTimer.Elapsed += new System.Timers.ElapsedEventHandler(pollTimer_Elapsed);
            //  this.pollTimer.SynchronizingObject = this;
        }

        public void SetState(int PollInterval)
        {
            if (PollInterval != 0)
            {
                this.pollTimer.Interval = PollInterval;
                this.pollTimer.Start();
            }
            else
                this.pollTimer.Stop();
        }

        void pollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Comms.com.WriteByte(GrblLegacy.ConvertRTCommand(GrblConstants.CMD_STATUS_REPORT));
        }
    }

    public static class GrblLegacy
    {
        public static byte ConvertRTCommand(byte cmd)
        {
            if (GrblInfo.UseLegacyRTCommands) switch (cmd)
            {
                case GrblConstants.CMD_STATUS_REPORT:
                    cmd = (byte)GrblConstants.CMD_STATUS_REPORT_LEGACY[0];
                    break;

                case GrblConstants.CMD_CYCLE_START:
                    cmd = (byte)GrblConstants.CMD_CYCLE_START_LEGACY[0];
                    break;

                case GrblConstants.CMD_FEED_HOLD:
                    cmd = (byte)GrblConstants.CMD_FEED_HOLD_LEGACY[0];
                    break;
            }

            return cmd;
        }
    }

    public static class JobTimer
    {
        private static bool paused = false;
        private static Stopwatch stopWatch = null;

        static JobTimer()
        {
            JobTimer.stopWatch = new Stopwatch();
        }

        public static bool IsRunning { get { return JobTimer.stopWatch.IsRunning || JobTimer.paused; } }

        public static bool IsPaused { get { return JobTimer.paused; } }

        public static bool Pause {
            get {
                return JobTimer.paused;
            }
            set
            {
                if (value) {
                    if (JobTimer.stopWatch.IsRunning)
                    {
                        JobTimer.stopWatch.Stop();
                        JobTimer.paused = true;
                    }
                }
                else if (JobTimer.paused)
                {
                    JobTimer.stopWatch.Start();
                    JobTimer.paused = false;
                }
            }
        }

        public static string RunTime
        {
            get
            {
                return String.Format("{0:00}:{1:00}:{2:00}",
                                       JobTimer.stopWatch.Elapsed.Hours, JobTimer.stopWatch.Elapsed.Minutes, JobTimer.stopWatch.Elapsed.Seconds);
            }
        }

        public static void Start()
        {
            JobTimer.paused = false;
            JobTimer.stopWatch.Reset();
            JobTimer.stopWatch.Start();
        }

        public static void Stop()
        {
            JobTimer.paused = false;
            JobTimer.stopWatch.Stop();
        }
    }

    public static class Parse
    {
        public static double[] Decimals(string s)
        {
            string[] v = s.Split(',');
            double[] values = new double[v.Length];

            for (int i = 0; i < v.Length; i++)
            {
                if (!double.TryParse(v[i], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out values[i]))
                    values[i] = 0.0d;
            }

            return values;
        }
    }
}
