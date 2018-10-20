using System;
using System.IO;
using System.Linq;
using GrandTheftAuto.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace GrandTheftAuto.Editor {
    public class TimingWindow : EditorWindow {

        private const float TIMING_ITEM_HEIGHT = 18f;

        private enum Sorting {
            Name = 1,
            Class = 2,
            TotalPercent = 3,
            SelfPercent = 4,
            TotalTime = 5,
            SelfTime = 6,
            Calls = 7,

            InverseName = -1,
            InverseClass = -2,
            InverseTotalPercent = -3,
            InverseSelfPercent = -4,
            InverseTotalTime = -5,
            InverseSelfTime = -6,
            InverseCalls = -7
        }

        private static class Styles {
            public static GUIStyle toolbar = "toolbar";
            public static GUIStyle toolbarButton = "toolbarButton";
            public static GUIStyle toolbarButtonSelected = "TE toolbarbutton";
            public static GUIStyle toolbarDropDown = "ToolbarDropDown";

            public static GUIStyle backgroundOdd = "ObjectPickerResultsOdd";
            public static GUIStyle backgroundEven = "ObjectPickerResultsEven";
            public static GUIStyle verticalLine = "EyeDropperVerticalLine";
        }

        [SerializeField]
        private string selectedFile = string.Empty;
        [SerializeField]
        private string[] timingFiles;
        [SerializeField]
        private string[] timingNames;
        private TimingSaved loadedFile;
        [SerializeField]
        private Vector2 scroll;

        private bool spaceOnHeader;
        private static PrefItem<Sorting> currentSorting = new PrefItem<Sorting>("GrandTheftAuto.Editor.TimingWindow.currentSorting", Sorting.TotalTime);

        [MenuItem("Grand Theft Auto/Timings", false, -1000)]
        private static void Open() {
            GetWindow<TimingWindow>("GTA Timings");
        }

        private void OnEnable() {
            TimingSave.OnDump += SetCurrentTimingGroup;
            ReloadFiles();
        }

        private void OnDisable() {
            TimingSave.OnDump -= SetCurrentTimingGroup;
        }

        private void OnInspectorUpdate() {
            ReloadFiles();
            Repaint();
        }

        private bool HasLoadedFile {
            get {
                return loadedFile.data != null;
            }
        }

        private void OnGUI() {
            DoToolbar();
            DoHeader();

            if(!File.Exists(selectedFile) && !HasLoadedFile)
                return;

            try {
                if(!HasLoadedFile)
                    loadedFile = TimingSave.Load(selectedFile);
            } catch(Exception e) {
                EditorGUILayout.HelpBox(e.ToString(), MessageType.Error, true);
            }

            DoTimingSampleList();
            DoFooter();
        }

        private void SetCurrentTimingGroup(TimingSaved group) {
            loadedFile = group;
            selectedFile = "Last Timing";
        }

        private static int Sort(TimingData a, TimingData b) {
            switch(currentSorting.Value) {
                case Sorting.Name:
                    return string.Compare(a.name, b.name);

                case Sorting.Class:
                    return string.Compare(a.stackClass, b.stackClass);

                case Sorting.TotalTime:
                case Sorting.TotalPercent:
                    return (int)(b.ticksTotal - a.ticksTotal);

                case Sorting.SelfTime:
                case Sorting.SelfPercent:
                    return (int)(b.ticksSelf - a.ticksSelf);

                case Sorting.Calls:
                    return b.calls - a.calls;

                case Sorting.InverseName:
                    return string.Compare(b.name, a.name);

                case Sorting.InverseClass:
                    return string.Compare(b.stackClass, a.stackClass);

                case Sorting.InverseTotalTime:
                case Sorting.InverseTotalPercent:
                    return (int)(a.ticksTotal - b.ticksTotal);

                case Sorting.InverseSelfTime:
                case Sorting.InverseSelfPercent:
                    return (int)(a.ticksSelf - b.ticksSelf);

                case Sorting.InverseCalls:
                    return a.calls - b.calls;

                default:
                    return 0;
            }
        }

        private void ReloadFiles() {
            timingFiles = Directory.GetFiles(TimingSave.TimingsFolder, "*.*");
            timingNames = new string[timingFiles.Length];

            for(var i = 0; i < timingFiles.Length; i++)
                timingNames[i] = Path.GetFileNameWithoutExtension(timingFiles[i]);

            if(!File.Exists(selectedFile) && HasLoadedFile)
                selectedFile = timingFiles.LastOrDefault();
        }

        private void DoToolbar() {
            using(new EditorGUILayout.HorizontalScope(Styles.toolbar)) {
                var selectedIndex = EditorGUILayout.Popup(Array.IndexOf(timingFiles, selectedFile), timingNames, Styles.toolbarDropDown);

                if(selectedIndex >= 0 && selectedIndex < timingFiles.Length) {
                    selectedFile = timingFiles[selectedIndex];
                    loadedFile = new TimingSaved();
                }

                GUILayout.FlexibleSpace();

                if(GUILayout.Button("Open Timings Folder", Styles.toolbarButton))
                    EditorUtility.RevealInFinder(TimingSave.TimingsFolder);

                GUILayout.Space(7f);

                GUI.enabled = !string.IsNullOrEmpty(selectedFile);
                if(GUILayout.Button("Delete File", Styles.toolbarButton)) {
                    File.Delete(selectedFile);
                    File.Delete(selectedFile + ".meta");
                    selectedFile = timingFiles.LastOrDefault();
                    loadedFile = new TimingSaved();
                }

                GUI.enabled = HasLoadedFile;
                if(GUILayout.Button("Save File", Styles.toolbarButton)) {
                    TimingSave.Dump(loadedFile.overheadTicks, loadedFile.data);
                }

                GUI.enabled = true;
                if(GUILayout.Button("Load File", Styles.toolbarButton)) {
                    var path = EditorUtility.OpenFilePanel("Select timing file", TimingSave.TimingsFolder, "*");
                    if(!string.IsNullOrEmpty(path)) {
                        selectedFile = path;
                        loadedFile = new TimingSaved();
                    }
                }
            }
        }

        private void DoHeader() {
            using(new EditorGUILayout.HorizontalScope(Styles.toolbar)) {
                DoSortToggle("Name", Sorting.Name, 150f, true);
                DoSortToggle("Class", Sorting.Class, 104f, false);
                DoSortToggle("Total", Sorting.TotalPercent, 54f, false);
                DoSortToggle("Self", Sorting.SelfPercent, 54f, false);
                DoSortToggle("Time", Sorting.TotalTime, 84f, false);
                DoSortToggle("Self Time", Sorting.SelfTime, 84f, false);
                DoSortToggle("Calls", Sorting.Calls, 60f, false);
                if(spaceOnHeader)
                    GUILayout.Space(15f);
            }
        }

        private void DoSortToggle(string name, Sorting sorting, float minWidth, bool flexible) {
            var selected = Mathf.Abs((int)currentSorting.Value) == Mathf.Abs((int)sorting);

            if(GUILayout.Button(name, selected ? Styles.toolbarButtonSelected : Styles.toolbarButton, GUILayout.ExpandWidth(flexible), GUILayout.MinWidth(minWidth + 5f)))
                currentSorting.Value = selected ? (Sorting)(-(int)currentSorting.Value) : sorting;
        }

        private void DoTimingSampleList() {
            using(var rect1 = new EditorGUILayout.VerticalScope())
            using(var scrollView = new EditorGUILayout.ScrollViewScope(scroll, false, false))
            using(var rect2 = new EditorGUILayout.VerticalScope()) {
                scroll = scrollView.scrollPosition;

                if(Event.current.type == EventType.Repaint)
                    spaceOnHeader = rect1.rect.height < rect2.rect.height;

                if(!HasLoadedFile)
                    return;

                var timings = loadedFile.data;
                Array.Sort(timings, Sort);

                for(var i = 0; i < timings.Length; i++)
                    DoTimingSample(timings[i], i);
            }
        }

        private void DrawLine() {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(1f));

            if(Event.current.type != EventType.Repaint)
                return;

            rect.yMin -= 2f;
            rect.yMax += 2f;

            Styles.verticalLine.Draw(rect, GUIContent.none, 0);
        }

        private void DoTimingSample(TimingData sample, int index) {
            using(new EditorGUILayout.HorizontalScope((index & 1) == 1 ? Styles.backgroundOdd : Styles.backgroundEven, GUILayout.Height(TIMING_ITEM_HEIGHT))) {
                EditorGUILayout.LabelField(sample.name, GUILayout.ExpandWidth(true), GUILayout.MinWidth(150f));
                DrawLine();
                EditorGUILayout.LabelField(sample.stackClass, GUILayout.ExpandWidth(false), GUILayout.Width(100f));
                DrawLine();
                EditorGUILayout.LabelField(((float)sample.ticksTotal / loadedFile.totalTicks).ToString("00.0%"), GUILayout.ExpandWidth(false), GUILayout.Width(50f));
                DrawLine();
                EditorGUILayout.LabelField(((float)sample.ticksSelf / loadedFile.totalTicks).ToString("00.0%"), GUILayout.ExpandWidth(false), GUILayout.Width(50f));
                DrawLine();
                EditorGUILayout.LabelField(new TimeSpan(sample.ticksTotal).GetTimeFormated(), GUILayout.ExpandWidth(false), GUILayout.Width(80f));
                DrawLine();
                EditorGUILayout.LabelField(new TimeSpan(sample.ticksSelf).GetTimeFormated(), GUILayout.ExpandWidth(false), GUILayout.Width(80f));
                DrawLine();
                EditorGUILayout.LabelField(sample.calls.ToString("000000"), GUILayout.ExpandWidth(false), GUILayout.Width(60f));
            }
        }

        private void DoFooter() {
            var totalStr = new TimeSpan(loadedFile.totalTicks).GetTimeFormated();
            var overheadStr = new TimeSpan(loadedFile.overheadTicks).GetTimeFormated();

            EditorGUILayout.HelpBox("Timings are just an average of performance, precision is not guaranteed.\nGC collect might interfere in the results.", MessageType.Warning);
            EditorGUILayout.HelpBox(string.Format("Total Time: {0}\nOverhead Time: {1}", totalStr, overheadStr), MessageType.Info);
        }

    }
}