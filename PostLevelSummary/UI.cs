using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using PostLevelSummary.Helpers;

namespace PostLevelSummary
{
    public static class UI
    {
        public static void Init()
        {
            GameObject hud = GameObject.Find("Game Hud");
            GameObject haul = GameObject.Find("Tax Haul");

            TMP_FontAsset font = haul.GetComponent<TMP_Text>().font;

            PostLevelSummary.TextInstance = new GameObject();
            PostLevelSummary.TextInstance.SetActive(false);
            PostLevelSummary.TextInstance.name = "Summary HUD";
            PostLevelSummary.TextInstance.AddComponent<TextMeshProUGUI>();

            PostLevelSummary.ValueText = PostLevelSummary.TextInstance.GetComponent<TextMeshProUGUI>();
            PostLevelSummary.ValueText.font = font;
            PostLevelSummary.ValueText.color = new Vector4(0.7882f, 0.9137f, 0.902f, 1);
            PostLevelSummary.ValueText.fontSize = 20f;
            PostLevelSummary.ValueText.enableWordWrapping = false;
            PostLevelSummary.ValueText.alignment = TextAlignmentOptions.BaselineRight;
            PostLevelSummary.ValueText.horizontalAlignment = HorizontalAlignmentOptions.Right;
            PostLevelSummary.ValueText.verticalAlignment = VerticalAlignmentOptions.Baseline;

            PostLevelSummary.TextInstance.transform.SetParent(hud.transform, false);

            RectTransform component = PostLevelSummary.TextInstance.GetComponent<RectTransform>();

            component.pivot = new Vector2(1f, 1f);
            component.anchoredPosition = new Vector2(1f, -1f);
            component.anchorMin = new Vector2(0f, 0f);
            component.anchorMax = new Vector2(1f, 0f);
            component.sizeDelta = new Vector2(0f, 0f);
            component.offsetMax = new Vector2(0, 225f);
            component.offsetMin = new Vector2(0f, 225f);

            PostLevelSummary.Logger.LogDebug("HUD generated");
        }
        public static void Update()
        {
            RectTransform component = PostLevelSummary.TextInstance.GetComponent<RectTransform>();

            component.pivot = new Vector2(1f, 1f);
            component.anchoredPosition = new Vector2(1f, -1f);
            component.anchorMin = new Vector2(0f, 0f);
            component.anchorMax = new Vector2(1f, 0f);
            component.sizeDelta = new Vector2(0f, 0f);
            component.offsetMax = new Vector2(0, 225f);
            component.offsetMin = new Vector2(0f, 225f);

            PostLevelSummary.ValueText.lineSpacing = -50f;

            PostLevelSummary.ValueText.SetText($@"
                    Extracted ${NumberFormatter.FormatToK(PostLevelSummary.Level.ExtractedValue)} out of ${NumberFormatter.FormatToK(PostLevelSummary.Level.TotalValue)}
                    {PostLevelSummary.Level.ExtractedItems} items out of {PostLevelSummary.Level.TotalItems}

                    Lost ${NumberFormatter.FormatToK(PostLevelSummary.Level.TotalValueLost)} in value
                    {PostLevelSummary.Level.ItemsBroken} {string.Format("item{0}", PostLevelSummary.Level.ItemsBroken == 1 ? "" : "s")} broken ({PostLevelSummary.Level.ItemsHit} hits)
                ");
        }
    }
}
