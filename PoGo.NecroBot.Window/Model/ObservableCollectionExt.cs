using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace System.Collections.Generic
{
    [Serializable]
    public class ObservableCollectionExt<T> : ObservableCollection<T>
    {
        public void RemoveAll(Predicate<T> predicate)
        {
            CheckReentrancy();

            List<T> itemsToRemove = Items.Where(x => predicate(x)).ToList();
            itemsToRemove.ForEach(item => Items.Remove(item));

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
