using ColossalFramework;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Cache.BuildingData;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.Base.ConfigurationContainers.OutsideConnections;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TransportLinesManager.ModShared
{
    public class TLMFacade : MonoBehaviour
    {
        public static TLMFacade Instance => TransportLinesManagerMod.Controller?.SharedInstance;

        internal void OnLineSymbolParameterChanged()
        {
            if (LoadingManager.instance.m_loadingComplete)
            {
                EventLineSymbolParameterChanged?.Invoke();
            }
        }

        internal void OnAutoNameParameterChanged()
        {
            if (LoadingManager.instance.m_loadingComplete)
            {
                EventAutoNameParameterChanged?.Invoke();
            }
        }

        internal void OnVehicleIdentifierParameterChanged()
        {
            if (LoadingManager.instance.m_loadingComplete)
            {
                EventVehicleIdentifierParameterChanged?.Invoke();
            }
        }

        internal void OnRegionalLineParameterChanged(ushort regionalLine)
        {
            if (LoadingManager.instance.m_loadingComplete)
            {
                EventRegionalLineParameterChanged?.Invoke(regionalLine);
            }
        }
        internal void OnLineDestinationsChanged(ushort lineId)
        {
            if (LoadingManager.instance.m_loadingComplete)
            {
                EventLineDestinationsChanged?.Invoke(lineId);
                if (TLMBaseConfigXML.Instance.UseAutoName)
                {
                    TLMController.AutoName(lineId);
                }
            }
        }

        public event Action EventLineSymbolParameterChanged;
        public event Action EventAutoNameParameterChanged;
        public event Action EventVehicleIdentifierParameterChanged;
        public event Action<ushort> EventLineDestinationsChanged;
        public event Action<ushort> EventRegionalLineParameterChanged;

        [Obsolete("Use version with regional line flag", true)]
        public static string GetFullStationName(ushort stopId, ushort lineId, ItemClass.SubService subService) =>
             GetFullStationName(stopId, lineId, false, subService);
        public ushort GetVehicleLine(ushort vehicleId, out bool regional) => TLMVehicleUtils.GetVehicleLine(vehicleId, out regional);
        public static string GetFullStationName(ushort stopId, ushort lineId, bool regional, ItemClass.SubService subService) =>
            stopId == 0 ? ""
                : TLMLineUtils.IsRoadLine(lineId, regional) ? TLMStationUtils.GetFullStationName(stopId, lineId, subService, regional)
                : TLMStationUtils.GetStationName(stopId, lineId, subService, regional);

        [Obsolete("Use version with regional line flag", true)]
        public static Tuple<string, Color, string> GetIconStringParameters(ushort lineID) => TLMLineUtils.GetIconStringParameters(lineID, false);
        public static int GetStopLine(ushort stopId, out bool isBuilding) => TLMLineUtils.GetStopLine(stopId, out isBuilding);
        public static string GetLineName(ushort lineId, bool regional) => TLMLineUtils.GetLineName(lineId, regional);
        public static Tuple<string, Color, string> GetIconStringParameters(ushort lineID, bool regionalLine) => TLMLineUtils.GetIconStringParameters(lineID, regionalLine);
        [Obsolete("Use version with regional line flag", true)]
        public static ushort GetStationBuilding(ushort stopId, ushort lineId) => TLMStationUtils.GetStationBuilding(stopId, lineId, false);
        public static Color GetLineColor(ushort lineId, bool regional) => TLMLineUtils.GetLineColor(lineId, regional);
        public static ushort GetStationBuilding(ushort stopId, ushort lineId, bool regional) => TLMStationUtils.GetStationBuilding(stopId, lineId, regional);

        [Obsolete("Use version with regional line flag", true)]
        public static string GetLineSortString(ushort lineId, ref TransportLine transportLine) => TLMLineUtils.GetLineSortString(lineId, false);
        public static string GetLineSortString(ushort lineId, bool regional) => TLMLineUtils.GetLineSortString(lineId, regional);

        public string GetVehicleIdentifier(ushort vehicleId)
        {
            var firstVehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].GetFirstVehicle(vehicleId);
            ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicle];
            var tsd = TransportSystemDefinition.From(vehicle.Info);
            if(tsd is null)
            {
                return vehicleId.ToString("X4");
            }
            var tlId = vehicle.m_transportLine;
            ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[tlId];

            var config = tsd.GetConfig();
            string identifierFormat = (tlId == 0 && vehicle.m_targetBuilding != 0 ? config.VehicleIdentifierFormatForeign : config.VehicleIdentifierFormatLocal);

            string result = "";

            string linePrefix = null;
            string depotPrefix = null;
            string modelPrefix = null;
            string vehicleString = null;
            string vehicleNthDepot = null;
            string vehicleNthTrailer = null;

            string GetLinePrefix()
            {
                if (linePrefix == null)
                {
                    ref TransportLine tl2 = ref TransportManager.instance.m_lines.m_buffer[tlId];
                    if (TLMPrefixesUtils.HasPrefix(ref tl2))
                    {
                        var tsd2 = TransportSystemDefinition.FromLocal(tl2.Info);
                        var prefix = (int)TLMPrefixesUtils.GetPrefix(tlId);
                        linePrefix = TLMPrefixesUtils.GetStringFromNameMode(tsd2.GetConfig().Prefix, prefix).Trim().PadLeft(3, '\0');
                    }
                    else
                    {
                        linePrefix = "\0\0\0";
                    }
                }
                return linePrefix;
            }
            string GetDepotPrefix()
            {
                if (depotPrefix == null)
                {
                    depotPrefix = VehicleManager.instance.m_vehicles.m_buffer[firstVehicle].m_sourceBuilding.ToString("D3");
                    depotPrefix = depotPrefix.Substring(depotPrefix.Length - 3, 3);
                }
                return depotPrefix;
            }
            string GetModelPrefix()
            {
                if (modelPrefix == null)
                {
                    var info = VehicleManager.instance.m_vehicles.m_buffer[firstVehicle].Info;

                    modelPrefix = (info.name.Contains(".") ? info.name.Split(['.'], 2)[1] : info.name).ToUpper().Substring(0, 3);
                }
                return modelPrefix;
            }

            string GetVehicleInstanceString()
            {
                vehicleString ??= vehicleId.ToString().PadLeft(5, '\0');
                return vehicleString;
            }

            string GetVehicleNthDepot()
            {
                if (vehicleNthDepot == null)
                {
                    int counter = 0;
                    ref Vehicle[] vBuffer = ref VehicleManager.instance.m_vehicles.m_buffer;
                    var targetVehicle = firstVehicle;
                    var depotId = vBuffer[targetVehicle].m_sourceBuilding;
                    ref Building[] buffer = ref BuildingManager.instance.m_buildings.m_buffer;
                    var nextVehicle = buffer[depotId].m_ownVehicles;
                    while (nextVehicle != targetVehicle)
                    {
                        counter++;
                        nextVehicle = vBuffer[nextVehicle].m_nextOwnVehicle;
                        if (nextVehicle == 0)
                        {
                            counter = -1;
                            break;
                        }
                        if (counter > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!A\n" + Environment.StackTrace);
                            break;
                        }
                    }

                    vehicleNthDepot = counter.ToString().PadLeft(3, '\0'); ;
                }
                return vehicleNthDepot;
            }

            string GetVehicleNthTrailer()
            {
                if (vehicleNthTrailer == null)
                {
                    int counter = 0;
                    ref Vehicle[] vBuffer = ref VehicleManager.instance.m_vehicles.m_buffer;
                    var nextVehicle = firstVehicle;
                    while (nextVehicle != vehicleId)
                    {
                        nextVehicle = vBuffer[nextVehicle].m_trailingVehicle;
                        counter++;
                        if (nextVehicle == 0)
                        {
                            counter = -1;
                            break;
                        }
                        if (counter > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!B\n" + Environment.StackTrace);
                            break;
                        }
                    }

                    vehicleNthTrailer = counter.ToString().PadLeft(3, '\0'); ;
                }
                return vehicleNthTrailer;
            }


            char GetLetter(char item)
            {
                return item switch
                {
                    'D' => GetDepotPrefix()[0],
                    'E' => GetDepotPrefix()[1],
                    'F' => GetDepotPrefix()[2],
                    'M' => GetModelPrefix()[0],
                    'N' => GetModelPrefix()[1],
                    'O' => GetModelPrefix()[2],
                    'P' => GetLinePrefix()[0],
                    'Q' => GetLinePrefix()[1],
                    'R' => GetLinePrefix()[2],
                    'J' => GetVehicleNthTrailer().Replace('\0', '0')[0],
                    'K' => GetVehicleNthTrailer().Replace('\0', '0')[1],
                    'L' => GetVehicleNthTrailer().Replace('\0', '0')[2],
                    'S' => GetVehicleNthDepot().Replace('\0', '0')[0],
                    'T' => GetVehicleNthDepot().Replace('\0', '0')[1],
                    'U' => GetVehicleNthDepot().Replace('\0', '0')[2],
                    'V' => GetVehicleInstanceString().Replace('\0', '0')[0],
                    'W' => GetVehicleInstanceString().Replace('\0', '0')[1],
                    'X' => GetVehicleInstanceString().Replace('\0', '0')[2],
                    'Y' => GetVehicleInstanceString().Replace('\0', '0')[3],
                    'Z' => GetVehicleInstanceString().Replace('\0', '0')[4],
                    'j' => GetVehicleNthTrailer()[0],
                    'k' => GetVehicleNthTrailer()[1],
                    'l' => GetVehicleNthTrailer()[2],
                    's' => GetVehicleNthDepot()[0],
                    't' => GetVehicleNthDepot()[1],
                    'u' => GetVehicleNthDepot()[2],
                    'v' => GetVehicleInstanceString()[0],
                    'w' => GetVehicleInstanceString()[1],
                    'x' => GetVehicleInstanceString()[2],
                    'y' => GetVehicleInstanceString()[3],
                    'z' => GetVehicleInstanceString()[4],
                    _ => item,
                };
                ;
            }

            bool escapeNext = false;
            foreach (char item in identifierFormat)
            {
                if (escapeNext)
                {
                    result += item;
                    escapeNext = false;
                }
                else if (item == '\\')
                {
                    escapeNext = true;
                }
                else
                {
                    result += GetLetter(item);
                }
            }
            return result.Replace("\0", "").Trim();
        }
        [Obsolete("Deprecated in TLM14, use the alternative signature with destination list.", true)]
        public static void CalculateAutoName(ushort lineId, out ushort startStation, out ushort endStation, out string startStationStr, out string endStationStr)
        {
            TLMLineUtils.CalculateAutoName(lineId, false, out List<DestinationPoco> destinations);
            if (destinations.Count > 0)
            {
                startStation = destinations.First().stopId;
                endStation = destinations.Last().stopId;
                startStationStr = destinations.First().stopName;
                endStationStr = destinations.Last().stopName;
            }
            else
            {
                startStation = endStation = 0;
                startStationStr = endStationStr = null;
            }

        }
        [Obsolete("Deprecated in TLM14, use the alternative signature with regional parameter.", true)]
        public static void CalculateAutoName(ushort lineId, out List<DestinationPoco> destinations)
            => TLMLineUtils.CalculateAutoName(lineId, false, out destinations);
        public static void CalculateAutoName(ushort lineId, bool regional, out List<DestinationPoco> destinations)
            => TLMLineUtils.CalculateAutoName(lineId, regional, out destinations);

        [Obsolete("Use version with boolean indicator for regional lines", true)]
        public static string GetLineStringId(ushort lineId) => TLMLineUtils.GetLineStringId(lineId, false);
        public static string GetLineStringId(ushort lineId, bool regionalLine) => TLMLineUtils.GetLineStringId(lineId, regionalLine);
        public static bool GetRegionalLineParameters(ushort buildingStopId, out string Identifier, out string formatBg, out Color color)
        {
            if (TransportLinesManagerMod.Controller.BuildingLines[buildingStopId] is InnerBuildingLine ibl && ibl.LineDataObject is OutsideConnectionLineInfo ocli)
            {
                Identifier = ocli.Identifier;
                formatBg = ocli.LineBgSprite.ToString();
                color = ocli.LineColor;
                return true;
            }
            else
            {
                Identifier = null;
                formatBg = null;
                color = default;
                return false;
            }
        }

        public class DestinationPoco
        {
            public ushort stopId;
            public string stopName;
        }
    }
}