using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Math;

namespace WPFEngine.Utils
{
    public static class GeometryUtils
    {
        public static float Cross(Vector2 o, Vector2 a, Vector2 b)
        {
            return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
        }

        public static List<Vector2> ConvexHullGrahamScan(List<Vector2> points)
        {
            if (points.Count < 3)
                return new List<Vector2>(points);

            var sorted = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            var lower = new List<Vector2>();
            foreach (var p in sorted)
            {
                while (lower.Count >= 2 && Cross(lower[^2], lower[^1], p) <= 0)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            var upper = new List<Vector2>();
            for (int i = sorted.Count - 1; i >= 0; i--)
            {
                var p = sorted[i];
                while (upper.Count >= 2 && Cross(upper[^2], upper[^1], p) <= 0)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);

            return lower;
        }

        public static List<Vector2> ConvexHullQuickhull(List<Vector2> points)
        {
            if (points.Count < 3)
                return new List<Vector2>(points);

            var hull = new List<Vector2>();
            var allPoints = new List<Vector2>(points);

            var minX = allPoints.Min(p => p.X);
            var maxX = allPoints.Max(p => p.X);
            var leftMost = allPoints.First(p => Math.Abs(p.X - minX) < 0.0001f);
            var rightMost = allPoints.First(p => Math.Abs(p.X - maxX) < 0.0001f);

            hull.Add(leftMost);
            hull.Add(rightMost);

            var leftSet = new List<Vector2>();
            var rightSet = new List<Vector2>();

            for (int i = 0; i < allPoints.Count; i++)
            {
                var p = allPoints[i];
                if (p == leftMost || p == rightMost) continue;

                if (Cross(leftMost, rightMost, p) > 0)
                    leftSet.Add(p);
                else
                    rightSet.Add(p);
            }

            QuickHullRecursive(leftMost, rightMost, leftSet, hull, allPoints);
            QuickHullRecursive(rightMost, leftMost, rightSet, hull, allPoints);

            return hull;
        }

        private static void QuickHullRecursive(Vector2 a, Vector2 b, List<Vector2> pointSet,
            List<Vector2> hull, List<Vector2> allPoints)
        {
            if (pointSet.Count == 0) return;

            var maxDist = 0f;
            Vector2 furthest = pointSet[0];
            var lineDist = 0f;

            for (int i = 0; i < pointSet.Count; i++)
            {
                var p = pointSet[i];
                var dist = Math.Abs(Cross(a, b, p)) / (a - b).Length();
                if (dist > maxDist)
                {
                    maxDist = dist;
                    furthest = p;
                    lineDist = Cross(a, b, p);
                }
            }

            var insertIdx = hull.IndexOf(b);
            hull.Insert(insertIdx, furthest);

            var leftSetAP = new List<Vector2>();
            var rightSetAP = new List<Vector2>();
            for (int i = 0; i < pointSet.Count; i++)
            {
                var p = pointSet[i];
                if (p == furthest) continue;
                if (Cross(a, furthest, p) > 0)
                    leftSetAP.Add(p);
                else if (Cross(furthest, b, p) > 0)
                    rightSetAP.Add(p);
            }

            QuickHullRecursive(a, furthest, leftSetAP, hull, allPoints);
            QuickHullRecursive(furthest, b, rightSetAP, hull, allPoints);
        }

        public static bool PointInPolygonRayCast(Vector2 point, List<Vector2> polygon)
        {
            if (polygon.Count < 3) return false;

            bool inside = false;
            int j = polygon.Count - 1;

            for (int i = 0; i < polygon.Count; i++)
            {
                if ((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
                    point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
        }

        public static bool PointInPolygonWinding(Vector2 point, List<Vector2> polygon)
        {
            if (polygon.Count < 3) return false;

            float winding = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                var v1 = polygon[i];
                var v2 = polygon[(i + 1) % polygon.Count];

                if (v1.Y <= point.Y)
                {
                    if (v2.Y > point.Y)
                    {
                        if (IsLeft(v1, v2, point) > 0)
                            winding += 1;
                    }
                }
                else
                {
                    if (v2.Y <= point.Y)
                    {
                        if (IsLeft(v1, v2, point) < 0)
                            winding -= 1;
                    }
                }
            }

            return winding != 0;
        }

        private static float IsLeft(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            return (v1.X - v0.X) * (v2.Y - v0.Y) - (v2.X - v0.X) * (v1.Y - v0.Y);
        }

        public static bool? LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            var d = (p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X);
            if (Math.Abs(d) < 1e-10f) return null;

            var t = ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X)) / d;
            var u = -((p1.X - p2.X) * (p1.Y - p3.Y) - (p1.Y - p2.Y) * (p1.X - p3.X)) / d;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                return new Vector2(p1.X + t * (p2.X - p1.X), p1.Y + t * (p2.Y - p1.Y));

            return null;
        }

