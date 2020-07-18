﻿namespace FishingScheduler
{
    class TranslationTextId
    {
        private TranslationCategory _category;
        private string _id;

        public TranslationTextId(TranslationCategory category, string id)
        {
            _category = category;
            _id = id;
        }

        public static bool operator == (TranslationTextId x, TranslationTextId y)
        {
            if (x == null)
                return y == null;
            return x.Equals(y);
        }

        public static bool operator !=(TranslationTextId x, TranslationTextId y)
        {
            if (x == null)
                return y != null;
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
            return string.Format("{0}.**{1}**", _category.ToInternalKeyText(), _id);
        }
    }
}
