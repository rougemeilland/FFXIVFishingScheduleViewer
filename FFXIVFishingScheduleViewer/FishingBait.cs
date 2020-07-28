using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer
{
    class FishingBait
        : IGameDataObject
    {
        private static int _serialNumber = 0;

        public FishingBait(string fishingBaitId)
        {
            Order = _serialNumber++;
            Id = new GameDataObjectId(GameDataObjectCategory.FishingBait, fishingBaitId);
            NameId = new TranslationTextId(TranslationCategory.FishingBait, fishingBaitId);
        }

        public int Order { get; }
        public GameDataObjectId Id { get; }
        public string Name => Translate.Instance[NameId];
        public TranslationTextId NameId { get; }

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

