using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using IndoorSceneEditor.Models;

namespace IndoorSceneEditor.Services
{
    /// <summary>
    /// 为每种室内物品类型生成 3D 网格。
    /// 使用 WPF 内置 Media3D，通过组合立方体构建家具的简化造型。
    /// </summary>
    public static class MeshFactory
    {
        public static MeshGeometry3D Create(ItemType type)
        {
            var b = new MeshBuilder();
            switch (type)
            {
                case ItemType.Sofa:    BuildSofa(b); break;
                case ItemType.Table:   BuildTable(b); break;
                case ItemType.Chair:   BuildChair(b); break;
                case ItemType.Bed:     BuildBed(b); break;
                case ItemType.Cabinet: BuildCabinet(b); break;
                case ItemType.Lamp:    BuildLamp(b); break;
                case ItemType.Plant:   BuildPlant(b); break;
                case ItemType.TV:      BuildTV(b); break;
                case ItemType.Shelf:   BuildShelf(b); break;
                case ItemType.Rug:     BuildRug(b); break;
            }
            return b.ToMesh();
        }

        // 每个单位 = 0.5 米，使尺寸接近真实家具比例
        private const double U = 1.0;

        private static void BuildSofa(MeshBuilder b)
        {
            double w = 3.0 * U, d = 1.2 * U, seatH = 0.5 * U, backH = 0.9 * U, armH = 0.7 * U;
            b.Box(0, seatH / 2, 0, w, seatH, d);                                  // 底座
            b.Box(0, seatH + backH / 2, -d / 2 + 0.15, w, backH, 0.3);           // 靠背
            b.Box(-w / 2 + 0.15, seatH + armH / 2, 0, 0.3, armH, d);             // 左扶手
            b.Box(w / 2 - 0.15, seatH + armH / 2, 0, 0.3, armH, d);              // 右扶手
            b.Box(0, seatH + 0.1, 0.1, w - 0.6, 0.2, d - 0.4);                   // 坐垫
        }

        private static void BuildTable(MeshBuilder b)
        {
            double w = 1.6 * U, d = 0.9 * U, top = 0.08, h = 0.75 * U;
            b.Box(0, h, 0, w, top, d);
            double lx = w / 2 - 0.08, lz = d / 2 - 0.08, leg = 0.08;
            b.Box(-lx, h / 2, -lz, leg, h, leg);
            b.Box(lx, h / 2, -lz, leg, h, leg);
            b.Box(-lx, h / 2, lz, leg, h, leg);
            b.Box(lx, h / 2, lz, leg, h, leg);
        }

        private static void BuildChair(MeshBuilder b)
        {
            double w = 0.5 * U, d = 0.5 * U, seatH = 0.45 * U, backH = 0.5 * U, leg = 0.06;
            b.Box(0, seatH, 0, w, 0.06, d);
            b.Box(0, seatH + backH / 2, -d / 2 + 0.06, w, backH, 0.06);
            double lx = w / 2 - 0.05, lz = d / 2 - 0.05;
            b.Box(-lx, seatH / 2, -lz, leg, seatH, leg);
            b.Box(lx, seatH / 2, -lz, leg, seatH, leg);
            b.Box(-lx, seatH / 2, lz, leg, seatH, leg);
            b.Box(lx, seatH / 2, lz, leg, seatH, leg);
        }

        private static void BuildBed(MeshBuilder b)
        {
            double w = 2.0 * U, d = 2.2 * U, frameH = 0.25 * U, mattH = 0.25 * U;
            b.Box(0, frameH / 2, 0, w, frameH, d);
            b.Box(0, frameH + mattH / 2, 0, w - 0.1, mattH, d - 0.1);
            b.Box(-0.4, frameH + mattH + 0.08, -d / 2 + 0.3, 0.5, 0.16, 0.35);
            b.Box(0.4, frameH + mattH + 0.08, -d / 2 + 0.3, 0.5, 0.16, 0.35);
            b.Box(0, 0.7, -d / 2 + 0.05, w, 1.0, 0.1);
        }

        private static void BuildCabinet(MeshBuilder b)
        {
            double w = 1.2 * U, d = 0.5 * U, h = 1.8 * U;
            b.Box(0, h / 2, 0, w, h, d);
            for (int i = 1; i < 4; i++)
            {
                double y = i * h / 4;
                b.Box(0, y, d / 2 - 0.02, w - 0.05, 0.02, 0.02);
            }
        }

