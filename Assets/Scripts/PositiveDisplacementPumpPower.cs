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
    public class PositiveDisplacementPumpPower : MultiConstructor, VolumePump
    {
        protected VolumePumpFlowDirection FlowDirection;
        [SerializeField] private MaterialChanger directionControl0;
        [SerializeField] private MaterialChanger directionControl1;
        public static readonly int ButtonHash = UnityEngine.Animator.StringToHash("Button");
        public static readonly float BasePowerDraw = 200f;
        public float Efficiency = .6f;

        private static readonly string[] FlowDirectionModes = new string[2]
        {
            "Right",
            "Left"
        };

        public override float MinOperatingSoundPitch => 0.75f;

        public override float MaxOperatingSoundPitch => 3f;

        public override string[] ModeStrings => PositiveDisplacementPumpPower.FlowDirectionModes;

        public override Thing.DelayedActionInstance InteractWith(
            Interactable interactable,
            Interaction interaction,
            bool doAction = true)
        {
            Thing.DelayedActionInstance delayedActionInstance = new Thing.DelayedActionInstance()
            {
                Duration = 0.0f,
                ActionMessage = interactable.ContextualName
            };
            delayedActionInstance.AppendStateMessage(GameStrings.VolumeChangeDirection);
            switch (interactable.Action)
            {
                case InteractableType.Button3:
                    if (!doAction)
                        return delayedActionInstance.Succeed();
                    this.FlowDirection = VolumePumpFlowDirection.Right;
                    OnServer.Interact(this.InteractMode, 0);
                    this.PlaySound(TurboVolumePump.ButtonHash);
                    return Thing.DelayedActionInstance.Success(interactable.ContextualName);
                case InteractableType.Button4:
                    if (!doAction)
                        return delayedActionInstance.Succeed();
                    this.FlowDirection = VolumePumpFlowDirection.Left;
                    OnServer.Interact(this.InteractMode, 1);
                    this.PlaySound(TurboVolumePump.ButtonHash);
                    return Thing.DelayedActionInstance.Success(interactable.ContextualName);
                default:
                    return base.InteractWith(interactable, interaction, doAction);
            }
        }

        protected override void RefreshAnimState(bool skipAnimation = false)
        {
            this.SwitchOnOff.RefreshState(skipAnimation);
            this.directionControl0.ChangeState(this.Mode == 0
                ? (!this.OnOff || !this.Powered ? Defines.Animator.On : Defines.Animator.OnPowered)
                : (!this.OnOff || !this.Powered ? Defines.Animator.Off : Defines.Animator.OffPowered));
            this.directionControl1.ChangeState(this.Mode == 1
                ? (!this.OnOff || !this.Powered ? Defines.Animator.On : Defines.Animator.OnPowered)
                : (!this.OnOff || !this.Powered ? Defines.Animator.Off : Defines.Animator.OffPowered));
        }

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
                    AtmosphereHelper.MoveLiquidVolume(inputAtmosphere, outputAtmosphere, new VolumeLitres((double) this.GetVolumeMoved(inputAtmosphere, outputAtmosphere)));
                    AtmosphereHelper.MoveToEqualize(inputAtmosphere, outputAtmosphere, PressurekPa.MaxValue, AtmosphereHelper.MatterState.Gas);
                    break;
                case AtmosphereHelper.MatterState.Gas:
                case AtmosphereHelper.MatterState.All:
                    AtmosphereHelper.MoveVolume(inputAtmosphere, outputAtmosphere, new VolumeLitres((double) this.GetVolumeMoved(inputAtmosphere, outputAtmosphere)), AtmosphereHelper.MatterState.All);
                    break;
            }
        }
        
        public new void FlowInDirection(VolumePumpFlowDirection flowDirection)
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

        public override VolumePumpFlowDirection PumpDirection => (VolumePumpFlowDirection)this.Mode;

        
    }
}