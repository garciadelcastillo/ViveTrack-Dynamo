using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

using Valve.VR;
using ViveTrack;

using DSPlane = Autodesk.DesignScript.Geometry.Plane;

public class Devices
{
    internal Devices() { }

    
    //  ██╗  ██╗███╗   ███╗██████╗ 
    //  ██║  ██║████╗ ████║██╔══██╗
    //  ███████║██╔████╔██║██║  ██║
    //  ██╔══██║██║╚██╔╝██║██║  ██║
    //  ██║  ██║██║ ╚═╝ ██║██████╔╝
    //  ╚═╝  ╚═╝╚═╝     ╚═╝╚═════╝ 
    //                             

    // Due to Zero Touch nature, these instances will be shared by all nodes... Not ideal, but good enough for 99% of situations.
    private static VrTrackedDevice _HMD_CurrentTrackedDevice;
    private static CoordinateSystem _HMD_CurrentCS;
    private static CoordinateSystem _HMD_OldCS;
    private static DSPlane _HMD_OldPlane;

    /// <summary>
    /// Tracking of HTC Vive Head Mounted Display (HMD).
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "Plane">The device's Plane.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    [MultiReturn(new[] { "Mesh", "Plane", "CoordinateSystem" })]
    public static Dictionary<string, object> HMD(object Vive, bool tracked = true)
    {
        OpenvrWrapper wrapper;
        try
        {
            wrapper = Vive as OpenvrWrapper;
        }
        catch
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("Please connect a Vive object to this node's input.");
            return null;
        }

