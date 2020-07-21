using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer
{
    class FishingSpot
        : IGameDataObject
    {
        private static int _serialNumber = 0;
        private TranslationTextId _nameId;

        public FishingSpot(Area area, string fishingSpotId)
        {
            Order = _serialNumber++;
            Area = area;
            _nameId = new TranslationTextId(TranslationCategory.FishingSpot, fishingSpotId);
            Id = new GameDataObjectId(GameDataObjectCategory.FishingSpot, fishingSpotId);
        }

        public int Order { get; }
        public Area Area { get; }
        public GameDataObjectId Id { get; }
        public string Name => Translate.Instance[_nameId];

        public IEnumerable<string> CheckTranslation()
        {
            return Translate.Instance.CheckTranslation(_nameId);
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return Id.Equals(((FishingSpot)o).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
