using Commons.UI.SpriteNames;
using UnityEngine;

namespace TransportLinesManager.Data.Tsd
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition TRAM = new(
                    ItemClass.SubService.PublicTransportTram,
            VehicleInfo.VehicleType.Tram,
            TransportInfo.TransportType.Tram,
            ItemClass.Level.Level1,
            [TransferManager.TransferReason.Tram],
            new Color32(73, 27, 137, 255),
            90,
            LineIconSpriteNames.TrapezeIcon,
            true);
    }

}
