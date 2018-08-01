using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;


public class Counter
{

    [IsVisibleInDynamoLibrary(false)]
    internal Counter() { }

    private int _ticks = 0;

    /// <summary>
    /// An object that increases by one unit with each update.
    /// </summary>
    /// <returns></returns>
    public static Counter Create()
    {
        return new Counter();
    }

    /// <summary>
    /// Tick the Counter object to increase by one unit.
    /// </summary>
    /// <param name="counter"></param>
    /// <param name="tick"></param>
    /// <param name="reset"></param>
    /// <returns></returns>
    [CanUpdatePeriodically(true)]
    public static int Tick(Counter counter, bool tick = true, bool reset = false)
    {
        if (reset)
        {
            counter._ticks = 0;
        }
        else if (tick)
        {
            counter._ticks++;
        }

        return counter._ticks;
    }

}
