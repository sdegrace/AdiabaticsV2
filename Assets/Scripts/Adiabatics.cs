using System;
using net.tanngrisnir.stationeers.adiabaticsV2;
using HarmonyLib;
using StationeersMods.Interface;
[StationeersMod("Adiabatics","Adiabatics [StationeersMods]","0.2.4657.21547.1")]
public class Adiabatics : ModBehaviour
{
    // private ConfigEntry<bool> configBool;
    
    public override void OnLoaded(ContentHandler contentHandler)
    {
        UnityEngine.Debug.Log("Adiabatics says: Hello World!");
        
        //Config example
        // configBool = Config.Bind("Input",
        //     "Boolean",
        //     true,
        //     "Boolean description");
        
        Harmony harmony = new Harmony("Adiabatics");
        PrefabPatch.prefabs = contentHandler.prefabs;
        harmony.PatchAll();
        UnityEngine.Debug.Log("Adiabatics Loaded with " + contentHandler.prefabs.Count + " prefab(s)");
    }
}
