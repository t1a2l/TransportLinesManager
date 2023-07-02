using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition PLANE = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportPlane,
            VehicleInfo.VehicleType.Plane,
            TransportInfo.TransportType.Airplane,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerPlane },
            new Color32(0xa8, 0x01, 0x7a, 255),
            200,
            LineIconSpriteNames.K45_PentagonIcon,
            true,
            ItemClass.Level.Level1);
    }

}
