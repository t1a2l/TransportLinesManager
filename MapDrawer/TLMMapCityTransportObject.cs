using System.Collections.Generic;
using System.Linq;
/**
   OBS: Change the way of painting the lines, create common paths between equal stations (helps on the tram)
   see how to make one-way lines on the tram
*/
namespace TransportLinesManager.MapDrawer
{
    public class TLMMapCityTransportObject
    {
        public Dictionary<ushort, TLMMapTransportLine> transportLines;
        public List<TLMMapStation> stations;
        public string ToJson() => $@"{{
            ""transportLines"":{{{string.Join(",\n", transportLines.Select(x => $"\"{x.Key}\":{x.Value.ToJson()}").ToArray())}}},
            ""stations"":[{string.Join(",\n", stations.Select((x, i) => x.ToJson()).ToArray())}]
        }}";
    }

}

