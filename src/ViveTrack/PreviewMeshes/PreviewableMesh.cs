using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;


abstract class PreviewableMesh : IGraphicItem
{
    internal abstract float[] Vertices { get; }
    internal abstract float[] Normals { get; }
    internal abstract uint[] Faces { get; }

    private float[] verticesTrans;
    private float[] normalTrans;
    private bool _preview = true;
    private Matrix4x4 _transform;
    private byte _red, _green, _blue, _alpha;

    internal PreviewableMesh()
    {
        verticesTrans = new float[Vertices.Length];
        normalTrans = new float[Normals.Length];
    }

    private void PushTriangleVertex(IRenderPackage package, Point p, Vector n)
    {
        package.AddTriangleVertex(p.X, p.Y, p.Z);
        package.AddTriangleVertexColor(255, 255, 255, 255);
        package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
        package.AddTriangleVertexUV(0, 0);
    }

    internal void Transform(CoordinateSystem cs)
    {
        if (!_preview) return;

        _transform = Util.CoordinateSystemToMatrix4x4(cs);

        Vector3 v = new Vector3();
        Vector3 n = new Vector3();

        for (int i = 0; i < Vertices.Length; i += 3)
        {
            v.X = Vertices[i];
            v.Y = Vertices[i + 1];
            v.Z = Vertices[i + 2];

            n.X = Normals[i];
            n.Y = Normals[i + 1];
            n.Z = Normals[i + 2];

            v = Vector3.Transform(v, _transform);
            n = Vector3.Transform(n, _transform);

            verticesTrans[i] = v.X;
            verticesTrans[i + 1] = v.Y;
            verticesTrans[i + 2] = v.Z;

            normalTrans[i] = n.X;
            normalTrans[i + 1] = n.Y;
            normalTrans[i + 2] = n.Z;
        }
    }

    internal void Preview(bool preview)
    {
        _preview = preview;
    }

    internal void MeshColor(byte r, byte g, byte b, byte a)
    {
        _red = r;
        _green = g;
        _blue = b;
        _alpha = a;
    }

    void IGraphicItem.Tessellate(IRenderPackage package, TessellationParameters parameters)
    {
        if (!_preview) return;

        uint fid;
        for (int i = 0; i < Faces.Length; i++)
        {
            fid = 3 * Faces[i];
            package.AddTriangleVertex(verticesTrans[fid], verticesTrans[fid + 1], verticesTrans[fid + 2]);
            package.AddTriangleVertexColor(_red, _green, _blue, _alpha);
            package.AddTriangleVertexNormal(normalTrans[fid], normalTrans[fid + 1], normalTrans[fid + 2]);
            package.AddTriangleVertexUV(0, 0);
        }
    }
}