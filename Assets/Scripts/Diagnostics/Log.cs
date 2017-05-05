using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GrandTheftAuto.Diagnostics {
    [Serializable]
    public struct Log {

        private const string LOG_FILE = "Logs.txt";
        private const string PREV_LOG_FILE = "Logs_prev.txt";

        [SerializeField]
        private string logString;
        [SerializeField]
        private string stackTrace;
        [SerializeField]
        private LogType type;

        public string LogString {
            get { return logString; }
            set { logString = value; }
        }
        public string StackTrace {
            get { return stackTrace; }
            set { stackTrace = value; }
        }
        public LogType Type {
            get { return type; }
            set { type = value; }
        }

        public static int MessagesCount { get; private set; }
        public static int WarningsCount { get; private set; }
        public static int ErrorsCount { get; private set; }
        public static List<Log> AllLogs { get; set; }

        public static string LogFilePath { get { return Path.Combine(Environment.CurrentDirectory, LOG_FILE); } }
        public static string PreviousLogPath { get { return Path.Combine(Environment.CurrentDirectory, PREV_LOG_FILE); } }

        private static Log current = new Log();
        private static readonly StreamWriter writer;

        static Log() {
            AllLogs = new List<Log>(5000);

            try {
                File.Delete(PreviousLogPath);
                if(File.Exists(LogFilePath))
                    File.Move(LogFilePath, PreviousLogPath);
            }
            catch { /*Failed to move the old log, ♫♪ but I really don't care ♫♪*/ }

            try { writer = new StreamWriter(LogFilePath); }
            catch { writer = new StreamWriter(LogFilePath + "_acessdenied"); /*This may happen if you open two instances of the program at once*/ }

            writer.AutoFlush = true;
            writer.WriteLine("\n=====================================");
            writer.WriteLine(DateTime.Now);
            writer.WriteLine("=====================================\n");
        }

        public static void Message(object message) {
            ProcessLog(LogType.Log, message.ToString());
        }

        public static void Message(string format, params object[] args) {
            ProcessLog(LogType.Log, format, null, args);
        }

        public static void Warning(object message) {
            ProcessLog(LogType.Warning, message.ToString());
        }

        public static void Warning(string format, params object[] args) {
            ProcessLog(LogType.Warning, format, null, args);
        }

        public static void Error(object message) {
            ProcessLog(LogType.Error, message.ToString());
        }

        public static void Error(string format, params object[] args) {
            ProcessLog(LogType.Error, format, null, args);
        }

        public static void Exception(Exception e) {
            ProcessLog(LogType.Exception, e.Message, e.StackTrace);
        }

        public static void Clear() {
            AllLogs.Clear();
            MessagesCount = 0;
            WarningsCount = 0;
            ErrorsCount = 0;
            ErrorsCount = 0;
        }

        public static void OpenLogFile() {
            Application.OpenURL(LogFilePath);
        }

        public static void OpenLogFile(bool previous) {
            Application.OpenURL(previous ? PreviousLogPath : LogFilePath);
        }

        public static void ProcessLog(LogType type, string message, string exceptionStack = null, params object[] args) {
            switch(type) {
                case LogType.Log:
                    MessagesCount++;
                    break;

                case LogType.Warning:
                    WarningsCount++;
                    break;

                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    ErrorsCount++;
                    break;
            }

            var timing = (Timing)null;

            if(Timing.Running)
                timing = new Timing("Processing Logs");

            try {
                current.LogString = string.Format(message, args);
                current.Type = type;

                if(string.IsNullOrEmpty(exceptionStack)) {
                    var stack = new StackFrame(2, true);

                    if(!string.IsNullOrEmpty(stack.GetFileName()))
                        current.StackTrace = string.Format(" at {0}, line {1}", Path.GetFileName(stack.GetFileName()), stack.GetFileLineNumber());
                }
                else
                    current.StackTrace = exceptionStack;

                AllLogs.Add(current);
                writer.WriteLine(current);
            }
            catch(Exception e) {
                Debug.LogException(e);
            }

            if(timing != null)
                timing.Dispose();
        }

        public override string ToString() {
            if(!string.IsNullOrEmpty(StackTrace))
                return string.Format("{0}{1}:\n{2}\n", Type, StackTrace, LogString);
            else
                return string.Format("{0}:\n{1}\n", Type, LogString);
        }

    }
}