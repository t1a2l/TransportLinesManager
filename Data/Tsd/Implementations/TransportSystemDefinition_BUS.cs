using Commons.UI.SpriteNames;
using UnityEngine;

namespace TransportLinesManager.Data.Tsd
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition BUS = new TransportSystemDefinition(
            ItemClass.SubService.PublicTransportBus,
            VehicleInfo.VehicleType.Car,
            TransportInfo.TransportType.Bus,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Bus },
            new Color32(53, 121, 188, 255),
            30,
            LineIconSpriteNames.HexagonIcon,
            true,
            ItemClass.Level.Level3,
            ItemClass.Level.Level2
        );
    }

}
