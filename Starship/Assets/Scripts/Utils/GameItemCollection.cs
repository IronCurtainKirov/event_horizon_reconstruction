using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public interface IReadOnlyGameItemCollection<T>
    {
        IEnumerable<KeyValuePair<T, ObscuredInt>> Items { get; }
        IEnumerable<T> Keys { get; }
        int GetQuantity(T item);
        int Count { get; }
    }

    public interface IGameItemCollection<T> : IReadOnlyGameItemCollection<T>
    {
        int this[T key] { get; set; }
        void Add(T item, int amount = 1);
        int Remove(T item, int amount = 1);
        void Assign(IEnumerable<KeyValuePair<T, int>> items);
        void Assign(IReadOnlyGameItemCollection<T> items);
        void Clear();
    }

    public class GameItemCollection<T> : IGameItemCollection<T>
    {
        public event Action CollectionChangedEvent;
        public event Action<T, int> AccurateCollectionChangedEvent;

        public void Assign(IEnumerable<KeyValuePair<T, int>> items)
        {
            _collection.Clear();
            foreach (var item in items)
                _collection.Add(item.Key, item.Value);

            IsDirty = true;
        }

        public void Assign(IReadOnlyGameItemCollection<T> items)
        {
            _collection.Clear();
            foreach (var item in items.Items)
                _collection.Add(item.Key, item.Value);

            IsDirty = true;
        }

        public void Assign(IEnumerable<KeyValuePair<T, ObscuredInt>> items)
        {
            _collection.Clear();
            foreach (var item in items)
                _collection.Add(item.Key, item.Value);

            IsDirty = true;
        }

        public int this[T key]
        {
            get { return GetQuantity(key); }
            set
            {
                _collection[key] = value;
                IsDirty = true;
                AccurateCollectionChangedEvent?.Invoke(key, value);
            }
        }

        public void Add(T item, int amount = 1)
        {
            ObscuredInt value;
            if (!_collection.TryGetValue(item, out value))
                value = 0;

            _collection[item] = value + amount;

            IsDirty = true;
            AccurateCollectionChangedEvent?.Invoke(item, value + amount);
        }

        public int Remove(T item, int amount = 1)
        {
            ObscuredInt value;
            if (!_collection.TryGetValue(item, out value))
                return 0;

            var quantity = value - amount;
            if (quantity <= 0)
                _collection.Remove(item);
            else
                _collection[item] = quantity;

            IsDirty = true;
            AccurateCollectionChangedEvent?.Invoke(item, quantity <= 0 ? 0 : quantity);

            return quantity >= 0 ? amount : amount + quantity;
        }

        public void Clear()
        {
            if (!_collection.Any())
                return;

            var keys = _collection.Keys;
            _collection.Clear();
            IsDirty = true;
            foreach (var key in keys)
                AccurateCollectionChangedEvent?.Invoke(key, 0);
        }

        public int GetQuantity(T item)
        {
            ObscuredInt value;
            return _collection.TryGetValue(item, out value) ? (int) value : 0;
        }

        public int Count
        {
            get { return _collection.Count; }
        }

        public IEnumerable<KeyValuePair<T, ObscuredInt>> Items
        {
            get { return _collection; }
        }

        public IEnumerable<T> Keys
        {
            get { return _collection.Keys; }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;

                if (_isDirty && CollectionChangedEvent != null)
                    CollectionChangedEvent.Invoke();
            }
        }

        private bool _isDirty;
        private readonly Dictionary<T, ObscuredInt> _collection = new Dictionary<T, ObscuredInt>();
    }
}