        public static (bool intersects, Vector2 point, Vector2 normal) LineSegmentIntersection(
            Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            var d = (p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X);
            if (Math.Abs(d) < 1e-10f)
                return (false, Vector2.Zero, Vector2.Zero);

            var t = ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X)) / d;
            var u = -((p1.X - p2.X) * (p1.Y - p3.Y) - (p1.Y - p2.Y) * (p1.X - p3.X)) / d;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                var point = new Vector2(p1.X + t * (p2.X - p1.X), p1.Y + t * (p2.Y - p1.Y));
                var segDir = p2 - p1;
                var normal = new Vector2(-segDir.Y, segDir.X).Normalized();
                return (true, point, normal);
            }

            return (false, Vector2.Zero, Vector2.Zero);
        }

        public static List<Vector2> PolygonTriangulationEarClip(List<Vector2> polygon)
        {
            if (polygon.Count < 3)
                return new List<Vector2>();

            var triangles = new List<Vector2>();
            var verts = new List<Vector2>(polygon);
            var indices = new List<int>();

            for (int i = 0; i < verts.Count; i++)
                indices.Add(i);

            while (verts.Count > 3)
            {
                bool earFound = false;
                for (int i = 0; i < verts.Count; i++)
                {
                    int prev = (i - 1 + verts.Count) % verts.Count;
                    int next = (i + 1) % verts.Count;

                    if (IsConvex(verts[prev], verts[i], verts[next]))
                    {
                        bool validEar = true;
                        for (int j = 0; j < verts.Count; j++)
                        {
                            if (j == prev || j == i || j == next) continue;
                            if (PointInPolygonRayCast(verts[j], new List<Vector2> { verts[prev], verts[i], verts[next] }))
                            {
                                validEar = false;
                                break;
                            }
                        }

                        if (validEar)
                        {
                            triangles.Add(verts[prev]);
                            triangles.Add(verts[i]);
                            triangles.Add(verts[next]);

                            verts.RemoveAt(i);
                            earFound = true;
                            break;
                        }
                    }
                }

                if (!earFound)
                    break;
            }

            if (verts.Count == 3)
            {
                triangles.Add(verts[0]);
                triangles.Add(verts[1]);
                triangles.Add(verts[2]);
            }

            return triangles;
        }

        private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
        {
            return Cross(a, b, c) >= 0;
        }

        public static List<Vector2> SutherlandHodgmanClip(List<Vector2> subjectPoly, List<Vector2> clipPoly)
        {
            var output = new List<Vector2>(subjectPoly);

            for (int i = 0; i < clipPoly.Count; i++)
            {
                if (output.Count == 0) break;

                var input = new List<Vector2>(output);
                output.Clear();

                Vector2 edgeStart = clipPoly[i];
                Vector2 edgeEnd = clipPoly[(i + 1) % clipPoly.Count];

                for (int j = 0; j < input.Count; j++)
                {
                    Vector2 current = input[j];
                    Vector2 previous = input[(j - 1 + input.Count) % input.Count];

                    float currentSide = (edgeEnd.X - edgeStart.X) * (current.Y - edgeStart.Y) -
                                       (edgeEnd.Y - edgeStart.Y) * (current.X - edgeStart.X);
                    float previousSide = (edgeEnd.X - edgeStart.X) * (previous.Y - edgeStart.Y) -
                                        (edgeEnd.Y - edgeStart.Y) * (previous.X - edgeStart.X);

                    if (currentSide >= 0)
                    {
                        if (previousSide < 0)
                        {
                            var intersection = LineIntersection(previous, current, edgeStart, edgeEnd);
                            if (intersection.HasValue)
                                output.Add(intersection.Value);
                        }
                        output.Add(current);
                    }
                    else if (previousSide >= 0)
                    {
                        var intersection = LineIntersection(previous, current, edgeStart, edgeEnd);
                        if (intersection.HasValue)
                            output.Add(intersection.Value);
                    }
                }
            }

            return output;
        }

        public static List<Vector2> MinkowskiSum(List<Vector2> polyA, List<Vector2> polyB)
        {
            var result = new List<Vector2>();
            int n = polyA.Count, m = polyB.Count;
            int i = 0, j = 0;

            while (i < n || j < m)
            {
                result.Add(new Vector2(polyA[i % n].X + polyB[j % m].X, polyA[i % n].Y + polyB[j % m].Y));

                float cosI = CosAngle(polyA[(i + 1) % n] - polyA[i], Vector2.Right);
                float cosJ = CosAngle(polyB[(j + 1) % m] - polyB[j], Vector2.Right);

                if (cosI >= cosJ)
                    i++;
                if (cosJ >= cosI)
                    j++;
            }

            return ConvexHullGrahamScan(result);
        }

        public static List<Vector2> MinkowskiDifference(List<Vector2> polyA, List<Vector2> polyB)
        {
            var negB = polyB.Select(p => -p).ToList();
            return MinkowskiSum(polyA, negB);
        }

        private static float CosAngle(Vector2 a, Vector2 b)
        {
            return a.Dot(b) / (a.Length() * b.Length() + 1e-10f);
        }

        public static float PolygonArea(List<Vector2> polygon)
        {
            float area = 0;
            int n = polygon.Count;
            for (int i = 0; i < n; i++)
            {
                area += polygon[i].X * polygon[(i + 1) % n].Y;
                area -= polygon[(i + 1) % n].X * polygon[i].Y;
            }
            return Math.Abs(area) / 2;
        }

        public static Vector2 PolygonCentroid(List<Vector2> polygon)
        {
            float cx = 0, cy = 0;
            float area = 0;
            int n = polygon.Count;

            for (int i = 0; i < n; i++)
            {
                float x0 = polygon[i].X, y0 = polygon[i].Y;
                float x1 = polygon[(i + 1) % n].X, y1 = polygon[(i + 1) % n].Y;
                float cross = x0 * y1 - x1 * y0;
                area += cross;
                cx += (x0 + x1) * cross;
                cy += (y0 + y1) * cross;
            }

            area /= 2;
            cx /= (6 * area);
            cy /= (6 * area);

            return new Vector2(cx, cy);
        }
    }

    public class BezierCurve
    {
        public Vector2 P0 { get; set; }
        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }
        public Vector2? P3 { get; set; }

        public bool IsCubic => P3.HasValue;

        public BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = null;
        }

        public BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }

        public Vector2 GetPoint(float t)
        {
            t = Math.Clamp(t, 0, 1);
            if (IsCubic)
            {
                float t2 = t * t;
                float t3 = t2 * t;
                float mt = 1 - t;
                float mt2 = mt * mt;
                float mt3 = mt2 * mt;
                return mt3 * P0 + 3 * mt2 * t * P1 + 3 * mt * t2 * P2 + t3 * P3!.Value;
            }
            else
            {
                float mt = 1 - t;
                return mt * mt * P0 + 2 * mt * t * P1 + t * t * P2;
            }
        }

        public Vector2 GetTangent(float t)
        {
            t = Math.Clamp(t, 0, 1);
            if (IsCubic)
            {
                float t2 = t * t;
                float mt = 1 - t;
                float mt2 = mt * mt;
                return 3 * mt2 * (P1 - P0) + 6 * mt * t * (P2 - P1) + 3 * t2 * (P3!.Value - P2);
            }
            else
            {
                return 2 * (1 - t) * (P1 - P0) + 2 * t * (P2 - P1);
            }
        }

        public Vector2 GetNormal(float t)
        {
            var tangent = GetTangent(t);
            return new Vector2(-tangent.Y, tangent.X).Normalized();
        }

        public float GetCurvature(float t)
        {
            var d1 = GetTangent(t);
            var d2 = GetSecondDerivative(t);
            return (d1.X * d2.Y - d1.Y * d2.X) / (float)Math.Pow(d1.Length(), 3);
        }

        private Vector2 GetSecondDerivative(float t)
        {
            if (IsCubic)
            {
                float mt = 1 - t;
                return 6 * mt * (P2 - 2 * P1 + P0) + 6 * t * (P3!.Value - 2 * P2 + P1);
            }
            else
            {
                return 2 * (P2 - 2 * P1 + P0);
            }
        }

        public float Length(int segments = 100)
        {
            float length = 0;
            Vector2 prev = GetPoint(0);
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 current = GetPoint(t);
                length += prev.Distance(current);
                prev = current;
            }
            return length;
        }

        public float ApproximateLength(int segments = 100)
        {
            if (IsCubic)
            {
                float bezierLen = GetPoint(0).Distance(P1) + GetPoint(1).Distance(P2);
                var chordLen = P0.Distance(P3!.Value);
                return (bezierLen * 2 + chordLen) / 3;
            }
            else
            {
                float bezierLen = GetPoint(0).Distance(P1) + GetPoint(1).Distance(P2);
                var chordLen = P0.Distance(P2);
                return (bezierLen + chordLen) / 2;
            }
        }

        public List<Vector2> Sample(int count)
        {
            var points = new List<Vector2>();
            for (int i = 0; i <= count; i++)
            {
                float t = (float)i / count;
                points.Add(GetPoint(t));
            }
            return points;
        }

        public float FindNearestT(Vector2 point, int iterations = 20)
        {
            float bestT = 0;
            float bestDist = float.MaxValue;

            for (int i = 0; i <= iterations; i++)
            {
                float t = (float)i / iterations;
                var p = GetPoint(t);
                float dist = p.Distance(point);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestT = t;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                float step = 0.1f / (float)Math.Pow(2, i);
                float tMin = Math.Max(0, bestT - step);
                float tMax = Math.Min(1, bestT + step);

                for (float t = tMin; t <= tMax; t += step / 10)
                {
                    var p = GetPoint(t);
                    float dist = p.Distance(point);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestT = t;
                    }
                }
            }

            return bestT;
        }

        public (float t, float distance) FindNearestPoint(Vector2 point)
        {
            float t = FindNearestT(point);
            return (t, GetPoint(t).Distance(point));
        }

        public static BezierCurve FromFourPoints(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return new BezierCurve(p0, p1, p2, p3);
        }

        public BezierCurve Split(float t)
        {
            if (IsCubic)
            {
                var p01 = P0.Lerp(P1, t);
                var p12 = P1.Lerp(P2, t);
                var p23 = P2.Lerp(P3!.Value, t);
                var p012 = p01.Lerp(p12, t);
                var p123 = p12.Lerp(p23, t);
                var p0123 = p012.Lerp(p123, t);
                return new BezierCurve(p0123, p123, p23, P3!.Value);
            }
            else
            {
                var p01 = P0.Lerp(P1, t);
                var p12 = P1.Lerp(P2, t);
                var p012 = p01.Lerp(p12, t);
                return new BezierCurve(p012, p12, P2);
            }
        }
    }

    public class BezierPath
    {
        private readonly List<BezierCurve> _segments;
        private readonly List<float> _segmentLengths;
        private float _totalLength;

        public IReadOnlyList<BezierCurve> Segments => _segments;
        public float TotalLength => _totalLength;

        public BezierPath()
        {
            _segments = new List<BezierCurve>();
            _segmentLengths = new List<float>();
            _totalLength = 0;
        }

        public BezierPath(IEnumerable<Vector2> controlPoints, bool cubic = false)
        {
            _segments = new List<BezierCurve>();
            _segmentLengths = new List<float>();
            _totalLength = 0;

            var points = controlPoints.ToList();
            if (points.Count < 2) return;

            if (cubic && points.Count >= 4)
            {
                for (int i = 0; i < points.Count - 3; i += 3)
                {
                    AddSegment(new BezierCurve(points[i], points[i + 1], points[i + 2], points[i + 3]));
                }
            }
            else
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    Vector2 p0 = points[i];
                    Vector2 p2 = points[i + 1];
                    Vector2 p1 = p0 + (p2 - p0) * 0.33f;
                    AddSegment(new BezierCurve(p0, p1, p2));
                }
            }
        }

        public void AddSegment(BezierCurve segment)
        {
            _segments.Add(segment);
            float len = segment.Length();
            _segmentLengths.Add(len);
            _totalLength += len;
        }

        public void AddSegment(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            AddSegment(new BezierCurve(p0, p1, p2));
        }

        public void AddSegment(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            AddSegment(new BezierCurve(p0, p1, p2, p3));
        }

        public Vector2 GetPointAtLength(float distance)
        {
            return GetPointAtT(distance / _totalLength);
        }

        public Vector2 GetPointAtT(float t)
        {
            if (_segments.Count == 0) return Vector2.Zero;
            if (_segments.Count == 1) return _segments[0].GetPoint(t);

            t = Math.Clamp(t, 0, 1);
            float totalT = t * _segments.Count;
            int segmentIndex = Math.Min((int)totalT, _segments.Count - 1);
            float localT = totalT - segmentIndex;

            return _segments[segmentIndex].GetPoint(localT);
        }

        public Vector2 GetTangentAtT(float t)
        {
            if (_segments.Count == 0) return Vector2.Right;
            if (_segments.Count == 1) return _segments[0].GetTangent(t);

            t = Math.Clamp(t, 0, 1);
            float totalT = t * _segments.Count;
            int segmentIndex = Math.Min((int)totalT, _segments.Count - 1);
            float localT = totalT - segmentIndex;

            return _segments[segmentIndex].GetTangent(localT).Normalized();
        }

        public Vector2 GetNormalAtT(float t)
        {
            var tangent = GetTangentAtT(t);
            return new Vector2(-tangent.Y, tangent.X);
        }

        public float GetCurvatureAtT(float t)
        {
            if (_segments.Count == 0) return 0;
            if (_segments.Count == 1) return _segments[0].GetCurvature(t);

            t = Math.Clamp(t, 0, 1);
            float totalT = t * _segments.Count;
            int segmentIndex = Math.Min((int)totalT, _segments.Count - 1);
            float localT = totalT - segmentIndex;

            return _segments[segmentIndex].GetCurvature(localT);
        }

        public List<Vector2> Sample(int samplesPerSegment = 20)
        {
            var points = new List<Vector2>();
            int totalSamples = _segments.Count * samplesPerSegment;

            for (int i = 0; i <= totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                points.Add(GetPointAtT(t));
            }

            return points;
        }

        public Vector2 FollowPath(float distance, ref float currentT)
        {
            currentT += distance / _totalLength;
            currentT = Math.Clamp(currentT, 0, 1);
            return GetPointAtT(currentT);
        }

        public List<Vector2> GetEquidistantPoints(int count)
        {
            var points = new List<Vector2>();
            if (count < 2) return points;

            float segmentSize = _totalLength / (count - 1);

            for (int i = 0; i < count; i++)
            {
                float distance = i * segmentSize;
                points.Add(GetPointAtLength(distance));
            }

            return points;
        }

        public static BezierPath CreateSmoothPath(List<Vector2> points, bool loop = false)
        {
            if (points.Count < 2) return new BezierPath();

            var path = new BezierPath();
            var pts = new List<Vector2>(points);

            if (loop)
            {
                pts.Add(pts[0]);
                pts.Insert(0, pts[^2]);
            }
            else
            {
                pts.Insert(0, pts[0] * 2 - pts[1]);
                pts.Add(pts[^2] * 2 - pts[^1]);
            }

            for (int i = 0; i < pts.Count - 3; i++)
            {
                Vector2 p0 = pts[i];
                Vector2 p1 = pts[i + 1];
                Vector2 p2 = pts[i + 2];
                Vector2 p3 = pts[i + 3];

                Vector2 cp1 = p1 + (p2 - p0) / 6;
                Vector2 cp2 = p2 - (p3 - p1) / 6;

                path.AddSegment(new BezierCurve(p1, cp1, cp2, p2));
            }

            return path;
        }
    }

    public class CatmullRomSpline
    {
        private readonly List<Vector2> _points;
        private readonly bool _loop;
        private readonly float _tension;

        public IReadOnlyList<Vector2> Points => _points;
        public bool IsLoop => _loop;

        public CatmullRomSpline(IEnumerable<Vector2> points, bool loop = false, float tension = 0.5f)
        {
            _points = new List<Vector2>(points);
            _loop = loop;
            _tension = tension;
        }

        public Vector2 GetPoint(float t)
        {
            if (_points.Count < 2)
                return _points.Count == 1 ? _points[0] : Vector2.Zero;

            int numSegments = _loop ? _points.Count : _points.Count - 1;
            t = Math.Clamp(t, 0, 1) * numSegments;

            int i0, i1, i2, i3;
            float localT;

            if (_loop)
            {
                int segmentIndex = (int)t % _points.Count;
                localT = t - (int)t;

                i1 = segmentIndex;
                i2 = (segmentIndex + 1) % _points.Count;
                i3 = (segmentIndex + 2) % _points.Count;
                i0 = (segmentIndex - 1 + _points.Count) % _points.Count;
            }
            else
            {
                int segmentIndex = Math.Min((int)t, numSegments - 1);
                localT = t - segmentIndex;

                i0 = Math.Max(0, segmentIndex - 1);
                i1 = segmentIndex;
                i2 = Math.Min(_points.Count - 1, segmentIndex + 1);
                i3 = Math.Min(_points.Count - 1, segmentIndex + 2);
            }

            return CatmullRom(_points[i0], _points[i1], _points[i2], _points[i3], localT);
        }

        private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float s = (1 - _tension) / 2;

            float b1 = s * (-t3 + 2 * t2 - t);
            float b2 = s * (-t3 + t2) + (2 * t3 - 3 * t2 + 1);
            float b3 = s * (t3 - 2 * t2 + t) + (-2 * t3 + 3 * t2);
            float b4 = s * (t3 - t2);

            return b1 * p0 + b2 * p1 + b3 * p2 + b4 * p3;
        }

        public Vector2 GetTangent(float t)
        {
            float epsilon = 0.001f;
            float t1 = Math.Max(0, t - epsilon);
            float t2 = Math.Min(1, t + epsilon);

            return (GetPoint(t2) - GetPoint(t1)).Normalized();
        }

        public List<Vector2> Sample(int count)
        {
            var points = new List<Vector2>();
            for (int i = 0; i <= count; i++)
            {
                float t = (float)i / count;
                points.Add(GetPoint(t));
            }
            return points;
        }
    }

    public class BSpline
    {
        private readonly List<Vector2> _controlPoints;
        private readonly int _degree;
        private readonly bool _loop;
        private readonly List<float> _knotVector;

        public IReadOnlyList<Vector2> ControlPoints => _controlPoints;
        public int Degree => _degree;

        public BSpline(IEnumerable<Vector2> controlPoints, int degree = 3, bool loop = false)
        {
            _controlPoints = new List<Vector2>(controlPoints);
            _degree = Math.Min(degree, _controlPoints.Count - 1);
            _loop = loop;

            int n = _controlPoints.Count - 1;
            int k = _degree + 1;
            _knotVector = new List<float>();

            for (int i = 0; i <= n + k; i++)
            {
                if (i < k)
                    _knotVector.Add(0);
                else if (i > n)
                    _knotVector.Add(1);
                else
                    _knotVector.Add((float)(i - k + 1) / (n - k + 2));
            }
        }

        public Vector2 GetPoint(float t)
        {
            t = Math.Clamp(t, 0, 1);
            int n = _controlPoints.Count - 1;
            int k = _degree + 1;

            int span = FindSpan(t);
            var basis = ComputeBasisFunctions(span, t);

            Vector2 result = Vector2.Zero;
            for (int j = 0; j <= _degree; j++)
            {
                result += _controlPoints[span - _degree + j] * basis[j];
            }

            return result;
        }

        private int FindSpan(float t)
        {
            int n = _controlPoints.Count - 1;
            int k = _degree + 1;

            if (t >= _knotVector[n + 1])
                return n;

            if (t <= _knotVector[k - 1])
                return k - 1;

            int low = k - 1;
            int high = n + 1;

            while (low < high)
            {
                int mid = (low + high) / 2;
                if (t <= _knotVector[mid])
                    high = mid;
                else
                    low = mid + 1;
            }

            return low;
        }

        private float[] ComputeBasisFunctions(int span, float t)
        {
            var N = new float[_degree + 1];
            var left = new float[_degree + 1];
            var right = new float[_degree + 1];

            N[0] = 1;

            for (int i = 1; i <= _degree; i++)
            {
                left[i] = t - _knotVector[span - i + 1];
                right[i] = _knotVector[span + i] - t;

                float saved = 0;
                for (int j = 0; j < i; j++)
                {
                    float denom = right[j + 1] + left[i - j];
                    float temp = denom > 1e-10f ? N[j] / denom : 0;
                    N[j] = saved + right[j + 1] * temp;
                    saved = left[i - j] * temp;
                }
                N[i] = saved;
            }

            return N;
        }

        public List<Vector2> Sample(int count)
        {
            var points = new List<Vector2>();
            for (int i = 0; i <= count; i++)
            {
                float t = (float)i / count;
                points.Add(GetPoint(t));
            }
            return points;
        }
    }

    public class HermiteSpline
    {
        private readonly List<Vector2> _positions;
        private readonly List<Vector2> _tangents;
        private readonly bool _loop;

        public IReadOnlyList<Vector2> Positions => _positions;
        public IReadOnlyList<Vector2> Tangents => _tangents;

        public HermiteSpline(IEnumerable<Vector2> positions, IEnumerable<Vector2> tangents, bool loop = false)
        {
            _positions = new List<Vector2>(positions);
            _tangents = new List<Vector2>(tangents);
            _loop = loop;
        }

        public static HermiteSpline FromPoints(IEnumerable<Vector2> points, bool loop = false, float tangentScale = 0.5f)
        {
            var pts = new List<Vector2>(points);
            var tangents = new List<Vector2>();

            for (int i = 0; i < pts.Count; i++)
            {
                Vector2 prev = pts[(i - 1 + pts.Count) % pts.Count];
                Vector2 next = pts[(i + 1) % pts.Count];
                tangents.Add((next - prev) * tangentScale);
            }

            return new HermiteSpline(pts, tangents, loop);
        }

        public Vector2 GetPoint(float t)
        {
            if (_positions.Count < 2)
                return _positions.Count == 1 ? _positions[0] : Vector2.Zero;

            int numSegments = _loop ? _positions.Count : _positions.Count - 1;
            t = Math.Clamp(t, 0, 1) * numSegments;

            int segmentIndex = Math.Min((int)t, numSegments - 1);
            float localT = t - segmentIndex;

            int i0 = segmentIndex;
            int i1 = (segmentIndex + 1) % _positions.Count;

            return HermiteInterp(_positions[i0], _positions[i1], _tangents[i0], _tangents[i1], localT);
        }

        public static Vector2 HermiteInterp(Vector2 p0, Vector2 p1, Vector2 m0, Vector2 m1, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float h00 = 2 * t3 - 3 * t2 + 1;
            float h10 = t3 - 2 * t2 + t;
            float h01 = -2 * t3 + 3 * t2;
            float h11 = t3 - t2;

            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }

        public Vector2 GetTangent(float t)
        {
            float epsilon = 0.001f;
            float t1 = Math.Max(0, t - epsilon);
            float t2 = Math.Min(1, t + epsilon);

            return (GetPoint(t2) - GetPoint(t1)).Normalized();
        }

        public List<Vector2> Sample(int count)
        {
            var points = new List<Vector2>();
            for (int i = 0; i <= count; i++)
            {
                float t = (float)i / count;
                points.Add(GetPoint(t));
            }
            return points;
        }
    }

    public class CellularAutomaton
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _seed;
        private readonly Random _random;
        private bool[,] _grid;
        private bool[,] _nextGrid;

        public int Width => _width;
        public int Height => _height;
        public bool[,] Grid => _grid;

        public CellularAutomaton(int width, int height, int? seed = null)
        {
            _width = width;
            _height = height;
            _seed = seed ?? Environment.TickCount;
            _random = new Random(_seed);
            _grid = new bool[width, height];
            _nextGrid = new bool[width, height];
        }

        public void Initialize(float fillProbability)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _grid[x, y] = _random.NextDouble() < fillProbability;
                }
            }
        }

        public void InitializeWithPattern(Func<int, int, bool> patternFunc)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _grid[x, y] = patternFunc(x, y);
                }
            }
        }

        public void Step(int birthThreshold = 4, int survivalThresholdMin = 3, int survivalThresholdMax = 3)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int neighbors = CountNeighbors(x, y);

                    if (_grid[x, y])
                    {
                        _nextGrid[x, y] = neighbors >= survivalThresholdMin && neighbors <= survivalThresholdMax;
                    }
                    else
                    {
                        _nextGrid[x, y] = neighbors >= birthThreshold;
                    }
                }
            }

            SwapGrids();
        }

        public void StepCustom(Func<bool, int, bool> rule)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int neighbors = CountNeighbors(x, y);
                    _nextGrid[x, y] = rule(_grid[x, y], neighbors);
                }
            }

            SwapGrids();
        }

        private int CountNeighbors(int x, int y)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = (x + dx + _width) % _width;
                    int ny = (y + dy + _height) % _height;

                    if (_grid[nx, ny]) count++;
                }
            }
            return count;
        }

        private void SwapGrids()
        {
            var temp = _grid;
            _grid = _nextGrid;
            _nextGrid = temp;
        }

        public void FillCell(int x, int y, bool value)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
                _grid[x, y] = value;
        }

        public bool GetCell(int x, int y)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
                return _grid[x, y];
            return false;
        }

        public void Simulate(int iterations, int birthThreshold = 4, int survivalThresholdMin = 3, int survivalThresholdMax = 3)
        {
            for (int i = 0; i < iterations; i++)
            {
                Step(birthThreshold, survivalThresholdMin, survivalThresholdMax);
            }
        }

        public List<List<int>> FindConnectedRegions()
        {
            var visited = new bool[_width, _height];
            var regions = new List<List<int>>();

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_grid[x, y] && !visited[x, y])
                    {
                        var region = new List<int>();
                        FloodFill(x, y, visited, region, true);
                        regions.Add(region);
                    }
                }
            }

            return regions;
        }

        public List<List<int>> FindEmptyRegions(int minSize = 10)
        {
            var visited = new bool[_width, _height];
            var regions = new List<List<int>>();

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (!_grid[x, y] && !visited[x, y])
                    {
                        var region = new List<int>();
                        FloodFill(x, y, visited, region, false);
                        if (region.Count >= minSize)
                            regions.Add(region);
                    }
                }
            }

            return regions;
        }

        private void FloodFill(int startX, int startY, bool[,] visited, List<int> region, bool value)
        {
            var stack = new Stack<(int x, int y)>();
            stack.Push((startX, startY));

            while (stack.Count > 0)
            {
                var (x, y) = stack.Pop();

                if (x < 0 || x >= _width || y < 0 || y >= _height)
                    continue;

                if (visited[x, y] || _grid[x, y] != value)
                    continue;

                visited[x, y] = true;
                region.Add(x * _height + y);

                stack.Push((x + 1, y));
                stack.Push((x - 1, y));
                stack.Push((x, y + 1));
                stack.Push((x, y - 1));
            }
        }

        public void ClearBorder(int borderSize = 1, bool value = true)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int i = 0; i < borderSize; i++)
                {
                    if (x + i < _width) _grid[x + i, i] = value;
                    if (x + i < _width && _height - 1 - i >= 0) _grid[x + i, _height - 1 - i] = value;
                }
            }

            for (int y = 0; y < _height; y++)
            {
                for (int i = 0; i < borderSize; i++)
                {
                    if (y + i < _height) _grid[i, y + i] = value;
                    if (y + i < _height && _width - 1 - i >= 0) _grid[_width - 1 - i, y + i] = value;
                }
            }
        }

        public float GetFillRatio()
        {
            int count = 0;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_grid[x, y]) count++;
                }
            }
            return (float)count / (_width * _height);
        }
    }

    public class PerlinNoise
    {
        private readonly int[] _permutation;
        private readonly int[] _p;
        private readonly int _seed;
        private readonly float _offsetX;
        private readonly float _offsetY;
        private readonly float _offsetZ;

        public PerlinNoise(int? seed = null, float offsetX = 0, float offsetY = 0, float offsetZ = 0)
        {
            _seed = seed ?? Environment.TickCount;
            _offsetX = offsetX;
            _offsetY = offsetY;
            _offsetZ = offsetZ;

            var random = new Random(_seed);
            _permutation = new int[256];
            _p = new int[512];

            for (int i = 0; i < 256; i++)
                _permutation[i] = i;

            for (int i = 255; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (_permutation[i], _permutation[j]) = (_permutation[j], _permutation[i]);
            }

            for (int i = 0; i < 512; i++)
                _p[i] = _permutation[i & 255];
        }

        public float GetNoise(float x, float y = 0, float z = 0)
        {
            x += _offsetX;
            y += _offsetY;
            z += _offsetZ;

            int xi = (int)Math.Floor(x) & 255;
            int yi = (int)Math.Floor(y) & 255;
            int zi = (int)Math.Floor(z) & 255;

            float xf = x - (float)Math.Floor(x);
            float yf = y - (float)Math.Floor(y);
            float zf = z - (float)Math.Floor(z);

            float u = Fade(xf);
            float v = Fade(yf);
            float w = Fade(zf);

            int aaa = _p[_p[_p[xi] + yi] + zi];
            int aba = _p[_p[_p[xi] + yi + 1] + zi];
            int aab = _p[_p[_p[xi] + yi] + zi + 1];
            int abb = _p[_p[_p[xi] + yi + 1] + zi + 1];
            int baa = _p[_p[_p[xi + 1] + yi] + zi];
            int bba = _p[_p[_p[xi + 1] + yi + 1] + zi];
            int bab = _p[_p[_p[xi + 1] + yi] + zi + 1];
            int bbb = _p[_p[_p[xi + 1] + yi + 1] + zi + 1];

            float x1 = Lerp(Grad3(aaa, xf, yf, zf), Grad3(baa, xf - 1, yf, zf), u);
            float x2 = Lerp(Grad3(aba, xf, yf - 1, zf), Grad3(bba, xf - 1, yf - 1, zf), u);
            float y1 = Lerp(x1, x2, v);

            x1 = Lerp(Grad3(aab, xf, yf, zf - 1), Grad3(bab, xf - 1, yf, zf - 1), u);
            x2 = Lerp(Grad3(abb, xf, yf - 1, zf - 1), Grad3(bbb, xf - 1, yf - 1, zf - 1), u);
            float y2 = Lerp(x1, x2, v);

            return (Lerp(y1, y2, w) + 1) / 2;
        }

        public float Fractal(int octaves, float x, float y, float persistence = 0.5f, float lacunarity = 2.0f)
        {
            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float maxValue = 0;

            for (int i = 0; i < octaves; i++)
            {
                total += GetNoise(x * frequency, y * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return total / maxValue;
        }

        public float Turbulence(int octaves, float x, float y, float baseAmplitude = 1.0f)
        {
            float total = 0;
            float frequency = 1;
            float amplitude = baseAmplitude;
            float maxValue = 0;

            for (int i = 0; i < octaves; i++)
            {
                total += Math.Abs((GetNoise(x * frequency, y * frequency) - 0.5f) * 2) * amplitude;
                maxValue += amplitude;
                amplitude *= 0.5f;
                frequency *= 2;
            }

            return total / maxValue;
        }

        public float Marble(float x, float y, int octaves = 4)
        {
            float noise = Fractal(octaves, x, y);
            return (float)(Math.Sin((x + noise * 3) * Math.PI) + 1) / 2;
        }

        public float Wood(float x, float y, int octaves = 4)
        {
            float noise = Fractal(octaves, x, y);
            return (float)(Math.Sin((x + noise * 5) * Math.PI * 0.5f) + 1) / 2;
        }

        private static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        private static float Grad3(int hash, float x, float y, float z)
        {
            int h = hash & 15;
            float u = h < 8 ? x : y;
            float v = h < 4 ? y : h == 12 || h == 14 ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
    }

    public class SimplexNoise
    {
        private readonly int[] _perm;
        private readonly int _seed;
        private static readonly float F2 = 0.5f * ((float)Math.Sqrt(3.0f) - 1.0f);
        private static readonly float G2 = (3.0f - (float)Math.Sqrt(3.0f)) / 6.0f;
        private static readonly float F3 = 1.0f / 3.0f;
        private static readonly float G3 = 1.0f / 6.0f;

        private static readonly int[][] Grad3 = {
            new[] {1, 1, 0}, new[] {-1, 1, 0}, new[] {1, -1, 0}, new[] {-1, -1, 0},
            new[] {1, 0, 1}, new[] {-1, 0, 1}, new[] {1, 0, -1}, new[] {-1, 0, -1},
            new[] {0, 1, 1}, new[] {0, -1, 1}, new[] {0, 1, -1}, new[] {0, -1, -1}
        };

        public SimplexNoise(int? seed = null)
        {
            _seed = seed ?? Environment.TickCount;
            var random = new Random(_seed);
            _perm = new int[256];

            for (int i = 0; i < 256; i++)
                _perm[i] = i;

            for (int i = 255; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (_perm[i], _perm[j]) = (_perm[j], _perm[i]);
            }

            var perm12 = new int[512];
            for (int i = 0; i < 512; i++)
                perm12[i] = _perm[i & 255];
            _perm = perm12;
        }

        public float GetNoise(float xin, float yin)
        {
            float s = (xin + yin) * F2;
            int i = (int)Math.Floor(xin + s);
            int j = (int)Math.Floor(yin + s);

            float t = (i + j) * G2;
            float X0 = i - t;
            float Y0 = j - t;
            float x0 = xin - X0;
            float y0 = yin - Y0;

            int i1, j1;
            if (x0 > y0) { i1 = 1; j1 = 0; }
            else { i1 = 0; j1 = 1; }

            float x1 = x0 - i1 + G2;
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1.0f + 2.0f * G2;
            float y2 = y0 - 1.0f + 2.0f * G2;

            int ii = i & 255;
            int jj = j & 255;

            float n0, n1, n2;

            float t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 < 0) n0 = 0;
            else { t0 *= t0; n0 = t0 * t0 * Dot(Grad3[_perm[ii + _perm[jj]] % 12], x0, y0); }

            float t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 < 0) n1 = 0;
            else { t1 *= t1; n1 = t1 * t1 * Dot(Grad3[_perm[ii + i1 + _perm[jj + j1]] % 12], x1, y1); }

            float t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 < 0) n2 = 0;
            else { t2 *= t2; n2 = t2 * t2 * Dot(Grad3[_perm[ii + 1 + _perm[jj + 1]] % 12], x2, y2); }

            return (70.0f * (n0 + n1 + n2) + 1) / 2;
        }

        public float GetNoise3D(float xin, float yin, float zin)
        {
            float s = (xin + yin + zin) * F3;
            int i = (int)Math.Floor(xin + s);
            int j = (int)Math.Floor(yin + s);
            int k = (int)Math.Floor(zin + s);

            float t = (i + j + k) * G3;
            float X0 = i - t;
            float Y0 = j - t;
            float Z0 = k - t;

            float x0 = xin - X0;
            float y0 = yin - Y0;
            float z0 = zin - Z0;

            int i1, j1, k1, i2, j2, k2;

            if (x0 >= y0)
            {
                if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
                else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; }
                else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }
            }
            else
            {
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; }
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; }
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
            }

            float x1 = x0 - i1 + G3;
            float y1 = y0 - j1 + G3;
            float z1 = z0 - k1 + G3;
            float x2 = x0 - i2 + 2.0f * G3;
            float y2 = y0 - j2 + 2.0f * G3;
            float z2 = z0 - k2 + 2.0f * G3;
            float x3 = x0 - 1.0f + 3.0f * G3;
            float y3 = y0 - 1.0f + 3.0f * G3;
            float z3 = z0 - 1.0f + 3.0f * G3;

            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;

            float n0, n1, n2, n3;

            float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0) n0 = 0;
            else { t0 *= t0; n0 = t0 * t0 * Dot(Grad3[_perm[ii + _perm[jj + _perm[kk]]] % 12], x0, y0, z0); }

            float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0) n1 = 0;
            else { t1 *= t1; n1 = t1 * t1 * Dot(Grad3[_perm[ii + i1 + _perm[jj + j1 + _perm[kk + k1]]] % 12], x1, y1, z1); }

            float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0) n2 = 0;
            else { t2 *= t2; n2 = t2 * t2 * Dot(Grad3[_perm[ii + i2 + _perm[jj + j2 + _perm[kk + k2]]] % 12], x2, y2, z2); }

            float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0) n3 = 0;
            else { t3 *= t3; n3 = t3 * t3 * Dot(Grad3[_perm[ii + 1 + _perm[jj + 1 + _perm[kk + 1]]] % 12], x3, y3, z3); }

            return (32.0f * (n0 + n1 + n2 + n3) + 1) / 2;
        }

        public float Fractal(int octaves, float x, float y, float persistence = 0.5f)
        {
            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float maxValue = 0;

            for (int i = 0; i < octaves; i++)
            {
                total += GetNoise(x * frequency, y * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }

        private static float Dot(int[] g, float x, float y)
        {
            return g[0] * x + g[1] * y;
        }

        private static float Dot(int[] g, float x, float y, float z)
        {
            return g[0] * x + g[1] * y + g[2] * z;
        }
    }

    public class WorleyNoise
    {
        private readonly int _seed;
        private readonly Random _random;
        private readonly int _cellCount;
        private Vector2[]? _points;

        public WorleyNoise(int? seed = null, int cellCount = 16)
        {
            _seed = seed ?? Environment.TickCount;
            _random = new Random(_seed);
            _cellCount = cellCount;
            GeneratePoints();
        }

        private void GeneratePoints()
        {
            _points = new Vector2[_cellCount * _cellCount];
            for (int i = 0; i < _cellCount; i++)
            {
                for (int j = 0; j < _cellCount; j++)
                {
                    _points[i * _cellCount + j] = new Vector2(
                        (float)_random.NextDouble(),
                        (float)_random.NextDouble()
                    );
                }
            }
        }

        public void Seed(int seed)
        {
            _random = new Random(seed);
            GeneratePoints();
        }

        public float GetNoise(float x, float y)
        {
            int xi = (int)(x * _cellCount) % _cellCount;
            int yi = (int)(y * _cellCount) % _cellCount;

            if (xi < 0) xi += _cellCount;
            if (yi < 0) yi += _cellCount;

            float minDist = float.MaxValue;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = (xi + dx + _cellCount) % _cellCount;
                    int ny = (yi + dy + _cellCount) % _cellCount;

                    var point = _points![ny * _cellCount + nx];
                    float px = (float)((nx + point.X) / (float)_cellCount);
                    float py = (float)((ny + point.Y) / (float)_cellCount);

                    float dist = (float)Math.Sqrt((x - px) * (x - px) + (y - py) * (y - py));
                    minDist = Math.Min(minDist, dist);
                }
            }

            return Math.Min(1, minDist * _cellCount);
        }

        public float GetF2MinusF1(float x, float y)
        {
            int xi = (int)(x * _cellCount) % _cellCount;
            int yi = (int)(y * _cellCount) % _cellCount;

            if (xi < 0) xi += _cellCount;
            if (yi < 0) yi += _cellCount;

            var distances = new List<float>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = (xi + dx + _cellCount) % _cellCount;
                    int ny = (yi + dy + _cellCount) % _cellCount;

                    var point = _points![ny * _cellCount + nx];
                    float px = (float)((nx + point.X) / (float)_cellCount);
                    float py = (float)((ny + point.Y) / (float)_cellCount);

                    float dist = (float)Math.Sqrt((x - px) * (x - px) + (y - py) * (y - py));
                    distances.Add(dist);
                }
            }

            distances.Sort();
            return Math.Min(1, (distances[1] - distances[0]) * _cellCount);
        }

        public Vector2 GetNearestPoint(float x, float y)
        {
            int xi = (int)(x * _cellCount) % _cellCount;
            int yi = (int)(y * _cellCount) % _cellCount;

            if (xi < 0) xi += _cellCount;
            if (yi < 0) yi += _cellCount;

            Vector2 nearest = Vector2.Zero;
            float minDist = float.MaxValue;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = (xi + dx + _cellCount) % _cellCount;
                    int ny = (yi + dy + _cellCount) % _cellCount;

                    var point = _points![ny * _cellCount + nx];
                    float px = (float)((nx + point.X) / (float)_cellCount);
                    float py = (float)((ny + point.Y) / (float)_cellCount);

                    float dist = (float)Math.Sqrt((x - px) * (x - px) + (y - py) * (y - py));
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = new Vector2(px, py);
                    }
                }
            }

            return nearest;
        }
    }

    public class WaveFunctionCollapse
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _tileCount;
        private readonly int[,] _grid;
        private readonly int[,] _possible;
        private readonly int[][][] _rules;
        private readonly int[][][] _compatible;
        private readonly Random _random;
        private readonly List<int>[] _weights;
        private readonly int _totalWeight;

        public int[,] Result => _grid;

        public WaveFunctionCollapse(int width, int height, int tileCount, int? seed = null)
        {
            _width = width;
            _height = height;
            _tileCount = tileCount;
            _random = new Random(seed ?? Environment.TickCount);
            _grid = new int[width, height];
            _possible = new int[width, height];
            _rules = new int[tileCount][][];
            _compatible = new int[tileCount][][];
            _weights = new List<int>[tileCount];

            int w = 0;
            _totalWeight = 0;
            for (int i = 0; i < tileCount; i++)
            {
                _weights[i] = new List<int>();
                for (int j = 0; j <= i; j++)
                {
                    _weights[i].Add(1);
                    w++;
                    _totalWeight++;
                }
            }

            for (int i = 0; i < tileCount; i++)
            {
                _rules[i] = new int[4][];
                _compatible[i] = new int[4][];
                for (int d = 0; d < 4; d++)
                {
                    _rules[i][d] = Enumerable.Range(0, tileCount).ToArray();
                    _compatible[i][d] = Enumerable.Range(0, tileCount).ToArray();
                }
            }

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _possible[x, y] = tileCount - 1;
                }
            }
        }

        public void AddRule(int tileA, int tileB, int direction)
        {
            int[] newRule = new int[_rules[tileA][direction].Length + 1];
            int count = 0;
            for (int i = 0; i < _rules[tileA][direction].Length; i++)
            {
                if (_rules[tileA][direction][i] == tileB)
                    return;
                newRule[count++] = _rules[tileA][direction][i];
            }
            newRule[count] = tileB;
            _rules[tileA][direction] = newRule;

            newRule = new int[_rules[tileB][(direction + 2) % 4].Length + 1];
            count = 0;
            for (int i = 0; i < _rules[tileB][(direction + 2) % 4].Length; i++)
            {
                if (_rules[tileB][(direction + 2) % 4][i] == tileA)
                    return;
                newRule[count++] = _rules[tileB][(direction + 2) % 4][i];
            }
            newRule[count] = tileA;
            _rules[tileB][(direction + 2) % 4] = newRule;
        }

        public bool Generate(int maxIterations = 1000)
        {
            for (int iter = 0; iter < maxIterations; iter++)
            {
                var (x, y) = FindMinimumEntropy();
                if (x < 0) return true;
                if (!Collapse(x, y)) return false;
                Propagate(x, y);
            }
            return false;
        }

        private (int x, int y) FindMinimumEntropy()
        {
            int minPossibilities = int.MaxValue;
            int minX = -1, minY = -1;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_grid[x, y] >= 0) continue;

                    int possibilities = CountPossible(x, y);
                    if (possibilities > 0 && possibilities < minPossibilities)
                    {
                        minPossibilities = possibilities;
                        minX = x;
                        minY = y;
                    }
                }
            }

            return (minX, minY);
        }

        private int CountPossible(int x, int y)
        {
            int count = 0;
            for (int t = 0; t <= _possible[x, y]; t++)
                count++;
            return count;
        }

        private bool Collapse(int x, int y)
        {
            int possibilities = CountPossible(x, y);
            if (possibilities == 0) return false;

            int r = _random.Next(possibilities);
            int tile = -1;
            int count = 0;
            for (int t = 0; t <= _possible[x, y]; t++)
            {
                if (_rules[t] != null)
                {
                    count++;
                    if (count > r) tile = t;
                }
            }
            if (tile < 0) tile = _possible[x, y];

            _grid[x, y] = tile;
            _possible[x, y] = tile;

            return true;
        }

        private void Propagate(int startX, int startY)
        {
            var stack = new Stack<(int x, int y)>();
            stack.Push((startX, startY));

            while (stack.Count > 0)
            {
                var (x, y) = stack.Pop();

                for (int d = 0; d < 4; d++)
                {
                    int nx = x + (d == 0 ? 1 : d == 1 ? 0 : d == 2 ? -1 : 0);
                    int ny = y + (d == 0 ? 0 : d == 1 ? 1 : d == 2 ? 0 : -1);

                    if (nx < 0 || nx >= _width || ny < 0 || ny >= _height) continue;
                    if (_grid[nx, ny] >= 0) continue;

                    bool[] possible = new bool[_tileCount];
                    for (int i = 0; i < _tileCount; i++) possible[i] = true;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int mx = nx + dx, my = ny + dy;
                            if (mx < 0 || mx >= _width || my < 0 || my >= _height) continue;
                            if (_grid[mx, my] < 0) continue;

                            int tile = _grid[mx, my];
                            int dir = (dx + 1) + (dy + 1) * 2;
                            for (int t = 0; t < _tileCount; t++)
                            {
                                if (!Contains(_rules[tile][dir], t))
                                    possible[t] = false;
                            }
                        }
                    }

                    int newPossible = -1;
                    for (int t = 0; t < _tileCount; t++)
                    {
                        if (possible[t])
                        {
                            if (newPossible < 0) newPossible = 0;
                            newPossible++;
                        }
                    }

                    if (newPossible > _possible[nx, ny])
                    {
                        _possible[nx, ny] = newPossible;
                        stack.Push((nx, ny));
                    }
                }
            }
        }

        private bool Contains(int[] array, int value)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == value) return true;
            return false;
        }

        public void Reset()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _grid[x, y] = -1;
                    _possible[x, y] = _tileCount - 1;
                }
            }
        }

        public void SetCell(int x, int y, int tile)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height) return;
            _grid[x, y] = tile;
            _possible[x, y] = tile;
        }
    }

    public class BSPNode
    {
        public BSPNode? Left { get; set; }
        public BSPNode? Right { get; set; }
        public Rect Bounds { get; set; }
        public bool IsLeaf => Left == null && Right == null;
        public int Depth { get; set; }
        public BSPNode? Parent { get; set; }
        public List<Vector2> Room { get; set; } = new List<Vector2>();
        public Vector2 RoomCenter { get; set; }
        public float RoomSize { get; set; }

        public BSPNode(Rect bounds, int depth = 0, BSPNode? parent = null)
        {
            Bounds = bounds;
            Depth = depth;
            Parent = parent;
        }

        public bool IsHorizontalSplit(float ratio = 0.45f)
        {
            return _random.NextDouble() < ratio;
        }

        private static Random _random = new Random();

        public static void SetSeed(int seed)
        {
            _random = new Random(seed);
        }
    }

    public class BSPTree
    {
        private readonly BSPNode _root;
        private readonly int _maxDepth;
        private readonly int _minWidth;
        private readonly int _minHeight;
        private readonly List<BSPNode> _leafNodes;

        public BSPNode Root => _root;
        public IReadOnlyList<BSPNode> LeafNodes => _leafNodes;

        public BSPTree(float x, float y, float width, float height, int maxDepth = 5, int minWidth = 10, int minHeight = 10)
        {
            _maxDepth = maxDepth;
            _minWidth = minWidth;
            _minHeight = minHeight;
            _leafNodes = new List<BSPNode>();
            _root = new BSPNode(new Rect(x, y, width, height));
            Split(_root);
            CollectLeaves(_root);
        }

        private void Split(BSPNode node)
        {
            if (node.Depth >= _maxDepth) return;

            float width = node.Bounds.Width;
            float height = node.Bounds.Height;

            if (width < _minWidth * 2 && height < _minHeight * 2) return;

            bool splitHorizontal;
            if (width < _minWidth * 2)
                splitHorizontal = true;
            else if (height < _minHeight * 2)
                splitHorizontal = false;
            else
                splitHorizontal = _random.NextDouble() < 0.5;

            if (splitHorizontal)
            {
                float split = (float)(_random.NextDouble() * 0.4 + 0.3);
                float splitY = node.Bounds.Y + height * split;

                node.Left = new BSPNode(new Rect(node.Bounds.X, node.Bounds.Y, width, height * split), node.Depth + 1, node);
                node.Right = new BSPNode(new Rect(node.Bounds.X, splitY, width, height * (1 - split)), node.Depth + 1, node);
            }
            else
            {
                float split = (float)(_random.NextDouble() * 0.4 + 0.3);
                float splitX = node.Bounds.X + width * split;

                node.Left = new BSPNode(new Rect(node.Bounds.X, node.Bounds.Y, width * split, height), node.Depth + 1, node);
                node.Right = new BSPNode(new Rect(splitX, node.Bounds.Y, width * (1 - split), height), node.Depth + 1, node);
            }

            Split(node.Left);
            Split(node.Right);
        }

        private void CollectLeaves(BSPNode node)
        {
            if (node.IsLeaf)
            {
                _leafNodes.Add(node);
            }
            else
            {
                CollectLeaves(node.Left!);
                CollectLeaves(node.Right!);
            }
        }

        public void GenerateRooms(float margin = 2, float maxSizeRatio = 0.8f)
        {
            foreach (var leaf in _leafNodes)
            {
                float marginX = leaf.Bounds.Width * (1 - maxSizeRatio) / 2;
                float marginY = leaf.Bounds.Height * (1 - maxSizeRatio) / 2;

                float roomX = leaf.Bounds.X + margin + marginX;
                float roomY = leaf.Bounds.Y + margin + marginY;
                float roomWidth = leaf.Bounds.Width - margin * 2 - marginX * 2;
                float roomHeight = leaf.Bounds.Height - margin * 2 - marginY * 2;

                if (roomWidth > margin * 2 && roomHeight > margin * 2)
                {
                    var room = new List<Vector2>
                    {
                        new Vector2(roomX, roomY),
                        new Vector2(roomX + roomWidth, roomY),
                        new Vector2(roomX + roomWidth, roomY + roomHeight),
                        new Vector2(roomX, roomY + roomHeight)
                    };

                    leaf.Room = room;
                    leaf.RoomCenter = new Vector2(roomX + roomWidth / 2, roomY + roomHeight / 2);
                    leaf.RoomSize = (roomWidth + roomHeight) / 2;
                }
            }
        }

        public List<(Vector2 from, Vector2 to)> GetCorridors()
        {
            var corridors = new List<(Vector2, Vector2)>();

            void ConnectToParent(BSPNode node)
            {
                if (node.Parent == null) return;

                var parent = node.Parent;
                Vector2 nodeCenter = node.RoomCenter;

                if (parent.Left == node)
                {
                    Vector2 siblingCenter = parent.Right?.RoomCenter ?? nodeCenter;
                    Vector2 mid = nodeCenter.Lerp(siblingCenter, 0.5f);
                    corridors.Add((nodeCenter, mid));
                    corridors.Add((mid, siblingCenter));
                }
                else
                {
                    Vector2 siblingCenter = parent.Left?.RoomCenter ?? nodeCenter;
                    Vector2 mid = nodeCenter.Lerp(siblingCenter, 0.5f);
                    corridors.Add((nodeCenter, mid));
                    corridors.Add((mid, siblingCenter));
                }

                ConnectToParent(parent);
            }

            foreach (var leaf in _leafNodes)
            {
                if (leaf.Room.Count > 0)
                    ConnectToParent(leaf);
            }

            return corridors.Distinct().ToList();
        }

        private static Random _random = new Random();

        public static void SetSeed(int seed)
        {
            _random = new Random(seed);
            BSPNode.SetSeed(seed);
        }
    }

    public class Rect
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;
        public Vector2 Center => new Vector2(X + Width / 2, Y + Height / 2);
    }

    public class GraphNode<T>
    {
        public T Value { get; set; }
        public int Id { get; set; }
        public Dictionary<int, float> Connections { get; set; } = new Dictionary<int, float>();

        public GraphNode(T value, int id)
        {
            Value = value;
            Id = id;
        }
    }

    public class Graph<T>
    {
        private readonly Dictionary<int, GraphNode<T>> _nodes = new Dictionary<int, GraphNode<T>>();
        private int _nextId;

        public IReadOnlyDictionary<int, GraphNode<T>> Nodes => _nodes;
        public int NodeCount => _nodes.Count;

        public GraphNode<T> AddNode(T value)
        {
            var node = new GraphNode<T>(value, _nextId++);
            _nodes[node.Id] = node;
            return node;
        }

        public void RemoveNode(int id)
        {
            if (!_nodes.ContainsKey(id)) return;

            foreach (var node in _nodes.Values)
            {
                node.Connections.Remove(id);
            }
            _nodes.Remove(id);
        }

        public void AddEdge(int fromId, int toId, float weight = 1.0f)
        {
            if (!_nodes.ContainsKey(fromId) || !_nodes.ContainsKey(toId)) return;
            _nodes[fromId].Connections[toId] = weight;
            _nodes[toId].Connections[fromId] = weight;
        }

        public void RemoveEdge(int fromId, int toId)
        {
            if (!_nodes.ContainsKey(fromId)) return;
            _nodes[fromId].Connections.Remove(toId);
            if (_nodes.ContainsKey(toId))
                _nodes[toId].Connections.Remove(fromId);
        }

        public bool HasEdge(int fromId, int toId)
        {
            return _nodes.ContainsKey(fromId) && _nodes[fromId].Connections.ContainsKey(toId);
        }

        public float GetEdgeWeight(int fromId, int toId)
        {
            if (!_nodes.ContainsKey(fromId) || !_nodes[fromId].Connections.ContainsKey(toId))
                return float.MaxValue;
            return _nodes[fromId].Connections[toId];
        }

        public List<int> GetNeighbors(int nodeId)
        {
            if (!_nodes.ContainsKey(nodeId)) return new List<int>();
            return _nodes[nodeId].Connections.Keys.ToList();
        }

        public List<(int from, int to, float weight)> GetEdges()
        {
            var edges = new List<(int, int, float)>();
            var added = new HashSet<string>();

            foreach (var node in _nodes.Values)
            {
                foreach (var connection in node.Connections)
                {
                    string key = connection.Key < node.Id
                        ? $"{connection.Key},{node.Id}"
                        : $"{node.Id},{connection.Key}";

                    if (!added.Contains(key))
                    {
                        added.Add(key);
                        edges.Add((node.Id, connection.Key, connection.Value));
                    }
                }
            }

            return edges;
        }

        public void Clear()
        {
            _nodes.Clear();
            _nextId = 0;
        }
    }

    public static class GraphUtils
    {
        public static List<int> Dijkstra<T>(Graph<T> graph, int startId, int endId)
        {
            var distances = new Dictionary<int, float>();
            var previous = new Dictionary<int, int>();
            var unvisited = new HashSet<int>();
            var priorityQueue = new PriorityQueue<int>();

            foreach (var nodeId in graph.Nodes.Keys)
            {
                distances[nodeId] = float.MaxValue;
                unvisited.Add(nodeId);
            }
            distances[startId] = 0;

            priorityQueue.Enqueue(startId, 0);

            while (unvisited.Count > 0 && priorityQueue.Count > 0)
            {
                var current = priorityQueue.Dequeue();
                if (!unvisited.Contains(current)) continue;

                if (current == endId) break;

                unvisited.Remove(current);

                foreach (var neighborId in graph.GetNeighbors(current))
                {
                    if (!unvisited.Contains(neighborId)) continue;

                    float weight = graph.GetEdgeWeight(current, neighborId);
                    float alt = distances[current] + weight;

                    if (alt < distances[neighborId])
                    {
                        distances[neighborId] = alt;
                        previous[neighborId] = current;
                        priorityQueue.Enqueue(neighborId, alt);
                    }
                }
            }

            if (!previous.ContainsKey(endId) && startId != endId)
                return new List<int>();

            var path = new List<int>();
            int currentNode = endId;
            while (currentNode != startId)
            {
                path.Add(currentNode);
                if (!previous.ContainsKey(currentNode)) break;
                currentNode = previous[currentNode];
            }
            path.Add(startId);
            path.Reverse();

            return path;
        }

        public static (List<int> path, float totalCost) AStar<T>(
            Graph<T> graph,
            int startId,
            int endId,
            Func<int, int, float> heuristic)
        {
            var openSet = new PriorityQueue<int>();
            var cameFrom = new Dictionary<int, int>();
            var gScore = new Dictionary<int, float>();
            var fScore = new Dictionary<int, float>();

            foreach (var nodeId in graph.Nodes.Keys)
            {
                gScore[nodeId] = float.MaxValue;
                fScore[nodeId] = float.MaxValue;
            }

            gScore[startId] = 0;
            fScore[startId] = heuristic(startId, endId);
            openSet.Enqueue(startId, fScore[startId]);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == endId)
                {
                    var path = ReconstructPath(cameFrom, current);
                    return (path, gScore[current]);
                }

                foreach (var neighborId in graph.GetNeighbors(current))
                {
                    float weight = graph.GetEdgeWeight(current, neighborId);
                    float tentativeGScore = gScore[current] + weight;

                    if (tentativeGScore < gScore[neighborId])
                    {
                        cameFrom[neighborId] = current;
                        gScore[neighborId] = tentativeGScore;
                        fScore[neighborId] = tentativeGScore + heuristic(neighborId, endId);
                        openSet.Enqueue(neighborId, fScore[neighborId]);
                    }
                }
            }

            return (new List<int>(), float.MaxValue);
        }

        private static List<int> ReconstructPath(Dictionary<int, int> cameFrom, int current)
        {
            var path = new List<int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        public static Graph<T> MinimumSpanningTree<T>(Graph<T> graph)
        {
            var mst = new Graph<T>();
            if (graph.Nodes.Count == 0) return mst;

            var startNode = graph.Nodes.Values.First();
            var mstNode = mst.AddNode(startNode.Value);
            var added = new HashSet<int> { startNode.Id };

            var edges = new List<(int from, int to, float weight)>();
            foreach (var node in graph.Nodes.Values)
            {
                foreach (var connection in node.Connections)
                {
                    edges.Add((node.Id, connection.Key, connection.Value));
                }
            }
            edges.Sort((a, b) => a.weight.CompareTo(b.weight));

            while (added.Count < graph.Nodes.Count)
            {
                (int from, int to, float weight) = edges.FirstOrDefault(e =>
                    (added.Contains(e.from) && !added.Contains(e.to)) ||
                    (!added.Contains(e.from) && added.Contains(e.to)));

                if (from == 0 && to == 0) break;

                if (!added.Contains(to))
                {
                    var node = graph.Nodes[to];
                    mst.AddNode(node.Value);
                    added.Add(to);
                }
                if (!added.Contains(from))
                {
                    var node = graph.Nodes[from];
                    mst.AddNode(node.Value);
                    added.Add(from);
                }

                mst.AddEdge(from, to, weight);
            }

            return mst;
        }

        public static List<List<int>> FindConnectedComponents<T>(Graph<T> graph)
        {
            var components = new List<List<int>>();
            var visited = new HashSet<int>();

            foreach (var nodeId in graph.Nodes.Keys)
            {
                if (visited.Contains(nodeId)) continue;

                var component = new List<int>();
                var stack = new Stack<int>();
                stack.Push(nodeId);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    if (visited.Contains(current)) continue;

                    visited.Add(current);
                    component.Add(current);

                    foreach (var neighbor in graph.GetNeighbors(current))
                    {
                        if (!visited.Contains(neighbor))
                            stack.Push(neighbor);
                    }
                }

                components.Add(component);
            }

            return components;
        }
    }

    public struct HexCoord
    {
        public int Q { get; set; }
        public int R { get; set; }

        public int S => -Q - R;

        public HexCoord(int q, int r)
        {
            Q = q;
            R = r;
        }

        public static HexCoord FromCube(int x, int y, int z)
        {
            return new HexCoord(x, z);
        }

        public static HexCoord FromOffset(int col, int row, bool oddq = false)
        {
            int q = col - (row - (row & 1)) / 2;
            int r = row;
            return new HexCoord(q, r);
        }

        public (int x, int y, int z) ToCube()
        {
            return (Q, -Q - R, R);
        }

        public (float x, float y) ToPixel(float size)
        {
            float x = size * ((float)Math.Sqrt(3) * Q + (float)Math.Sqrt(3) / 2 * R);
            float y = size * (3.0f / 2 * R);
            return (x, y);
        }

        public static HexCoord FromPixel(float x, float y, float size)
        {
            float q = ((float)Math.Sqrt(3) / 3 * x - 1.0f / 3 * y) / size;
            float r = (2.0f / 3 * y) / size;
            return RoundHex(q, r);
        }

        private static HexCoord RoundHex(float q, float r)
        {
            int s = -MathHelper.RoundToInt(q) - MathHelper.RoundToInt(r);
            float qDiff = Math.Abs(MathHelper.RoundToInt(q) - q);
            float rDiff = Math.Abs(MathHelper.RoundToInt(r) - r);
            float sDiff = Math.Abs(s + MathHelper.RoundToInt(q) + MathHelper.RoundToInt(r));

            if (qDiff > rDiff && qDiff > sDiff)
                return new HexCoord(-MathHelper.RoundToInt(r) - MathHelper.RoundToInt(r), MathHelper.RoundToInt(r));
            else if (rDiff > sDiff)
                return new HexCoord(MathHelper.RoundToInt(q), -MathHelper.RoundToInt(q) - s);
            else
                return new HexCoord(MathHelper.RoundToInt(q), MathHelper.RoundToInt(r));
        }

        public HexCoord[] GetNeighbors()
        {
            return new[]
            {
                new HexCoord(Q + 1, R),
                new HexCoord(Q + 1, R - 1),
                new HexCoord(Q, R - 1),
                new HexCoord(Q - 1, R),
                new HexCoord(Q - 1, R + 1),
                new HexCoord(Q, R + 1)
            };
        }

        public float DistanceTo(HexCoord other)
        {
            return (Math.Abs(Q - other.Q) + Math.Abs(R - other.R) + Math.Abs(S - other.S)) / 2;
        }

        public static float Distance(HexCoord a, HexCoord b)
        {
            return a.DistanceTo(b);
        }

        public List<HexCoord> GetRing(int radius)
        {
            if (radius == 0) return new List<HexCoord> { this };

            var results = new List<HexCoord>();
            var directions = new[]
            {
                new HexCoord(1, 0),
                new HexCoord(1, -1),
                new HexCoord(0, -1),
                new HexCoord(-1, 0),
                new HexCoord(-1, 1),
                new HexCoord(0, 1)
            };

            var hex = this + directions[4] * radius;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    results.Add(hex);
                    hex += directions[i];
                }
            }

            return results;
        }

        public List<HexCoord> GetSpiral(int radius)
        {
            var results = new List<HexCoord> { this };

            for (int r = 1; r <= radius; r++)
            {
                results.AddRange(GetRing(r));
            }

            return results;
        }

        public static HexCoord operator +(HexCoord a, HexCoord b)
        {
            return new HexCoord(a.Q + b.Q, a.R + b.R);
        }

        public static HexCoord operator -(HexCoord a, HexCoord b)
        {
            return new HexCoord(a.Q - b.Q, a.R - b.R);
        }

        public static HexCoord operator *(HexCoord a, int multiplier)
        {
            return new HexCoord(a.Q * multiplier, a.R * multiplier);
        }

        public override bool Equals(object? obj)
        {
            if (obj is HexCoord other)
                return Q == other.Q && R == other.R;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Q, R);
        }

        public override string ToString()
        {
            return $"({Q}, {R})";
        }
    }

    public class HexagonalGrid<T>
    {
        private readonly Dictionary<HexCoord, T> _cells = new Dictionary<HexCoord, T>();
        private readonly float _cellSize;
        private readonly Func<HexCoord, T> _defaultFactory;

        public int Count => _cells.Count;
        public float CellSize => _cellSize;
        public IReadOnlyDictionary<HexCoord, T> Cells => _cells;

        public HexagonalGrid(float cellSize, Func<HexCoord, T>? defaultFactory = null)
        {
            _cellSize = cellSize;
            _defaultFactory = defaultFactory ?? (_ => default!);
        }

        public T GetCell(HexCoord coord)
        {
            if (_cells.TryGetValue(coord, out var cell))
                return cell;

            var newCell = _defaultFactory(coord);
            _cells[coord] = newCell;
            return newCell;
        }

        public void SetCell(HexCoord coord, T value)
        {
            _cells[coord] = value;
        }

        public bool HasCell(HexCoord coord)
        {
            return _cells.ContainsKey(coord);
        }

        public void RemoveCell(HexCoord coord)
        {
            _cells.Remove(coord);
        }

        public List<HexCoord> GetNeighbors(HexCoord coord)
        {
            return coord.GetNeighbors().Where(HasCell).ToList();
        }

        public List<(HexCoord coord, T cell)> GetNeighborCells(HexCoord coord)
        {
            return GetNeighbors(coord).Select(c => (c, GetCell(c))).ToList();
        }

        public List<HexCoord> GetRing(HexCoord center, int radius)
        {
            return center.GetRing(radius).Where(HasCell).ToList();
        }

        public List<HexCoord> GetSpiral(HexCoord center, int radius)
        {
            return center.GetSpiral(radius).Where(HasCell).ToList();
        }

        public List<HexCoord> GetCellsInRect(HexCoord topLeft, int width, int height)
        {
            var result = new List<HexCoord>();

            for (int r = 0; r < height; r++)
            {
                int rOffset = r >> 1;
                for (int q = 0; q < width; q++)
                {
                    var coord = new HexCoord(topLeft.Q + q - rOffset, topLeft.R + r);
                    if (HasCell(coord))
                        result.Add(coord);
                }
            }

            return result;
        }

        public Vector2 HexToPixel(HexCoord coord)
        {
            var (x, y) = coord.ToPixel(_cellSize);
            return new Vector2(x, y);
        }

        public HexCoord PixelToHex(Vector2 pixel)
        {
            return HexCoord.FromPixel(pixel.X, pixel.Y, _cellSize);
        }

        public List<HexCoord> FindPath(HexCoord start, HexCoord end, Func<T, T, float>? costFunc = null, Func<HexCoord, HexCoord, bool>? passableFunc = null)
        {
            if (!HasCell(start) || !HasCell(end)) return new List<HexCoord>();

            costFunc ??= (_, _) => 1.0f;
            passableFunc ??= (_, _) => true;

            var openSet = new PriorityQueue<HexCoord>();
            var cameFrom = new Dictionary<HexCoord, HexCoord>();
            var gScore = new Dictionary<HexCoord, float>();
            var fScore = new Dictionary<HexCoord, float>();

            gScore[start] = 0;
            fScore[start] = HexCoord.Distance(start, end);
            openSet.Enqueue(start, fScore[start]);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current.Equals(end))
                    return ReconstructPath(cameFrom, current);

                foreach (var neighbor in current.GetNeighbors())
                {
                    if (!HasCell(neighbor) || !passableFunc(GetCell(current), GetCell(neighbor)))
                        continue;

                    float cost = costFunc(GetCell(current), GetCell(neighbor));
                    float tentativeG = gScore.GetValueOrDefault(current, float.MaxValue) + cost;

                    if (tentativeG < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + HexCoord.Distance(neighbor, end);
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }

            return new List<HexCoord>();
        }

        private List<HexCoord> ReconstructPath(Dictionary<HexCoord, HexCoord> cameFrom, HexCoord current)
        {
            var path = new List<HexCoord> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        public void Clear()
        {
            _cells.Clear();
        }
    }

    public class QuadTree<T> where T : class
    {
        private readonly Rect _bounds;
        private readonly int _maxObjects;
        private readonly int _maxLevels;
        private readonly int _level;
        private readonly List<(T obj, Rect bounds)> _objects;
        private QuadTree<T>?[] _nodes;

        public QuadTree(Rect bounds, int maxObjects = 10, int maxLevels = 5, int level = 0)
        {
            _bounds = bounds;
            _maxObjects = maxObjects;
            _maxLevels = maxLevels;
            _level = level;
            _objects = new List<(T, Rect)>();
            _nodes = new QuadTree<T>?[4];
        }

        public void Clear()
        {
            _objects.Clear();
            for (int i = 0; i < 4; i++)
            {
                _nodes[i]?.Clear();
                _nodes[i] = null;
            }
        }

        private void Split()
        {
            float subWidth = _bounds.Width / 2;
            float subHeight = _bounds.Height / 2;
            float x = _bounds.X;
            float y = _bounds.Y;

            _nodes[0] = new QuadTree<T>(new Rect(x + subWidth, y, subWidth, subHeight), _maxObjects, _maxLevels, _level + 1);
            _nodes[1] = new QuadTree<T>(new Rect(x, y, subWidth, subHeight), _maxObjects, _maxLevels, _level + 1);
            _nodes[2] = new QuadTree<T>(new Rect(x, y + subHeight, subWidth, subHeight), _maxObjects, _maxLevels, _level + 1);
            _nodes[3] = new QuadTree<T>(new Rect(x + subWidth, y + subHeight, subWidth, subHeight), _maxObjects, _maxLevels, _level + 1);
        }

        private int GetIndex(Rect rect)
        {
            int index = -1;
            float verticalMidpoint = _bounds.X + _bounds.Width / 2;
            float horizontalMidpoint = _bounds.Y + _bounds.Height / 2;

            bool topQuadrant = rect.Y < horizontalMidpoint && rect.Y + rect.Height < horizontalMidpoint;
            bool bottomQuadrant = rect.Y > horizontalMidpoint;

            if (rect.X < verticalMidpoint && rect.X + rect.Width < verticalMidpoint)
            {
                if (topQuadrant) index = 1;
                else if (bottomQuadrant) index = 2;
            }
            else if (rect.X > verticalMidpoint)
            {
                if (topQuadrant) index = 0;
                else if (bottomQuadrant) index = 3;
            }

            return index;
        }

        public void Insert(T obj, Rect bounds)
        {
            if (_nodes[0] != null)
            {
                int index = GetIndex(bounds);
                if (index != -1)
                {
                    _nodes[index]!.Insert(obj, bounds);
                    return;
                }
            }

            _objects.Add((obj, bounds));

            if (_objects.Count > _maxObjects && _level < _maxLevels)
            {
                if (_nodes[0] == null)
                    Split();

                int i = 0;
                while (i < _objects.Count)
                {
                    int index = GetIndex(_objects[i].bounds);
                    if (index != -1)
                    {
                        var objBounds = _objects[i];
                        _objects.RemoveAt(i);
                        _nodes[index]!.Insert(objBounds.obj, objBounds.bounds);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        public List<T> Retrieve(Rect bounds)
        {
            var result = new List<T>();
            RetrieveInternal(bounds, result);
            return result;
        }

        private void RetrieveInternal(Rect bounds, List<T> result)
        {
            foreach (var objBounds in _objects)
            {
                if (BoundsIntersects(bounds, objBounds.bounds))
                    result.Add(objBounds.obj);
            }

            if (_nodes[0] != null)
            {
                int index = GetIndex(bounds);
                if (index != -1)
                {
                    _nodes[index]!.RetrieveInternal(bounds, result);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (BoundsIntersects(bounds, _nodes[i]!._bounds))
                            _nodes[i]!.RetrieveInternal(bounds, result);
                    }
                }
            }
        }

        public List<T> RetrieveAll()
        {
            var result = new List<T>();
            RetrieveAllInternal(result);
            return result;
        }

        private void RetrieveAllInternal(List<T> result)
        {
            foreach (var objBounds in _objects)
                result.Add(objBounds.obj);

            if (_nodes[0] != null)
            {
                for (int i = 0; i < 4; i++)
                    _nodes[i]!.RetrieveAllInternal(result);
            }
        }

        private static bool BoundsIntersects(Rect a, Rect b)
        {
            return !(a.X + a.Width < b.X || b.X + b.Width < a.X ||
                     a.Y + a.Height < b.Y || b.Y + b.Height < a.Y);
        }
    }

    public class KDTree<T> where T : class
    {
        private readonly KDNode<T>? _root;
        private readonly Func<T, int, float> _getCoordinate;

        public KDTree(IEnumerable<T> points, Func<T, int, float> getCoordinate)
        {
            var pointList = points.ToList();
            _getCoordinate = getCoordinate;

            if (pointList.Count > 0)
                _root = BuildNode(pointList, 0, 0, pointList.Count);
        }

        private KDNode<T>? BuildNode(List<T> points, int depth, int start, int end)
        {
            if (start >= end) return null;

            int dim = depth % 2;
            int mid = (start + end) / 2;

            QuickSelect(points, start, end - 1, mid, dim);

            var node = new KDNode<T>(points[mid], dim);
            node.Left = BuildNode(points, depth + 1, start, mid);
            node.Right = BuildNode(points, depth + 1, mid + 1, end);

            return node;
        }

        private void QuickSelect(List<T> points, int left, int right, int k, int dim)
        {
            while (right > left)
            {
                int pivotIndex = Partition(points, left, right, k, dim);
                if (pivotIndex == k)
                    break;
                else if (pivotIndex > k)
                    right = pivotIndex - 1;
                else
                    left = pivotIndex + 1;
            }
        }

        private int Partition(List<T> points, int left, int right, int k, int dim)
        {
            T pivot = points[k];
            float pivotValue = _getCoordinate(pivot, dim);
            Swap(points, k, right);

            int storeIndex = left;
            for (int i = left; i < right; i++)
            {
                if (_getCoordinate(points[i], dim) < pivotValue)
                {
                    Swap(points, i, storeIndex);
                    storeIndex++;
                }
            }

            Swap(points, storeIndex, right);
            return storeIndex;
        }

        private void Swap(List<T> points, int i, int j)
        {
            (points[i], points[j]) = (points[j], points[i]);
        }

        public (T? nearest, float distance) FindNearest(Vector2 point)
        {
            if (_root == null)
                return (null, float.MaxValue);

            var best = new NearestData { Point = _root.Point, DistanceSquared = float.MaxValue };

            void Search(KDNode<T>? node, int depth)
            {
                if (node == null) return;

                float currentDist = DistanceSquared(node.Point, point);
                if (currentDist < best.DistanceSquared)
                {
                    best.Point = node.Point;
                    best.DistanceSquared = currentDist;
                }

                int dim = depth % 2;
                float coord = dim == 0 ? point.X : point.Y;
                float nodeCoord = dim == 0 ? _getCoordinate(node.Point, 0) : _getCoordinate(node.Point, 1);

                var first = nodeCoord < coord ? node.Left : node.Right;
                var second = nodeCoord < coord ? node.Right : node.Left;

                Search(first, depth + 1);

                float diff = Math.Abs(coord - nodeCoord);
                if (diff * diff < best.DistanceSquared)
                    Search(second, depth + 1);
            }

            Search(_root, 0);
            return (best.Point, (float)Math.Sqrt(best.DistanceSquared));
        }

        private float DistanceSquared(T a, Vector2 b)
        {
            float dx = _getCoordinate(a, 0) - b.X;
            float dy = _getCoordinate(a, 1) - b.Y;
            return dx * dx + dy * dy;
        }

        private class NearestData
        {
            public T? Point { get; set; }
            public float DistanceSquared { get; set; }
        }

        private class KDNode<U> where U : class
        {
            public U Point { get; }
            public int Dimension { get; }
            public KDNode<U>? Left { get; set; }
            public KDNode<U>? Right { get; set; }

            public KDNode(U point, int dimension)
            {
                Point = point;
                Dimension = dimension;
            }
        }
    }

    public class DelaunayTriangulation
    {
        private List<Vector2> _points = new List<Vector2>();
        private List<(int a, int b, int c)> _triangles = new List<(int, int, int)>();
        private readonly float _superTriangleMargin = 1000f;

        public IReadOnlyList<Vector2> Points => _points;
        public IReadOnlyList<(int a, int b, int c)> Triangles => _triangles;

        public void AddPoint(Vector2 point)
        {
            _points.Add(point);
        }

        public void AddPoints(IEnumerable<Vector2> points)
        {
            _points.AddRange(points);
        }

        public void Clear()
        {
            _points.Clear();
            _triangles.Clear();
        }

        public void Compute()
        {
            if (_points.Count < 3) return;

            float minX = _points.Min(p => p.X);
            float maxX = _points.Max(p => p.X);
            float minY = _points.Min(p => p.Y);
            float maxY = _points.Max(p => p.Y);

            float dx = maxX - minX;
            float dy = maxY - minY;
            float deltaMax = Math.Max(dx, dy);
            float midX = (minX + maxX) / 2;
            float midY = (minY + maxY) / 2;

            var p1 = new Vector2(midX - 2 * deltaMax, midY - deltaMax);
            var p2 = new Vector2(midX, midY + 2 * deltaMax);
            var p3 = new Vector2(midX + 2 * deltaMax, midY - deltaMax);

            _triangles.Clear();
            _triangles.Add((_points.Count, _points.Count + 1, _points.Count + 2));

            _points.Add(p1);
            _points.Add(p2);
            _points.Add(p3);

            for (int i = 0; i < _points.Count - 3; i++)
            {
                var point = _points[i];
                var badTriangles = new List<(int, int, int)>();

                foreach (var tri in _triangles)
                {
                    if (IsPointInCircumcircle(point, _points[tri.a], _points[tri.b], _points[tri.c]))
                    {
                        badTriangles.Add(tri);
                    }
                }

                var polygon = new List<(int, int)>();
                foreach (var tri in badTriangles)
                {
                    bool ab = badTriangles.Any(t => t != tri && EdgeShares(tri.a, tri.b, t));
                    bool bc = badTriangles.Any(t => t != tri && EdgeShares(tri.b, tri.c, t));
                    bool ca = badTriangles.Any(t => t != tri && EdgeShares(tri.c, tri.a, t));

                    if (!ab) polygon.Add((tri.a, tri.b));
                    if (!bc) polygon.Add((tri.b, tri.c));
                    if (!ca) polygon.Add((tri.c, tri.a));
                }

                _triangles.RemoveAll(t => badTriangles.Contains(t));

                foreach (var edge in polygon)
                {
                    _triangles.Add((edge.Item1, edge.Item2, i));
                }
            }

            _triangles.RemoveAll(t =>
                t.a >= _points.Count - 3 || t.b >= _points.Count - 3 || t.c >= _points.Count - 3);

            _points.RemoveRange(_points.Count - 3, 3);
        }

        private bool EdgeShares(int a, int b, (int a, int b, int c) tri)
        {
            return (tri.a == a && tri.b == b) || (tri.a == a && tri.c == b) ||
                   (tri.b == a && tri.c == b) || (tri.b == a && tri.a == b) ||
                   (tri.c == a && tri.b == b) || (tri.c == a && tri.a == b);
        }

        private bool IsPointInCircumcircle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            float ax = a.X - point.X;
            float ay = a.Y - point.Y;
            float bx = b.X - point.X;
            float by = b.Y - point.Y;
            float cx = c.X - point.X;
            float cy = c.Y - point.Y;

            float det = (ax * ax + ay * ay) * (bx * cy - cx * by) -
                        (bx * bx + by * by) * (ax * cy - cx * ay) +
                        (cx * cx + cy * cy) * (ax * by - bx * ay);

            return det > 0;
        }

        public List<Vector2> GetEdges()
        {
            var edges = new HashSet<(int, int)>();
            var result = new List<Vector2>();

            foreach (var tri in _triangles)
            {
                AddEdgeIfNotExists(edges, result, tri.a, tri.b);
                AddEdgeIfNotExists(edges, result, tri.b, tri.c);
                AddEdgeIfNotExists(edges, result, tri.c, tri.a);
            }

            return result;
        }

        private void AddEdgeIfNotExists(HashSet<(int, int)> edges, List<Vector2> result, int a, int b)
        {
            var edge = a < b ? (a, b) : (b, a);
            if (!edges.Contains(edge))
            {
                edges.Add(edge);
                result.Add(_points[edge.Item1]);
                result.Add(_points[edge.Item2]);
            }
        }
    }

    public class VoronoiDiagram
    {
        private List<Vector2> _sites = new List<Vector2>();
        private DelaunayTriangulation _delaunay = new DelaunayTriangulation();

        public IReadOnlyList<Vector2> Sites => _sites;

        public void SetSites(IEnumerable<Vector2> sites)
        {
            _sites = sites.ToList();
        }

        public List<List<Vector2>> ComputeCells(float minX, float minY, float maxX, float maxY)
        {
            _delaunay.Clear();
            _delaunay.AddPoints(_sites);
            _delaunay.Compute();

            var cells = new List<List<Vector2>>();
            var processed = new HashSet<int>();

            for (int i = 0; i < _sites.Count; i++)
            {
                if (processed.Contains(i)) continue;

                var cell = ComputeCellForSite(i, minX, minY, maxX, maxY);
                cells.Add(cell);
                processed.Add(i);
            }

            return cells;
        }

        private List<Vector2> ComputeCellForSite(int siteIndex, float minX, float minY, float maxX, float maxY)
        {
            var adjacentSites = new HashSet<int>();
            var triangles = _delaunay.Triangles;

            foreach (var tri in triangles)
            {
                if (tri.a == siteIndex || tri.b == siteIndex || tri.c == siteIndex)
                {
                    if (tri.a != siteIndex) adjacentSites.Add(tri.a);
                    if (tri.b != siteIndex) adjacentSites.Add(tri.b);
                    if (tri.c != siteIndex) adjacentSites.Add(tri.c);
                }
            }

            var halfPlanes = new List<(Vector2 point, Vector2 normal)>();

            foreach (var adjIndex in adjacentSites)
            {
                var adjSite = _sites[adjIndex];
                var site = _sites[siteIndex];
                var midpoint = (site + adjSite) * 0.5f;
                var direction = (adjSite - site).Normalized();
                var normal = new Vector2(-direction.Y, direction.X);
                halfPlanes.Add((midpoint, normal));
            }

            var boundingBox = new List<Vector2>
            {
                new Vector2(minX, minY),
                new Vector2(maxX, minY),
                new Vector2(maxX, maxY),
                new Vector2(minX, maxY)
            };

            var cell = SutherlandHodgmanCell(boundingBox, halfPlanes);
            return cell;
        }

        private List<Vector2> SutherlandHodgmanCell(List<Vector2> polygon, List<(Vector2 point, Vector2 normal)> halfPlanes)
        {
            var result = polygon;

            foreach (var (point, normal) in halfPlanes)
            {
                if (result.Count == 0) break;

                var input = new List<Vector2>(result);
                result.Clear();

                for (int i = 0; i < input.Count; i++)
                {
                    var current = input[i];
                    var previous = input[(i - 1 + input.Count) % input.Count];

                    float currentSide = (current - point).Dot(normal);
                    float previousSide = (previous - point).Dot(normal);

                    if (currentSide >= 0)
                    {
                        if (previousSide < 0)
                        {
                            var intersection = LineIntersectionPoint(previous, current, point, point + normal.Perpendicular());
                            if (intersection.HasValue)
                                result.Add(intersection.Value);
                        }
                        result.Add(current);
                    }
                    else if (previousSide >= 0)
                    {
                        var intersection = LineIntersectionPoint(previous, current, point, point + normal.Perpendicular());
                        if (intersection.HasValue)
                            result.Add(intersection.Value);
                    }
                }
            }

            return result;
        }

        private Vector2? LineIntersectionPoint(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float d = (p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X);
            if (Math.Abs(d) < 1e-10f) return null;

            float t = ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X)) / d;
            return new Vector2(p1.X + t * (p2.X - p1.X), p1.Y + t * (p2.Y - p1.Y));
        }

        public List<Vector2> GetEdges()
        {
            return _delaunay.GetEdges();
        }
    }

    public class Complex
    {
        public float Real { get; set; }
        public float Imaginary { get; set; }

        public Complex(float real = 0, float imaginary = 0)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public float Magnitude => (float)Math.Sqrt(Real * Real + Imaginary * Imaginary);
        public float MagnitudeSquared => Real * Real + Imaginary * Imaginary;
        public float Phase => (float)Math.Atan2(Imaginary, Real);

        public Complex Conjugate => new Complex(Real, -Imaginary);

        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
        }

        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(
                a.Real * b.Real - a.Imaginary * b.Imaginary,
                a.Real * b.Imaginary + a.Imaginary * b.Real
            );
        }

        public static Complex operator *(Complex a, float scalar)
        {
            return new Complex(a.Real * scalar, a.Imaginary * scalar);
        }

        public static Complex operator /(Complex a, Complex b)
        {
            float denom = b.Real * b.Real + b.Imaginary * b.Imaginary;
            return new Complex(
                (a.Real * b.Real + a.Imaginary * b.Imaginary) / denom,
                (a.Imaginary * b.Real - a.Real * b.Imaginary) / denom
            );
        }

        public static Complex operator /(Complex a, float scalar)
        {
            return new Complex(a.Real / scalar, a.Imaginary / scalar);
        }

        public static Complex operator -(Complex a)
        {
            return new Complex(-a.Real, -a.Imaginary);
        }

        public Complex Power(float exponent)
        {
            float r = Magnitude;
            float theta = Phase;
            float newR = (float)Math.Pow(r, exponent);
            float newTheta = theta * exponent;
            return new Complex(newR * (float)Math.Cos(newTheta), newR * (float)Math.Sin(newTheta));
        }

        public Complex Sqrt()
        {
            float r = Magnitude;
            float theta = Phase;
            float newR = (float)Math.Sqrt(r);
            float newTheta = theta / 2;
            return new Complex(newR * (float)Math.Cos(newTheta), newR * (float)Math.Sin(newTheta));
        }

        public Complex Exp()
        {
            float expReal = (float)Math.Exp(Real);
            return new Complex(expReal * (float)Math.Cos(Imaginary), expReal * (float)Math.Sin(Imaginary));
        }

        public Complex Log()
        {
            return new Complex((float)Math.Log(Magnitude), Phase);
        }

        public override string ToString()
        {
            if (Imaginary >= 0)
                return $"{Real} + {Imaginary}i";
            return $"{Real} - {-Imaginary}i";
        }
    }

    public class Polynomial
    {
        private readonly float[] _coefficients;

        public int Degree => _coefficients.Length - 1;
        public float[] Coefficients => _coefficients;

        public Polynomial(params float[] coefficients)
        {
            int degree = coefficients.Length - 1;
            while (degree > 0 && Math.Abs(coefficients[degree]) < 1e-10f)
                degree--;

            _coefficients = new float[degree + 1];
            for (int i = 0; i <= degree; i++)
                _coefficients[i] = coefficients[i];
        }

        public float Evaluate(float x)
        {
            float result = 0;
            float power = 1;
            for (int i = 0; i <= Degree; i++)
            {
                result += _coefficients[i] * power;
                power *= x;
            }
            return result;
        }

        public float EvaluateHorner(float x)
        {
            float result = _coefficients[Degree];
            for (int i = Degree - 1; i >= 0; i--)
            {
                result = result * x + _coefficients[i];
            }
            return result;
        }

        public Polynomial Derivative()
        {
            if (Degree == 0) return new Polynomial(0);

            var derivCoef = new float[Degree];
            for (int i = 0; i < Degree; i++)
            {
                derivCoef[i] = _coefficients[i + 1] * (i + 1);
            }
            return new Polynomial(derivCoef);
        }

        public Polynomial Integrate(float constant = 0)
        {
            var integCoef = new float[Degree + 2];
            integCoef[0] = constant;
            for (int i = 0; i <= Degree; i++)
            {
                integCoef[i + 1] = _coefficients[i] / (i + 1);
            }
            return new Polynomial(integCoef);
        }

        public Polynomial Add(Polynomial other)
        {
            int maxDegree = Math.Max(Degree, other.Degree);
            var result = new float[maxDegree + 1];

            for (int i = 0; i <= Degree; i++)
                result[i] += _coefficients[i];
            for (int i = 0; i <= other.Degree; i++)
                result[i] += other._coefficients[i];

            return new Polynomial(result);
        }

        public Polynomial Multiply(Polynomial other)
        {
            if (Degree == 0 || other.Degree == 0)
            {
                float value = Degree == 0 ? _coefficients[0] : other._coefficients[0];
                var result = new float[Degree + other.Degree + 1];
                for (int i = 0; i <= Degree + other.Degree; i++)
                    result[i] = value;
                return new Polynomial(result);
            }

            var product = new float[Degree + other.Degree + 1];
            for (int i = 0; i <= Degree; i++)
            {
                for (int j = 0; j <= other.Degree; j++)
                {
                    product[i + j] += _coefficients[i] * other._coefficients[j];
                }
            }
            return new Polynomial(product);
        }

        public Polynomial Scale(float scalar)
        {
            var scaled = new float[_coefficients.Length];
            for (int i = 0; i <= Degree; i++)
                scaled[i] = _coefficients[i] * scalar;
            return new Polynomial(scaled);
        }

        public List<float> SolveQuadratic()
        {
            if (Degree != 2) throw new InvalidOperationException("Must be a quadratic polynomial");

            float a = _coefficients[2];
            float b = _coefficients[1];
            float c = _coefficients[0];

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
                return new List<float>();
            if (discriminant == 0)
                return new List<float> { -b / (2 * a) };

            float sqrtD = (float)Math.Sqrt(discriminant);
            return new List<float>
            {
                (-b - sqrtD) / (2 * a),
                (-b + sqrtD) / (2 * a)
            };
        }

        public List<float> SolveCubic()
        {
            if (Degree != 3) throw new InvalidOperationException("Must be a cubic polynomial");

            float a = _coefficients[3];
            float b = _coefficients[2];
            float c = _coefficients[1];
            float d = _coefficients[0];

            float p = (3 * a * c - b * b) / (3 * a * a);
            float q = (2 * b * b * b - 9 * a * b * c + 27 * a * a * d) / (27 * a * a * a);

            float discriminant = q * q / 4 + p * p * p / 27;

            var roots = new List<float>();

            if (discriminant > 0)
            {
                float sqrtD = (float)Math.Sqrt(discriminant);
                float u = (float)Math.Cbrt(-q / 2 + sqrtD);
                float v = (float)Math.Cbrt(-q / 2 - sqrtD);
                roots.Add(u + v - b / (3 * a));
            }
            else if (discriminant == 0)
            {
                if (Math.Abs(q) < 1e-10f)
                {
                    roots.Add(0 - b / (3 * a));
                }
                else
                {
                    float u = (float)Math.Cbrt(-q / 2);
                    roots.Add(2 * u - b / (3 * a));
                    roots.Add(-u - b / (3 * a));
                }
            }
            else
            {
                float r = (float)Math.Sqrt(-p * p * p / 27);
                float theta = (float)Math.Acos(-q / (2 * r));
                float m = 2 * (float)Math.Sqrt(-p / 3);

                roots.Add(m * (float)Math.Cos(theta / 3) - b / (3 * a));
                roots.Add(m * (float)Math.Cos((theta + 2 * Math.PI) / 3) - b / (3 * a));
                roots.Add(m * (float)Math.Cos((theta + 4 * Math.PI) / 3) - b / (3 * a));
            }

            return roots;
        }

        public List<float> SolveQuartic()
        {
            if (Degree != 4) throw new InvalidOperationException("Must be a quartic polynomial");

            float a = _coefficients[4];
            float b = _coefficients[3] / a;
            float c = _coefficients[2] / a;
            float d = _coefficients[1] / a;
            float e = _coefficients[0] / a;

            float cb = c * b / 4;
            float p = c - 3 * b * b / 8;
            float q = d - b * c / 2 + b * b * b / 8;
            float r = e - b * d / 4 + b * b * c / 16 - 3 * b * b * b * b / 256;

            var roots = new List<float>();

            var cubicPoly = new Polynomial(1, p / 2, (p * p - 4 * r) / 16, -q * q / 64);
            var yValues = cubicPoly.SolveCubic();

            float y0 = yValues.FirstOrDefault();
            float sqrtY0 = (float)Math.Sqrt(Math.Max(0, y0));

            float[] pSqCoeff = { p / 2 + y0, -q / (2 * sqrtY0), r };

            float discriminant1 = pSqCoeff[1] * pSqCoeff[1] - 4 * pSqCoeff[0];
            if (discriminant1 >= 0)
            {
                float sqrtD1 = (float)Math.Sqrt(discriminant1);
                roots.Add((-pSqCoeff[1] + sqrtD1) / 2 - b / 4);
                roots.Add((-pSqCoeff[1] - sqrtD1) / 2 - b / 4);
            }

            float[] qSqCoeff = { p / 2 + y0, q / (2 * sqrtY0), r };
            float discriminant2 = qSqCoeff[1] * qSqCoeff[1] - 4 * qSqCoeff[0];
            if (discriminant2 >= 0)
            {
                float sqrtD2 = (float)Math.Sqrt(discriminant2);
                roots.Add((-qSqCoeff[1] + sqrtD2) / 2 - b / 4);
                roots.Add((-qSqCoeff[1] - sqrtD2) / 2 - b / 4);
            }

            return roots;
        }

        public override string ToString()
        {
            var terms = new List<string>();
            for (int i = Degree; i >= 0; i--)
            {
                if (Math.Abs(_coefficients[i]) < 1e-10f) continue;

                string term = "";
                if (i == 0)
                    term = $"{_coefficients[i]:F2}";
                else if (i == 1)
                    term = $"{_coefficients[i]:F2}x";
                else
                    term = $"{_coefficients[i]:F2}x^{i}";

                if (terms.Count > 0 && _coefficients[i] > 0)
                    terms.Add("+ " + term);
                else
                    terms.Add(term);
            }

            return terms.Count > 0 ? string.Join(" ", terms) : "0";
        }
    }
}
