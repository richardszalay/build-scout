using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface ISettingsFacade : IDictionary<string, object>
    {
        void Save();
        void Reload();
    }

    public class IsolatedStorageSettingsFacade : ISettingsFacade
    {
        private readonly IsolatedStorageSettings settings;

        public IsolatedStorageSettingsFacade(IsolatedStorageSettings settings)
        {
            this.settings = settings;
        }

        #region ISettingsFacade Members

        void ISettingsFacade.Save()
        {
            settings.Save();
        }

        void ISettingsFacade.Reload()
        {
        }

        #endregion

        #region IDictionary<string,object> Members

        void IDictionary<string, object>.Add(string key, object value)
        {
            settings.Add(key, value);
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return ((IDictionary<string, object>) settings).ContainsKey(key);
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return ((IDictionary<string, object>) settings).Keys; }
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            return ((IDictionary<string, object>) settings).Remove(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            return ((IDictionary<string, object>) settings).TryGetValue(key, out value);
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { return ((IDictionary<string, object>)settings).Values; }
        }

        object IDictionary<string, object>.this[string key]
        {
            get { return ((IDictionary<string, object>) settings)[key]; }
            set { ((IDictionary<string, object>) settings)[key] = value; }
        }

        #endregion

        #region ICollection<KeyValuePair<string,object>> Members

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            ((ICollection<KeyValuePair<string, object>>) settings).Add(item);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            ((ICollection<KeyValuePair<string, object>>) settings).Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>) settings).Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object>>) settings).CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get { return ((ICollection<KeyValuePair<string, object>>) settings).Count; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<string, object>>) settings).IsReadOnly; }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>) settings).Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,object>> Members

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<string, object>>) settings).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) settings).GetEnumerator();
        }

        #endregion
    }
}
