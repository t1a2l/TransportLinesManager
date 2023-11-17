﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.UI.SpriteNames;
using Commons.Utils;
using Commons.Utils.StructExtensions;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Data.Managers;
using TransportLinesManager.Overrides;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.NearLines;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TransportLinesManager.WorldInfoPanels.UVMPublicTransportWorldInfoPanel.UVMPublicTransportWorldInfoPanelObject;

namespace TransportLinesManager.WorldInfoPanels.Components
{
    public class LinearMapStationContainer : UICustomControl
    {
        public const string TEMPLATE_NAME = "TLM_LinearMapStopTemplate";
        public static void EnsureTemplate()
        {
            if (UITemplateUtils.GetTemplateDict().ContainsKey(TEMPLATE_NAME))
            {
                return;
            }
            var go = new GameObject();
            UIPanel panel = go.AddComponent<UIPanel>();
            panel.size = new Vector2(36, 36);
            UIButton button = UITemplateManager.Get<UIButton>("StopButton");
            panel.AttachUIComponent(button.gameObject).transform.localScale = Vector3.one;
            button.relativePosition = Vector2.zero;
            button.name = "StopButton";
            button.scaleFactor = 1f;
            button.spritePadding.top = 2;
            button.isTooltipLocalized = true;
            MonoUtils.InitButtonFg(button, false, "DistrictOptionBrushMedium");
            MonoUtils.InitButtonSameSprite(button, "");

            UILabel uilabel = button.Find<UILabel>("PassengerCount");
            panel.AttachUIComponent(uilabel.gameObject).transform.localScale = Vector3.one;
            uilabel.relativePosition = new Vector3(38, 12);
            uilabel.processMarkup = true;
            uilabel.isVisible = true;
            uilabel.minimumSize = new Vector2(175, 50);
            uilabel.verticalAlignment = UIVerticalAlignment.Middle;
            MonoUtils.LimitWidthAndBox(uilabel, 175, true);


            UIPanel connectionPanel = panel.AddUIComponent<UIPanel>();
            connectionPanel.name = "ConnectionPanel";
            connectionPanel.relativePosition = new Vector3(-50, 5);
            connectionPanel.size = new Vector3(50, 40);
            connectionPanel.autoLayout = true;
            connectionPanel.wrapLayout = true;
            connectionPanel.autoLayoutDirection = LayoutDirection.Vertical;
            connectionPanel.autoLayoutStart = LayoutStart.TopRight;



            UILabel distLabel = panel.AddUIComponent<UILabel>();

            distLabel.name = "Distance";
            distLabel.relativePosition = new Vector3(-12, 37);
            distLabel.textAlignment = UIHorizontalAlignment.Center;
            distLabel.textScale = 0.65f;
            distLabel.suffix = "m";
            distLabel.useOutline = true;
            distLabel.minimumSize = new Vector2(60, 0);
            distLabel.outlineColor = Color.black;

            MonoUtils.CreateUIElement(out UITextField lineNameField, panel.transform, "StopNameField", new Vector4(38, -6, 175, 50));
            lineNameField.maxLength = 256;
            lineNameField.isVisible = false;
            lineNameField.verticalAlignment = UIVerticalAlignment.Middle;
            lineNameField.horizontalAlignment = UIHorizontalAlignment.Left;
            lineNameField.selectionSprite = "EmptySprite";
            lineNameField.builtinKeyNavigation = true;
            lineNameField.textScale = uilabel.textScale;
            lineNameField.padding.top = 18;
            lineNameField.padding.left = 5;
            lineNameField.padding.bottom = 14;
            MonoUtils.InitButtonFull(lineNameField, false, "TextFieldPanel");

            UITemplateUtils.GetTemplateDict()[TEMPLATE_NAME] = go.AddComponent<LinearMapStationContainer>().component;
        }

        private ItemClass.SubService CurrentSubService => TransportSystemDefinition.GetDefinitionForLine(m_lineId, m_fromBuilding).SubService;

        private ushort m_stopId;
        private ushort m_lineId;
        private bool m_fromBuilding;
        private UIPanel bg;
        private UIPanel connectionPanel;
        private UITextField stopNameField;
        private UILabel uilabel;
        private UIButton uibutton;
        private UILabel dist;
        private UITemplateList<UIButton> connections;
        private bool m_dirtyNames;
        private bool m_dirtyTerminal;
        internal int m_kMaxConnectionsLine = 4;

