﻿
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Extensions.UI;
using Commons.UI;
using Commons.Utils;
using Commons.Utils.StructExtensions;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.Managers;
using TransportLinesManager.Utils;
using UnityEngine;

namespace TransportLinesManager.CommonsWindow.List.Components
{
    internal class UVMLineListItem : UICustomControl
    {
        private static readonly Color32 BackgroundColor = new Color32(49, 52, 58, 255);
        private static readonly Color32 BrokenBackgroundColor = new Color32(80, 26, 24, 255);
        private static readonly Color32 Line0BackgroundColor = new Color32(88, 28, 124, 255);
        private static readonly Color32 ForegroundColor = new Color32(185, 221, 254, 255);

        private ushort m_lineID = ushort.MaxValue;

        private UICheckBox m_lineIsVisible;

        private UIColorField m_lineColor;

        private UILabel m_lineName;
        private UITextField m_lineNameField;

        private UILabel m_lineStops;

        private UILabel m_lineVehicles;
        private UILabel m_lineBudgetLabel;
        private UILabel m_linePassengers;

        private UIButton m_lineNumberFormatted;
        private UIButton m_deleteLine;

        private UILabel m_lineBalance;

        private UIPanel m_background;
        private bool m_mouseIsOver;
        private UIButton m_buttonAutoName;
        private UIButton m_buttonAutoColor;

        private bool m_isUpdatingVisibility = false;

        public ushort LineID
        {
            get => m_lineID;
            set => SetLineID(value);
        }

        public string LineName => m_lineName.text;

        public int StopCounts => int.Parse(m_lineStops.text);

        public int VehicleCounts => int.Parse(m_lineVehicles.text);

        public int LineNumber { get; private set; }
        public int PassengerCountsInt { get; private set; }

        private void SetLineID(ushort id)
        {
            m_lineID = id >= TransportManager.MAX_LINE_COUNT ? throw new System.Exception($"INVALID LINE IDX: {id}") : id;
            m_lastUpdate = 0;
        }

