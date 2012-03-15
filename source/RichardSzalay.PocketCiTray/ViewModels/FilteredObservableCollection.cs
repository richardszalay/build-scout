using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class FilteredObservableCollection<TList> : IList<TList>, IList, INotifyCollectionChanged
    {
        private List<TList> filteredList = new List<TList>();
        private List<int> hiddenIndexes = new List<int>();

        private readonly IList<TList> source;
        private readonly Func<TList, string, bool> filterCallback;
        private string filter;
        private string previousFilter;
        private IList<TList> visibleItems;

        public FilteredObservableCollection(IList<TList> source, Func<TList, string, bool> filterCallback)
        {
            this.source = source;
            this.filterCallback = filterCallback;

            this.visibleItems = source;
        }

        public IEnumerator<TList> GetEnumerator()
        {
            return visibleItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TList item)
        {
            throw new NotSupportedException();
        }

        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        public bool Contains(object value)
        {
            throw new NotSupportedException();
        }

        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        public int IndexOf(object value)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        void ICollection<TList>.Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(TList item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(TList[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TList item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        int ICollection.Count
        {
            get { return source.Count - hiddenIndexes.Count; }
        }

        public object SyncRoot
        {
            get { return null; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        int ICollection<TList>.Count
        {
            get { return ((ICollection)this).Count; }
        }

        bool ICollection<TList>.IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(TList item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, TList item)
        {
            throw new NotSupportedException();
        }

        void IList<TList>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public TList this[int index]
        {
            get { return visibleItems[index]; }
            set { throw new NotSupportedException(); }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public string Filter
        {

            get { return filter; }
            set
            {
                filter = value;

                UpdateFilter();
            }
        }

        private void UpdateFilter()
        {
            if (String.IsNullOrEmpty(filter))
            {
                hiddenIndexes = new List<int>();
                visibleItems = source;
            }

            var indexEnumerator = this.hiddenIndexes.GetEnumerator();
            bool moreHiddenIndexes = indexEnumerator.MoveNext();

            var newHiddenIndexes = new List<int>(source.Count);
            var changes = new List<NotifyCollectionChangedEventArgs>(source.Count);
            var newVisibleItems = new List<TList>(source.Count);

            int visibleIndex = 0;

            for (int i=0; i<source.Count; i++)
            {
                bool itemVisible = (filter == null || filterCallback(source[i], filter));

                while(moreHiddenIndexes && i < indexEnumerator.Current)
                {
                    moreHiddenIndexes = indexEnumerator.MoveNext();
                }

                bool wasVisible = !(moreHiddenIndexes && i == indexEnumerator.Current);

                if (wasVisible != itemVisible)
                {
                    var change = itemVisible
                        ? CreateAdded(visibleIndex, source[i])
                        : CreateRemoved(visibleIndex, source[i]);

                    changes.Add(change);
                }

                if (!wasVisible && moreHiddenIndexes)
                {
                    moreHiddenIndexes = indexEnumerator.MoveNext();
                }

                if (itemVisible)
                {
                    newVisibleItems.Add(source[i]);
                    visibleIndex++;
                }
                else
                {
                    newHiddenIndexes.Add(i);
                }
            }

            newHiddenIndexes.Sort();

            visibleItems = newVisibleItems;
            hiddenIndexes = newHiddenIndexes;

            var handler = CollectionChanged;

            if (handler != null)
            {
                //handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                foreach(var change in changes)
                {
                    handler(this, change);
                }
            }
        }

        private static NotifyCollectionChangedEventArgs CreateAdded(int index, TList item)
        {
            return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
        }

        private static NotifyCollectionChangedEventArgs CreateRemoved(int index, TList item)
        {
            return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
        }

        //private static IEnumerable<NotifyCollectionChangedEventArgs>
    }
}
