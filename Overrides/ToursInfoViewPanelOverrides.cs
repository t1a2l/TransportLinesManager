using HarmonyLib;
using TransportLinesManager.CommonsWindow;
using TransportLinesManager.Data.Tsd;

namespace TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(ToursInfoViewPanel))]
	public static class ToursInfoViewPanelOverrides 
	{
		[HarmonyPatch(typeof(ToursInfoViewPanel), "OpenDetailPanel")]
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
	}
}
