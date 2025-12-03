using Commons.Utils;
using TransportLinesManager.Data.Tsd;

namespace TransportLinesManager.Utils
{
    internal static class NamingTypeExtensions
    {
        public static int GetNamePrecedenceRate(this NamingType namingType)
        {
            return namingType switch
            {
                NamingType.NONE => 0x7FFFFFFF,
                NamingType.PLANE => -0x00000005,
                NamingType.BLIMP => 0x00000001,
                NamingType.SHIP => -0x00000002,
                NamingType.FERRY => 0x00000001,
                NamingType.TRAIN => 0x00000003,
                NamingType.MONORAIL => 0x00000004,
                NamingType.TRAM => 0x00000006,
                NamingType.METRO => 0x00000005,
                NamingType.BUS => 0x00000007,
                NamingType.TOUR_BUS => 0x00000009,
                NamingType.MONUMENT => 0x00000005,
                NamingType.CAMPUS => 0x00000010,
                NamingType.BEAUTIFICATION => 0x0000000a,
                NamingType.HEALTHCARE => 0x0000000b,
                NamingType.POLICEDEPARTMENT => 0x0000000b,
                NamingType.FIREDEPARTMENT => 0x0000000b,
                NamingType.EDUCATION => 0x0000000c,
                NamingType.DISASTER => 0x0000000d,
                NamingType.GARBAGE => 0x0000000f,
                NamingType.PARKAREA => 0x00000010,
                NamingType.DISTRICT => 0x00000010,
                NamingType.INDUSTRY_AREA => 0x00000010,
                NamingType.ADDRESS => 0x00000011,
                NamingType.RICO => 0x000000e,
                NamingType.CABLE_CAR => 0x00000004,
                NamingType.TROLLEY => 0x00000006,
                NamingType.HELICOPTER => 0x00000001,
                NamingType.TERMINAL => -0x00000020,
                _ => 0x7FFFFFFF,
            };
        }

        public static NamingType From(TransportSystemDefinition tsd) => tsd == TransportSystemDefinition.PLANE ? NamingType.PLANE
                    : tsd == TransportSystemDefinition.SHIP ? NamingType.SHIP
                    : tsd == TransportSystemDefinition.BLIMP ? NamingType.BLIMP
                    : tsd == TransportSystemDefinition.HELICOPTER ? NamingType.HELICOPTER
                    : tsd == TransportSystemDefinition.TRAIN ? NamingType.TRAIN
                    : tsd == TransportSystemDefinition.FERRY ? NamingType.FERRY
                    : tsd == TransportSystemDefinition.MONORAIL ? NamingType.MONORAIL
                    : tsd == TransportSystemDefinition.METRO ? NamingType.METRO
                    : tsd == TransportSystemDefinition.CABLE_CAR ? NamingType.CABLE_CAR
                    : tsd == TransportSystemDefinition.TROLLEY ? NamingType.TROLLEY
                    : tsd == TransportSystemDefinition.TRAM ? NamingType.TRAM
                    : tsd == TransportSystemDefinition.BUS ? NamingType.BUS
                    : tsd == TransportSystemDefinition.TOUR_BUS ? NamingType.TOUR_BUS
                    : NamingType.NONE;

        public static NamingType From(ItemClass.Service service)
        {
            switch (service)
            {
                case ItemClass.Service.Monument: return NamingType.MONUMENT;
                case ItemClass.Service.Natural:
                case ItemClass.Service.Fishing:
                case ItemClass.Service.Beautification: return NamingType.BEAUTIFICATION;
                case ItemClass.Service.HealthCare: return NamingType.HEALTHCARE;
                case ItemClass.Service.PoliceDepartment: return NamingType.POLICEDEPARTMENT;
                case ItemClass.Service.FireDepartment: return NamingType.FIREDEPARTMENT;
                case ItemClass.Service.Education: return NamingType.EDUCATION;
                case ItemClass.Service.Disaster: return NamingType.DISASTER;
                case ItemClass.Service.Water:
                case ItemClass.Service.Electricity:
                case ItemClass.Service.Garbage: return NamingType.GARBAGE;
                case ItemClass.Service.VarsitySports:
                case ItemClass.Service.PlayerEducation:
                case ItemClass.Service.Museums: return NamingType.CAMPUS;
                case ItemClass.Service.PlayerIndustry: return NamingType.INDUSTRY_AREA;
                case ItemClass.Service.Office:
                case ItemClass.Service.Residential:
                case ItemClass.Service.Industrial:
                case ItemClass.Service.Commercial: return NamingType.RICO;
                default:
                    LogUtils.DoErrorLog($"UNREGISTRED NAMING TYPE:{service}");
                    return NamingType.NONE;

            }
        }
    }

}
