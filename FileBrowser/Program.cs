using System;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FileBrowser {
    internal class Program {

        private struct FileBrowserSettings {
            public string title;
            public string defaultDirectory;
            public string defaultFilename;
            public string defaultExtension;
            public string filters;
            public bool multiSelect;
            public bool isFolderPicker;
            public bool save;

            public static FileBrowserSettings Read(BinaryReader reader) => new FileBrowserSettings() {
                title = reader.ReadString(),
                defaultDirectory = reader.ReadString(),
                defaultFilename = reader.ReadString(),
                defaultExtension = reader.ReadString(),
                filters = reader.ReadString(),

                multiSelect = reader.ReadBoolean(),
                isFolderPicker = reader.ReadBoolean(),
                save = reader.ReadBoolean(),
            };
        }

        [STAThread]
        public static void Main(string[] args) {
            ShowHideWindow.Show(false);
            CommonFileDialog dialog;

            var br = new BinaryReader(Console.OpenStandardInput());
            var settings = FileBrowserSettings.Read(br);

            if(!settings.save)
                dialog = new CommonOpenFileDialog(settings.title) { IsFolderPicker = settings.isFolderPicker, Multiselect = settings.multiSelect };
            else
                dialog = new CommonSaveFileDialog(settings.title) { AlwaysAppendDefaultExtension = true };

            using(dialog) {
                if(!string.IsNullOrEmpty(settings.title))
                    dialog.Title = settings.title;
                if(!string.IsNullOrEmpty(settings.defaultDirectory))
                    dialog.InitialDirectory = settings.defaultDirectory;
                if(!string.IsNullOrEmpty(settings.defaultFilename))
                    dialog.DefaultFileName = settings.defaultFilename;
                if(!string.IsNullOrEmpty(settings.defaultExtension))
                    dialog.DefaultExtension = settings.defaultExtension;

                try {
                    if(!string.IsNullOrEmpty(settings.filters))
                        foreach(var filter in settings.filters.Split(';'))
                            dialog.Filters.Add(new CommonFileDialogFilter(filter.Split('|')[0], filter.Split('|')[1]));
                }
                catch { Console.Error.WriteLine("Invalid filter list format"); }

                dialog.EnsureFileExists = true;
                dialog.EnsurePathExists = true;
                dialog.EnsureValidNames = true;

                switch(dialog.ShowDialog()) {
                    case CommonFileDialogResult.Ok:
                        if(dialog is CommonOpenFileDialog)
                            foreach(var file in (dialog as CommonOpenFileDialog).FileNames)
                                Console.WriteLine(file);
                        else
                            Console.WriteLine(dialog.FileName);
                        break;
                }
            }
        }
    }
}