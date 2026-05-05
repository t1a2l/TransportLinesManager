using ColossalFramework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/**
   OBS: Change the way of painting the lines, create common paths between equal stations (helps on the tram)
   see how to make one-way lines on the tram
*/
namespace TransportLinesManager.MapDrawer
{
    public class TLMMapStation
    {

        public string name;
        public Dictionary<ushort, Vector3> stopsWithWorldPos = [];
        public ushort stopId;


        public List<ushort> LinesPassing { get; private set; } = [];
        public int Id { get; private set; }
        private readonly ItemClass.Service service;
        private readonly string districtName;
        private readonly ushort districtId;

        public string ToJson() => $@"{{""name"":""{name}"",""linesPassingCount"":{ LinesPassing.Count},
                ""stops"":{{{string.Join(",", [.. stopsWithWorldPos.Select(x => $@"""{x.Key}"":[{x.Value.x},{x.Value.y},{x.Value.z}]")])}}},
                ""stopId"":{stopId},""linesPassing"":[{string.Join(",", [.. LinesPassing.Select(x => x.ToString())])}],
                ""id"":{Id},""service"":""{service}"",""districtId"":{districtId},""districtName"":""{districtName}""}}";

        public TLMMapStation(string n, Vector3 worldPos, Dictionary<ushort, Vector3> stops, int stationId, ItemClass.Service service, ushort stopId)
        {
            name = n;

            stopsWithWorldPos = stops;
            Id = stationId;
            this.stopId = stopId;
            this.service = service;
            DistrictManager dm = Singleton<DistrictManager>.instance;
            districtId = dm.GetDistrict(worldPos);
            districtName = dm.GetDistrictName(districtId);
        }

        public void AddLine(ushort lineId)
        {
            if (!LinesPassing.Contains(lineId))
            {
                LinesPassing.Add(lineId);
            }
        }
        public List<ushort> GetAllStationOffsetPoints() => LinesPassing;
    }

}

