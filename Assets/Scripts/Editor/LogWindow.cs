using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GrandTheftAuto.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace GrandTheftAuto.Editor {
    public class LogWindow : EditorWindow {

        private const float LOG_ITEM_HEIGHT = 33f;
        private const float SELECTED_INFO_MIN_HEIGHT = 75f;
        private const float SELECTED_INFO_WINDOW_PERCENT = 0.3f;

        private static class Styles {
            public static GUIStyle label = "CN EntryInfo";
            public static GUIStyle info = "CN EntryInfoIcon";
            public static GUIStyle warning = "CN EntryWarnIcon";
            public static GUIStyle error = "CN EntryErrorIcon";
            public static GUIStyle background = "CN Box";
            public static GUIStyle messages = "CN Message";

            public static GUIStyle toolbar = "toolbar";
            public static GUIStyle toolbarButton = "toolbarButton";

            public static GUIStyle backgroundOdd = "CN EntryBackOdd";
            public static GUIStyle backgroundEven = "CN EntryBackEven";

            public static Texture2D iconError;
            public static Texture2D iconErrorMono;
            public static Texture2D iconErrorSmall;
            public static Texture2D iconInfo;
            public static Texture2D iconInfoMono;
            public static Texture2D iconInfoSmall;
            public static Texture2D iconWarn;
            public static Texture2D iconWarnMono;
            public static Texture2D iconWarnSmall;
            public static bool iconsLoaded;

            public static void LoadIcons() {
                var console = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ConsoleWindow");
                var fullBinding = BindingFlags.Static | BindingFlags.NonPublic;

                iconError = (Texture2D)console.GetField("iconError", fullBinding).GetValue(null);
                iconErrorMono = (Texture2D)console.GetField("iconErrorMono", fullBinding).GetValue(null);
                iconErrorSmall = (Texture2D)console.GetField("iconErrorSmall", fullBinding).GetValue(null);

                iconInfo = (Texture2D)console.GetField("iconInfo", fullBinding).GetValue(null);
                iconInfoMono = (Texture2D)console.GetField("iconInfoMono", fullBinding).GetValue(null);
                iconInfoSmall = (Texture2D)console.GetField("iconInfoSmall", fullBinding).GetValue(null);

                iconWarn = (Texture2D)console.GetField("iconWarn", fullBinding).GetValue(null);
                iconWarnMono = (Texture2D)console.GetField("iconWarnMono", fullBinding).GetValue(null);
                iconWarnSmall = (Texture2D)console.GetField("iconWarnSmall", fullBinding).GetValue(null);

                iconsLoaded = iconError && iconErrorMono && iconErrorSmall && iconInfo && iconInfoMono && iconInfoSmall && iconWarn && iconWarnMono && iconWarnSmall;
            }
        }

        [SerializeField]
        private int selectedLogIndex = -1;
        [SerializeField]
        private int lastLogCount;
        [SerializeField]
        private Vector2 scroll;
        [SerializeField]
        private Vector2 selectedScroll;
        [SerializeField]
        private GUIContent tempContent = new GUIContent();
        [SerializeField]
        private List<Log> filteredLogs;
        private static PrefItem<bool> showMessages = new PrefItem<bool>("GrandTheftAuto.Editor.LogWindow.showMessages", true);
        private static PrefItem<bool> showWarnings = new PrefItem<bool>("GrandTheftAuto.Editor.LogWindow.showWarnings", true);
        private static PrefItem<bool> showErrors = new PrefItem<bool>("GrandTheftAuto.Editor.LogWindow.showErrors", true);

        private Log SelectedLog {
            get {
                try {
                    return filteredLogs[selectedLogIndex];
                } catch {
                    return new Log();
                }
            }
        }

        [MenuItem("Grand Theft Auto/Console", false, -1000)]
        private static void Open() {
            GetWindow<LogWindow>("GTA Console");
        }

        private void OnEnable() {
            if(filteredLogs != null)
                foreach(var log in filteredLogs) {
                    Log.ProcessLog(log.Type, log.LogString, log.StackTrace);
                }

            filteredLogs = new List<Log>();
            FilterLogs();
        }

        private void OnInspectorUpdate() {
            Repaint();
        }

        private void OnGUI() {
            if(!Styles.iconsLoaded)
                Styles.LoadIcons();

            DoToolbar();

            if(lastLogCount != Log.AllLogs.Count)
                FilterLogs();

            DoLogList();
            DoMessageBox();
        }

        private void FilterLogs() {
            filteredLogs.Clear();

            foreach(var log in Log.AllLogs)
                switch(log.Type) {
                    case LogType.Log:
                        if(showMessages)
                            filteredLogs.Add(log);
                        break;

                    case LogType.Warning:
                        if(showWarnings)
                            filteredLogs.Add(log);
                        break;

                    case LogType.Exception:
                    case LogType.Assert:
                    case LogType.Error:
                        if(showErrors)
                            filteredLogs.Add(log);
                        break;
                }

            scroll.y = filteredLogs.Count * LOG_ITEM_HEIGHT;
            selectedLogIndex = -1;
            lastLogCount = Log.AllLogs.Count;
        }

        private void DoToolbar() {
            using(new EditorGUILayout.HorizontalScope(Styles.toolbar)) {
                if(GUILayout.Button("Clear", Styles.toolbarButton)) {
                    selectedLogIndex = -1;
                    Log.Clear();
                }

                GUILayout.Space(7f);
                GUI.enabled = File.Exists(Log.LogFilePath);
                if(GUILayout.Button("Open Current Log", Styles.toolbarButton))
                    Log.OpenLogFile();
                GUI.enabled = File.Exists(Log.PreviousLogPath);
                if(GUILayout.Button("Open Previous Log", Styles.toolbarButton))
                    Log.OpenLogFile(true);
                GUI.enabled = true;
                GUILayout.FlexibleSpace();

                var logContent = new GUIContent(Log.MessagesCount.ToString(), Log.MessagesCount <= 0 ? Styles.iconInfoMono : Styles.iconInfoSmall);
                var warningContent = new GUIContent(Log.WarningsCount.ToString(), Log.WarningsCount <= 0 ? Styles.iconWarnMono : Styles.iconWarnSmall);
                var errorContent = new GUIContent(Log.ErrorsCount.ToString(), Log.ErrorsCount <= 0 ? Styles.iconErrorMono : Styles.iconErrorSmall);

                GUI.changed = false;

                showMessages.Value = GUILayout.Toggle(showMessages, logContent, Styles.toolbarButton);
                showWarnings.Value = GUILayout.Toggle(showWarnings, warningContent, Styles.toolbarButton);
                showErrors.Value = GUILayout.Toggle(showErrors, errorContent, Styles.toolbarButton);

                if(GUI.changed)
                    FilterLogs();
            }
        }

        private void DoLogList() {
            using(var verticalScope = new EditorGUILayout.VerticalScope(Styles.background))
            using(var scrollView = new EditorGUILayout.ScrollViewScope(scroll)) {
                scroll = scrollView.scrollPosition;

                var mainRect = EditorGUILayout.GetControlRect(GUILayout.Height(filteredLogs.Count * LOG_ITEM_HEIGHT));
                var logStartIndex = (int)(scroll.y / LOG_ITEM_HEIGHT);
                var logEndIndex = Mathf.Min(logStartIndex + (int)(verticalScope.rect.height / LOG_ITEM_HEIGHT), filteredLogs.Count - 1);
                var rect = new Rect(mainRect.x, logStartIndex * LOG_ITEM_HEIGHT, mainRect.width, LOG_ITEM_HEIGHT);

                for(var i = logStartIndex; i <= logEndIndex; i++, rect.y += LOG_ITEM_HEIGHT)
                    DoLog(filteredLogs[i], rect, i);

                if(selectedLogIndex >= 0)
                    switch(Event.current.type) {
                        case EventType.KeyDown:
                            switch(Event.current.keyCode) {
                                case KeyCode.UpArrow:
                                    if(selectedLogIndex > 0)
                                        selectedLogIndex--;
                                    if(selectedLogIndex - 1 < logStartIndex)
                                        scroll.y = selectedLogIndex * LOG_ITEM_HEIGHT;
                                    Event.current.Use();
                                    break;

                                case KeyCode.DownArrow:
                                    if(selectedLogIndex < filteredLogs.Count - 1)
                                        selectedLogIndex++;
                                    if(selectedLogIndex + 1 > logEndIndex)
                                        scroll.y = (selectedLogIndex + 1) * LOG_ITEM_HEIGHT - verticalScope.rect.height;
                                    Event.current.Use();
                                    break;
                            }
                            break;
                    }
            }
        }

        private void DoLog(Log log, Rect rect, int index) {
            switch(Event.current.type) {
                case EventType.MouseDown:
                    if(Event.current.button == 0 && rect.Contains(Event.current.mousePosition)) {
                        if(Event.current.clickCount >= 2)
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(log.FileName, log.LineNumber);
                        else
                            selectedLogIndex = index;
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    GUIStyle style;

                    switch(log.Type) {
                        case LogType.Log:
                            style = Styles.info;
                            break;

                        case LogType.Warning:
                            style = Styles.warning;
                            break;

                        case LogType.Exception:
                        case LogType.Assert:
                        case LogType.Error:
                            style = Styles.error;
                            break;

                        default:
                            style = null;
                            break;
                    }

                    tempContent.image = style.normal.background;
                    tempContent.text = string.Format("{0}\n<b><size=9>{1}</size></b>", log.LogString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)[0], log.StackTrace);
                    ((index & 1) == 1 ? Styles.backgroundOdd : Styles.backgroundEven).Draw(rect, false, false, selectedLogIndex == index, false);
                    style.Draw(rect, tempContent, 0, selectedLogIndex == index);
                    Styles.label.Draw(rect, tempContent, 0, selectedLogIndex == index);
                    break;
            }

        }

        private void DoMessageBox() {
            var height = Mathf.Max(SELECTED_INFO_MIN_HEIGHT, SELECTED_INFO_WINDOW_PERCENT * position.height);

            using(var scrollView = new EditorGUILayout.ScrollViewScope(selectedScroll, GUILayout.Height(height))) {
                selectedScroll = scrollView.scrollPosition;
                tempContent.text = string.Format("{0}\n{1}", SelectedLog.LogString, SelectedLog.StackTrace);

                var minHeight = Styles.messages.CalcHeight(tempContent, position.width);
                var options = new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinHeight(minHeight) };

                EditorGUILayout.SelectableLabel(tempContent.text, Styles.messages, options);
            }
        }
    }
}