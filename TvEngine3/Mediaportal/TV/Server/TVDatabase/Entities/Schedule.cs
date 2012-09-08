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
    [KnownType(typeof(Channel))]
    [KnownType(typeof(Schedule))]
    [KnownType(typeof(Recording))]
    [KnownType(typeof(CanceledSchedule))]
    [KnownType(typeof(Conflict))]
    public partial class Schedule: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int id_Schedule
        {
            get { return _id_Schedule; }
            set
            {
                if (_id_Schedule != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'id_Schedule' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _id_Schedule = value;
                    OnPropertyChanged("id_Schedule");
                }
            }
        }
        private int _id_Schedule;
    
        [DataMember]
        public int idChannel
        {
            get { return _idChannel; }
            set
            {
                if (_idChannel != value)
                {
                    ChangeTracker.RecordOriginalValue("idChannel", _idChannel);
                    if (!IsDeserializing)
                    {
                        if (Channel != null && Channel.idChannel != value)
                        {
                            Channel = null;
                        }
                    }
                    _idChannel = value;
                    OnPropertyChanged("idChannel");
                }
            }
        }
        private int _idChannel;
    
        [DataMember]
        public int scheduleType
        {
            get { return _scheduleType; }
            set
            {
                if (_scheduleType != value)
                {
                    _scheduleType = value;
                    OnPropertyChanged("scheduleType");
                }
            }
        }
        private int _scheduleType;
    
        [DataMember]
        public string programName
        {
            get { return _programName; }
            set
            {
                if (_programName != value)
                {
                    _programName = value;
                    OnPropertyChanged("programName");
                }
            }
        }
        private string _programName;
    
        [DataMember]
        public System.DateTime startTime
        {
            get { return _startTime; }
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    OnPropertyChanged("startTime");
                }
            }
        }
        private System.DateTime _startTime;
    
        [DataMember]
        public System.DateTime endTime
        {
            get { return _endTime; }
            set
            {
                if (_endTime != value)
                {
                    _endTime = value;
                    OnPropertyChanged("endTime");
                }
            }
        }
        private System.DateTime _endTime;
    
        [DataMember]
        public int maxAirings
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
        public int priority
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
        public string directory
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
        public int quality
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
        public int keepMethod
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
        public Nullable<System.DateTime> keepDate
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
        public int preRecordInterval
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
        public int postRecordInterval
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
        public System.DateTime canceled
        {
            get { return _canceled; }
            set
            {
                if (_canceled != value)
                {
                    _canceled = value;
                    OnPropertyChanged("canceled");
                }
            }
        }
        private System.DateTime _canceled;
    
        [DataMember]
        public bool series
        {
            get { return _series; }
            set
            {
                if (_series != value)
                {
                    _series = value;
                    OnPropertyChanged("series");
                }
            }
        }
        private bool _series;
    
        [DataMember]
        public Nullable<int> idParentSchedule
        {
            get { return _idParentSchedule; }
            set
            {
                if (_idParentSchedule != value)
                {
                    ChangeTracker.RecordOriginalValue("idParentSchedule", _idParentSchedule);
                    if (!IsDeserializing)
                    {
                        if (ParentSchedule != null && ParentSchedule.id_Schedule != value)
                        {
                            ParentSchedule = null;
                        }
                    }
                    _idParentSchedule = value;
                    OnPropertyChanged("idParentSchedule");
                }
            }
        }
        private Nullable<int> _idParentSchedule;

        #endregion
        #region Navigation Properties
    
        [DataMember]
        public Channel Channel
        {
            get { return _channel; }
            set
            {
                if (!ReferenceEquals(_channel, value))
                {
                    var previousValue = _channel;
                    _channel = value;
                    FixupChannel(previousValue);
                    OnNavigationPropertyChanged("Channel");
                }
            }
        }
        private Channel _channel;
    
        [DataMember]
        public TrackableCollection<Schedule> Schedules
        {
            get
            {
                if (_schedules == null)
                {
                    _schedules = new TrackableCollection<Schedule>();
                    _schedules.CollectionChanged += FixupSchedules;
                }
                return _schedules;
            }
            set
            {
                if (!ReferenceEquals(_schedules, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_schedules != null)
                    {
                        _schedules.CollectionChanged -= FixupSchedules;
                    }
                    _schedules = value;
                    if (_schedules != null)
                    {
                        _schedules.CollectionChanged += FixupSchedules;
                    }
                    OnNavigationPropertyChanged("Schedules");
                }
            }
        }
        private TrackableCollection<Schedule> _schedules;
    
        [DataMember]
        public Schedule ParentSchedule
        {
            get { return _parentSchedule; }
            set
            {
                if (!ReferenceEquals(_parentSchedule, value))
                {
                    var previousValue = _parentSchedule;
                    _parentSchedule = value;
                    FixupParentSchedule(previousValue);
                    OnNavigationPropertyChanged("ParentSchedule");
                }
            }
        }
        private Schedule _parentSchedule;
    
        [DataMember]
        public TrackableCollection<Recording> Recordings
        {
            get
            {
                if (_recordings == null)
                {
                    _recordings = new TrackableCollection<Recording>();
                    _recordings.CollectionChanged += FixupRecordings;
                }
                return _recordings;
            }
            set
            {
                if (!ReferenceEquals(_recordings, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_recordings != null)
                    {
                        _recordings.CollectionChanged -= FixupRecordings;
                    }
                    _recordings = value;
                    if (_recordings != null)
                    {
                        _recordings.CollectionChanged += FixupRecordings;
                    }
                    OnNavigationPropertyChanged("Recordings");
                }
            }
        }
        private TrackableCollection<Recording> _recordings;
    
        [DataMember]
        public TrackableCollection<CanceledSchedule> CanceledSchedules
        {
            get
            {
                if (_canceledSchedules == null)
                {
                    _canceledSchedules = new TrackableCollection<CanceledSchedule>();
                    _canceledSchedules.CollectionChanged += FixupCanceledSchedules;
                }
                return _canceledSchedules;
            }
            set
            {
                if (!ReferenceEquals(_canceledSchedules, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_canceledSchedules != null)
                    {
                        _canceledSchedules.CollectionChanged -= FixupCanceledSchedules;
                        // This is the principal end in an association that performs cascade deletes.
                        // Remove the cascade delete event handler for any entities in the current collection.
                        foreach (CanceledSchedule item in _canceledSchedules)
                        {
                            ChangeTracker.ObjectStateChanging -= item.HandleCascadeDelete;
                        }
                    }
                    _canceledSchedules = value;
                    if (_canceledSchedules != null)
                    {
                        _canceledSchedules.CollectionChanged += FixupCanceledSchedules;
                        // This is the principal end in an association that performs cascade deletes.
                        // Add the cascade delete event handler for any entities that are already in the new collection.
                        foreach (CanceledSchedule item in _canceledSchedules)
                        {
                            ChangeTracker.ObjectStateChanging += item.HandleCascadeDelete;
                        }
                    }
                    OnNavigationPropertyChanged("CanceledSchedules");
                }
            }
        }
        private TrackableCollection<CanceledSchedule> _canceledSchedules;
    
        [DataMember]
        public TrackableCollection<Conflict> Conflicts
        {
            get
            {
                if (_conflicts == null)
                {
                    _conflicts = new TrackableCollection<Conflict>();
                    _conflicts.CollectionChanged += FixupConflicts;
                }
                return _conflicts;
            }
            set
            {
                if (!ReferenceEquals(_conflicts, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_conflicts != null)
                    {
                        _conflicts.CollectionChanged -= FixupConflicts;
                    }
                    _conflicts = value;
                    if (_conflicts != null)
                    {
                        _conflicts.CollectionChanged += FixupConflicts;
                    }
                    OnNavigationPropertyChanged("Conflicts");
                }
            }
        }
        private TrackableCollection<Conflict> _conflicts;
    
        [DataMember]
        public TrackableCollection<Conflict> ConflictingSchedules
        {
            get
            {
                if (_conflictingSchedules == null)
                {
                    _conflictingSchedules = new TrackableCollection<Conflict>();
                    _conflictingSchedules.CollectionChanged += FixupConflictingSchedules;
                }
                return _conflictingSchedules;
            }
            set
            {
                if (!ReferenceEquals(_conflictingSchedules, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_conflictingSchedules != null)
                    {
                        _conflictingSchedules.CollectionChanged -= FixupConflictingSchedules;
                    }
                    _conflictingSchedules = value;
                    if (_conflictingSchedules != null)
                    {
                        _conflictingSchedules.CollectionChanged += FixupConflictingSchedules;
                    }
                    OnNavigationPropertyChanged("ConflictingSchedules");
                }
            }
        }
        private TrackableCollection<Conflict> _conflictingSchedules;

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
            Channel = null;
            Schedules.Clear();
            ParentSchedule = null;
            Recordings.Clear();
            CanceledSchedules.Clear();
            Conflicts.Clear();
            ConflictingSchedules.Clear();
        }

        #endregion
        #region Association Fixup
    
        private void FixupChannel(Channel previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Schedules.Contains(this))
            {
                previousValue.Schedules.Remove(this);
            }
    
            if (Channel != null)
            {
                if (!Channel.Schedules.Contains(this))
                {
                    Channel.Schedules.Add(this);
                }
    
                idChannel = Channel.idChannel;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Channel")
                    && (ChangeTracker.OriginalValues["Channel"] == Channel))
                {
                    ChangeTracker.OriginalValues.Remove("Channel");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Channel", previousValue);
                }
                if (Channel != null && !Channel.ChangeTracker.ChangeTrackingEnabled)
                {
                    Channel.StartTracking();
                }
            }
        }
    
        private void FixupParentSchedule(Schedule previousValue, bool skipKeys = false)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Schedules.Contains(this))
            {
                previousValue.Schedules.Remove(this);
            }
    
            if (ParentSchedule != null)
            {
                if (!ParentSchedule.Schedules.Contains(this))
                {
                    ParentSchedule.Schedules.Add(this);
                }
    
                idParentSchedule = ParentSchedule.id_Schedule;
            }
            else if (!skipKeys)
            {
                idParentSchedule = null;
            }
    
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("ParentSchedule")
                    && (ChangeTracker.OriginalValues["ParentSchedule"] == ParentSchedule))
                {
                    ChangeTracker.OriginalValues.Remove("ParentSchedule");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("ParentSchedule", previousValue);
                }
                if (ParentSchedule != null && !ParentSchedule.ChangeTracker.ChangeTrackingEnabled)
                {
                    ParentSchedule.StartTracking();
                }
            }
        }
    
        private void FixupSchedules(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (Schedule item in e.NewItems)
                {
                    item.ParentSchedule = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("Schedules", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Schedule item in e.OldItems)
                {
                    if (ReferenceEquals(item.ParentSchedule, this))
                    {
                        item.ParentSchedule = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("Schedules", item);
                    }
                }
            }
        }
    
        private void FixupRecordings(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (Recording item in e.NewItems)
                {
                    item.Schedule = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("Recordings", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Recording item in e.OldItems)
                {
                    if (ReferenceEquals(item.Schedule, this))
                    {
                        item.Schedule = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("Recordings", item);
                    }
                }
            }
        }
    
        private void FixupCanceledSchedules(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (CanceledSchedule item in e.NewItems)
                {
                    item.Schedule = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("CanceledSchedules", item);
                    }
                    // This is the principal end in an association that performs cascade deletes.
                    // Update the event listener to refer to the new dependent.
                    ChangeTracker.ObjectStateChanging += item.HandleCascadeDelete;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (CanceledSchedule item in e.OldItems)
                {
                    if (ReferenceEquals(item.Schedule, this))
                    {
                        item.Schedule = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("CanceledSchedules", item);
                    }
                    // This is the principal end in an association that performs cascade deletes.
                    // Remove the previous dependent from the event listener.
                    ChangeTracker.ObjectStateChanging -= item.HandleCascadeDelete;
                }
            }
        }
    
        private void FixupConflicts(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (Conflict item in e.NewItems)
                {
                    item.Schedule = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("Conflicts", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Conflict item in e.OldItems)
                {
                    if (ReferenceEquals(item.Schedule, this))
                    {
                        item.Schedule = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("Conflicts", item);
                    }
                }
            }
        }
    
        private void FixupConflictingSchedules(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (Conflict item in e.NewItems)
                {
                    item.ConflictingSchedule = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("ConflictingSchedules", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Conflict item in e.OldItems)
                {
                    if (ReferenceEquals(item.ConflictingSchedule, this))
                    {
                        item.ConflictingSchedule = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("ConflictingSchedules", item);
                    }
                }
            }
        }

        #endregion
    }
}
