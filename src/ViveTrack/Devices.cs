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

using DSCore;

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
    private static PreviewableHMD _HMD_Mesh = new PreviewableHMD();
    private static Color _HMD_MeshDefaultColor = Color.ByARGB(255, 133, 191, 242);

    /// <summary>
    /// Tracking of HTC Vive Head Mounted Display (HMD).
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <param name="previewMesh">Render a preview mesh of the device? Will slow things down...</param>
    /// <param name="previewColor">Color to shade the preview mesh with.</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    [MultiReturn(new[] { "Mesh", "CoordinateSystem" })]
    public static Dictionary<string, object> HMD(object Vive,
        [DefaultArgumentAttribute("true")]bool tracked,
        [DefaultArgumentAttribute("true")]bool previewMesh,
        [DefaultArgumentAttribute("Color.ByARGB(255, 133, 191, 242)")]Color previewColor)
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
            using (CoordinateSystem cm = Util.Matrix4x4ToCoordinateSystem(_HMD_CurrentTrackedDevice.CorrectedMatrix4x4, false))
            {
                _HMD_CurrentCS = _HMD_CurrentCS.Transform(cm);
            }

            _HMD_OldCS = _HMD_CurrentCS;
            
            // Turns out, for some reason, nodes don't take Color as a DefaultArgumentAttribute... because it's a DSCore object?
            // So, left the "fake" declaration on the input for user feedback, but handle the incorrect population of the input with an internal default.
            if (previewColor == null)
            {
                _HMD_Mesh.MeshColor(_HMD_MeshDefaultColor.Red, _HMD_MeshDefaultColor.Green, _HMD_MeshDefaultColor.Blue, _HMD_MeshDefaultColor.Alpha);
            }
            else
            {
                _HMD_Mesh.MeshColor(previewColor.Red, previewColor.Green, previewColor.Blue, previewColor.Alpha);
            }
            _HMD_Mesh.Preview(previewMesh);
            _HMD_Mesh.Transform(_HMD_OldCS);
        }
        
        return new Dictionary<string, object>()
        {
            { "Mesh", _HMD_Mesh },
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
    private static PreviewableController _Controller1_Mesh = new PreviewableController();
    private static Color _Controller1_MeshDefaultColor = Color.ByARGB(255, 142, 242, 109);

    /// <summary>
    /// Tracking of HTC Vive Controller #1.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <param name="previewMesh">Render a preview mesh of the device? Will slow things down...</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    /// <returns name = "TriggerPressed">Is the trigger pressed?</returns>
    /// <returns name = "TriggerClicked">Is the trigger clicked (pressed all the way in)?</returns>
    /// <returns name = "TriggerValue">Trigger level from 0 (not pressed) to 1 (fully pressed).</returns>
    /// <returns name = "TouchPadTouched">Is the touchpad being touched?</returns>
    /// <returns name = "TouchPadClicked">Is the touchpad being clicked (pressed all the way in)?</returns>
    /// <returns name = "TouchPadValueX">Touch value from -1 (left) to 1 (right).</returns>
    /// <returns name = "TouchPadValueY">Touch value from -1 (bottom) to 1 (top).</returns>
    [MultiReturn(new[] { "Mesh", "CoordinateSystem", "TriggerPressed", "TriggerClicked", "TriggerValue", "TouchPadTouched", "TouchPadClicked", "TouchPadValueX", "TouchPadValueY" })]
    public static Dictionary<string, object> Controller1(object Vive,
        [DefaultArgumentAttribute("true")]bool tracked,
        [DefaultArgumentAttribute("true")]bool previewMesh,
        [DefaultArgumentAttribute("Color.ByARGB(255, 142, 242, 109)")]Color previewColor)
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
            using (CoordinateSystem cm = Util.Matrix4x4ToCoordinateSystem(_Controller1_CurrentTrackedDevice.CorrectedMatrix4x4, false))
            {
                _Controller1_CurrentCS = _Controller1_CurrentCS.Transform(cm);
            }

            _Controller1_OldCS = _Controller1_CurrentCS;

            if (previewColor == null)
            {
                _Controller1_Mesh.MeshColor(_Controller1_MeshDefaultColor.Red, _Controller1_MeshDefaultColor.Green, _Controller1_MeshDefaultColor.Blue, _Controller1_MeshDefaultColor.Alpha);
            }
            else
            {
                _Controller1_Mesh.MeshColor(previewColor.Red, previewColor.Green, previewColor.Blue, previewColor.Alpha);
            }
            _Controller1_Mesh.Preview(previewMesh);
            _Controller1_Mesh.Transform(_Controller1_OldCS);

            _Controller1_CurrentTrackedDevice.GetControllerTriggerState();
        }

        return new Dictionary<string, object>()
        {
            { "Mesh", _Controller1_Mesh },
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
    private static PreviewableController _Controller2_Mesh = new PreviewableController();
    private static Color _Controller2_MeshDefaultColor = Color.ByARGB(255, 142, 242, 109);

    /// <summary>
    /// Tracking of HTC Vive Controller #2.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <param name="previewMesh">Render a preview mesh of the device? Will slow things down...</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    /// <returns name = "TriggerPressed">Is the trigger pressed?</returns>
    /// <returns name = "TriggerClicked">Is the trigger clicked (pressed all the way in)?</returns>
    /// <returns name = "TriggerValue">Trigger level from 0 (not pressed) to 1 (fully pressed).</returns>
    /// <returns name = "TouchPadTouched">Is the touchpad being touched?</returns>
    /// <returns name = "TouchPadClicked">Is the touchpad being clicked (pressed all the way in)?</returns>
    /// <returns name = "TouchPadValueX">Touch value from -1 (left) to 1 (right).</returns>
    /// <returns name = "TouchPadValueY">Touch value from -1 (bottom) to 1 (top).</returns>
    [MultiReturn(new[] { "Mesh", "CoordinateSystem", "TriggerPressed", "TriggerClicked", "TriggerValue", "TouchPadTouched", "TouchPadClicked", "TouchPadValueX", "TouchPadValueY" })]
    public static Dictionary<string, object> Controller2(object Vive,
        [DefaultArgumentAttribute("true")]bool tracked,
        [DefaultArgumentAttribute("true")]bool previewMesh,
        [DefaultArgumentAttribute("Color.ByARGB(255, 142, 242, 109)")]Color previewColor)
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
            int index = wrapper.TrackedDevices.IndexesByClasses["Controller"][1];

            _Controller2_CurrentTrackedDevice = wrapper.TrackedDevices.AllDevices[index];
            _Controller2_CurrentTrackedDevice.ConvertPose();

            _Controller2_CurrentCS = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Util.Matrix4x4ToCoordinateSystem(_Controller2_CurrentTrackedDevice.CorrectedMatrix4x4, false))
            {
                _Controller2_CurrentCS = _Controller2_CurrentCS.Transform(cm);
            }

            _Controller2_OldCS = _Controller2_CurrentCS;

            if (previewColor == null)
            {
                _Controller2_Mesh.MeshColor(_Controller2_MeshDefaultColor.Red, _Controller2_MeshDefaultColor.Green, _Controller2_MeshDefaultColor.Blue, _Controller2_MeshDefaultColor.Alpha);
            }
            else
            {
                _Controller2_Mesh.MeshColor(previewColor.Red, previewColor.Green, previewColor.Blue, previewColor.Alpha);
            }
            _Controller2_Mesh.Preview(previewMesh);
            _Controller2_Mesh.Transform(_Controller2_OldCS);

            _Controller2_CurrentTrackedDevice.GetControllerTriggerState();
        }

        return new Dictionary<string, object>()
        {
            { "Mesh", _Controller2_Mesh },
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
    private static PreviewableLighthouse _Lighthouse1_Mesh = new PreviewableLighthouse();
    private static Color _Lighthouse1_MeshDefaultColor = Color.ByARGB(255, 242, 181, 232);

    /// <summary>
    /// Tracking of HTC Vive Lighthouse #1.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <param name="previewMesh">Render a preview mesh of the device? Will slow things down...</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    [MultiReturn(new[] { "Mesh", "CoordinateSystem" })]
    public static Dictionary<string, object> Lighthouse1(object Vive,
        [DefaultArgumentAttribute("true")]bool tracked,
        [DefaultArgumentAttribute("true")]bool previewMesh,
        [DefaultArgumentAttribute("Color.ByARGB(255, 242, 181, 232)")]Color previewColor)
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
            using (CoordinateSystem cm = Util.Matrix4x4ToCoordinateSystem(_Lighthouse1_CurrentTrackedDevice.CorrectedMatrix4x4, false))
            {
                _Lighthouse1_CurrentCS = _Lighthouse1_CurrentCS.Transform(cm);
            }

            _Lighthouse1_OldCS = _Lighthouse1_CurrentCS;

            if (previewColor == null)
            {
                _Lighthouse1_Mesh.MeshColor(_Lighthouse1_MeshDefaultColor.Red, _Lighthouse1_MeshDefaultColor.Green, _Lighthouse1_MeshDefaultColor.Blue, _Lighthouse1_MeshDefaultColor.Alpha);
            }
            else
            {
                _Lighthouse1_Mesh.MeshColor(previewColor.Red, previewColor.Green, previewColor.Blue, previewColor.Alpha);
            }
            _Lighthouse1_Mesh.Preview(previewMesh);
            _Lighthouse1_Mesh.Transform(_Lighthouse1_OldCS);

        }


        return new Dictionary<string, object>()
        {
            { "Mesh", _Lighthouse1_Mesh },
            { "CoordinateSystem", _Lighthouse1_OldCS }
        };
    }



    private static VrTrackedDevice _Lighthouse2_CurrentTrackedDevice;
    private static CoordinateSystem _Lighthouse2_CurrentCS;
    private static CoordinateSystem _Lighthouse2_OldCS;
    private static PreviewableLighthouse _Lighthouse2_Mesh = new PreviewableLighthouse();
    private static Color _Lighthouse2_MeshDefaultColor = Color.ByARGB(255, 242, 181, 232);

    /// <summary>
    /// Tracking of HTC Vive Lighthouse #2.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <param name="previewMesh">Render a preview mesh of the device? Will slow things down...</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    [MultiReturn(new[] { "Mesh", "CoordinateSystem" })]
    public static Dictionary<string, object> Lighthouse2(object Vive,
        [DefaultArgumentAttribute("true")]bool tracked,
        [DefaultArgumentAttribute("true")]bool previewMesh,
        [DefaultArgumentAttribute("Color.ByARGB(255, 242, 181, 232)")]Color previewColor)
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
            int index = wrapper.TrackedDevices.IndexesByClasses["Lighthouse"][1];

            _Lighthouse2_CurrentTrackedDevice = wrapper.TrackedDevices.AllDevices[index];
            _Lighthouse2_CurrentTrackedDevice.ConvertPose();

            _Lighthouse2_CurrentCS = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Util.Matrix4x4ToCoordinateSystem(_Lighthouse2_CurrentTrackedDevice.CorrectedMatrix4x4, false))
            {
                _Lighthouse2_CurrentCS = _Lighthouse2_CurrentCS.Transform(cm);
            }

            _Lighthouse2_OldCS = _Lighthouse2_CurrentCS;

            if (previewColor == null)
            {
                _Lighthouse2_Mesh.MeshColor(_Lighthouse2_MeshDefaultColor.Red, _Lighthouse2_MeshDefaultColor.Green, _Lighthouse2_MeshDefaultColor.Blue, _Lighthouse2_MeshDefaultColor.Alpha);
            }
            else
            {
                _Lighthouse2_Mesh.MeshColor(previewColor.Red, previewColor.Green, previewColor.Blue, previewColor.Alpha);
            }
            _Lighthouse2_Mesh.Preview(previewMesh);
            _Lighthouse2_Mesh.Transform(_Lighthouse2_OldCS);

        }


        return new Dictionary<string, object>()
        {
            { "Mesh", _Lighthouse2_Mesh },
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
    private static PreviewableGenericTracker[] _GenericTracker_Mesh = new PreviewableGenericTracker[8];
    private static Color _GenericTracker_MeshDefaultColor = Color.ByARGB(255, 244, 149, 66);

    /// <summary>
    /// Tracking of HTC Vive Generic Tracker.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="index">If more than one Tracker, choose index number.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <param name="previewMesh">Render a preview mesh of the device? Will slow things down...</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    [MultiReturn(new[] { "Mesh", "CoordinateSystem" })]
    public static Dictionary<string, object> GenericTracker(object Vive,
        [DefaultArgumentAttribute("0")]int index,
        [DefaultArgumentAttribute("true")]bool tracked,
        [DefaultArgumentAttribute("true")]bool previewMesh,
        [DefaultArgumentAttribute("Color.ByARGB(255, 244, 149, 66)")]Color previewColor)
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
            using (CoordinateSystem cm = Util.Matrix4x4ToCoordinateSystem(_GenericTracker_CurrentTrackedDevice[index].CorrectedMatrix4x4, false))
            {
                _GenericTracker_CurrentCS[index] = _GenericTracker_CurrentCS[index].Transform(cm);
            }

            _GenericTracker_OldCS[index] = _GenericTracker_CurrentCS[index];

            if (_GenericTracker_Mesh[index] == null)
            {
                _GenericTracker_Mesh[index] = new PreviewableGenericTracker();
            }

            if (previewColor == null)
            {
                _GenericTracker_Mesh[index].MeshColor(_GenericTracker_MeshDefaultColor.Red, _GenericTracker_MeshDefaultColor.Green, _GenericTracker_MeshDefaultColor.Blue, _GenericTracker_MeshDefaultColor.Alpha);
            }
            else
            {
                _GenericTracker_Mesh[index].MeshColor(previewColor.Red, previewColor.Green, previewColor.Blue, previewColor.Alpha);
            }
            _GenericTracker_Mesh[index].Preview(previewMesh);
            _GenericTracker_Mesh[index].Transform(_GenericTracker_OldCS[index]);

        }

        return new Dictionary<string, object>()
        {
            { "Mesh", _GenericTracker_Mesh[index] },
            { "CoordinateSystem", _GenericTracker_OldCS[index] }
        };
    }


}
