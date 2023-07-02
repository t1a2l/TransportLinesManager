using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition SHIP = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportShip,
            VehicleInfo.VehicleType.Ship,
            TransportInfo.TransportType.Ship,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerShip },
            new Color32(0xa3, 0xb0, 0, 255),
            100,
            LineIconSpriteNames.K45_DiamondIcon,
            true,
            ItemClass.Level.Level1);
    }

}
