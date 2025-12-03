using ColossalFramework;
using Commons.Utils;
using TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TransportLinesManager.MapDrawer
{
    public struct TLMMapTransportLine
    {
        public ushort lineId;
        private readonly List<TLMMapStation> stations;
        public TransportInfo.TransportType TransportType { get; private set; }
        private readonly ItemClass.SubService subservice;
        private readonly VehicleInfo.VehicleType vehicleType;
        public string lineName;
        public string lineStringIdentifier;
        public Color32 lineColor;
        public bool activeDay;
        public bool activeNight;
        public ushort lineNumber;

        public TLMMapTransportLine(Color32 color, bool day, bool night, ushort lineId)
        {
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            lineName = Singleton<TransportManager>.instance.GetLineName(lineId);
            lineStringIdentifier = TLMLineUtils.GetLineStringId(lineId, false);
            TransportType = t.Info.m_transportType;
            subservice = t.Info.GetSubService();
            vehicleType = t.Info.m_vehicleType;
            stations = [];
            lineColor = color;
            activeDay = day;
            activeNight = night;
            this.lineId = lineId;
            lineNumber = t.m_lineNumber;
        }

        public readonly void AddStation(ref TLMMapStation s)
        {
            stations.Add(s);
            s.AddLine(lineId);
        }

        public readonly TLMMapStation this[int i] => stations[i];

        public readonly int StationsCount() => stations.Count;

        public string ToJson()
        {
            bool simmetry = GeneralUtils.FindSimetry([.. stations.Select(x => (int)x.stopId)], out int middle);
            return $"{{\"lineId\": {lineId},\"stations\": [{string.Join(",", [.. stations.Select(x => x.ToJson())])}],\"transportType\": \"{TransportType}\"," +
                $"\"subservice\": \"{subservice}\",\"vehicleType\": \"{vehicleType}\",\"lineName\": \"{lineName}\",\"lineStringIdentifier\": \"{lineStringIdentifier}\"," +
                $"\"lineColor\": \"#{(lineColor.r.ToString("X2") + lineColor.g.ToString("X2") + lineColor.b.ToString("X2"))}\",\"activeDay\": {activeDay.ToString().ToLower()}," +
                $" \"activeNight\": {activeNight.ToString().ToLower()}, \"lineNumber\": {lineNumber}, \"simmetryRange\": " + (simmetry ? ("[" + middle + "," + (middle + (stations.Count / 2) + 2) + "]") : "null") + "}";
        }
    }

}
