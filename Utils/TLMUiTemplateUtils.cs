using ColossalFramework.UI;
using Commons.Extensions;
using System.Collections.Generic;

namespace TransportLinesManager.Utils
{
    internal class TLMUiTemplateUtils
    {
        public static Dictionary<string, UIComponent> GetTemplateDict() => (Dictionary<string, UIComponent>) typeof(UITemplateManager).GetField("m_Templates", Patcher.allFlags).GetValue(UITemplateManager.instance);
    }

}

