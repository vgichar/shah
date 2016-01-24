using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Helper class, keeps the string descriptions of a move.
    /// </summary>
    public class MoveStringDescription
    {
        /// <summary>
        /// The index representation of the move.
        /// </summary>
        private string index;

        /// <summary>
        /// The index representation of the move.
        /// </summary>
        public string Index
        {
            get { return index; }
        }

        /// <summary>
        /// The move SAN.
        /// </summary>
        private string san;

        /// <summary>
        /// The move SAN.
        /// </summary>
        public string SAN
        {
            get { return san; }
        }

        /// <summary>
        /// The index and the SAN of the move.
        /// </summary>
        private string indexedSAN;

        /// <summary>
        /// True if this is a white move, false otherwise.
        /// </summary>
        public bool IsWhiteMove
        {
            get { return !Index.EndsWith("..."); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="SAN"></param>
        internal MoveStringDescription(string index, string SAN)
        {
            this.index = index;
            this.san = SAN;
            indexedSAN = index + ' ' + SAN;
        }

        /// <summary>
        /// The index and the SAN of the move.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return indexedSAN;
        }

    }

    /// <summary>
    /// Helper class, implements the move history string descriptions as a binding list.
    /// It can be modified only from this assembly, it is read-only outside of the assembly
    /// </summary>
    public class MoveHistoryStringDescriptionBindingList : IBindingList, INotifyPropertyChanged
    {
        /// <summary>
        /// The curent board index.
        /// </summary>
        private int boardIndex;

        /// <summary>
        /// The internal list.
        /// </summary>
        private List<MoveStringDescription> internalList = new List<MoveStringDescription>(ValilGame.MeanHistoryLength);

        /// <summary>
        /// True to raise ListChanged events, false otherwise.
        /// </summary>
        private bool raiseListChangeEvents;

        /// <summary>
        /// Lock object.
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// ListChanged event.
        /// </summary>
        public event ListChangedEventHandler ListChanged;

        /// <summary>
        /// PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The current board index.
        /// </summary>
        public int BoardIndex
        {
            get { return boardIndex; }
            internal set
            {
                if (boardIndex != value)
                {
                    boardIndex = value;
                    OnPropertyChanged("BoardIndex");
                }
            }
        }

        /// <summary>
        /// True to raise ListChanged events, false otherwise.
        /// </summary>
        public bool RaiseListChangeEvents
        {
            get { return raiseListChangeEvents; }
            internal set { raiseListChangeEvents = value; }
        }

        /// <summary>
        /// Raises ListChanged event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnListChanged(object sender, ListChangedEventArgs e)
        {
            if (ListChanged != null) { ListChanged(sender, e); }
        }

        /// <summary>
        /// Raises PropertyChanged event.
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }

        #region IBindingList Members

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        object IBindingList.AddNew()
        {
            throw new NotSupportedException();
        }

        bool IBindingList.AllowEdit
        {
            get { return false; }
        }

        bool IBindingList.AllowNew
        {
            get { return false; }
        }

        bool IBindingList.AllowRemove
        {
            get { return false; }
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotSupportedException();
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }

        bool IBindingList.IsSorted
        {
            get { return false; }
        }

        event ListChangedEventHandler IBindingList.ListChanged
        {
            add { ListChanged += value; }
            remove { ListChanged -= value; }
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        void IBindingList.RemoveSort()
        {
            throw new NotSupportedException();
        }

        ListSortDirection IBindingList.SortDirection
        {
            get { throw new NotSupportedException(); }
        }

        PropertyDescriptor IBindingList.SortProperty
        {
            get { throw new NotSupportedException(); }
        }

        bool IBindingList.SupportsChangeNotification
        {
            get { return true; }
        }

        bool IBindingList.SupportsSearching
        {
            get { return false; }
        }

        bool IBindingList.SupportsSorting
        {
            get { return false; }
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object value)
        {
            if (!(value is MoveStringDescription)) { throw new ArgumentException(); }

            return internalList.Contains(value as MoveStringDescription);
        }

        int IList.IndexOf(object value)
        {
            if (!(value is MoveStringDescription)) { throw new ArgumentException(); }

            return internalList.IndexOf(value as MoveStringDescription);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Read-only indexer.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object IList.this[int index]
        {
            get { return internalList[index]; }
            set { throw new NotSupportedException(); }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            if (array is MoveStringDescription[])
            {
                internalList.CopyTo(array as MoveStringDescription[], index);
            }
            // for example, the array could be of type object[]
            // and even if object[] cannot be converted to MoveStringDescription[]
            // the collection elements can be copied into the array
            else
            {
                int i = index;
                foreach (MoveStringDescription moveDesc in internalList)
                {
                    array.SetValue(moveDesc, i++);
                }
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return lockObj; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Adds an item to the list.
        /// </summary>
        /// <param name="item"></param>
        internal void Add(MoveStringDescription item)
        {
            internalList.Add(item);
            if (raiseListChangeEvents)
            {
                OnListChanged(this, new ListChangedEventArgs(ListChangedType.ItemAdded, internalList.Count - 1));
            }
        }

        /// <summary>
        /// Removes the items between the ranges.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        internal void RemoveRange(int index, int count)
        {
            internalList.RemoveRange(index, count);
            if (raiseListChangeEvents)
            {
                OnListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }

        /// <summary>
        /// Clears the list.
        /// </summary>
        internal void Clear()
        {
            internalList.Clear();
            if (raiseListChangeEvents)
            {
                OnListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }

        /// <summary>
        /// Refreshes the list.
        /// </summary>
        internal void Refresh()
        {
            if (raiseListChangeEvents)
            {
                OnListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }

        /// <summary>
        /// Gets the numbers of items in this list.
        /// </summary>
        public int Count
        {
            get { return internalList.Count; }
        }

    }
}
