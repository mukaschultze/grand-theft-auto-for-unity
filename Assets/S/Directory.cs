using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Shared;
using IODir = System.IO.Directory;

namespace GrandTheftAuto.New {
    public struct Directory {

        private string path;

        public Directory(string path, bool create = false) {
            this.path = path;

            if(create)
                EnsureExists();
        }

        public bool Exists() {
            return IODir.Exists(path);
        }

        public void ClearContents() {
            if(!Exists())
                return;

            IODir.Delete(path, true);
            IODir.CreateDirectory(path);
            Log.Message("Cleared contents of '{0}'", path);
        }

        public void EnsureExists() {
            if(!Exists()) {
                IODir.CreateDirectory(path);
                Log.Message("Created '{0}'", path);
            }
        }

        public static implicit operator string(Directory dir) {
            return dir.path;
        }

        public static implicit operator Directory(string path) {
            return new Directory(path);
        }

        public static Directory operator +(Directory a, Directory b) {
            return new Directory(Path.Combine(a, b));
        }

        public static Directory operator +(string a, Directory b) {
            return new Directory(Path.Combine(a, b));
        }

        public static Directory operator +(Directory a, string b) {
            return new Directory(Path.Combine(a, b));
        }

    }
}