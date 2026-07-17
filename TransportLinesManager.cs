using System.Collections.Generic;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Globalization;
using Commons.Extensions.UI;
using Commons.Interfaces;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.CommonsWindow;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.MapDrawer;
using TransportLinesManager.OptionsMenu;
using TransportLinesManager.Utils;

[assembly: AssemblyVersion("14.6.0.*")]
namespace TransportLinesManager
{
    public class TransportLinesManagerMod : BasicIUserMod<TransportLinesManagerMod, TLMController, TLMPanel>
    {
        public override string SimpleName => "Transport Lines Manager";

        public override string Description => "Allows to customize and manage your public transport systems.";

        public override bool UseGroup9 => false;

        protected override Dictionary<ulong, string> IncompatibleModList { get; } = [];

        private static readonly SavedInt m_dataVersion = new("TLM_DataVersion", Settings.gameSettingsFile, 0, true);
        private const int DATA_VERSION_14_6 = 1461;

        private static readonly SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel = new("TLM_showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
        private static readonly SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new("TLM_showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
        private static readonly SavedBool m_savedUseGameClockAsReferenceIfNoDayNightCycle = new("TLM_useGameClockAsReferenceIfNoDayNightCycle", Settings.gameSettingsFile, false, true);
        private static readonly SavedBool m_showDistanceInLinearMap = new("TLM_showDistanceInLinearMap", Settings.gameSettingsFile, true, true);

        public TransportLinesManagerMod() : base()
        {
            IncompatibleModList[TLMController.IPT2_MOD_ID] = "IPT2 is incompatible since TLM changes the same code behavior. Isn't recommended to use both together.";
            IncompatibleModList[TLMController.IPT2_ESSENTIALS_MOD_ID] = "IPT2 Essentials is incompatible since TLM changes the same code behavior. Isn't recommended to use both together.";
            IncompatibleModList[TLMController.IPT3_MOD_ID] = "IPT3 is incompatible since TLM changes the same code behavior. Isn't recommended to use both together.";
            IncompatibleModList[TLMController.RETURN_VEHICLE_MOD_ID] = "Transport Vehicle Return Patch is not necessary to use along the TLM since version 14." +
            " With the introduction of the Express Buses system, now the vehicles are emptied in the next terminal stop before get to the depot.";
        }

        protected override List<string> IncompatibleDllModList { get; } =
        [
            "ImprovedPublicTransport2",
            "ImprovedPublicTransportEssentials",
            "ImprovedPublicTransport3"
        ];

        public override void TopSettingsUI(UIHelperExtension helper) => TLMConfigOptions.instance.GenerateOptionsMenu(helper);

        public void PopulateGroup9(UIHelperExtension helper) => CreateGroup9(helper);

        public override void Group9SettingsUI(UIHelperExtension group9)
        {
            group9.AddButton(Locale.Get("TLM_DRAW_CITY_MAP"), TLMMapDrawer.DrawCityMap);
            group9.AddButton("Open generated map folder", () => ColossalFramework.Utils.OpenInFileBrowser(TLMController.ExportedMapsFolder));
            group9.AddSpace(2);
            group9.AddButton(Locale.Get("TLM_RELOAD_DEFAULT_CONFIGURATION"), () =>
            {
                TLMBaseConfigXML.ReloadGlobalFile();
                TLMConfigOptions.instance.ReloadData();
            });
            if (IsCityLoaded)
            {
                group9.AddButton(Locale.Get("TLM_SAVE_CURRENT_CITY_CONFIG_AS_DEFAULT"), () =>
                {
                    TLMBaseConfigXML.Instance.ExportAsGlobalConfig();
                    TLMConfigWarehouse.GetConfig(null, null).ReloadFromDisk();
                    TLMConfigOptions.instance.ReloadData();
                });
                group9.AddButton(Locale.Get("TLM_LOAD_DEFAULT_AS_CURRENT_CITY_CONFIG"), () =>
                {
                    TLMBaseConfigXML.Instance.LoadFromGlobal();
                    TLMConfigOptions.instance.ReloadData();
                });

            }
            else
            {
                group9.AddButton(Locale.Get("TLM_SAVE_CURRENT_CITY_CONFIG_AS_DEFAULT"), TLMBaseConfigXML.GlobalFile.ExportAsGlobalConfig);
            }
            TLMConfigOptions.instance.ReloadData();
            base.Group9SettingsUI(group9);
        }

        public static bool ShowNearLinesPlop
        {
            get => m_savedShowNearLinesInCityServicesWorldInfoPanel.value;
            set => m_savedShowNearLinesInCityServicesWorldInfoPanel.value = value;
        }

        public static bool ShowNearLinesGrow
        {
            get => m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value;
            set => m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value = value;
        }

        public static bool ShowDistanceLinearMap
        {
            get => m_showDistanceInLinearMap.value;
            set => m_showDistanceInLinearMap.value = value;
        }

        public static bool UseGameClockAsReferenceIfNoDayNight
        {
            get => m_savedUseGameClockAsReferenceIfNoDayNightCycle.value;
            set => m_savedUseGameClockAsReferenceIfNoDayNightCycle.value = value;
        }

        public override string IconName => "TLM_Icon";

        protected override void OnLevelLoadingInternal()
        {
            base.OnLevelLoadingInternal();

            TLMController.VerifyIfRealTimeIsEnabled();
            TransportSystemDefinition.TransportInfoDict.ToString();

            if (m_dataVersion.value < DATA_VERSION_14_6)
            {
                TLMController.MigrateOldVehicleCountData();
                TLMController.MigrateLegacyDefaultTicketPrice();
                m_dataVersion.value = DATA_VERSION_14_6;
            }

            if (TLMController.IsSchoolBusesEnabled)
            {
                SchoolBusUtils.SetExternalSpawnControl(true);
                SchoolBusUtils.SetVehicleSupplyEnabled(false);
            }
        }

        protected override Tuple<string, string> GetButtonLink() => Tuple.New("TLM's GitHub", "https://github.com/t1a2l/TransportLinesManager");
    }
}
