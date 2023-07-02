using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition TROLLEY = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTrolleybus,
            VehicleInfo.VehicleType.Trolleybus,
            TransportInfo.TransportType.Trolleybus,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Trolleybus },
            new Color(1, .517f, 0, 1),
            30,
            LineIconSpriteNames.K45_OvalIcon,
            true);
    }

}
