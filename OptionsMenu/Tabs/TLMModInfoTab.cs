using ColossalFramework.UI;
using Commons.Extensions.UI;

namespace TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMModInfoTab : UICustomControl, ITLMConfigOptionsTab
    {
        private UIComponent parent;

        public void ReloadData() { }

        public void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group9 = new(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel)group9.Self).wrapLayout = true;
            ((UIScrollablePanel)group9.Self).width = 700;
            TransportLinesManagerMod.Instance.PopulateGroup9(group9);
        }

    }
}
