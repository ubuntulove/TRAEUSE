using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WPFEngine.ECS;

namespace WPFEngine.Scene
{
    public class Transform : Component
    {
        private Vector2 localPosition;
        private Vector2 localScale;
        private float localRotation;
        private Vector2 anchoredPosition;
        private bool useAnchors;
        private Transform parent;
        private List<Transform> children;
        private Matrix2D localMatrix;
        private Matrix2D worldMatrix;
        private bool matricesDirty;
        private Vector2 cachedWorldPosition;
        private Vector2 cachedWorldScale;
        private float cachedWorldRotation;
        private Vector2 velocity;
        private Vector2 angularVelocity;
        private float dampeningPosition;
        private float dampeningRotation;

        public event EventHandler<TransformEventArgs> PositionChanged;
        public event EventHandler<TransformEventArgs> RotationChanged;
        public event EventHandler<TransformEventArgs> ScaleChanged;
        public event EventHandler<TransformEventArgs> HierarchyChanged;

        public Vector2 LocalPosition
        {
            get => localPosition;
            set
            {
                if (localPosition != value)
                {
                    localPosition = value;
                    InvalidateMatrices();
                    PositionChanged?.Invoke(this, new TransformEventArgs(this));
                    GameObject?.MarkDirty();
                }
            }
        }

        public Vector2 LocalScale
        {
            get => localScale;
            set
            {
                if (localScale != value)
                {
                    localScale = value;
                    InvalidateMatrices();
                    ScaleChanged?.Invoke(this, new TransformEventArgs(this));
                    GameObject?.MarkDirty();
                }
            }
        }

        public float LocalRotation
        {
            get => localRotation;
            set
            {
                var normalizedRotation = NormalizeAngle(value);
                if (Math.Abs(localRotation - normalizedRotation) > 0.0001f)
                {
                    localRotation = normalizedRotation;
                    InvalidateMatrices();
                    RotationChanged?.Invoke(this, new TransformEventArgs(this));
                    GameObject?.MarkDirty();
                }
            }
        }

        public Vector2 Position
        {
            get => WorldPosition;
            set
            {
                var worldPos = value;
                if (Parent != null)
                {
                    var inverseParentMatrix = Parent.WorldMatrix.Inverse();
                    worldPos = inverseParentMatrix.TransformPoint(value);
                }
                LocalPosition = worldPos;
            }
        }

        public Vector2 Scale
        {
            get => WorldScale;
            set
            {
                var worldScale = value;
                if (Parent != null)
                {
                    worldScale = new Vector2(
                        worldScale.X / Parent.WorldScale.X,
                        worldScale.Y / Parent.WorldScale.Y
                    );
                }
                LocalScale = worldScale;
            }
        }

        public float Rotation
        {
            get => WorldRotation;
            set
            {
                var worldRotation = value;
                if (Parent != null)
                {
                    worldRotation -= Parent.WorldRotation;
                }
                LocalRotation = worldRotation;
            }
        }

        public Vector2 WorldPosition
        {
            get
            {
                UpdateMatricesIfNeeded();
                return cachedWorldPosition;
            }
        }

        public Vector2 WorldScale
        {
            get
            {
                UpdateMatricesIfNeeded();
                return cachedWorldScale;
            }
        }

        public float WorldRotation
        {
            get
            {
                UpdateMatricesIfNeeded();
                return cachedWorldRotation;
            }
        }

        public Vector2 Right => new Vector2((float)Math.Cos(LocalRotation * Math.PI / 180), (float)Math.Sin(LocalRotation * Math.PI / 180));
        public Vector2 Up => new Vector2(-(float)Math.Sin(LocalRotation * Math.PI / 180), (float)Math.Cos(LocalRotation * Math.PI / 180));

        public Vector2 Forward => Vector2.Up;

        public Transform Parent
        {
            get => parent;
            set
            {
                if (parent != value)
                {
                    var oldParent = parent;
                    parent?.RemoveChild(this);
                    parent = value;
                    parent?.AddChild(this);
                    InvalidateMatrices();
                    NotifyHierarchyChanged();
                }
            }
        }

        public int ChildCount => children?.Count ?? 0;

        public ReadOnlyCollection<Transform> Children
        {
            get
            {
                if (children == null)
                    children = new List<Transform>();
                return children.AsReadOnly();
            }
        }

        public Matrix2D LocalMatrix
        {
            get
            {
                UpdateMatricesIfNeeded();
                return localMatrix;
            }
        }

        public Matrix2D WorldMatrix
        {
            get
            {
                UpdateMatricesIfNeeded();
                return worldMatrix;
            }
        }

