using Assets.Scripts.Objects;
using StationeersMods.Interface;
using UnityEngine;

namespace net.tanngrisnir.stationeers.adiabaticsV2
{
    public class MultiConstructor :
        Assets.Scripts.Objects.MultiConstructor,
        IPatchOnLoad
    {
        [SerializeField]
        public string ModelCopyPrefab;

        public void PatchOnLoad()
        {
            if (this.ModelCopyPrefab != "")
            {
                Debug.Log(this.ModelCopyPrefab + " being copied into " + this.name);
                var src = StationeersModsUtility.FindPrefab(this.ModelCopyPrefab);
                this.Thumbnail = src.Thumbnail;
                this.Blueprint = src.Blueprint;
                this.PaintableMaterial = src.PaintableMaterial;
                if (typeof(Constructor).IsInstanceOfType(src))
                {
                    this.Constructables.Add(((Constructor) src).BuildStructure);
                }
                else
                {
                    this.Constructables = ((MultiConstructor) src).Constructables;
                }

                var srcMf = src.GetComponent<MeshFilter>();
                var mf = this.GetComponent<MeshFilter>();
                mf.mesh = srcMf.mesh;

                var srcRenderer = src.GetComponent<MeshRenderer>();
                var renderer = this.GetComponent<MeshRenderer>();
                renderer.materials = srcRenderer.materials;

                var srcCollider = src.GetComponent<BoxCollider>();
                var collider = this.GetComponent<BoxCollider>();
                collider.center = srcCollider.center;
                collider.size = srcCollider.size;
            }
        }

        bool IPatchOnLoad.SkipMaterialPatch() => this.ModelCopyPrefab != "";
    }
}