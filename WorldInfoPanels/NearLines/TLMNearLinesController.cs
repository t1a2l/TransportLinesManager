using ColossalFramework;
using ColossalFramework.UI;
using Commons.Utils;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Cache.BuildingData;
using TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.NearLines
{
    public class TLMNearLinesController : UICustomControl
    {
        private UIPanel m_containerParent;
        private UIPanel m_localContainer;
        private UILabel m_title;
        private UIPanel m_listContainer;
        private UITemplateList<UIButton> m_localLinesTemplateList;
        private UIPanel m_regionalContainer;
        private UILabel m_regTitle;
        private UIPanel m_regListContainer;
        private UITemplateList<UIButton> m_regionalLinesTemplateList;
        private ushort lastBuildingSelected = 0;

        private bool m_dirty = true;
        private bool m_mayShow = true;


        internal static TLMNearLinesController InitPanelNearLinesOnWorldInfoPanel(UIComponent parent)
        {
            MonoUtils.CreateUIElement(out UIPanel panel, parent.transform);
            return panel.gameObject.AddComponent<TLMNearLinesController>();
        }

        public void Awake()
        {
            m_containerParent = GetComponent<UIPanel>();
            m_containerParent.backgroundSprite = "GenericPanelDark";


            m_containerParent.autoFitChildrenVertically = true;
            m_containerParent.autoLayout = true;
            m_containerParent.autoLayoutDirection = LayoutDirection.Vertical;
            m_containerParent.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_containerParent.padding = new RectOffset(2, 2, 2, 2);
            m_containerParent.autoLayoutStart = LayoutStart.TopLeft;
            m_containerParent.name = "TLMLinesNear";
            m_containerParent.width = 300;
            m_containerParent.padding.top = 5;
            m_containerParent.padding.bottom = 5;


            MonoUtils.CreateUIElement(out m_localContainer, m_containerParent.transform);
            m_localContainer.width = m_containerParent.width;
            m_localContainer.autoFitChildrenVertically = true;
            m_localContainer.autoLayout = true;
            m_localContainer.autoLayoutDirection = LayoutDirection.Vertical;
            m_localContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_localContainer.padding = new RectOffset(2, 2, 2, 2);
            m_localContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_localContainer.name = "TLMLinesNearRegional";

            MonoUtils.CreateUIElement(out m_title, m_localContainer.transform);
            m_title.autoSize = false;
            m_title.width = m_localContainer.width;
            m_title.textAlignment = UIHorizontalAlignment.Left;
            m_title.localeID = "TLM_NEAR_LINES";
            m_title.useOutline = true;
            m_title.height = 18;

            MonoUtils.CreateUIElement(out m_listContainer, m_localContainer.transform);
            m_listContainer.width = m_localContainer.width;
            m_listContainer.autoFitChildrenVertically = true;
            m_listContainer.autoLayout = true;
            m_listContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            m_listContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_listContainer.padding = new RectOffset(2, 2, 2, 2);
            m_listContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_listContainer.wrapLayout = true;
            m_listContainer.name = "TLMLinesNearList";
            TLMLineItemButtonControl.EnsureTemplate();
            m_localLinesTemplateList = new UITemplateList<UIButton>(m_listContainer, TLMLineItemButtonControl.LINE_ITEM_TEMPLATE);


            MonoUtils.CreateUIElement(out m_regionalContainer, m_containerParent.transform);
            m_regionalContainer.width = m_containerParent.width;
            m_regionalContainer.autoFitChildrenVertically = true;
            m_regionalContainer.autoLayout = true;
            m_regionalContainer.autoLayoutDirection = LayoutDirection.Vertical;
            m_regionalContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_regionalContainer.padding = new RectOffset(2, 2, 2, 2);
            m_regionalContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_regionalContainer.name = "TLMLinesNearRegional";

            MonoUtils.CreateUIElement(out m_regTitle, m_regionalContainer.transform);
            m_regTitle.autoSize = false;
            m_regTitle.width = m_regionalContainer.width;
            m_regTitle.textAlignment = UIHorizontalAlignment.Left;
            m_regTitle.localeID = "TLM_NEAR_LINES_REGIONAL";
            m_regTitle.useOutline = true;
            m_regTitle.height = 18;


            MonoUtils.CreateUIElement(out m_regListContainer, m_regionalContainer.transform);
            m_regListContainer.width = m_regionalContainer.width;
            m_regListContainer.autoFitChildrenVertically = true;
            m_regListContainer.autoLayout = true;
            m_regListContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            m_regListContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_regListContainer.padding = new RectOffset(2, 2, 2, 2);
            m_regListContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_regListContainer.wrapLayout = true;
            m_regListContainer.name = "TLMLinesNearListRegional";
            m_regionalLinesTemplateList = new UITemplateList<UIButton>(m_regListContainer, TLMLineItemButtonControl.LINE_ITEM_TEMPLATE);
        }
        internal void EventWIPChanged(bool isGrow)
        {
            m_dirty = true;
            m_mayShow = isGrow ? TransportLinesManagerMod.ShowNearLinesGrow : TransportLinesManagerMod.ShowNearLinesPlop;
        }

        private void Update()
        {
            if (m_dirty && component.parent.isVisible)
            {
                UpdateNearLines(m_mayShow, true);
                m_dirty = false;
            }
        }

        private void UpdateNearLines(bool show, bool force = false)
        {
            m_containerParent.isVisible = show;
            if (!show)
            {
                return;
            }
            ushort buildingId = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (lastBuildingSelected == buildingId && !force)
            {
                return;
            }
            else
            {
                lastBuildingSelected = buildingId;
            }
            ref Building b = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];

            var nearLines = new List<ushort>();
            Vector3 sidewalk = b.CalculateSidewalkPosition();
            TLMLineUtils.GetNearLines(sidewalk, 120f, ref nearLines);
            bool showLocal = nearLines.Count > 0;
            if (showLocal)
            {
                var localLines = TLMLineUtils.SortLines(nearLines.Select(x => Tuple.New(x, false)).ToList()).Values.ToArray();
                var itemsEntries = m_localLinesTemplateList.SetItemCount(localLines.Length);
                for (int idx = 0; idx < localLines.Length; idx++)
                {
                    ushort lineId = localLines[idx].First;
                    var itemControl = itemsEntries[idx].GetComponent<TLMLineItemButtonControl>();
                    itemControl.ResetData(false, lineId, sidewalk);
                }
            }

            var showRegional = false;
            if (TransportLinesManagerMod.Controller.BuildingLines.SafeGet(buildingId) is BuildingTransportDataCache btdc)
            {
                var regionalLines = btdc.RegionalLines.Keys.ToArray();
                showRegional = regionalLines != null && regionalLines.Length > 0;
                if (showRegional)
                {
                    var itemsEntries = m_regionalLinesTemplateList.SetItemCount(regionalLines.Length);
                    for (ushort idx = 0; idx < regionalLines.Length; idx++)
                    {
                        var itemControl = itemsEntries[idx].GetComponent<TLMLineItemButtonControl>();
                        itemControl.ResetData(true, (ushort)regionalLines[idx], sidewalk);
                    }
                }
            }

            m_localContainer.isVisible = showLocal;
            m_regionalContainer.isVisible = showRegional;
            m_containerParent.isVisible = showLocal || showRegional;
        }
    }
}
