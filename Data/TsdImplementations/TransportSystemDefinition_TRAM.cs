using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition TRAM = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTram,
            VehicleInfo.VehicleType.Tram,
            TransportInfo.TransportType.Tram,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Tram },
            new Color32(73, 27, 137, 255),
            90,
            LineIconSpriteNames.K45_TrapezeIcon,
            true);
    }

}