        private static void BuildLamp(MeshBuilder b)
        {
            double baseR = 0.25 * U, poleH = 1.5 * U, shadeH = 0.3 * U, shadeR = 0.4 * U;
            b.Box(0, 0.03, 0, baseR * 2, 0.06, baseR * 2);
            b.Box(0, poleH / 2, 0, 0.06, poleH, 0.06);
            b.Box(0, poleH + shadeH * 0.33, 0, shadeR * 2 * 0.7, shadeH * 0.66, shadeR * 2 * 0.7);
            b.Box(0, poleH + shadeH * 0.8, 0, shadeR * 2, shadeH * 0.4, shadeR * 2);
        }

        private static void BuildPlant(MeshBuilder b)
        {
            double potR = 0.2 * U, potH = 0.3 * U;
            b.Box(0, potH / 2, 0, potR * 2, potH, potR * 2);
            double cy = potH;
            b.Box(0, cy + 0.2, 0, 0.5, 0.4, 0.5);
            b.Box(0, cy + 0.5, 0, 0.7, 0.3, 0.7);
            b.Box(0, cy + 0.75, 0, 0.45, 0.25, 0.45);
        }

        private static void BuildTV(MeshBuilder b)
        {
            double w = 1.4 * U, h = 0.8 * U, thick = 0.08;
            b.Box(0, h / 2 + 0.3, 0, w, h, thick);
            b.Box(0, 0.15, 0, 0.5, 0.3, 0.3);
            b.Box(0, 0.35, 0, 0.08, 0.3, 0.08);
        }

        private static void BuildShelf(MeshBuilder b)
        {
            double w = 1.0 * U, d = 0.35 * U, h = 1.8 * U, t = 0.05;
            b.Box(-w / 2 + t / 2, h / 2, 0, t, h, d);
            b.Box(w / 2 - t / 2, h / 2, 0, t, h, d);
            b.Box(0, h / 2, -d / 2 + t / 2, w, h, t);
            for (int i = 0; i <= 4; i++)
            {
                double y = i * h / 4;
                b.Box(0, y + t / 2, 0, w, t, d);
            }
        }

        private static void BuildRug(MeshBuilder b)
        {
            b.Box(0, 0.01, 0, 2.4 * U, 0.02, 1.6 * U);
        }
    }

    /// <summary>
    /// 简单的网格构建器，支持添加轴对齐立方体。
    /// 每个面使用独立顶点，以保证法线正确（平面着色）。
    /// </summary>
    internal class MeshBuilder
    {
        private readonly List<Point3D> _positions = new();
        private readonly List<Vector3D> _normals = new();
        private readonly List<Point> _texCoords = new();
        private readonly List<int> _indices = new();

        public void Box(double cx, double cy, double cz, double sx, double sy, double sz)
        {
            double hx = sx / 2, hy = sy / 2, hz = sz / 2;
            var p = new[]
            {
                new Point3D(cx - hx, cy - hy, cz - hz), // 0
                new Point3D(cx + hx, cy - hy, cz - hz), // 1
                new Point3D(cx + hx, cy + hy, cz - hz), // 2
                new Point3D(cx - hx, cy + hy, cz - hz), // 3
                new Point3D(cx - hx, cy - hy, cz + hz), // 4
                new Point3D(cx + hx, cy - hy, cz + hz), // 5
                new Point3D(cx + hx, cy + hy, cz + hz), // 6
                new Point3D(cx - hx, cy + hy, cz + hz), // 7
            };

            // 6 个面：back, front, left, right, top, bottom
            Face(p[0], p[1], p[2], p[3], new Vector3D(0, 0, -1));
            Face(p[5], p[4], p[7], p[6], new Vector3D(0, 0, 1));
            Face(p[4], p[0], p[3], p[7], new Vector3D(-1, 0, 0));
            Face(p[1], p[5], p[6], p[2], new Vector3D(1, 0, 0));
            Face(p[3], p[2], p[6], p[7], new Vector3D(0, 1, 0));
            Face(p[4], p[5], p[1], p[0], new Vector3D(0, -1, 0));
        }

        private void Face(Point3D a, Point3D b, Point3D c, Point3D d, Vector3D n)
        {
            int start = _positions.Count;
            _positions.Add(a); _positions.Add(b); _positions.Add(c); _positions.Add(d);
            for (int i = 0; i < 4; i++)
            {
                _normals.Add(n);
                _texCoords.Add(new Point(0, 0));
            }
            _indices.Add(start);
            _indices.Add(start + 1);
            _indices.Add(start + 2);
            _indices.Add(start);
            _indices.Add(start + 2);
            _indices.Add(start + 3);
        }

        public MeshGeometry3D ToMesh()
        {
            var mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection(_positions),
                Normals = new Vector3DCollection(_normals),
                TextureCoordinates = new PointCollection(_texCoords),
                TriangleIndices = new Int32Collection(_indices)
            };
            mesh.Freeze();
            return mesh;
        }
    }
}
