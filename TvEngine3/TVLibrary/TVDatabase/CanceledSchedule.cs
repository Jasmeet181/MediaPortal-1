//========================================================================
// This file was generated using the MyGeneration tool in combination
// with the Gentle.NET Business Entity template, $Rev: 965 $
//========================================================================
using System;
using System.Collections.Generic;
using Gentle.Framework;
using TvLibrary.Log;

namespace TvDatabase
{
  /// <summary>
  /// Instances of this class represent the properties and methods of a row in the table <b>CanceledSchedule</b>.
  /// </summary>
  [TableName("CanceledSchedule")]
  public class CanceledSchedule : Persistent
  {
    #region Members

    private bool isChanged;
    [TableColumn("idCanceledSchedule", NotNull = true), PrimaryKey(AutoGenerated = true)] private int idCanceledSchedule;
    [TableColumn("idSchedule", NotNull = true), ForeignKey("Schedule", "id_Schedule")] private int idSchedule;
    [TableColumn("cancelDateTime", NotNull = true)] private DateTime cancelDateTime;

    #endregion

    #region Constructors

    /// <summary> 
    /// Create a new object by specifying all fields (except the auto-generated primary key field). 
    /// </summary> 
    public CanceledSchedule(int idSchedule, DateTime cancelDateTime)
    {
      isChanged = true;
      this.idSchedule = idSchedule;
      this.cancelDateTime = cancelDateTime;
    }

    /// <summary> 
    /// Create an object from an existing row of data. This will be used by Gentle to 
    /// construct objects from retrieved rows. 
    /// </summary> 
    public CanceledSchedule(int idCanceledSchedule, int idSchedule, DateTime cancelDateTime)
    {
      this.idCanceledSchedule = idCanceledSchedule;
      this.idSchedule = idSchedule;
      this.cancelDateTime = cancelDateTime;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates whether the entity is changed and requires saving or not.
    /// </summary>
    public bool IsChanged
    {
      get { return isChanged; }
    }

    /// <summary>
    /// Property relating to database column idCanceledSchedule
    /// </summary>
    public int IdCanceledSchedule
    {
      get { return idCanceledSchedule; }
    }

    /// <summary>
    /// Property relating to database column idSchedule
    /// </summary>
    public int IdSchedule
    {
      get { return idSchedule; }
      set
      {
        isChanged |= idSchedule != value;
        idSchedule = value;
      }
    }

    /// <summary>
    /// Property relating to database column cancelDateTime
    /// </summary>
    public DateTime CancelDateTime
    {
      get { return cancelDateTime; }
      set
      {
        isChanged |= cancelDateTime != value;
        cancelDateTime = value;
      }
    }

    #endregion

    #region Storage and Retrieval

    /// <summary>
    /// Static method to retrieve all instances that are stored in the database in one call
    /// </summary>
    public static IList<CanceledSchedule> ListAll()
    {
      return Broker.RetrieveList<CanceledSchedule>();
    }

    /// <summary>
    /// Retrieves an entity given it's id.
    /// </summary>
    public static CanceledSchedule Retrieve(int id)
    {
      // Return null if id is smaller than seed and/or increment for autokey
      if (id < 1)
      {
        return null;
      }
      Key key = new Key(typeof (CanceledSchedule), true, "idCanceledSchedule", id);
      return Broker.RetrieveInstance<CanceledSchedule>(key);
    }

    /// <summary>
    /// Retrieves an entity given it's id, using Gentle.Framework.Key class.
    /// This allows retrieval based on multi-column keys.
    /// </summary>
    public static CanceledSchedule Retrieve(Key key)
    {
      return Broker.RetrieveInstance<CanceledSchedule>(key);
    }

    /// <summary>
    /// Persists the entity if it was never persisted or was changed.
    /// </summary>
    public override void Persist()
    {
      if (IsChanged || !IsPersisted)
      {
        try
        {
          base.Persist();
          Program.ResetSingleState(this.cancelDateTime, this.ReferencedSchedule().ReferencedChannel().IdChannel, this.ReferencedSchedule().ProgramName);          
        }
        catch (Exception ex)
        {
          Log.Error("Exception in CanceledSchedule.Persist() with Message {0}", ex.Message);
          return;
        }
        isChanged = false;
      }
    }

    #endregion

    #region Relations

    /// <summary>
    ///
    /// </summary>
    public Schedule ReferencedSchedule()
    {
      return Schedule.Retrieve(IdSchedule);
    }

    #endregion
  }
}