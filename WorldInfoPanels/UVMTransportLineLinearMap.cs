﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Extensions.UI;
using Commons.Utils;
using TransportLinesManager.CommonsWindow;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.Managers;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.Components;
using TransportLinesManager.WorldInfoPanels.NearLines;
using TransportLinesManager.WorldInfoPanels.Tabs;
using System;
using System.Linq;
using UnityEngine;
using static TransportLinesManager.WorldInfoPanels.UVMPublicTransportWorldInfoPanel.UVMPublicTransportWorldInfoPanelObject;

namespace TransportLinesManager.WorldInfoPanels
{

    public class UVMTransportLineLinearMap : UICustomControl, IUVMPTWIPChild
    {
        private UIScrollablePanel m_bg;
        private UIScrollbar m_bgScrollbar;
        private UICheckBox m_unscaledCheck;
        private MapMode m_currentMode = MapMode.NONE;
        private bool m_unscaledMode = true;
        private bool m_cachedUnscaledMode = true;
        private static bool m_dirty;

        private UILabel m_labelLineIncomplete;
        internal UISprite m_stopsLineSprite;

        internal UISprite m_lineEnd;


        internal float m_uILineLength;

        internal float m_uILineOffset;

        internal bool m_vehicleCountMismatch;
        private UIPanel m_stopsContainer;
        internal UITemplateList<UIPanel> m_stopButtons;

        internal UITemplateList<UIButton> m_vehicleButtons;

        internal float m_kstopsX = 170;
        internal float m_kstopsXForWalkingTours = 170;
        internal float m_kvehiclesX = 130;
        internal float m_kminStopDistance = 50f;
        internal float m_kvehicleButtonHeight = 36f;
        internal float m_kminUILineLength = 370f;
        internal float m_kmaxUILineLength = 10000f;

        internal float m_actualStopsX;
        internal Vector2 m_kLineSSpritePosition = new Vector2(175f, 20f);
        internal Vector2 m_kLineSSpritePositionForWalkingTours = new Vector2(175f, 20f);

        internal UILabel m_stopsLabel;

        internal UILabel m_vehiclesLabel;

        internal UILabel m_connectionLabel;
        internal TLMLineItemButtonControl m_lineTitleBtnCtrl;

        public static UIScrollablePanel m_scrollPanel;

        internal static Vector2 m_cachedScrollPosition;
        private UIDropDown m_mapModeDropDown;
        private UIPanel m_panelModeSelector;
        private ushort[] m_cachedStopOrder;

        private static TransportSystemDefinition TransportSystem => UVMPublicTransportWorldInfoPanel.GetCurrentTSD();

        #region Overridable
        public void Awake()
        {
            m_bg = component as UIScrollablePanel;

            PublicTransportWorldInfoPanel ptwip = UVMPublicTransportWorldInfoPanel.m_obj.origInstance;


            ptwip.component.width = 800;

            BindComponents(ptwip);
            AdjustLineStopsPanel(ptwip);

            MonoUtils.CreateUIElement(out m_panelModeSelector, m_bg.parent.transform);
            m_panelModeSelector.autoFitChildrenHorizontally = true;
            m_panelModeSelector.autoFitChildrenVertically = true;
            m_panelModeSelector.autoLayout = true;
            m_panelModeSelector.autoLayoutDirection = LayoutDirection.Horizontal;
            m_mapModeDropDown = UIHelperExtension.CloneBasicDropDownNoLabel(Enum.GetValues(typeof(MapMode)).Cast<MapMode>().Where(x => x >= 0).Select(x => Locale.Get("TLM_LINEAR_MAP_VIEW_MODE", x.ToString())).ToArray(), (int idx) =>
               {
                   m_currentMode = (MapMode)idx;
                   RefreshVehicleButtons(GetLineID(out bool fromBuilding), fromBuilding);
                   MarkDirty();
               }, m_panelModeSelector);
            m_mapModeDropDown.textScale = 0.75f;
            m_mapModeDropDown.size = new Vector2(200, 25);
            m_mapModeDropDown.itemHeight = 16;

            m_unscaledCheck = UIHelperExtension.AddCheckboxLocale(m_panelModeSelector, "TLM_LINEAR_MAP_SHOW_UNSCALED", m_unscaledMode, (val) =>
            {
                m_unscaledMode = val;
                MarkDirty();
            });
            MonoUtils.LimitWidthAndBox(m_unscaledCheck.label, 165);

        }


