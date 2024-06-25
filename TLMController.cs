using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using Commons.Interfaces;
using Commons.Utils;
using Commons.Utils.StructExtensions;
using TransportLinesManager.Cache.BuildingData;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.ModShared;
using TransportLinesManager.Overrides;
using TransportLinesManager.UI;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels;
using TransportLinesManager.WorldInfoPanels.NearLines;
using TransportLinesManager.WorldInfoPanels.PlatformEditor;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TransportLinesManager
{
    public class TLMController : BaseController<TransportLinesManagerMod, TLMController>
    {
        internal static TLMController Instance => TransportLinesManagerMod.Controller;

        public bool initializedWIP = false;

        public static readonly string FOLDER_NAME = "TransportLinesManager";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string PALETTE_SUBFOLDER_NAME = "ColorPalettes";
        public const string EXPORTED_MAPS_SUBFOLDER_NAME = "ExportedMaps";
        public const ulong REALTIME_MOD_ID = 1420955187;
        public const ulong IPT2_MOD_ID = 928128676;
        public const ulong RETURN_VEHICLE_MOD_ID = 2101977903UL;
        public BuildingTransportLinesCache BuildingLines { get; private set; }

        private bool? m_isRealTimeEnabled = null;
        protected static string GlobalBaseConfigFileName { get; } = "TLM_GlobalData.xml";
        public static string GlobalBaseConfigPath { get; } = Path.Combine(FOLDER_PATH, GlobalBaseConfigFileName);

        public static bool IsRealTimeEnabled
        {
            get
            {
                if (Instance?.m_isRealTimeEnabled == null)
                {
                    VerifyIfIsRealTimeEnabled();
                }
                return Instance?.m_isRealTimeEnabled == true;
            }
        }
        public static void VerifyIfIsRealTimeEnabled()
        {
            if (Instance != null)
            {
                Instance.m_isRealTimeEnabled = VerifyModEnabled(REALTIME_MOD_ID);
            }
        }

        public static bool IsIPT2Enabled() => VerifyModEnabled(IPT2_MOD_ID);

        private static bool VerifyModEnabled(ulong modId)
        {
            PluginManager.PluginInfo pluginInfo = Singleton<PluginManager>.instance.GetPluginsInfo().FirstOrDefault((PluginManager.PluginInfo pi) => pi.publishedFileID.AsUInt64 == modId);
            return !(pluginInfo == null || !pluginInfo.isEnabled);
        }

        public static string PalettesFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + PALETTE_SUBFOLDER_NAME;
        public static string ExportedMapsFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + EXPORTED_MAPS_SUBFOLDER_NAME;

        public ushort CurrentSelectedId { get; private set; }
        public void SetCurrentSelectedId(ushort line) => CurrentSelectedId = line;

        internal TLMLineCreationToolbox LineCreationToolbox => PublicTransportInfoViewPanelOverrides.Toolbox;

        public TLMFacade SharedInstance { get; internal set; }
        internal IBridgeADR ConnectorADR { get; private set; }
        internal IBridgeWTS ConnectorWTS { get; private set; }

        private bool m_dirtyRegionalLines;

        public static Color AutoColor(ushort i, bool ignoreRandomIfSet = true, bool ignoreAnyIfSet = false)
        {
            ref TransportLine t = ref TransportManager.instance.m_lines.m_buffer[i];
            try
            {
                var tsd = TransportSystemDefinition.GetDefinitionForLine(i, false);
                if (tsd == default || (((t.m_flags & TransportLine.Flags.CustomColor) > 0) && ignoreAnyIfSet))
                {
                    return Color.clear;
                }
                Color c = TLMPrefixesUtils.CalculateAutoColor(t.m_lineNumber, tsd, ((t.m_flags & TransportLine.Flags.CustomColor) > 0) && ignoreRandomIfSet, true);
                if (c.a == 1)
                {
                    Instance.StartCoroutine(TLMLineUtils.RunColorChange(Instance, i, c));
                }
                else
                {
                    c = Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_color;
                }
                LogUtils.DoLog("Colocada a cor #{0} na linha {1} ({3} {2})", c.ToRGB(), i, t.m_lineNumber, t.Info.m_transportType);
                return c;
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("ERRO!!!!! " + e.Message);
                TLMBaseConfigXML.Instance.UseAutoColor = false;
                return Color.clear;
            }
        }

        public static void AutoName(ushort m_LineID) => TLMLineUtils.SetLineName(m_LineID, TLMLineUtils.CalculateAutoName(m_LineID, false, out _));

        //------------------------------------

        protected override void StartActions()
        {
            BuildingLines = gameObject.AddComponent<BuildingTransportLinesCache>();

            TLMTransportTypeDataContainer.Instance.RefreshCapacities();

            StartCoroutine(VehicleUtils.UpdateCapacityUnits());
            InitWipSidePanels();

            m_dirtyRegionalLines = true;
        }

        internal static bool UpdateRegionalLinesFromBuilding(ushort buildingId)
        {
            if (BuildingManager.instance.m_buildings.m_buffer[buildingId].Info.m_buildingAI is TransportStationAI)
            {
                TransportLinesManagerMod.Controller.m_dirtyRegionalLines = true;
                return true;
            }
            return false;
        }
        internal static bool UpdateRegionalLinesFromNode(ushort nodeId)
        {
            if (NetManager.instance.m_nodes.m_buffer[nodeId].Info.m_netAI is TransportLineAI && NetManager.instance.m_nodes.m_buffer[nodeId].m_transportLine == 0)
            {
                TransportLinesManagerMod.Controller.m_dirtyRegionalLines = true;
                return true;
            }
            return false;
        }

        public void Update()
        {
            if (m_dirtyRegionalLines)
            {
                foreach (var item in TLMBuildingDataContainer.Instance.GetAvailableEntries())
                {
                    if (item.TlmManagedRegionalLines)
                    {
                        TransportLinesManagerMod.Controller.BuildingLines.SafeGet((ushort)(item.Id ?? 0));
                        if (TLMFacade.Instance != null)
                        {
                            foreach (var plats in item.PlatformMappings.Values)
                            {
                                foreach (var regLine in plats.TargetOutsideConnections)
                                {
                                    TLMFacade.Instance.OnRegionalLineParameterChanged(regLine.Value.m_nodeStation);
                                }
                            }
                        }
                    }
                }
                m_dirtyRegionalLines = false;
            }
        }

        private static void InitWipSidePanels()
        {
            BuildingWorldInfoPanel[] panelList = UIView.GetAView().GetComponentsInChildren<BuildingWorldInfoPanel>();
            LogUtils.DoLog("WIP LIST: [{0}]", string.Join(", ", panelList.Select(x => x.name).ToArray()));
            TLMLineItemButtonControl.EnsureTemplate();
            foreach (BuildingWorldInfoPanel wip in panelList)
            {
                LogUtils.DoLog("LOADING WIP HOOK FOR: {0}", wip.name);
                UIComponent parent = wip.GetComponent<UIComponent>();
                if (parent is null)
                {
                    continue;
                }
                MonoUtils.CreateUIElement(out UIPanel parent2, parent.transform, "TLMSidePanels", new Vector4(parent.width + 15, 50, 300, 0));
                parent2.autoLayout = true;
                parent2.autoLayoutPadding.bottom = 5;
                parent2.autoFitChildrenVertically = true;
                parent2.autoLayoutDirection = LayoutDirection.Vertical;
                var isGrow = wip is ZonedBuildingWorldInfoPanel;
                var controller = TLMNearLinesController.InitPanelNearLinesOnWorldInfoPanel(parent2);
                parent.eventVisibilityChanged += (x, y) => controller?.EventWIPChanged(isGrow);
                parent.eventPositionChanged += (x, y) => controller?.EventWIPChanged(isGrow);
                parent.eventSizeChanged += (x, y) => controller?.EventWIPChanged(isGrow);
                if (wip is CityServiceWorldInfoPanel)
                {
                    var controllerP = TLMRegionalPlatformSelection.Init(parent2);
                    parent.eventVisibilityChanged += (x, y) => controllerP?.EventWIPChanged();
                    parent.eventPositionChanged += (x, y) => controllerP?.EventWIPChanged();
                    parent.eventSizeChanged += (x, y) => controllerP?.EventWIPChanged();
                    NetManagerOverrides.EventNodeChanged += (x) =>
                    {
                        controllerP?.EventWIPChanged();
                        controller?.EventWIPChanged(isGrow);
                    };
                }

            }

        }


        public void OpenTLMPanel() => TransportLinesManagerMod.Instance.OpenPanelAtModTab();
        public void CloseTLMPanel() => TransportLinesManagerMod.Instance.ClosePanel();

        public IEnumerator RenameCoroutine(ushort id, string newName)
        {
            if (Singleton<SimulationManager>.exists)
            {
                AsyncTask<bool> task = Singleton<SimulationManager>.instance.AddAction(Singleton<TransportManager>.instance.SetLineName(id, newName));
                yield return task.WaitTaskCompleted(this);
                UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
                if (id > 0 && lineId == id && !fromBuilding)
                {
                    UVMPublicTransportWorldInfoPanel.m_obj.m_nameField.text = Singleton<TransportManager>.instance.GetLineName(id);
                }
            }
            yield break;
        }

        internal void Awake()
        {
            SharedInstance = gameObject.AddComponent<TLMFacade>();
            ConnectorADR = PluginUtils.GetImplementationTypeForMod<BridgeADRFallback, IBridgeADR>(gameObject, "Addresses", "2.99.99.0", "TransportLinesManager.ModShared.BridgeADR");
            ConnectorWTS = PluginUtils.GetImplementationTypeForMod<BridgeWTSFallback, IBridgeWTS>(gameObject, "WriteTheSigns", "0.3.0.0", "TransportLinesManager.ModShared.BridgeWTS");
        }

        internal static readonly Color[] COLOR_ORDER =
             [
                Color.red,
                Color.Lerp(Color.red, Color.yellow,0.5f),
                Color.yellow,
                Color.green,
                Color.cyan,
                Color.blue,
                Color.Lerp(Color.blue, Color.magenta,0.5f),
                Color.magenta,
                Color.white,
                Color.black,
                Color.Lerp( Color.red,                                    Color.black,0.5f),
                Color.Lerp( Color.Lerp(Color.red, Color.yellow,0.5f),     Color.black,0.5f),
                Color.Lerp( Color.yellow,                                 Color.black,0.5f),
                Color.Lerp( Color.green,                                  Color.black,0.5f),
                Color.Lerp( Color.cyan,                                   Color.black,0.5f),
                Color.Lerp( Color.blue,                                   Color.black,0.5f),
                Color.Lerp( Color.Lerp(Color.blue, Color.magenta,0.5f),   Color.black,0.5f),
                Color.Lerp( Color.magenta,                                Color.black,0.5f),
                Color.Lerp( Color.white,                                  Color.black,0.25f),
                Color.Lerp( Color.white,                                  Color.black,0.75f)
             ];
    }

}