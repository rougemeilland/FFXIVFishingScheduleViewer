using System.Collections.Generic;

namespace FishingScheduler
{
    class FishingBait
        : IGameDataObject
    {
        private TranslationTextId _nameId;

        public FishingBait(string fishingBaitId)
        {
            Id = new GameDataObjectId(GameDataObjectCategory.FishingBait, fishingBaitId);
            _nameId = new TranslationTextId(TranslationCategory.FishingBait, fishingBaitId);
        }

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
            return Id.Equals(((FishingBait)o).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