        public void RefreshData(bool updateColors, bool updateVisibility)
        {
            if (m_lineID < Singleton<TransportManager>.instance.m_lines.m_buffer.Length)
            {
                if (m_lineID > 0)
                {
                    m_lineName.text = Singleton<TransportManager>.instance.GetLineName(m_lineID);
                    LineNumber = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_lineNumber;


                    int averageCount = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_passengers.m_residentPassengers.m_averageCount;
                    int averageCount2 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_passengers.m_touristPassengers.m_averageCount;
                    m_linePassengers.isVisible = true;
                    m_linePassengers.text = (averageCount + averageCount2).ToString("N0");

                    m_linePassengers.tooltip = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_PASSENGERS", new object[]
                    {
                averageCount,
                averageCount2
                    });
                    TLMLineUtils.SetLineNumberCircleOnRef(LineID, false, m_lineNumberFormatted, 0.8f);
                    m_lineColor.atlas = m_linePassengers.atlas;
                    m_lineColor.normalFgSprite = TLMLineUtils.GetIconForLine(LineID, false);
                    m_lineColor.isVisible = true;
                    m_deleteLine.isVisible = true;
                    m_buttonAutoName.isVisible = true;
                    m_buttonAutoColor.isVisible = true;
                    m_lineStops.isVisible = true;

                    PassengerCountsInt = averageCount + averageCount2;

                    SetBackgroundColor(((Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None));

                    m_lineIsVisible.isVisible = true;
                    var tsd = TransportSystemDefinition.FromLineId(m_lineID, false);
                    if (updateColors)
                    {
                        m_lineColor.selectedColor = Singleton<TransportManager>.instance.GetLineColor(m_lineID);
                    }
                    if (updateVisibility)
                    {
                        m_isUpdatingVisibility = true;
                        m_lineIsVisible.isChecked = ((Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_flags & TransportLine.Flags.Hidden) == TransportLine.Flags.None);
                        m_isUpdatingVisibility = false;
                    }


                    if (tsd.HasVehicles())
                    {
                        m_lineVehicles.isVisible = true;
                        TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].Info;
                        float overallBudget = Singleton<EconomyManager>.instance.GetBudget(info.m_class) / 100f;


                        string vehTooltip = string.Format("{0} {1}", m_lineVehicles.text, Locale.Get("PUBLICTRANSPORT_VEHICLES"));
                        m_lineVehicles.tooltip = vehTooltip;
                        m_lineStops.text = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].CountStops(m_lineID).ToString("N0");
                        m_lineVehicles.text = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].CountVehicles(m_lineID).ToString("N0");

                        TLMTransportLineStatusesManager.instance.GetLastWeekIncomeAndExpensesForLine(LineID, out long income, out long expense);
                        long balance = (income - expense);
                        m_lineBalance.text = (balance / 100.0f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
                        m_lineBalance.textColor = balance >= 0 ? ColorExtensions.FromRGB("00c000") : ColorExtensions.FromRGB("c00000");
                        m_lineBalance.isVisible = true;

                        m_lineBudgetLabel.text = string.Format("{0:0%}", TLMLineUtils.GetEffectiveBudget(LineID));
                        m_lineBudgetLabel.tooltip = string.Format(Locale.Get("TLM_LINE_BUDGET_EXPLAIN_2"),
                            Locale.Get("TRANSPORT_LINE", Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].Info.m_transportType.ToString()),
                            overallBudget, Singleton<TransportManager>.instance.m_lines.m_buffer[LineID].m_budget / 100f, TLMLineUtils.GetEffectiveBudget(LineID));
                        m_lineBudgetLabel.isVisible = true;

                    }
                    else
                    {
                        m_lineBudgetLabel.isVisible = false;
                        m_lineBalance.isVisible = false;
                        m_lineVehicles.isVisible = false;
                    }
                }
                else
                {
                    var currentTSD = TLMPanel.Instance.m_linesPanel.TSD;
                    m_lineName.text = string.Format(Locale.Get("TLM_OUTSIDECONNECTION_LISTNAMETEMPLATE"), currentTSD.GetTransportName());
                    LineNumber = 0;
                    m_linePassengers.isVisible = false;
                    m_lineColor.isVisible = false;
                    m_lineBudgetLabel.isVisible = false;
                    m_lineBalance.isVisible = false;
                    m_lineIsVisible.isVisible = false;
                    m_deleteLine.isVisible = false;
                    m_buttonAutoName.isVisible = false;
                    m_buttonAutoColor.isVisible = false;
                    m_lineVehicles.isVisible = false;
                    m_lineStops.isVisible = false;
                    SetBackgroundColor(false, true);
                }
            }
        }

        public void SetBackgroundColor(bool broken = false, bool lineZero = false)
        {
            Color32 backgroundColor = lineZero ? Line0BackgroundColor : !broken ? BackgroundColor : BrokenBackgroundColor;
            backgroundColor.a = !broken ? (byte)((base.component.zOrder % 2 != 0) ? 127 : 255) : (byte)Mathf.Lerp(127, 255, Mathf.Abs((SimulationManager.instance.m_currentTickIndex % 60 / 30f) - 1));
            if (m_mouseIsOver)
            {
                backgroundColor.r = (byte)Mathf.Min(255, (backgroundColor.r * 3) >> 1);
                backgroundColor.g = (byte)Mathf.Min(255, (backgroundColor.g * 3) >> 1);
                backgroundColor.b = (byte)Mathf.Min(255, (backgroundColor.b * 3) >> 1);
            }
            m_background.color = backgroundColor;
        }

        private uint m_lastUpdate;

        private void LateUpdate()
        {
            if (component.isVisible && m_lastUpdate + 30 < SimulationManager.instance.m_referenceFrameIndex)
            {
                RefreshData(m_lastUpdate == 0, m_lastUpdate == 0);
                m_lastUpdate = SimulationManager.instance.m_referenceFrameIndex;
            }
            else if (m_lineID > 0 && m_lineID < Singleton<TransportManager>.instance.m_lines.m_buffer.Length && ((Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None))
            {
                SetBackgroundColor(true);
            }
        }




        private static void CreateLabel(out UILabel label, Transform transform)
        {
            MonoUtils.CreateUIElement(out label, transform, "LineStops");
            label.textAlignment = UIHorizontalAlignment.Center;
            label.textColor = ForegroundColor;
            label.minimumSize = new Vector2(80, 18);
            label.pivot = UIPivotPoint.TopLeft;
            label.wordWrap = false;
            label.autoSize = true;
        }


        public void ChangeLineVisibility(bool r)
        {
            if (m_lineID < Singleton<TransportManager>.instance.m_lines.m_buffer.Length && m_lineID != 0 && !m_isUpdatingVisibility)
            {
                Singleton<SimulationManager>.instance.AddAction(() =>
                {
                    if (r)
                    {
                        Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_flags &= ~TransportLine.Flags.Hidden;
                    }
                    else
                    {
                        Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_flags |= TransportLine.Flags.Hidden;
                    }
                });
            }
        }

        public void DoAutoColor() => TLMController.AutoColor(m_lineID);

        public void DoAutoName() => TLMLineUtils.SetLineName(m_lineID, TLMLineUtils.CalculateAutoName(m_lineID, false, out _));

        private void OnMouseEnter(UIComponent comp, UIMouseEventParameter param)
        {
            if (!m_mouseIsOver)
            {
                m_mouseIsOver = true;
                if (m_lineID != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        if ((Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                        {
                            Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_flags |= TransportLine.Flags.Highlighted;
                        }
                    });
                }
            }
        }

        private void OnMouseLeave(UIComponent comp, UIMouseEventParameter param)
        {
            if (m_mouseIsOver)
            {
                m_mouseIsOver = false;
                if (m_lineID != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        if ((Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                        {
                            Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_flags &= ~TransportLine.Flags.Highlighted;
                        }
                    });
                }
            }
        }

        private void OnEnable()
        {
            Singleton<TransportManager>.instance.eventLineColorChanged += new TransportManager.LineColorChangedHandler(OnLineChanged);
            Singleton<TransportManager>.instance.eventLineNameChanged += new TransportManager.LineNameChangedHandler(OnLineChanged);
        }

        private void OnDisable()
        {
            Singleton<TransportManager>.instance.eventLineColorChanged -= new TransportManager.LineColorChangedHandler(OnLineChanged);
            Singleton<TransportManager>.instance.eventLineNameChanged -= new TransportManager.LineNameChangedHandler(OnLineChanged);
        }


        private void OnLineChanged(ushort id)
        {
            if (id == m_lineID)
            {
                RefreshData(true, true);
            }
        }

        private void OnColorChanged(UIComponent x, Color color) => TLMLineUtils.SetLineColor(this, m_lineID, color);

        public void Awake()
        {
            m_mouseIsOver = false;
            m_background = Find<UIPanel>("BG");
            m_lineName = Find<UILabel>("LineName");
            m_lineNameField = Find<UITextField>("LineNameField");
            m_lineStops = Find<UILabel>("lineStops");
            m_linePassengers = Find<UILabel>("LinePassengers");
            m_lineBalance = Find<UILabel>("LineExpenses");
            m_lineVehicles = Find<UILabel>("LineVehicles");
            m_lineVehicles = Find<UILabel>("LineVehicles");
            var viewLine = Find<UIButton>("ViewLine");
            m_deleteLine = Find<UIButton>("DeleteLine");
            m_lineBudgetLabel = Find<UILabel>("LineBudget");
            m_lineIsVisible = Find<UICheckBox>("LineVisibility");
            m_lineColor = Find<UIColorField>("LineColor");
            m_lineNumberFormatted = m_lineColor.GetComponentInChildren<UIButton>();
            m_buttonAutoName = Find<UIButton>("AutoNameBtn");
            m_buttonAutoColor = Find<UIButton>("AutoColorBtn");

            m_buttonAutoName.eventClick += (component, eventParam) => DoAutoName();
            m_buttonAutoColor.eventClick += (component, eventParam) => DoAutoColor();

            viewLine.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
             {
                 if (m_lineID != 0)
                 {
                     Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_stops].m_position;
                     InstanceID iid = InstanceID.Empty;
                     iid.TransportLine = m_lineID;
                     WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, iid);
                 }
                 else
                 {
                     Vector3 position = default;
                     InstanceID iid = InstanceID.Empty;
                     iid.Set(TLMInstanceType.TransportSystemDefinition, TLMPanel.Instance.m_linesPanel.TSD.Id);
                     WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, iid);
                 }
             };

            m_deleteLine.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                if (m_lineID != 0)
                {
                    ConfirmPanel.ShowModal("CONFIRM_LINEDELETE", delegate (UIComponent comp, int ret)
                    {
                        if (ret == 1)
                        {
                            Singleton<SimulationManager>.instance.AddAction(delegate
                            {
                                Singleton<TransportManager>.instance.ReleaseLine(m_lineID);

                            });
                        }
                    });
                }
            };

            component.eventVisibilityChanged += delegate (UIComponent c, bool v)
            {
                if (v)
                {
                    RefreshData(true, true);
                }
            };

            component.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
            component.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            m_lineName.eventMouseEnter += delegate (UIComponent c, UIMouseEventParameter r)
            {
                m_lineName.backgroundSprite = "TextFieldPanelHovered";
            };
            m_lineName.eventMouseLeave += delegate (UIComponent c, UIMouseEventParameter r)
            {
                m_lineName.backgroundSprite = string.Empty;
            };
            m_lineName.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                m_lineName.Hide();
                m_lineNameField.Show();
                m_lineNameField.text = m_lineName.text;
                m_lineNameField.Focus();
            };
            m_lineNameField.eventLeaveFocus += delegate (UIComponent c, UIFocusEventParameter r)
            {
                m_lineNameField.Hide();
                TLMLineUtils.SetLineName(LineID, m_lineNameField.text);
                m_lineName.Show();
                m_lineName.text = m_lineNameField.text;
            };
            m_lineIsVisible.eventCheckChanged += (x, y) => ChangeLineVisibility(y);
            m_lineColor.eventSelectedColorReleased += OnColorChanged;

            MonoUtils.LimitWidthAndBox(m_lineStops, out UIPanel containerLineStops);
            MonoUtils.LimitWidthAndBox(m_linePassengers, out UIPanel containerPassenger);
            MonoUtils.LimitWidthAndBox(m_lineBalance, out UIPanel containerLineBalance);
            MonoUtils.LimitWidthAndBox(m_lineVehicles, out UIPanel containerVehicles);
            MonoUtils.LimitWidthAndBox(m_lineBudgetLabel, out UIPanel containerBudget);

            containerLineBalance.relativePosition = new Vector3(625, 10);
            containerPassenger.relativePosition = new Vector3(540, 10);
            containerLineStops.relativePosition = new Vector3(340, 10);
            containerVehicles.relativePosition = new Vector2(445, 0);
            containerBudget.relativePosition = new Vector2(445, 19);

            component.eventVisibilityChanged += delegate (UIComponent c, bool v)
            {
                if (v)
                {
                    RefreshData(true, true);
                }
            };

            TransportManager.instance.eventLineColorChanged += (x) =>
            {
                if (x == LineID && m_lineColor != null)
                {
                    m_lineColor.selectedColor = TransportManager.instance.m_lines.m_buffer[x].GetColor();
                }
            };
        }

        protected void Start() => m_lineColor.gameObject.AddComponent<UIColorFieldExtension>();

        public const string LINE_LIST_ITEM_TEMPLATE = "TLM_LineListItemTemplate";
        public static void EnsureTemplate()
        {
            if (UITemplateUtils.GetTemplateDict().ContainsKey(LINE_LIST_ITEM_TEMPLATE))
            {
                return;
            }

            var go = new GameObject();
            var transform = go.transform;
            var m_uIHelper = new UIHelperExtension(go.AddComponent<UIPanel>());
            MonoUtils.CreateUIElement(out UIPanel m_background, transform, "BG");
            m_background.width = 844;
            m_background.height = 38;

            m_uIHelper.Self.width = 844;
            m_uIHelper.Self.height = 38;
            m_background.backgroundSprite = "InfoviewPanel";

            MonoUtils.CreateUIElement(out UILabel m_lineName, transform, "LineName", new Vector4(146, 2, 198, 35));
            m_lineName.textColor = ForegroundColor;
            m_lineName.textAlignment = UIHorizontalAlignment.Center;
            m_lineName.verticalAlignment = UIVerticalAlignment.Middle;
            m_lineName.wordWrap = true;
            MonoUtils.CreateUIElement(out UITextField m_lineNameField, transform, "LineNameField", new Vector4(146, 10, 198, 20));
            m_lineNameField.maxLength = 256;
            m_lineNameField.isVisible = false;
            m_lineNameField.verticalAlignment = UIVerticalAlignment.Middle;
            m_lineNameField.horizontalAlignment = UIHorizontalAlignment.Center;
            m_lineNameField.selectionSprite = "EmptySprite";
            m_lineNameField.builtinKeyNavigation = true;

            CreateLabel(out UILabel m_lineStops, transform);
            m_lineStops.name = "lineStops";

            CreateLabel(out UILabel m_linePassengers, transform);
            m_linePassengers.name = "LinePassengers";


            CreateLabel(out UILabel m_lineBalance, transform);
            m_lineBalance.name = "LineExpenses";
            m_lineBalance.minimumSize = new Vector2(145, 18);


            MonoUtils.CreateUIElement(out UIButton view, transform, "ViewLine", new Vector4(784, 5, 28, 28));
            MonoUtils.InitButton(view, true, "LineDetailButton");


            MonoUtils.CreateUIElement(out UIButton view2, transform, "DeleteLine", new Vector4(816, 5, 28, 28));
            MonoUtils.InitButton(view2, true, "DeleteLineButton");


            MonoUtils.CreateUIElement(out UILabel m_lineVehicles, transform, "LineVehicles");
            m_lineVehicles.autoSize = true;
            m_lineVehicles.pivot = UIPivotPoint.TopLeft;
            m_lineVehicles.verticalAlignment = UIVerticalAlignment.Middle;
            m_lineVehicles.minimumSize = new Vector2(80, 18);
            m_lineVehicles.textAlignment = UIHorizontalAlignment.Center;
            m_lineVehicles.textColor = ForegroundColor;

            MonoUtils.CreateUIElement(out UILabel m_lineBudgetLabel, transform, "LineBudget");
            m_lineBudgetLabel.autoSize = true;
            m_lineBudgetLabel.pivot = UIPivotPoint.TopLeft;
            m_lineBudgetLabel.verticalAlignment = UIVerticalAlignment.Middle;
            m_lineBudgetLabel.minimumSize = new Vector2(80, 18);
            m_lineBudgetLabel.textAlignment = UIHorizontalAlignment.Center;
            m_lineBudgetLabel.textColor = ForegroundColor;


            var m_lineIsVisible = m_uIHelper.AddCheckboxNoLabel("LineVisibility");
            ((UISprite)m_lineIsVisible.checkedBoxObject).spriteName = "LineVisibilityToggleOn";
            ((UISprite)m_lineIsVisible.checkedBoxObject).tooltip = Locale.Get("PUBLICTRANSPORT_HIDELINE");
            ((UISprite)m_lineIsVisible.checkedBoxObject).isTooltipLocalized = true;

            ((UISprite)m_lineIsVisible.checkedBoxObject).size = new Vector2(24, 24);
            ((UISprite)m_lineIsVisible.components[0]).spriteName = "LineVisibilityToggleOff";
            ((UISprite)m_lineIsVisible.components[0]).tooltip = Locale.Get("PUBLICTRANSPORT_SHOWLINE");
            ((UISprite)m_lineIsVisible.components[0]).isTooltipLocalized = true;
            ((UISprite)m_lineIsVisible.components[0]).size = new Vector2(24, 24);
            m_lineIsVisible.relativePosition = new Vector3(20, 10);

            var m_lineColor = m_uIHelper.AddColorPickerNoLabel("LineColor", Color.clear);
            m_lineColor.normalBgSprite = "";
            m_lineColor.focusedBgSprite = "";
            m_lineColor.hoveredBgSprite = "";
            m_lineColor.width = 40;
            m_lineColor.height = 40;
            m_lineColor.atlas = UIView.GetAView().defaultAtlas;

            var m_lineNumberFormatted = m_lineColor.GetComponentInChildren<UIButton>();
            m_lineNumberFormatted.textScale = 1.5f;
            m_lineNumberFormatted.useOutline = true;

            m_lineColor.relativePosition = new Vector3(90, 0);

            //Auto color & Auto Name
            MonoUtils.CreateUIElement(out UIButton buttonAutoName, transform, "AutoNameBtn");
            buttonAutoName.pivot = UIPivotPoint.TopRight;
            buttonAutoName.relativePosition = new Vector3(164, 0);
            buttonAutoName.text = "A";
            buttonAutoName.textScale = 0.6f;
            buttonAutoName.width = 15;
            buttonAutoName.height = 15;
            buttonAutoName.tooltip = Locale.Get("TLM_AUTO_NAME_SIMPLE_BUTTON_TOOLTIP");
            MonoUtils.InitButton(buttonAutoName, true, "ButtonMenu");
            buttonAutoName.isVisible = true;


            MonoUtils.CreateUIElement(out UIButton buttonAutoColor, transform, "AutoColorBtn");
            buttonAutoColor.pivot = UIPivotPoint.TopRight;
            buttonAutoColor.relativePosition = new Vector3(83, 0);
            buttonAutoColor.text = "A";
            buttonAutoColor.textScale = 0.6f;
            buttonAutoColor.width = 15;
            buttonAutoColor.height = 15;
            buttonAutoColor.tooltip = Locale.Get("TLM_AUTO_COLOR_SIMPLE_BUTTON_TOOLTIP");
            MonoUtils.InitButton(buttonAutoColor, true, "ButtonMenu");
            buttonAutoColor.isVisible = true;

            go.AddComponent<UVMLineListItem>();
            UITemplateUtils.GetTemplateDict()[LINE_LIST_ITEM_TEMPLATE] = m_uIHelper.Self;
        }

    }
}
