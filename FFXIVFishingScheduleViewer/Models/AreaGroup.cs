using FFXIVFishingScheduleViewer.Strings;
using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer.Models
{
    class AreaGroup
        : IGameDataObject
    {
        private static int _serialNumber = 0;
        private string _rawId;

        public AreaGroup(string areaGroupId)
        {
            Order = _serialNumber++;
            _rawId = areaGroupId;
            Id = new GameDataObjectId(GameDataObjectCategory.AreaGroup, areaGroupId);
            NameId = new TranslationTextId(TranslationCategory.AreaGroup, areaGroupId);
            Areas = new AreaCollection();
        }

        string IGameDataObject.InternalId => _rawId;
        public int Order { get; }
        public GameDataObjectId Id { get; }
        public TranslationTextId NameId { get; }
        public string Name => Translate.Instance[NameId];
        public AreaCollection Areas { get; }

        public IEnumerable<string> CheckTranslation()
        {
            return Translate.Instance.CheckTranslation(NameId);
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
