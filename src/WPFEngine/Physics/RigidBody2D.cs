using System;
using System.Collections.Generic;
using WPFEngine.ECS;
using WPFEngine.Scene;

namespace WPFEngine.Physics
{
    public enum RigidBodyType
    {
        Static,
        Dynamic,
        Kinematic
    }

    public enum InterpolationMode
    {
        None,
        Interpolate,
        Extrapolate
    }

    public enum CollisionDetection
    {
        Discrete,
        Continuous
    }

    public class RigidBody2D : Component, IDisposable
    {
        private RigidBodyType bodyType;
        private float mass;
        private float drag;
        private float angularDrag;
        private float gravityScale;
        private bool useGravity;
        private bool isKinematic;
        private Vector2 velocity;
        private Vector2 angularVelocity;
        private Vector2 force;
        private Vector2 torque;
        private float maxVelocity;
        private float maxAngularVelocity;
        private float sleepThreshold;
        private float wakeForce;
        private bool autoSleep;
        private bool isAwake;
        private bool mustUpdateAngularVelocity;
        private float inertia;
        private float inverseInertia;
        private float area;
        private float density;
        private Vector2 worldCenterOfMass;
        private Vector2 localCenterOfMass;
        private float rotation;
        private Vector2 position;
        private InterpolationMode interpolation;
        private CollisionDetection collisionDetection;
        private List<Collider2D> attachedColliders;
        private Vector2 previousPosition;
        private float previousRotation;
        private Vector2 interpolatedPosition;
        private float interpolatedRotation;
        private bool freezePositionX;
        private bool freezePositionY;
        private bool freezeRotation;
        private Vector2 constantForce;
        private float constantTorque;
        private Vector2 externalForce;
        private Vector2 externalTorque;
        private int layerIndex;
        private string layerName;
        private float charge;
        private float maxCharge;
        private bool isElectricConductor;
        private float temperature;
        private bool useTemperature;
        private float thermalConductivity;
        private Dictionary<string, object> customData;

        public event EventHandler<RigidBodyEventArgs> VelocityChanged;
        public event EventHandler<RigidBodyEventArgs> ForceApplied;
        public event EventHandler<RigidBodyEventArgs> CollisionDetected;
        public event EventHandler<RigidBodyEventArgs> SleepStateChanged;
        public event EventHandler<RigidBodyEventArgs> WakeUp;
        public event EventHandler<RigidBodyEventArgs> GoToSleep;

        public RigidBodyType BodyType
        {
            get => bodyType;
            set
            {
                if (bodyType != value)
                {
                    bodyType = value;
                    isKinematic = value == RigidBodyType.Kinematic;
                    OnBodyTypeChanged();
                }
            }
        }

        public float Mass
        {
            get => mass;
            set
            {
                var newMass = Math.Max(0.0001f, value);
                if (Math.Abs(mass - newMass) > 0.0001f)
                {
                    mass = newMass;
                    UpdateInverseMass();
                }
            }
        }

        public float Drag
        {
            get => drag;
            set => drag = Math.Max(0f, value);
        }

        public float AngularDrag
        {
            get => angularDrag;
            set => angularDrag = Math.Max(0f, value);
        }

        public float GravityScale
        {
            get => gravityScale;
            set
            {
                if (Math.Abs(gravityScale - value) > 0.0001f)
                {
                    gravityScale = value;
                    OnGravityChanged();
                }
            }
        }

        public bool UseGravity
        {
            get => useGravity;
            set
            {
                if (useGravity != value)
                {
                    useGravity = value;
                    OnGravityChanged();
                }
            }
        }

        public bool IsKinematic
        {
            get => isKinematic;
            set
            {
                if (isKinematic != value)
                {
                    isKinematic = value;
                    if (value)
                        WakeUpBody();
                }
            }
        }

        public Vector2 Velocity
        {
            get => velocity;
            set
            {
                if (velocity != value)
                {
                    velocity = value;
                    WakeUpBody();
                    VelocityChanged?.Invoke(this, new RigidBodyEventArgs(this));
                }
            }
        }

        public Vector2 AngularVelocity
        {
            get => angularVelocity;
            set
            {
                if (angularVelocity != value)
                {
                    angularVelocity = value;
                    WakeUpBody();
                    VelocityChanged?.Invoke(this, new RigidBodyEventArgs(this));
                }
            }
        }

