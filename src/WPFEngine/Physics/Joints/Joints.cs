using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Math;

namespace WPFEngine.Physics.Joints
{
    public enum JointType
    {
        Hinge,
        Slider,
        Distance,
        Spring,
        Weld,
        Rope,
        Pulley,
        Wheel,
        Gear,
        Fixed,
        Prismatic = Slider
    }

    public enum LimitState
    {
        Inactive,
        AtLower,
        AtUpper,
        Equal,
        Unable
    }

    public interface IJoint
    {
        JointType Type { get; }
        RigidBody? BodyA { get; }
        RigidBody? BodyB { get; }
        Vector2 LocalAnchorA { get; }
        Vector2 LocalAnchorB { get; }
        bool IsEnabled { get; set; }
        bool CollideConnected { get; }
        float BreakForce { get; set; }
        float BreakTorque { get; set; }
        bool IsBroken { get; }
        
        Vector2 GetReactionForce(float invDt);
        float GetReactionTorque(float invDt);
        void WakeBodies();
        void InitializeVelocityConstraints(SolverData data);
        void SolveVelocityConstraints(SolverData data);
        bool SolvePositionConstraints(SolverData data);
    }

    public struct SolverData
    {
        public float DeltaTime;
        public float InvDeltaTime;
        public float DeltaTimeRatio;
        public int Iterations;
        public int IndexA;
        public int IndexB;
        public float InvMassA;
        public float InvMassB;
        public float InvIA;
        public float InvIB;
        public Vector2 PositionA;
        public Vector2 PositionB;
        public float AngleA;
        public float AngleB;
        public Vector2 VelocityA;
        public Vector2 VelocityB;
        public float AngularVelocityA;
        public float AngularVelocityB;
    }

    public abstract class Joint : IJoint
    {
        protected JointType _type;
        protected RigidBody? _bodyA;
        protected RigidBody? _bodyB;
        protected Vector2 _localAnchorA;
        protected Vector2 _localAnchorB;
        protected Vector2 _localCenterA;
        protected Vector2 _localCenterB;
        protected float _invMassA;
        protected float _invMassB;
        protected float _invIA;
        protected float _invIB;
        protected bool _isEnabled;
        protected bool _collideConnected;
        protected float _breakForce;
        protected float _breakTorque;
        protected bool _isBroken;
        protected Vector2 _rA;
        protected Vector2 _rB;
        protected Vector2 _localYAxisA;
        protected Vector2 _localXAxisA;
        protected float _referenceAngle;
        protected float _jointError;
        protected bool _enableLimits;
        protected bool _enableMotor;
        protected bool _isMotorEnabled;

        public JointType Type => _type;
        public RigidBody? BodyA => _bodyA;
        public RigidBody? BodyB => _bodyB;
        public Vector2 LocalAnchorA => _localAnchorA;
        public Vector2 LocalAnchorB => _localAnchorB;
        
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }
        
        public bool CollideConnected
        {
            get => _collideConnected;
            set => _collideConnected = value;
        }
        
        public float BreakForce
        {
            get => _breakForce;
            set => _breakForce = Math.Max(0f, value);
        }
        
        public float BreakTorque
        {
            get => _breakTorque;
            set => _breakTorque = Math.Max(0f, value);
        }
        
        public bool IsBroken => _isBroken;

        public Vector2 LocalAnchorB1 => _localAnchorB;

        public event EventHandler? JointBreak;

        protected Joint(JointType type, RigidBody? bodyA, RigidBody? bodyB)
        {
            _type = type;
            _bodyA = bodyA;
            _bodyB = bodyB;
            _localAnchorA = Vector2.Zero;
            _localAnchorB = Vector2.Zero;
            _isEnabled = true;
            _collideConnected = false;
            _breakForce = float.MaxValue;
            _breakTorque = float.MaxValue;
            _isBroken = false;
            _enableLimits = false;
            _enableMotor = false;
            
            InitializeDefaults();
        }

        protected virtual void InitializeDefaults()
        {
        }

        public void SetAnchors(Vector2 worldAnchorA, Vector2 worldAnchorB)
        {
            _localAnchorA = GetBodyAAnchor(worldAnchorA);
            _localAnchorB = GetBodyBAnchor(worldAnchorB);
        }

        public void SetAnchorA(Vector2 worldAnchor)
        {
            _localAnchorA = GetBodyAAnchor(worldAnchor);
        }

        public void SetAnchorB(Vector2 worldAnchor)
        {
            _localAnchorB = GetBodyBAnchor(worldAnchor);
        }

        protected Vector2 GetBodyAAnchor(Vector2 worldAnchor)
        {
            if (_bodyA == null) return worldAnchor;
            Vector2 c = _bodyA.Position;
            float a = _bodyA.Rotation;
            Vector2 p = worldAnchor;
            return new Vector2(
                (p.X - c.X) * MathHelper.Cos(-a) - (p.Y - c.Y) * MathHelper.Sin(-a),
                (p.X - c.X) * MathHelper.Sin(-a) + (p.Y - c.Y) * MathHelper.Cos(-a)
            );
        }

        protected Vector2 GetBodyBAnchor(Vector2 worldAnchor)
        {
            if (_bodyB == null) return worldAnchor;
            Vector2 c = _bodyB.Position;
            float a = _bodyB.Rotation;
            Vector2 p = worldAnchor;
            return new Vector2(
                (p.X - c.X) * MathHelper.Cos(-a) - (p.Y - c.Y) * MathHelper.Sin(-a),
                (p.X - c.X) * MathHelper.Sin(-a) + (p.Y - c.Y) * MathHelper.Cos(-a)
            );
        }

        protected Vector2 GetWorldAnchorA()
        {
            return GetBodyPoint(_bodyA, _localAnchorA);
        }

        protected Vector2 GetWorldAnchorB()
        {
            return GetBodyPoint(_bodyB, _localAnchorB);
        }

        protected Vector2 GetBodyPoint(RigidBody? body, Vector2 localPoint)
        {
            if (body == null) return localPoint;
            float cos = MathHelper.Cos(body.Rotation);
            float sin = MathHelper.Sin(body.Rotation);
            return new Vector2(
                body.Position.X + cos * localPoint.X - sin * localPoint.Y,
                body.Position.Y + sin * localPoint.X + cos * localPoint.Y
            );
        }

        public virtual Vector2 GetReactionForce(float invDt)
        {
            return Vector2.Zero;
        }

        public virtual float GetReactionTorque(float invDt)
        {
            return 0f;
        }

        public virtual void WakeBodies()
        {
            if (_bodyA != null && _bodyA.BodyType != BodyType.Static)
                _bodyA.WakeUp();
            if (_bodyB != null && _bodyB.BodyType != BodyType.Static)
                _bodyB.WakeUp();
        }

        public abstract void InitializeVelocityConstraints(SolverData data);
        public abstract void SolveVelocityConstraints(SolverData data);
        public abstract bool SolvePositionConstraints(SolverData data);

        protected Vector2 Mul(Vec2 A, Vector2 v)
        {
            return new Vector2(A.X * v.X - A.Y * v.Y, A.Y * v.X + A.X * v.Y);
        }

        protected Vector2 MulT(Vec2 A, Vector2 v)
        {
            return new Vector2(A.X * v.X + A.Y * v.Y, -A.X * v.Y + A.Y * v.X);
        }

