// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/AboveAverageInspector         --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace instance.id.AAI.Extensions
{
    // -------------------------------------------------------------------
    // -- SerializedDictionaryDrawer.cs ----------------------------------
    [Serializable]
    public class SerialDict<TKey, TValue> : SerializedDictionary<TKey, TValue> { }

    [Serializable]
    public class SerializedDictionary<KeyType, TValue> : Dictionary<KeyType, TValue>, ISerializationCallbackReceiver
    {
        public const string KeyProperty = nameof(_keys);
        public const string ValueProperty = nameof(_values);

        // These are protected so they can be found by the editor.
        [SerializeField] protected List<KeyType> _keys = new List<KeyType>();
        [SerializeField] protected List<TValue> _values = new List<TValue>();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            ConvertToLists();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            ConvertFromLists();
        }

        private void ConvertToLists()
        {
            _keys.Clear();
            _values.Clear();
            foreach (var entry in this)
            {
                _keys.Add(entry.Key);
                _values.Add(entry.Value);
            }
        }

        private void ConvertFromLists()
        {
            Clear();
            var count = Math.Min(_keys.Count, _values.Count);
            for (var i = 0; i < count; i++) Add(_keys[i], _values[i]);
        }
    }
}