        private void BindComponents(PublicTransportWorldInfoPanel __instance)
        {
            //STOPS
            m_stopsContainer = __instance.Find<UIPanel>("StopsPanel");
            LinearMapStationContainer.EnsureTemplate();
            m_stopButtons = new UITemplateList<UIPanel>(m_stopsContainer, LinearMapStationContainer.TEMPLATE_NAME);
            m_vehicleButtons = new UITemplateList<UIButton>(m_stopsContainer, "VehicleButton");
            m_stopsLineSprite = __instance.Find<UISprite>("StopsLineSprite");
            m_lineEnd = __instance.Find<UISprite>("LineEnd");
            m_stopsLabel = __instance.Find<UILabel>("StopsLabel");
            m_vehiclesLabel = __instance.Find<UILabel>("VehiclesLabel");
            m_labelLineIncomplete = __instance.Find<UILabel>("LabelLineIncomplete");
            m_bgScrollbar = __instance.Find<UIScrollbar>("Scrollbar");


            UISprite lineStart = __instance.Find<UISprite>("LineStart");
            lineStart.relativePosition = new Vector3(4, -8);

            m_vehiclesLabel.relativePosition = new Vector3(100, 12);

            m_stopsLineSprite.spriteName = "PlainWhite";
            m_stopsLineSprite.width = 25;

            m_connectionLabel = Instantiate(m_vehiclesLabel);
            m_connectionLabel.transform.SetParent(m_vehiclesLabel.transform.parent);
            m_connectionLabel.absolutePosition = m_vehiclesLabel.absolutePosition;
            m_connectionLabel.localeID = "TLM_CONNECTIONS";

            TLMLineItemButtonControl.EnsureTemplate();
            var lineStringButton = m_vehiclesLabel.parent.AttachUIComponent(UITemplateManager.GetAsGameObject(TLMLineItemButtonControl.LINE_ITEM_TEMPLATE)) as UIButton;
            m_lineTitleBtnCtrl = lineStringButton.GetComponent<TLMLineItemButtonControl>();
            m_lineTitleBtnCtrl.Resize(36);
            lineStringButton.relativePosition = new Vector3(170, 4);
            lineStringButton.Disable();
        }

        private void AdjustLineStopsPanel(PublicTransportWorldInfoPanel __instance)
        {
            m_scrollPanel = __instance.Find<UIScrollablePanel>("ScrollablePanel");
            m_scrollPanel.eventGotFocus += OnGotFocusBind;
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }
        private uint m_lastDrawTick;
        public void UpdateBindings()
        {
            if (component.isVisible && (m_lastDrawTick + 23 < SimulationManager.instance.m_referenceFrameIndex || m_dirty))
            {
                ushort lineID = GetLineID(out bool fromBuilding);
                if (lineID != 0 || !fromBuilding)
                {
                    if (m_cachedUnscaledMode != m_unscaledMode || m_dirty)
                    {
                        OnSetTarget(null);
                        m_cachedUnscaledMode = m_unscaledMode;
                        m_dirty = false;
                    }
                    UpdateVehicleButtons(lineID, fromBuilding);
                    UpdateStopButtons();
                    m_panelModeSelector.relativePosition = new Vector3(405, 45);
                }
                m_lastDrawTick = SimulationManager.instance.m_referenceFrameIndex;
            }
        }

        public static void MarkDirty() => m_dirty = true;

        public static bool OnLinesOverviewClicked()
        {
            TransportLinesManagerMod.Instance.OpenPanelAtModTab();
            TLMPanel.Instance.OpenAt(TransportSystem);
            return false;
        }