        var list = wrapper.TrackedDevices.IndexesByClasses["HMD"];
        if (list.Count == 0)
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("No HMD detected.");
            return null;
        }


        if (tracked)
        {
            int index = wrapper.TrackedDevices.IndexesByClasses["HMD"][0];

            _HMD_CurrentTrackedDevice = wrapper.TrackedDevices.AllDevices[index];
            _HMD_CurrentTrackedDevice.ConvertPose();

            _HMD_CurrentCS = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Matrix4x4ToCoordinateSystem(_HMD_CurrentTrackedDevice.CorrectedMatrix4x4, false))
            {
                _HMD_CurrentCS = _HMD_CurrentCS.Transform(cm);
            }

            _HMD_OldCS = _HMD_CurrentCS;
            _HMD_OldPlane = CoordinateSystemToPlane(_HMD_OldCS);

        }

        // @TODO: figure out mesh representation

        return new Dictionary<string, object>()
        {
            { "Mesh", null },
            { "Plane", _HMD_OldPlane },
            { "CoordinateSystem", _HMD_OldCS }
        };
    }



    //   ██████╗ ██████╗ ███╗   ██╗████████╗██████╗  ██████╗ ██╗     ██╗     ███████╗██████╗ ███████╗
    //  ██╔════╝██╔═══██╗████╗  ██║╚══██╔══╝██╔══██╗██╔═══██╗██║     ██║     ██╔════╝██╔══██╗██╔════╝
    //  ██║     ██║   ██║██╔██╗ ██║   ██║   ██████╔╝██║   ██║██║     ██║     █████╗  ██████╔╝███████╗
    //  ██║     ██║   ██║██║╚██╗██║   ██║   ██╔══██╗██║   ██║██║     ██║     ██╔══╝  ██╔══██╗╚════██║
    //  ╚██████╗╚██████╔╝██║ ╚████║   ██║   ██║  ██║╚██████╔╝███████╗███████╗███████╗██║  ██║███████║
    //   ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝╚══════╝
    //                                                                                               

    private static VrTrackedDevice _Controller1_CurrentTrackedDevice;
    private static CoordinateSystem _Controller1_CurrentCS;
    private static CoordinateSystem _Controller1_OldCS;
    private static DSPlane _Controller1_OldPlane;

    /// <summary>
    /// Tracking of HTC Vive Controller #1.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "Plane">The device's Plane.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    /// <returns name = "TriggerPressed">Is the trigger pressed?</returns>
    /// <returns name = "TriggerClicked">Is the trigger clicked (pressed all the way in)?</returns>
    /// <returns name = "TriggerValue">Trigger level from 0 (not pressed) to 1 (fully pressed).</returns>
    /// <returns name = "TouchPadTouched">Is the touchpad being touched?</returns>
    /// <returns name = "TouchPadClicked">Is the touchpad being clicked (pressed all the way in)?</returns>
    /// <returns name = "TouchPadValueX">Touch value from -1 (left) to 1 (right).</returns>
    /// <returns name = "TouchPadValueY">Touch value from -1 (bottom) to 1 (top).</returns>
    [MultiReturn(new[] { "Mesh", "Plane", "CoordinateSystem", "TriggerPressed", "TriggerClicked", "TriggerValue", "TouchPadTouched", "TouchPadClicked", "TouchPadValueX", "TouchPadValueY" })]
    public static Dictionary<string, object> Controller1(object Vive, bool tracked = true)
    {
        OpenvrWrapper wrapper;
        try
        {
            wrapper = Vive as OpenvrWrapper;
        }
        catch
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("Please connect a Vive object to this node's input.");
            return null;
        }

        var list = wrapper.TrackedDevices.IndexesByClasses["Controller"];
        if (list.Count == 0)
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("No Controller detected.");
            return null;
        }
        
        if (tracked)
        {
            int index = wrapper.TrackedDevices.IndexesByClasses["Controller"][0];

            _Controller1_CurrentTrackedDevice = wrapper.TrackedDevices.AllDevices[index];
            _Controller1_CurrentTrackedDevice.ConvertPose();

            _Controller1_CurrentCS = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Matrix4x4ToCoordinateSystem(_Controller1_CurrentTrackedDevice.CorrectedMatrix4x4, false))
            {
                _Controller1_CurrentCS = _Controller1_CurrentCS.Transform(cm);
            }

            _Controller1_OldCS = _Controller1_CurrentCS;
            _Controller1_OldPlane = CoordinateSystemToPlane(_Controller1_OldCS);
        }

        // @TODO: figure out mesh representation

        _Controller1_CurrentTrackedDevice.GetControllerTriggerState();

        return new Dictionary<string, object>()
        {
            { "Mesh", null },
            { "Plane", _Controller1_OldPlane },
            { "CoordinateSystem", _Controller1_OldCS },
            { "TriggerPressed", _Controller1_CurrentTrackedDevice.TriggerPressed },
            { "TriggerClicked", _Controller1_CurrentTrackedDevice.TriggerClicked },
            { "TriggerValue", _Controller1_CurrentTrackedDevice.TriggerValue },
            { "TouchPadTouched", _Controller1_CurrentTrackedDevice.TouchPadTouched },
            { "TouchPadClicked", _Controller1_CurrentTrackedDevice.TouchPadClicked },
            { "TouchPadValueX", _Controller1_CurrentTrackedDevice.TouchPadValueX },
            { "TouchPadValueY", _Controller1_CurrentTrackedDevice.TouchPadValueY }
        };
    }



    private static VrTrackedDevice _Controller2_CurrentTrackedDevice;
    private static CoordinateSystem _Controller2_CurrentCS;
    private static CoordinateSystem _Controller2_OldCS;
    private static DSPlane _Controller2_OldPlane;

    /// <summary>
    /// Tracking of HTC Vive Controller #2.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "Plane">The device's Plane.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    /// <returns name = "TriggerPressed">Is the trigger pressed?</returns>
    /// <returns name = "TriggerClicked">Is the trigger clicked (pressed all the way in)?</returns>
    /// <returns name = "TriggerValue">Trigger level from 0 (not pressed) to 1 (fully pressed).</returns>
    /// <returns name = "TouchPadTouched">Is the touchpad being touched?</returns>
    /// <returns name = "TouchPadClicked">Is the touchpad being clicked (pressed all the way in)?</returns>
    /// <returns name = "TouchPadValueX">Touch value from -1 (left) to 1 (right).</returns>
    /// <returns name = "TouchPadValueY">Touch value from -1 (bottom) to 1 (top).</returns>
    [MultiReturn(new[] { "Mesh", "Plane", "CoordinateSystem", "TriggerPressed", "TriggerClicked", "TriggerValue", "TouchPadTouched", "TouchPadClicked", "TouchPadValueX", "TouchPadValueY" })]
    public static Dictionary<string, object> Controller2(object Vive, bool tracked = true)
    {
        OpenvrWrapper wrapper;
        try
        {
            wrapper = Vive as OpenvrWrapper;
        }
        catch
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("Please connect a Vive object to this node's input.");
            return null;
        }

        var list = wrapper.TrackedDevices.IndexesByClasses["Controller"];
        if (list.Count < 2)
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("No Controller detected.");
            return null;
        }

        if (tracked)
        {
            int index = wrapper.TrackedDevices.IndexesByClasses["Controller"][1];

            _Controller2_CurrentTrackedDevice = wrapper.TrackedDevices.AllDevices[index];
            _Controller2_CurrentTrackedDevice.ConvertPose();

            _Controller2_CurrentCS = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Matrix4x4ToCoordinateSystem(_Controller2_CurrentTrackedDevice.CorrectedMatrix4x4, false))
            {
                _Controller2_CurrentCS = _Controller2_CurrentCS.Transform(cm);
            }

            _Controller2_OldCS = _Controller2_CurrentCS;
            _Controller2_OldPlane = CoordinateSystemToPlane(_Controller2_OldCS);
        }

        // @TODO: figure out mesh representation

        _Controller2_CurrentTrackedDevice.GetControllerTriggerState();

        return new Dictionary<string, object>()
        {
            { "Mesh", null },
            { "Plane", _Controller2_OldPlane },
            { "CoordinateSystem", _Controller2_OldCS },
            { "TriggerPressed", _Controller2_CurrentTrackedDevice.TriggerPressed },
            { "TriggerClicked", _Controller2_CurrentTrackedDevice.TriggerClicked },
            { "TriggerValue", _Controller2_CurrentTrackedDevice.TriggerValue },
            { "TouchPadTouched", _Controller2_CurrentTrackedDevice.TouchPadTouched },
            { "TouchPadClicked", _Controller2_CurrentTrackedDevice.TouchPadClicked },
            { "TouchPadValueX", _Controller2_CurrentTrackedDevice.TouchPadValueX },
            { "TouchPadValueY", _Controller2_CurrentTrackedDevice.TouchPadValueY }
        };
    }




    //  ██╗     ██╗ ██████╗ ██╗  ██╗████████╗██╗  ██╗ ██████╗ ██╗   ██╗███████╗███████╗███████╗
    //  ██║     ██║██╔════╝ ██║  ██║╚══██╔══╝██║  ██║██╔═══██╗██║   ██║██╔════╝██╔════╝██╔════╝
    //  ██║     ██║██║  ███╗███████║   ██║   ███████║██║   ██║██║   ██║███████╗█████╗  ███████╗
    //  ██║     ██║██║   ██║██╔══██║   ██║   ██╔══██║██║   ██║██║   ██║╚════██║██╔══╝  ╚════██║
    //  ███████╗██║╚██████╔╝██║  ██║   ██║   ██║  ██║╚██████╔╝╚██████╔╝███████║███████╗███████║
    //  ╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝  ╚═════╝ ╚══════╝╚══════╝╚══════╝
    //                                                                                         

    private static VrTrackedDevice _Lighthouse1_CurrentTrackedDevice;
    private static CoordinateSystem _Lighthouse1_CurrentCS;
    private static CoordinateSystem _Lighthouse1_OldCS;
    private static DSPlane _Lighthouse1_OldPlane;

    /// <summary>
    /// Tracking of HTC Vive Lighthouse #1.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "Plane">The device's Plane.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    [MultiReturn(new[] { "Mesh", "Plane", "CoordinateSystem" })]
    public static Dictionary<string, object> Lighthouse1(object Vive, bool tracked = true)
    {
        OpenvrWrapper wrapper;
        try
        {
            wrapper = Vive as OpenvrWrapper;
        }
        catch
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("Please connect a Vive object to this node's input.");
            return null;
        }

        var list = wrapper.TrackedDevices.IndexesByClasses["Lighthouse"];
        if (list.Count == 0)
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("No Lighthouse detected.");
            return null;
        }


        if (tracked)
        {
            int index = wrapper.TrackedDevices.IndexesByClasses["Lighthouse"][0];

            _Lighthouse1_CurrentTrackedDevice = wrapper.TrackedDevices.AllDevices[index];
            _Lighthouse1_CurrentTrackedDevice.ConvertPose();

            _Lighthouse1_CurrentCS = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Matrix4x4ToCoordinateSystem(_Lighthouse1_CurrentTrackedDevice.CorrectedMatrix4x4, false))
            {
                _Lighthouse1_CurrentCS = _Lighthouse1_CurrentCS.Transform(cm);
            }

            _Lighthouse1_OldCS = _Lighthouse1_CurrentCS;
            _Lighthouse1_OldPlane = CoordinateSystemToPlane(_Lighthouse1_OldCS);

        }

        // @TODO: figure out mesh representation

        return new Dictionary<string, object>()
        {
            { "Mesh", null },
            { "Plane", _Lighthouse1_OldPlane },
            { "CoordinateSystem", _Lighthouse1_OldCS }
        };
    }



    private static VrTrackedDevice _Lighthouse2_CurrentTrackedDevice;
    private static CoordinateSystem _Lighthouse2_CurrentCS;
    private static CoordinateSystem _Lighthouse2_OldCS;
    private static DSPlane _Lighthouse2_OldPlane;

    /// <summary>
    /// Tracking of HTC Vive Lighthouse #2.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "Plane">The device's Plane.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    [MultiReturn(new[] { "Mesh", "Plane", "CoordinateSystem" })]
    public static Dictionary<string, object> Lighthouse2(object Vive, bool tracked = true)
    {
        OpenvrWrapper wrapper;
        try
        {
            wrapper = Vive as OpenvrWrapper;
        }
        catch
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("Please connect a Vive object to this node's input.");
            return null;
        }

        var list = wrapper.TrackedDevices.IndexesByClasses["Lighthouse"];
        if (list.Count < 2)
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("No Lighthouse detected.");
            return null;
        }


        if (tracked)
        {
            int index = wrapper.TrackedDevices.IndexesByClasses["Lighthouse"][1];

            _Lighthouse2_CurrentTrackedDevice = wrapper.TrackedDevices.AllDevices[index];
            _Lighthouse2_CurrentTrackedDevice.ConvertPose();

            _Lighthouse2_CurrentCS = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Matrix4x4ToCoordinateSystem(_Lighthouse2_CurrentTrackedDevice.CorrectedMatrix4x4, false))
            {
                _Lighthouse2_CurrentCS = _Lighthouse2_CurrentCS.Transform(cm);
            }

            _Lighthouse2_OldCS = _Lighthouse2_CurrentCS;
            _Lighthouse2_OldPlane = CoordinateSystemToPlane(_Lighthouse2_OldCS);
        }

        // @TODO: figure out mesh representation

        return new Dictionary<string, object>()
        {
            { "Mesh", null },
            { "Plane", _Lighthouse2_OldPlane },
            { "CoordinateSystem", _Lighthouse2_OldCS }
        };
    }



    //  ████████╗██████╗  █████╗  ██████╗██╗  ██╗███████╗██████╗ 
    //  ╚══██╔══╝██╔══██╗██╔══██╗██╔════╝██║ ██╔╝██╔════╝██╔══██╗
    //     ██║   ██████╔╝███████║██║     █████╔╝ █████╗  ██████╔╝
    //     ██║   ██╔══██╗██╔══██║██║     ██╔═██╗ ██╔══╝  ██╔══██╗
    //     ██║   ██║  ██║██║  ██║╚██████╗██║  ██╗███████╗██║  ██║
    //     ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝
    //                                                           

    private static VrTrackedDevice[] _GenericTracker_CurrentTrackedDevice = new VrTrackedDevice[8];
    private static CoordinateSystem[] _GenericTracker_CurrentCS = new CoordinateSystem[8];
    private static CoordinateSystem[] _GenericTracker_OldCS = new CoordinateSystem[8];
    private static DSPlane[] _GenericTracker_OldPlane = new DSPlane[8];

    /// <summary>
    /// Tracking of HTC Vive Generic Tracker.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="index">If more than one Tracker, choose index number.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "Plane">The device's Plane.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    [MultiReturn(new[] { "Mesh", "Plane", "CoordinateSystem" })]
    public static Dictionary<string, object> GenericTracker(object Vive, int index = 0, bool tracked = true)
    {
        OpenvrWrapper wrapper;
        try
        {
            wrapper = Vive as OpenvrWrapper;
        }
        catch
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("Please connect a Vive object to this node's input.");
            return null;
        }

        var list = wrapper.TrackedDevices.IndexesByClasses["Tracker"];
        if (list.Count < index + 1)
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("Cannot find Generic Tracker. Wrong index?");
            return null;
        }
        
        if (tracked)
        {
            int id = wrapper.TrackedDevices.IndexesByClasses["Tracker"][index];

            _GenericTracker_CurrentTrackedDevice[index] = wrapper.TrackedDevices.AllDevices[id];
            _GenericTracker_CurrentTrackedDevice[index].ConvertPose();
            _GenericTracker_CurrentTrackedDevice[index].CorrectGenericTrackerMatrix();

            _GenericTracker_CurrentCS[index] = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Matrix4x4ToCoordinateSystem(_GenericTracker_CurrentTrackedDevice[index].CorrectedMatrix4x4, false))
            {
                _GenericTracker_CurrentCS[index] = _GenericTracker_CurrentCS[index].Transform(cm);
            }

            _GenericTracker_OldCS[index] = _GenericTracker_CurrentCS[index];
            _GenericTracker_OldPlane[index] = CoordinateSystemToPlane(_GenericTracker_OldCS[index]);
        }

        // @TODO: figure out mesh representation

        return new Dictionary<string, object>()
        {
            { "Mesh", null },
            { "Plane", _GenericTracker_OldPlane[index] },
            { "CoordinateSystem", _GenericTracker_OldCS[index] }
        };
    }







    //  ██╗   ██╗████████╗██╗██╗     ███████╗
    //  ██║   ██║╚══██╔══╝██║██║     ██╔════╝
    //  ██║   ██║   ██║   ██║██║     ███████╗
    //  ██║   ██║   ██║   ██║██║     ╚════██║
    //  ╚██████╔╝   ██║   ██║███████╗███████║
    //   ╚═════╝    ╚═╝   ╚═╝╚══════╝╚══════╝
    //                                       

    internal static DSPlane CoordinateSystemToPlane(CoordinateSystem cs)
    {
        DSPlane pl = DSPlane.ByOriginXAxisYAxis(cs.Origin, cs.XAxis, cs.YAxis);
        return pl;
    }

    internal static CoordinateSystem Matrix4x4ToCoordinateSystem(Matrix4x4 m, bool transpose)
    {
        double[] mm = Matrix4x4ToDoubleArray(m, transpose);
        CoordinateSystem cs = CoordinateSystem.ByMatrix(mm);
        return cs;
    }

    internal static Matrix4x4 DSPlaneToMatrix4x4(DSPlane plane)
    {
        double[] pl = DSPlaneToDoubleArray(plane, true);
        Matrix4x4 m = new Matrix4x4(
            (float)pl[0], (float)pl[1], (float)pl[2], (float)pl[3],
            (float)pl[4], (float)pl[5], (float)pl[6], (float)pl[7],
            (float)pl[8], (float)pl[9], (float)pl[10], (float)pl[11],
            (float)pl[12], (float)pl[13], (float)pl[14], (float)pl[15]);
        return m;
    }

    internal static double[] Matrix4x4ToDoubleArray(Matrix4x4 m, bool transpose)
    {
        double[] a = new double[16];
        if (transpose)
        {
            a[0] = m.M11; a[1] = m.M21; a[2] = m.M31; a[3] = m.M41;
            a[4] = m.M12; a[5] = m.M22; a[6] = m.M32; a[7] = m.M42;
            a[8] = m.M13; a[9] = m.M23; a[10] = m.M33; a[11] = m.M43;
            a[12] = m.M14; a[13] = m.M24; a[14] = m.M34; a[15] = m.M44;
        }
        else
        {
            a[0] = m.M11; a[1] = m.M12; a[2] = m.M13; a[3] = m.M14;
            a[4] = m.M21; a[5] = m.M22; a[6] = m.M23; a[7] = m.M24;
            a[8] = m.M31; a[9] = m.M32; a[10] = m.M33; a[11] = m.M34;
            a[12] = m.M41; a[13] = m.M42; a[14] = m.M43; a[15] = m.M44;
        }
        return a;
    }

    internal static double[] DSPlaneToDoubleArray(DSPlane pl, bool transpose)
    {
        double[] a = new double[16];
        if (transpose)
        {
            a[0] = pl.XAxis.X; a[1] = pl.XAxis.Y; a[2] = pl.XAxis.Z; a[3] = 0;
            a[4] = pl.YAxis.X; a[5] = pl.YAxis.Y; a[6] = pl.YAxis.Z; a[7] = 0;
            a[8] = pl.Normal.X; a[9] = pl.Normal.Y; a[10] = pl.Normal.Z; a[11] = 0;
            a[12] = pl.Origin.X; a[13] = pl.Origin.Y; a[14] = pl.Origin.Z; a[15] = 1;
        }
        else
        {
            a[0] = pl.XAxis.X; a[1] = pl.YAxis.X; a[2] = pl.Normal.X; a[3] = pl.Origin.X;
            a[4] = pl.XAxis.Y; a[5] = pl.YAxis.Y; a[6] = pl.Normal.Y; a[7] = pl.Origin.Y;
            a[8] = pl.XAxis.Z; a[9] = pl.YAxis.Z; a[10] = pl.Normal.Z; a[11] = pl.Origin.Z;
            a[12] = 0; a[13] = 0; a[14] = 0; a[15] = 1;
        }
        return a;
    }
    
}
