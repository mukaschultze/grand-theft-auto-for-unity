using System;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Img;
using UnityEngine;

namespace GrandTheftAuto.Ipl {
    public class BinaryIpl {
        private const int BNRY = 2037542498;

        private readonly ItemPlacement[] placements;

        public ItemPlacement[] Placements { get { return placements; } }

        public ItemPlacement this[int index] { get { return placements[index]; } }

        public BinaryIpl(string filePath) : this(new FileEntry(filePath)) { }

        public BinaryIpl(FileEntry file) {
            using(new Timing("Loading Binary IPL")) {
                var reader = file.Reader;

                if(reader.ReadInt32() != BNRY)
                    throw new ArgumentException("The file is not a valid binary IPL");

                var placement = new ItemPlacement();
                var position = new Vector3();
                var rotation = new Quaternion();
                var instances = reader.ReadInt32();

                placements = new ItemPlacement[instances];

                reader.SkipStream(68);
                reader.PrewarmBuffer(instances * 40);

                for(var i = 0; i < instances; i++) {
                    position.x = reader.ReadSingle();
                    position.z = reader.ReadSingle();
                    position.y = reader.ReadSingle();

                    rotation.x = reader.ReadSingle();
                    rotation.z = reader.ReadSingle();
                    rotation.y = reader.ReadSingle();
                    rotation.w = reader.ReadSingle();

                    placement.DefinitionID = reader.ReadInt32();
                    placement.ItemName = "Streaming Placement";
                    placement.Position = position;
                    placement.Rotation = rotation;
                    placement.Scale = Vector3.one;

                    reader.Skip(4);
                    placement.LodDefinitionID = reader.ReadInt32();

                    placements[i] = placement;
                }
            }
        }
    }
}