        public float MaxVelocity
        {
            get => maxVelocity;
            set => maxVelocity = Math.Max(0f, value);
        }

        public float MaxAngularVelocity
        {
            get => maxAngularVelocity;
            set => maxAngularVelocity = Math.Max(0f, value);
        }

        public float SleepThreshold
        {
            get => sleepThreshold;
            set => sleepThreshold = Math.Max(0f, value);
        }

        public bool AutoSleep
        {
            get => autoSleep;
            set => autoSleep = value;
        }

        public bool IsAwake
        {
            get => isAwake;
            set
            {
                if (isAwake != value)
                {
                    isAwake = value;
                    if (value)
                        WakeUpBody();
                    else
                        SleepBody();
                }
            }
        }

        public float Inertia
        {
            get => inertia;
            set
            {
                inertia = Math.Max(0f, value);
                inverseInertia = inertia > 0f ? 1f / inertia : 0f;
            }
        }

        public float InverseInertia => inverseInertia;

        public float Density
        {
            get => density;
            set => density = Math.Max(0f, value);
        }

        public Vector2 WorldCenterOfMass
        {
            get => worldCenterOfMass;
            private set => worldCenterOfMass = value;
        }

        public Vector2 LocalCenterOfMass
        {
            get => localCenterOfMass;
            set => localCenterOfMass = value;
        }

        public float Rotation
        {
            get => rotation;
            set
            {
                var normalizedRotation = NormalizeAngle(value);
                if (Math.Abs(rotation - normalizedRotation) > 0.0001f)
                {
                    rotation = normalizedRotation;
                    UpdateWorldCenterOfMass();
                }
            }
        }

