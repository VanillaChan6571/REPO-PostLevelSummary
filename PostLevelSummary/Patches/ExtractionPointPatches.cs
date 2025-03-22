using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using TMPro;

namespace PostLevelSummary.Patches
{
    [HarmonyPatch(typeof(ExtractionPoint))]
    class ExtractionPointPatches
    {
        [HarmonyPatch("StateComplete")]
        [HarmonyPostfix]
        public static void StateCompletePostfix(ExtractionPoint __instance)
        {
            if (PostLevelSummary.InGame)
            {
                // Get the total extracted value from the game
                float extractedValue = 0f;
                try
                {
                    // Try to get the extraction value from the game's UI
                    var taxHaulObject = GameObject.Find("Tax Haul");
                    if (taxHaulObject != null)
                    {
                        var taxHaulText = taxHaulObject.GetComponent<TMPro.TMP_Text>();
                        if (taxHaulText != null)
                        {
                            string taxText = taxHaulText.text;
                            // Parse the tax value from UI (typically formatted like "$19K")
                            if (taxText.StartsWith("$"))
                            {
                                taxText = taxText.Substring(1); // Remove the $ sign
                                if (taxText.EndsWith("K"))
                                {
                                    // Handle thousands format (like "19K")
                                    taxText = taxText.Substring(0, taxText.Length - 1);
                                    float value = float.Parse(taxText) * 1000f;
                                    extractedValue = value;
                                }
                                else
                                {
                                    // Handle regular number format
                                    float value = float.Parse(taxText);
                                    extractedValue = value;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    PostLevelSummary.Logger.LogError($"Error parsing extraction value: {ex.Message}");
                }

                // Calculate extraction values based on item tracking
                PostLevelSummary.Level.Extracted();

                // If we got a value from the UI and our calculated value is significantly different, use the UI value
                if (extractedValue > 0 && Math.Abs(PostLevelSummary.Level.ExtractedValue - extractedValue) > 100)
                {
                    PostLevelSummary.Logger.LogDebug(
                        $"Updating extraction value from UI: ${extractedValue} (was ${PostLevelSummary.Level.ExtractedValue})");

                    // Update the mod's extraction value to match the game's value
                    PostLevelSummary.Level.ExtractedValue = extractedValue;

                    // Estimate extracted items if we didn't track any
                    if (PostLevelSummary.Level.ExtractedItems == 0 && PostLevelSummary.Level.TotalItems > 0)
                    {
                        // Estimate based on value proportion
                        float proportion = extractedValue / PostLevelSummary.Level.TotalValue;
                        PostLevelSummary.Level.ExtractedItems = Math.Min(
                            (int)Math.Round(PostLevelSummary.Level.TotalItems * proportion),
                            PostLevelSummary.Level.TotalItems
                        );
                        PostLevelSummary.Logger.LogDebug(
                            $"Estimated extracted items: {PostLevelSummary.Level.ExtractedItems}");
                    }
                }
            }
        }
    }
}
