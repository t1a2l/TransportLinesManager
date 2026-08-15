using System;
using System.Linq;
using System.Text;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Extensions.UI;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base.Enums;
using TransportLinesManager.Data.DataContainers;
using UnityEngine;
using static Commons.UI.DefaultEditorUILib;

namespace TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMSpawnDelayOptionsTab : UICustomControl, ITLMConfigOptionsTab
    {
        // Layout constants.
        private UIComponent parent;

        // Slider display string caching.
        private readonly string approxString = Locale.Get("TLM_TIME_APPROXIMATELY");
        private readonly string hourString = Locale.Get("TLM_TIME_HOUR");
        private readonly string hoursString = Locale.Get("TLM_TIME_HOURS");
        private readonly string minuteString = Locale.Get("TLM_TIME_MINUTE");
        private readonly string minutesString = Locale.Get("TLM_TIME_MINUTES");
        private readonly string secondsNormalString = Locale.Get("TLM_TIME_SECONDS");

        private UIDropDown spawnDelayScope;
        private UISlider busSlider;
        private UISlider tramSlider;
        private UISlider trolleybusSlider;
        private UISlider ferrySlider;
        private UISlider blimpSlider;
        private UISlider passengerHelicopterSlider;
        private UISlider touristBusSlider;

        public void ReloadData()
        {
            if (TLMBaseConfigXML.Instance is null)
            {
                return;
            }
            spawnDelayScope.selectedIndex = (int)TLMBaseConfigXML.CurrentContextConfig.SpawnDelayScope;
            busSlider.value = TLMBaseConfigXML.CurrentContextConfig.BusDelay;
            tramSlider.value = TLMBaseConfigXML.CurrentContextConfig.TramDelay;
            trolleybusSlider.value = TLMBaseConfigXML.CurrentContextConfig.TrolleybusDelay;
            ferrySlider.value = TLMBaseConfigXML.CurrentContextConfig.FerryDelay;
            blimpSlider.value = TLMBaseConfigXML.CurrentContextConfig.BlimpDelay;
            passengerHelicopterSlider.value = TLMBaseConfigXML.CurrentContextConfig.PassengerHelicopterDelay;
            touristBusSlider.value = TLMBaseConfigXML.CurrentContextConfig.TouristBusDelay;
        }

        public void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group7 = new(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel)group7.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)group7.Self).wrapLayout = true;
            ((UIScrollablePanel)group7.Self).width = 730;

            group7.AddLabel(Locale.Get("TLM_SPAWN_DELAY_CONFIG"));
            group7.AddSpace(15);

            AddDropdown(Locale.Get("TLM_SPAWN_DELAY_SCOPE"), out spawnDelayScope, group7, Enum.GetValues(typeof(SpawnDelayScope)).OfType<SpawnDelayScope>().Select(x => Tuple.New(x.GetName(), x)).ToArray(), (x) =>
            {
                TLMBaseConfigXML.CurrentContextConfig.SpawnDelayScope = x;
            });
            group7.AddSpace(15);

            group7.AddLabel(Locale.Get("TLM_SPAWN_DELAY_GLOBAL_DESC"));
            group7.AddSpace(1);
            group7.AddLabel(Locale.Get("TLM_SPAWN_DELAY_DEPOT_DESC"));
            group7.AddSpace(1);
            group7.AddLabel(Locale.Get("TLM_SPAWN_DELAY_LINE_DESC"));

            group7.AddSpace(15);
            
            busSlider = AddDelaySlider(group7, "TLM_BUS_SPAWN_DELAY", TLMBaseConfigXML.CurrentContextConfig.BusDelay);
            busSlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                TLMBaseConfigXML.CurrentContextConfig.BusDelay = (uint)val;
            };
            group7.AddSpace(40);

            tramSlider = AddDelaySlider(group7, "TLM_TRAM_SPAWN_DELAY", TLMBaseConfigXML.CurrentContextConfig.TramDelay);
            tramSlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                TLMBaseConfigXML.CurrentContextConfig.TramDelay = (uint)val;
            };
            group7.AddSpace(40);

            trolleybusSlider = AddDelaySlider(group7, "TLM_TROLLEYBUS_SPAWN_DELAY", TLMBaseConfigXML.CurrentContextConfig.TrolleybusDelay);
            trolleybusSlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                TLMBaseConfigXML.CurrentContextConfig.TrolleybusDelay = (uint)val;
            };
            ((UILabel)trolleybusSlider.objectUserData).width = 700;
            group7.AddSpace(40);

            ferrySlider = AddDelaySlider(group7, "TLM_FERRY_SPAWN_DELAY", TLMBaseConfigXML.CurrentContextConfig.FerryDelay);
            ferrySlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                TLMBaseConfigXML.CurrentContextConfig.FerryDelay = (uint)val;
            };
            group7.AddSpace(40);

            blimpSlider = AddDelaySlider(group7, "TLM_BLIMP_SPAWN_DELAY", TLMBaseConfigXML.CurrentContextConfig.BlimpDelay);
            blimpSlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                TLMBaseConfigXML.CurrentContextConfig.BlimpDelay = (uint)val;
            };
            group7.AddSpace(40);

            passengerHelicopterSlider = AddDelaySlider(group7, "TLM_PASSENGER_HELICOPTER_SPAWN_DELAY", TLMBaseConfigXML.CurrentContextConfig.PassengerHelicopterDelay);
            passengerHelicopterSlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                TLMBaseConfigXML.CurrentContextConfig.PassengerHelicopterDelay = (uint)val;
            };
            group7.AddSpace(40);

            touristBusSlider = AddDelaySlider(group7, "TLM_TOURIST_BUS_SPAWN_DELAY", TLMBaseConfigXML.CurrentContextConfig.TouristBusDelay);
            touristBusSlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                TLMBaseConfigXML.CurrentContextConfig.TouristBusDelay = (uint)val;
            };
            group7.AddSpace(40);
        }

        /// <summary>
        /// Adds a delay slider.
        /// </summary>
        /// <param name="labelKey">Translation key for slider label.</param>
        /// <param name="initialValue">Initial slider value.</param>
        /// <returns>New delay slider with attached game-time label.</returns>
        private UISlider AddDelaySlider(UIHelperExtension uIHelperExtension, string labelKey, uint initialValue)
        {
            // Create new slider.
            UISlider newSlider = UIHelperExtension.AddSlider(uIHelperExtension.Self, Locale.Get(labelKey), 0f, 16636f, 1f, initialValue, (x) => { }, out UILabel label);
            label.width = 500;
            newSlider.width = 600;

            // Value label.
            UILabel valueLabel = newSlider.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.relativePosition = PositionRightOf(newSlider, 8f, -30f);

            // Set initial value and event handler to update on value change.
            valueLabel.text = FormatValue(newSlider.value);
            newSlider.eventValueChanged += (c, value) => valueLabel.text = FormatValue(value);

            // Game-time label.
            UILabel timeLabel = UIHelperExtension.AddLabel(newSlider.parent, string.Empty, 700, out UIPanel timeContainer);
            newSlider.objectUserData = timeLabel;

            // Force set slider value to populate initial time label and add event handler.
            SetTimeLabel(newSlider, initialValue);
            newSlider.eventValueChanged += SetTimeLabel;

            return newSlider;
        }

        /// <summary>
        /// Sets the game-time label for a delay slider.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="value">Slider value.</param>
        private void SetTimeLabel(UIComponent c, float value)
        {
            // Ensure that there's a valid label attached to the slider.
            if (c.objectUserData is UILabel label)
            {
                // Convert frame count to hours per current SimulationManager settings.
                TimeSpan timespan = TimeSpan.FromHours(value / SimulationManager.DAYTIME_HOUR_TO_FRAME);

                // Format label to display hours and minutes.
                StringBuilder labelString = new(((uint)value >> 6).ToString());
                labelString.Append(' ');
                labelString.Append(secondsNormalString);
                labelString.Append(Environment.NewLine);
                labelString.Append(approxString);
                labelString.Append(' ');
                labelString.Append(timespan.Hours);
                labelString.Append(' ');
                labelString.Append(timespan.Hours == 1 ? hourString : hoursString);
                labelString.Append(' ');
                labelString.Append(timespan.Minutes);
                labelString.Append(' ');
                labelString.Append(timespan.Minutes == 1 ? minuteString : minutesString);
                labelString.Append(' ');

                label.text = labelString.ToString();
            }
        }

        private string FormatValue(float value)
        {
            return value.RoundToNearest(1f).ToString("N");
        }

        private static Vector2 PositionRightOf(UIComponent uIComponent, float margin = 8f, float verticalOffset = 0f)
        {
            return new Vector2(uIComponent.relativePosition.x + uIComponent.width + margin, uIComponent.relativePosition.y + verticalOffset);
        }

    }
}
