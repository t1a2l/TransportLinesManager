using TransportLinesManager.Data.Base.ConfigurationContainers;
using TransportLinesManager.Data.Base.Enums;
using TransportLinesManager.Data.DataContainers;
using static ItemClass;

namespace TransportLinesManager.Data.Extensions
{
    internal static class GameServiceExtensions
    {
        public static TLMSpecialNamingClass GetNamingClass(this DistrictPark park) =>
              park.IsCampus ? TLMSpecialNamingClass.Campus
            : park.IsIndustry ? TLMSpecialNamingClass.Industrial
            : TLMSpecialNamingClass.ParkArea;
        public static TLMAutoNameConfigurationData<Service> GetConfig(this Service service) => TLMBaseConfigXML.Instance.GetAutoNameData(service);

    }
}
