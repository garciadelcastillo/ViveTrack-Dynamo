using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Numerics;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

using DSPlane = Autodesk.DesignScript.Geometry.Plane;


//  ██╗   ██╗████████╗██╗██╗     ███████╗
//  ██║   ██║╚══██╔══╝██║██║     ██╔════╝
//  ██║   ██║   ██║   ██║██║     ███████╗
//  ██║   ██║   ██║   ██║██║     ╚════██║
//  ╚██████╔╝   ██║   ██║███████╗███████║
//   ╚═════╝    ╚═╝   ╚═╝╚══════╝╚══════╝
//                                       

class Util
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

