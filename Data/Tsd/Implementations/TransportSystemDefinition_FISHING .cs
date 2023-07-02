using UnityEngine;

namespace Klyte.TransportLinesManager.Data.Tsd
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition FISHING = new TransportSystemDefinition(
                    ItemClass.SubService.None,
            VehicleInfo.VehicleType.Ship,
            TransportInfo.TransportType.Fishing,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Fish },
            new Color(.671f, .333f, 0, 1),
            1,
            default,
            false);
    }

}
