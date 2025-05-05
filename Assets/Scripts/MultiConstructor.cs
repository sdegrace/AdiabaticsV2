using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using StationeersMods.Interface;
using UnityEngine;

namespace net.tanngrisnir.stationeers.adiabaticsV2
{
    public class MultiConstructor: Assets.Scripts.Objects.MultiConstructor, IPatchOnLoad
    {
        [SerializeField]
        public string ModelCopyPrefab;
        public void PatchOnLoad()
        {
            if (this.ModelCopyPrefab != "") {
                var existing = StationeersModsUtility.FindPrefab("StructureVolumePump");
                if (existing != null)
                {
                    Debug.Log("Found Stationeers mod prefab");
                }

                this.Thumbnail = existing.Thumbnail;
                this.Blueprint = existing.Blueprint;

                var erenderer = existing.GetComponent<MeshRenderer>();
                var renderer = this.GetComponent<MeshRenderer>();
                Debug.Log("Found renderer");
                renderer.materials = erenderer.materials;
                Debug.Log("Set renderer");

                var emesh = existing.GetComponent<MeshFilter>();
                Debug.Log("Found emesh");
                var mesh = this.GetComponent<MeshFilter>();
                Debug.Log("Found mesh");
                mesh.mesh = emesh.mesh;
                Debug.Log("Set emesh");
            }
        }
    }
}