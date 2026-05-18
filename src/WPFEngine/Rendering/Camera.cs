using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WPFEngine.ECS;
using WPFEngine.Scene;

namespace WPFEngine.Rendering
{
    public enum CameraType
    {
        Perspective,
        Orthographic
    }

    public enum CameraProjection
    {
        Orthographic,
        Perspective
    }

    public enum CameraClearFlags
    {
        Skybox,
        SolidColor,
        DepthOnly,
        Nothing
    }

    public class Camera : Component, IDisposable
    {
        private CameraType cameraType;
        private CameraProjection projection;
        private CameraClearFlags clearFlags;
        private Color backgroundColor;
        private float fieldOfView;
        private float orthographicSize;
        private float aspectRatio;
        private float nearClipPlane;
        private float farClipPlane;
        private float zoom;
        private Vector2 viewportOffset;
        private Rect viewportRect;
        private Rect cameraRect;
        private bool isMain;
        private int depth;
        private float depthValue;
        private bool allowHDR;
        private bool allowMSAA;
        private bool forceIntoRenderTexture;
        private float mipMapBias;
        private int targetTextureWidth;
        private int targetTextureHeight;
        private RenderTexture renderTexture;
        private Matrix2D viewMatrix;
        private Matrix2D projectionMatrix;
        private Matrix2D viewProjectionMatrix;
        private bool matricesDirty;
        private GameObject followTarget;
        private Vector2 followOffset;
        private float followSpeed;
        private bool useFollowSmoothing;
        private float followSmoothingTime;
        private Vector2 currentVelocity;
        private Vector2 targetPosition;
        private bool useSmoothFollow;
        private float smoothDampTime;
        private Vector2 smoothDampVelocity;
        private float rotation;
        private float currentRotation;
        private float rotationSpeed;
        private bool allowRotation;
        private bool useMousePan;
        private bool useKeyboardPan;
        private Vector2 panSpeed;
        private float zoomSpeed;
        private float minZoom;
        private float maxZoom;
        private bool useZoomBounds;
        private Rect panBounds;
        private bool usePanBounds;
        private List<GameObject> culledObjects;
        private Plane[] frustumPlanes;
        private float cullingDistance;
        private bool enableCulling;
        private LayerMask cullingMask;
        private LayerMask visibleLayers;
        private bool isOrthographicView;
        private float viewportWidth;
        private float viewportHeight;

        public event EventHandler<CameraEventArgs> ProjectionChanged;
        public event EventHandler<CameraEventArgs> ViewportChanged;
        public event EventHandler<CameraEventArgs> TargetChanged;
        public event EventHandler<CameraEventArgs> RenderStarted;
        public event EventHandler<CameraEventArgs> RenderCompleted;

        public CameraType CameraType
        {
            get => cameraType;
            set
            {
                if (cameraType != value)
                {
                    cameraType = value;
                    projection = value == CameraType.Orthographic ? 
                        CameraProjection.Orthographic : CameraProjection.Perspective;
                    InvalidateMatrices();
                    ProjectionChanged?.Invoke(this, new CameraEventArgs(this));
                }
            }
        }

        public CameraProjection Projection
        {
            get => projection;
            set
            {
                if (projection != value)
                {
                    projection = value;
                    cameraType = value == CameraProjection.Orthographic ? 
                        CameraType.Orthographic : CameraType.Perspective;
                    InvalidateMatrices();
                    ProjectionChanged?.Invoke(this, new CameraEventArgs(this));
                }
            }
        }

