using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition HELICOPTER = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportPlane,
            VehicleInfo.VehicleType.Helicopter,
            TransportInfo.TransportType.Helicopter,
            ItemClass.Level.Level3,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerHelicopter },
            new Color(.671f, .333f, .604f, 1),
            10,
            LineIconSpriteNames.K45_S05StarIcon,
            true);
    }

}
