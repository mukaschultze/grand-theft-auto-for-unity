//using System.IO;
//using GrandTheftAuto.Img;

//namespace GrandTheftAuto.Caches {
//    public abstract class Cache {

//        public void Load(GtaVersion version) {
//            Load(Directories.GetCacheDirectory(GetType(), version));
//        }

//        public void Load(string path) {
//            using(var stream = File.Open(path, FileMode.Open))
//                Load(stream);
//        }

//        public void Load(FileEntry file) {
//            using(var stream = new MemoryStream(file.GetData()))
//                Load(stream);
//        }

//        public void Load(Stream stream) {
//            using(var reader = new BufferReader(stream))
//                Load(reader);
//        }

//        public abstract void Load(BufferReader reader);

//        public void Save(GtaVersion version) {
//            Save(Directories.GetCacheDirectory(GetType(), version));
//        }

//        public void Save(string path) {
//            using(var stream = File.Open(path, FileMode.Create))
//                Save(stream);
//        }

//        public void Save(Stream stream) {
//            using(var writer = new BinaryWriter(stream))
//                Save(writer);
//        }

//        public abstract void Save(BinaryWriter writer);

//        public void LoadOrCreate(GtaVersion version) {
//            if(Directories.HasCache(GetType(), version))
//                Load(version);
//            else
//                Create(version);
//        }

//        public abstract void Create(GtaVersion version);
//    }
//}
