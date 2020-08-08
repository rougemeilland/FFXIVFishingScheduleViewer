using FFXIVFishingScheduleViewer.Strings;
using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer.Models
{
    class FishingBait
        : IGameDataObject
    {
        private static int _serialNumber = 0;
        private string _rawId;

        public FishingBait(string fishingBaitId)
        {
            Order = _serialNumber++;
            _rawId = fishingBaitId;
            Id = new GameDataObjectId(GameDataObjectCategory.FishingBait, fishingBaitId);
            NameId = new TranslationTextId(TranslationCategory.FishingBait, fishingBaitId);
        }

        string IGameDataObject.InternalId => _rawId;
        public int Order { get; }
        public GameDataObjectId Id { get; }
        public TranslationTextId NameId { get; }
        public string Name => Translate.Instance[NameId];

        public IEnumerable<string> CheckTranslation()
        {
            return Translate.Instance.CheckTranslation(NameId);
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return Id.Equals(((FishingBait)o).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

