using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using GrandTheftAuto.Shared;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GrandTheftAuto.Diagnostics {
    [Serializable]
    public struct Log {

        [SerializeField]
        private string logString;
        [SerializeField]
        private string fileName;
        [SerializeField]
        private int lineNumber;
        [SerializeField]
        private string stackTrace;
        [SerializeField]
        private LogType type;

        public string LogString {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return logString; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { logString = value; }
        }
        public string StackTrace {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return stackTrace; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { stackTrace = value; }
        }
        public LogType Type {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return type; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { type = value; }
        }
        public string FileName {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return fileName; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { fileName = value; }
        }
        public int LineNumber {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return lineNumber; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { lineNumber = value; }
        }

        public static int MessagesCount {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private set;
        }
        public static int WarningsCount {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private set;
        }
        public static int ErrorsCount {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private set;
        }
        public static List<Log> AllLogs {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set;
        }

        public static string LogFilePath { get { return Settings.Instance.logFilePath; } }
        public static string PreviousLogPath { get { return LogFilePath + "_old"; } }

        private static Log current = new Log();
        private static readonly StreamWriter writer;

        static Log() {
            AllLogs = new List<Log>(5000);

            var folder = Path.GetDirectoryName(LogFilePath);

            if(!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            try {
                File.Delete(PreviousLogPath);
                if(File.Exists(LogFilePath))
                    File.Move(LogFilePath, PreviousLogPath);
            } catch { /*Failed to move the old log, ♫♪ but I really don't care ♫♪*/ }

            try {
                writer = new StreamWriter(LogFilePath);
            } catch {
                writer = new StreamWriter(LogFilePath + "_acessdenied"); /*This may happen if you open two instances of the program at once*/
            }

            writer.AutoFlush = true;
            writer.WriteLine("\n=====================================");
            writer.WriteLine(DateTime.Now);
            writer.WriteLine("=====================================\n");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Message(object message) {
            ProcessLog(LogType.Log, message.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Message(string format, params object[] args) {
            ProcessLog(LogType.Log, format, null, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(object message) {
            ProcessLog(LogType.Warning, message.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string format, params object[] args) {
            ProcessLog(LogType.Warning, format, null, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(object message) {
            ProcessLog(LogType.Error, message.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string format, params object[] args) {
            ProcessLog(LogType.Error, format, null, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            // if(Timing.Running)
            //     timing = Timing.Get("Processing Logs");

            try {
                current.LogString = string.Format(message, args);
                current.Type = type;

                if(!string.IsNullOrEmpty(exceptionStack)) {
                    current.StackTrace = exceptionStack;
                } else if(!Settings.Instance.stackTraceEnabled) {
                    current.stackTrace = string.Empty;
                } else {
                    var stack = new StackFrame(2, true);

                    if(!string.IsNullOrEmpty(stack.GetFileName())) {
                        current.FileName = stack.GetFileName();
                        current.LineNumber = stack.GetFileLineNumber();
                        current.StackTrace = string.Format(" at {0}, line {1}", Path.GetFileName(current.FileName), current.LineNumber);
                    }
                }

                AllLogs.Add(current);
                writer.WriteLine(current);
            } catch(Exception e) {
                Debug.LogException(e);
            }

            if(timing != null)
                timing.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() {
            if(!string.IsNullOrEmpty(StackTrace))
                return string.Format("{0}{1}:\n{2}\n", Type, StackTrace, LogString);
            else
                return string.Format("{0}:\n{1}\n", Type, LogString);
        }

    }
}