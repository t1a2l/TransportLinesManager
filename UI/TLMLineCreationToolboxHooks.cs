using HarmonyLib;
using ColossalFramework.UI;
using UnityEngine;
using static Commons.Extensions.Patcher;

namespace TransportLinesManager.UI
{
    internal class TLMLineCreationToolboxHooks : MonoBehaviour, IPatcher
    {

        [HarmonyPatch(typeof(GeneratedScrollPanel), "OnClick")]
        [HarmonyPrefix]
        public static void PreOnClick(GeneratedScrollPanel __instance, UIComponent comp, UIMouseEventParameter p, ref TransportInfo __state)
		{
            TLMLineCreationToolbox.OnButtonClickedPre(ref __state);
		}

        [HarmonyPatch(typeof(GeneratedScrollPanel), "OnClick")]
        [HarmonyPostfix]
        public static void AfterOnClick(GeneratedScrollPanel __instance, UIComponent comp, UIMouseEventParameter p, ref TransportInfo __state)
		{
            TLMLineCreationToolbox.OnButtonClickedPos(ref __state);
		}

    }
}