        public static bool ResetScrollPosition()
        {
            m_scrollPanel.scrollPosition = m_cachedScrollPosition;
            return false;
        }


        public void OnSetTarget(Type source)
        {
            if (source == GetType())
            {
                return;
            }

            ushort lineID = GetLineID(out bool fromBuilding);
            if (lineID != 0 || fromBuilding)
            {
                m_bg.isVisible = true;
                m_bgScrollbar.isVisible = true;
                m_unscaledCheck.isVisible = !fromBuilding;
                LineType lineType = GetLineType(lineID, fromBuilding);
                bool isTour = (lineType == LineType.WalkingTour);
                m_mapModeDropDown.isVisible = !isTour && !fromBuilding;
                m_vehiclesLabel.isVisible = !isTour && m_currentMode != MapMode.CONNECTIONS && m_currentMode != MapMode.WAITING_AND_CONNECTIONS;
                m_connectionLabel.isVisible = m_currentMode == MapMode.WAITING_AND_CONNECTIONS || m_currentMode == MapMode.CONNECTIONS;


                if (isTour)
                {
                    m_currentMode = MapMode.CONNECTIONS;
                    m_stopsLabel.relativePosition = new Vector3(215f, 12f, 0f);
                    m_stopsLineSprite.relativePosition = m_kLineSSpritePositionForWalkingTours;
                    m_actualStopsX = m_kstopsXForWalkingTours;
                }
                else
                {
                    m_stopsLabel.relativePosition = new Vector3(215f, 12f, 0f);
                    m_stopsLineSprite.relativePosition = m_kLineSSpritePosition;
                    m_actualStopsX = m_kstopsX;
                }

                if (fromBuilding)
                {
                    m_currentMode = MapMode.WAITING_AND_CONNECTIONS;
                }

                m_lineTitleBtnCtrl.ResetData(fromBuilding, lineID, Vector3.zero);
                Color color;
                int stopsCount;
                ushort firstStop;
                if (fromBuilding)
                {
                    var line = TransportLinesManagerMod.Controller.BuildingLines[lineID];
                    color = line.LineDataObject == null ? TLMController.COLOR_ORDER[lineID % TLMController.COLOR_ORDER.Length] : line.LineDataObject.LineColor;
                    stopsCount = line.CountStops();
                    firstStop = line.SrcStop;
                }
                else
                {
                    color = Singleton<TransportManager>.instance.GetLineColor(lineID);
                    stopsCount = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CountStops(lineID);
                    firstStop = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_stops;
                }
                m_stopsLineSprite.color = color;

                NetManager instance = Singleton<NetManager>.instance;
                float[] stopPositions = new float[stopsCount];
                m_cachedStopOrder = new ushort[stopsCount];
                float minDistance = float.MaxValue;
                float lineLength = 0f;
                UIPanel[] stopsButtons = m_stopButtons.SetItemCount(stopsCount);
                ushort currentStop = firstStop;
                int idx = 0;
                while (currentStop != 0 && idx < stopsButtons.Length)
                {
                    var container = stopsButtons[idx].GetComponent<LinearMapStationContainer>();
                    m_cachedStopOrder[idx] = currentStop;
                    string distance = "(???)";
                    ushort nextStop = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        ushort segmentId = instance.m_nodes.m_buffer[currentStop].GetSegment(i);
                        if (segmentId != 0 && instance.m_segments.m_buffer[segmentId].m_startNode == currentStop)
                        {
                            nextStop = instance.m_segments.m_buffer[segmentId].m_endNode;
                            distance = (instance.m_segments.m_buffer[segmentId].m_averageLength).ToString("0");
                            float segmentSize = m_unscaledMode || fromBuilding ? m_kminStopDistance : instance.m_segments.m_buffer[segmentId].m_averageLength;
                            if (segmentSize == 0f)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Two transport line stops have zero distance");
                                segmentSize = 100f;
                            }
                            stopPositions[idx] = segmentSize;
                            if (segmentSize < minDistance)
                            {
                                minDistance = segmentSize;
                            }
                            lineLength += stopPositions[idx];
                            break;
                        }
                    }
                    container.SetTarget(currentStop, fromBuilding, lineID, distance);
                    currentStop = nextStop;

                    if (stopsCount > 2 && currentStop == firstStop)
                    {
                        break;
                    }
                    if (stopsCount == 2 && idx > 0)
                    {
                        break;
                    }
                    if (stopsCount == 1 || currentStop == 0)
                    {
                        break;
                    }
                    if (++idx >= 32768)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
                float stopDistanceFactor = m_kminStopDistance / minDistance;
                m_uILineLength = stopDistanceFactor * lineLength;
                m_uILineLength = CalcClampedUILineLength(m_uILineLength, out float lineLenFactor);
                stopDistanceFactor *= lineLenFactor;
                if (stopsCount <= 2)
                {
                    m_uILineOffset = (stopDistanceFactor * stopPositions[stopPositions.Length - 1]) - 30f;
                }
                if (m_uILineOffset < 20f || stopsCount > 2)
                {
                    m_uILineOffset = stopDistanceFactor * stopPositions[stopPositions.Length - 1] / 2f;
                }
                m_uILineLength += 20;
                m_stopsLineSprite.height = m_uILineLength;
                m_stopsContainer.height = m_uILineLength + m_kvehicleButtonHeight;
                Vector3 relativePosition = m_lineEnd.relativePosition;
                relativePosition.y = m_uILineLength + 13f;
                relativePosition.x = m_stopsLineSprite.relativePosition.x + 4f;
                m_lineEnd.relativePosition = relativePosition;
                float num8 = 0f;
                for (int j = 0; j < stopsCount; j++)
                {
                    Vector3 relativePosition2 = m_stopButtons.items[j].relativePosition;
                    relativePosition2.x = m_actualStopsX;
                    relativePosition2.y = ShiftVerticalPosition(num8);
                    m_stopButtons.items[j].relativePosition = relativePosition2;
                    num8 += stopPositions[j] * stopDistanceFactor;
                }
                RefreshVehicleButtons(lineID, fromBuilding);
                if (fromBuilding || (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None)
                {
                    m_labelLineIncomplete.isVisible = false;
                    m_stopsContainer.isVisible = true;
                }
                else
                {
                    m_labelLineIncomplete.isVisible = true;
                    m_stopsContainer.isVisible = false;
                }
                MarkDirty();
            }
        }

