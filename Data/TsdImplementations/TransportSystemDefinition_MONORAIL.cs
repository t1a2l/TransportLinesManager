using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition MONORAIL = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportMonorail,
            VehicleInfo.VehicleType.Monorail,
            TransportInfo.TransportType.Monorail,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Monorail },
            new Color32(217, 51, 89, 255),
            180,
            LineIconSpriteNames.K45_RoundedSquareIcon,
            true);
    }
}
