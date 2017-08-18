using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PoGo.NecroBot.Window
{
    public class ObservablePairCollection<TKey, TValue> : ObservableCollection<Pair<TKey, TValue>>
    {
        public Dictionary<TKey, TValue> GetDictionary()
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();

            foreach (var item in this)
            {
                result.Add(item.Key, item.Value);
            }
            return result;
        }
        private IDictionary<TKey, TValue> innerDictionary = new Dictionary<TKey, TValue>();

        public ObservablePairCollection()
            : base()
        {
        }

        public ObservablePairCollection(IEnumerable<Pair<TKey, TValue>> enumerable)
            : base(enumerable)
        {
        }

        public ObservablePairCollection(List<Pair<TKey, TValue>> list)
            : base(list)
        {
        }

        public ObservablePairCollection(IDictionary<TKey, TValue> dictionary)
        {
            foreach (var kv in dictionary)
            {
                Add(new Pair<TKey, TValue>(kv));
            }
        }

        public void Add(TKey key, TValue value)
        {
            Add(new Pair<TKey, TValue>(key, value));
        }
    }
}
