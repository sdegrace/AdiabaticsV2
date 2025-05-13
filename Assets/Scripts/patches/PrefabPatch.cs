using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Objects;
using HarmonyLib;
using Objects.Pipes;
using StationeersMods.Interface;
using UnityEngine;
// using UnityEngine.Object;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

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
                //https://discord.com/channels/276525882049429515/412447900120121344/1369013180826648687
                Debug.Log("Prefab Patch started");
                foreach (var gameObject in prefabs)
                {
                    Thing thing = gameObject.GetComponent<Thing>();
                    // Additional patching goes here, like setting references to materials(colors) or tools from the game
                    if (thing != null)
                    {
                        Debug.Log(gameObject.name + " getting added to WorldManager");
                        if (thing is IPatchOnLoad patchable)
                        {
                            patchable.PatchOnLoad();
                        }
                
                        WorldManager.Instance.AddPrefab(thing);
                        Debug.Log(gameObject.name + " added to WorldManager");
                    }
                }

                // PrefabPatch.makePump();
            }
            catch (Exception ex)
            {
                Debug.Log("IT WAS HERE!");
                Debug.Log(ex.Message);
                Debug.LogException(ex);
                Debug.Log(ex.StackTrace);
            }
        }

        [HarmonyPatch(typeof(Prefab), "LoadAll")]
        public static void Postfix()
        {
            Debug.Log("Prefab Patch Postfix");
        }


        private static void makePump()
        {
            // Thing srcPump = WorldManager.Instance.SourcePrefabs.Find(p => p.name == "StructureVolumePump");
            Thing srcPump = StationeersModsUtility.FindPrefab("StructureVolumePump");
            Debug.Log(srcPump + " found");
            var ePumpScriptComp = ((Structure)srcPump).GetComponent<VolumePump>();
            Debug.Log(ePumpScriptComp + " found");
            Thing myPumpThing = UnityEngine.Object.Instantiate<Thing>(srcPump, Prefab.PrefabsGameObject?.transform);
            GameObject myPump = myPumpThing.gameObject;
            myPumpThing.name = "AAAAAclone";
            myPumpThing.PrefabName = myPump.name;
            myPumpThing.PrefabHash = UnityEngine.Animator.StringToHash(myPumpThing.PrefabName);
            
            WorldManager.Instance.SourcePrefabs.Add(myPumpThing);
            
            
            // myPump.transform = srcPump.Transform;
            // myPump.transform.SetParent();
            // if (StationeersModsUtility.FindPrefab("AAAAAA") == null)
            // {
            //     Thing myPumpClone = srcPump.Copy();
            //     myPumpClone.name = "AAAAAA";
            //     myPumpClone.PrefabName = srcPump.name;
            //     myPumpClone.PrefabHash = UnityEngine.Animator.StringToHash(myPumpThing.PrefabName);
            //     Debug.Log("Hash");
            //     WorldManager.Instance.SourcePrefabs.Add(myPumpClone);
            // }
            //
            // GameObject myPump = myPumpThing.gameObject;
            Object.DestroyImmediate(myPump.GetComponent<VolumePump>());
            myPump.AddComponent<PositiveDisplacementPumpPower>();
            Debug.Log("AddScript");


            var properties = typeof(VolumePump).GetProperties(); // It was 171
            var fields = typeof(VolumePump).GetFields();
            // PropertyInfo[] properties = typeof(VolumePump).GetProperties();
            var thisType = typeof(PositiveDisplacementPumpPower);
            //Get all the properties of your class.
            Debug.Log("Setting " + properties.Count() + " properties and " + fields.Count() + " fields");
            var setObj = myPump.GetComponent<PositiveDisplacementPumpPower>();

            foreach (PropertyInfo pI in properties)
            {
                try
                {
                    Debug.Log("Setting " + pI);
                    if (pI.CanWrite & pI.Name != "PrefabName" & pI.Name != "PrefabHash" & pI.Name != "name")
                    {
                        var setpI = thisType.GetProperty(pI.Name);
                        var value = pI.GetValue(ePumpScriptComp);
                        Debug.Log("Setting " + setpI.Name + " to " + value.ToString());
                        setpI.SetValue(setObj, value);
                    }
                    // thisType.GetProperty(pI.Name).SetValue(myPump.GetComponent<PositiveDisplacementPumpPower>(), pI.GetValue((VolumePump)srcPump));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.Log(e.StackTrace);
                }
            }

            foreach (FieldInfo fi in fields)
            {
                try
                {
                    Debug.Log("Setting " + fi);
                    if (fi.Name != "PrefabName" & fi.Name != "PrefabHash" & fi.Name != "name")
                    {
                        var setFI = thisType.GetField(fi.Name);
                        var value = fi.GetValue(ePumpScriptComp);

                        Debug.Log("Setting " + setFI.Name + " to " + value.ToString());
                        setFI.SetValue(setObj, value);
                    }
                    // thisType.GetProperty(pI.Name).SetValue(myPump.GetComponent<PositiveDisplacementPumpPower>(), pI.GetValue((VolumePump)srcPump));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.Log(e.StackTrace);
                }
            }

            myPumpThing = myPump.GetComponent<Thing>();

            // myPump.transform.parent.
            // // Make a clone in the hidden place for prefabs
            // Thing myPump = UnityEngine.Object.Instantiate<Thing>(srcPump as Thing, Vector3.zero, Quaternion.identity,
            //     Prefab.PrefabsGameObject?.transform);

            // First thing you need to do is making it a unique prefab, give it a name, and its unique hash (based on the name).
            string name = "ADB_PDPumpPower";
            myPumpThing.name = name;
            myPumpThing.PrefabName = myPump.name;
            myPumpThing.PrefabHash = UnityEngine.Animator.StringToHash(myPumpThing.PrefabName);
            Debug.Log("Hash");

            // YOU CAN ADD CHANGES TO YOUR PREFAB NOW
            // Because your new pump is already a valid pump using the VolumePump Class you can
            // change the way this pump works, for example, doubling the pump throughput and 
            // halving the power consumption
            //
            // Prefab.RegisterExisting(myPump);
            // Debug.Log("Register");
            // WorldManager.Instance.SourcePrefabs.Add(myPump);

            // Object.Destroy(scriptObject);

            // Debug.Log(myPump.ToString() + " gone?");

            // var newPump = WorldManager.Instance.SourcePrefabs.Find(p => p.name == name);
            // Debug.Log(newPump.ToString() + " back");


            // Finish by registering your prefab to the game 


            // This is a structure prefab, so it needs something to be constructed from

            // A) Make your own constructor, or 
            // B) add it an existing constructor, in this case you CAN'T add it to the "ItemPipeVolumePump"
            // we will use "ItemPipeValve" for this example
            // Thing srcPumpKit = WorldManager.Instance.SourcePrefabs.Find(p => p.name == "ADBItemPipeVolumePump");
            Thing srcPumpKit = StationeersModsUtility.FindPrefab("ADBItemPipeVolumePump");
            Debug.Log("FindKit");
            MultiConstructor srcPumpKitConstructor = (MultiConstructor)srcPumpKit;
            Debug.Log("CastKit");

            // Convert your pump to Structure
            Structure myPumpStructure = (Structure)myPumpThing;
            Debug.Log("CastPump");
            //
            srcPumpKitConstructor.Constructables.Add(myPumpStructure);
            Debug.Log("AddStructure");
            //
            // // But because we are building this asset from an ItemPipeValve, we have to change
            // // its entry tool to use this Item instead of the ItemPipeVolumePump
            myPumpStructure.BuildStates[0].Tool.ToolEntry = srcPumpKitConstructor;
            Debug.Log("addedTool");

            WorldManager.Instance.SourcePrefabs.Add(myPumpThing);
        }
    }

    // [HarmonyPatch(typeof(Prefab), "LoadCorePrefabs")]
    // public class AddStructureIntoKit
    // {
    //     [HarmonyPostfix]
    //     public static void Postfix()
    //     {
    //         Debug.Log("AddStructureIntoKit Postfix");
    //         MultiConstructor kit = Prefab.Find<MultiConstructor>("ADBItemPipeVolumePump");
    //         Structure newStructure = Prefab.Find<Structure>("ADBStructurePositiveDisplacementPumpPower");
    //         if (kit != null && newStructure != null)
    //         {
    //             
    //             kit.Constructables.Add(newStructure);
    //             newStructure.BuildStates[0].Tool.ToolEntry = kit;
    //             
    //             Debug.Log("StructurePositiveDisplacementPumpPower added to ItemPipeVolumePump");
    //         }
    //     }
    // }


    public interface IPatchOnLoad
    {
        void PatchOnLoad();

        bool SkipMaterialPatch() => false;
    }
}