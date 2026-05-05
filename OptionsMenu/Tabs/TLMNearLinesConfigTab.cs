using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Extensions.UI;

namespace TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMNearLinesConfigTab : UICustomControl, ITLMConfigOptionsTab
    {
        private UIComponent parent;

        public void ReloadData() { }

        public void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group7 = new(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel)group7.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)group7.Self).wrapLayout = true;
            ((UIScrollablePanel)group7.Self).width = 730;

            group7.AddLabel(Locale.Get("TLM_NEAR_LINES_CONFIG"));
            group7.AddSpace(15);

            group7.AddCheckbox(Locale.Get("TLM_NEAR_LINES_SHOW_IN_SERVICES_BUILDINGS"), TransportLinesManagerMod.ShowNearLinesPlop, ToggleShowNearLinesInCityServicesWorldInfoPanel);
            group7.AddCheckbox(Locale.Get("TLM_NEAR_LINES_SHOW_IN_ZONED_BUILDINGS"), TransportLinesManagerMod.ShowNearLinesGrow, ToggleShowNearLinesInZonedBuildingWorldInfoPanel);

        }
        private void ToggleShowNearLinesInCityServicesWorldInfoPanel(bool b) => TransportLinesManagerMod.ShowNearLinesPlop = b;

        private void ToggleShowNearLinesInZonedBuildingWorldInfoPanel(bool b) => TransportLinesManagerMod.ShowNearLinesGrow = b;


    }
}
