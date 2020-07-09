using System.Collections.Generic;

namespace FishingScheduler
{
    class AreaGroup
    {
        private static int _serialNumber = 0;
        private KeyValueCollection<string, Area> _areas;

        public AreaGroup(string name)
        {
            Order = _serialNumber++;
            AreaGroupName = name;
            _areas = new KeyValueCollection<string, Area>();
        }

        public int Order { get; }

        public string AreaGroupName { get; }

        public IKeyValueCollection<string, Area> Areas => _areas;

        public void AddArea(Area area)
        {
            _areas.Add(area.AreaName, area);
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return AreaGroupName.Equals(((AreaGroup)o).AreaGroupName);
        }

        public override int GetHashCode()
        {
            return AreaGroupName.GetHashCode();
        }

    }
}
