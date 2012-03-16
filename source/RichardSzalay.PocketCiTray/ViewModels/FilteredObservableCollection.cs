using System;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class FilteredObservableCollection<TList> : IList<TList>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private List<TList> filteredList = new List<TList>();
        private List<int> hiddenIndexes = new List<int>();

        private readonly IList<TList> source;
        private readonly Func<TList, string, bool> filterCallback;
        private readonly IScheduler userInterface;
        private string filter;
        private string previousFilter;
        private IList<TList> visibleItems;

        private readonly SerialDisposable eventDispatcherSubscription = new SerialDisposable();

        public FilteredObservableCollection(IList<TList> source, 
            Func<TList, string, bool> filterCallback, IScheduler userInterface)
        {
            this.source = source;
            this.filterCallback = filterCallback;
            this.userInterface = userInterface;

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
            return visibleItems.IndexOf((TList)value);
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

        public int Count
        {
            get { return visibleItems.Count; }
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
        public event PropertyChangedEventHandler PropertyChanged;

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
            var handler = CollectionChanged;

            var indexEnumerator = this.hiddenIndexes.GetEnumerator();
            bool moreHiddenIndexes = indexEnumerator.MoveNext();

            var newHiddenIndexes = new List<int>(source.Count);
            var changes = new List<NotifyCollectionChangedEventArgs>(source.Count);
            var newVisibleItems = new List<TList>(source.Count);

            int visibleIndex = 0;

            for (int i=0; i<source.Count; i++)
            {
                bool itemVisible = (filter == null || filterCallback(source[i], filter));

                while(moreHiddenIndexes && indexEnumerator.Current < i)
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

            if (handler != null)
            {
                eventDispatcherSubscription.Disposable = userInterface.Schedule(() =>
                {
                    foreach (var change in changes)
                    {
                        handler(this, change);
                    }

                    OnPropertyChanged("Count");
                    OnPropertyChanged("Item[]");
                });
            }
        }

        private void OnPropertyChanged(string property)
        {
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
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

        [Conditional("DEBUG")]
        public static void TraceCollectionChangedEvent(NotifyCollectionChangedEventArgs args)
        {
#if !DEBUG
            return;
#endif

            string symbol = (args.Action == NotifyCollectionChangedAction.Add) ? "+"
                : (args.Action == NotifyCollectionChangedAction.Remove) ? "-"
                : (args.Action == NotifyCollectionChangedAction.Replace) ? "="
                : "!";

            string items = "";
            string index = "-";

            if (args.Action == NotifyCollectionChangedAction.Add ||
                args.Action == NotifyCollectionChangedAction.Replace)
            {
                items = String.Join(", ", args.NewItems.Cast<Object>().Select(x => x.ToString()));
                index = args.NewStartingIndex.ToString();
            }

            if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                items = String.Join(", ", args.OldItems.Cast<Object>().Select(x => x.ToString()));
                index = args.OldStartingIndex.ToString();
            }

            Debug.WriteLine("{0}[{1}] {2}", symbol, index, items);
        }

        //private static IEnumerable<NotifyCollectionChangedEventArgs>
    }
}
