using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition FERRY = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportShip,
            VehicleInfo.VehicleType.Ferry,
            TransportInfo.TransportType.Ship,
            ItemClass.Level.Level2,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Ferry },
            new Color32(0xe3, 0xf0, 0, 255),
            50,
            LineIconSpriteNames.K45_S08StarIcon,
            true);
    }

}
