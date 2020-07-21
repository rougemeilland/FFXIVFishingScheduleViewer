using System.Collections;
using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer
{
    class KeyValueCollection<VALUE_T>
        : IKeyValueCollection<VALUE_T>
        where VALUE_T: IGameDataObject
    {
        private ICollection<VALUE_T> _list;
        private IDictionary<GameDataObjectId, VALUE_T> _dic;

        public KeyValueCollection()
        {
            _list = new List<VALUE_T>();
            _dic = new Dictionary<GameDataObjectId, VALUE_T>();
        }

        public void Add(VALUE_T value)
        {
            _dic.Add(value.Id, value);
            _list.Add(value);
        }

        public IEnumerator<VALUE_T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }

        public VALUE_T this[GameDataObjectId key]
        {
            get
            {
                return _dic[key];
            }
        }
    }
}
