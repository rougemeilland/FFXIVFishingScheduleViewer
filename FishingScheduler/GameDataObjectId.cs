using System;

namespace FishingScheduler
{
    class GameDataObjectId
        : IEquatable<GameDataObjectId>
    {
        private GameDataObjectCategory _category;
        private string _id;

        public GameDataObjectId(GameDataObjectCategory category, string id)
        {
            _category = category;
            _id = id;
        }

        public bool CheckCategory(GameDataObjectCategory category)
        {
            return category == _category;
        }

        public bool Equals(GameDataObjectId other)
        {
            if (other == null)
                return false;
            if (!_category.Equals(other._category))
                return false;
            if (!_id.Equals(other._id))
                return false;
            return true;
        }

        public static bool operator ==(GameDataObjectId x, GameDataObjectId y)
        {
            if ((object)x == null)
                return (object)y == null;
            return x.Equals(y);
        }

        public static bool operator !=(GameDataObjectId x, GameDataObjectId y)
        {
            if ((object)x == null)
                return (object)y != null;
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var p = (GameDataObjectId)obj;
            if (!_category.Equals(p._category))
                return false;
            if (!_id.Equals(p._id))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode() ^ _category.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}.**{1}**", _category.ToInternalKeyText(), _id);
        }
    }
}
