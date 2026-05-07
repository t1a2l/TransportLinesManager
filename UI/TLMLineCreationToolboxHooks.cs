using HarmonyLib;
using ColossalFramework.UI;
using UnityEngine;
using static Commons.Extensions.Patcher;

namespace TransportLinesManager.UI
{
    [HarmonyPatch(typeof(GeneratedScrollPanel))]
    internal class TLMLineCreationToolboxHooks : MonoBehaviour, IPatcher
    {
        [HarmonyPatch(typeof(GeneratedScrollPanel), "OnClick")]
        [HarmonyPrefix]
#pragma warning disable IDE0060 // Remove unused parameter
        public static void PreOnClick(GeneratedScrollPanel __instance, UIComponent comp, UIMouseEventParameter p, ref TransportInfo __state)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            TLMLineCreationToolbox.OnButtonClickedPre(ref __state);
		}

        [HarmonyPatch(typeof(GeneratedScrollPanel), "OnClick")]
        [HarmonyPostfix]
#pragma warning disable IDE0060 // Remove unused parameter
        public static void AfterOnClick(GeneratedScrollPanel __instance, UIComponent comp, UIMouseEventParameter p, ref TransportInfo __state)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            TLMLineCreationToolbox.OnButtonClickedPos(ref __state);
		}

    }
}
