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
    public partial class RuleBasedSchedule: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int IdRuleBasedSchedule
        {
            get { return _idRuleBasedSchedule; }
            set
            {
                if (_idRuleBasedSchedule != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'id_RuleBasedSchedule' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _idRuleBasedSchedule = value;
                    OnPropertyChanged("id_RuleBasedSchedule");
                }
            }
        }
        private int _idRuleBasedSchedule;
    
        [DataMember]
        public string ScheduleName
        {
            get { return _scheduleName; }
            set
            {
                if (_scheduleName != value)
                {
                    _scheduleName = value;
                    OnPropertyChanged("scheduleName");
                }
            }
        }
        private string _scheduleName;
    
        [DataMember]
        public int MaxAirings
        {
            get { return _maxAirings; }
            set
            {
                if (_maxAirings != value)
                {
                    _maxAirings = value;
                    OnPropertyChanged("maxAirings");
                }
            }
        }
        private int _maxAirings;
    
        [DataMember]
        public int Priority
        {
            get { return _priority; }
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged("priority");
                }
            }
        }
        private int _priority;
    
        [DataMember]
        public string Directory
        {
            get { return _directory; }
            set
            {
                if (_directory != value)
                {
                    _directory = value;
                    OnPropertyChanged("directory");
                }
            }
        }
        private string _directory;
    
        [DataMember]
        public int Quality
        {
            get { return _quality; }
            set
            {
                if (_quality != value)
                {
                    _quality = value;
                    OnPropertyChanged("quality");
                }
            }
        }
        private int _quality;
    
        [DataMember]
        public int KeepMethod
        {
            get { return _keepMethod; }
            set
            {
                if (_keepMethod != value)
                {
                    _keepMethod = value;
                    OnPropertyChanged("keepMethod");
                }
            }
        }
        private int _keepMethod;
    
        [DataMember]
        public Nullable<System.DateTime> KeepDate
        {
            get { return _keepDate; }
            set
            {
                if (_keepDate != value)
                {
                    _keepDate = value;
                    OnPropertyChanged("keepDate");
                }
            }
        }
        private Nullable<System.DateTime> _keepDate;
    
        [DataMember]
        public int PreRecordInterval
        {
            get { return _preRecordInterval; }
            set
            {
                if (_preRecordInterval != value)
                {
                    _preRecordInterval = value;
                    OnPropertyChanged("preRecordInterval");
                }
            }
        }
        private int _preRecordInterval;
    
        [DataMember]
        public int PostRecordInterval
        {
            get { return _postRecordInterval; }
            set
            {
                if (_postRecordInterval != value)
                {
                    _postRecordInterval = value;
                    OnPropertyChanged("postRecordInterval");
                }
            }
        }
        private int _postRecordInterval;
    
        [DataMember]
        public string Rules
        {
            get { return _rules; }
            set
            {
                if (_rules != value)
                {
                    _rules = value;
                    OnPropertyChanged("rules");
                }
            }
        }
        private string _rules;

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
        }

        #endregion
    }
}
