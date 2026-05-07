using System;
using System.Linq;
using System.Collections.Generic;
using ColossalFramework.Plugins;
using UnityEngine;
using WriteEverywhere.Singleton;
using WriteEverywhere.ModShared;
using WriteEverywhere.TransportLines;
using TransportLinesManager.ModShared;
using TransportLinesManager.Data.Tsd;

namespace TLM2WE
{
    internal class BridgeWE : IBridgeTLM
    {
        public override int Priority => -1000;

        public override bool IsBridgeEnabled => true;

        public BridgeWE()
        {
            if (!PluginManager.instance.GetPluginsInfo().Any(x => x.assemblyCount > 0 && x.isEnabled && x.ContainsAssembly(typeof(WEFacade).Assembly)))
            {
                throw new Exception("The WE bridge isn't available due to the mod not being active. Using fallback!");
            }
            TLMFacade.Instance.EventLineSymbolParameterChanged += () =>
            {
                WTSCacheSingleton.ClearCacheLineId();
            };
            TLMFacade.Instance.EventAutoNameParameterChanged += WEFacade.OnAutoNameParameterChanged;
            TLMFacade.Instance.EventVehicleIdentifierParameterChanged += WTSCacheSingleton.ClearCacheVehicleNumber;
            TLMFacade.Instance.EventLineDestinationsChanged += (lineId) =>
            {
                WEFacade.BuildingPropsSingleton.ResetLines();
                WTSCacheSingleton.ClearCacheLineName(new WTSLine(lineId, false));
            };
            TLMFacade.Instance.EventRegionalLineParameterChanged += (lineId) =>
            {
                WEFacade.BuildingPropsSingleton.ResetLines();
                WTSCacheSingleton.ClearCacheLineName(new WTSLine(lineId, true));
            };
        }

        public override LineLogoParameter GetLineLogoParameters(WTSLine lineObj)
        {
            var result = TLMFacade.GetIconStringParameters(lineObj.lineId, lineObj.regional);
            return new LineLogoParameter(result.First, result.Second, result.Third);
        }

        public override string GetStopName(ushort stopId, WTSLine lineObj)
            => TLMFacade.GetFullStationName(
                stopId,
                lineObj.lineId,
                lineObj.regional,
                TransportSystemDefinition.GetDefinitionForLine(lineObj.lineId, lineObj.regional)?.SubService ?? (lineObj.regional ? NetManager.instance.m_nodes.m_buffer[lineObj.lineId].Info.GetSubService() : default)
                );
        public override ushort GetStopBuildingInternal(ushort stopId, WTSLine lineObj) => TLMFacade.GetStationBuilding(stopId, lineObj.lineId, lineObj.regional);
        public override string GetLineSortString(WTSLine lineObj) => TLMFacade.GetLineSortString(lineObj.lineId, lineObj.regional);

        public override WTSLine GetVehicleLine(ushort vehicleId) => new WTSLine(TLMFacade.Instance.GetVehicleLine(vehicleId, out bool regional), regional);
        public override string GetLineIdString(WTSLine lineObj) => TLMFacade.GetLineStringId(lineObj.lineId, lineObj.regional);

        public override WTSLine GetStopLine(ushort stopId)
        {
            var lineId = (ushort)TLMFacade.GetStopLine(stopId, out bool isBuilding);
            return new WTSLine(lineId, isBuilding);
        }

        public override string GetLineName(WTSLine lineObj) => TLMFacade.GetLineName(lineObj.lineId, lineObj.regional);
        public override Color GetLineColor(WTSLine lineObj) => TLMFacade.GetLineColor(lineObj.lineId, lineObj.regional);

        public override void MapLineDestinations(WTSLine lineObj, ref StopInformation[] cacheToUpdate)
        {
            TLMFacade.CalculateAutoName(lineObj.lineId, lineObj.regional, out List<TLMFacade.DestinationPoco> destinations);
            FillStops(lineObj, destinations.Select(x => new DestinationPoco { stopId = x.stopId, stopName = x.stopName }).ToList(), ref cacheToUpdate);
        }
    }
}