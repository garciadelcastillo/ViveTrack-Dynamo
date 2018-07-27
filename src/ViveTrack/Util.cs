using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Numerics;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

using DSPlane = Autodesk.DesignScript.Geometry.Plane;
using System.Collections.Generic;


//  ██╗   ██╗████████╗██╗██╗     ███████╗
//  ██║   ██║╚══██╔══╝██║██║     ██╔════╝
//  ██║   ██║   ██║   ██║██║     ███████╗
//  ██║   ██║   ██║   ██║██║     ╚════██║
//  ╚██████╔╝   ██║   ██║███████╗███████║
//   ╚═════╝    ╚═╝   ╚═╝╚══════╝╚══════╝
//                                       

static class Util
{
    
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
        double[] pl = DSPlaneToDoubleArray(plane, false);
        Matrix4x4 m = new Matrix4x4(
            (float)pl[0], (float)pl[1], (float)pl[2], (float)pl[3],
            (float)pl[4], (float)pl[5], (float)pl[6], (float)pl[7],
            (float)pl[8], (float)pl[9], (float)pl[10], (float)pl[11],
            (float)pl[12], (float)pl[13], (float)pl[14], (float)pl[15]);
        return m;
    }

    internal static Matrix4x4 CoordinateSystemToMatrix4x4(CoordinateSystem cs)
    {
        Matrix4x4 m = new Matrix4x4(
            (float)cs.XAxis.X, (float)cs.XAxis.Y, (float)cs.XAxis.Z, 0f,
            (float)cs.YAxis.X, (float)cs.YAxis.Y, (float)cs.YAxis.Z, 0f,
            (float)cs.ZAxis.X, (float)cs.ZAxis.Y, (float)cs.ZAxis.Z, 0f,
            (float)cs.Origin.X, (float)cs.Origin.Y, (float)cs.Origin.Z, 0f);
        return m;
    }

    internal static double[] Matrix4x4ToDoubleArray(Matrix4x4 m, bool transpose)
    {
        double[] a;
        if (transpose)
        {
            a = new double[] {
                m.M11, m.M21, m.M31, m.M41,
                m.M12, m.M22, m.M32, m.M42,
                m.M13, m.M23, m.M33, m.M43,
                m.M14, m.M24, m.M34, m.M44
            };
        }
        else
        {
            a = new double[] {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            };
        }
        return a;
    }

    internal static double[] DSPlaneToDoubleArray(DSPlane pl, bool transpose)
    {
        double[] a;
        if (transpose)
        {
            a = new double[]
            {
               pl.XAxis.X, pl.XAxis.Y, pl.XAxis.Z, 0,
               pl.YAxis.X, pl.YAxis.Y, pl.YAxis.Z, 0,
               pl.Normal.X, pl.Normal.Y, pl.Normal.Z, 0,
               pl.Origin.X, pl.Origin.Y, pl.Origin.Z, 1
            };
        }
        else
        {
            a = new double[]
            {
                pl.XAxis.X, pl.YAxis.X, pl.Normal.X, pl.Origin.X,
                pl.XAxis.Y, pl.YAxis.Y, pl.Normal.Y, pl.Origin.Y,
                pl.XAxis.Z, pl.YAxis.Z, pl.Normal.Z, pl.Origin.Z,
                0, 0, 0, 1
            };
        }
        return a;
    }
}



