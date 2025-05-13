using System.Reflection;
using Effects;
using UnityEngine;

namespace net.tanngrisnir.stationeers.adiabaticsV2
{
    public class ADB_MaterialChanger: MaterialChanger
    {
        // public Effects.MaterialAnimState[] states;

    }
    
    public static class MaterialChangerReflection
    {
        public static MaterialAnimState[] GetStates(MaterialChanger changer)
        {
            var field = typeof(MaterialChanger).GetField("states", BindingFlags.NonPublic | BindingFlags.Instance);
            return (MaterialAnimState[])field.GetValue(changer);
        }

        public static void SetStates(MaterialChanger changer, MaterialAnimState[] states)
        {
            var field = typeof(MaterialChanger).GetField("states", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(changer, states);
        }

        public static Material[] GetMaterials(MaterialAnimState state)
        {
            var field = typeof(MaterialAnimState).GetField("materials", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Material[])field.GetValue(state);
        }

        public static void SetMaterials(MaterialAnimState state, Material[] newMaterials)
        {
            var field = typeof(MaterialAnimState).GetField("materials", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(state, newMaterials);
        }
    }

}