using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using GrandTheftAuto.Diagnostics;
using UnityEngine;

namespace GrandTheftAuto {
    public sealed class FileBrowser : CustomYieldInstruction {
        private bool _keepWaiting = true;
        private Process process;

        public string Title { get; private set; }
        public string DefaultDirectory { get; private set; }
        public string DefaultName { get; private set; }
        public string DefaultExtension { get; private set; }
        public string Filters { get; private set; } // Usage: Image Files|*.png,*.jpg,*.bmp
        public bool MultiSelect { get; private set; }
        public bool IsFolderPicker { get; private set; }
        public bool IsSavePanel { get; private set; }
        public string FileName { get { return FileNames.FirstOrDefault(); } }
        public string[] FileNames { get; private set; }
        public override bool keepWaiting { get { return _keepWaiting; } }
        public static string ApplicationPath { get { return Path.Combine(Application.streamingAssetsPath, "FileBrowser/FileBrowser.exe"); } }

        private void Execute() {
            if(!File.Exists(ApplicationPath))
                throw new FileNotFoundException("Could not find file browser application", ApplicationPath);

            var results = new List<string>();

            try {
                using(process) {
                    process = new Process() {
                        StartInfo = new ProcessStartInfo() {
                            FileName = ApplicationPath,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            UseShellExecute = false
                        }
                    };
                    process.OutputDataReceived += (sender, data) => {
                        if(!string.IsNullOrEmpty(data.Data))
                            results.Add(data.Data.Replace("\n", string.Empty).Replace("\r", string.Empty));
                    };
                    process.ErrorDataReceived += (sender, data) => {
                        if(!string.IsNullOrEmpty(data.Data))
                            Log.Error(data.Data);
                    };
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    using(var writer = new BinaryWriter(process.StandardInput.BaseStream)) {
                        EncodeSettings(writer);
                        writer.Flush();
                        process.WaitForExit();
                    }

                    FileNames = results.ToArray();
                }
            }
            finally {
                _keepWaiting = false;
            }
        }

        private void ExecuteAsync() {
            _keepWaiting = true;
            new Thread(Execute).Start();
        }

        private void EncodeSettings(BinaryWriter writer) {
            writer.Write(Title ?? string.Empty);
            writer.Write(DefaultDirectory ?? string.Empty);
            writer.Write(DefaultName ?? string.Empty);
            writer.Write(DefaultExtension ?? string.Empty);
            writer.Write(Filters ?? string.Empty);

            writer.Write(MultiSelect);
            writer.Write(IsFolderPicker);
            writer.Write(IsSavePanel);
        }

        #region Static Methods
        public static string OpenFile(string title, string defaultDirectory, string defaultFilename, string defaultExtension) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                DefaultExtension = defaultExtension,
                Filters = defaultExtension + "|*." + defaultExtension
            };
            browser.Execute();
            return browser.FileName;
        }

        public static string OpenFile(string title, string defaultDirectory, string defaultFilename, string defaultExtension, params string[] filters) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                DefaultExtension = defaultExtension,
                Filters = string.Join(";", filters)
            };
            browser.Execute();
            return browser.FileName;
        }

        public static string OpenFolder(string title, string defaultDirectory, string defaultName) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultName,
                IsFolderPicker = true
            };
            browser.Execute();
            return browser.FileName;
        }

        public static string[] OpenFiles(string title, string defaultDirectory, string defaultFilename, string defaultExtension) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                DefaultExtension = defaultExtension,
                Filters = defaultExtension + "|*." + defaultExtension,
                MultiSelect = true
            };
            browser.Execute();
            return browser.FileNames;
        }

        public static string[] OpenFiles(string title, string defaultDirectory, string defaultFilename, string defaultExtension, params string[] filters) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                DefaultExtension = defaultExtension,
                Filters = string.Join(";", filters),
                MultiSelect = true
            };
            browser.Execute();
            return browser.FileNames;
        }

        public static string[] OpenFolders(string title, string defaultDirectory, string defaultName) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultName,
                MultiSelect = true,
                IsFolderPicker = true
            };
            browser.Execute();
            return browser.FileNames;
        }

        public static string SaveFile(string title, string defaultDirectory, string defaultFilename, string defaultExtension) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                Filters = defaultExtension + "|*." + defaultExtension,
                MultiSelect = true,
                IsSavePanel = true
            };
            browser.Execute();
            return browser.FileName;
        }

        public static string SaveFile(string title, string defaultDirectory, string defaultFilename, string defaultExtension, params string[] allowedExtensions) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                Filters = string.Join(";", allowedExtensions),
                MultiSelect = true,
                IsSavePanel = true
            };
            browser.Execute();
            return browser.FileName;
        }

        public static FileBrowser OpenFileAsync(string title, string defaultDirectory, string defaultFilename, string defaultExtension) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                DefaultExtension = defaultExtension,
                Filters = defaultExtension + "|*." + defaultExtension
            };
            browser.ExecuteAsync();
            return browser;
        }

        public static FileBrowser OpenFileAsync(string title, string defaultDirectory, string defaultFilename, string defaultExtension, params string[] filters) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                DefaultExtension = defaultExtension,
                Filters = string.Join(";", filters)
            };
            browser.ExecuteAsync();
            return browser;
        }

        public static FileBrowser OpenFolderAsync(string title, string defaultDirectory, string defaultName) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultName,
                IsFolderPicker = true
            };
            browser.ExecuteAsync();
            return browser;
        }

        public static FileBrowser OpenFilesAsync(string title, string defaultDirectory, string defaultFilename, string defaultExtension) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                DefaultExtension = defaultExtension,
                Filters = defaultExtension + "|*." + defaultExtension,
                MultiSelect = true
            };
            browser.ExecuteAsync();
            return browser;
        }

        public static FileBrowser OpenFilesAsync(string title, string defaultDirectory, string defaultFilename, string defaultExtension, params string[] filters) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                DefaultExtension = defaultExtension,
                Filters = string.Join(";", filters),
                MultiSelect = true
            };
            browser.ExecuteAsync();
            return browser;
        }

        public static FileBrowser OpenFoldersAsync(string title, string defaultDirectory, string defaultName) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultName,
                MultiSelect = true,
                IsFolderPicker = true
            };
            browser.ExecuteAsync();
            return browser;
        }

        public static FileBrowser SaveFileAsync(string title, string defaultDirectory, string defaultFilename, string defaultExtension) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                Filters = defaultExtension + "|*." + defaultExtension,
                MultiSelect = true,
                IsSavePanel = true
            };
            browser.ExecuteAsync();
            return browser;
        }

        public static FileBrowser SaveFileAsync(string title, string defaultDirectory, string defaultFilename, string defaultExtension, params string[] allowedExtensions) {
            var browser = new FileBrowser() {
                Title = title,
                DefaultDirectory = defaultDirectory,
                DefaultName = defaultFilename,
                Filters = string.Join(";", allowedExtensions),
                MultiSelect = true,
                IsSavePanel = true
            };
            browser.ExecuteAsync();
            return browser;
        }
        #endregion

    }
}