        public Vector2 Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    UpdateWorldCenterOfMass();
                    if (Transform != null)
                        Transform.LocalPosition = value;
                }
            }
        }

        public InterpolationMode Interpolation
        {
            get => interpolation;
            set => interpolation = value;
        }

        public CollisionDetection CollisionDetectionMode
        {
            get => collisionDetection;
            set => collisionDetection = value;
        }

        public bool FreezePositionX
        {
            get => freezePositionX;
            set => freezePositionX = value;
        }

        public bool FreezePositionY
        {
            get => freezePositionY;
            set => freezePositionY = value;
        }

        public bool FreezeRotation
        {
            get => freezeRotation;
            set => freezeRotation = value;
        }

        public Vector2 ConstantForce
        {
            get => constantForce;
            set => constantForce = value;
        }

        public float ConstantTorque
        {
            get => constantTorque;
            set => constantTorque = value;
        }

        public int LayerIndex
        {
            get => layerIndex;
            set => layerIndex = value;
        }

        public string LayerName
        {
            get => layerName;
            set => layerName = value;
        }

        public IReadOnlyList<Collider2D> AttachedColliders => attachedColliders.AsReadOnly();

        public float KineticEnergy => 0.5f * mass * velocity.SqrMagnitude + 0.5f * inertia * angularVelocity.SqrMagnitude;

        public float PotentialEnergy
        {
            get
            {
                if (!useGravity)
                    return 0f;
                return mass * PhysicsWorld.Gravity.Y * position.Y;
            }
        }

        public float TotalEnergy => KineticEnergy + PotentialEnergy;

        public RigidBody2D()
        {
            bodyType = RigidBodyType.Dynamic;
            mass = 1f;
            drag = 0f;
            angularDrag = 0.05f;
            gravityScale = 1f;
            useGravity = true;
            isKinematic = false;
            velocity = Vector2.Zero;
            angularVelocity = Vector2.Zero;
            force = Vector2.Zero;
            torque = Vector2.Zero;
            maxVelocity = 100f;
            maxAngularVelocity = 360f * 10f;
            sleepThreshold = 0.005f;
            wakeForce = 0.001f;
            autoSleep = true;
            isAwake = true;
            mustUpdateAngularVelocity = false;
            inertia = 1f;
            inverseInertia = 1f;
            area = 1f;
            density = 1f;
            worldCenterOfMass = Vector2.Zero;
            localCenterOfMass = Vector2.Zero;
            rotation = 0f;
            position = Vector2.Zero;
            interpolation = InterpolationMode.None;
            collisionDetection = CollisionDetection.Discrete;
            attachedColliders = new List<Collider2D>();
            previousPosition = Vector2.Zero;
            previousRotation = 0f;
            interpolatedPosition = Vector2.Zero;
            interpolatedRotation = 0f;
            freezePositionX = false;
            freezePositionY = false;
            freezeRotation = false;
            constantForce = Vector2.Zero;
            constantTorque = 0f;
            layerIndex = 0;
            layerName = "Default";
            charge = 0f;
            maxCharge = 100f;
            isElectricConductor = false;
            temperature = 20f;
            useTemperature = false;
            thermalConductivity = 1f;
            customData = new Dictionary<string, object>();
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);

            if (Transform != null)
            {
                position = Transform.LocalPosition;
                rotation = Transform.LocalRotation;
                previousPosition = position;
                previousRotation = rotation;
                interpolatedPosition = position;
                interpolatedRotation = rotation;
                Transform.PositionChanged += OnTransformPositionChanged;
                Transform.RotationChanged += OnTransformRotationChanged;
            }

            UpdateWorldCenterOfMass();
            UpdateMassFromColliders();
        }

        private void OnTransformPositionChanged(object sender, TransformEventArgs e)
        {
            if (Transform != null)
            {
                position = Transform.LocalPosition;
            }
        }

        private void OnTransformRotationChanged(object sender, TransformEventArgs e)
        {
            if (Transform != null)
            {
                rotation = Transform.LocalRotation;
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!isAwake && autoSleep)
                return;

            if (interpolation == InterpolationMode.Interpolate)
            {
                UpdateInterpolation(deltaTime);
            }

            ApplyForces(deltaTime);

            if (bodyType == RigidBodyType.Dynamic && !isKinematic)
            {
                IntegrateVelocity(deltaTime);
                IntegratePosition(deltaTime);
            }

            UpdateAngularVelocity();
            ApplyDrag(deltaTime);
            CheckSleepCondition(deltaTime);
            UpdateTransform();
        }

        private void UpdateInterpolation(float deltaTime)
        {
            var t = 1f;
            interpolatedPosition = Vector2.Lerp(previousPosition, position, t);
            interpolatedRotation = Mathf.LerpAngle(previousRotation, rotation, t);
        }

        private void ApplyForces(float deltaTime)
        {
            if (bodyType != RigidBodyType.Dynamic || isKinematic)
                return;

            if (useGravity)
            {
                var gravityForce = PhysicsWorld.Gravity * mass * gravityScale;
                force += gravityForce;
            }

            force += constantForce;
            torque += constantTorque;

            var acceleration = force / mass;
            velocity += acceleration * deltaTime;

            var angularAcceleration = torque / inertia;
            angularVelocity += angularAcceleration * deltaTime;
        }

        private void IntegrateVelocity(float deltaTime)
        {
            if (freezePositionX)
                velocity.X = 0f;
            if (freezePositionY)
                velocity.Y = 0f;
            if (freezeRotation)
                angularVelocity = Vector2.Zero;

            var speed = velocity.Magnitude;
            if (speed > maxVelocity)
            {
                velocity = velocity.Normalized * maxVelocity;
            }

            var angularSpeed = Math.Abs(angularVelocity.Magnitude);
            if (angularSpeed > maxAngularVelocity)
            {
                angularVelocity = angularVelocity.Normalized * maxAngularVelocity;
            }
        }

        private void IntegratePosition(float deltaTime)
        {
            position += velocity * deltaTime;
            rotation += angularVelocity.X * deltaTime;
            rotation = NormalizeAngle(rotation);
        }

        private void UpdateAngularVelocity()
        {
            if (mustUpdateAngularVelocity)
            {
                angularVelocity = new Vector2(rotation - previousRotation, 0);
                mustUpdateAngularVelocity = false;
            }
        }

        private void ApplyDrag(float deltaTime)
        {
            if (drag > 0)
            {
                var dragForce = -velocity.Normalized * drag * velocity.Magnitude;
                velocity += dragForce * deltaTime;
            }

            if (angularDrag > 0)
            {
                var angularDragTorque = -angularVelocity.Normalized * angularDrag * angularVelocity.Magnitude;
                angularVelocity += angularDragTorque * deltaTime;
            }
        }

        private void CheckSleepCondition(float deltaTime)
        {
            if (!autoSleep || bodyType != RigidBodyType.Dynamic)
                return;

            var speed = velocity.Magnitude;
            var angularSpeed = Math.Abs(angularVelocity.Magnitude);

            if (speed < sleepThreshold && angularSpeed < sleepThreshold)
            {
                SleepBody();
            }
            else
            {
                WakeUpBody();
            }
        }

        private void UpdateTransform()
        {
            if (Transform != null)
            {
                Transform.LocalPosition = position;
                Transform.LocalRotation = rotation;
            }

            previousPosition = position;
            previousRotation = rotation;
        }

        public void AddForce(Vector2 force)
        {
            this.force += force;
            WakeUpBody();
            ForceApplied?.Invoke(this, new RigidBodyEventArgs(this));
        }

        public void AddForce(Vector2 force, Vector2 point)
        {
            this.force += force;
            var torqueAmount = (point - worldCenterOfMass).Cross(force);
            torque += new Vector2(torqueAmount, 0);
            WakeUpBody();
            ForceApplied?.Invoke(this, new RigidBodyEventArgs(this));
        }

        public void AddForceAtPosition(Vector2 force, Vector2 position)
        {
            AddForce(force, position);
        }

        public void AddRelativeForce(Vector2 force)
        {
            if (Transform == null)
                return;

            var worldForce = Transform.TransformDirection(force);
            AddForce(worldForce);
        }

        public void AddTorque(float torque)
        {
            this.torque += new Vector2(torque, 0);
            WakeUpBody();
            ForceApplied?.Invoke(this, new RigidBodyEventArgs(this));
        }

        public void AddTorque(Vector2 torque)
        {
            this.torque += torque;
            WakeUpBody();
            ForceApplied?.Invoke(this, new RigidBodyEventArgs(this));
        }

        public void ClearForce()
        {
            force = Vector2.Zero;
        }

        public void ClearTorque()
        {
            torque = Vector2.Zero;
        }

        public void ClearForces()
        {
            ClearForce();
            ClearTorque();
        }

        public void SetVelocity(Vector2 velocity)
        {
            this.velocity = velocity;
            WakeUpBody();
            VelocityChanged?.Invoke(this, new RigidBodyEventArgs(this));
        }

        public void SetVelocity(float x, float y)
        {
            SetVelocity(new Vector2(x, y));
        }

        public void SetAngularVelocity(float angularVel)
        {
            angularVelocity = new Vector2(angularVel, 0);
            WakeUpBody();
            VelocityChanged?.Invoke(this, new RigidBodyEventArgs(this));
        }

        public void SetPosition(Vector2 position)
        {
            this.position = position;
            if (Transform != null)
                Transform.LocalPosition = position;
        }

        public void SetPosition(float x, float y)
        {
            SetPosition(new Vector2(x, y));
        }

        public void SetRotation(float angle)
        {
            rotation = NormalizeAngle(angle);
            if (Transform != null)
                Transform.LocalRotation = rotation;
        }

        public void MovePosition(Vector2 targetPosition)
        {
            var displacement = targetPosition - position;
            position = targetPosition;
            velocity = displacement / Time.fixedDeltaTime;
            UpdateTransform();
        }

        public void MoveRotation(float targetRotation)
        {
            var deltaRotation = targetRotation - rotation;
            rotation = NormalizeAngle(targetRotation);
            angularVelocity = new Vector2(deltaRotation / Time.fixedDeltaTime, 0);
            UpdateTransform();
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            velocity += impulse / mass;
            WakeUpBody();
        }

        public void ApplyImpulseAtPoint(Vector2 impulse, Vector2 point)
        {
            velocity += impulse / mass;
            var torqueAmount = (point - worldCenterOfMass).Cross(impulse);
            angularVelocity += new Vector2(torqueAmount / inertia, 0);
            WakeUpBody();
        }

        public void ApplyAngularImpulse(float impulse)
        {
            angularVelocity += new Vector2(impulse / inertia, 0);
            WakeUpBody();
        }

        public Vector2 GetVelocityAtPoint(Vector2 point)
        {
            var r = point - worldCenterOfMass;
            return velocity + new Vector2(-angularVelocity.Y * r.Y, angularVelocity.Y * r.X);
        }

        public Vector2 GetRelativeVelocity(Vector2 point)
        {
            return GetVelocityAtPoint(point) - velocity;
        }

        public float GetKineticEnergy()
        {
            return 0.5f * mass * velocity.SqrMagnitude;
        }

        public float GetAngularKineticEnergy()
        {
            return 0.5f * inertia * angularVelocity.SqrMagnitude;
        }

        public void WakeUpBody()
        {
            if (!isAwake)
            {
                isAwake = true;
                WakeUp?.Invoke(this, new RigidBodyEventArgs(this));
            }
        }

        public void SleepBody()
        {
            if (isAwake)
            {
                isAwake = false;
                velocity = Vector2.Zero;
                angularVelocity = Vector2.Zero;
                SleepBody?.Invoke(this, new RigidBodyEventArgs(this));
            }
        }

        public void AddCollider(Collider2D collider)
        {
            if (collider != null && !attachedColliders.Contains(collider))
            {
                attachedColliders.Add(collider);
                UpdateMassFromColliders();
            }
        }

        public void RemoveCollider(Collider2D collider)
        {
            if (collider != null && attachedColliders.Remove(collider))
            {
                UpdateMassFromColliders();
            }
        }

        private void UpdateMassFromColliders()
        {
            if (mass <= 0f || attachedColliders.Count == 0)
                return;

            var totalArea = 0f;
            foreach (var collider in attachedColliders)
            {
                if (collider is BoxCollider box)
                {
                    totalArea += box.Size.X * box.Size.Y;
                }
                else if (collider is CircleCollider circle)
                {
                    totalArea += (float)Math.PI * circle.Radius * circle.Radius;
                }
            }

            if (totalArea > 0)
            {
                mass = totalArea * density;
                UpdateInverseMass();
            }
        }

        private void UpdateInverseMass()
        {
        }

        private void UpdateWorldCenterOfMass()
        {
            if (Transform != null)
            {
                worldCenterOfMass = Transform.TransformPoint(localCenterOfMass);
            }
            else
            {
                worldCenterOfMass = localCenterOfMass;
            }
        }

        private void OnBodyTypeChanged()
        {
            if (bodyType == RigidBodyType.Static)
            {
                isKinematic = true;
                velocity = Vector2.Zero;
                angularVelocity = Vector2.Zero;
                useGravity = false;
            }
            else if (bodyType == RigidBodyType.Kinematic)
            {
                isKinematic = true;
            }
            else
            {
                isKinematic = false;
            }
        }

        private void OnGravityChanged()
        {
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        public Vector2 GetPointVelocity(Vector2 worldPoint)
        {
            return GetVelocityAtPoint(worldPoint);
        }

        public Vector2 GetWorldPoint(Vector2 localPoint)
        {
            if (Transform != null)
            {
                return Transform.TransformPoint(localPoint);
            }
            return localPoint;
        }

        public Vector2 GetLocalPoint(Vector2 worldPoint)
        {
            if (Transform != null)
            {
                return Transform.InverseTransformPoint(worldPoint);
            }
            return worldPoint;
        }

        public Vector2 GetWorldVector(Vector2 localVector)
        {
            if (Transform != null)
            {
                return Transform.TransformDirection(localVector);
            }
            return localVector;
        }

        public Vector2 GetLocalVector(Vector2 worldVector)
        {
            if (Transform != null)
            {
                return Transform.InverseTransformDirection(worldVector);
            }
            return worldVector;
        }

        public void SetDensity(float newDensity)
        {
            density = Math.Max(0f, newDensity);
            UpdateMassFromColliders();
        }

        public void SetMass(float newMass)
        {
            Mass = newMass;
        }

        public void SetDrag(float newDrag)
        {
            Drag = newDrag;
        }

        public void SetAngularDrag(float newAngularDrag)
        {
            AngularDrag = newAngularDrag;
        }

        public void SetFreezePosition(bool freezeX, bool freezeY)
        {
            freezePositionX = freezeX;
            freezePositionY = freezeY;
        }

        public void SetFreezeRotation(bool freeze)
        {
            freezeRotation = freeze;
        }

        public bool IsFrozenPosX => freezePositionX;
        public bool IsFrozenPosY => freezePositionY;
        public bool IsFrozenRot => freezeRotation;

        public override string ToString()
        {
            return $"RigidBody2D(Position={Position}, Velocity={Velocity}, Mass={Mass:F2}, Type={BodyType})";
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
                attachedColliders.Clear();
                customData.Clear();
                VelocityChanged = null;
                ForceApplied = null;
                CollisionDetected = null;
                SleepStateChanged = null;
                WakeUp = null;
                GoToSleep = null;
            }
        }
    }

    public class RigidBodyEventArgs : EventArgs
    {
        public RigidBody2D RigidBody { get; }
        public Vector2 Velocity { get; }
        public Vector2 Force { get; }
        public float AngularVelocity { get; }

        public RigidBodyEventArgs(RigidBody2D rigidBody)
        {
            RigidBody = rigidBody;
            Velocity = rigidBody?.Velocity ?? Vector2.Zero;
            Force = Vector2.Zero;
            AngularVelocity = rigidBody?.AngularVelocity.Magnitude ?? 0f;
        }

        public RigidBodyEventArgs(RigidBody2D rigidBody, Vector2 velocity, Vector2 force)
        {
            RigidBody = rigidBody;
            Velocity = velocity;
            Force = force;
            AngularVelocity = rigidBody?.AngularVelocity.Magnitude ?? 0f;
        }
    }

    public static class PhysicsWorld
    {
        private static Vector2 gravity = new Vector2(0, -9.81f);
        private static float fixedDeltaTime = 0.02f;
        private static int velocityIterations = 8;
        private static int positionIterations = 3;
        private static float baumgarteScale = 0.2f;
        private static float slop = 0.01f;
        private static float penetrationSlop = 0.01f;
        private static bool autoSimulation = true;
        private static float timeScale = 1f;
        private static int maxSubSteps = 4;

        public static Vector2 Gravity
        {
            get => gravity;
            set => gravity = value;
        }

        public static float FixedDeltaTime
        {
            get => fixedDeltaTime;
            set => fixedDeltaTime = Math.Max(0.0001f, value);
        }

        public static int VelocityIterations
        {
            get => velocityIterations;
            set => velocityIterations = Math.Max(1, value);
        }

        public static int PositionIterations
        {
            get => positionIterations;
            set => positionIterations = Math.Max(1, value);
        }

        public static float BaumgarteScale
        {
            get => baumgarteScale;
            set => baumgarteScale = Math.Max(0f, Math.Min(1f, value));
        }

        public static float Slop
        {
            get => slop;
            set => slop = Math.Max(0f, value);
        }

        public static float PenetrationSlop
        {
            get => penetrationSlop;
            set => penetrationSlop = Math.Max(0f, value);
        }

        public static bool AutoSimulation
        {
            get => autoSimulation;
            set => autoSimulation = value;
        }

        public static float TimeScale
        {
            get => timeScale;
            set => timeScale = Math.Max(0f, value);
        }

        public static int MaxSubSteps
        {
            get => maxSubSteps;
            set => maxSubSteps = Math.Max(1, value);
        }

        public static void SetGravity(float x, float y)
        {
            gravity = new Vector2(x, y);
        }

        public static void SetGravity(Vector2 newGravity)
        {
            gravity = newGravity;
        }

        public static Vector2 GetGravity()
        {
            return gravity;
        }

        public static void Reset()
        {
            gravity = new Vector2(0, -9.81f);
            fixedDeltaTime = 0.02f;
            velocityIterations = 8;
            positionIterations = 3;
            baumgarteScale = 0.2f;
            slop = 0.01f;
            penetrationSlop = 0.01f;
            autoSimulation = true;
            timeScale = 1f;
            maxSubSteps = 4;
        }
    }

    public static class Mathf
    {
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        public static float LerpAngle(float a, float b, float t)
        {
            var delta = Repeat(b - a, 360f);
            if (delta > 180f)
                delta -= 360f;
            return a + delta * Clamp01(t);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        public static float Repeat(float value, float length)
        {
            return Clamp(value - Floor(value / length) * length, 0f, length);
        }

        public static float Floor(float value)
        {
            return (float)Math.Floor(value);
        }

        public static float Sign(float value)
        {
            return value >= 0f ? 1f : -1f;
        }

        public static float Abs(float value)
        {
            return Math.Abs(value);
        }

        public static float Min(float a, float b)
        {
            return a < b ? a : b;
        }

        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }

        public static int Sign(int value)
        {
            return value >= 0 ? 1 : -1;
        }

        public static int Abs(int value)
        {
            return Math.Abs(value);
        }

        public static float SmoothDamp(float current, float target, ref float velocity, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
        {
            smoothTime = Max(0.0001f, smoothTime);
            var omega = 2f / smoothTime;
            var x = omega * deltaTime;
            var exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            var change = current - target;
            var maxChange = maxSpeed * smoothTime;
            change = Clamp(change, -maxChange, maxChange);
            var temp = (velocity + omega * change) * deltaTime;
            velocity = (velocity - omega * temp) * exp;
            var output = target + (change + temp) * exp;
            if (target - current > 0f == output > target)
            {
                output = target;
                velocity = (output - target) / deltaTime;
            }
            return output;
        }

        public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 velocity, float smoothTime, float deltaTime)
        {
            var smoothTimeClamped = Max(0.0001f, smoothTime);
            var omega = 2f / smoothTimeClamped;
            var x = omega * deltaTime;
            var exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            var change = current - target;
            var maxChange = float.MaxValue * smoothTimeClamped;
            change = Vector2.ClampMagnitude(change, maxChange);
            var temp = (velocity + omega * change) * deltaTime;
            velocity = (velocity - omega * temp) * exp;
            var output = target + (change + temp) * exp;
            return output;
        }
    }

    public static class Vector2Extensions
    {
        public static Vector2 ClampMagnitude(this Vector2 vector, float maxLength)
        {
            var sqrMagnitude = vector.SqrMagnitude;
            if (sqrMagnitude > maxLength * maxLength)
            {
                var magnitude = (float)Math.Sqrt(sqrMagnitude);
                return new Vector2(vector.X / magnitude * maxLength, vector.Y / magnitude * maxLength);
            }
            return vector;
        }

        public static Vector2 Rotate(this Vector2 vector, float angleInDegrees)
        {
            var angleInRadians = angleInDegrees * (float)Math.PI / 180f;
            var cos = (float)Math.Cos(angleInRadians);
            var sin = (float)Math.Sin(angleInRadians);
            return new Vector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            );
        }

        public static float Angle(this Vector2 from, Vector2 to)
        {
            var diff = to - from;
            return (float)Math.Atan2(diff.Y, diff.X) * 180f / (float)Math.PI;
        }
    }

    public class Time
    {
        private static float deltaTime = 0f;
        private static float fixedDeltaTime = 0.02f;
        private static float time = 0f;
        private static float timeScale = 1f;
        private static float unscaledTime = 0f;
        private static int frameCount = 0;
        private static float frameRate = 0f;
        private static float realtimeSinceStartup = 0f;
        private static float maximumDeltaTime = 0.1f;

        public static float deltaTime => deltaTime;
        public static float fixedDeltaTime => fixedDeltaTime;
        public static float time => time;
        public static float timeScale { get => timeScale; set => timeScale = Math.Max(0f, value); }
        public static float unscaledTime => unscaledTime;
        public static int frameCount => frameCount;
        public static float frameRate => frameRate;
        public static float realtimeSinceStartup => realtimeSinceStartup;
        public static float maximumDeltaTime { get => maximumDeltaTime; set => maximumDeltaTime = Math.Max(0f, value); }

        public static float fixedTime => time;
        public static float timeSinceLevelLoad => time;
        public static float smoothedFrameRate => frameRate;

        internal static void Update(float dt)
        {
            deltaTime = dt * timeScale;
            if (deltaTime > maximumDeltaTime)
                deltaTime = maximumDeltaTime;

            fixedDeltaTime = PhysicsWorld.FixedDeltaTime;
            time += deltaTime;
            unscaledTime += dt;
            frameCount++;
            frameRate = 1f / deltaTime;
            realtimeSinceStartup += dt;
        }
    }
}
