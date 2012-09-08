//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(Card))]
    [KnownType(typeof(CardGroup))]
    public partial class CardGroupMap: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int idMapping
        {
            get { return _idMapping; }
            set
            {
                if (_idMapping != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'idMapping' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _idMapping = value;
                    OnPropertyChanged("idMapping");
                }
            }
        }
        private int _idMapping;
    
        [DataMember]
        public int idCard
        {
            get { return _idCard; }
            set
            {
                if (_idCard != value)
                {
                    ChangeTracker.RecordOriginalValue("idCard", _idCard);
                    if (!IsDeserializing)
                    {
                        if (Card != null && Card.idCard != value)
                        {
                            Card = null;
                        }
                    }
                    _idCard = value;
                    OnPropertyChanged("idCard");
                }
            }
        }
        private int _idCard;
    
        [DataMember]
        public int idCardGroup
        {
            get { return _idCardGroup; }
            set
            {
                if (_idCardGroup != value)
                {
                    ChangeTracker.RecordOriginalValue("idCardGroup", _idCardGroup);
                    if (!IsDeserializing)
                    {
                        if (CardGroup != null && CardGroup.idCardGroup != value)
                        {
                            CardGroup = null;
                        }
                    }
                    _idCardGroup = value;
                    OnPropertyChanged("idCardGroup");
                }
            }
        }
        private int _idCardGroup;

        #endregion
        #region Navigation Properties
    
        [DataMember]
        public Card Card
        {
            get { return _card; }
            set
            {
                if (!ReferenceEquals(_card, value))
                {
                    var previousValue = _card;
                    _card = value;
                    FixupCard(previousValue);
                    OnNavigationPropertyChanged("Card");
                }
            }
        }
        private Card _card;
    
        [DataMember]
        public CardGroup CardGroup
        {
            get { return _cardGroup; }
            set
            {
                if (!ReferenceEquals(_cardGroup, value))
                {
                    var previousValue = _cardGroup;
                    _cardGroup = value;
                    FixupCardGroup(previousValue);
                    OnNavigationPropertyChanged("CardGroup");
                }
            }
        }
        private CardGroup _cardGroup;

        #endregion
        #region ChangeTracking
    
        protected virtual void OnPropertyChanged(String propertyName)
        {
            if (ChangeTracker.State != ObjectState.Added && ChangeTracker.State != ObjectState.Deleted)
            {
                ChangeTracker.State = ObjectState.Modified;
            }
            if (_propertyChanged != null)
            {
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
        protected virtual void OnNavigationPropertyChanged(String propertyName)
        {
            if (_propertyChanged != null)
            {
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged{ add { _propertyChanged += value; } remove { _propertyChanged -= value; } }
        private event PropertyChangedEventHandler _propertyChanged;
        private ObjectChangeTracker _changeTracker;
    
        [DataMember]
        public ObjectChangeTracker ChangeTracker
        {
            get
            {
                if (_changeTracker == null)
                {
                    _changeTracker = new ObjectChangeTracker();
                    _changeTracker.ObjectStateChanging += HandleObjectStateChanging;
                }
                return _changeTracker;
            }
            set
            {
                if(_changeTracker != null)
                {
                    _changeTracker.ObjectStateChanging -= HandleObjectStateChanging;
                }
                _changeTracker = value;
                if(_changeTracker != null)
                {
                    _changeTracker.ObjectStateChanging += HandleObjectStateChanging;
                }
            }
        }
    
        private void HandleObjectStateChanging(object sender, ObjectStateChangingEventArgs e)
        {
            if (e.NewState == ObjectState.Deleted)
            {
                ClearNavigationProperties();
            }
        }
    
        // This entity type is the dependent end in at least one association that performs cascade deletes.
        // This event handler will process notifications that occur when the principal end is deleted.
        internal void HandleCascadeDelete(object sender, ObjectStateChangingEventArgs e)
        {
            if (e.NewState == ObjectState.Deleted)
            {
                this.MarkAsDeleted();
            }
        }
    
        protected bool IsDeserializing { get; private set; }
    
        [OnDeserializing]
        public void OnDeserializingMethod(StreamingContext context)
        {
            IsDeserializing = true;
        }
    
        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            IsDeserializing = false;
            ChangeTracker.ChangeTrackingEnabled = true;
        }
    
        protected virtual void ClearNavigationProperties()
        {
            Card = null;
            CardGroup = null;
        }

        #endregion
        #region Association Fixup
    
        private void FixupCard(Card previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.CardGroupMaps.Contains(this))
            {
                previousValue.CardGroupMaps.Remove(this);
            }
    
            if (Card != null)
            {
                if (!Card.CardGroupMaps.Contains(this))
                {
                    Card.CardGroupMaps.Add(this);
                }
    
                idCard = Card.idCard;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Card")
                    && (ChangeTracker.OriginalValues["Card"] == Card))
                {
                    ChangeTracker.OriginalValues.Remove("Card");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Card", previousValue);
                }
                if (Card != null && !Card.ChangeTracker.ChangeTrackingEnabled)
                {
                    Card.StartTracking();
                }
            }
        }
    
        private void FixupCardGroup(CardGroup previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.CardGroupMaps.Contains(this))
            {
                previousValue.CardGroupMaps.Remove(this);
            }
    
            if (CardGroup != null)
            {
                if (!CardGroup.CardGroupMaps.Contains(this))
                {
                    CardGroup.CardGroupMaps.Add(this);
                }
    
                idCardGroup = CardGroup.idCardGroup;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("CardGroup")
                    && (ChangeTracker.OriginalValues["CardGroup"] == CardGroup))
                {
                    ChangeTracker.OriginalValues.Remove("CardGroup");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("CardGroup", previousValue);
                }
                if (CardGroup != null && !CardGroup.ChangeTracker.ChangeTrackingEnabled)
                {
                    CardGroup.StartTracking();
                }
            }
        }

        #endregion
    }
}
