using TransportLinesManager.Data.Tsd;
using System.Collections.Generic;

namespace TransportLinesManager.Utils
{
    internal class TLMDepotUtils
    {
        public static List<ushort> GetAllDepotsFromCity(TransportSystemDefinition tsd)
        {
            var saida = new List<ushort>();
            BuildingManager bm = BuildingManager.instance;
            FastList<ushort> buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            foreach (ushort i in buildings)
            {
                var building = bm.m_buildings.m_buffer[i];
                var buildingAI = building.Info.m_buildingAI;
                if ((building.m_flags & Building.Flags.Untouchable) == 0 && buildingAI is DepotAI depotAI && depotAI.m_maxVehicleCount > 0 && tsd.IsFromSystem(depotAI))
                {
                    saida.Add(i);
                }
            }
            return saida;
        }

    }

}

