using Commons.Utils;

namespace TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector
{
    public class VehicleItem
    {
        // Vehicle prefab.
        private VehicleInfo _vehicleInfo;
        private string _vehicleName;

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleItem"/> class.
        /// </summary>
        /// <param name="prefab">Vehicle prefab for this item.</param>
        public VehicleItem(VehicleInfo prefab)
        {
            Info = prefab;
        }

        /// <summary>
        /// Gets the vehicles's name (empty string if none).
        /// </summary>
        public string Name => _vehicleName;

        /// <summary>
        /// Gets or sets the vehicle prefab for this record.
        /// </summary>
        public VehicleInfo Info
        {
            get => _vehicleInfo;

            set
            {
                _vehicleInfo = value;

                // Set display name.
                _vehicleName = PrefabUtils.GetDisplayName(value);
            }
        }
    }
}
