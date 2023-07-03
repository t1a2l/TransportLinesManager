using System;

namespace TransportLinesManager.WorldInfoPanels.Tabs
{
    public interface IUVMPTWIPChild
    {
        void UpdateBindings();
        void OnEnable();
        void OnDisable();
        void OnSetTarget(Type source);
        void OnGotFocus();
        bool MayBeVisible();
        void Hide();
    }
}