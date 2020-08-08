using FFXIVFishingScheduleViewer.Strings;
using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer.Models
{
    class FishingSpot
        : IGameDataObject
    {
        private static int _serialNumber = 0;
        private string _rawId;

        public FishingSpot(Area area, string fishingSpotId)
        {
            Order = _serialNumber++;
            _rawId = fishingSpotId;
            NameId = new TranslationTextId(TranslationCategory.FishingSpot, fishingSpotId);
            Id = new GameDataObjectId(GameDataObjectCategory.FishingSpot, fishingSpotId);
            Area = area;
        }

        string IGameDataObject.InternalId => _rawId;
        public int Order { get; }
        public GameDataObjectId Id { get; }
        public TranslationTextId NameId { get; }
        public string Name => Translate.Instance[NameId];
        public Area Area { get; }

        public IEnumerable<string> CheckTranslation()
        {
            return Translate.Instance.CheckTranslation(NameId);
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
