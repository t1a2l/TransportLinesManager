using ColossalFramework;
using ColossalFramework.UI;
using Commons.Utils;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Cache.BuildingData;
using TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace TransportLinesManager.WorldInfoPanels.NearLines
{
    public class TLMNearLinesController : UICustomControl
    {
        private UIPanel m_containerParent;
        private UIPanel m_localContainer;
        private UILabel m_title;
        private UIScrollablePanel m_localListContainer;
        private UIScrollbar m_localListScrollbar;
        private UITemplateList<TLMNearLineRowControl> m_localLinesTemplateList;
        private UIPanel m_regionalContainer;
        private UILabel m_regTitle;
        private UIScrollablePanel m_regListContainer;
        private UIScrollbar m_regListScrollbar;
        private UITemplateList<TLMNearLineRowControl> m_regionalLinesTemplateList;
        private ushort lastBuildingSelected = 0;

        private bool m_dirty = true;
        private bool m_mayShow = true;

        private const float LIST_WIDTH = 285f;
        private const float LIST_HEIGHT = 176f;
        private const float SCROLLBAR_WIDTH = 10f;

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
            m_localContainer.relativePosition = Vector3.zero;
            m_localContainer.autoLayoutDirection = LayoutDirection.Vertical;
            m_localContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_localContainer.padding = new RectOffset(2, 2, 2, 2);
            m_localContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_localContainer.name = "TLMLinesNearLocal";

            MonoUtils.CreateUIElement(out m_title, m_localContainer.transform);
            m_title.autoSize = false;
            m_title.width = m_localContainer.width;
            m_title.textAlignment = UIHorizontalAlignment.Left;
            m_title.localeID = "TLM_NEAR_LINES";
            m_title.useOutline = true;
            m_title.height = 18;

            MonoUtils.CreateUIElement(out UIPanel localListRow, m_localContainer.transform);
            localListRow.width = m_localContainer.width;
            localListRow.height = (int)LIST_HEIGHT;
            localListRow.autoLayout = true;
            localListRow.autoLayoutDirection = LayoutDirection.Horizontal;
            localListRow.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            localListRow.padding = new RectOffset(0, 0, 0, 0);
            localListRow.wrapLayout = false;
            localListRow.name = "TLMLinesNearLocalListRow";

            MonoUtils.CreateUIElement(out m_localListContainer, localListRow.transform);
            m_localListContainer.width = LIST_WIDTH;
            m_localListContainer.height = LIST_HEIGHT;
            m_localListContainer.clipChildren = true;
            m_localListContainer.autoLayout = true;
            m_localListContainer.autoLayoutDirection = LayoutDirection.Vertical;
            m_localListContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_localListContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_localListContainer.wrapLayout = false;
            m_localListContainer.name = "TLMLinesNearLocalList";
            m_localListContainer.scrollWheelAmount = 42;

            MonoUtils.CreateUIElement(out UIPanel localScrollbarHost, localListRow.transform);
            localScrollbarHost.width = SCROLLBAR_WIDTH;
            localScrollbarHost.height = LIST_HEIGHT;
            localScrollbarHost.name = "TLMLinesNearLocalListScrollbarHost";

            MonoUtils.CreateUIElement(out m_localListScrollbar, localScrollbarHost.transform);
            m_localListScrollbar.width = SCROLLBAR_WIDTH;
            m_localListScrollbar.height = LIST_HEIGHT;
            m_localListScrollbar.orientation = UIOrientation.Vertical;
            m_localListScrollbar.minValue = 0f;
            m_localListScrollbar.value = 0f;
            m_localListScrollbar.relativePosition = new Vector3(-2f, 0f);
            m_localListScrollbar.incrementAmount = 42f;
            m_localListScrollbar.name = "TLMLinesNearLocalListScrollbar";

            MonoUtils.CreateUIElement(out UISlicedSprite localScrollBg, m_localListScrollbar.transform);
            localScrollBg.relativePosition = Vector2.zero;
            localScrollBg.autoSize = true;
            localScrollBg.size = m_localListScrollbar.size;
            localScrollBg.fillDirection = UIFillDirection.Vertical;
            localScrollBg.spriteName = "LocalScrollbarTrack";
            m_localListScrollbar.trackObject = localScrollBg;

            MonoUtils.CreateUIElement(out UISlicedSprite localScrollThumb, localScrollBg.transform);
            localScrollThumb.relativePosition = Vector2.zero;
            localScrollThumb.fillDirection = UIFillDirection.Vertical;
            localScrollThumb.autoSize = true;
            localScrollThumb.width = localScrollBg.width - 4f;
            localScrollThumb.spriteName = "LocalScrollbarThumb";
            m_localListScrollbar.thumbObject = localScrollThumb;

            m_localListContainer.verticalScrollbar = m_localListScrollbar;
            m_localListContainer.eventMouseWheel += (c, p) =>
            {
                m_localListContainer.scrollPosition = new Vector2(0f, m_localListContainer.scrollPosition.y + Mathf.Sign(p.wheelDelta) * -m_localListScrollbar.incrementAmount);
                p.Use();
            };

            TLMLineItemButtonControl.EnsureTemplate();
            TLMNearLineRowControl.EnsureTemplate();
            m_localLinesTemplateList = new UITemplateList<TLMNearLineRowControl>(m_localListContainer, TLMNearLineRowControl.ROW_TEMPLATE);

            MonoUtils.CreateUIElement(out m_regionalContainer, m_containerParent.transform);
            m_regionalContainer.width = m_containerParent.width;
            m_regionalContainer.autoFitChildrenVertically = true;
            m_regionalContainer.autoLayout = true;
            m_regionalContainer.relativePosition = Vector3.zero;
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

            MonoUtils.CreateUIElement(out UIPanel regListRow, m_regionalContainer.transform);
            regListRow.width = m_regionalContainer.width;
            regListRow.height = (int)LIST_HEIGHT;
            regListRow.autoLayout = true;
            regListRow.autoLayoutDirection = LayoutDirection.Horizontal;
            regListRow.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            regListRow.padding = new RectOffset(0, 0, 0, 0);
            regListRow.wrapLayout = false;
            regListRow.name = "TLMLinesNearRegionalListRow";

            MonoUtils.CreateUIElement(out m_regListContainer, regListRow.transform);
            m_regListContainer.width = m_regionalContainer.width;
            m_regListContainer.autoLayout = true;
            m_regListContainer.autoLayoutDirection = LayoutDirection.Vertical;
            m_regListContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_regListContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_regListContainer.wrapLayout = false;
            m_regListContainer.name = "TLMLinesNearRegionalList";
            m_regionalLinesTemplateList = new UITemplateList<TLMNearLineRowControl>(m_regListContainer, TLMNearLineRowControl.ROW_TEMPLATE);

            MonoUtils.CreateUIElement(out UIPanel regScrollbarHost, regListRow.transform);
            regScrollbarHost.width = SCROLLBAR_WIDTH;
            regScrollbarHost.height = LIST_HEIGHT;
            regScrollbarHost.name = "TLMLinesNearRegionalListScrollbarHost";

            MonoUtils.CreateUIElement(out m_regListScrollbar, regScrollbarHost.transform);
            m_regListScrollbar.width = SCROLLBAR_WIDTH;
            m_regListScrollbar.height = LIST_HEIGHT;
            m_regListScrollbar.orientation = UIOrientation.Vertical;
            m_regListScrollbar.minValue = 0f;
            m_regListScrollbar.value = 0f;
            m_regListScrollbar.relativePosition = new Vector3(-2f, 0f);
            m_regListScrollbar.incrementAmount = 42f;
            m_regListScrollbar.name = "TLMLinesNearRegionalListScrollbar";

            MonoUtils.CreateUIElement(out UISlicedSprite regScrollBg, m_regListScrollbar.transform);
            regScrollBg.relativePosition = Vector2.zero;
            regScrollBg.autoSize = true;
            regScrollBg.size = m_regListScrollbar.size;
            regScrollBg.fillDirection = UIFillDirection.Vertical;
            regScrollBg.spriteName = "RegionalScrollbarTrack";
            m_regListScrollbar.trackObject = regScrollBg;

            MonoUtils.CreateUIElement(out UISlicedSprite regScrollThumb, regScrollBg.transform);
            regScrollThumb.relativePosition = Vector2.zero;
            regScrollThumb.fillDirection = UIFillDirection.Vertical;
            regScrollThumb.autoSize = true;
            regScrollThumb.width = localScrollBg.width - 4f;
            regScrollThumb.spriteName = "RegionalScrollbarThumb";
            m_regListScrollbar.thumbObject = regScrollThumb;

            m_regListContainer.verticalScrollbar = m_regListScrollbar;
            m_regListContainer.eventMouseWheel += (c, p) =>
            {
                m_regListContainer.scrollPosition = new Vector2(0f, m_regListContainer.scrollPosition.y + Mathf.Sign(p.wheelDelta) * -m_regListScrollbar.incrementAmount);
                p.Use();
            };
        }
        internal void EventWIPChanged(bool isGrow)
        {
            m_dirty = true;
            m_mayShow = isGrow ? TransportLinesManagerMod.ShowNearLinesGrow : TransportLinesManagerMod.ShowNearLinesPlop;
        }

        private void Update()
        {
            if (!m_dirty)
            {
                return;
            }

            if (component?.parent == null || !component.parent.isVisible)
            {
                return;
            }

            var current = WorldInfoPanel.GetCurrentInstanceID();
            if (current.Building == 0 && current.Type != (InstanceType)TLMInstanceType.BuildingLines)
            {
                return;
            }

            UpdateNearLines(m_mayShow, true);
            m_dirty = false;
        }

        private void UpdateNearLines(bool show, bool force = false)
        {
            if (m_containerParent == null || m_localLinesTemplateList == null || m_regionalLinesTemplateList == null)
            {
                return;
            }

            m_containerParent.isVisible = show;
            if (!show)
            {
                return;
            }

            ushort buildingId = WorldInfoPanel.GetCurrentInstanceID().Building;

            if (buildingId == 0)
            {
                m_localLinesTemplateList.SetItemCount(0);
                m_regionalLinesTemplateList.SetItemCount(0);
                m_localContainer.isVisible = false;
                m_regionalContainer.isVisible = false;
                m_containerParent.isVisible = false;
                return;
            }

            ushort previousBuilding = lastBuildingSelected;

            if (lastBuildingSelected == buildingId && !force)
            {
                return;
            }
            else
            {
                lastBuildingSelected = buildingId;
            }

            if (previousBuilding != buildingId)
            {
                m_localListContainer.scrollPosition = Vector2.zero;
                m_regListContainer.scrollPosition = Vector2.zero;
            }

            ref Building b = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];

            var nearLines = new List<ushort>();
            Vector3 sidewalk = b.CalculateSidewalkPosition();
            bool hasStationWaitData = TryGetStationLineWaitData(buildingId, out var waitingByLine);
            TLMLineUtils.GetNearLines(sidewalk, 120f, ref nearLines);
            bool showLocal = nearLines.Count > 0;
            if (showLocal)
            {
                var localLines = TLMLineUtils.SortLines([.. nearLines.Select(x => Tuple.New(x, false))]).Values.ToArray();
                var itemsEntries = m_localLinesTemplateList.SetItemCount(localLines.Length);
                for (int idx = 0; idx < localLines.Length; idx++)
                {
                    ushort lineId = localLines[idx].First;
                    string lineName = GetShortLineName(false, lineId);
                    int waiting = hasStationWaitData && waitingByLine.TryGetValue(lineId, out int count) ? count : -1;
                    itemsEntries[idx].ResetData(false, lineId, sidewalk, lineName, waiting);
                }
                m_localListContainer?.Invalidate();
                m_localListContainer?.PerformLayout();

            }
            else
            {
                m_localLinesTemplateList.SetItemCount(0);
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
                        ushort lineId = (ushort)regionalLines[idx];
                        string lineName = GetShortLineName(true, lineId);
                        int waiting = hasStationWaitData && waitingByLine.TryGetValue(lineId, out int count) ? count : -1;
                        itemsEntries[idx].ResetData(true, lineId, sidewalk, lineName, waiting);
                    }
                    m_regListContainer?.Invalidate();
                    m_regListContainer?.PerformLayout();
                }
                else
                {
                    m_regionalLinesTemplateList.SetItemCount(0);
                }
            }
            else
            {
                m_regionalLinesTemplateList.SetItemCount(0);
            }

            m_localContainer.isVisible = showLocal;
            m_regionalContainer.isVisible = showRegional;
            m_containerParent.isVisible = showLocal || showRegional;
        }

        private static string GetShortLineName(bool fromBuilding, ushort lineId)
        {
            string name = fromBuilding ? TLMLineUtils.GetLineStringId(lineId, true) : TransportManager.instance.GetLineName(lineId);

            if (name.IsNullOrWhiteSpace())
            {
                name = TLMLineUtils.GetLineStringId(lineId, fromBuilding);
            }

            const int maxLength = 24;
            return name != null && name.Length > maxLength ? name.Substring(0, maxLength - 1) + "…" : name ?? "";
        }

        private static bool TryGetStationLineWaitData(ushort buildingId, out Dictionary<ushort, int> waitingByLine)
        {
            waitingByLine = [];

            ushort[] stopNodes = GetStationStops(buildingId); // your IPT-style adapted helper
            if (stopNodes == null || stopNodes.Length == 0)
            {
                return false;
            }

            var nm = NetManager.instance;

            for (int i = 0; i < stopNodes.Length; i++)
            {
                ushort nodeId = stopNodes[i];
                if (nodeId == 0 || nodeId >= nm.m_nodes.m_buffer.Length)
                {
                    continue;
                }

                ref NetNode node = ref nm.m_nodes.m_buffer[nodeId];
                ushort lineId = node.m_transportLine;
                if (lineId == 0)
                {
                    continue;
                }

                int waiting = QueryStopWaiting(nodeId, out _, out _); // implement this separately
                if (waitingByLine.TryGetValue(lineId, out int current))
                {
                    waitingByLine[lineId] = current + waiting;
                }
                else
                {
                    waitingByLine[lineId] = waiting;
                }
            }

            return waitingByLine.Count > 0;
        }

        private static ushort[] GetStationStops(ushort buildingID)
        {
            List<ushort> stationStops = [];
            NetManager instance = Singleton<NetManager>.instance;
            ushort num1 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_netNode;
            int num2 = 0;
            while (num1 != 0)
            {
                if (instance.m_nodes.m_buffer[num1].Info.m_class.m_layer != ItemClass.Layer.PublicTransport)
                {
                    for (int index = 0; index < 8; ++index)
                    {
                        ushort segment = instance.m_nodes.m_buffer[num1].GetSegment(index);
                        if (segment != 0 && instance.m_segments.m_buffer[segment].m_startNode == num1)
                            CalculateLanes(instance.m_segments.m_buffer[segment].m_lanes, ref stationStops);
                    }
                }
                num1 = instance.m_nodes.m_buffer[num1].m_nextBuildingNode;
                if (++num2 > 32768)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
            return [.. stationStops];
        }

        private static void CalculateLanes(uint firstLane, ref List<ushort> stationStops)
        {
            NetManager instance = Singleton<NetManager>.instance;
            uint num1 = firstLane;
            int num2 = 0;
            while ((int)num1 != 0)
            {
                ushort nodes = instance.m_lanes.m_buffer[(int)(uint)(UIntPtr)num1].m_nodes;
                if (nodes != 0)
                    CalculateLaneNodes(nodes, ref stationStops);
                num1 = instance.m_lanes.m_buffer[(int)(uint)(UIntPtr)num1].m_nextLane;
                if (++num2 > 262144)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        private static void CalculateLaneNodes(ushort firstNode, ref List<ushort> stationStops)
        {
            NetManager instance = Singleton<NetManager>.instance;
            ushort num1 = firstNode;
            int num2 = 0;
            while ((int)num1 != 0)
            {
                if ((int)instance.m_nodes.m_buffer[(int)num1].m_transportLine != 0)
                    stationStops.Add(num1);
                num1 = instance.m_nodes.m_buffer[(int)num1].m_nextLaneNode;
                if (++num2 > 32768)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }
  
        private static int QueryStopWaiting(ushort currentStop, out ushort nextStop, out byte max)
        {
            nextStop = TransportLine.GetNextStop(currentStop);
            max = 0;

            if (currentStop == 0 || nextStop == 0)
            {
                return 0;
            }

            var cm = Singleton<CitizenManager>.instance;
            var nm = Singleton<NetManager>.instance;

            if (currentStop >= nm.m_nodes.m_buffer.Length || nextStop >= nm.m_nodes.m_buffer.Length)
            {
                return 0;
            }

            Vector3 position1 = nm.m_nodes.m_buffer[currentStop].m_position;
            Vector3 position2 = nm.m_nodes.m_buffer[nextStop].m_position;

            int minX = Mathf.Max((int)((position1.x - 64f) / 8f + 1080f), 0);
            int minZ = Mathf.Max((int)((position1.z - 64f) / 8f + 1080f), 0);
            int maxX = Mathf.Min((int)((position1.x + 64f) / 8f + 1080f), 2159);
            int maxZ = Mathf.Min((int)((position1.z + 64f) / 8f + 1080f), 2159);

            int count = 0;

            for (int gridZ = minZ; gridZ <= maxZ; ++gridZ)
            {
                for (int gridX = minX; gridX <= maxX; ++gridX)
                {
                    ushort instanceId = cm.m_citizenGrid[gridZ * 2160 + gridX];
                    int guard = 0;

                    while (instanceId != 0)
                    {
                        ref CitizenInstance citizenInstance = ref cm.m_instances.m_buffer[instanceId];
                        ushort nextGridInstance = (ushort)citizenInstance.m_nextGridInstance;

                        if ((citizenInstance.m_flags & CitizenInstance.Flags.WaitingTransport) != 0 &&
                            Vector3.SqrMagnitude((Vector3)citizenInstance.m_targetPos - position1) < 4096f &&
                            citizenInstance.Info != null &&
                            citizenInstance.Info.m_citizenAI.TransportArriveAtSource(instanceId, ref citizenInstance, position1, position2))
                        {
                            max = Math.Max(max, citizenInstance.m_waitCounter);
                            count++;
                        }

                        instanceId = nextGridInstance;

                        if (++guard > 65536)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid citizen grid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            return count;
        }
    }
}
