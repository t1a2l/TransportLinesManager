namespace TransportLinesManager.Data.Tsd
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition POST = new(
                    ItemClass.SubService.PublicTransportPost,
            VehicleInfo.VehicleType.None,
            TransportInfo.TransportType.Post,
            ItemClass.Level.Level2,
            [],
            default,
            1,
            default,
            false);  
    }

}
