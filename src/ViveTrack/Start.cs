using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

using Valve.VR;

using ViveTrack;

public class Start
{
    internal Start() { }

    public static OpenvrWrapper Vive = new OpenvrWrapper();
    public static CoordinateSystem CalibrationTransform = CoordinateSystem.Identity();
    public static Plane CalibrationPlane;
    public static string OutMsg;


    /// <summary>
    /// Start HTC Vive. Make sure SteamVR is running, and Dynamo is set to Periodic update.
    /// </summary>
    /// <param name="Connect">Connect to SteamVR?</param>
    /// <returns></returns>
    [CanUpdatePeriodically(true)]
    [MultiReturn(new[] { "Msg", "Vive" })]
    public static object ConnectToVive(bool Connect)
    {
        if (!Connect) return null;

        if (!DetectSteamVR())
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("SteamVR not running.Please run SteamVR first.");
            return null;
        }

        OutMsg = "Connected!";


        return new Dictionary<string, object> {
            { "Msg", OutMsg },
            { "Vive", Vive }
        };
    }


    internal static bool DetectSteamVR()
    {
        Process[] vrServer = Process.GetProcessesByName("vrserver");
        Process[] vrMonitor = Process.GetProcessesByName("vrmonitor");
        if ((vrServer.Length == 0) || (vrMonitor.Length == 0)) return false;
        if (!OpenVR.IsRuntimeInstalled()) return false;

        return true;
    }




    public static object CalibrateOrigin()
    {
        return null;
    }
}

