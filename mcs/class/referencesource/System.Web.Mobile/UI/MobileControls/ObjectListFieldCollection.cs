//------------------------------------------------------------------------------
// <copyright file="ObjectListFieldCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Object List Field Collection class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ObjectListFieldCollection : ArrayListCollectionBase, IObjectListFieldCollection, IStateManager
    {
        private ObjectList _owner;
        private bool _marked = false;
        private bool _fieldsSetDirty = false;
        // Used for primary field collection (the one modified by the user).

        internal ObjectListFieldCollection(ObjectList owner) : base(new ArrayList())
        {
            _owner = owner;
        }

        // Used for autogenerated field collections.

        internal ObjectListFieldCollection(ObjectList owner, ArrayList fields) : base(fields)
        {
            _owner = owner;
            foreach (ObjectListField field in fields)
            {
                field.SetOwner(owner);
            }
        }

        /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection.GetAll"]/*' />
        public ObjectListField[] GetAll()
        {
            int n = Count;
            ObjectListField[] allFields = new ObjectListField[n];
            if (n > 0) 
            {
                Items.CopyTo(0, allFields, 0, n);
            }
            return allFields;
        }

        /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection.SetAll"]/*' />
        public void SetAll(ObjectListField[] value)
        {
            Items = new ArrayList(value);
            foreach(ObjectListField field in Items)
            {
                field.SetOwner(_owner);
            }
            if(_marked)
            {
                SetFieldsDirty();
            }
        }

        /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection.this"]/*' />
        public ObjectListField this[int index] 
        {
            get 
            {
                return (ObjectListField)Items[index];
            }
        }

        /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection.Add"]/*' />
        public void Add(ObjectListField field)
        {
            AddAt(-1, field);
        }

        /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection.AddAt"]/*' />
        public void AddAt(int index, ObjectListField field)
        {
            if (index == -1)
            {
                Items.Add(field);
            }
            else
            {
                Items.Insert(index, field);
            }
            if (_marked)
            {
                if (!_fieldsSetDirty && index > -1)
                {
                    SetFieldsDirty();
                }
                else
                {
                    ((IStateManager)field).TrackViewState();
                    field.SetDirty();
                }
            }
            field.SetOwner(_owner);
            NotifyOwnerChange();
        }

        private void SetFieldsDirty()
        {
            foreach (ObjectListField fld in Items)
            {
                ((IStateManager)fld).TrackViewState();
                fld.SetDirty();
            }
            _fieldsSetDirty = true;
        }

        /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection.Clear"]/*' />
        public void Clear()
        {
            Items.Clear();
            if (_marked)
            {
                // Each field will be marked dirty as it is added.
                _fieldsSetDirty = true;
            }
            NotifyOwnerChange();
        }

        /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection.RemoveAt"]/*' />
        public void RemoveAt(int index) 
        {
            if ((index >= 0) && (index < Count)) 
            {
                Items.RemoveAt(index);
            }
            if(_marked && !_fieldsSetDirty)
            {
                SetFieldsDirty();
            }
            NotifyOwnerChange();
        }

        /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection.Remove"]/*' />
        public void Remove(ObjectListField field)
        {
            int index = IndexOf(field);
            if (index >= 0) 
            {
                RemoveAt(index);
            }
        }

        /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection.IndexOf"]/*' />
        public int IndexOf(ObjectListField field)
        {
            if (field != null) 
            {
                return Items.IndexOf(field, 0, Count);
            }
            return -1;
        }

        /// <include file='doc\ObjectListFieldCollection.uex' path='docs/doc[@for="ObjectListFieldCollection.IndexOf1"]/*' />
        public int IndexOf(String fieldIDOrName)
        {
            ArrayList fields = Items;
            int i = 0;
            foreach (ObjectListField field in fields)
            {
                String id = field.UniqueID;
                if (id != null && String.Compare(id, fieldIDOrName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        private void NotifyOwnerChange()
        {
            if (_owner != null)
            {
                _owner.OnFieldChanged(true);    // fieldAddedOrRemoved = true
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  STATE MANAGEMENT
        /////////////////////////////////////////////////////////////////////////

        /// <internalonly/>
        protected bool IsTrackingViewState
        {
            get
            {
                return _marked;
            }
        }

        /// <internalonly/>
        protected void TrackViewState()
        {
            _marked = true;
            foreach (IStateManager field in Items)
            {
                field.TrackViewState();
            }
        }

        /// <internalonly/>
        protected void LoadViewState(Object savedState)
        {
            if (savedState != null)
            {
                Object[] stateArray = (Object[]) savedState;
                bool allFieldsSaved = (bool) stateArray[0];
                if (allFieldsSaved) 
                {
                    // All fields are in view state.  Any fields loaded until now are invalid.
                    ClearFieldsViewState();
                }
                Object[] fieldStates = (Object[])stateArray[1];
                EnsureCount(fieldStates.Length);
                for (int i = 0; i < fieldStates.Length; i++)
                {
                    ((IStateManager)Items[i]).LoadViewState(fieldStates[i]);
                }
                if (allFieldsSaved)
                {
                    SetFieldsDirty();
                }
            }
        }

        /// <internalonly/>
        protected Object SaveViewState()
        {
            int fieldsCount = Count;
            if (fieldsCount > 0)
            {
                Object[] fieldStates = new Object[fieldsCount];
                bool haveState = _fieldsSetDirty;
                for (int i = 0; i < fieldsCount; i++)
                {
                    fieldStates[i] = ((IStateManager)Items[i]).SaveViewState();
                    if (fieldStates[i] != null)
                    {
                        haveState = true;
                    }
                }
                if (haveState)
                {
                    return new Object[]{_fieldsSetDirty, fieldStates};
                }
            }
            return null;
        }

        private void EnsureCount(int count)
        {

            int diff = Count - count;
            if (diff > 0)
            {
                Items.RemoveRange(count, diff);
                NotifyOwnerChange();
            }
            else
            {
                // Set owner = null, to avoid multiple change notifications. 
                // We'll call it just once later.

                ObjectList prevOwner = _owner;
                _owner = null;
                for (int i = diff; i < 0; i++)
                {
                    ObjectListField field = new ObjectListField();
                    Add(field);
                    field.SetOwner(prevOwner);
                }
                _owner = prevOwner;
                NotifyOwnerChange();
            }
        }

        private void ClearFieldsViewState()
        {
            foreach (ObjectListField field in Items)
            {
                field.ClearViewState();
            }
        }

        #region Implementation of IStateManager
        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }

        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }

        /// <internalonly/>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }
        #endregion
    }
}

