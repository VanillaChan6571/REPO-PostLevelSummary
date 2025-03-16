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

        public int ItemCount { get {  return Valuables.Count; } }

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
            ValuableValue vv = ValuableValues.Find(v => v.InstanceId == val.GetInstanceID());

            if (vv.Value != val.dollarValueCurrent)
            {
                var lostValue = vv.Value - val.dollarValueCurrent;
                PostLevelSummary.Logger.LogDebug($"{val.name} lost {lostValue} value!");

                ItemsHit = ItemsHit + 1;
                TotalValueLost = TotalValueLost + lostValue;
                vv.Value = val.dollarValueCurrent;
            }
        }

        public void ItemBroken(ValuableObject val)
        {
            if (val.dollarValueCurrent != 0f) return;

            ValuableValue vv = ValuableValues.Find(v => v.InstanceId == val.GetInstanceID());

            var lostValue = vv.Value - val.dollarValueCurrent;
            PostLevelSummary.Logger.LogDebug($"Broken {val.name}!");
            ItemsHit = ItemsHit + 1;
            TotalValueLost = TotalValueLost + lostValue;
            ItemsBroken += 1;

            ValuableValues.Remove(vv);
        }

        public void Extracted()
        {
            if (Valuables.Any(v => v.IsDestroyed()))
            {
                var existing = Valuables.FindAll(v => v.GetInstanceID() != 0).Select(v => v.GetInstanceID());
                var extracted = ValuableValues.FindAll(v => !existing.Any(id => id == v.InstanceId));

                ExtractedValue += extracted.Select(v => v.Value).Sum();
                ExtractedItems += extracted.Count;

                Valuables.RemoveAll(v => v.GetInstanceID() == 0);
                ValuableValues.RemoveAll(v => !existing.Any(id => id == v.InstanceId));
            }
        }
    }
}
