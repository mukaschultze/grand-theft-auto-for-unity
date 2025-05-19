﻿using System;
using System.Collections.Generic;
using GrandTheftAuto.Dff;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Ide.Fx;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityMaterial = UnityEngine.Material;

namespace GrandTheftAuto.Ide {
    [Serializable]
    public class ItemDefinition {
        public int ID { get; protected set; }
        public string DffName { get; protected set; }
        public string TxdName { get; protected set; }
        public List<FxDefinition> Effects { get; set; }

        public delegate void GameObjectModificationCallback(GameObject go, DffFile dff, ItemDefinition definition);
        public delegate void TransformModificationCallback(Transform transform, Frame frame, ItemDefinition definition);
        public delegate void RendererModificationCallback(Renderer renderer, Geometry geometry, string txdName, ItemDefinition definition);

        public static GameObjectModificationCallback GameObjectModifiers { get; set; }
        public static TransformModificationCallback TransformModifiers { get; set; }
        public static RendererModificationCallback RendererModifiers { get; set; }

        private GameObject loadedObj;

        public ItemDefinition(string dffName) {
            DffName = dffName;
        }

        public ItemDefinition(string[] tokens) {
            ID = int.Parse(tokens[0]);
            DffName = tokens[1];
            TxdName = tokens[2];
        }

        static ItemDefinition() {
            GameObjectModifiers = (go, dff, definition) => { };
            TransformModifiers = (transform, frame, definition) => { };
            RendererModifiers = (renderer, geometry, txdName, definition) => { };
        }

        public GameObject GetObject(Vector3 position, Quaternion rotation) {
            if(loadedObj)
                using(new Timing("Instantiating Object")) {
                    var obj = Object.Instantiate(loadedObj);
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    return obj;
                }

            return loadedObj = CreateObject(position, rotation);
        }

        private GameObject CreateObject(Vector3 position, Quaternion rotation) {
            using(new Timing("Creating Object"))
                try {
                    if(!Loader.ModelCollection.TryGetValue(DffName, out var dff)) {
                        Log.Error("Dff {0} not found", DffName);
                        return new GameObject("DFF NOT FOUND " + DffName);
                    }

                    var go = CreateTransform(dff.RootFrame, null, TxdName).gameObject;
                    go.AddComponent<ItemDefinitionComponent>().RegisterDefinition(this);

                    go.transform.position = position;
                    go.transform.rotation = rotation;

                    // Need to this after setting position because street signs use world position instead of local position
                    foreach(var fx in dff.Effects)
                        fx.CreateEffectInUnity(go);

                    GameObjectModifiers(go, dff, this);
                    return go;
                } catch(Exception e) {
                    Log.Error("Failed to create object {0} (ID {1}): {2}", DffName, ID, e);
                    return new GameObject("ERROR " + DffName);
                }
        }

        private Transform CreateTransform(Frame frame, Transform parent, string txdName) {
            using(new Timing("Creating Transform")) {
                if(frame == null) {
                    Log.Warning("Null frame on object \"{0}\"(ID {1})", DffName, ID);
                    return new GameObject("NULL FRAME " + DffName).transform;
                }

                var go = new GameObject(frame.Name);
                var transform = go.transform;

                if(frame.Geometry != null)
                    CreateRenderer(frame, go, txdName);

                transform.SetParent(parent);

                // transform.localPosition = frame.Position;
                // transform.rotation = frame.Rotation.GetQuaternionFromMatrix();

                foreach(var child in frame.Children)
                    CreateTransform(child, transform, txdName);

                TransformModifiers(transform, frame, this);

                return transform;
            }
        }

        private Renderer CreateRenderer(Frame frame, GameObject go, string txdName) {
            using(new Timing("Creating Renderer")) {
                var meshFilter = go.AddComponent<MeshFilter>();
                var renderer = go.AddComponent<MeshRenderer>();

                meshFilter.sharedMesh = frame.Geometry;
                meshFilter.sharedMesh.name = frame.Name;

                var materials = new UnityMaterial[meshFilter.sharedMesh.subMeshCount];
                var count = materials.Length;

                for(var i = 0; i < count; i++)
                    materials[i] = frame.Geometry.Materials[i].GetUnityMaterial(txdName);

                renderer.materials = materials;

                RendererModifiers(renderer, frame.Geometry, txdName, this);

                return renderer;
            }
        }

    }
}