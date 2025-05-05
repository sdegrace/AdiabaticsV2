using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Localization2;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Util;
using Effects;
using Objects.Pipes;
using StationeersMods.Interface;
using UnityEngine;

namespace net.tanngrisnir.stationeers.adiabaticsV2
{
    public class PositiveDisplacementPumpPower : VolumePump, IPatchOnLoad
    {
        public float Efficiency = .6f;

        public override float GetUsedPower(CableNetwork cableNetwork)
        {
            if (!(bool)(Object)this.PowerCable || this.PowerCable.CableNetwork != cableNetwork)
                return -1f;
            return !this.OnOff
                ? 0.0f
                : (float)this.Setting;
        }

        private float GetVolumeMoved(Atmosphere inputAtmosphere, Atmosphere outputAtmosphere)
        {
            float dp = outputAtmosphere.PressureGassesAndLiquidsInPa - inputAtmosphere.PressureGassesAndLiquidsInPa;
            return (float)this.Setting * Efficiency / dp;
        }

        private void MoveAtmosphere(Atmosphere inputAtmosphere, Atmosphere outputAtmosphere)
        {
            switch (outputAtmosphere.AllowedMatterState)
            {
                case AtmosphereHelper.MatterState.Liquid:
                    AtmosphereHelper.MoveLiquidVolume(inputAtmosphere, outputAtmosphere,
                        new VolumeLitres((double)this.GetVolumeMoved(inputAtmosphere, outputAtmosphere)));
                    AtmosphereHelper.MoveToEqualize(inputAtmosphere, outputAtmosphere, PressurekPa.MaxValue,
                        AtmosphereHelper.MatterState.Gas);
                    break;
                case AtmosphereHelper.MatterState.Gas:
                case AtmosphereHelper.MatterState.All:
                    AtmosphereHelper.MoveVolume(inputAtmosphere, outputAtmosphere,
                        new VolumeLitres((double)this.GetVolumeMoved(inputAtmosphere, outputAtmosphere)),
                        AtmosphereHelper.MatterState.All);
                    break;
            }
        }

        public void PatchOnLoad()
        {
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