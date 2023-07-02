using Klyte.Commons.UI.SpriteNames;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.Tsd
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition TOUR_BUS = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTours,
            VehicleInfo.VehicleType.Car,
            TransportInfo.TransportType.TouristBus,
            ItemClass.Level.Level3,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.TouristBus },
            new Color32(110, 152, 251, 255),
            30,
            LineIconSpriteNames.K45_CameraIcon,
            true);
    }

}