        protected Vec2 Add(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X + b.X, a.Y + b.Y);
        }

        protected Vec2 Sub(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X - b.X, a.Y - b.Y);
        }

        protected Vec2 Mul(float s, Vec2 a)
        {
            return new Vec2(s * a.X, s * a.Y);
        }

        protected float Cross(Vec2 a, Vec2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        protected Vector2 Cross(float s, Vector2 a)
        {
            return new Vector2(-s * a.Y, s * a.X);
        }

        protected float Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        protected void CheckBreakLimit()
        {
            if (_breakForce < float.MaxValue || _breakTorque < float.MaxValue)
            {
                Vector2 reaction = GetReactionForce(1f / 60f);
                float reactionTorque = GetReactionTorque(1f / 60f);
                
                if (reaction.Length() > _breakForce || Math.Abs(reactionTorque) > _breakTorque)
                {
                    _isBroken = true;
                    _isEnabled = false;
                    JointBreak?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public override string ToString()
        {
            return $"{_type} Joint: A={GetWorldAnchorA()}, B={GetWorldAnchorB()}, Enabled={_isEnabled}, Broken={_isBroken}";
        }

        public virtual string ToDebugString()
        {
            return $"Type: {_type}, AnchorA: {_localAnchorA}, AnchorB: {_localAnchorB}, Enabled: {_isEnabled}, Broken: {_isBroken}";
        }

        protected float ComputeAngularSlop(float angularOffset, float slop)
        {
            return MathHelper.Pi - MathHelper.Acos(MathHelper.Clamp(1f - angularOffset * angularOffset / (slop * slop), -1f, 1f));
        }

        protected float ComputeLinearSlop(float linearOffset, float slop)
        {
            return MathHelper.Pow(linearOffset, 2f) / (2f * slop);
        }
    }

    public struct Vec2
    {
        public float X, Y;

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Vector2(Vec2 v) => new Vector2(v.X, v.Y);
        public static implicit operator Vec2(Vector2 v) => new Vec2(v.X, v.Y);
    }

    public class LimitStateHelper
    {
        public static LimitState ComputeLimitState(float lower, float upper, float accumulated, float slop)
        {
            if (MathHelper.Approximately(lower, upper))
            {
                return LimitState.Equal;
            }

            if (accumulated <= lower - slop)
            {
                return LimitState.AtLower;
            }

            if (accumulated >= upper + slop)
            {
                return LimitState.AtUpper;
            }

            return LimitState.Inactive;
        }
    }

    public class HingeJoint : Joint
    {
        private float _motorMass;
        private float _limitMass;
        private float _axialMass;
        private float _motor;
        private float _lowerLimit;
        private float _upperLimit;
        private LimitState _limitState;
        private LimitState _limitState1;
        private Vec2 _axis;
        private Vec2 _ perpAxis;
        private Vec2 _ solverDataTemp;
        private Vec2 _a1;
        private Vec2 _a2;
        private Vector2 _impulse;
        private Vector2 _motorImpulse;
        private float _motorSpeed;
        private float _maxMotorTorque;
        private bool _enableLimit;
        private bool _referenceAngleCorrection;
        private float _angularOffset;
        private float _rA;
        private float _rB;
        private Vec2 _d;
        private float _m;
        private float _iA;
        private float _iB;
        private float _angularMass;
        private float _mass;
        private float _referenceAngle;
        private float _upperAngle;
        private float _lowerAngle;
        private bool _enableAngularLimits;

        public float ReferenceAngle
        {
            get => _referenceAngle;
            set => _referenceAngle = value;
        }

        public float LowerLimit
        {
            get => _lowerLimit;
            set
            {
                _lowerLimit = MathHelper.WrapAngle(value);
                _enableLimits = true;
            }
        }

        public float UpperLimit
        {
            get => _upperLimit;
            set
            {
                _upperLimit = MathHelper.WrapAngle(value);
                _enableLimits = true;
            }
        }

        public bool EnableLimits
        {
            get => _enableLimits;
            set => _enableLimits = value;
        }

        public float MotorSpeed
        {
            get => _motorSpeed;
            set
            {
                _motorSpeed = value;
                _enableMotor = true;
            }
        }

        public float MaxMotorTorque
        {
            get => _maxMotorTorque;
            set
            {
                _maxMotorTorque = value;
                _enableMotor = true;
            }
        }

        public bool MotorEnabled
        {
            get => _enableMotor;
            set => _enableMotor = value;
        }

        public float CurrentAngle => _bodyA != null && _bodyB != null ? _bodyA.Rotation - _bodyB.Rotation - _referenceAngle : 0f;
        public float CurrentSpeed => _bodyA != null && _bodyB != null ? _bodyA.AngularVelocity - _bodyB.AngularVelocity : 0f;

        public HingeJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 worldAnchor, Vector2 axis) : base(JointType.Hinge, bodyA, bodyB)
        {
            _localAnchorA = GetBodyAAnchor(worldAnchor);
            _localAnchorB = GetBodyBAnchor(worldAnchor);
            _referenceAngle = _bodyA != null && _bodyB != null ? _bodyA.Rotation - _bodyB.Rotation : 0f;
            _axis = axis;
            _localYAxisA = new Vec2(-_axis.Y, _axis.X);
            _motorSpeed = 0f;
            _maxMotorTorque = 0f;
            _enableLimits = false;
            _lowerLimit = 0f;
            _upperLimit = 0f;
            _angularOffset = 0f;
            _rA = 0f;
            _rB = 0f;
            _d = new Vec2();
            _m = 0f;
            _iA = 0f;
            _iB = 0f;
            _angularMass = 0f;
            _mass = 0f;
            _referenceAngle = 0f;
            _upperAngle = 0f;
            _lowerAngle = 0f;
            _enableAngularLimits = false;
        }

        protected override void InitializeDefaults()
        {
            _motorSpeed = 0f;
            _maxMotorTorque = 0f;
            _enableLimits = false;
            _lowerLimit = 0f;
            _upperLimit = 0f;
            _angularOffset = 0f;
            _rA = 0f;
            _rB = 0f;
            _d = new Vec2();
            _m = 0f;
            _iA = 0f;
            _iB = 0f;
            _angularMass = 0f;
            _mass = 0f;
            _referenceAngle = 0f;
            _upperAngle = 0f;
            _lowerAngle = 0f;
            _enableAngularLimits = false;
            _motorImpulse = Vector2.Zero;
            _impulse = Vector2.Zero;
        }

        public override void InitializeVelocityConstraints(SolverData data)
        {
            _invMassA = data.InvMassA;
            _invMassB = data.InvMassB;
            _invIA = data.InvIA;
            _invIB = data.InvIB;

            Vec2 cA = data.PositionA;
            float aA = data.AngleA;
            Vec2 cB = data.PositionB;
            float aB = data.AngleB;

            float mA = _invMassA;
            float mB = _invMassB;
            float iA = _invIA;
            float iB = _invIB;

            Vec2 d = new Vec2(
                (_bodyB != null ? (_bodyB.Position - _bodyA.Position).X : 0),
                (_bodyB != null ? (_bodyB.Position - _bodyA.Position).Y : 0)
            );

            float angularMass = iA + iB;

            if (_enableMotor || _enableLimits)
            {
                _mass = angularMass > 0f ? 1f / angularMass : 0f;
            }

            if (_enableLimits)
            {
                float jointAngle = aB - aA - _referenceAngle;
                LimitState state = LimitStateHelper.ComputeLimitState(_lowerLimit, _upperLimit, jointAngle, 0f);
                _limitState = state;
            }
            else
            {
                _limitState = LimitState.Inactive;
            }

            WakeBodies();
        }

        public override void SolveVelocityConstraints(SolverData data)
        {
            Vec2 vA = data.VelocityA;
            float wA = data.AngularVelocityA;
            Vec2 vB = data.VelocityB;
            float wB = data.AngularVelocityB;

            float mA = _invMassA;
            float mB = _invMassB;
            float iA = _invIA;
            float iB = _invIB;

            if (_enableMotor && _limitState != LimitState.AtUpper)
            {
                float Cdot = wB - wA - _motorSpeed;
                float motor = -_mass * Cdot;
                float oldMotorImpulse = _motorImpulse.Y;
                float maxMotor = _maxMotorTorque * data.DeltaTimeRatio;
                _motorImpulse.Y = MathHelper.Clamp(_motorImpulse.Y + motor, -maxMotor, maxMotor);
                motor = _motorImpulse.Y - oldMotorImpulse;
                wA -= iA * motor;
                wB += iB * motor;
            }

            if (_enableLimits && _limitState != LimitState.Inactive)
            {
                Vec2 P = _impulse;

                float Cdot = wB - wA;
                float limitMotor = -_mass * Cdot;
                float oldLimitImpulse = _motorImpulse.Y;
                float maxLimit = float.MaxValue;
                _motorImpulse.Y = MathHelper.Clamp(_motorImpulse.Y + limitMotor, -maxLimit, maxLimit);
                limitMotor = _motorImpulse.Y - oldLimitImpulse;
                wA -= iA * limitMotor;
                wB += iB * limitMotor;
            }
            else
            {
                _motorImpulse.Y = 0f;
            }

            data.VelocityA = vA;
            data.AngularVelocityA = wA;
            data.VelocityB = vB;
            data.AngularVelocityB = wB;
        }

        public override bool SolvePositionConstraints(SolverData data)
        {
            Vec2 cA = data.PositionA;
            float aA = data.AngleA;
            Vec2 cB = data.PositionB;
            float aB = data.AngleB;

            float mA = _invMassA;
            float mB = _invMassB;
            float iA = _invIA;
            float iB = _invIB;

            float angularError = 0f;
            float linearError = 0f;

            if (_enableLimits)
            {
                float angle = aB - aA - _referenceAngle;
                float C = 0f;

                if (_limitState == LimitState.AtLower)
                {
                    C = MathHelper.Max(angle - _lowerLimit, 0f);
                    angularError = MathHelper.Abs(C);
                }
                else if (_limitState == LimitState.AtUpper)
                {
                    C = MathHelper.Min(angle - _upperLimit, 0f);
                    angularError = MathHelper.Abs(C);
                }

                float limitMass = iA + iB;
                if (limitMass > 0f)
                {
                    float impulse = -C / limitMass;
                    aA -= iA * impulse;
                    aB += iB * impulse;
                }
            }

            return angularError <= 0.005f && linearError <= 0.005f;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return new Vector2(_impulse.X * invDt, _impulse.Y * invDt);
        }

        public override float GetReactionTorque(float invDt)
        {
            return _motorImpulse.Y * invDt;
        }

        public void SetLimits(float lower, float upper)
        {
            _lowerLimit = lower;
            _upperLimit = upper;
            _enableLimits = true;
        }

        public void DisableLimits()
        {
            _enableLimits = false;
        }

        public void EnableMotor(float speed, float maxTorque)
        {
            _motorSpeed = speed;
            _maxMotorTorque = maxTorque;
            _enableMotor = true;
        }

        public void DisableMotor()
        {
            _enableMotor = false;
        }
    }

    public class SliderJoint : Joint
    {
        private float _axisAngle;
        private Vec2 _perp;
        private float _s1, _s2;
        private float _a1, _a2;
        private Vec2 _u1, _u2;
        private Vec2 _rA;
        private Vec2 _rB;
        private Vec2 _d;
        private float _m;
        private float _iA, _iB;
        private float _mass;
        private float _motorMass;
        private float _axialMass;
        private float _motor;
        private float _lowerLimit;
        private float _upperLimit;
        private float _maxMotorForce;
        private float _motorSpeed;
        private float _referenceAngle;
        private LimitState _limitState;
        private float _translation;
        private float _speed;
        private Vector2 _impulse;
        private Vector2 _motorImpulse;

        public float AxisAngle
        {
            get => _axisAngle;
            set
            {
                _axisAngle = value;
                UpdateAxis();
            }
        }

        public Vec2 Axis => new Vec2(MathHelper.Cos(_axisAngle), MathHelper.Sin(_axisAngle));

        public float LowerLimit
        {
            get => _lowerLimit;
            set => _lowerLimit = value;
        }

        public float UpperLimit
        {
            get => _upperLimit;
            set => _upperLimit = value;
        }

        public bool LimitEnabled
        {
            get => _enableLimits;
            set => _enableLimits = value;
        }

        public float MotorSpeed
        {
            get => _motorSpeed;
            set
            {
                _motorSpeed = value;
                _enableMotor = true;
            }
        }

        public float MaxMotorForce
        {
            get => _maxMotorForce;
            set
            {
                _maxMotorForce = value;
                _enableMotor = true;
            }
        }

        public bool MotorEnabled
        {
            get => _enableMotor;
            set => _enableMotor = value;
        }

        public float CurrentTranslation => _translation;
        public float CurrentSpeed => _speed;

        public SliderJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 worldAnchor, float angle) : base(JointType.Slider, bodyA, bodyB)
        {
            _localAnchorA = GetBodyAAnchor(worldAnchor);
            _localAnchorB = GetBodyBAnchor(worldAnchor);
            _referenceAngle = _bodyA != null && _bodyB != null ? _bodyA.Rotation - _bodyB.Rotation : 0f;
            _axisAngle = angle;
            _lowerLimit = -1f;
            _upperLimit = 1f;
            _enableLimits = false;
            _enableMotor = false;
            _motorSpeed = 0f;
            _maxMotorForce = 0f;
            _motorMass = 0f;
            _motor = 0f;
            _impulse = Vector2.Zero;
            _motorImpulse = Vector2.Zero;
            _translation = 0f;
            _speed = 0f;
            _limitState = LimitState.Inactive;
        }

        private void UpdateAxis()
        {
        }

        public override void InitializeVelocityConstraints(SolverData data)
        {
            _invMassA = data.InvMassA;
            _invMassB = data.InvMassB;
            _invIA = data.InvIA;
            _invIB = data.InvIB;

            Vec2 cA = data.PositionA;
            float aA = data.AngleA;
            Vec2 cB = data.PositionB;
            float aB = data.AngleB;

            Vec2 axis = Axis;
            Vec2 perp = new Vec2(-axis.Y, axis.X);

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(aA), MathHelper.Sin(aA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(aB), MathHelper.Sin(aB)), _localAnchorB);
            Vec2 d = Sub(Sub(Add(cB, rB), cA), rA);

            float mA = _invMassA;
            float mB = _invMassB;
            float iA = _invIA;
            float iB = _invIB;

            float s1 = Cross(Add(d, rA), perp);
            float s2 = Cross(rB, perp);

            float a1 = Cross(rA, perp);
            float a2 = Cross(rB, perp);

            float k11 = mA + mB + iA * s1 * s1 + iB * s2 * s2;
            float k22 = mA + mB + iA * a1 * a1 + iB * a2 * a2;
            _mass = k11 > 0f ? 1f / k11 : 0f;
            _axialMass = k22 > 0f ? 1f / k22 : 0f;

            float c1 = Dot(d, perp);
            float C = c1;
            if (_enableLimits)
            {
                float translation = Dot(axis, d);
                _translation = translation;
                _limitState = LimitStateHelper.ComputeLimitState(_lowerLimit, _upperLimit, translation, 0.005f);
            }
            else
            {
                _limitState = LimitState.Inactive;
            }

            if (_enableMotor)
            {
                float Cdot = Dot(axis, Sub(vB, vA)) + a2 * wB - a1 * wA;
                _speed = Cdot;
                _motorMass = mA + mB + iA * a1 * a1 + iB * a2 * a2;
                if (_motorMass > 0f)
                    _motorMass = 1f / _motorMass;
            }

            if (_limitState == LimitState.Inactive)
            {
                _s1 = s1;
                _s2 = s2;
                _a1 = a1;
                _a2 = a2;
            }

            WakeBodies();
        }

        public override void SolveVelocityConstraints(SolverData data)
        {
            Vec2 vA = data.VelocityA;
            float wA = data.AngularVelocityA;
            Vec2 vB = data.VelocityB;
            float wB = data.AngularVelocityB;

            float mA = _invMassA;
            float mB = _invMassB;
            float iA = _invIA;
            float iB = _invIB;

            if (_enableMotor && _limitState != LimitState.AtUpper)
            {
                Vec2 axis = Axis;
                Vec2 perp = new Vec2(-axis.Y, axis.X);

                float Cdot = Dot(axis, Sub(vB, vA)) + _a2 * wB - _a1 * wA;
                float motor = -_motorMass * (Cdot + _motorSpeed);
                float maxMotor = _maxMotorForce * data.DeltaTimeRatio;
                float oldMotorImpulse = _motorImpulse.Y;
                _motorImpulse.Y = MathHelper.Clamp(_motorImpulse.Y + motor, -maxMotor, maxMotor);
                motor = _motorImpulse.Y - oldMotorImpulse;

                Vec2 P = Mul(motor, axis);
                float wP = motor * _s1;
                float wR = motor * _s2;

                vA = Sub(vA, Mul(mA, P));
                wA -= iA * wP;
                vB = Add(vB, Mul(mB, P));
                wB += iB * wR;
            }

            Vec2 axis2 = Axis;
            Vec2 perp2 = new Vec2(-axis2.Y, axis2.X);

            Vec2 rA2 = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            Vec2 rB2 = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            if (_enableLimits && _limitState != LimitState.Inactive)
            {
                Vec2 d2 = Sub(Sub(Add(data.PositionB, rB2), data.PositionA), rA2);

                float Cdot = Dot(axis2, Sub(vB, vA)) + Cross(rB2, vB) - Cross(rA2, vA);
                float limitMotor = -_axialMass * Cdot;
                float maxLimit = float.MaxValue;
                float oldLimitImpulse = _motorImpulse.Y;
                _motorImpulse.Y = MathHelper.Clamp(_motorImpulse.Y + limitMotor, -maxLimit, maxLimit);
                limitMotor = _motorImpulse.Y - oldLimitImpulse;

                Vec2 P2 = Mul(limitMotor, axis2);
                float wP2 = limitMotor * _a1;
                float wR2 = limitMotor * _a2;

                vA = Sub(vA, Mul(mA, P2));
                wA -= iA * wP2;
                vB = Add(vB, Mul(mB, P2));
                wB += iB * wR2;
            }

            float Cdot2 = Dot(perp2, Sub(vB, vA)) + Cross(rB2, vB) - Cross(rA2, vA);
            float impulse = -_mass * Cdot2;
            _impulse.X += impulse;

            Vec2 P3 = Mul(impulse, perp2);
            float wP3 = impulse * _s1;
            float wR3 = impulse * _s2;

            vA = Sub(vA, Mul(mA, P3));
            wA -= iA * wP3;
            vB = Add(vB, Mul(mB, P3));
            wB += iB * wR3;

            data.VelocityA = vA;
            data.AngularVelocityA = wA;
            data.VelocityB = vB;
            data.AngularVelocityB = wB;
        }

        public override bool SolvePositionConstraints(SolverData data)
        {
            Vec2 cA = data.PositionA;
            float aA = data.AngleA;
            Vec2 cB = data.PositionB;
            float aB = data.AngleB;

            Vec2 axis = Axis;
            Vec2 perp = new Vec2(-axis.Y, axis.X);

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(aA), MathHelper.Sin(aA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(aB), MathHelper.Sin(aB)), _localAnchorB);
            Vec2 d = Sub(Sub(Add(cB, rB), cA), rA);

            float s1 = Cross(Add(d, rA), perp);
            float s2 = Cross(rB, perp);

            float a1 = Cross(rA, perp);
            float a2 = Cross(rB, perp);

            float k11 = _invMassA + _invMassB + _invIA * s1 * s1 + _invIB * s2 * s2;
            float k12 = _invMassA + _invMassB + _invIA * a1 * a1 + _invIB * a2 * a2;
            float k22 = _invMassA + _invMassB + _invIA + _invIB;

            float k = k11 * k22 - k12 * k12;
            if (k == 0f) k = 0.0001f;
            float invK = 1f / k;

            float C = Dot(d, perp);
            float impulse = -invK * C;
            _impulse.X += impulse;

            Vec2 P = Mul(impulse, perp);
            float wP = impulse * s1;
            float wR = impulse * s2;

            cA = Sub(cA, Mul(_invMassA, P));
            aA -= _invIA * wP;
            cB = Add(cB, Mul(_invMassB, P));
            aB += _invIB * wR;

            float linearError = MathHelper.Abs(C);
            return linearError < 0.005f;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return new Vector2(_impulse.X * invDt, _impulse.Y * invDt);
        }

        public override float GetReactionTorque(float invDt)
        {
            return _motorImpulse.Y * invDt;
        }

        public void SetLimits(float lower, float upper)
        {
            _lowerLimit = lower;
            _upperLimit = upper;
            _enableLimits = true;
        }

        public void DisableLimits()
        {
            _enableLimits = false;
        }

        public void EnableMotor(float speed, float maxForce)
        {
            _motorSpeed = speed;
            _maxMotorForce = maxForce;
            _enableMotor = true;
        }

        public void DisableMotor()
        {
            _enableMotor = false;
        }
    }

    public class DistanceJoint : Joint
    {
        private float _length;
        private float _minLength;
        private float _maxLength;
        private float _stiffness;
        private float _damping;
        private float _impulse;
        private float _lowerImpulse;
        private float _upperImpulse;
        private float _currentLength;
        private float _bias;
        private float _mass;
        private float _softMass;
        private float _gamma;
        private float _epsilon;
        private float _uX;
        private float _uY;
        private bool _lengthEnabled;

        public float Length
        {
            get => _length;
            set => _length = Math.Max(0f, value);
        }

        public float MinLength
        {
            get => _minLength;
            set => _minLength = Math.Max(0f, value);
        }

        public float MaxLength
        {
            get => _maxLength;
            set => _maxLength = Math.Max(0f, value);
        }

        public float Stiffness
        {
            get => _stiffness;
            set
            {
                _stiffness = value;
                _lengthEnabled = true;
            }
        }

        public float Damping
        {
            get => _damping;
            set
            {
                _damping = value;
                _lengthEnabled = true;
            }
        }

        public bool LengthConstraintsEnabled
        {
            get => _lengthEnabled;
            set => _lengthEnabled = value;
        }

        public float CurrentLength => _currentLength;

        public DistanceJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchorA, Vector2 anchorB) : base(JointType.Distance, bodyA, bodyB)
        {
            _localAnchorA = anchorA;
            _localAnchorB = anchorB;
            _length = (GetBodyPoint(bodyA, anchorA) - GetBodyPoint(bodyB, anchorB)).Length();
            _minLength = _length;
            _maxLength = _length;
            _stiffness = 0f;
            _damping = 0f;
            _impulse = 0f;
            _lowerImpulse = 0f;
            _upperImpulse = 0f;
            _currentLength = 0f;
            _bias = 0f;
            _mass = 0f;
            _softMass = 0f;
            _gamma = 0f;
            _epsilon = 0f;
        }

        public override void InitializeVelocityConstraints(SolverData data)
        {
            _invMassA = data.InvMassA;
            _invMassB = data.InvMassB;
            _invIA = data.InvIA;
            _invIB = data.InvIB;

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            Vec2 d = Sub(Sub(Add(data.PositionB, rB), data.PositionA), rA);
            _currentLength = d.Length();

            Vec2 u = d;
            if (_currentLength > MathHelper.Epsilon)
                u = Mul(1f / _currentLength, d);
            else
                u = new Vec2(0f, 0f);

            _uX = u.X;
            _uY = u.Y;

            float crA = Cross(rA, u);
            float crB = Cross(rB, u);
            float invMass = _invMassA + _invIA * crA * crA + _invMassB + _invIB * crB * crB;

            _mass = invMass > 0f ? 1f / invMass : 0f;

            if (_stiffness > 0f || _damping > 0f)
            {
                float C = _currentLength - _length;
                float omega = MathHelper.Sqrt(_stiffness / invMass);
                float d = 2f * invMass * _damping * omega;
                float k = invMass * omega * omega;

                _gamma = data.DeltaTime * (d + data.DeltaTime * k);
                if (_gamma > 0f)
                    _gamma = 1f / _gamma;

                _bias = C * data.DeltaTime * k * _gamma;

                _softMass = invMass + _gamma;
                if (_softMass > 0f)
                    _softMass = 1f / _softMass;
            }
            else
            {
                _gamma = 0f;
                _bias = 0f;
                _softMass = _mass;
            }

            WakeBodies();
        }

        public override void SolveVelocityConstraints(SolverData data)
        {
            Vec2 vA = data.VelocityA;
            Vec2 vB = data.VelocityB;
            float wA = data.AngularVelocityA;
            float wB = data.AngularVelocityB;

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            Vec2 vAN = Sub(vA, Add(new Vec2(-wA * rA.Y, wA * rA.X), new Vec2(0f, 0f)));
            Vec2 vBN = Sub(vB, Add(new Vec2(-wB * rB.Y, wB * rB.X), new Vec2(0f, 0f)));

            float Cdot = _uX * (vBN.X - vA.X) + _uY * (vBN.Y - vA.Y);

            float motor = _mass * (-Cdot);
            float maxMotor = float.MaxValue;
            float oldImpulse = _impulse;
            _impulse = MathHelper.Clamp(_impulse + motor, -maxMotor, maxMotor);
            motor = _impulse - oldImpulse;

            Vec2 P = Mul(motor, new Vec2(_uX, _uY));

            vA.X += _invMassA * P.X;
            vA.Y += _invMassA * P.Y;
            wA -= _invIA * Cross(rA, P);
            vB.X -= _invMassB * P.X;
            vB.Y -= _invMassB * P.Y;
            wB += _invIB * Cross(rB, P);

            if (_lengthEnabled && (_minLength < _maxLength))
            {
                Vec2 vA2 = new Vec2(vA.X, vA.Y);
                Vec2 vB2 = new Vec2(vB.X, vB.Y);

                float Cdot2 = _uX * (vB2.X - vA2.X) + _uY * (vB2.Y - vA2.Y);

                float impulse = -_softMass * Cdot2;
                float oldLowerImpulse = _lowerImpulse;
                float oldUpperImpulse = _upperImpulse;
                float lowerLimit = MathHelper.Max(_minLength - _length, 0f);
                float upperLimit = MathHelper.Min(_maxLength - _length, 0f);
                float targetLength = MathHelper.Clamp(_currentLength - _length, lowerLimit, upperLimit);
                _lowerImpulse = MathHelper.Clamp(_lowerImpulse + impulse, -0f, 0f);
                _upperImpulse = MathHelper.Clamp(_upperImpulse + impulse, 0f, 0f);
                impulse = _upperImpulse - oldUpperImpulse + _lowerImpulse - oldLowerImpulse;

                Vec2 P2 = Mul(impulse, new Vec2(_uX, _uY));

                vA2.X += _invMassA * P2.X;
                vA2.Y += _invMassA * P2.Y;
                wA -= _invIA * Cross(rA, P2);
                vB2.X -= _invMassB * P2.X;
                vB2.Y -= _invMassB * P2.Y;
                wB += _invIB * Cross(rB, P2);
            }

            data.VelocityA = vA;
            data.AngularVelocityA = wA;
            data.VelocityB = vB;
            data.AngularVelocityB = wB;
        }

        public override bool SolvePositionConstraints(SolverData data)
        {
            if (_lengthEnabled)
            {
                Vec2 rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
                Vec2 rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

                Vec2 d = Sub(Sub(Add(data.PositionB, rB), data.PositionA), rA);

                float length = d.Length();
                Vec2 u = d;
                if (length > MathHelper.Epsilon)
                    u = Mul(1f / length, d);
                else
                    u = new Vec2(0f, 0f);

                float C = length - _length;
                C = MathHelper.Clamp(C, _minLength - _length, _maxLength - _length);
                float impulse = -_mass * C;
                Vec2 P = Mul(impulse, u);

                data.PositionA = Add(data.PositionA, Mul(_invMassA, P));
                data.AngleA -= _invIA * Cross(rA, P);
                data.PositionB = Sub(data.PositionB, Mul(_invMassB, P));
                data.AngleB += _invIB * Cross(rB, P);

                return MathHelper.Abs(C) < 0.005f;
            }

            return true;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            Vec2 P = Mul(_impulse + _lowerImpulse + _upperImpulse, new Vec2(_uX, _uY));
            return new Vector2(P.X * invDt, P.Y * invDt);
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0f;
        }

        public void SetLength(float length)
        {
            _length = Math.Max(0f, length);
        }

        public void SetMinLength(float minLength)
        {
            _minLength = Math.Max(0f, Math.Min(minLength, _maxLength));
        }

        public void SetMaxLength(float maxLength)
        {
            _maxLength = Math.Max(0f, Math.Max(maxLength, _minLength));
        }

        public void SetStiffness(float stiffness)
        {
            _stiffness = stiffness;
        }

        public void SetDamping(float damping)
        {
            _damping = damping;
        }
    }

    public class SpringJoint : Joint
    {
        private float _naturalLength;
        private float _stiffness;
        private float _damping;
        private float _localAnchorA;
        private float _localAnchorB;
        private float _bias;
        private float _gamma;
        private float _mass;
        private float _currentLength;
        private float _uX;
        private float _uY;

        public float NaturalLength
        {
            get => _naturalLength;
            set => _naturalLength = Math.Max(0f, value);
        }

        public float Stiffness
        {
            get => _stiffness;
            set => _stiffness = Math.Max(0f, value);
        }

        public float Damping
        {
            get => _damping;
            set => _damping = Math.Max(0f, value);
        }

        public float CurrentLength => _currentLength;
        public float PotentialEnergy => 0.5f * _stiffness * MathHelper.Pow(_currentLength - _naturalLength, 2f);

        public SpringJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchorA, Vector2 anchorB) : base(JointType.Spring, bodyA, bodyB)
        {
            _localAnchorA = GetBodyAAnchor(anchorA);
            _localAnchorB = GetBodyBAnchor(anchorB);
            _naturalLength = (anchorA - anchorB).Length();
            _stiffness = 0f;
            _damping = 0f;
            _bias = 0f;
            _gamma = 0f;
            _mass = 0f;
            _currentLength = 0f;
        }

        public override void InitializeVelocityConstraints(SolverData data)
        {
            _invMassA = data.InvMassA;
            _invMassB = data.InvMassB;
            _invIA = data.InvIA;
            _invIB = data.InvIB;

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            Vec2 d = Sub(Sub(Add(data.PositionB, rB), data.PositionA), rA);
            _currentLength = d.Length();

            Vec2 u = d;
            if (_currentLength > MathHelper.Epsilon)
                u = Mul(1f / _currentLength, d);
            else
                u = new Vec2(0f, 0f);

            _uX = u.X;
            _uY = u.Y;

            float crA = Cross(rA, u);
            float crB = Cross(rB, u);
            float invMass = _invMassA + _invIA * crA * crA + _invMassB + _invIB * crB * crB;

            _mass = invMass > 0f ? 1f / invMass : 0f;

            if (_stiffness > 0f || _damping > 0f)
            {
                float C = _currentLength - _naturalLength;
                float omega = MathHelper.Sqrt(_stiffness / invMass);
                float d2 = 2f * invMass * _damping * omega;
                float k = invMass * omega * omega;

                _gamma = data.DeltaTime * (d2 + data.DeltaTime * k);
                if (_gamma > 0f)
                    _gamma = 1f / _gamma;

                _bias = C * data.DeltaTime * k * _gamma;

                float springMass = invMass + _gamma;
                if (springMass > 0f)
                    _mass = 1f / springMass;
            }

            WakeBodies();
        }

        public override void SolveVelocityConstraints(SolverData data)
        {
            Vec2 vA = data.VelocityA;
            Vec2 vB = data.VelocityB;
            float wA = data.AngularVelocityA;
            float wB = data.AngularVelocityB;

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            Vec2 vA2 = new Vec2(vA.X, vA.Y);
            Vec2 vB2 = new Vec2(vB.X, vB.Y);

            float Cdot = _uX * (vB2.X - vA2.X) + _uY * (vB2.Y - vA2.Y);

            float impulse = -_mass * (Cdot + _bias + _gamma * 0f);
            Vec2 P = Mul(impulse, new Vec2(_uX, _uY));

            vA2.X += _invMassA * P.X;
            vA2.Y += _invMassA * P.Y;
            wA -= _invIA * Cross(rA, P);
            vB2.X -= _invMassB * P.X;
            vB2.Y -= _invMassB * P.Y;
            wB += _invIB * Cross(rB, P);

            data.VelocityA = vA;
            data.AngularVelocityA = wA;
            data.VelocityB = vB;
            data.AngularVelocityB = wB;
        }

        public override bool SolvePositionConstraints(SolverData data)
        {
            Vec2 rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            Vec2 d = Sub(Sub(Add(data.PositionB, rB), data.PositionA), rA);

            float length = d.Length();
            Vec2 u = d;
            if (length > MathHelper.Epsilon)
                u = Mul(1f / length, d);
            else
                u = new Vec2(0f, 0f);

            float C = length - _naturalLength;
            float impulse = -_mass * C;
            Vec2 P = Mul(impulse, u);

            data.PositionA = Add(data.PositionA, Mul(_invMassA, P));
            data.AngleA -= _invIA * Cross(rA, P);
            data.PositionB = Sub(data.PositionB, Mul(_invMassB, P));
            data.AngleB += _invIB * Cross(rB, P);

            return MathHelper.Abs(C) < 0.005f;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return Vector2.Zero;
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0f;
        }

        public void SetSpring(float stiffness, float damping)
        {
            _stiffness = stiffness;
            _damping = damping;
        }
    }

    public class WeldJoint : Joint
    {
        private float _referenceAngle;
        private float _stiffness;
        private float _damping;
        private float _beta;
        private float _gamma;
        private Vec2 _rA;
        private Vec2 _rB;
        private Vec2 _localCenterA;
        private Vec2 _localCenterB;
        private float _invMassA;
        private float _invMassB;
        private float _invIA;
        private float _invIB;
        private Mat22 _mass;
        private float _angularMass;
        private float _impulse;

        public float ReferenceAngle
        {
            get => _referenceAngle;
            set => _referenceAngle = value;
        }

        public float Stiffness
        {
            get => _stiffness;
            set => _stiffness = Math.Max(0f, value);
        }

        public float Damping
        {
            get => _damping;
            set => _damping = Math.Max(0f, value);
        }

        public WeldJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 worldAnchor) : base(JointType.Weld, bodyA, bodyB)
        {
            _localAnchorA = GetBodyAAnchor(worldAnchor);
            _localAnchorB = GetBodyBAnchor(worldAnchor);
            _referenceAngle = _bodyA != null && _bodyB != null ? _bodyA.Rotation - _bodyB.Rotation : 0f;
            _stiffness = 0f;
            _damping = 0f;
            _beta = 0f;
            _gamma = 0f;
            _impulse = 0f;
        }

        public override void InitializeVelocityConstraints(SolverData data)
        {
            _invMassA = data.InvMassA;
            _invMassB = data.InvMassB;
            _invIA = data.InvIA;
            _invIB = data.InvIB;

            _rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            _rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            _invMassA = data.InvMassA;
            _invMassB = data.InvMassB;
            _invIA = data.InvIA;
            _invIB = data.InvIB;

            Mat22 mA = new Mat22(_invMassA, 0f, 0f, _invIA);
            Mat22 mB = new Mat22(_invMassB, 0f, 0f, _invIB);

            Mat22 k1 = Mat22.Add(mA, mB);
            Mat22 k2 = new Mat22(_invIA * _rA.Y * _rA.Y, -_invIA * _rA.X * _rA.Y, -_invIA * _rA.X * _rA.Y, _invIA * _rA.X * _rA.X);
            Mat22 k3 = new Mat22(_invIB * _rB.Y * _rB.Y, -_invIB * _rB.X * _rB.Y, -_invIB * _rB.X * _rB.Y, _invIB * _rB.X * _rB.X);

            _mass = Mat22.Add(Mat22.Add(k1, k2), k3);
            _angularMass = _invIA + _invIB;
            if (_angularMass > 0f)
                _angularMass = 1f / _angularMass;

            if (_stiffness > 0f || _damping > 0f)
            {
                float C = data.AngleB - data.AngleA - _referenceAngle;
                float omega = MathHelper.Sqrt(_stiffness / _angularMass);
                float d = 2f * _angularMass * _damping * omega;
                float k = _angularMass * omega * omega;

                _gamma = data.DeltaTime * (d + data.DeltaTime * k);
                if (_gamma > 0f)
                    _gamma = 1f / _gamma;

                _beta = data.DeltaTime * k * _gamma;
            }

            WakeBodies();
        }

        public override void SolveVelocityConstraints(SolverData data)
        {
            Vec2 vA = data.VelocityA;
            float wA = data.AngularVelocityA;
            Vec2 vB = data.VelocityB;
            float wB = data.AngularVelocityB;

            float mA = _invMassA;
            float mB = _invMassB;
            float iA = _invIA;
            float iB = _invIB;

            if (_stiffness > 0f || _damping > 0f)
            {
                float Cdot = wB - wA;
                float motor = -_angularMass * Cdot;
                float maxMotor = float.MaxValue;
                float oldMotorImpulse = _impulse;
                _impulse = MathHelper.Clamp(_impulse + motor, -maxMotor, maxMotor);
                motor = _impulse - oldMotorImpulse;

                wA -= iA * motor;
                wB += iB * motor;
            }

            Vec2 cdotA = Sub(vA, new Vec2(-wA * _rA.Y, wA * _rA.X));
            Vec2 cdotB = Sub(vB, new Vec2(-wB * _rB.Y, wB * _rB.X));
            Vec2 cdot = Sub(cdotB, cdotA);

            Vec2 impulse = Mat22.MulT(_mass, cdot);
            _impulse += impulse.X;

            Vec2 P = impulse;
            vA = Sub(vA, Mul(mA, P));
            wA -= iA * Cross(_rA, P);
            vB = Add(vB, Mul(mB, P));
            wB += iB * Cross(_rB, P);

            data.VelocityA = vA;
            data.AngularVelocityA = wA;
            data.VelocityB = vB;
            data.AngularVelocityB = wB;
        }

        public override bool SolvePositionConstraints(SolverData data)
        {
            Vec2 cA = data.PositionA;
            float aA = data.AngleA;
            Vec2 cB = data.PositionB;
            float aB = data.AngleB;

            float mA = _invMassA;
            float mB = _invMassB;
            float iA = _invIA;
            float iB = _invIB;

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(aA), MathHelper.Sin(aA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(aB), MathHelper.Sin(aB)), _localAnchorB);

            Vec2 positionError = Vector2.Zero;
            float angularError = 0f;

            Mat22 k1 = new Mat22(mA + mB, 0f, 0f, mA + mB);
            Mat22 k2 = new Mat22(iA * rA.Y * rA.Y, -iA * rA.X * rA.Y, -iA * rA.X * rA.Y, iA * rA.X * rA.X);
            Mat22 k3 = new Mat22(iB * rB.Y * rB.Y, -iB * rB.X * rB.Y, -iB * rB.X * rB.Y, iB * rB.X * rB.X);

            Mat22 m = Mat22.Add(Mat22.Add(k1, k2), k3);

            Vec2 C = Sub(Sub(Add(cB, rB), cA), rA);
            positionError = new Vec2(MathHelper.Abs(C.X), MathHelper.Abs(C.Y));

            Vec2 impulse = Mat22.MulT(m, C);
            Vec2 P = impulse;

            cA = Sub(cA, Mul(mA, P));
            aA -= iA * Cross(rA, P);
            cB = Add(cB, Mul(mB, P));
            aB += iB * Cross(rB, P);

            float CAngular = aB - aA - _referenceAngle;
            angularError = MathHelper.Abs(CAngular);

            float angularMass = iA + iB;
            if (angularMass > 0f)
            {
                float angularImpulse = -angularMass * CAngular;
                aA -= iA * angularImpulse;
                aB += iB * angularImpulse;
            }

            data.PositionA = cA;
            data.AngleA = aA;
            data.PositionB = cB;
            data.AngleB = aB;

            return positionError.X < 0.005f && positionError.Y < 0.005f && angularError < 0.005f;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return new Vector2(_impulse * invDt, 0f);
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0f;
        }
    }

    public class RopeJoint : Joint
    {
        private float _maxLength;
        private float _currentLength;
        private LimitState _limitState;
        private Vec2 _u;
        private Vec2 _rA;
        private Vec2 _rB;
        private Vec2 _d;
        private float _invMassA;
        private float _invMassB;
        private float _invIA;
        private float _invIB;
        private float _mass;
        private float _impulse;
        private float _limitMass;
        private float _limitImpulse;

        public float MaxLength
        {
            get => _maxLength;
            set => _maxLength = Math.Max(0f, value);
        }

        public float CurrentLength => _currentLength;
        public LimitState LimitState => _limitState;

        public RopeJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchorA, Vector2 anchorB) : base(JointType.Rope, bodyA, bodyB)
        {
            _localAnchorA = anchorA;
            _localAnchorB = anchorB;
            _maxLength = (_localAnchorA - _localAnchorB).Length();
            _limitState = LimitState.Inactive;
            _currentLength = 0f;
            _impulse = 0f;
            _limitImpulse = 0f;
        }

        public override void InitializeVelocityConstraints(SolverData data)
        {
            _invMassA = data.InvMassA;
            _invMassB = data.InvMassB;
            _invIA = data.InvIA;
            _invIB = data.InvIB;

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            Vec2 d = Sub(Sub(Add(data.PositionB, rB), data.PositionA), rA);
            _currentLength = d.Length();

            if (_currentLength > MathHelper.Epsilon)
                _u = Mul(1f / _currentLength, d);
            else
                _u = new Vec2(0f, 0f);

            _rA = rA;
            _rB = rB;

            float crA = Cross(rA, _u);
            float crB = Cross(rB, _u);
            float invMass = _invMassA + _invIA * crA * crA + _invMassB + _invIB * crB * crB;

            _mass = invMass > 0f ? 1f / invMass : 0f;

            _limitState = LimitStateHelper.ComputeLimitState(0f, _maxLength, _currentLength, 0.005f);

            WakeBodies();
        }

        public override void SolveVelocityConstraints(SolverData data)
        {
            Vec2 vA = data.VelocityA;
            Vec2 vB = data.VelocityB;
            float wA = data.AngularVelocityA;
            float wB = data.AngularVelocityB;

            Vec2 vA2 = Sub(vA, new Vec2(-wA * _rA.Y, wA * _rA.X));
            Vec2 vB2 = Sub(vB, new Vec2(-wB * _rB.Y, wB * _rB.X));

            float Cdot = _u.X * (vB2.X - vA2.X) + _u.Y * (vB2.Y - vA2.Y);

            if (_limitState != LimitState.Inactive)
            {
                Vec2 vA3 = new Vec2(vA.X, vA.Y);
                Vec2 vB3 = new Vec2(vB.X, vB.Y);

                float Cdot2 = _u.X * (vB3.X - vA3.X) + _u.Y * (vB3.Y - vA3.Y);

                float impulse = -_mass * Cdot2;
                float oldLimitImpulse = _limitImpulse;
                float maxImpulse = float.MaxValue;
                _limitImpulse = MathHelper.Clamp(_limitImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _limitImpulse - oldLimitImpulse;

                Vec2 P = Mul(impulse, _u);

                vA3.X -= _invMassA * P.X;
                vA3.Y -= _invMassA * P.Y;
                wA -= _invIA * Cross(_rA, P);
                vB3.X += _invMassB * P.X;
                vB3.Y += _invMassB * P.Y;
                wB += _invIB * Cross(_rB, P);
            }

            float impulse2 = -_mass * Cdot;
            _impulse += impulse2;

            Vec2 P2 = Mul(impulse2, _u);

            vA.X -= _invMassA * P2.X;
            vA.Y -= _invMassA * P2.Y;
            wA -= _invIA * Cross(_rA, P2);
            vB.X += _invMassB * P2.X;
            vB.Y += _invMassB * P2.Y;
            wB += _invIB * Cross(_rB, P2);

            data.VelocityA = vA;
            data.AngularVelocityA = wA;
            data.VelocityB = vB;
            data.AngularVelocityB = wB;
        }

        public override bool SolvePositionConstraints(SolverData data)
        {
            Vec2 cA = data.PositionA;
            float aA = data.AngleA;
            Vec2 cB = data.PositionB;
            float aB = data.AngleB;

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(aA), MathHelper.Sin(aA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(aB), MathHelper.Sin(aB)), _localAnchorB);

            Vec2 d = Sub(Sub(Add(cB, rB), cA), rA);

            float length = d.Length();
            Vec2 u = d;
            if (length > MathHelper.Epsilon)
                u = Mul(1f / length, d);
            else
                u = new Vec2(0f, 0f);

            float C = length - _maxLength;
            C = MathHelper.Clamp(C, 0f, float.MaxValue);

            float impulse = -_mass * C;
            Vec2 P = Mul(impulse, u);

            cA.X -= _invMassA * P.X;
            cA.Y -= _invMassA * P.Y;
            aA -= _invIA * Cross(rA, P);
            cB.X += _invMassB * P.X;
            cB.Y += _invMassB * P.Y;
            aB += _invIB * Cross(rB, P);

            data.PositionA = cA;
            data.AngleA = aA;
            data.PositionB = cB;
            data.AngleB = aB;

            return length - _maxLength < 0.005f;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            Vec2 P = Mul(_impulse + _limitImpulse, _u);
            return new Vector2(P.X * invDt, P.Y * invDt);
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0f;
        }

        public void SetMaxLength(float length)
        {
            _maxLength = Math.Max(0f, length);
        }
    }

    public class PulleyJoint : Joint
    {
        private float _groundAnchorA;
        private float _groundAnchorB;
        private float _lengthA;
        private float _lengthB;
        private float _ratio;
        private float _currentLengthA;
        private float _currentLengthB;
        private float _limitStateA;
        private float _limitStateB;
        private float _uA;
        private float _uB;
        private float _rA;
        private float _rB;
        private float _groundAnchorAx;
        private float _groundAnchorAy;
        private float _groundAnchorBx;
        private float _groundAnchorBy;
        private float _invMassA;
        private float _invMassB;
        private float _invIA;
        private float _invIB;
        private float _mass;
        private float _impulse;

        public Vector2 GroundAnchorA => new Vector2(_groundAnchorAx, _groundAnchorAy);
        public Vector2 GroundAnchorB => new Vector2(_groundAnchorBx, _groundAnchorBy);

        public float LengthA
        {
            get => _lengthA;
            set => _lengthA = Math.Max(0f, value);
        }

        public float LengthB
        {
            get => _lengthB;
            set => _lengthB = Math.Max(0f, value);
        }

        public float Ratio
        {
            get => _ratio;
            set => _ratio = Math.Max(0f, value);
        }

        public float CurrentLengthA => _currentLengthA;
        public float CurrentLengthB => _currentLengthB;

        public PulleyJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 groundAnchorA, Vector2 groundAnchorB, Vector2 anchorA, Vector2 anchorB, float ratio) : base(JointType.Pulley, bodyA, bodyB)
        {
            _groundAnchorAx = groundAnchorA.X;
            _groundAnchorAy = groundAnchorA.Y;
            _groundAnchorBx = groundAnchorB.X;
            _groundAnchorBy = groundAnchorB.Y;
            _localAnchorA = GetBodyAAnchor(anchorA);
            _localAnchorB = GetBodyBAnchor(anchorB);
            _lengthA = (anchorA - groundAnchorA).Length();
            _lengthB = (anchorB - groundAnchorB).Length();
            _ratio = ratio;
            _impulse = 0f;
        }

        public override void InitializeVelocityConstraints(SolverData data)
        {
            _invMassA = data.InvMassA;
            _invMassB = data.InvMassB;
            _invIA = data.InvIA;
            _invIB = data.InvIB;

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            Vec2 pA = Sub(Sub(data.PositionA, rA), new Vec2(_groundAnchorAx, _groundAnchorAy));
            Vec2 pB = Sub(Sub(data.PositionB, rB), new Vec2(_groundAnchorBx, _groundAnchorBy));

            _currentLengthA = pA.Length();
            _currentLengthB = pB.Length();

            Vec2 uA = pA;
            if (_currentLengthA > MathHelper.Epsilon)
                uA = Mul(1f / _currentLengthA, pA);
            else
                uA = new Vec2(0f, 0f);

            Vec2 uB = pB;
            if (_currentLengthB > MathHelper.Epsilon)
                uB = Mul(1f / _currentLengthB, pB);
            else
                uB = new Vec2(0f, 0f);

            _uA = uA.X;
            _uB = uB.X;
            float sA = Cross(rA, uA);
            float sB = Cross(rB, uB);

            float invMass = _invMassA + _invIA * sA * sA + _invMassB + _invIB * sB * sB;
            _mass = invMass > 0f ? 1f / invMass : 0f;

            _rA = rA.X;
            _rB = rB.X;

            WakeBodies();
        }

        public override void SolveVelocityConstraints(SolverData data)
        {
            Vec2 vA = data.VelocityA;
            Vec2 vB = data.VelocityB;
            float wA = data.AngularVelocityA;
            float wB = data.AngularVelocityB;

            Vec2 rA = new Vec2(_rA * MathHelper.Cos(data.AngleA - data.AngleA), _rA * MathHelper.Sin(data.AngleA - data.AngleA));
            Vec2 rB = new Vec2(_rB * MathHelper.Cos(data.AngleB - data.AngleB), _rB * MathHelper.Sin(data.AngleB - data.AngleB));

            Vec2 vA2 = Sub(vA, new Vec2(-wA * rA.Y, wA * rA.X));
            Vec2 vB2 = Sub(vB, new Vec2(-wB * rB.Y, wB * rB.X));

            float Cdot = -_uA * (vA2.X) - _uB * (vB2.X);

            float impulse = -_mass * Cdot;
            _impulse += impulse;

            Vec2 PA = Mul(-impulse, new Vec2(_uA, 0f));
            Vec2 PB = Mul(_ratio * impulse, new Vec2(_uB, 0f));

            vA = Sub(vA, Mul(_invMassA, PA));
            wA -= _invIA * Cross(rA, PA);
            vB = Add(vB, Mul(_invMassB, PB));
            wB += _invIB * Cross(rB, PB);

            data.VelocityA = vA;
            data.AngularVelocityA = wA;
            data.VelocityB = vB;
            data.AngularVelocityB = wB;
        }

        public override bool SolvePositionConstraints(SolverData data)
        {
            Vec2 rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            Vec2 pA = Sub(Sub(data.PositionA, rA), new Vec2(_groundAnchorAx, _groundAnchorAy));
            Vec2 pB = Sub(Sub(data.PositionB, rB), new Vec2(_groundAnchorBx, _groundAnchorBy));

            float lengthA = pA.Length();
            Vec2 uA = pA;
            if (lengthA > MathHelper.Epsilon)
                uA = Mul(1f / lengthA, pA);
            else
                uA = new Vec2(0f, 0f);

            float lengthB = pB.Length();
            Vec2 uB = pB;
            if (lengthB > MathHelper.Epsilon)
                uB = Mul(1f / lengthB, pB);
            else
                uB = new Vec2(0f, 0f);

            float C = -lengthA - _ratio * lengthB;
            C += _lengthA + _ratio * _lengthB;
            C = MathHelper.Clamp(C, -0.005f, float.MaxValue);

            float impulse = -_mass * C;
            Vec2 PA = Mul(-impulse, uA);
            Vec2 PB = Mul(_ratio * impulse, uB);

            data.PositionA = Add(data.PositionA, Mul(_invMassA, PA));
            data.AngleA -= _invIA * Cross(rA, PA);
            data.PositionB = Sub(data.PositionB, Mul(_invMassB, PB));
            data.AngleB += _invIB * Cross(rB, PB);

            return MathHelper.Abs(C) < 0.005f;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            Vec2 P = Mul(_impulse, new Vec2(_uA, 0f));
            return new Vector2(P.X * invDt, P.Y * invDt);
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0f;
        }

        public void SetLengths(float lengthA, float lengthB)
        {
            _lengthA = Math.Max(0f, lengthA);
            _lengthB = Math.Max(0f, lengthB);
        }

        public void SetRatio(float ratio)
        {
            _ratio = Math.Max(0f, ratio);
        }
    }

    public class WheelJoint : Joint
    {
        private Vec2 _axis;
        private float _motorMass;
        private float _motorImpulse;
        private float _springMass;
        private float _springImpulse;
        private float _lowerLimit;
        private float _upperLimit;
        private float _maxMotorTorque;
        private float _motorSpeed;
        private float _stiffness;
        private float _damping;
        private LimitState _limitState;
        private float _localAxis;
        private Vec2 _perp;
        private float _s1, _s2;
        private float _a1, _a2;
        private float _rA;
        private float _rB;
        private float _d;
        private float _invMassA;
        private float _invMassB;
        private float _invIA;
        private float _invIB;
        private float _motor;
        private float _spring;
        private float _bias;
        private float _gamma;
        private float _mass;

        public Vec2 Axis
        {
            get => _axis;
            set => _axis = value;
        }

        public float MotorSpeed
        {
            get => _motorSpeed;
            set
            {
                _motorSpeed = value;
                _enableMotor = true;
            }
        }

        public float MaxMotorTorque
        {
            get => _maxMotorTorque;
            set
            {
                _maxMotorTorque = value;
                _enableMotor = true;
            }
        }

        public bool MotorEnabled
        {
            get => _enableMotor;
            set => _enableMotor = value;
        }

        public float LowerLimit
        {
            get => _lowerLimit;
            set => _lowerLimit = value;
        }

        public float UpperLimit
        {
            get => _upperLimit;
            set => _upperLimit = value;
        }

        public bool LimitEnabled
        {
            get => _enableLimits;
            set => _enableLimits = value;
        }

        public float Stiffness
        {
            get => _stiffness;
            set => _stiffness = value;
        }

        public float Damping
        {
            get => _damping;
            set => _damping = value;
        }

        public float CurrentSpeed => _motorSpeed;
        public LimitState LimitState => _limitState;

        public WheelJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 worldAnchor, Vec2 axis) : base(JointType.Wheel, bodyA, bodyB)
        {
            _localAnchorA = GetBodyAAnchor(worldAnchor);
            _localAnchorB = GetBodyBAnchor(worldAnchor);
            _axis = axis;
            _motorSpeed = 0f;
            _maxMotorTorque = 0f;
            _enableMotor = false;
            _stiffness = 0f;
            _damping = 0f;
            _lowerLimit = 0f;
            _upperLimit = 0f;
            _enableLimits = false;
            _motorImpulse = 0f;
            _springImpulse = 0f;
        }

        public override void InitializeVelocityConstraints(SolverData data)
        {
            _invMassA = data.InvMassA;
            _invMassB = data.InvMassB;
            _invIA = data.InvIA;
            _invIB = data.InvIB;

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(data.AngleA), MathHelper.Sin(data.AngleA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(data.AngleB), MathHelper.Sin(data.AngleB)), _localAnchorB);

            Vec2 d = Sub(Sub(Add(data.PositionB, rB), data.PositionA), rA);

            _perp = new Vec2(-_axis.Y, _axis.X);

            float s1 = Cross(Add(d, rA), _perp);
            float s2 = Cross(rB, _perp);

            float a1 = Cross(Add(d, rA), _axis);
            float a2 = Cross(rB, _axis);

            float k11 = _invMassA + _invMassB + _invIA * s1 * s1 + _invIB * s2 * s2;
            float k12 = _invMassA + _invMassB + _invIA * a1 * a1 + _invIB * a2 * a2;

            _mass = k11 > 0f ? 1f / k11 : 0f;

            _motorMass = _invMassA + _invMassB + _invIA * a1 * a1 + _invIB * a2 * a2;
            if (_motorMass > 0f)
                _motorMass = 1f / _motorMass;

            if (_stiffness > 0f || _damping > 0f)
            {
                float c = Dot(_axis, d);
                float omega = MathHelper.Sqrt(_stiffness / _motorMass);
                float d2 = 2f * _motorMass * _damping * omega;
                float k = _motorMass * omega * omega;

                _gamma = data.DeltaTime * (d2 + data.DeltaTime * k);
                if (_gamma > 0f)
                    _gamma = 1f / _gamma;

                _bias = c * data.DeltaTime * k * _gamma;
                _springMass = _motorMass + _gamma;
                if (_springMass > 0f)
                    _springMass = 1f / _springMass;
            }

            if (_enableLimits)
            {
                float translation = Dot(_axis, d);
                _limitState = LimitStateHelper.ComputeLimitState(_lowerLimit, _upperLimit, translation, 0.005f);
            }
            else
            {
                _limitState = LimitState.Inactive;
            }

            _s1 = s1;
            _s2 = s2;
            _a1 = a1;
            _a2 = a2;
            _rA = rA.X;
            _rB = rB.X;
            _d = d.X;

            WakeBodies();
        }

        public override void SolveVelocityConstraints(SolverData data)
        {
            Vec2 vA = data.VelocityA;
            Vec2 vB = data.VelocityB;
            float wA = data.AngularVelocityA;
            float wB = data.AngularVelocityB;

            float mA = _invMassA;
            float mB = _invMassB;
            float iA = _invIA;
            float iB = _invIB;

            if (_enableMotor && _limitState != LimitState.AtUpper)
            {
                float Cdot = Dot(_axis, Sub(vB, vA)) + _a2 * wB - _a1 * wA;
                float motor = -_motorMass * (Cdot - _motorSpeed);
                float oldMotorImpulse = _motorImpulse;
                float maxMotor = _maxMotorTorque * data.DeltaTimeRatio;
                _motorImpulse = MathHelper.Clamp(_motorImpulse + motor, -maxMotor, maxMotor);
                motor = _motorImpulse - oldMotorImpulse;

                Vec2 P = Mul(motor, _axis);
                float wP = motor * _a1;
                float wR = motor * _a2;

                vA = Sub(vA, Mul(mA, P));
                wA -= iA * wP;
                vB = Add(vB, Mul(mB, P));
                wB += iB * wR;
            }

            if (_stiffness > 0f && _limitState != LimitState.Inactive)
            {
                Vec2 vA2 = Sub(vA, new Vec2(-wA * _d, wA * _d));
                Vec2 vB2 = Sub(vB, new Vec2(-wB * _d, wB * _d));

                float Cdot = Dot(_axis, Sub(vB2, vA2));
                float springMotor = -_springMass * Cdot;
                float oldSpringImpulse = _springImpulse;
                float maxSpring = float.MaxValue;
                _springImpulse = MathHelper.Clamp(_springImpulse + springMotor, -maxSpring, maxSpring);
                springMotor = _springImpulse - oldSpringImpulse;

                Vec2 P = Mul(springMotor, _axis);
                float wP = springMotor * _s1;
                float wR = springMotor * _s2;

                vA = Sub(vA, Mul(mA, P));
                wA -= iA * wP;
                vB = Add(vB, Mul(mB, P));
                wB += iB * wR;
            }

            float Cdot2 = Dot(_perp, Sub(vB, vA)) + _s2 * wB - _s1 * wA;
            float impulse = -_mass * Cdot2;
            _springImpulse += impulse;

            Vec2 P2 = Mul(impulse, _perp);
            float wP2 = impulse * _s1;
            float wR2 = impulse * _s2;

            vA = Sub(vA, Mul(mA, P2));
            wA -= iA * wP2;
            vB = Add(vB, Mul(mB, P2));
            wB += iB * wR2;

            data.VelocityA = vA;
            data.AngularVelocityA = wA;
            data.VelocityB = vB;
            data.AngularVelocityB = wB;
        }

        public override bool SolvePositionConstraints(SolverData data)
        {
            Vec2 cA = data.PositionA;
            float aA = data.AngleA;
            Vec2 cB = data.PositionB;
            float aB = data.AngleB;

            Vec2 rA = Mul(new Vec2(MathHelper.Cos(aA), MathHelper.Sin(aA)), _localAnchorA);
            Vec2 rB = Mul(new Vec2(MathHelper.Cos(aB), MathHelper.Sin(aB)), _localAnchorB);

            Vec2 d = Sub(Sub(Add(cB, rB), cA), rA);

            Vec2 perp = new Vec2(-_axis.Y, _axis.X);

            float s1 = Cross(Add(d, rA), perp);
            float s2 = Cross(rB, perp);

            float a3 = Cross(Add(d, rA), _axis);
            float a4 = Cross(rB, _axis);

            float k11 = _invMassA + _invMassB + _invIA * s1 * s1 + _invIB * s2 * s2;
            float k12 = _invMassA + _invMassB + _invIA * a3 * a3 + _invIB * a4 * a4;
            float k = k11 > 0f ? 1f / k11 : 0f;

            float C = Dot(perp, d);
            float impulse = -k * C;
            Vec2 P = Mul(impulse, perp);

            cA = Sub(cA, Mul(_invMassA, P));
            aA -= _invIA * impulse * s1;
            cB = Add(cB, Mul(_invMassB, P));
            aB += _invIB * impulse * s2;

            float linearError = MathHelper.Abs(C);
            return linearError < 0.005f;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return Vector2.Zero;
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0f;
        }

        public void EnableMotor(float speed, float maxTorque)
        {
            _motorSpeed = speed;
            _maxMotorTorque = maxTorque;
            _enableMotor = true;
        }

        public void DisableMotor()
        {
            _enableMotor = false;
        }

        public void SetLimits(float lower, float upper)
        {
            _lowerLimit = lower;
            _upperLimit = upper;
            _enableLimits = true;
        }

        public void DisableLimits()
        {
            _enableLimits = false;
        }

        public void SetSpring(float stiffness, float damping)
        {
            _stiffness = stiffness;
            _damping = damping;
        }
    }

    public class GearJoint : Joint
    {
        private Joint? _jointA;
        private Joint? _jointB;
        private JointType _typeA;
        private JointType _typeB;
        private float _ratioA;
        private float _ratioB;
        private float _referenceAngleA;
        private float _referenceAngleB;
        private float _gearConstant;
        private float _impulse;
        private float _mass;
        private float _tempAngle;
        private float _axisA;
        private float _axisB;
        private float _perpA;
        private float _perpB;

        public Joint? JointA => _jointA;
        public Joint? JointB => _jointB;

        public float RatioA
        {
            get => _ratioA;
            set => _ratioA = value;
        }

        public float RatioB
        {
            get => _ratioB;
            set => _ratioB = value;
        }

        public float GearRatio => _ratioA / _ratioB;

        public GearJoint(RigidBody? bodyA, RigidBody? bodyB, Joint jointA, Joint jointB, float ratio) : base(JointType.Gear, bodyA, bodyB)
        {
            _jointA = jointA;
            _jointB = jointB;
            _ratioA = ratio;
            _ratioB = 1f;
            _referenceAngleA = 0f;
            _referenceAngleB = 0f;
            _gearConstant = ratio;
            _impulse = 0f;
            _mass = 0f;
        }

        public override void InitializeVelocityConstraints(SolverData data)
        {
            _invMassA = data.InvMassA;
            _invMassB = data.InvMassB;
            _invIA = data.InvIA;
            _invIB = data.InvIB;

            float I_A = data.InvIA;
            float I_B = data.InvIB;

            float m_A = _invMassA;
            float m_B = _invMassB;

            if (_jointA is HingeJoint hingeA)
            {
                _referenceAngleA = hingeA.ReferenceAngle;
            }
            else if (_jointA is SliderJoint sliderA)
            {
                _referenceAngleA = sliderA.ReferenceAngle;
            }

            if (_jointB is HingeJoint hingeB)
            {
                _referenceAngleB = hingeB.ReferenceAngle;
            }
            else if (_jointB is SliderJoint sliderB)
            {
                _referenceAngleB = sliderB.ReferenceAngle;
            }

            _mass = m_A + m_B + I_A * _ratioA * _ratioA + I_B * _ratioB * _ratioB;
            if (_mass > 0f)
                _mass = 1f / _mass;

            WakeBodies();
        }

        public override void SolveVelocityConstraints(SolverData data)
        {
            Vec2 vA = data.VelocityA;
            Vec2 vB = data.VelocityB;
            float wA = data.AngularVelocityA;
            float wB = data.AngularVelocityB;

            float Cdot = _ratioA * wA - _ratioB * wB;

            float impulse = -_mass * Cdot;
            _impulse += impulse;

            wA += _invIA * _ratioA * impulse;
            wB -= _invIB * _ratioB * impulse;

            data.VelocityA = vA;
            data.AngularVelocityA = wA;
            data.VelocityB = vB;
            data.AngularVelocityB = wB;
        }

        public override bool SolvePositionConstraints(SolverData data)
        {
            Vec2 cA = data.PositionA;
            float aA = data.AngleA;
            Vec2 cB = data.PositionB;
            float aB = data.AngleB;

            float C = _ratioA * (aA - _referenceAngleA) - _ratioB * (aB - _referenceAngleB);
            float impulse = -_mass * C;
            aA += _invIA * _ratioA * impulse;
            aB -= _invIB * _ratioB * impulse;

            data.PositionA = cA;
            data.AngleA = aA;
            data.PositionB = cB;
            data.AngleB = aB;

            return MathHelper.Abs(C) < 0.005f;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return Vector2.Zero;
        }

        public override float GetReactionTorque(float invDt)
        {
            return 0f;
        }

        public void SetRatio(float ratio)
        {
            _ratioA = ratio;
        }
    }

    public class Mat22
    {
        public float ExX, ExY, EyX, EyY;

        public Mat22(float exX, float exY, float eyX, float eyY)
        {
            ExX = exX;
            ExY = exY;
            EyX = eyX;
            EyY = eyY;
        }

        public static Mat22 Identity => new Mat22(1f, 0f, 0f, 1f);

        public static Mat22 Add(Mat22 a, Mat22 b)
        {
            return new Mat22(a.ExX + b.ExX, a.ExY + b.ExY, a.EyX + b.EyX, a.EyY + b.EyY);
        }

        public static Mat22 Sub(Mat22 a, Mat22 b)
        {
            return new Mat22(a.ExX - b.ExX, a.ExY - b.ExY, a.EyX - b.EyX, a.EyY - b.EyY);
        }

        public static Mat22 operator +(Mat22 a, Mat22 b)
        {
            return new Mat22(a.ExX + b.ExX, a.ExY + b.ExY, a.EyX + b.EyX, a.EyY + b.EyY);
        }

        public static Mat22 operator -(Mat22 a, Mat22 b)
        {
            return new Mat22(a.ExX - b.ExX, a.ExY - b.ExY, a.EyX - b.EyX, a.EyY - b.EyY);
        }

        public static Mat22 operator *(float s, Mat22 m)
        {
            return new Mat22(s * m.ExX, s * m.ExY, s * m.EyX, s * m.EyY);
        }

        public static Vec2 operator *(Mat22 m, Vec2 v)
        {
            return new Vec2(m.ExX * v.X + m.EyX * v.Y, m.ExY * v.X + m.EyY * v.Y);
        }

        public static Vec2 MulT(Mat22 m, Vec2 v)
        {
            return new Vec2(m.ExX * v.X + m.ExY * v.Y, m.EyX * v.X + m.EyY * v.Y);
        }
    }

    public class JointSolver
    {
        private readonly List<IJoint> _joints;
        private readonly List<SolverData> _solverData;
        private int _velocityIterations;
        private int _positionIterations;
        private float _tolerance;
        private float _maxLinearCorrection;
        private float _maxAngularCorrection;
        private float _baumgarte;
        private float _slop;

        public int VelocityIterations
        {
            get => _velocityIterations;
            set => _velocityIterations = Math.Max(1, value);
        }

        public int PositionIterations
        {
            get => _positionIterations;
            set => _positionIterations = Math.Max(1, value);
        }

        public float Tolerance
        {
            get => _tolerance;
            set => _tolerance = Math.Max(0f, value);
        }

        public float MaxLinearCorrection
        {
            get => _maxLinearCorrection;
            set => _maxLinearCorrection = Math.Max(0f, value);
        }

        public float MaxAngularCorrection
        {
            get => _maxAngularCorrection;
            set => _maxAngularCorrection = Math.Max(0f, value);
        }

        public float Baumgarte
        {
            get => _baumgarte;
            set => _baumgarte = Math.Clamp(value, 0f, 1f);
        }

        public float Slop
        {
            get => _slop;
            set => _slop = Math.Max(0f, value);
        }

        public JointSolver()
        {
            _joints = new List<IJoint>();
            _solverData = new List<SolverData>();
            _velocityIterations = 8;
            _positionIterations = 3;
            _tolerance = 0.0001f;
            _maxLinearCorrection = 0.2f;
            _maxAngularCorrection = 0.005f;
            _baumgarte = 0.2f;
            _slop = 0.005f;
        }

        public void AddJoint(IJoint joint)
        {
            _joints.Add(joint);
        }

        public void RemoveJoint(IJoint joint)
        {
            _joints.Remove(joint);
        }

        public void Clear()
        {
            _joints.Clear();
        }

        public void Solve(List<RigidBody> bodies, float deltaTime)
        {
            if (_joints.Count == 0)
                return;

            float invDt = deltaTime > 0f ? 1f / deltaTime : 0f;

            InitializeVelocityConstraints(bodies, deltaTime, invDt);

            for (int i = 0; i < _velocityIterations; i++)
            {
                SolveVelocities();
            }

            for (int i = 0; i < _positionIterations; i++)
            {
                bool positionsSolved = SolvePositions();
                if (positionsSolved)
                    break;
            }
        }

        private void InitializeVelocityConstraints(List<RigidBody> bodies, float deltaTime, float invDt)
        {
            _solverData.Clear();

            foreach (var joint in _joints)
            {
                if (!joint.IsEnabled || joint.IsBroken)
                    continue;

                RigidBody? bodyA = joint.BodyA;
                RigidBody? bodyB = joint.BodyB;

                if (bodyA == null || bodyB == null)
                    continue;

                var data = new SolverData
                {
                    DeltaTime = deltaTime,
                    InvDeltaTime = invDt,
                    DeltaTimeRatio = deltaTime * 60f,
                    IndexA = bodies.IndexOf(bodyA),
                    IndexB = bodies.IndexOf(bodyB),
                    InvMassA = bodyA.InverseMass,
                    InvMassB = bodyB.InverseMass,
                    InvIA = bodyA.InverseInertia,
                    InvIB = bodyB.InverseInertia,
                    PositionA = bodyA.Position,
                    PositionB = bodyB.Position,
                    AngleA = bodyA.Rotation,
                    AngleB = bodyB.Rotation,
                    VelocityA = bodyA.Velocity,
                    VelocityB = bodyB.Velocity,
                    AngularVelocityA = bodyA.AngularVelocity,
                    AngularVelocityB = bodyB.AngularVelocity
                };

                joint.InitializeVelocityConstraints(data);
                _solverData.Add(data);
            }
        }

        private void SolveVelocities()
        {
            for (int i = 0; i < _solverData.Count; i++)
            {
                if (i < _joints.Count)
                {
                    var joint = _joints[i];
                    if (!joint.IsEnabled || joint.IsBroken)
                        continue;

                    var data = _solverData[i];
                    joint.SolveVelocityConstraints(data);
                    _solverData[i] = data;
                }
            }
        }

        private bool SolvePositions()
        {
            bool allSolved = true;

            for (int i = 0; i < _solverData.Count; i++)
            {
                if (i < _joints.Count)
                {
                    var joint = _joints[i];
                    if (!joint.IsEnabled || joint.IsBroken)
                        continue;

                    var data = _solverData[i];
                    bool solved = joint.SolvePositionConstraints(data);
                    _solverData[i] = data;

                    if (!solved)
                        allSolved = false;
                }
            }

            return allSolved;
        }

        public void ApplyImpulses()
        {
            for (int i = 0; i < _solverData.Count; i++)
            {
                if (i < _joints.Count)
                {
                    var joint = _joints[i];
                    if (!joint.IsEnabled || joint.IsBroken)
                        continue;

                    var data = _solverData[i];

                    RigidBody? bodyA = joint.BodyA;
                    RigidBody? bodyB = joint.BodyB;

                    if (bodyA != null)
                    {
                        bodyA.Velocity = data.VelocityA;
                        bodyA.AngularVelocity = data.AngularVelocityA;
                    }

                    if (bodyB != null)
                    {
                        bodyB.Velocity = data.VelocityB;
                        bodyB.AngularVelocity = data.AngularVelocityB;
                    }
                }
            }
        }

        public List<Vector2> GetReactionForces(float invDt)
        {
            var forces = new List<Vector2>();
            foreach (var joint in _joints)
            {
                if (!joint.IsEnabled || joint.IsBroken)
                {
                    forces.Add(Vector2.Zero);
                    continue;
                }
                forces.Add(joint.GetReactionForce(invDt));
            }
            return forces;
        }

        public List<float> GetReactionTorques(float invDt)
        {
            var torques = new List<float>();
            foreach (var joint in _joints)
            {
                if (!joint.IsEnabled || joint.IsBroken)
                {
                    torques.Add(0f);
                    continue;
                }
                torques.Add(joint.GetReactionTorque(invDt));
            }
            return torques;
        }
    }

    public static class JointFactory
    {
        public static HingeJoint CreateHingeJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 worldAnchor, Vector2 axis)
        {
            return new HingeJoint(bodyA, bodyB, worldAnchor, axis);
        }

        public static SliderJoint CreateSliderJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 worldAnchor, float angle)
        {
            return new SliderJoint(bodyA, bodyB, worldAnchor, angle);
        }

        public static DistanceJoint CreateDistanceJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchorA, Vector2 anchorB)
        {
            return new DistanceJoint(bodyA, bodyB, anchorA, anchorB);
        }

        public static SpringJoint CreateSpringJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchorA, Vector2 anchorB)
        {
            return new SpringJoint(bodyA, bodyB, anchorA, anchorB);
        }

        public static WeldJoint CreateWeldJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 worldAnchor)
        {
            return new WeldJoint(bodyA, bodyB, worldAnchor);
        }

        public static RopeJoint CreateRopeJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchorA, Vector2 anchorB)
        {
            return new RopeJoint(bodyA, bodyB, anchorA, anchorB);
        }

        public static PulleyJoint CreatePulleyJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 groundAnchorA, Vector2 groundAnchorB, Vector2 anchorA, Vector2 anchorB, float ratio)
        {
            return new PulleyJoint(bodyA, bodyB, groundAnchorA, groundAnchorB, anchorA, anchorB, ratio);
        }

        public static WheelJoint CreateWheelJoint(RigidBody? bodyA, RigidBody? bodyB, Vector2 worldAnchor, Vec2 axis)
        {
            return new WheelJoint(bodyA, bodyB, worldAnchor, axis);
        }

        public static GearJoint CreateGearJoint(RigidBody? bodyA, RigidBody? bodyB, Joint jointA, Joint jointB, float ratio)
        {
            return new GearJoint(bodyA, bodyB, jointA, jointB, ratio);
        }

        public static T CreateJoint<T>(JointType type, RigidBody? bodyA, RigidBody? bodyB, Vector2 anchor) where T : Joint
        {
            switch (type)
            {
                case JointType.Hinge:
                    return (T)(Joint)new HingeJoint(bodyA, bodyB, anchor, Vector2.Right);
                case JointType.Slider:
                    return (T)(Joint)new SliderJoint(bodyA, bodyB, anchor, 0f);
                case JointType.Distance:
                    return (T)(Joint)new DistanceJoint(bodyA, bodyB, anchor, anchor);
                case JointType.Spring:
                    return (T)(Joint)new SpringJoint(bodyA, bodyB, anchor, anchor);
                case JointType.Weld:
                    return (T)(Joint)new WeldJoint(bodyA, bodyB, anchor);
                case JointType.Rope:
                    return (T)(Joint)new RopeJoint(bodyA, bodyB, anchor, anchor);
                case JointType.Wheel:
                    return (T)(Joint)new WheelJoint(bodyA, bodyB, anchor, Vector2.Right);
                default:
                    throw new ArgumentException($"Unsupported joint type: {type}");
            }
        }
    }

    public class JointDebugDraw
    {
        private readonly List<JointDebugInfo> _debugInfo;

        public JointDebugDraw()
        {
            _debugInfo = new List<JointDebugInfo>();
        }

        public void Update(IEnumerable<IJoint> joints)
        {
            _debugInfo.Clear();

            foreach (var joint in joints)
            {
                if (!joint.IsEnabled)
                    continue;

                var info = new JointDebugInfo
                {
                    JointType = joint.Type,
                    AnchorA = joint.BodyA != null ? GetWorldAnchor(joint.BodyA, joint.LocalAnchorA) : Vector2.Zero,
                    AnchorB = joint.BodyB != null ? GetWorldAnchor(joint.BodyB, joint.LocalAnchorB) : Vector2.Zero,
                    IsBroken = joint.IsBroken,
                    Color = GetJointColor(joint.Type)
                };

                if (joint is DistanceJoint distance)
                {
                    info.Length = distance.CurrentLength;
                    info.NaturalLength = distance.NaturalLength;
                    info.Stiffness = distance.Stiffness;
                    info.Damping = distance.Damping;
                }
                else if (joint is HingeJoint hinge)
                {
                    info.Angle = hinge.CurrentAngle;
                    info.AngularVelocity = hinge.CurrentSpeed;
                }
                else if (joint is SliderJoint slider)
                {
                    info.Translation = slider.CurrentTranslation;
                    info.Speed = slider.CurrentSpeed;
                }

                _debugInfo.Add(info);
            }
        }

        private Vector2 GetWorldAnchor(RigidBody body, Vector2 localAnchor)
        {
            float cos = MathHelper.Cos(body.Rotation);
            float sin = MathHelper.Sin(body.Rotation);
            return new Vector2(
                body.Position.X + cos * localAnchor.X - sin * localAnchor.Y,
                body.Position.Y + sin * localAnchor.X + cos * localAnchor.Y
            );
        }

        private Color GetJointColor(JointType type)
        {
            return type switch
            {
                JointType.Hinge => new Color(1f, 0f, 0f),
                JointType.Slider => new Color(0f, 1f, 0f),
                JointType.Distance => new Color(0f, 0f, 1f),
                JointType.Spring => new Color(1f, 1f, 0f),
                JointType.Weld => new Color(1f, 0f, 1f),
                JointType.Rope => new Color(0f, 1f, 1f),
                JointType.Pulley => new Color(0.5f, 0.5f, 0f),
                JointType.Wheel => new Color(0.5f, 0f, 0.5f),
                JointType.Gear => new Color(0f, 0.5f, 0.5f),
                _ => new Color(1f, 1f, 1f)
            };
        }

        public List<JointDebugInfo> GetDebugInfo() => new List<JointDebugInfo>(_debugInfo);

        public void DrawJoint(IJoint joint)
        {
            if (joint.BodyA == null || joint.BodyB == null)
                return;

            Vector2 anchorA = GetWorldAnchor(joint.BodyA, joint.LocalAnchorA);
            Vector2 anchorB = GetWorldAnchor(joint.BodyB, joint.LocalAnchorB);

            DrawLine(anchorA, anchorB, GetJointColor(joint.Type));
            DrawCircle(anchorA, 0.1f, GetJointColor(joint.Type));
            DrawCircle(anchorB, 0.1f, GetJointColor(joint.Type));
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color)
        {
        }

        public void DrawCircle(Vector2 center, float radius, Color color)
        {
        }

        public string GetJointInfo(IJoint joint)
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"Joint Type: {joint.Type}");
            info.AppendLine($"Body A: {(joint.BodyA != null ? joint.BodyA.ToString() : "None")}");
            info.AppendLine($"Body B: {(joint.BodyB != null ? joint.BodyB.ToString() : "None")}");
            info.AppendLine($"Local Anchor A: {joint.LocalAnchorA}");
            info.AppendLine($"Local Anchor B: {joint.LocalAnchorB}");
            info.AppendLine($"Enabled: {joint.IsEnabled}");
            info.AppendLine($"Broken: {joint.IsBroken}");

            if (joint is HingeJoint hinge)
            {
                info.AppendLine($"Current Angle: {hinge.CurrentAngle:F2}°");
                info.AppendLine($"Motor Speed: {hinge.MotorSpeed:F2}");
                info.AppendLine($"Limits: {(hinge.EnableLimits ? $"{hinge.LowerLimit:F2}° to {hinge.UpperLimit:F2}°" : "Disabled")}");
            }
            else if (joint is SliderJoint slider)
            {
                info.AppendLine($"Current Translation: {slider.CurrentTranslation:F2}");
                info.AppendLine($"Speed: {slider.CurrentSpeed:F2}");
                info.AppendLine($"Limits: {(slider.LimitEnabled ? $"{slider.LowerLimit:F2} to {slider.UpperLimit:F2}" : "Disabled")}");
            }
            else if (joint is DistanceJoint distance)
            {
                info.AppendLine($"Current Length: {distance.CurrentLength:F2}");
                info.AppendLine($"Natural Length: {distance.NaturalLength:F2}");
                info.AppendLine($"Stiffness: {distance.Stiffness:F2}");
                info.AppendLine($"Damping: {distance.Damping:F2}");
            }
            else if (joint is SpringJoint spring)
            {
                info.AppendLine($"Current Length: {spring.CurrentLength:F2}");
                info.AppendLine($"Natural Length: {spring.NaturalLength:F2}");
                info.AppendLine($"Stiffness: {spring.Stiffness:F2}");
                info.AppendLine($"Damping: {spring.Damping:F2}");
                info.AppendLine($"Potential Energy: {spring.PotentialEnergy:F2}");
            }
            else if (joint is RopeJoint rope)
            {
                info.AppendLine($"Current Length: {rope.CurrentLength:F2}");
                info.AppendLine($"Max Length: {rope.MaxLength:F2}");
                info.AppendLine($"Limit State: {rope.LimitState}");
            }
            else if (joint is PulleyJoint pulley)
            {
                info.AppendLine($"Length A: {pulley.CurrentLengthA:F2}");
                info.AppendLine($"Length B: {pulley.CurrentLengthB:F2}");
                info.AppendLine($"Ratio: {pulley.Ratio:F2}");
            }
            else if (joint is WheelJoint wheel)
            {
                info.AppendLine($"Motor Speed: {wheel.MotorSpeed:F2}");
                info.AppendLine($"Limit State: {wheel.LimitState}");
            }

            Vector2 reactionForce = joint.GetReactionForce(1f / 60f);
            float reactionTorque = joint.GetReactionTorque(1f / 60f);
            info.AppendLine($"Reaction Force: {reactionForce}");
            info.AppendLine($"Reaction Torque: {reactionTorque:F2}");

            return info.ToString();
        }
    }

    public class JointDebugInfo
    {
        public JointType JointType;
        public Vector2 AnchorA;
        public Vector2 AnchorB;
        public bool IsBroken;
        public Color Color;
        public float Length;
        public float NaturalLength;
        public float Stiffness;
        public float Damping;
        public float Angle;
        public float AngularVelocity;
        public float Translation;
        public float Speed;
    }

    public struct Color
    {
        public float R, G, B, A;

        public Color(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color White => new Color(1f, 1f, 1f);
        public static Color Black => new Color(0f, 0f, 0f);
        public static Color Red => new Color(1f, 0f, 0f);
        public static Color Green => new Color(0f, 1f, 0f);
        public static Color Blue => new Color(0f, 0f, 1f);
        public static Color Yellow => new Color(1f, 1f, 0f);
        public static Color Cyan => new Color(0f, 1f, 1f);
        public static Color Magenta => new Color(1f, 0f, 1f);
        public static Color Gray => new Color(0.5f, 0.5f, 0.5f);
        public static Color Orange => new Color(1f, 0.5f, 0f);
        public static Color Purple => new Color(0.5f, 0f, 0.5f);
        public static Color Teal => new Color(0f, 0.5f, 0.5f);
    }

    public static class JointToString
    {
        public static string ToDebugString(IJoint joint)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== {joint.Type} Joint Debug Info ===");
            sb.AppendLine($"Type: {joint.Type}");
            sb.AppendLine($"Body A: {(joint.BodyA != null ? $"ID={joint.BodyA.GetHashCode()}" : "null")}");
            sb.AppendLine($"Body B: {(joint.BodyB != null ? $"ID={joint.BodyB.GetHashCode()}" : "null")}");
            sb.AppendLine($"Local Anchor A: {joint.LocalAnchorA}");
            sb.AppendLine($"Local Anchor B: {joint.LocalAnchorB}");
            sb.AppendLine($"World Anchor A: {GetWorldAnchor(joint.BodyA, joint.LocalAnchorA)}");
            sb.AppendLine($"World Anchor B: {GetWorldAnchor(joint.BodyB, joint.LocalAnchorB)}");
            sb.AppendLine($"Enabled: {joint.IsEnabled}");
            sb.AppendLine($"Collide Connected: {joint.CollideConnected}");
            sb.AppendLine($"Break Force: {joint.BreakForce:F2}");
            sb.AppendLine($"Break Torque: {joint.BreakTorque:F2}");
            sb.AppendLine($"Is Broken: {joint.IsBroken}");
            sb.AppendLine();
            sb.AppendLine("Type-specific properties:");

            if (joint is HingeJoint hinge)
            {
                sb.AppendLine($"  Reference Angle: {hinge.ReferenceAngle:F2}");
                sb.AppendLine($"  Current Angle: {hinge.CurrentAngle:F2}");
                sb.AppendLine($"  Motor Speed: {hinge.MotorSpeed:F2}");
                sb.AppendLine($"  Max Motor Torque: {hinge.MaxMotorTorque:F2}");
                sb.AppendLine($"  Motor Enabled: {hinge.MotorEnabled}");
                sb.AppendLine($"  Limits Enabled: {hinge.EnableLimits}");
                sb.AppendLine($"  Lower Limit: {hinge.LowerLimit:F2}");
                sb.AppendLine($"  Upper Limit: {hinge.UpperLimit:F2}");
            }
            else if (joint is SliderJoint slider)
            {
                sb.AppendLine($"  Axis Angle: {slider.AxisAngle:F2}");
                sb.AppendLine($"  Current Translation: {slider.CurrentTranslation:F2}");
                sb.AppendLine($"  Motor Speed: {slider.MotorSpeed:F2}");
                sb.AppendLine($"  Limits Enabled: {slider.LimitEnabled}");
            }
            else if (joint is DistanceJoint distance)
            {
                sb.AppendLine($"  Length: {distance.Length:F2}");
                sb.AppendLine($"  Min Length: {distance.MinLength:F2}");
                sb.AppendLine($"  Max Length: {distance.MaxLength:F2}");
                sb.AppendLine($"  Current Length: {distance.CurrentLength:F2}");
                sb.AppendLine($"  Stiffness: {distance.Stiffness:F2}");
                sb.AppendLine($"  Damping: {distance.Damping:F2}");
            }
            else if (joint is SpringJoint spring)
            {
                sb.AppendLine($"  Natural Length: {spring.NaturalLength:F2}");
                sb.AppendLine($"  Current Length: {spring.CurrentLength:F2}");
                sb.AppendLine($"  Stiffness: {spring.Stiffness:F2}");
                sb.AppendLine($"  Damping: {spring.Damping:F2}");
                sb.AppendLine($"  Potential Energy: {spring.PotentialEnergy:F2}");
            }
            else if (joint is WeldJoint weld)
            {
                sb.AppendLine($"  Reference Angle: {weld.ReferenceAngle:F2}");
                sb.AppendLine($"  Stiffness: {weld.Stiffness:F2}");
                sb.AppendLine($"  Damping: {weld.Damping:F2}");
            }
            else if (joint is RopeJoint rope)
            {
                sb.AppendLine($"  Max Length: {rope.MaxLength:F2}");
                sb.AppendLine($"  Current Length: {rope.CurrentLength:F2}");
                sb.AppendLine($"  Limit State: {rope.LimitState}");
            }
            else if (joint is PulleyJoint pulley)
            {
                sb.AppendLine($"  Ground Anchor A: {pulley.GroundAnchorA}");
                sb.AppendLine($"  Ground Anchor B: {pulley.GroundAnchorB}");
                sb.AppendLine($"  Length A: {pulley.LengthA:F2}");
                sb.AppendLine($"  Length B: {pulley.LengthB:F2}");
                sb.AppendLine($"  Ratio: {pulley.Ratio:F2}");
            }
            else if (joint is WheelJoint wheel)
            {
                sb.AppendLine($"  Axis: ({wheel.Axis.X:F2}, {wheel.Axis.Y:F2})");
                sb.AppendLine($"  Motor Speed: {wheel.MotorSpeed:F2}");
                sb.AppendLine($"  Stiffness: {wheel.Stiffness:F2}");
                sb.AppendLine($"  Damping: {wheel.Damping:F2}");
            }
            else if (joint is GearJoint gear)
            {
                sb.AppendLine($"  Joint A: {(gear.JointA != null ? gear.JointA.Type.ToString() : "null")}");
                sb.AppendLine($"  Joint B: {(gear.JointB != null ? gear.JointB.Type.ToString() : "null")}");
                sb.AppendLine($"  Ratio A: {gear.RatioA:F2}");
                sb.AppendLine($"  Ratio B: {gear.RatioB:F2}");
                sb.AppendLine($"  Gear Ratio: {gear.GearRatio:F2}");
            }

            sb.AppendLine();
            sb.AppendLine($"Reaction Force: {joint.GetReactionForce(1f / 60f)}");
            sb.AppendLine($"Reaction Torque: {joint.GetReactionTorque(1f / 60f):F2}");

            return sb.ToString();
        }

        private static Vector2 GetWorldAnchor(RigidBody? body, Vector2 localAnchor)
        {
            if (body == null)
                return Vector2.Zero;

            float cos = MathHelper.Cos(body.Rotation);
            float sin = MathHelper.Sin(body.Rotation);
            return new Vector2(
                body.Position.X + cos * localAnchor.X - sin * localAnchor.Y,
                body.Position.Y + sin * localAnchor.X + cos * localAnchor.Y
            );
        }

        public static string ToShortString(IJoint joint)
        {
            Vector2 worldA = GetWorldAnchor(joint.BodyA, joint.LocalAnchorA);
            Vector2 worldB = GetWorldAnchor(joint.BodyB, joint.LocalAnchorB);
            return $"{joint.Type}: A{worldA} <-> B{worldB} [{joint.IsEnabled}]";
        }

        public static string ToTableRow(IJoint joint)
        {
            return $"{joint.Type,-12} | {(joint.BodyA?.GetHashCode() ?? 0),8} | {(joint.BodyB?.GetHashCode() ?? 0),8} | {joint.IsEnabled,-8} | {joint.IsBroken,-8}";
        }

        public static string TableHeader()
        {
            return $"{new string('─', 12)}─┼──────────┼──────────┼────────┬────────";
        }
    }

    public static class JointExtensions
    {
        public static TJoint WithBreakForce<TJoint>(this TJoint joint, float force) where TJoint : IJoint
        {
            joint.BreakForce = force;
            return joint;
        }

        public static TJoint WithBreakTorque<TJoint>(this TJoint joint, float torque) where TJoint : IJoint
        {
            joint.BreakTorque = torque;
            return joint;
        }

        public static TJoint WithCollideConnected<TJoint>(this TJoint joint, bool collide) where TJoint : Joint
        {
            joint.CollideConnected = collide;
            return joint;
        }

        public static TJoint WithEnabled<TJoint>(this TJoint joint, bool enabled) where TJoint : IJoint
        {
            joint.IsEnabled = enabled;
            return joint;
        }
    }

    public class JointConstraintBuilder
    {
        private readonly List<IJoint> _joints;

        public JointConstraintBuilder()
        {
            _joints = new List<IJoint>();
        }

        public JointConstraintBuilder AddHinge(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchor)
        {
            var joint = JointFactory.CreateHingeJoint(bodyA, bodyB, anchor, Vector2.Right);
            _joints.Add(joint);
            return this;
        }

        public JointConstraintBuilder AddSlider(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchor, float angle)
        {
            var joint = JointFactory.CreateSliderJoint(bodyA, bodyB, anchor, angle);
            _joints.Add(joint);
            return this;
        }

        public JointConstraintBuilder AddDistance(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchorA, Vector2 anchorB)
        {
            var joint = JointFactory.CreateDistanceJoint(bodyA, bodyB, anchorA, anchorB);
            _joints.Add(joint);
            return this;
        }

        public JointConstraintBuilder AddSpring(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchorA, Vector2 anchorB, float stiffness, float damping)
        {
            var joint = JointFactory.CreateSpringJoint(bodyA, bodyB, anchorA, anchorB);
            joint.Stiffness = stiffness;
            joint.Damping = damping;
            _joints.Add(joint);
            return this;
        }

        public JointConstraintBuilder AddWeld(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchor)
        {
            var joint = JointFactory.CreateWeldJoint(bodyA, bodyB, anchor);
            _joints.Add(joint);
            return this;
        }

        public JointConstraintBuilder AddRope(RigidBody? bodyA, RigidBody? bodyB, Vector2 anchorA, Vector2 anchorB, float maxLength)
        {
            var joint = JointFactory.CreateRopeJoint(bodyA, bodyB, anchorA, anchorB);
            joint.MaxLength = maxLength;
            _joints.Add(joint);
            return this;
        }

        public List<IJoint> Build()
        {
            return new List<IJoint>(_joints);
        }

        public void Clear()
        {
            _joints.Clear();
        }
    }
}
