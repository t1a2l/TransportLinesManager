using HarmonyLib;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Data.Base;
using Klyte.TransportLinesManager.Data.TsdImplementations;

namespace Klyte.TransportLinesManager.Overrides
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
                    def = TransportSystemDefinitionType.BUS;
                    break;
                case 1:
                    def = TransportSystemDefinitionType.TROLLEY;
                    break;
                case 2:
                    def = TransportSystemDefinitionType.TRAM;
                    break;
                case 3:
                    def = TransportSystemDefinitionType.METRO;
                    break;
                case 4:
                    def = TransportSystemDefinitionType.TRAIN;
                    break;
                case 5:
                    def = TransportSystemDefinitionType.FERRY;
                    break;
                case 6:
                    def = TransportSystemDefinitionType.BLIMP;
                    break;
                case 7:
                    def = TransportSystemDefinitionType.MONORAIL;
                    break;
                case 9:
                    def = TransportSystemDefinitionType.TOUR_PED;
                    break;
                case 10:
                    def = TransportSystemDefinitionType.TOUR_BUS;
                    break;
                default:
                    def = TransportSystemDefinitionType.BUS;
                    break;
            }

            TLMPanel.Instance?.OpenAt(def);
            return false;
        }
	}
}
