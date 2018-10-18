using System;
using System.IO;
using GrandTheftAuto.Data;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto.Shared {
    public static class Directories {

        public const string EXE_GTA_III = "gta3.exe";
        public const string EXE_GTA_VC = "gta-vc.exe";
        public const string EXE_GTA_SA = "gta_sa.exe";

        public static string IIIDirectory {
            get {
                return Settings.Instance.gtaIII;
            }
            set {
                Settings.Instance.gtaIII = value;
                Settings.Instance.SaveSettingsFile();
            }
        }

        public static string ViceCityDirectory {
            get {
                return Settings.Instance.gtaVC;
            }
            set {
                Settings.Instance.gtaVC = value;
                Settings.Instance.SaveSettingsFile();
            }
        }

        public static string SanAndreasDirectory {
            get {
                return Settings.Instance.gtaSA;
            }
            set {
                Settings.Instance.gtaSA = value;
                Settings.Instance.SaveSettingsFile();
            }
        }

        public static string GetPathFromVersion(GtaVersion version) {
            switch(version) {
                case GtaVersion.III:
                    if(!IsValidGtaDirectory(IIIDirectory, version))
                        IIIDirectory = FileBrowser.OpenFolder("Select GTA III folder", "", "Grand Theft Auto III");
                    return IIIDirectory;

                case GtaVersion.ViceCity:
                    if(!IsValidGtaDirectory(ViceCityDirectory, version))
                        ViceCityDirectory = FileBrowser.OpenFolder("Select GTA Vice City folder", "", "Grand Theft Auto Vice City");
                    return ViceCityDirectory;

                case GtaVersion.SanAndreas:
                    if(!IsValidGtaDirectory(SanAndreasDirectory, version))
                        SanAndreasDirectory = FileBrowser.OpenFolder("Select GTA San Andreas folder", "", "Grand Theft Auto San Andreas");
                    return SanAndreasDirectory;

                default:
                    throw new ArgumentException("Invalid GTA version");
            }
        }

        public static bool IsValidGtaDirectory(string path) {
            try {
                if(!Directory.Exists(path) || !File.Exists(Path.Combine(path, DataFile.DAT_MAIN)))
                    return false;

                if(File.Exists(Path.Combine(path, EXE_GTA_III)))
                    return true;

                if(File.Exists(Path.Combine(path, EXE_GTA_VC)))
                    return true;

                if(File.Exists(Path.Combine(path, EXE_GTA_SA)))
                    return true;

                return false;
            }
            catch(Exception e) {
                Log.Exception(e);
                return false;
            }
        }

        public static bool IsValidGtaDirectory(string path, GtaVersion version) {
            try {
                if(!Directory.Exists(path) || !File.Exists(Path.Combine(path, DataFile.DAT_MAIN)))
                    return false;

                switch(version) {
                    case GtaVersion.III:
                        return File.Exists(Path.Combine(path, EXE_GTA_III));

                    case GtaVersion.ViceCity:
                        return File.Exists(Path.Combine(path, EXE_GTA_VC));

                    case GtaVersion.SanAndreas:
                        return File.Exists(Path.Combine(path, EXE_GTA_SA));

                    default:
                        return false;
                }

            }
            catch(Exception e) {
                Log.Exception(e);
                return false;
            }
        }

        public static GtaVersion GetVersionFromPath(string path) {

            if(!File.Exists(Path.Combine(path, DataFile.DAT_MAIN)))
                return GtaVersion.Unknown;

            if(File.Exists(Path.Combine(path, EXE_GTA_III)))
                return GtaVersion.III;

            if(File.Exists(Path.Combine(path, EXE_GTA_VC)))
                return GtaVersion.ViceCity;

            if(File.Exists(Path.Combine(path, EXE_GTA_SA)))
                return GtaVersion.SanAndreas;

            return GtaVersion.Unknown;

        }
    }
}