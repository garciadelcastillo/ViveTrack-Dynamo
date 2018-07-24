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

    // Due to Zero Touch nature, these instances will be shared by all nodes... Not ideal, but good enough for 99% of situations.
    private static VrTrackedDevice _HMD_CurrentTrackedDevice;
    private static CoordinateSystem _HMD_CurrentCS;
    private static CoordinateSystem _HMD_OldCS;
    private static Autodesk.DesignScript.Geometry.Plane _HMD_OldPlane;


    [MultiReturn(new[] { "Mesh", "Plane", "CoordinateSystem", "m44" })]
    public static Dictionary<string, object> HMD(object Vive, bool read = true)
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


        if (read)
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



        //XyPlane = Plane.WorldXY;
        //XyPlane.Transform(CurrenTrackedDevice.CorrectedMatrix4X4);
        //if (!Paused)
        //{
        //    OldPlane = XyPlane;
        //    OldTransform = CurrenTrackedDevice.CorrectedMatrix4X4;
        //}

        //var newHMD = HMD.Duplicate();
        //newHMD.Transform(OldTransform);
        //DA.SetData("HMD", newHMD);
        //DA.SetData("Plane", OldPlane);
        //DA.SetData("Matrix", OldTransform);

        return new Dictionary<string, object>()
        {
            { "HMD", null },
            { "Plane", _HMD_OldPlane },
            { "CoordinateSystem", _HMD_OldCS },
            { "m44", _HMD_CurrentTrackedDevice.CorrectedMatrix4x4 }
        };
    }


    public static Matrix4x4 trans4x4(double x = 5, double y = 10, double z = 15)
    {
        Matrix4x4 mm = Matrix4x4.CreateTranslation((float)x, (float)y, (float)z);
        return mm;
    }

    public static Matrix4x4 AxisVector(Vector axis, double angle)
    {
        Vector3 v = new Vector3((float)axis.X, (float)axis.Y, (float)axis.Z);
        Matrix4x4 mm = Matrix4x4.CreateFromAxisAngle(v, (float)angle);
        return mm;
    }

    public static DSPlane CoordinateSystemToPlane(CoordinateSystem cs)
    {
        DSPlane pl = DSPlane.ByOriginXAxisYAxis(cs.Origin, cs.XAxis, cs.YAxis);
        return pl;
    }

    public static CoordinateSystem Matrix4x4ToCoordinateSystem(Matrix4x4 m, bool transpose)
    {
        double[] mm = Matrix4x4ToDoubleArray(m, transpose);
        CoordinateSystem cs = CoordinateSystem.ByMatrix(mm);
        return cs;
    }

    public static double[] Matrix4x4ToDoubleArray(Matrix4x4 m, bool transpose)
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
}
