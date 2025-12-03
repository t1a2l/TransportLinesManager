using TransportLinesManager.CommonsWindow;
using TransportLinesManager.UI;
using HarmonyLib;
using TransportLinesManager.Data.Tsd;

namespace TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(PublicTransportInfoViewPanel))]
    internal static class PublicTransportInfoViewPanelOverrides
    {

        public static TLMLineCreationToolbox Toolbox { get; private set; }

        [HarmonyPatch(typeof(PublicTransportInfoViewPanel), "OpenDetailPanel")]
        [HarmonyPrefix]
        public static bool OpenDetailPanel(int idx)
        {
            TransportSystemDefinition def = idx switch
            {
                0 => TransportSystemDefinition.BUS,
                1 => TransportSystemDefinition.TROLLEY,
                2 => TransportSystemDefinition.TRAM,
                3 => TransportSystemDefinition.METRO,
                4 => TransportSystemDefinition.TRAIN,
                5 => TransportSystemDefinition.FERRY,
                6 => TransportSystemDefinition.BLIMP,
                7 => TransportSystemDefinition.MONORAIL,
                9 => TransportSystemDefinition.TOUR_PED,
                10 => TransportSystemDefinition.TOUR_BUS,
                _ => TransportSystemDefinition.BUS,
            };
            TLMPanel.Instance?.OpenAt(def);
            return false;
        }

        [HarmonyPatch(typeof(PublicTransportInfoViewPanel), "OpenDetailPanelDefaultTab")]
        [HarmonyPrefix]
        public static bool OpenDetailPanelDefaultTab()
        {
            OpenDetailPanel(0);
            return false;
        }

        [HarmonyPatch(typeof(PublicTransportInfoViewPanel), "Start")]
        [HarmonyPrefix]
        public static void AfterAwake(PublicTransportInfoViewPanel __instance) => Toolbox = __instance.gameObject.AddComponent<TLMLineCreationToolbox>();

    }

}
