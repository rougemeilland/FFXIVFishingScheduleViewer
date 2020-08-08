namespace FFXIVFishingScheduleViewer.Strings
{
    class TranslationTextId
    {
        private TranslationCategory _category;
        private string _id;
        private string _text;

        public TranslationTextId(TranslationCategory category, string id)
        {
            _category = category;
            _id = id;
            _text = string.Format("{0}.**{1}**", category.ToInternalKeyText(), id);
        }

        public static bool operator == (TranslationTextId x, TranslationTextId y)
        {
            if ((object)x == null)
                return (object)y == null;
            return x.Equals(y);
        }

        public static bool operator !=(TranslationTextId x, TranslationTextId y)
        {
            if ((object)x == null)
                return (object)y != null;
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var p = (TranslationTextId)obj;
            if (!_category.Equals(p._category))
                return false;
            if (!_id.Equals(p._id))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return _category.GetHashCode() ^ _id.GetHashCode();
        }

        public override string ToString()
        {
            return _text;
        }
    }
}
