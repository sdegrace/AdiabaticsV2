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
using Assets.Scripts.Objects.Pipes;
using Object = UnityEngine.Object;

namespace net.tanngrisnir.stationeers.adiabaticsV2
{
    public class PositiveDisplacementPumpPower : VolumePump, IPatchOnLoad
    {
        public float Efficiency = .6f;

        public MoleEnergy WasteEnergy => new MoleEnergy((double)((1 - this.Efficiency) * this.OutputSetting));

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
            Debug.Log($"dp: {dp}, Volume: {(float)this.Setting * Efficiency / dp}");
            return (float)this.Setting * Efficiency / dp;
        }
        
        public override void OnAtmosphericTick()
        {
            base.OnAtmosphericTick();
            if (!this.OnOff || !this.Powered || this.Error == 1 || this.InputNetwork?.Atmosphere == null || this.OutputNetwork?.Atmosphere == null)
                return;
            this.FlowInDirection(this.PumpDirection);
        }
        
        public void FlowInDirection(VolumePumpFlowDirection flowDirection)
        {
            if (flowDirection != VolumePumpFlowDirection.Right)
            {
                if (flowDirection != VolumePumpFlowDirection.Left)
                    return;
                this.MoveAtmosphere(this.OutputNetwork.Atmosphere, this.InputNetwork.Atmosphere);
            }
            else
                this.MoveAtmosphere(this.InputNetwork.Atmosphere, this.OutputNetwork.Atmosphere);
        }

        private void MoveAtmosphere(Atmosphere inputAtmosphere, Atmosphere outputAtmosphere)
        {
            double volumeMoved = this.GetVolumeMoved(inputAtmosphere, outputAtmosphere);
            switch (outputAtmosphere.AllowedMatterState)
            {
                case AtmosphereHelper.MatterState.Liquid:
                    AtmosphereHelper.MoveLiquidVolume(inputAtmosphere, outputAtmosphere,
                        new VolumeLitres(volumeMoved));
                    AtmosphereHelper.MoveToEqualize(inputAtmosphere, outputAtmosphere, PressurekPa.MaxValue,
                        AtmosphereHelper.MatterState.Gas);
                    break;
                case AtmosphereHelper.MatterState.Gas:
                case AtmosphereHelper.MatterState.All:
                    AtmosphereHelper.MoveVolume(inputAtmosphere, outputAtmosphere,
                        new VolumeLitres(volumeMoved),
                        AtmosphereHelper.MatterState.All);
                    break;
            }

            outputAtmosphere.GasMixture.AddEnergy(this.WasteEnergy);
        }

        public override Thing.DelayedActionInstance InteractWith(
            Interactable interactable,
            Interaction interaction,
            bool doAction = true)
        {
            Thing.DelayedActionInstance delayedActionInstance1 =
                this.HandleButtonSetting(interactable, interaction, doAction);
            if (delayedActionInstance1 != null)
                return delayedActionInstance1;
            Thing.DelayedActionInstance delayedActionInstance2 = new Thing.DelayedActionInstance()
            {
                Duration = 0.0f,
                ActionMessage = interactable.ContextualName
            };
            delayedActionInstance2.AppendStateMessage("Power " + StringManager.Get(this.OutputSetting) + " W");
            delayedActionInstance2.AppendStateMessage(GameStrings.HoldForSmallIncrements,
                Localization.QuantityModifierKey);
            delayedActionInstance2.AppendStateMessage(GameStrings.UseLabelerToSet);
            // delayedActionInstance2.AppendStateMessage("Flow " + StringManager.Get(this.GetVolumeMoved(this.InputNetwork.Atmosphere, this.OutputNetwork.Atmosphere)) + " L");
            switch (interactable.Action)
            {
                case InteractableType.Button1:
                    if (!doAction)
                        return delayedActionInstance2.Succeed();
                    SettingWheel.PlayWheelSound((Thing)this, this.SettingWheel.Wheel, true, interaction.AltKey,
                        this.OutputSetting, this.MinSetting, this.MaxSetting, this.WheelSettingIncrement,
                        this.WheelAltSettingIncrement);
                    if (GameManager.RunSimulation)
                        this.OutputSetting +=
                            interaction.AltKey ? this.WheelAltSettingIncrement : this.WheelSettingIncrement;
                    return Thing.DelayedActionInstance.Success(interactable.ContextualName);
                case InteractableType.Button2:
                    if (!doAction)
                        return delayedActionInstance2.Succeed();
                    SettingWheel.PlayWheelSound((Thing)this, this.SettingWheel.Wheel, false, interaction.AltKey,
                        this.OutputSetting, this.MinSetting, this.MaxSetting, this.WheelSettingIncrement,
                        this.WheelAltSettingIncrement);
                    if (GameManager.RunSimulation)
                        this.OutputSetting -=
                            interaction.AltKey ? this.WheelAltSettingIncrement : this.WheelSettingIncrement;
                    return Thing.DelayedActionInstance.Success(interactable.ContextualName);
                default:
                    return base.InteractWith(interactable, interaction, doAction);
            }
        }


        public void PatchOnLoad()
        {
            Structure existing = (Structure)StationeersModsUtility.FindPrefab("StructureVolumePump");
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

            var ewheel = existing.transform.Find("Wheel");
            var wheel = this.transform.Find("Wheel");

            wheel.GetComponent<MeshFilter>().mesh = ewheel.GetComponent<MeshFilter>().mesh;
            wheel.GetComponent<MeshRenderer>().materials = ewheel.GetComponent<MeshRenderer>().materials;

            var eMaterialChanger = existing.GetComponent<MaterialChanger>();
            var materialChanger = this.GetComponent<MaterialChanger>();

            var estates = MaterialChangerReflection.GetStates(eMaterialChanger);
            MaterialChangerReflection.SetStates(materialChanger, estates);
            var states = MaterialChangerReflection.GetStates(materialChanger);
            for (int i = 0; i < estates.Length; i++)
            {
                var mats = MaterialChangerReflection.GetMaterials(estates[i]);
                MaterialChangerReflection.SetMaterials(states[i], mats);
            }
            // var states = MaterialChangerReflection.GetStates(materialChanger);
        }
    }
}