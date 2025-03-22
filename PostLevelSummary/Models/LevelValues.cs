using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Unity.VisualScripting;

namespace PostLevelSummary.Models
{
    public class LevelValues
    {
        public int TotalItems = 0;
        public float TotalValue = 0f;
        public List<ValuableObject> Valuables = new();
        public List<ValuableValue> ValuableValues = new();

        public int ItemsHit = 0;
        public float TotalValueLost = 0f;
        public int ItemsBroken = 0;
        public float ExtractedValue = 0f;
        public int ExtractedItems = 0;

        public int ItemCount
        {
            get { return Valuables.Count; }
        }

        public void Clear()
        {
            PostLevelSummary.Logger.LogDebug("Clearing level values!");

            TotalItems = 0;
            TotalValue = 0f;
            Valuables.Clear();
            ValuableValues.Clear();

            ItemsHit = 0;
            TotalValueLost = 0f;
            ItemsBroken = 0;
        }

        public void AddValuable(ValuableObject val)
        {
            TotalItems += 1;
            TotalValue += val.dollarValueOriginal;
            Valuables.Add(val);
            ValuableValues.Add(new ValuableValue
            {
                InstanceId = val.GetInstanceID(),
                Value = val.dollarValueOriginal
            });

            PostLevelSummary.Logger.LogDebug($"Created Valuable Object! {val.name} Val: {val.dollarValueOriginal}");
        }

        public void CheckValueChange(ValuableObject val)
        {
            if (val == null) return;

            ValuableValue vv = ValuableValues.Find(v => v.InstanceId == val.GetInstanceID());

            if (vv == null) return;

            if (vv.Value != val.dollarValueCurrent)
            {
                var lostValue = vv.Value - val.dollarValueCurrent;
                PostLevelSummary.Logger.LogDebug($"{val.name} lost {lostValue} value!");

                ItemsHit = ItemsHit + 1;
                TotalValueLost = TotalValueLost + lostValue;
                vv.Value = val.dollarValueCurrent;
            }
        }

// Modify ItemBroken method for better handling
        public void ItemBroken(ValuableObject val)
        {
            if (val == null) return;
            if (val.dollarValueCurrent != 0f) return;

            // First run cleanup to ensure lists are consistent
            CleanupDestroyedObjects();

            ValuableValue vv = ValuableValues.Find(v => v.InstanceId == val.GetInstanceID());

            // Add null check to prevent NullReferenceException
            if (vv == null)
            {
                PostLevelSummary.Logger.LogDebug(
                    $"Couldn't find valuable value for {val.name} with ID {val.GetInstanceID()}");
                PostLevelSummary.Logger.LogDebug(
                    "If you are getting this debug message, something has gone very horribly!");
                return;
            }

            var lostValue = vv.Value - val.dollarValueCurrent;
            PostLevelSummary.Logger.LogDebug($"Broken {val.name}!");
            ItemsHit = ItemsHit + 1;
            TotalValueLost = TotalValueLost + lostValue;
            ItemsBroken += 1;

            ValuableValues.Remove(vv);
        }

        public void CleanupDestroyedObjects()
        {
            try
            {
                int beforeCount = Valuables.Count;

                // Remove valuables that have been destroyed or have null references
                Valuables.RemoveAll(v => v == null || v.IsDestroyed());

                // Update ValuableValues to match only existing Valuables
                var existingIds = Valuables.Where(v => v != null && !v.IsDestroyed())
                    .Select(v => v.GetInstanceID())
                    .ToList();

                int removedValues = ValuableValues.RemoveAll(vv => !existingIds.Contains(vv.InstanceId));

                if (beforeCount != Valuables.Count || removedValues > 0)
                {
                    PostLevelSummary.Logger.LogDebug(
                        $"Cleaned up {beforeCount - Valuables.Count} destroyed valuable objects and {removedValues} value entries");
                }
            }
            catch (Exception ex)
            {
                PostLevelSummary.Logger.LogError($"Error during cleanup: {ex.Message}");
            }
        }

// Update Extracted method to be more robust
        public void Extracted()
        {
            try
            {
                // First clean up any ghost objects
                CleanupDestroyedObjects();

                // Check if any items have been extracted by comparing current objects with our tracking list
                int beforeCount = ValuableValues.Count;

                // Get the IDs of valuable objects that still exist in the scene
                HashSet<int> existingIds = new HashSet<int>();
                foreach (var valuable in Valuables)
                {
                    if (valuable != null && !valuable.IsDestroyed())
                    {
                        existingIds.Add(valuable.GetInstanceID());
                    }
                }

                // Find valuable values that are no longer in the scene (these were extracted)
                List<ValuableValue> extractedValues = new List<ValuableValue>();
                foreach (var value in ValuableValues.ToList())
                {
                    if (!existingIds.Contains(value.InstanceId))
                    {
                        extractedValues.Add(value);
                    }
                }

                // Update extracted metrics
                if (extractedValues.Count > 0)
                {
                    float totalExtractedValue = 0f;
                    foreach (var value in extractedValues)
                    {
                        totalExtractedValue += value.Value;
                        ValuableValues.Remove(value);
                    }

                    ExtractedValue += totalExtractedValue;
                    ExtractedItems += extractedValues.Count;

                    PostLevelSummary.Logger.LogDebug(
                        $"Extracted {extractedValues.Count} items worth ${totalExtractedValue}");
                }
                else
                {
                    PostLevelSummary.Logger.LogDebug("No items detected as extracted (comparison method)");

                    // Alternative detection: if our valuable list has items missing but we didn't detect extractions
                    if (beforeCount > ValuableValues.Count)
                    {
                        int missingCount = beforeCount - ValuableValues.Count;
                        PostLevelSummary.Logger.LogDebug(
                            $"Detected {missingCount} missing valuables, considering them extracted");

                        // Estimate extracted value as a proportion of total value
                        float estimatedValue = (TotalValue / TotalItems) * missingCount;
                        ExtractedValue += estimatedValue;
                        ExtractedItems += missingCount;
                    }
                }
            }
            catch (Exception ex)
            {
                PostLevelSummary.Logger.LogError($"Error during extraction calculation: {ex.Message}");
            }
        }
    }
}