using System.Collections.Generic;

namespace FishingScheduler
{
    class AreaGroup
        : IGameDataObject
    {
        private static int _serialNumber = 0;
        private TranslationTextId _nameId;

        public AreaGroup(string areaGroupId)
        {
            Order = _serialNumber++;
            Id = new GameDataObjectId(GameDataObjectCategory.AreaGroup, areaGroupId);
            _nameId = new TranslationTextId(TranslationCategory.AreaGroup, areaGroupId);
            Areas = new AreaCollection();
        }

        public int Order { get; }
        public GameDataObjectId Id { get; }
        public string Name => Translate.Instance[_nameId];
        public AreaCollection Areas { get; }

        public IEnumerable<string> CheckTranslation()
        {
            return Translate.Instance.CheckTranslation(_nameId);
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return Id.Equals(((AreaGroup)o).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
