using HarmonyLib;
using System.Linq;
using UnityEngine;


namespace StackFilterSizes
{

    [HarmonyPatch(typeof(PlayerInspector), "LateUpdate")]
    public static class PlayerInspectorLateUpdatePatch
    {
        private static GameObject lastHitObject = null;

        static void Postfix(PlayerInspector __instance)
        {
            var playerCameraField = AccessTools.Field(typeof(PlayerInspector), "playerCamera");
            Transform playerCamera = (Transform)playerCameraField.GetValue(__instance);
            var collisionLayersField = AccessTools.Field(typeof(PlayerInspector), "collisionLayers");
            LayerMask collisionLayers = (LayerMask)collisionLayersField.GetValue(__instance);

            var machineHitField = AccessTools.Field(typeof(PlayerInspector), "_machineHit");
            var machineHit = machineHitField.GetValue(__instance);

            if (playerCamera != null && SharedState.ShowDEBUG )
            {
                Ray ray = new Ray(playerCamera.position, playerCamera.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100f, collisionLayers))
                {
                    if (hit.collider.gameObject != lastHitObject)
                    {
                        SharedState.LastHitObjectTag = hit.collider.gameObject.tag;
                        SharedState.LastHitObjectLayer = LayerMask.LayerToName(hit.collider.gameObject.layer);

                        if (machineHit != null && machineHit is GenericMachineInstanceRef machineInfo)
                        {
                            if (!machineInfo.IsValid())
                            {
                                Debug.Log("Machine reference is not valid.");
                                return;
                            }

                            SharedState.LastMachineInfo = $"Machine Hit ID: {machineInfo}\n";

                            var inventoriesList = machineInfo.GetInventoriesList();
                            if (inventoriesList != null && inventoriesList.Any())
                            {

                                var inventory = inventoriesList.First();

                                var inventoryDetails = new SharedState.InventoryDetails
                                {
                                    NumSlots = inventory.numSlots,
                                    ItemStacks = inventory.myStacks.Where(s => s.id != -1).Select(s => new SharedState.ItemStack
                                    {
                                        ItemID = s.id,
                                        Count = s.count,
                                        MaxStack = s.maxStack
                                    }).ToList()
                                };


                                SharedState.LastInventoryDetails = inventoryDetails;
                            }
                            else
                            {
                                Debug.Log("No inventories found in the machine.");
                                SharedState.LastInventoryDetails = null;
                            }
                        }
                        lastHitObject = hit.collider.gameObject;
                    }
                }
                else
                {
                    if (lastHitObject != null)
                    {
                        lastHitObject = null;
                        SharedState.LastHitObjectTag = "";
                        SharedState.LastHitObjectLayer = "";
                        SharedState.LastMachineInfo = "No object in sight.";
                        SharedState.LastInventoryDetails = null;
                    }
                }
            }
            else
            {
                if (SharedState.ShowDEBUG)
                {
                    Debug.LogWarning("PlayerInspectorLateUpdatePatch: playerCamera or collisionLayers is null.");
                }
            }
        }
    }
}
