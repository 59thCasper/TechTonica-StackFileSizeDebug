using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace StackFilterSizes
{
    [BepInPlugin("com.casper.StackFilterSizes", "StackFilterSizes", "1.0.0.0")]
    public class InserterModPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("com.casper.StackFilterSizes");
            harmony.PatchAll();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Period))
            {
                SharedState.ShowGUI = !SharedState.ShowGUI;
            }
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                SharedState.ShowDEBUG = !SharedState.ShowDEBUG;
            }
        }

        private void OnGUI()
        {
            if (SharedState.ShowGUI)
            {
                float windowWidth = 640;
                float windowHeight = 660;
                float centerX = (Screen.width - windowWidth) / 2;
                float centerY = (Screen.height - windowHeight) / 2;

                GUIStyle titleStyle = new GUIStyle(GUI.skin.window)
                {
                    fontSize = 18, 
                    alignment = TextAnchor.UpperCenter 
                };

                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 16 
                };

                GUI.Window(0, new Rect(centerX, centerY, windowWidth, windowHeight), DrawWindow, "Inserter Stack Size", titleStyle);

                DisplayModStatusLabel();
            }

            if (SharedState.ShowDEBUG)
            {
                DisplayMachineDebugInfo();
                DisplayInventoryInfo();
            }
        }

        void DrawWindow(int windowID)
        {
            if (GUILayout.Button("4", GUILayout.Height(50))) SharedState.InserterValue = 4; 
            if (GUILayout.Button("8", GUILayout.Height(50))) SharedState.InserterValue = 8;
            if (GUILayout.Button("12", GUILayout.Height(50))) SharedState.InserterValue = 12;
            if (GUILayout.Button("16", GUILayout.Height(50))) SharedState.InserterValue = 16;
            if (GUILayout.Button("20", GUILayout.Height(50))) SharedState.InserterValue = 20; 
            if (GUILayout.Button("40", GUILayout.Height(50))) SharedState.InserterValue = 40;
            if (GUILayout.Button("100", GUILayout.Height(50))) SharedState.InserterValue = 100;
            if (GUILayout.Button("200", GUILayout.Height(50))) SharedState.InserterValue = 200;
            if (GUILayout.Button("400", GUILayout.Height(50))) SharedState.InserterValue = 400;
            if (GUILayout.Button("500", GUILayout.Height(50))) SharedState.InserterValue = 500;
            if (GUILayout.Button("Toggle Mod On/Off", GUILayout.Height(50))) SharedState.PatchEnabled = !SharedState.PatchEnabled;
        }
        void DisplayModStatusLabel()
        {

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24, 
                fontStyle = FontStyle.Bold, 
                normal = { textColor = Color.white } 
            };

     
            Rect labelPosition = new Rect(10, Screen.height - 530, 300, 30); 

           
            string modStatusText = SharedState.PatchEnabled ? "Mod is On" : "Mod is Off";
            GUI.Label(labelPosition, modStatusText, labelStyle);
        }

        void DisplayMachineDebugInfo()
        {

            GUIStyle debugStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 24, 
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true 
            };

            float debugWidth = 650;
            float debugHeight = 250;
            Rect debugRect = new Rect(Screen.width - debugWidth - 500, Screen.height - debugHeight - 600, debugWidth, debugHeight);


            string debugContent = $"Hit Object Tag:   {SharedState.LastHitObjectTag}\n\n" +
                                  $"Layer:   {SharedState.LastHitObjectLayer}\n\n" +
                                  $"{SharedState.LastMachineInfo}";

            GUI.Box(debugRect, debugContent, debugStyle);
        }

        void DisplayInventoryInfo()
        {
            GUIStyle inventoryStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 20,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            };

            float inventoryWidth = 500;
            float inventoryHeight = 1000; 

            float debugWidth = 650;
            float debugXPosition = Screen.width - debugWidth - 500;

           
            float inventoryXPosition = debugXPosition + debugWidth + 20;
            float inventoryYPosition = Screen.height - inventoryHeight - 50;

           
            Rect inventoryRect = new Rect(inventoryXPosition, inventoryYPosition - 400, inventoryWidth, inventoryHeight);

            StringBuilder inventoryContentBuilder = new StringBuilder("Inventory Details:\n");

            if (SharedState.LastInventoryDetails != null)
            {
                // Assuming each unique item ID represents a stack and occupies one slot
                int totalUniqueValidItemIDs = SharedState.LastInventoryDetails.ItemStacks
                    .Where(item => item.ItemID > 0)
                    .Select(item => item.ItemID)
                    .Distinct()
                    .Count();

                int availableSlots = SharedState.LastInventoryDetails.NumSlots - totalUniqueValidItemIDs;

                inventoryContentBuilder.Append($"Num Slots: {SharedState.LastInventoryDetails.NumSlots}\n");
                inventoryContentBuilder.Append($"Available Slots: {availableSlots}\n\n"); // Displaying available slots

                var groupedItems = SharedState.LastInventoryDetails.ItemStacks
                    .Where(item => item.ItemID > 0) // Filter for valid items
                    .GroupBy(item => item.ItemID)
                    .Select(group =>
                    {
                        var resInfo = SaveState.GetResInfoFromId(group.Key); // Fetch display name using GetResInfoFromId
                        string itemName = resInfo != null ? resInfo.displayName : $"Item ID: {group.Key}";
                        int totalCount = group.Sum(item => item.Count);

                        return new { ItemID = group.Key, TotalCount = totalCount, ItemName = itemName };
                    });

                foreach (var item in groupedItems)
                {
                    // Now appending the display names with total counts
                    inventoryContentBuilder.AppendLine($"{item.ItemName}: Total Count: {item.TotalCount}");
                }
            }
            else
            {
                inventoryContentBuilder.Append("No inventory details available.");
            }

            GUI.Box(inventoryRect, inventoryContentBuilder.ToString(), inventoryStyle);
        }


    }

    public static class SharedState
    {
        public static bool ShowGUI = false;
        public static bool ShowDEBUG = false;
        public static int InserterValue = 100;
        public static bool PatchEnabled = true;
        public static bool FuelPatchEnabled = false;
        public static string LastHitObjectTag = "";
        public static string LastHitObjectLayer = "";
        public static string LastMachineInfo = "";

        public class InventoryDetails
        {
            public int NumSlots { get; set; }
            public List<ItemStack> ItemStacks { get; set; } = new List<ItemStack>();

            public int AvailableSlots => NumSlots - ItemStacks.Count;
        }

        public class ItemStack
        {
            public int ItemID { get; set; }
            public int Count { get; set; }
            public int MaxStack { get; set; }
        }

        public static InventoryDetails LastInventoryDetails { get; set; }
    }


}