        public void Awake()
        {
            bg = GetComponent<UIPanel>();
            connectionPanel = bg.Find<UIPanel>("ConnectionPanel");
            stopNameField = bg.Find<UITextField>("StopNameField");
            uilabel = bg.Find<UILabel>("PassengerCount");
            uibutton = bg.Find<UIButton>("StopButton");
            dist = bg.Find<UILabel>("Distance");
            TLMLineItemButtonControl.EnsureTemplate();
            connections = new UITemplateList<UIButton>(connectionPanel, TLMLineItemButtonControl.LINE_ITEM_TEMPLATE);

            uilabel.eventMouseEnter += (c, r) => uilabel.backgroundSprite = "TextFieldPanelHovered";
            uilabel.eventMouseLeave += (c, r) => uilabel.backgroundSprite = string.Empty;
            uilabel.eventClick += (c, r) =>
            {
                uilabel.Hide();
                stopNameField.Show();
                stopNameField.text = TLMStationUtils.GetStationName(m_stopId,
                    m_lineId,
                    CurrentSubService,
                    m_fromBuilding);
                stopNameField.Focus();
            };
            stopNameField.eventLeaveFocus += delegate (UIComponent c, UIFocusEventParameter r)
            {
                stopNameField.Hide();
                uilabel.Show();
            };
            stopNameField.eventTextSubmitted += (x, y) =>
            {
                if (!m_fromBuilding)
                {
                    TLMStationUtils.SetStopName(y.Trim(), m_stopId, m_lineId, () =>
                    {
                        uilabel.prefix = $"<color white>{TLMStationUtils.GetFullStationName(m_stopId, m_lineId, CurrentSubService, m_fromBuilding)}</color>";
                    });
                }
            };
            uibutton.eventMouseUp += (x, y) =>
            {
                if ((y.buttons & UIMouseButton.Right) != 0 && !m_fromBuilding && TransportSystemDefinition.FromLineId(m_lineId, m_fromBuilding).CanHaveTerminals() && m_stopId != Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineId].m_stops)
                {
                    var newVal = TLMStopDataContainer.Instance.SafeGet(m_stopId).IsTerminal;
                    TLMStopDataContainer.Instance.SafeGet(m_stopId).IsTerminal = !newVal;
                    NetProperties properties = NetManager.instance.m_properties;
                    if (!(properties is null) && !(properties.m_drawSound is null))
                    {
                        AudioManager.instance.DefaultGroup.AddPlayer(0, properties.m_drawSound, 1f);
                    }
                    m_dirtyTerminal = true;
                    UVMTransportLineLinearMap.MarkDirty();
                }
            };

            InstanceManagerOverrides.EventOnBuildingRenamed += (x) => m_dirtyNames = true;
        }

        internal void SetTarget(ushort stopId, bool fromBuilding, ushort lineId, string distance)
        {
            m_stopId = stopId;
            m_fromBuilding = fromBuilding;
            m_lineId = lineId;
            uibutton.objectUserData = stopId;

            uilabel.prefix = TLMStationUtils.GetFullStationName(m_stopId, m_lineId, CurrentSubService, m_fromBuilding);
            uilabel.text = "";

            dist.text = distance;
            UpdateConnectionPanel();
            UpdateTerminalStatus();
            uibutton.tooltipLocaleID = m_fromBuilding || !TransportSystemDefinition.FromLineId(m_lineId, m_fromBuilding).CanHaveTerminals()
                ? ""
                : m_stopId == TransportManager.instance.m_lines.m_buffer[m_lineId].m_stops
                    ? "TLM_FIRSTSTOPALWAYSTERMINAL"
                    : "TLM_RIGHTCLICKSETTERMINAL";
        }


