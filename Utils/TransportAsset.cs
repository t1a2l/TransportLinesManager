using System.Collections.Generic;

namespace TransportLinesManager.Utils
{
	public struct TransportAsset
	{
		public string name;

		public List<int> spawn_percent;

		public Dictionary<int, Count> count;

		public int capacity;

	}

    public struct Count
	{
        public int totalCount;

        public int usedCount;
	}
}
