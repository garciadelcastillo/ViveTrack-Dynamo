using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

public class DataRecorder
{
    [IsVisibleInDynamoLibrary(false)]
    internal DataRecorder() { }

    private List<object> _records = new List<object>();

    /// <summary>
    /// Create a DataRecorder object to store varying data.
    /// </summary>
    /// <returns name = "DataRecorder">The DataRecorder object.</returns>
    public static DataRecorder Create()
    {
        return new DataRecorder();
    }

    /// <summary>
    /// Record varying data in a DataRecorder. 
    /// </summary>
    /// <param name="DataRecorder">Plug a DataRecorder object.</param>
    /// <param name="data">Data to record.</param>
    /// <param name="record">Record data?</param>
    /// <param name="reset">Flush all data from the DataRecorder?</param>
    /// <returns name = "recordedData"></returns>
    public static List<object> Record(DataRecorder DataRecorder, object data, bool record = true, bool reset = false)
    {
        if (record)
        {
            DataRecorder.Add(data);
        }

        if (reset)
        {
            DataRecorder.Flush();
        }

        return DataRecorder.GetRecords();
    }

    internal bool Add(object data)
    {
        if (_records.Count == 0)
        {
            _records.Add(data);
            return true;
        }
        else
        {
            object last = _records[_records.Count - 1];
            if (data.GetHashCode() != last.GetHashCode())
            {
                _records.Add(data);
                return true;
            }
        }
        return false;
    }

    internal void Flush()
    {
        _records.Clear();
    }

    internal List<object> GetRecords()
    {
        return _records;
    }

}
