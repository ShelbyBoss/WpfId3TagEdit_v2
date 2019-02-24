using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WpfId3TagEdit
{
    public class SortedObservableCollection<T> : ObservableCollection<T>
    {
        private IComparer<T> comparer;

        public IComparer<T> Comparer
        {
            get { return comparer; }
            set
            {
                if (value == comparer) return;

                comparer = value;
                UpdateCollection();
            }
        }

        public SortedObservableCollection() : base()
        {
        }

        public SortedObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public SortedObservableCollection(IComparer<T> comparer) : base()
        {
            Comparer = comparer;
        }

        public SortedObservableCollection(IEnumerable<T> collection, IComparer<T> compareTo) : base(collection)
        {
            Comparer = comparer;
        }

        public void UpdateCollection()
        {
            if (Comparer == null) return;

            int i = 0;
            foreach (T item in this.OrderBy(it => it, Comparer))
            {
                int currentIndex = IndexOf(item);

                if (currentIndex != i) base.MoveItem(currentIndex, i);

                i++;
            }
        }

        protected override void InsertItem(int index, T item)
        {
            if (Comparer != null)
            {
                index = 0;
                foreach (T i in this)
                {
                    if (Comparer.Compare(i, item) == -1) index++;
                    else break;
                }
            }

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);

            UpdateCollection();
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);

            UpdateCollection();
        }
    }
}
