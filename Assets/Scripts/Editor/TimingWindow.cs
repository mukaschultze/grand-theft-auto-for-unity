using System;
using System.IO;
using System.Linq;
using System.Text;
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

            InverseFlag = 1 << 10
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
        [SerializeField]
        private TimingsContainer loadedContainer;
        [SerializeField]
        private Vector2 scroll;

        private bool spaceOnHeader;
        private static PrefItem<Sorting> currentSorting = new PrefItem<Sorting>("TimingsSorting", Sorting.TotalTime);

        [MenuItem("Grand Theft Auto/Timings", false, -1000)]
        private static void Open() {
            GetWindow<TimingWindow>("GTA Timings");
        }

        private void OnEnable() {
            TimingsContainer.OnDump += SetCurrentTimingGroup;
            ReloadFiles();
        }

        private void OnDisable() {
            TimingsContainer.OnDump -= SetCurrentTimingGroup;
        }

        private void OnInspectorUpdate() {
            ReloadFiles();
            Repaint();
        }

        private bool HasLoadedContainer {
            get {
                return loadedContainer.samples != null;
            }
        }

        private void OnGUI() {
            DoToolbar();
            DoHeader();

            if(!File.Exists(selectedFile) && !HasLoadedContainer)
                return;

            try {
                if(!HasLoadedContainer)
                    loadedContainer = TimingsContainer.Load(selectedFile);
            } catch(Exception e) {
                EditorGUILayout.HelpBox(e.ToString(), MessageType.Error, true);
            }

            DoTimingSampleList();
            DoFooter();
        }

        private void SetCurrentTimingGroup(TimingsContainer newData) {
            loadedContainer = newData;
            selectedFile = "Last Timing";
        }

        private static int Sort(TimingSample a, TimingSample b) {

            if((currentSorting.Value & Sorting.InverseFlag) != 0) {
                var aux = a;
                a = b;
                b = aux;
            }

            switch(currentSorting.Value & ~Sorting.InverseFlag) {
                case Sorting.Name:
                    return string.Compare(a.label, b.label);

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

                default:
                    return 0;
            }
        }

        private void ReloadFiles() {
            timingFiles = Directory.GetFiles(TimingsContainer.TimingsFolder, "*.*");
            timingNames = new string[timingFiles.Length];

            for(var i = 0; i < timingFiles.Length; i++)
                timingNames[i] = Path.GetFileNameWithoutExtension(timingFiles[i]);

            if(!File.Exists(selectedFile) && HasLoadedContainer)
                selectedFile = timingFiles.LastOrDefault();

        }

        private void DoToolbar() {
            using(new EditorGUILayout.HorizontalScope(Styles.toolbar)) {
                var selectedIndex = EditorGUILayout.Popup(Array.IndexOf(timingFiles, selectedFile), timingNames, Styles.toolbarDropDown);

                if(selectedIndex >= 0 && selectedIndex < timingFiles.Length) {
                    selectedFile = timingFiles[selectedIndex];
                    loadedContainer = new TimingsContainer();
                }

                GUILayout.FlexibleSpace();

                if(GUILayout.Button("Open Timings Folder", Styles.toolbarButton))
                    EditorUtility.RevealInFinder(TimingsContainer.TimingsFolder);

                GUILayout.Space(7f);

                GUI.enabled = !string.IsNullOrEmpty(selectedFile);
                if(GUILayout.Button("Delete File", Styles.toolbarButton)) {
                    File.Delete(selectedFile);
                    File.Delete(selectedFile + ".meta");
                    selectedFile = timingFiles.LastOrDefault();
                    loadedContainer = new TimingsContainer();
                }

                // GUI.enabled = HasLoadedContainer;
                // if(GUILayout.Button("Save File", Styles.toolbarButton)) {
                //     TimingsContainer.Dump(loadedContainer.overheadTicks, loadedContainer.samples);
                // }

                GUI.enabled = true;
                if(GUILayout.Button("Load File", Styles.toolbarButton)) {
                    var path = EditorUtility.OpenFilePanel("Select timing file", TimingsContainer.TimingsFolder, "*");
                    if(!string.IsNullOrEmpty(path)) {
                        selectedFile = path;
                        loadedContainer = new TimingsContainer();
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
            var selected = (currentSorting.Value & ~Sorting.InverseFlag) == (sorting & ~Sorting.InverseFlag);

            if(GUILayout.Button(name, selected ? Styles.toolbarButtonSelected : Styles.toolbarButton, GUILayout.ExpandWidth(flexible), GUILayout.MinWidth(minWidth + 5f))) {
                currentSorting.Value = selected ? (currentSorting.Value ^ Sorting.InverseFlag) : sorting;
            }
        }

        private void DoTimingSampleList() {
            using(var rect1 = new EditorGUILayout.VerticalScope())
            using(var scrollView = new EditorGUILayout.ScrollViewScope(scroll, false, false))
            using(var rect2 = new EditorGUILayout.VerticalScope()) {
                scroll = scrollView.scrollPosition;

                if(Event.current.type == EventType.Repaint)
                    spaceOnHeader = rect1.rect.height < rect2.rect.height;

                if(!HasLoadedContainer)
                    return;

                var timings = loadedContainer.samples;
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

        private void DoTimingSample(TimingSample sample, int index) {
            using(new EditorGUILayout.HorizontalScope((index & 1) == 1 ? Styles.backgroundOdd : Styles.backgroundEven, GUILayout.Height(TIMING_ITEM_HEIGHT))) {
                EditorGUILayout.LabelField(sample.label, GUILayout.ExpandWidth(true), GUILayout.MinWidth(150f));
                DrawLine();
                EditorGUILayout.LabelField(sample.stackClass, GUILayout.ExpandWidth(false), GUILayout.Width(100f));
                DrawLine();
                EditorGUILayout.LabelField(((float)sample.ticksTotal / loadedContainer.totalTicks).ToString("00.0%"), GUILayout.ExpandWidth(false), GUILayout.Width(50f));
                DrawLine();
                EditorGUILayout.LabelField(((float)sample.ticksSelf / loadedContainer.totalTicks).ToString("00.0%"), GUILayout.ExpandWidth(false), GUILayout.Width(50f));
                DrawLine();
                EditorGUILayout.LabelField(new TimeSpan(sample.ticksTotal).GetTimeFormated(), GUILayout.ExpandWidth(false), GUILayout.Width(80f));
                DrawLine();
                EditorGUILayout.LabelField(new TimeSpan(sample.ticksSelf).GetTimeFormated(), GUILayout.ExpandWidth(false), GUILayout.Width(80f));
                DrawLine();
                EditorGUILayout.LabelField(sample.calls.ToString("000000"), GUILayout.ExpandWidth(false), GUILayout.Width(60f));
            }
        }

        private void DoFooter() {
            var str = new StringBuilder(300);

            if(!HasLoadedContainer)
                return;

            str.AppendLine();

            foreach(var sample in loadedContainer.fastSamples) {
                str.Append(sample.label);
                str.Append(": ");
                str.Append(new TimeSpan(sample.ticksSelf).GetTimeFormated());
                str.AppendFormat(" (Called {0} times)", sample.calls);
                str.AppendLine();
            }

            EditorGUILayout.HelpBox("Timings are just an average of performance, precision is not guaranteed.\nGC collect might interfere in the results.", MessageType.Warning);
            EditorGUILayout.HelpBox(str.ToString(), MessageType.Info);
        }

    }
}