        internal void UpdateBindings(MapMode currentMode)
        {

            if (m_dirtyNames)
            {
                uilabel.prefix = TLMStationUtils.GetFullStationName((ushort)uibutton.objectUserData, m_lineId, CurrentSubService, m_fromBuilding);
                m_dirtyNames = false;
            }
            if (m_dirtyTerminal)
            {
                UpdateTerminalStatus();
                m_dirtyTerminal = false;
            }
            connectionPanel.isVisible = currentMode == MapMode.CONNECTIONS || currentMode == MapMode.WAITING_AND_CONNECTIONS;
            if (GetLineType() == LineType.WalkingTour)
            {
                return;
            }


            switch (currentMode)
            {
                case MapMode.WAITING:
                case MapMode.WAITING_AND_CONNECTIONS:
                    TLMLineUtils.GetQuantityPassengerWaiting(m_stopId, out int residents, out int tourists, out int timeTillBored);
                    uilabel.text = "\n" + string.Format(Locale.Get("TLM_WAITING_PASSENGERS_RESIDENT_TOURSTS"), residents + tourists, residents, tourists) + "\n";
                    uibutton.color = Color.Lerp(Color.red, Color.white, timeTillBored / 255f);
                    uilabel.suffix = string.Format(Locale.Get("TLM_TIME_TILL_BORED_TEMPLATE_STATION_MAP"), uibutton.color.ToRGB(), timeTillBored);
                    break;
                case MapMode.NONE:
                    uibutton.color = Color.white;
                    uilabel.text = "";
                    uilabel.suffix = "";
                    uibutton.tooltip = "";
                    break;
                case MapMode.CONNECTIONS:
                    uibutton.color = Color.white;
                    uilabel.text = "";
                    uilabel.suffix = "";
                    uibutton.tooltip = "";
                    break;
                case MapMode.EARNINGS_ALL_TIME:
                    TLMTransportLineStatusesManager.instance.GetStopIncome(m_stopId, out long income);
                    PrintIncomeStop(income);
                    break;
                case MapMode.EARNINGS_CURRENT_WEEK:
                    TLMTransportLineStatusesManager.instance.GetCurrentStopIncome(m_stopId, out long income2);
                    PrintIncomeStop(income2);
                    break;
                case MapMode.EARNINGS_LAST_WEEK:
                    TLMTransportLineStatusesManager.instance.GetLastWeekStopIncome(m_stopId, out long income3);
                    PrintIncomeStop(income3);
                    break;
            }
        }

        private LineType GetLineType() => UVMPublicTransportWorldInfoPanel.GetLineType(m_lineId, m_fromBuilding);

        private void UpdateTerminalStatus() => uibutton.normalBgSprite =
                              !m_fromBuilding && TransportSystemDefinition.FromLineId(m_lineId, m_fromBuilding).CanHaveTerminals() && (m_stopId == Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineId].m_stops || TLMStopDataContainer.Instance.SafeGet(m_stopId).IsTerminal)
                                ? ResourceLoader.GetDefaultSpriteNameFor(LineIconSpriteNames.S05StarIcon, true)
                                : "";

        private void PrintIncomeStop(long income)
        {
            uibutton.color = Color.Lerp(Color.white, Color.green, income / (1000f * Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineId].Info.m_ticketPrice));
            uilabel.text = $"\n<color #00cc00>{(income / 100.0f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}</color>";
            uibutton.tooltip = "";
            uilabel.suffix = "";
        }
        private void UpdateConnectionPanel()
        {
            var linesFound = new List<ushort>();
            var targetPos = NetManager.instance.m_nodes.m_buffer[m_stopId].m_position;
            TLMLineUtils.GetNearLines(targetPos, 150f, ref linesFound);
            if (!m_fromBuilding)
            {
                linesFound.Remove(m_lineId);
            }
            var targBuilding = TLMStationUtils.GetStationBuilding(m_stopId, m_lineId, m_fromBuilding);
            var lines = BuildingManager.instance.m_buildings.m_buffer[targBuilding].Info.m_buildingAI is TransportStationAI tsai && (tsai.m_transportLineInfo?.m_class.m_subService == ItemClass.SubService.PublicTransportTrain || tsai.m_transportLineInfo?.m_class.m_subService == ItemClass.SubService.PublicTransportBus) ? TransportLinesManagerMod.Controller.BuildingLines.SafeGet(targBuilding) : null;

            var buildingLines = lines is null ? new List<long>() : lines.RegionalLines.Keys.ToList();
            if (m_fromBuilding)
            {
                buildingLines.Remove(m_lineId);
            }

            var itemsEntries = connections.SetItemCount(linesFound.Count + buildingLines.Count);
            int newSize = itemsEntries.Length > m_kMaxConnectionsLine ? 18 : 36;
            int idx = 0;
            for (; idx < linesFound.Count; idx++)
            {
                ushort lineId = linesFound[idx];
                var itemControl = itemsEntries[idx].GetComponent<TLMLineItemButtonControl>();
                itemControl.Resize(newSize);
                itemControl.ResetData(false, lineId, targetPos);
            }
            for (int j = 0; j < buildingLines.Count; idx++, j++)
            {
                long line = buildingLines[j];
                var itemControl = itemsEntries[idx].GetComponent<TLMLineItemButtonControl>();
                itemControl.Resize(newSize);
                itemControl.ResetData(true, (ushort)line, targetPos);
            }
        }

    }
}