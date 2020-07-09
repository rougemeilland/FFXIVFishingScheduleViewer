namespace FishingScheduler
{
    class FishingGround
    {
        private static int _serialNumber = 0;

        public FishingGround(Area area, string fishingGroundName)
        {
            Order = _serialNumber++;
            Area = area;
            FishingGroundName = fishingGroundName;
        }

        public int Order { get; }

        public Area Area { get; }

        public string FishingGroundName { get; }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return FishingGroundName.Equals(((FishingGround)o).FishingGroundName);
        }

        public override int GetHashCode()
        {
            return FishingGroundName.GetHashCode();
        }
    }
}
