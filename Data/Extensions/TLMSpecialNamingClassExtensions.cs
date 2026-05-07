using ColossalFramework.Globalization;
using TransportLinesManager.Data.Base.ConfigurationContainers;
using TransportLinesManager.Data.Base.Enums;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Utils;

namespace TransportLinesManager.Data.Extensions
{
    public static class TLMSpecialNamingClassExtensions
    {
        public static TLMAutoNameConfigurationData<TLMSpecialNamingClass> GetConfig(this TLMSpecialNamingClass clazz) => TLMBaseConfigXML.Instance.GetAutoNameData(clazz);

        public static NamingType ToNamingType(this TLMSpecialNamingClass clazz)
        {
            return clazz switch
            {
                TLMSpecialNamingClass.Campus => NamingType.CAMPUS,
                TLMSpecialNamingClass.Industrial => NamingType.INDUSTRY_AREA,
                TLMSpecialNamingClass.ParkArea => NamingType.PARKAREA,
                TLMSpecialNamingClass.District => NamingType.DISTRICT,
                TLMSpecialNamingClass.Address => NamingType.ADDRESS,
                _ => 0,
            };
        }

        public static string GetLocalizedName(this TLMSpecialNamingClass clazz) => Locale.Get("TLM_SPECIALNAMINGCLASS", clazz.ToString());
    }
}