        public Matrix2D LocalToWorldMatrix
        {
            get
            {
                UpdateMatricesIfNeeded();
                return worldMatrix;
            }
        }

        public Matrix2D WorldToLocalMatrix
        {
            get
            {
                UpdateMatricesIfNeeded();
                return worldMatrix.Inverse();
            }
        }

        public Vector2 Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        public Vector2 AngularVelocity
        {
            get => angularVelocity;
            set => angularVelocity = value;
        }

        public Transform()
        {
            localPosition = Vector2.Zero;
            localScale = Vector2.One;
            localRotation = 0f;
            children = new List<Transform>();
            matricesDirty = true;
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            if (GameObject?.Parent != null)
            {
                Parent = GameObject.Parent.Transform;
            }
        }

        public void Translate(Vector2 translation)
        {
            LocalPosition += translation;
        }

        public void Translate(float x, float y)
        {
            Translate(new Vector2(x, y));
        }

        public void Translate(Vector2 direction, float distance)
        {
            LocalPosition += direction.Normalized * distance;
        }

        public void Rotate(float angle)
        {
            LocalRotation += angle;
        }

        public void Rotate(float angle, Vector2 pivot)
        {
            var pivotWorld = TransformPoint(pivot);
            LocalRotation += angle;
            var newWorldPivot = TransformPoint(pivot);
            LocalPosition += pivotWorld - newWorldPivot;
        }

        public void ScaleBy(Vector2 factor)
        {
            LocalScale = new Vector2(LocalScale.X * factor.X, LocalScale.Y * factor.Y);
        }

        public void ScaleBy(float factor)
        {
            ScaleBy(new Vector2(factor, factor));
        }

        public void SetPositionAndRotation(Vector2 position, float rotation)
        {
            var positionChanged = localPosition != position;
            var rotationChanged = Math.Abs(localRotation - rotation) > 0.0001f;

            localPosition = position;
            localRotation = NormalizeAngle(rotation);

            if (positionChanged || rotationChanged)
            {
                InvalidateMatrices();
                if (positionChanged)
                    PositionChanged?.Invoke(this, new TransformEventArgs(this));
                if (rotationChanged)
                    RotationChanged?.Invoke(this, new TransformEventArgs(this));
                GameObject?.MarkDirty();
            }
        }

        public void LookAt(Vector2 target)
        {
            LookAt(target, Vector2.Up);
        }

        public void LookAt(Vector2 target, Vector2 worldUp)
        {
            var direction = target - WorldPosition;
            if (direction.Magnitude < 0.0001f)
                return;

            var angle = (float)Math.Atan2(direction.Y, direction.X) * 180f / (float)Math.PI;
            if (worldUp.Y < 0)
                angle = 180f - angle;
            Rotation = angle + 90f;
        }

        public Vector2 TransformPoint(Vector2 point)
        {
            return WorldMatrix.TransformPoint(point);
        }

        public Vector2 InverseTransformPoint(Vector2 point)
        {
            return WorldToLocalMatrix.TransformPoint(point);
        }

        public Vector2 TransformDirection(Vector2 direction)
        {
            return WorldMatrix.TransformDirection(direction);
        }

        public Vector2 InverseTransformDirection(Vector2 direction)
        {
            return WorldToLocalMatrix.TransformDirection(direction);
        }

        public Vector2 TransformVector(Vector2 vector)
        {
            return WorldMatrix.TransformVector(vector);
        }

        public Vector2 InverseTransformVector(Vector2 vector)
        {
            return WorldToLocalMatrix.TransformVector(vector);
        }

        public float GetAngle(Vector2 direction)
        {
            var normalized = direction.Normalized;
            return (float)Math.Atan2(normalized.Y, normalized.X) * 180f / (float)Math.PI;
        }

        public float GetAngle(Vector2 from, Vector2 to)
        {
            var diff = to - from;
            return GetAngle(diff);
        }

        public float GetSignedAngle(Vector2 to)
        {
            var angle = GetAngle(to);
            if (angle > 180f)
                angle -= 360f;
            return angle;
        }

        public void SetAsFirstSibling()
        {
            if (Parent == null)
                return;
            Parent.DetachChild(this);
            Parent.AttachChildAt(this, 0);
        }

        public void SetAsLastSibling()
        {
            if (Parent == null)
                return;
            Parent.DetachChild(this);
            Parent.AttachChildAt(this, Parent.ChildCount);
        }

        public void SetSiblingIndex(int index)
        {
            if (Parent == null)
                return;
            Parent.DetachChild(this);
            Parent.AttachChildAt(this, index);
        }

