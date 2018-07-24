using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;


    public class Start
    {
        internal Start() { }


        /// <summary>
        /// Starts Vive tracking...
        /// </summary>
        /// <returns></returns>
        [CanUpdatePeriodically(true)]
        public static object StartVive()
        {
            return null;
        }

        public static object CalibrateOrigin()
        {
            return null;
        }
    }