        #endregion

        private void UpdateVehicleButtons(ushort lineID, bool fromBuilding)
        {
            if (m_vehicleCountMismatch)
            {
                RefreshVehicleButtons(lineID, fromBuilding);
                m_vehicleCountMismatch = false;
            }
            if (m_currentMode == MapMode.CONNECTIONS || m_currentMode == MapMode.WAITING_AND_CONNECTIONS)
            {
                return;
            }
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort vehicleId = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_vehicles;
            int idx = 0;
            while (vehicleId != 0)
            {
                if (idx > m_vehicleButtons.items.Count - 1)
                {
                    m_vehicleCountMismatch = true;
                    break;
                }
                VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].Info;


                if (m_unscaledMode)
                {
                    ushort nextStop = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding;
                    float prevStationIdx = Array.IndexOf(m_cachedStopOrder, TransportLine.GetPrevStop(nextStop));
                    float nextStationIdx = Array.IndexOf(m_cachedStopOrder, nextStop);
                    if (nextStationIdx < prevStationIdx)
                    {
                        nextStationIdx += m_cachedStopOrder.Length;
                    }
                    Vector3 relativePosition = m_vehicleButtons.items[idx].relativePosition;
                    // determine basic relative position
                    // 4 switch cases for each of 4 unscaled display states
                    float weightingPrev, weightingNext;
                    var vehicle_flags = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags;
                    if ((vehicle_flags & (Vehicle.Flags.Leaving)) != 0)
                    {
                        // vehicle stopped at a stop; current stop is "prev" because next stop is "next"
                        weightingPrev = 1;
                        weightingNext = 0;
                    }
                    else if ((vehicle_flags & (Vehicle.Flags.Arriving)) != 0)
                    {
                        // vehicle departing from a stop
                        weightingPrev = 0.75f;
                        weightingNext = 0.25f;
                    }
                    else if ((vehicle_flags & (Vehicle.Flags.Stopped)) != 0)
                    {
                        // vehicle arriving at the next stop
                        weightingPrev = 0.25f;
                        weightingNext = 0.75f;
                    }
                    else
                    {
                        // vehicle in progress to next stop
                        weightingPrev = weightingNext = 0.5f;
                    }
                    // apply the basic relative position first
                    relativePosition.y = prevStationIdx * weightingPrev + nextStationIdx * weightingNext;
                    // then, apply the adjustment for low-stop-count positioning
                    int stopCount = m_cachedStopOrder.Length;
                    float extraScaling = 1;
                    if (stopCount < 8)
                    {
                        // logic should be the same as the stop position
                        float uncheckedLineLen = m_kminStopDistance * stopCount;
                        CalcClampedUILineLength(uncheckedLineLen, out extraScaling);
                    }
                    relativePosition.y = ShiftVerticalPosition(relativePosition.y * m_kminStopDistance * extraScaling);
                    m_vehicleButtons.items[idx].relativePosition = relativePosition;
                }
                else
                {
                    if (info.m_vehicleAI.GetProgressStatus(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out float current, out float max))
                    {
                        float percent = current / max;
                        float y = m_uILineLength * percent;
                        Vector3 relativePosition = m_vehicleButtons.items[idx].relativePosition;
                        relativePosition.y = ShiftVerticalPosition(y);
                        m_vehicleButtons.items[idx].relativePosition = relativePosition;
                    }
                }

                info.m_vehicleAI.GetBufferStatus(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out _, out int passengerQuantity, out int passengerCapacity);
                UILabel labelVehicle = m_vehicleButtons.items[idx].Find<UILabel>("PassengerCount");
                labelVehicle.prefix = passengerQuantity.ToString() + "/" + passengerCapacity.ToString();
                labelVehicle.processMarkup = true;
                labelVehicle.textAlignment = UIHorizontalAlignment.Right;
                switch (m_currentMode)
                {
                    case MapMode.WAITING:
                    case MapMode.NONE:
                    case MapMode.CONNECTIONS:
                    case MapMode.WAITING_AND_CONNECTIONS:
                        labelVehicle.text = "";
                        labelVehicle.suffix = "";
                        break;
                    case MapMode.EARNINGS_ALL_TIME:
                        TLMTransportLineStatusesManager.instance.GetIncomeAndExpensesForVehicle(vehicleId, out long income, out long expense);
                        PrintIncomeExpenseVehicle(lineID, fromBuilding, idx, labelVehicle, income, expense, 100);
                        break;
                    case MapMode.EARNINGS_LAST_WEEK:
                        TLMTransportLineStatusesManager.instance.GetLastWeekIncomeAndExpensesForVehicles(vehicleId, out long income2, out long expense2);
                        PrintIncomeExpenseVehicle(lineID, fromBuilding, idx, labelVehicle, income2, expense2, 8);
                        break;
                    case MapMode.EARNINGS_CURRENT_WEEK:
                        TLMTransportLineStatusesManager.instance.GetCurrentIncomeAndExpensesForVehicles(vehicleId, out long income3, out long expense3);
                        PrintIncomeExpenseVehicle(lineID, fromBuilding, idx, labelVehicle, income3, expense3, 8);
                        break;
                }

                vehicleId = instance.m_vehicles.m_buffer[vehicleId].m_nextLineVehicle;
                if (++idx >= 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            if (idx < m_vehicleButtons.items.Count - 1)
            {
                m_vehicleCountMismatch = true;
            }
        }

        private void PrintIncomeExpenseVehicle(ushort lineID, bool fromBuilding, int idx, UILabel labelVehicle, long income, long expense, float scale)
        {
            if (!fromBuilding)
            {
                var tsd = TransportSystemDefinition.FromLineId(lineID, fromBuilding);
                m_vehicleButtons.items[idx].color = Color.Lerp(Color.white, income > expense ? Color.green : Color.red, Mathf.Max(income, expense) / scale * TLMLineUtils.GetTicketPriceForLine(tsd, lineID).First.Value);
                labelVehicle.text = $"\n<color #00cc00>{(income / 100.0f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}</color>";
                labelVehicle.suffix = $"\n<color #ff0000>{(expense / 100.0f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}</color>";
            }
        }

        public void OnGotFocus() => m_cachedScrollPosition = m_scrollPanel.scrollPosition;
        private void OnGotFocusBind(UIComponent component, UIFocusEventParameter eventParam) => m_cachedScrollPosition = m_scrollPanel.scrollPosition;

        internal LineType GetLineType(ushort lineID, bool fromBuilding) => UVMPublicTransportWorldInfoPanel.GetLineType(lineID, fromBuilding);

        private float CalcClampedUILineLength(float initialLineLen, out float lineLenFactor)
        {
            float acceptableLineLen = Mathf.Clamp(initialLineLen, m_kminUILineLength, m_kmaxUILineLength);
            lineLenFactor = acceptableLineLen / initialLineLen;
            return acceptableLineLen;
        }

        private float ShiftVerticalPosition(float y)
        {
            y += m_uILineOffset;
            if (y > m_uILineLength + 5f)
            {
                y -= m_uILineLength;
            }
            return y;
        }

        private void RefreshVehicleButtons(ushort lineID, bool fromBuilding)
        {
            if (m_currentMode == MapMode.CONNECTIONS || m_currentMode == MapMode.WAITING_AND_CONNECTIONS || fromBuilding)
            {
                m_vehiclesLabel.isVisible = false;
                m_vehicleButtons.SetItemCount(0);
                m_connectionLabel.isVisible = true;
            }
            else
            {
                m_vehiclesLabel.isVisible = true;
                m_connectionLabel.isVisible = false;
                m_vehicleButtons.SetItemCount(Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CountVehicles(lineID));
                VehicleManager instance = Singleton<VehicleManager>.instance;
                ushort num = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_vehicles;
                int num2 = 0;
                while (num != 0)
                {
                    Vector3 relativePosition = m_vehicleButtons.items[num2].relativePosition;
                    relativePosition.x = m_kvehiclesX;
                    m_vehicleButtons.items[num2].relativePosition = relativePosition;
                    m_vehicleButtons.items[num2].objectUserData = num;
                    num = instance.m_vehicles.m_buffer[num].m_nextLineVehicle;
                    if (++num2 >= 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
        }

        public void Hide()
        {
            m_bg.isVisible = false;
            m_bgScrollbar.isVisible = false;
            m_unscaledCheck.isVisible = false;
            m_mapModeDropDown.isVisible = false;
            m_labelLineIncomplete.isVisible = false;
        }

        internal ushort GetLineID(out bool fromBuilding) => UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out fromBuilding) ? lineId : (ushort)0;

        private void UpdateStopButtons()
        {
            foreach (UIPanel uiPanel in m_stopButtons.items)
            {
                uiPanel.GetComponent<LinearMapStationContainer>().UpdateBindings(m_currentMode);
            }
        }

        public bool MayBeVisible() => UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && (fromBuilding || lineId > 0);


    }
    internal enum MapMode
    {
        NONE,
        WAITING,
        CONNECTIONS,
        EARNINGS_CURRENT_WEEK,
        EARNINGS_LAST_WEEK,
        EARNINGS_ALL_TIME,
        WAITING_AND_CONNECTIONS = -1
    }
}