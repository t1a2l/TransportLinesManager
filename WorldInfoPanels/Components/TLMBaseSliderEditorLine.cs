using System;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Extensions.UI;
using Commons.UI.SpriteNames;
using Commons.Utils;
using TransportLinesManager.Data.Base;
using System.Collections.Generic;
using TransportLinesManager.WorldInfoPanels.Tabs;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Components
{
    public abstract class TLMBaseSliderEditorLine<L, V> : UICustomControl where L : TLMBaseSliderEditorLine<L, V> where V : UintValueHourEntryXml<V>
    {
        protected UIPanel m_container;
        protected UITextField m_timeInput;
        protected UILabel m_value;
        private UITextField m_valueField;
        protected UIButton m_die;
        protected UISlider m_valueSlider;
        protected UIButton m_default;

        private bool m_loading = false;

        public int ZOrder
        {
            get => m_container.zOrder;
            set => m_container.zOrder = value;
        }

        public V Entry
        {
            get => m_entry; 
            set
            {
                m_entry = value;
                FillData();
            }
        }

        private Action<V> m_onDie;
        private Action<V> m_onDefault;

        public Action<V> OnDie
        {
            get => m_onDie;
            set
            {
                m_onDie = value;
                if (m_die != null)
                {
                    m_die.isEnabled = value != null;
                    m_die.isInteractive = value != null;
                }
            }
        }

        public Action<V> OnDefault
        {
            get => m_onDefault;
            set
            {
                m_onDefault = value;
                if (m_default != null)
                {
                    m_default.isEnabled = value != null;
                    m_default.isInteractive = value != null;
                }
            }
        }

        public Action<V, int> OnTimeChanged;
        public Action<V, float> OnBudgetChanged;
        private V m_entry;

        private bool alreadyCalling = false;

        public void Awake()
        {
            m_container = GetComponent<UIPanel>();
            m_value = Find<UILabel>("ValueLabel");
            m_valueField = Find<UITextField>("ValueField");
            m_timeInput = Find<UITextField>("HourInput");
            m_timeInput.eventTextSubmitted += SendText;
            m_valueSlider = GetComponentInChildren<UISlider>();
            m_valueSlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                SetValue(val);
            };
            m_die = Find<UIButton>("Delete");
            m_die?.eventClick += (component, eventParam) => m_onDie?.Invoke(Entry);
            m_default = Find<UIButton>("Default");
            m_default?.eventClick += (component, eventParam) => m_onDefault?.Invoke(Entry);
            MonoUtils.LimitWidthAndBox(m_value, 60, out UIPanel container, true);

            container.AttachUIComponent(m_valueField.gameObject);

            m_value.eventMouseEnter += delegate (UIComponent c, UIMouseEventParameter r)
            {
                m_value.backgroundSprite = "TextFieldPanelHovered";
            };
            m_value.eventMouseLeave += delegate (UIComponent c, UIMouseEventParameter r)
            {
                m_value.backgroundSprite = string.Empty;
            };
            m_value.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
                if (!fromBuilding)
                {
                    m_value.Hide();
                    m_valueField.Show();
                    m_valueField.text = GetValueAsInt(ref TransportManager.instance.m_lines.m_buffer[lineId]).ToString();
                    m_valueField.Focus();
                }
            };
            m_valueField.eventLeaveFocus += delegate (UIComponent c, UIFocusEventParameter r)
            {
                UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
                if (!fromBuilding)
                {
                    m_valueField.Hide();
                    if (uint.TryParse(m_valueField.text, out uint val))
                    {
                        SetValueFromTyping(ref TransportManager.instance.m_lines.m_buffer[lineId], val);
                    }
                    m_value.Show();
                }

            };

        }

        public void SetSliderParams(Color c, float maxValue)
        {
            ((UISprite)m_valueSlider.thumbObject).color = c;
            m_valueSlider.maxValue = maxValue;
        }

        public string GetCurrentVal() => m_timeInput.text;

        public abstract string GetValueFormat(ref TransportLine t);

        public abstract uint GetValueAsInt(ref TransportLine t);

        public abstract void SetValueFromTyping(ref TransportLine t, uint value);

        protected static void EnsureTemplate(string templateName)
        {
            if (UITemplateUtils.GetTemplateDict().ContainsKey(templateName))
            {
                return;
            }
            var go = new GameObject();

            var m_container = go.AddComponent<UIPanel>();
            m_container.width = 350;
            m_container.height = 30;
            m_container.autoLayout = true;
            m_container.autoLayoutDirection = LayoutDirection.Horizontal;
            m_container.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_container.wrapLayout = false;
            m_container.name = "EntryLine";

            MonoUtils.CreateUIElement(out UITextField m_timeInput, m_container.transform, "HourInput");
            MonoUtils.UiTextFieldDefaults(m_timeInput);
            m_timeInput.normalBgSprite = "OptionsDropboxListbox";
            m_timeInput.width = 50;
            m_timeInput.height = 28;
            m_timeInput.textScale = 1.6f;
            m_timeInput.maxLength = 2;
            m_timeInput.numericalOnly = true;
            m_timeInput.text = "0";
            m_timeInput.submitOnFocusLost = true;

            MonoUtils.CreateUIElement(out UILabel m_value, m_container.transform, "ValueLabel");
            m_value.autoSize = false;
            m_value.width = 60;
            m_value.height = 30;
            m_value.textScale = 1.125f;
            m_value.textAlignment = UIHorizontalAlignment.Center;
            m_value.padding = new RectOffset(3, 3, 5, 3);
            m_value.processMarkup = true;

            MonoUtils.CreateUIElement(out UITextField m_valueField, m_container.transform, "ValueField", new Vector4(0, 0, 60, 26));
            m_valueField.numericalOnly = true;
            m_valueField.allowNegative = false;
            m_valueField.allowFloats = false;
            m_valueField.text = "";
            m_valueField.maxLength = 5;
            m_valueField.verticalAlignment = UIVerticalAlignment.Middle;
            m_valueField.horizontalAlignment = UIHorizontalAlignment.Center;
            m_valueField.selectionSprite = "EmptySprite";
            m_valueField.builtinKeyNavigation = true;
            m_valueField.isVisible = false;
            m_valueField.padding.top = 5;
            m_valueField.textScale = 1.125f;

            var m_ValueSlider = UIHelperExtension.AddSlider(m_container, null, 0, 500, 5, -1, (x) => { });
            Destroy(m_ValueSlider.transform.parent.GetComponentInChildren<UILabel>());
            UIPanel budgetSliderPanel = m_ValueSlider.GetComponentInParent<UIPanel>();

            budgetSliderPanel.width = 170;
            budgetSliderPanel.height = 20;
            budgetSliderPanel.autoLayout = true;

            m_ValueSlider.size = new Vector2(170, 20);
            m_ValueSlider.scrollWheelAmount = 0;
            m_ValueSlider.clipChildren = true;
            m_ValueSlider.thumbOffset = new Vector2(-200, 0);
            m_ValueSlider.color = new Color32(128, 128, 128, 128);
            m_ValueSlider.name = "ValueSlider";
            m_ValueSlider.thumbObject.width = 400;
            m_ValueSlider.thumbObject.height = 20;
            ((UISprite)m_ValueSlider.thumbObject).spriteName = "PlainWhite";
            ((UISprite)m_ValueSlider.thumbObject).color = new Color32(1, 140, 46, 255);

            MonoUtils.CreateUIElement(out UIButton m_default, m_container.transform, "Default");
            m_default.textScale = 1f;
            m_default.width = 25;
            m_default.height = 25;
            m_default.tooltip = Locale.Get("TLM_SET_TO_DEFAULT_STOP_TICKET_PRICE_LIST");
            MonoUtils.InitButton(m_default, true, "OptionBase");
            m_default.isVisible = true;
            m_default.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            m_default.normalFgSprite = ResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.Reload);

            MonoUtils.CreateUIElement(out UIButton m_die, m_container.transform, "Delete");
            m_die.textScale = 1f;
            m_die.width = 25;
            m_die.height = 25;
            m_die.tooltip = Locale.Get("TLM_DELETE_STOP_TICKET_PRICE_LIST");
            MonoUtils.InitButton(m_die, true, "OptionBase");
            m_die.isVisible = true;
            m_die.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            m_die.normalFgSprite = ResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.Delete);

            go.AddComponent<L>();

            UITemplateUtils.GetTemplateDict()[templateName] = m_container;
        }

        protected virtual IEnumerable<V> GetEntriesForDuplicateCheck(ushort lineId)
        {
            return null;
        }

        protected virtual void ExtraOnFillData(ref TransportLine t) { }

        protected void SetValue(float val)
        {
            if (m_loading)
            {
                return;
            }

            OnBudgetChanged?.Invoke(Entry, val);
            FillData();
            TLMAssetSelectorTab.MarkDirty();
        }

        private void FillData()
        {
            m_loading = true;
            try
            {
                UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
                if (!fromBuilding)
                {
                    ref TransportLine t = ref TransportManager.instance.m_lines.m_buffer[lineId];
                    m_timeInput.text = Entry.HourOfDay.ToString();
                    ExtraOnFillData(ref t);
                    m_valueSlider.value = Entry.Value;
                    m_value.text = GetValueFormat(ref t);
                }
            }
            finally
            {
                m_loading = false;
            }
        }

        private void SendText(UIComponent x, string y)
        {
            if (m_loading)
            {
                return;
            }
            if (alreadyCalling)
            {
                LogUtils.DoErrorLog($"TLMTicketPriceEditorLine: Recursive eventTextChanged call! {Environment.StackTrace}");
            }
            alreadyCalling = true;
            try
            {
                if (int.TryParse(y, out int time))
                {
                    if (time < 0 || time > 23)
                    {
                        m_timeInput.color = Color.red;
                    }
                    else
                    {
                        bool isDuplicate = false;
                        if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && !fromBuilding)
                        {
                            var entries = GetEntriesForDuplicateCheck(lineId);
                            if (entries != null)
                            {
                                foreach (var entry in entries)
                                {
                                    if (ReferenceEquals(entry, Entry))
                                    {
                                        continue;
                                    }
                                    if (entry.HourOfDay == time)
                                    {
                                        isDuplicate = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (isDuplicate)
                        {
                            m_timeInput.color = Color.red;
                        }
                        else
                        {
                            m_timeInput.color = Color.white;
                            m_timeInput.tooltip = string.Empty;
                            OnTimeChanged?.Invoke(Entry, time);
                        }
                    }
                }
            }
            finally
            {
                alreadyCalling = false;
            }

        }
    }

}

