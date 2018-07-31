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
    private static VrTrackedDevice[] _HMD_CurrentTrackedDevice = new VrTrackedDevice[8];
    private static CoordinateSystem[] _HMD_CurrentCS = new CoordinateSystem[8];
    private static CoordinateSystem[] _HMD_OldCS = new CoordinateSystem[8];
    private static PreviewableHMD[] _HMD_Mesh = new PreviewableHMD[8];
    private static Color _HMD_MeshDefaultColor = Color.ByARGB(255, 133, 191, 242);

    /// <summary>
    /// Tracking of HTC Vive Head Mounted Display (HMD).
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="index">If more than one Tracker, choose index number.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <param name="previewMesh">Render a preview mesh of the device? Will slow things down...</param>
    /// <param name="previewColor">Color to shade the preview mesh with.</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    [MultiReturn(new[] { "Mesh", "CoordinateSystem" })]
    public static Dictionary<string, object> HMD(object Vive,
        [DefaultArgumentAttribute("0")]int index,
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
        if (list.Count < index + 1)
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("No HMD detected.");
            return null;
        }        

        if (tracked)
        {
            int id = wrapper.TrackedDevices.IndexesByClasses["HMD"][index];

            _HMD_CurrentTrackedDevice[index] = wrapper.TrackedDevices.AllDevices[id];
            _HMD_CurrentTrackedDevice[index].ConvertPose();

            _HMD_CurrentCS[index] = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Util.Matrix4x4ToCoordinateSystem(_HMD_CurrentTrackedDevice[index].CorrectedMatrix4x4, false))
            {
                _HMD_CurrentCS[index] = _HMD_CurrentCS[index].Transform(cm);
            }

            _HMD_OldCS[index] = _HMD_CurrentCS[index];
            
        }

        if (previewMesh)
        {
            if (_HMD_Mesh[index] == null)
            {
                _HMD_Mesh[index] = new PreviewableHMD();
            }

            // Turns out, for some reason, nodes don't take Color as a DefaultArgumentAttribute... because it's a DSCore object?
            // So, left the "fake" declaration on the input for user feedback, but handle the incorrect population of the input with an internal default.
            if (previewColor == null)
            {
                _HMD_Mesh[index].MeshColor(_HMD_MeshDefaultColor.Red, _HMD_MeshDefaultColor.Green, _HMD_MeshDefaultColor.Blue, _HMD_MeshDefaultColor.Alpha);
            }
            else
            {
                _HMD_Mesh[index].MeshColor(previewColor.Red, previewColor.Green, previewColor.Blue, previewColor.Alpha);
            }

            if (tracked)
            {
                _HMD_Mesh[index].Transform(_HMD_OldCS[index]);
            }

            _HMD_Mesh[index].Preview(true);
        }
        else if (_HMD_Mesh[index] != null)
        {
            _HMD_Mesh[index].Preview(false);
        }

        return new Dictionary<string, object>()
        {
            { "Mesh", _HMD_Mesh[index] },
            { "CoordinateSystem", _HMD_OldCS[index] }
        };
    }




    //   ██████╗ ██████╗ ███╗   ██╗████████╗██████╗  ██████╗ ██╗     ██╗     ███████╗██████╗ 
    //  ██╔════╝██╔═══██╗████╗  ██║╚══██╔══╝██╔══██╗██╔═══██╗██║     ██║     ██╔════╝██╔══██╗
    //  ██║     ██║   ██║██╔██╗ ██║   ██║   ██████╔╝██║   ██║██║     ██║     █████╗  ██████╔╝
    //  ██║     ██║   ██║██║╚██╗██║   ██║   ██╔══██╗██║   ██║██║     ██║     ██╔══╝  ██╔══██╗
    //  ╚██████╗╚██████╔╝██║ ╚████║   ██║   ██║  ██║╚██████╔╝███████╗███████╗███████╗██║  ██║
    //   ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝

    private static VrTrackedDevice[] _Controller_CurrentTrackedDevice = new VrTrackedDevice[8];
    private static CoordinateSystem[] _Controller_CurrentCS = new CoordinateSystem[8];
    private static CoordinateSystem[] _Controller_OldCS = new CoordinateSystem[8];
    private static PreviewableController[] _Controller_Mesh = new PreviewableController[8];
    private static Color _Controller_MeshDefaultColor = Color.ByARGB(255, 142, 242, 109);

    /// <summary>
    /// Tracking of HTC Vive Controllers.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="index">If more than one Tracker, choose index number.</param>
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
    public static Dictionary<string, object> Controller(object Vive,
        [DefaultArgumentAttribute("0")]int index,
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
        if (list.Count < index + 1)
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("No Controller detected.");
            return null;
        }

        
        if (tracked)
        {
            int id = wrapper.TrackedDevices.IndexesByClasses["Controller"][index];

            _Controller_CurrentTrackedDevice[index] = wrapper.TrackedDevices.AllDevices[id];
            _Controller_CurrentTrackedDevice[index].ConvertPose();

            _Controller_CurrentCS[index] = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Util.Matrix4x4ToCoordinateSystem(_Controller_CurrentTrackedDevice[index].CorrectedMatrix4x4, false))
            {
                _Controller_CurrentCS[index] = _Controller_CurrentCS[index].Transform(cm);
            }

            _Controller_OldCS[index] = _Controller_CurrentCS[index];
            
            _Controller_CurrentTrackedDevice[index].GetControllerTriggerState();
        }

        
        if (previewMesh)
        {
            if (_Controller_Mesh[index] == null)
            {
                _Controller_Mesh[index] = new PreviewableController();
            }

            if (previewColor == null)
            {
                _Controller_Mesh[index].MeshColor(_Controller_MeshDefaultColor.Red, _Controller_MeshDefaultColor.Green, _Controller_MeshDefaultColor.Blue, _Controller_MeshDefaultColor.Alpha);
            }
            else
            {
                _Controller_Mesh[index].MeshColor(previewColor.Red, previewColor.Green, previewColor.Blue, previewColor.Alpha);
            }

            if (tracked)
            {
                _Controller_Mesh[index].Transform(_Controller_OldCS[index]);
            }

            _Controller_Mesh[index].Preview(true);
        }
        else if (_Controller_Mesh[index] != null)
        {
            _Controller_Mesh[index].Preview(false);
        }
        

        return new Dictionary<string, object>()
        {
            { "Mesh", _HMD_Mesh[index] },
            { "CoordinateSystem", _Controller_OldCS[index] },
            { "TriggerPressed", _Controller_CurrentTrackedDevice[index].TriggerPressed },
            { "TriggerClicked", _Controller_CurrentTrackedDevice[index].TriggerClicked },
            { "TriggerValue", _Controller_CurrentTrackedDevice[index].TriggerValue },
            { "TouchPadTouched", _Controller_CurrentTrackedDevice[index].TouchPadTouched },
            { "TouchPadClicked", _Controller_CurrentTrackedDevice[index].TouchPadClicked },
            { "TouchPadValueX", _Controller_CurrentTrackedDevice[index].TouchPadValueX },
            { "TouchPadValueY", _Controller_CurrentTrackedDevice[index].TouchPadValueY }
        };
    }









    //  ██╗     ██╗ ██████╗ ██╗  ██╗████████╗██╗  ██╗ ██████╗ ██╗   ██╗███████╗███████╗
    //  ██║     ██║██╔════╝ ██║  ██║╚══██╔══╝██║  ██║██╔═══██╗██║   ██║██╔════╝██╔════╝
    //  ██║     ██║██║  ███╗███████║   ██║   ███████║██║   ██║██║   ██║███████╗█████╗  
    //  ██║     ██║██║   ██║██╔══██║   ██║   ██╔══██║██║   ██║██║   ██║╚════██║██╔══╝  
    //  ███████╗██║╚██████╔╝██║  ██║   ██║   ██║  ██║╚██████╔╝╚██████╔╝███████║███████╗
    //  ╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝  ╚═════╝ ╚══════╝╚══════╝

    private static VrTrackedDevice[] _Lighthouse_CurrentTrackedDevice = new VrTrackedDevice[8];
    private static CoordinateSystem[] _Lighthouse_CurrentCS = new CoordinateSystem[8];
    private static CoordinateSystem[] _Lighthouse_OldCS = new CoordinateSystem[8];
    private static PreviewableLighthouse[] _Lighthouse_Mesh = new PreviewableLighthouse[8];
    private static Color _Lighthouse_MeshDefaultColor = Color.ByARGB(255, 242, 181, 232);

    /// <summary>
    /// Tracking of HTC Vive Lighthouses.
    /// </summary>
    /// <param name="Vive">The Vive object to read from.</param>
    /// <param name="index">If more than one Tracker, choose index number.</param>
    /// <param name="tracked">Should the device be tracked?</param>
    /// <param name="previewMesh">Render a preview mesh of the device? Will slow things down...</param>
    /// <returns name = "Mesh">Mesh representation of the device.</returns>
    /// <returns name = "CoordinateSystem">The device's CoordinateSystem.</returns>
    [MultiReturn(new[] { "Mesh", "CoordinateSystem" })]
    public static Dictionary<string, object> Lighthouse(object Vive,
        [DefaultArgumentAttribute("0")]int index,
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
        if (list.Count < index + 1)
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage("No Lighthouse detected.");
            return null;
        }

        if (tracked)
        {
            int id = wrapper.TrackedDevices.IndexesByClasses["Lighthouse"][index];

            _Lighthouse_CurrentTrackedDevice[index] = wrapper.TrackedDevices.AllDevices[id];
            _Lighthouse_CurrentTrackedDevice[index].ConvertPose();

            _Lighthouse_CurrentCS[index] = CoordinateSystem.Identity();
            using (CoordinateSystem cm = Util.Matrix4x4ToCoordinateSystem(_Lighthouse_CurrentTrackedDevice[index].CorrectedMatrix4x4, false))
            {
                _Lighthouse_CurrentCS[index] = _Lighthouse_CurrentCS[index].Transform(cm);
            }

            _Lighthouse_OldCS[index] = _Lighthouse_CurrentCS[index];

        }

        if (previewMesh)
        {
            if (_Lighthouse_Mesh[index] == null)
            {
                _Lighthouse_Mesh[index] = new PreviewableLighthouse();
            }

            if (previewColor == null)
            {
                _Lighthouse_Mesh[index].MeshColor(_Lighthouse_MeshDefaultColor.Red, _Lighthouse_MeshDefaultColor.Green, _Lighthouse_MeshDefaultColor.Blue, _Lighthouse_MeshDefaultColor.Alpha);
            }
            else
            {
                _Lighthouse_Mesh[index].MeshColor(previewColor.Red, previewColor.Green, previewColor.Blue, previewColor.Alpha);
            }

            if (tracked)
            {
                _Lighthouse_Mesh[index].Transform(_Lighthouse_OldCS[index]);
            }

            _Lighthouse_Mesh[index].Preview(true);
        }
        else if (_Lighthouse_Mesh[index] != null)
        {
            _Lighthouse_Mesh[index].Preview(false);
        }

        return new Dictionary<string, object>()
        {
            { "Mesh", _Lighthouse_Mesh[index] },
            { "CoordinateSystem", _Lighthouse_OldCS[index] }
        };
    }





    //   ██████╗ ███████╗███╗   ██╗███████╗██████╗ ██╗ ██████╗████████╗██████╗  █████╗  ██████╗██╗  ██╗███████╗██████╗ 
    //  ██╔════╝ ██╔════╝████╗  ██║██╔════╝██╔══██╗██║██╔════╝╚══██╔══╝██╔══██╗██╔══██╗██╔════╝██║ ██╔╝██╔════╝██╔══██╗
    //  ██║  ███╗█████╗  ██╔██╗ ██║█████╗  ██████╔╝██║██║        ██║   ██████╔╝███████║██║     █████╔╝ █████╗  ██████╔╝
    //  ██║   ██║██╔══╝  ██║╚██╗██║██╔══╝  ██╔══██╗██║██║        ██║   ██╔══██╗██╔══██║██║     ██╔═██╗ ██╔══╝  ██╔══██╗
    //  ╚██████╔╝███████╗██║ ╚████║███████╗██║  ██║██║╚██████╗   ██║   ██║  ██║██║  ██║╚██████╗██║  ██╗███████╗██║  ██║
    //   ╚═════╝ ╚══════╝╚═╝  ╚═══╝╚══════╝╚═╝  ╚═╝╚═╝ ╚═════╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝

    private static VrTrackedDevice[] _GenericTracker_CurrentTrackedDevice = new VrTrackedDevice[8];
    private static CoordinateSystem[] _GenericTracker_CurrentCS = new CoordinateSystem[8];
    private static CoordinateSystem[] _GenericTracker_OldCS = new CoordinateSystem[8];
    private static PreviewableGenericTracker[] _GenericTracker_Mesh = new PreviewableGenericTracker[8];
    private static Color _GenericTracker_MeshDefaultColor = Color.ByARGB(255, 244, 149, 66);

    /// <summary>
    /// Tracking of HTC Vive Generic Trackers.
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

        }


        if (previewMesh)
        {
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

            if (tracked)
            {
                _GenericTracker_Mesh[index].Transform(_GenericTracker_OldCS[index]);
            }

            _GenericTracker_Mesh[index].Preview(true);
        }
        else if (_GenericTracker_Mesh[index] != null)
        {
            _GenericTracker_Mesh[index].Preview(false);
        }

        return new Dictionary<string, object>()
        {
            { "Mesh", _GenericTracker_Mesh[index] },
            { "CoordinateSystem", _GenericTracker_OldCS[index] }
        };
    }


}
