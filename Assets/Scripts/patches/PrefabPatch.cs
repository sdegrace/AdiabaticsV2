using System;
using System.Collections.ObjectModel;
using Assets.Scripts.Objects;
using HarmonyLib;
using StationeersMods.Interface;
using UnityEngine;
using UnityEngine.Rendering;
namespace net.tanngrisnir.stationeers.adiabaticsV2
{
    [HarmonyPatch]
    public class PrefabPatch
    {
        public static ReadOnlyCollection<GameObject> prefabs { get; set; }
        [HarmonyPatch(typeof(Prefab), "LoadAll")]
        public static void Prefix()
        {
            try
            {
                Debug.Log("Prefab Patch started");
                foreach (var gameObject in prefabs)
                {
                    Thing thing = gameObject.GetComponent<Thing>();
                    // Additional patching goes here, like setting references to materials(colors) or tools from the game
                    if (thing != null)
                    {
                        Debug.Log(gameObject.name + " added to WorldManager");
                        WorldManager.Instance.SourcePrefabs.Add(thing);
                        if (thing is IPatchOnLoad patchable)
                        {
                            patchable.PatchOnLoad();
                        }
                        
                        WorldManager.Instance.AddPrefab(thing);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                Debug.LogException(ex);
            }
        }
    }
    
    [HarmonyPatch(typeof(Prefab), "LoadCorePrefabs")]
    public class AddStructureIntoKit
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Debug.Log("AddStructureIntoKit Postfix");
            MultiConstructor kit = Prefab.Find<MultiConstructor>("ItemPipeVolumePump");
            Structure newStructure = Prefab.Find<Structure>("StructurePositiveDisplacementPumpPower");
            if (kit != null && newStructure != null)
            {
                kit.Constructables.Add(newStructure);
                newStructure.BuildStates[0].Tool.ToolEntry = kit;
                
                Debug.Log("StructurePositiveDisplacementPumpPower added to ItemPipeVolumePump");
            }
        }
    }
    
    public interface IPatchOnLoad
    {
        void PatchOnLoad();

        bool SkipMaterialPatch() => false;
    }
}
