namespace FishingScheduler
{
    class FishingBait
    {
        public FishingBait(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return Name.Equals(((FishingBait)o).Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}

