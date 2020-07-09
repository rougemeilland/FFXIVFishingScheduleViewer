using System.Collections;
using System.Collections.Generic;

namespace FishingScheduler
{
    class KeyValueCollection<KEY_T, VALUE_T>
        : IKeyValueCollection<KEY_T, VALUE_T>
    {
        private ICollection<VALUE_T> _list;
        private IDictionary<KEY_T, VALUE_T> _dic;

        public KeyValueCollection()
        {
            _list = new List<VALUE_T>();
            _dic = new Dictionary<KEY_T, VALUE_T>();
        }

        public void Add(KEY_T key, VALUE_T value)
        {
            _dic.Add(key, value);
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

        public VALUE_T this[KEY_T key]
        {
            get
            {
                return _dic[key];
            }
        }
    }
}