        public int GetSiblingIndex()
        {
            if (Parent == null)
                return -1;
            return Parent.Children.IndexOf(this);
        }

        public Transform GetChild(int index)
        {
            if (index < 0 || index >= ChildCount)
                return null;
            return children[index];
        }

        public Transform Find(string name)
        {
            if (GameObject?.Name == name)
                return this;

            foreach (var child in children)
            {
                var found = child.Find(name);
                if (found != null)
                    return found;
            }

            return null;
        }

        public Transform[] GetComponentsInChildren<T>() where T : Component
        {
            var results = new List<Transform>();
            CollectComponents<T>(results);
            return results.ToArray();
        }

        private void CollectComponents<T>(List<Transform> results) where T : Component
        {
            var component = GameObject?.GetComponent<T>();
            if (component != null)
                results.Add(this);

            foreach (var child in children)
            {
                child.CollectComponents<T>(results);
            }
        }

        internal void AttachChild(Transform child)
        {
            if (child == null || children.Contains(child))
                return;

            child.Parent = this;
            children.Add(child);
            child.InvalidateMatrices();
            NotifyHierarchyChanged();
        }

        internal void AttachChildAt(Transform child, int index)
        {
            if (child == null)
                return;

            index = Math.Max(0, Math.Min(index, children.Count));
            child.Parent = this;
            children.Insert(index, child);
            child.InvalidateMatrices();
            NotifyHierarchyChanged();
        }

        internal void DetachChild(Transform child)
        {
            if (child == null || !children.Contains(child))
                return;

            children.Remove(child);
            child.parent = null;
            child.InvalidateMatrices();
            NotifyHierarchyChanged();
        }

        public void DetachChildren()
        {
            foreach (var child in children.ToArray())
            {
                child.parent = null;
            }
            children.Clear();
            NotifyHierarchyChanged();
        }

        private void RemoveChild(Transform child)
        {
            if (child != null)
            {
                children.Remove(child);
            }
        }

        private void AddChild(Transform child)
        {
            if (child != null && !children.Contains(child))
            {
                children.Add(child);
            }
        }

        private void InvalidateMatrices()
        {
            matricesDirty = true;
            foreach (var child in children)
            {
                child?.InvalidateMatrices();
            }
        }

        private void UpdateMatricesIfNeeded()
        {
            if (!matricesDirty)
                return;

            CalculateLocalMatrix();
            CalculateWorldMatrix();
            matricesDirty = false;
        }

        private void CalculateLocalMatrix()
        {
            var rotationMatrix = Matrix2D.CreateRotation(localRotation * (float)Math.PI / 180f);
            var scaleMatrix = Matrix2D.CreateScale(localScale.X, localScale.Y);
            var translationMatrix = Matrix2D.CreateTranslation(localPosition.X, localPosition.Y);
            localMatrix = translationMatrix * rotationMatrix * scaleMatrix;
        }

