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
            TransportSystemDefinition def;
            switch (idx)
            {
                case 0:
                    def = TransportSystemDefinition.BUS;
                    break;
                case 1:
                    def = TransportSystemDefinition.TROLLEY;
                    break;
                case 2:
                    def = TransportSystemDefinition.TRAM;
                    break;
                case 3:
                    def = TransportSystemDefinition.METRO;
                    break;
                case 4:
                    def = TransportSystemDefinition.TRAIN;
                    break;
                case 5:
                    def = TransportSystemDefinition.FERRY;
                    break;
                case 6:
                    def = TransportSystemDefinition.BLIMP;
                    break;
                case 7:
                    def = TransportSystemDefinition.MONORAIL;
                    break;
                case 9:
                    def = TransportSystemDefinition.TOUR_PED;
                    break;
                case 10:
                    def = TransportSystemDefinition.TOUR_BUS;
                    break;
                default:
                    def = TransportSystemDefinition.BUS;
                    break;
            }

            TLMPanel.Instance?.OpenAt(def);
            return false;
        }
	}
}