        public CameraClearFlags ClearFlags
        {
            get => clearFlags;
            set
            {
                if (clearFlags != value)
                {
                    clearFlags = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                if (backgroundColor != value)
                {
                    backgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public float FieldOfView
        {
            get => fieldOfView;
            set
            {
                var clampedFov = Math.Max(1f, Math.Min(179f, value));
                if (Math.Abs(fieldOfView - clampedFov) > 0.0001f)
                {
                    fieldOfView = clampedFov;
                    if (projection == CameraProjection.Perspective)
                    {
                        InvalidateMatrices();
                        OnPropertyChanged();
                    }
                }
            }
        }

        public float OrthographicSize
        {
            get => orthographicSize;
            set
            {
                var clampedSize = Math.Max(0.1f, value);
                if (Math.Abs(orthographicSize - clampedSize) > 0.0001f)
                {
                    orthographicSize = clampedSize;
                    if (projection == CameraProjection.Orthographic)
                    {
                        InvalidateMatrices();
                        OnPropertyChanged();
                    }
                }
            }
        }

        public float AspectRatio
        {
            get => aspectRatio;
            set
            {
                var clampedAspect = Math.Max(0.1f, value);
                if (Math.Abs(aspectRatio - clampedAspect) > 0.0001f)
                {
                    aspectRatio = clampedAspect;
                    InvalidateMatrices();
                    OnPropertyChanged();
                }
            }
        }

        public float NearClipPlane
        {
            get => nearClipPlane;
            set
            {
                if (nearClipPlane != value)
                {
                    nearClipPlane = value;
                    InvalidateMatrices();
                    OnPropertyChanged();
                }
            }
        }

        public float FarClipPlane
        {
            get => farClipPlane;
            set
            {
                if (farClipPlane != value)
                {
                    farClipPlane = value;
                    InvalidateMatrices();
                    OnPropertyChanged();
                }
            }
        }

        public float Zoom
        {
            get => zoom;
            set
            {
                var newZoom = Math.Max(0.01f, value);
                if (Math.Abs(zoom - newZoom) > 0.0001f)
                {
                    zoom = newZoom;
                    InvalidateMatrices();
                    OnPropertyChanged();
                }
            }
        }

        public Vector2 ViewportOffset
        {
            get => viewportOffset;
            set
            {
                if (viewportOffset != value)
                {
                    viewportOffset = value;
                    UpdateViewport();
                    OnPropertyChanged();
                }
            }
        }

        public Rect ViewportRect
        {
            get => viewportRect;
            set
            {
                if (viewportRect != value)
                {
                    viewportRect = value;
                    UpdateViewport();
                    OnPropertyChanged();
                }
            }
        }

        public Rect CameraRect => cameraRect;

        public bool IsMain
        {
            get => isMain;
            set
            {
                if (isMain != value)
                {
                    isMain = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Depth
        {
            get => depth;
            set
            {
                if (depth != value)
                {
                    depth = value;
                    depthValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public float DepthValue
        {
            get => depthValue;
            set
            {
                if (Math.Abs(depthValue - value) > 0.0001f)
                {
                    depthValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AllowHDR
        {
            get => allowHDR;
            set
            {
                if (allowHDR != value)
                {
                    allowHDR = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AllowMSAA
        {
            get => allowMSAA;
            set
            {
                if (allowMSAA != value)
                {
                    allowMSAA = value;
                    OnPropertyChanged();
                }
            }
        }

        public GameObject FollowTarget
        {
            get => followTarget;
            set
            {
                if (followTarget != value)
                {
                    followTarget = value;
                    TargetChanged?.Invoke(this, new CameraEventArgs(this));
                    OnPropertyChanged();
                }
            }
        }

        public Vector2 FollowOffset
        {
            get => followOffset;
            set
            {
                if (followOffset != value)
                {
                    followOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        public float FollowSpeed
        {
            get => followSpeed;
            set
            {
                if (Math.Abs(followSpeed - value) > 0.0001f)
                {
                    followSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseFollowSmoothing
        {
            get => useFollowSmoothing;
            set
            {
                if (useFollowSmoothing != value)
                {
                    useFollowSmoothing = value;
                    OnPropertyChanged();
                }
            }
        }

        public float FollowSmoothingTime
        {
            get => followSmoothingTime;
            set
            {
                if (Math.Abs(followSmoothingTime - value) > 0.0001f)
                {
                    followSmoothingTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Rotation
        {
            get => rotation;
            set
            {
                if (Math.Abs(rotation - value) > 0.0001f)
                {
                    rotation = value;
                    InvalidateMatrices();
                    OnPropertyChanged();
                }
            }
        }

        public float RotationSpeed
        {
            get => rotationSpeed;
            set
            {
                if (Math.Abs(rotationSpeed - value) > 0.0001f)
                {
                    rotationSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseSmoothFollow
        {
            get => useSmoothFollow;
            set
            {
                if (useSmoothFollow != value)
                {
                    useSmoothFollow = value;
                    OnPropertyChanged();
                }
            }
        }

        public float MinZoom
        {
            get => minZoom;
            set
            {
                if (Math.Abs(minZoom - value) > 0.0001f)
                {
                    minZoom = value;
                    OnPropertyChanged();
                }
            }
        }

        public float MaxZoom
        {
            get => maxZoom;
            set
            {
                if (Math.Abs(maxZoom - value) > 0.0001f)
                {
                    maxZoom = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseZoomBounds
        {
            get => useZoomBounds;
            set
            {
                if (useZoomBounds != value)
                {
                    useZoomBounds = value;
                    OnPropertyChanged();
                }
            }
        }

        public Rect PanBounds
        {
            get => panBounds;
            set
            {
                if (panBounds != value)
                {
                    panBounds = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UsePanBounds
        {
            get => usePanBounds;
            set
            {
                if (usePanBounds != value)
                {
                    usePanBounds = value;
                    OnPropertyChanged();
                }
            }
        }

        public float CullingDistance
        {
            get => cullingDistance;
            set
            {
                if (Math.Abs(cullingDistance - value) > 0.0001f)
                {
                    cullingDistance = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EnableCulling
        {
            get => enableCulling;
            set
            {
                if (enableCulling != value)
                {
                    enableCulling = value;
                    OnPropertyChanged();
                }
            }
        }

        public LayerMask CullingMask
        {
            get => cullingMask;
            set
            {
                if (cullingMask != value)
                {
                    cullingMask = value;
                    OnPropertyChanged();
                }
            }
        }

        public LayerMask VisibleLayers
        {
            get => visibleLayers;
            set
            {
                if (visibleLayers != value)
                {
                    visibleLayers = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ViewportWidth
        {
            get => (int)viewportWidth;
            set
            {
                if ((int)viewportWidth != value)
                {
                    viewportWidth = value;
                    UpdateAspectRatio();
                    OnPropertyChanged();
                }
            }
        }

        public int ViewportHeight
        {
            get => (int)viewportHeight;
            set
            {
                if ((int)viewportHeight != value)
                {
                    viewportHeight = value;
                    UpdateAspectRatio();
                    OnPropertyChanged();
                }
            }
        }

        public Matrix2D ViewMatrix => viewMatrix;
        public Matrix2D ProjectionMatrix => projectionMatrix;
        public Matrix2D ViewProjectionMatrix => viewProjectionMatrix;

        public Plane[] FrustumPlanes
        {
            get
            {
                UpdateMatricesIfNeeded();
                return frustumPlanes;
            }
        }

        public Camera()
        {
            cameraType = CameraType.Orthographic;
            projection = CameraProjection.Orthographic;
            clearFlags = CameraClearFlags.Skybox;
            backgroundColor = Colors.Black;
            fieldOfView = 60f;
            orthographicSize = 5f;
            aspectRatio = 16f / 9f;
            nearClipPlane = 0.1f;
            farClipPlane = 1000f;
            zoom = 1f;
            viewportOffset = Vector2.Zero;
            viewportRect = new Rect(0, 0, 1, 1);
            isMain = false;
            depth = 0;
            depthValue = 0;
            allowHDR = true;
            allowMSAA = false;
            forceIntoRenderTexture = false;
            mipMapBias = 0f;
            targetTextureWidth = 0;
            targetTextureHeight = 0;
            renderTexture = null;
            viewMatrix = Matrix2D.Identity;
            projectionMatrix = Matrix2D.Identity;
            viewProjectionMatrix = Matrix2D.Identity;
            matricesDirty = true;
            followTarget = null;
            followOffset = Vector2.Zero;
            followSpeed = 5f;
            useFollowSmoothing = true;
            followSmoothingTime = 0.1f;
            currentVelocity = Vector2.Zero;
            targetPosition = Vector2.Zero;
            useSmoothFollow = false;
            smoothDampTime = 0.3f;
            smoothDampVelocity = Vector2.Zero;
            rotation = 0f;
            currentRotation = 0f;
            rotationSpeed = 5f;
            allowRotation = false;
            useMousePan = false;
            useKeyboardPan = false;
            panSpeed = new Vector2(10f, 10f);
            zoomSpeed = 2f;
            minZoom = 0.1f;
            maxZoom = 10f;
            useZoomBounds = false;
            panBounds = new Rect(-1000, -1000, 2000, 2000);
            usePanBounds = false;
            culledObjects = new List<GameObject>();
            frustumPlanes = new Plane[4];
            cullingDistance = 1000f;
            enableCulling = true;
            cullingMask = -1;
            visibleLayers = -1;
            isOrthographicView = true;
            viewportWidth = 1920;
            viewportHeight = 1080;
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);

            if (Transform != null)
            {
                Transform.PositionChanged += OnTransformChanged;
                Transform.RotationChanged += OnTransformChanged;
            }

            UpdateAspectRatio();
            InvalidateMatrices();
        }

        private void OnTransformChanged(object sender, TransformEventArgs e)
        {
            InvalidateMatrices();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (followTarget != null)
            {
                UpdateFollow(deltaTime);
            }

            UpdateMatricesIfNeeded();
        }

        private void UpdateFollow(float deltaTime)
        {
            if (Transform == null || followTarget.Transform == null)
                return;

            var targetPos = followTarget.Transform.WorldPosition + followOffset;
            
            if (useSmoothFollow)
            {
                var smoothTime = followSmoothingTime > 0 ? followSmoothingTime : 0.1f;
                var newPosition = SmoothDamp(
                    Transform.WorldPosition, 
                    targetPos, 
                    ref smoothDampVelocity, 
                    smoothTime, 
                    deltaTime
                );
                Transform.LocalPosition = newPosition;
            }
            else if (followSpeed > 0)
            {
                var diff = targetPos - Transform.WorldPosition;
                var distance = diff.Magnitude;
                
                if (distance > 0.01f)
                {
                    var moveAmount = Math.Min(distance, followSpeed * deltaTime);
                    var direction = diff.Normalized;
                    Transform.LocalPosition += direction * moveAmount;
                }
            }
            else
            {
                Transform.LocalPosition = targetPos;
            }

            if (usePanBounds)
            {
                ClampToPanBounds();
            }
        }

        private Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 velocity, float smoothTime, float deltaTime)
        {
            var omega = 2f / smoothTime;
            var x = omega * deltaTime;
            var exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            
            var change = current - target;
            var temp = (velocity + omega * change) * deltaTime;
            velocity = (velocity - omega * temp) * exp;
            
            var output = target + (change + temp) * exp;
            return output;
        }

        private void ClampToPanBounds()
        {
            if (Transform == null)
                return;

            var pos = Transform.LocalPosition;
            pos.X = Math.Max(panBounds.X, Math.Min(panBounds.X + panBounds.Width, pos.X));
            pos.Y = Math.Max(panBounds.Y, Math.Min(panBounds.Y + panBounds.Height, pos.Y));
            Transform.LocalPosition = pos;
        }

        public void Pan(Vector2 direction, float speed)
        {
            if (Transform == null)
                return;

            var worldDir = Transform.TransformDirection(direction);
            Transform.Translate(worldDir * speed);
        }

        public void PanTo(Vector2 position)
        {
            if (Transform == null)
                return;

            Transform.LocalPosition = position;
        }

        public void ZoomIn(float amount)
        {
            var newZoom = zoom + amount;
            if (useZoomBounds)
            {
                newZoom = Math.Max(minZoom, Math.Min(maxZoom, newZoom));
            }
            Zoom = newZoom;
        }

        public void ZoomOut(float amount)
        {
            ZoomIn(-amount);
        }

        public void ZoomTo(float targetZoom)
        {
            var newZoom = targetZoom;
            if (useZoomBounds)
            {
                newZoom = Math.Max(minZoom, Math.Min(maxZoom, newZoom));
            }
            Zoom = newZoom;
        }

        public void Rotate(float angle)
        {
            Rotation += angle;
        }

        public void RotateAround(Vector2 point, float angle)
        {
            if (Transform == null)
                return;

            var currentPos = Transform.WorldPosition;
            var rotationMatrix = Matrix2D.CreateRotation(angle * (float)Math.PI / 180f);
            var offset = currentPos - point;
            var rotatedOffset = rotationMatrix.TransformPoint(offset);
            var newPos = point + rotatedOffset;
            
            Transform.LocalPosition = newPos;
            Rotation += angle;
        }

        public Vector2 WorldToScreenPoint(Vector2 worldPosition)
        {
            UpdateMatricesIfNeeded();
            
            var viewPos = viewMatrix.TransformPoint(worldPosition);
            var screenX = (viewPos.X + 1f) * 0.5f * viewportWidth * viewportRect.Width + viewportRect.X * viewportWidth;
            var screenY = (1f - (viewPos.Y + 1f) * 0.5f) * viewportHeight * viewportRect.Height + viewportRect.Y * viewportHeight;
            
            return new Vector2(screenX, screenY);
        }

        public Vector2 ScreenToWorldPoint(Vector2 screenPosition)
        {
            UpdateMatricesIfNeeded();
            
            var normX = (screenPosition.X - viewportRect.X * viewportWidth) / (viewportRect.Width * viewportWidth) * 2f - 1f;
            var normY = 1f - (screenPosition.Y - viewportRect.Y * viewportHeight) / (viewportRect.Height * viewportHeight) * 2f;
            
            var viewPos = new Vector2(normX, normY);
            var inverseVP = viewProjectionMatrix.Inverse();
            var worldPos = inverseVP.TransformPoint(viewPos);
            
            return worldPos;
        }

        public Vector2 WorldToViewportPoint(Vector2 worldPosition)
        {
            UpdateMatricesIfNeeded();
            
            var viewPos = viewMatrix.TransformPoint(worldPosition);
            return new Vector2(
                (viewPos.X + 1f) * 0.5f,
                (1f - (viewPos.Y + 1f) * 0.5f)
            );
        }

        public Vector2 ViewportToWorldPoint(Vector2 viewportPoint)
        {
            UpdateMatricesIfNeeded();
            
            var viewPos = new Vector2(
                viewportPoint.X * 2f - 1f,
                1f - viewportPoint.Y * 2f
            );
            
            var inverseVP = viewProjectionMatrix.Inverse();
            return inverseVP.TransformPoint(viewPos);
        }

        public Ray ScreenPointToRay(Vector2 screenPosition)
        {
            var worldPoint = ScreenToWorldPoint(screenPosition);
            var forward = GetForwardVector();
            return new Ray(worldPoint, forward);
        }

        public Vector2 GetForwardVector()
        {
            var angle = rotation * (float)Math.PI / 180f;
            return new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
        }

        public Vector2 GetRightVector()
        {
            var angle = rotation * (float)Math.PI / 180f;
            return new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
        }

        private void UpdateMatricesIfNeeded()
        {
            if (!matricesDirty)
                return;

            CalculateViewMatrix();
            CalculateProjectionMatrix();
            viewProjectionMatrix = projectionMatrix * viewMatrix;
            UpdateFrustumPlanes();
            
            matricesDirty = false;
        }

        private void CalculateViewMatrix()
        {
            if (Transform == null)
            {
                viewMatrix = Matrix2D.Identity;
                return;
            }

            var cameraPos = Transform.WorldPosition;
            var cameraRot = rotation;
            
            var rotationRad = -cameraRot * (float)Math.PI / 180f;
            var cos = (float)Math.Cos(rotationRad);
            var sin = (float)Math.Sin(rotationRad);
            
            viewMatrix = new Matrix2D
            {
                M11 = cos,
                M12 = sin,
                M21 = -sin,
                M22 = cos,
                M13 = -(cos * cameraPos.X + sin * cameraPos.Y),
                M23 = -(-sin * cameraPos.X + cos * cameraPos.Y)
            };
        }

        private void CalculateProjectionMatrix()
        {
            if (projection == CameraProjection.Orthographic)
            {
                var halfWidth = orthographicSize * aspectRatio * 0.5f / zoom;
                var halfHeight = orthographicSize * 0.5f / zoom;
                
                projectionMatrix = new Matrix2D
                {
                    M11 = 1f / halfWidth,
                    M22 = 1f / halfHeight,
                    M33 = 1f,
                    M13 = -cameraRect.X / halfWidth - 1f,
                    M23 = -cameraRect.Y / halfHeight - 1f
                };
            }
            else
            {
                var tanHalfFov = (float)Math.Tan(fieldOfView * 0.5f * Math.PI / 180f);
                var near = nearClipPlane;
                var far = farClipPlane;
                
                projectionMatrix = new Matrix2D
                {
                    M11 = 1f / (aspectRatio * tanHalfFov),
                    M22 = 1f / tanHalfFov,
                    M33 = -(far + near) / (far - near),
                    M34 = -1f,
                    M43 = -2f * far * near / (far - near)
                };
            }
        }

        private void UpdateFrustumPlanes()
        {
            frustumPlanes[0] = new Plane(new Vector2(1, 0), -orthographicSize * aspectRatio / zoom);
            frustumPlanes[1] = new Plane(new Vector2(-1, 0), -orthographicSize * aspectRatio / zoom);
            frustumPlanes[2] = new Plane(new Vector2(0, 1), -orthographicSize / zoom);
            frustumPlanes[3] = new Plane(new Vector2(0, -1), -orthographicSize / zoom);
        }

        private void UpdateAspectRatio()
        {
            if (viewportHeight > 0)
            {
                aspectRatio = viewportWidth / viewportHeight;
                InvalidateMatrices();
            }
        }

        private void UpdateViewport()
        {
            var x = viewportRect.X * viewportWidth;
            var y = viewportRect.Y * viewportHeight;
            var width = viewportRect.Width * viewportWidth;
            var height = viewportRect.Height * viewportHeight;
            cameraRect = new Rect(x, y, width, height);
            
            ViewportChanged?.Invoke(this, new CameraEventArgs(this));
        }

        private void InvalidateMatrices()
        {
            matricesDirty = true;
        }

        protected virtual void OnPropertyChanged()
        {
        }

        public bool IsInView(Vector2 worldPosition)
        {
            UpdateMatricesIfNeeded();
            
            foreach (var plane in frustumPlanes)
            {
                if (plane.GetDistanceToPoint(worldPosition) < 0)
                    return false;
            }
            return true;
        }

        public bool IsInView(Bounds bounds)
        {
            var corners = bounds.GetCorners();
            foreach (var corner in corners)
            {
                if (IsInView(corner))
                    return true;
            }
            return false;
        }

        public List<GameObject> GetObjectsInView(List<GameObject> allObjects)
        {
            culledObjects.Clear();
            
            foreach (var obj in allObjects)
            {
                if (obj == null || !obj.IsActiveInHierarchy)
                    continue;

                if (enableCulling)
                {
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        var bounds = renderer.GetWorldBounds();
                        if (IsInView(bounds))
                        {
                            culledObjects.Add(obj);
                        }
                    }
                    else if (obj.Transform != null && IsInView(obj.Transform.WorldPosition))
                    {
                        culledObjects.Add(obj);
                    }
                }
                else
                {
                    culledObjects.Add(obj);
                }
            }
            
            return culledObjects;
        }

        public void Render()
        {
            RenderStarted?.Invoke(this, new CameraEventArgs(this));
            
            RenderCompleted?.Invoke(this, new CameraEventArgs(this));
        }

        public void ForceOrthographic()
        {
            projection = CameraProjection.Orthographic;
            cameraType = CameraType.Orthographic;
            isOrthographicView = true;
            InvalidateMatrices();
            ProjectionChanged?.Invoke(this, new CameraEventArgs(this));
        }

        public void ForcePerspective()
        {
            projection = CameraProjection.Perspective;
            cameraType = CameraType.Perspective;
            isOrthographicView = false;
            InvalidateMatrices();
            ProjectionChanged?.Invoke(this, new CameraEventArgs(this));
        }

        public override string ToString()
        {
            return $"Camera(Projection={Projection}, Zoom={Zoom:F2}, OrthoSize={OrthographicSize:F2})";
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ProjectionChanged = null;
                ViewportChanged = null;
                TargetChanged = null;
                RenderStarted = null;
                RenderCompleted = null;
                renderTexture?.Dispose();
                culledObjects?.Clear();
            }
        }
    }

    public class CameraEventArgs : EventArgs
    {
        public Camera Camera { get; }

        public CameraEventArgs(Camera camera)
        {
            Camera = camera;
        }
    }

    public class Plane
    {
        public Vector2 Normal { get; }
        public float Distance { get; }

        public Plane(Vector2 normal, float distance)
        {
            Normal = normal;
            Distance = distance;
        }

        public float GetDistanceToPoint(Vector2 point)
        {
            return Normal.Dot(point) + Distance;
        }

        public bool GetSide(Vector2 point)
        {
            return GetDistanceToPoint(point) >= 0;
        }
    }

    public class Ray
    {
        public Vector2 Origin { get; }
        public Vector2 Direction { get; }

        public Ray(Vector2 origin, Vector2 direction)
        {
            Origin = origin;
            Direction = direction.Normalized;
        }

        public Vector2 GetPoint(float distance)
        {
            return Origin + Direction * distance;
        }

        public bool Intersects(Bounds bounds, out float distance)
        {
            distance = float.MaxValue;
            
            var invDir = new Vector2(1f / Direction.X, 1f / Direction.Y);
            
            var t1 = (bounds.Min.X - Origin.X) * invDir.X;
            var t2 = (bounds.Max.X - Origin.X) * invDir.X;
            var t3 = (bounds.Min.Y - Origin.Y) * invDir.Y;
            var t4 = (bounds.Max.Y - Origin.Y) * invDir.Y;
            
            var tmin = Math.Max(Math.Min(t1, t2), Math.Min(t3, t4));
            var tmax = Math.Min(Math.Max(t1, t2), Math.Max(t3, t4));
            
            if (tmax < 0 || tmin > tmax)
            {
                distance = float.MaxValue;
                return false;
            }
            
            distance = tmin > 0 ? tmin : tmax;
            return distance >= 0;
        }
    }

    public struct LayerMask
    {
        public int Value { get; }

        public LayerMask(int value)
        {
            Value = value;
        }

        public static LayerMask All => new LayerMask(-1);
        public static LayerMask None => new LayerMask(0);

        public static implicit operator int(LayerMask mask) => mask.Value;
        public static implicit operator LayerMask(int value) => new LayerMask(value);

        public bool Contains(int layer)
        {
            return (Value & (1 << layer)) != 0;
        }
    }

    public static class BoundsExtensions
    {
        public static Vector2[] GetCorners(this Bounds bounds)
        {
            var min = bounds.Min;
            var max = bounds.Max;
            
            return new[]
            {
                new Vector2(min.X, min.Y),
                new Vector2(max.X, min.Y),
                new Vector2(max.X, max.Y),
                new Vector2(min.X, max.Y)
            };
        }
    }

    public class RenderTexture : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Format Format { get; private set; }
        public bool IsCreated { get; private set; }

        public RenderTexture(int width, int height)
        {
            Width = width;
            Height = height;
            Format = Format.RGBA32;
            IsCreated = true;
        }

        public void Release()
        {
            IsCreated = false;
        }

        public void Dispose()
        {
            Release();
        }

        public enum Format
        {
            R8,
            RGB24,
            RGBA32,
            ARGB32
        }
    }
}