        private void CalculateWorldMatrix()
        {
            if (parent != null)
            {
                worldMatrix = parent.WorldMatrix * localMatrix;
                cachedWorldPosition = worldMatrix.TransformPoint(Vector2.Zero);
                cachedWorldRotation = parent.cachedWorldRotation + localRotation;
                cachedWorldScale = new Vector2(
                    parent.cachedWorldScale.X * localScale.X,
                    parent.cachedWorldScale.Y * localScale.Y
                );
            }
            else
            {
                worldMatrix = localMatrix;
                cachedWorldPosition = localPosition;
                cachedWorldRotation = localRotation;
                cachedWorldScale = localScale;
            }

            cachedWorldRotation = NormalizeAngle(cachedWorldRotation);
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        private void NotifyHierarchyChanged()
        {
            HierarchyChanged?.Invoke(this, new TransformEventArgs(this));
            foreach (var child in children)
            {
                child?.NotifyHierarchyChanged();
            }
        }

        public Bounds GetWorldBounds()
        {
            var position = WorldPosition;
            var scale = WorldScale;
            return new Bounds(position, new Vector2(scale.X * 100, scale.Y * 100));
        }

        public Bounds GetLocalBounds()
        {
            return new Bounds(localPosition, new Vector2(localScale.X * 100, localScale.Y * 100));
        }

        public void SetWorldPosition(Vector2 position)
        {
            if (parent != null)
            {
                var inverseMatrix = parent.WorldToLocalMatrix;
                localPosition = inverseMatrix.TransformPoint(position);
            }
            else
            {
                localPosition = position;
            }
            InvalidateMatrices();
            PositionChanged?.Invoke(this, new TransformEventArgs(this));
        }

        public void SetWorldScale(Vector2 scale)
        {
            if (parent != null)
            {
                localScale = new Vector2(
                    scale.X / parent.cachedWorldScale.X,
                    scale.Y / parent.cachedWorldScale.Y
                );
            }
            else
            {
                localScale = scale;
            }
            InvalidateMatrices();
            ScaleChanged?.Invoke(this, new TransformEventArgs(this));
        }

        public void SetWorldRotation(float rotation)
        {
            if (parent != null)
            {
                localRotation = rotation - parent.cachedWorldRotation;
            }
            else
            {
                localRotation = rotation;
            }
            localRotation = NormalizeAngle(localRotation);
            InvalidateMatrices();
            RotationChanged?.Invoke(this, new TransformEventArgs(this));
        }

        public Vector2 GetForwardVector()
        {
            var angle = WorldRotation * (float)Math.PI / 180f;
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public Vector2 GetRightVector()
        {
            var angle = WorldRotation * (float)Math.PI / 180f;
            return new Vector2(-(float)Math.Sin(angle), (float)Math.Cos(angle));
        }

        public override string ToString()
        {
            return $"Transform(Position={WorldPosition}, Rotation={WorldRotation:F1}°, Scale={WorldScale})";
        }
    }

    public class TransformEventArgs : EventArgs
    {
        public Transform Transform { get; }

        public TransformEventArgs(Transform transform)
        {
            Transform = transform;
        }
    }

    public class Matrix2D
    {
        public float M11, M12, M13;
        public float M21, M22, M23;
        public float M31, M32, M33;

        public static Matrix2D Identity => new Matrix2D
        {
            M11 = 1; M22 = 1; M33 = 1
        };

        public Matrix2D()
        {
            SetIdentity();
        }

        public Matrix2D(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33)
        {
            M11 = m11; M12 = m12; M13 = m13;
            M21 = m21; M22 = m22; M23 = m23;
            M31 = m31; M32 = m32; M33 = m33;
        }

        public void SetIdentity()
        {
            M11 = 1; M12 = 0; M13 = 0;
            M21 = 0; M22 = 1; M23 = 0;
            M31 = 0; M32 = 0; M33 = 1;
        }

        public static Matrix2D CreateTranslation(float x, float y)
        {
            return new Matrix2D
            {
                M11 = 1, M12 = 0, M13 = x,
                M21 = 0, M22 = 1, M23 = y,
                M31 = 0, M32 = 0, M33 = 1
            };
        }

        public static Matrix2D CreateScale(float scaleX, float scaleY)
        {
            return new Matrix2D
            {
                M11 = scaleX, M12 = 0, M13 = 0,
                M21 = 0, M22 = scaleY, M23 = 0,
                M31 = 0, M32 = 0, M33 = 1
            };
        }

        public static Matrix2D CreateRotation(float angleInRadians)
        {
            var cos = (float)Math.Cos(angleInRadians);
            var sin = (float)Math.Sin(angleInRadians);
            return new Matrix2D
            {
                M11 = cos, M12 = -sin, M13 = 0,
                M21 = sin, M22 = cos, M23 = 0,
                M31 = 0, M32 = 0, M33 = 1
            };
        }

        public static Matrix2D CreateSkew(float skewX, float skewY)
        {
            return new Matrix2D
            {
                M11 = 1, M12 = (float)Math.Tan(skewX * Math.PI / 180), M13 = 0,
                M21 = (float)Math.Tan(skewY * Math.PI / 180), M22 = 1, M23 = 0,
                M31 = 0, M32 = 0, M33 = 1
            };
        }

        public static Matrix2D operator *(Matrix2D a, Matrix2D b)
        {
            return new Matrix2D
            {
                M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
                M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
                M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
            };
        }

        public Vector2 TransformPoint(Vector2 point)
        {
            return new Vector2(
                M11 * point.X + M12 * point.Y + M13,
                M21 * point.X + M22 * point.Y + M23
            );
        }

        public Vector2 TransformDirection(Vector2 direction)
        {
            return new Vector2(
                M11 * direction.X + M12 * direction.Y,
                M21 * direction.X + M22 * direction.Y
            );
        }

        public Vector2 TransformVector(Vector2 vector)
        {
            return new Vector2(
                M11 * vector.X + M12 * vector.Y,
                M21 * vector.X + M22 * vector.Y
            );
        }

        public float GetDeterminant()
        {
            return M11 * (M22 * M33 - M23 * M32) - M12 * (M21 * M33 - M23 * M31) + M13 * (M21 * M32 - M22 * M31);
        }

        public Matrix2D Inverse()
        {
            var det = GetDeterminant();
            if (Math.Abs(det) < 0.0001f)
                return Identity;

            var invDet = 1f / det;
            return new Matrix2D
            {
                M11 = (M22 * M33 - M23 * M32) * invDet,
                M12 = -(M12 * M33 - M13 * M32) * invDet,
                M13 = (M12 * M23 - M13 * M22) * invDet,
                M21 = -(M21 * M33 - M23 * M31) * invDet,
                M22 = (M11 * M33 - M13 * M31) * invDet,
                M23 = -(M11 * M23 - M13 * M21) * invDet,
                M31 = (M21 * M32 - M22 * M31) * invDet,
                M32 = -(M11 * M32 - M12 * M31) * invDet,
                M33 = (M11 * M22 - M12 * M21) * invDet
            };
        }

        public Matrix2D Transpose()
        {
            return new Matrix2D
            {
                M11 = M11, M12 = M21, M13 = M31,
                M21 = M12, M22 = M22, M23 = M32,
                M31 = M13, M32 = M23, M33 = M33
            };
        }

        public override string ToString()
        {
            return $"[{M11:F2}, {M12:F2}, {M13:F2}; {M21:F2}, {M22:F2}, {M23:F2}; {M31:F2}, {M32:F2}, {M33:F2}]";
        }
    }

    public class Bounds
    {
        public Vector2 Center { get; set; }
        public Vector2 Size { get; set; }

        public Vector2 Min
        {
            get => new Vector2(Center.X - Size.X / 2, Center.Y - Size.Y / 2);
            set
            {
                var max = Max;
                Center = new Vector2((value.X + max.X) / 2, (value.Y + max.Y) / 2);
                Size = new Vector2(max.X - value.X, max.Y - value.Y);
            }
        }

        public Vector2 Max
        {
            get => new Vector2(Center.X + Size.X / 2, Center.Y + Size.Y / 2);
            set
            {
                var min = Min;
                Center = new Vector2((value.X + min.X) / 2, (value.Y + min.Y) / 2);
                Size = new Vector2(value.X - min.X, value.Y - min.Y);
            }
        }

        public Vector2 MinMax[2] => new[] { Min, Max };

        public float XMin
        {
            get => Min.X;
            set
            {
                var newMin = Min;
                newMin.X = value;
                Min = newMin;
            }
        }

        public float YMin
        {
            get => Min.Y;
            set
            {
                var newMin = Min;
                newMin.Y = value;
                Min = newMin;
            }
        }

        public float XMax
        {
            get => Max.X;
            set
            {
                var newMax = Max;
                newMax.X = value;
                Max = newMax;
            }
        }

        public float YMax
        {
            get => Max.Y;
            set
            {
                var newMax = Max;
                newMax.Y = value;
                Max = newMax;
            }
        }

        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }

        public Bounds()
        {
            Center = Vector2.Zero;
            Size = Vector2.Zero;
        }

        public Bounds(Vector2 center, Vector2 size)
        {
            Center = center;
            Size = size;
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y;
        }

        public bool Contains(Bounds bounds)
        {
            return Contains(bounds.Min) && Contains(bounds.Max);
        }

        public Vector2 ClosestPoint(Vector2 point)
        {
            return new Vector2(
                Math.Max(Min.X, Math.Min(Max.X, point.X)),
                Math.Max(Min.Y, Math.Min(Max.Y, point.Y))
            );
        }

        public float GetDistance(Vector2 point)
        {
            var closest = ClosestPoint(point);
            return (point - closest).Magnitude;
        }

        public bool Intersects(Bounds other)
        {
            return Min.X <= other.Max.X && Max.X >= other.Min.X &&
                   Min.Y <= other.Max.Y && Max.Y >= other.Min.Y;
        }

        public void Encapsulate(Vector2 point)
        {
            var newMin = new Vector2(Math.Min(Min.X, point.X), Math.Min(Min.Y, point.Y));
            var newMax = new Vector2(Math.Max(Max.X, point.X), Math.Max(Max.Y, point.Y));
            Min = newMin;
            Max = newMax;
        }

        public void Encapsulate(Bounds bounds)
        {
            Encapsulate(bounds.Min);
            Encapsulate(bounds.Max);
        }

        public void Expand(float amount)
        {
            Size = new Vector2(Size.X + amount * 2, Size.Y + amount * 2);
        }

        public void Expand(Vector2 amount)
        {
            Size = new Vector2(Size.X + amount.X * 2, Size.Y + amount.Y * 2);
        }

        public override string ToString() => $"Bounds(Center={Center}, Size={Size})";
    }
}
