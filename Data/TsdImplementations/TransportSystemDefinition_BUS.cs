using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition BUS = new TransportSystemDefinition(
            ItemClass.SubService.PublicTransportBus,
            VehicleInfo.VehicleType.Car,
            TransportInfo.TransportType.Bus,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Bus },
            new Color32(53, 121, 188, 255),
            30,
            LineIconSpriteNames.K45_HexagonIcon,
            true,
            ItemClass.Level.Level3,
            ItemClass.Level.Level2
        );
    }

}
