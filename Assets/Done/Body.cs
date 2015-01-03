using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Collider = OrbItProcs.Collider;
namespace OrbItProcs
{
    /// <summary>
    /// The Body holds the node's size, position and other properties used in physics and gravity calculations.
    /// </summary>
    [Info(UserLevel.User, "The Body holds the node's size, position and other properties used in physics and gravity calculations.")]
    public class Body : Collider
    {
        public Vector2 velocity = new Vector2(0, 0);
        [Info(UserLevel.Developer)]
        public override bool HandlersEnabled
        {
            get { return _HandlersEnabled; }
            set
            {
                _HandlersEnabled = value;
                if (parent != null && parent.collision.active)
                {
                    parent.collision.UpdateCollisionSet();
                }
            }
        }
        private bool _ResolveCollision = true;
        /// <summary>
        /// Controls whether the node will collide and bounce off other nodes.
        /// </summary>
        [Info(UserLevel.User, "Controls whether the node will collide and bounce off other nodes.")]
        public bool isSolid
        {
            get { return _ResolveCollision; }
            set 
            {
                _ResolveCollision = value;
                if (parent != null && parent.collision.active)
                {
                    parent.collision.UpdateCollisionSet();
                }
            }
        }
        /// <summary>
        /// Sets the radius of the node's circle, to change it's size.
        /// </summary>
        [Info(UserLevel.User, "Sets the radius of the node's circle, to change it's size.")]
        public override float radius
        {
            get { return shape.radius; }
            set
            {
                shape.radius = value;
                EvaluateScale();
            }
        }
        /// <summary>
        /// Also sets the nodes radius, using a scale factor.
        /// </summary>
        [Info(UserLevel.Advanced, "Also sets the nodes radius, using a scale factor.")]
        public float scale
        {
            get { return _scale; }
            //set 
            //{
            //    float halfwidth = getTexture() != null ? (parent.getTexture().Width / 2f) : 128f;
            //    shape.radius = value * halfwidth;
            //    _scale = value;
            //}
        }
        /// <summary>
        /// Controls how heavy the node is. Mass is used in collision, gravity, and other velocity based calculations.
        /// </summary>
        [Info(UserLevel.User, "Controls how heavy the node is. Mass is used in collision, gravity, and other velocity based calculations.")]
        public float mass
        {
            get { return _mass; }
            set { _mass = value; if (value == 0) invmass = 0; else invmass = 1.0f / value; } //infinite mass is represented by 0
        }
        /// <summary>
        /// Controls how easy it is for the node to spin (change it's angular velocity.)
        /// </summary>
        [Info(UserLevel.Advanced, "Controls how easy it is for the node to spin (change it's angular velocity.)")]
        public float inertia
        {
            get { return _inertia; }
            set { _inertia = value; if (value == 0) invinertia = 0; else invinertia = 1.0f / value; } //infinite mass is represented by 0
        }
        [Info(UserLevel.Developer)]
        public float angularVelocity { get { return _angularVelocity; } set { _angularVelocity = value; } }
        [Info(UserLevel.Developer)]
        public float torque { get { return _torque; } set { _torque = value; } }
        /// <summary>
        /// Controls static friction for collisions.
        /// </summary>
        [Info(UserLevel.Advanced, "Controls static friction for collisions.")]
        public float staticFriction { get { return _staticFriction; } set { _staticFriction = value; } }
        /// <summary>
        /// Controls dynamic friction for collisions.
        /// </summary>
        [Info(UserLevel.Advanced, "Controls dynamic friction for collisions.")]
        public float dynamicFriction { get { return _dynamicFriction; } set { _dynamicFriction = value; } }
        /// <summary>
        /// Controls how bouncy the node is during collisions. The higher the value, the more bounce.
        /// </summary>
        [Info(UserLevel.User, "Controls how bouncy the node is during collisions. The higher the value, the more bounce.")]
        public float restitution { get { return _restitution; } set { _restitution = value; } }

        public bool DrawPolygonCenter { get; set; }
        /// <summary>
        /// Sets the X position of the node.
        /// </summary>
        [Info(UserLevel.Advanced, "Sets the X position of the node.")]
        public float X
        {
            get { return pos.x; }
            set { pos.x = value; }
        }
        /// <summary>
        /// Sets the Y position of the node.
        /// </summary>
        [Info(UserLevel.Advanced, "Sets the Y position of the node.")]
        public float Y
        {
            get { return pos.y; }
            set { pos.y = value; }
        }

        public Color color = new Color(255, 255, 255);
        public Color permaColor = new Color(255, 255, 255);
        private textures _texture;

        public Vector2 effvelocity = new Vector2(0, 0);
        public Vector2 force = new Vector2(0, 0);

        private float _angularVelocity = 0;
        private float _torque = 0;
        private float _orient = 0;
        private float _inertia; // moment of inertia
        private float _mass = 10f;

        private float _staticFriction = 0.5f;
        private float _dynamicFriction = 0.3f;
        private float _restitution = 0.2f;

        //private float _radius = 25f;
        private float _scale = 1f;


        /// <summary>
        /// Sets the angle of orientation of the node. (Radians)
        /// </summary>
        [Info(UserLevel.Advanced, "Sets the angle of orientation of the node. (Radians)")]
        public float orient
        {
            get { return _orient; }
            set
            {
                value = value.between0and2pi();
                _orient = value;
                if (shape != null) shape.SetOrient(value);
            }
        }
        public float invmass
        {
            get;
            protected set;
        }
        public float invinertia
        {
            get;
            protected set;
        }
        /// <summary>
        /// The texture that the node will use to render. (This is the picture of the node.)
        /// </summary>
        [Info(UserLevel.User, "The texture that the node will use to render. (This is the picture of the node.)")]
        public textures texture
        {
            get { return _texture; }
            set { _texture = value; EvaluateScale(); }
        }

        public Body() : this(shape: null) { }
        public Body(Shape shape, Node parent = null)
        {
            if (parent != null) this.parent = parent;
            this.shape = shape ?? new Circle(Node.defaultNodeSize);
            this.shape.body = this;
            this.shape.Initialize();
            DrawPolygonCenter = true;
            AfterCloning();
        }
        public void EvaluateScale()
        {
            float halfwidth = getTexture() != null ? (parent.getSprite().texture.width / 2f) : 128f;
            if (_scale != (float)radius / halfwidth) _scale = radius / halfwidth;
        }
        public override void CheckCollisionBody(Body other)
        {
            //if (!active || !other.active) { return; }
            //if (exclusions.Contains(other)) return;
            if (invmass == 0 && other.invmass == 0)
                return;

            Manifold m = new Manifold(this, other);
            m.Solve();

            if (m.contact_count > 0)
            {
                if (DoExclusionCheck(other)) return;
                if (HandlersEnabled)
                {
                    //todo:add to handler list
                    if (OnCollisionStay != null)
                    {
                        OnCollisionStay(parent, other.parent);
                    }
                    bool parentEnter = OnCollisionEnter != null;
                    if (parentEnter || OnCollisionExit != null || OnCollisionFirstEnter != null || OnCollisionAllExit != null)
                    {
                        HashSet<Collider> lastframe = previousCollision;
                        HashSet<Collider> thisframe = currentCollision;
                        thisframe.Add(other);
                        if (!lastframe.Contains(other) && parentEnter)
                        {
                            OnCollisionEnter(parent, other.parent);
                        }
                    }
                }
                if (other.HandlersEnabled)
                {
                    if (other.OnCollisionStay != null)
                    {
                        other.OnCollisionStay(other.parent, parent);
                    }
                    bool otherEnter = other.OnCollisionEnter != null;
                    if (otherEnter || other.OnCollisionExit != null || other.OnCollisionFirstEnter != null || other.OnCollisionAllExit != null)
                    {
                        //HashSet<Node> lastframe = other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
                        //HashSet<Node> thisframe = !other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
                        HashSet<Collider> lastframe = other.previousCollision;
                        HashSet<Collider> thisframe = other.currentCollision;
                        thisframe.Add(this);
                        if (!lastframe.Contains(this) && otherEnter)
                        {
                            other.OnCollisionEnter(other.parent, parent);
                        }
                    }
                }
                if (DoExclusionCheckResolution(other)) return;
                if (isSolid && other.isSolid)
                    room.collisionManager.AddManifold(m);
            }
        }

        public override void CheckCollisionCollider(Collider other)
        {
            //if (!active || !other.active) { return; }
            //if (exclusions.Contains(other)) return;

            //Manifold m = new Manifold(this, other);
            //m.Solve();
            bool iscolliding = Collision.CheckCollision(this, other);

            if (iscolliding)
            {
                if (DoExclusionCheck(other)) return;
                if (other.HandlersEnabled)
                {
                    //todo:add to handler list
                    //if (OnCollisionStay != null)
                    //{
                    //    OnCollisionStay(parent, other.parent);
                    //}
                    //bool parentEnter = OnCollisionEnter != null;
                    //if (parentEnter || OnCollisionExit != null || OnCollisionFirstEnter != null || OnCollisionAllExit != null)
                    //{
                    //    HashSet<Collider> lastframe = previousCollision;
                    //    HashSet<Collider> thisframe = currentCollision;
                    //    thisframe.Add(other);
                    //    if (!lastframe.Contains(other) && parentEnter)
                    //    {
                    //        OnCollisionEnter(parent, other.parent);
                    //    }
                    //}

                    if (other.OnCollisionStay != null)
                    {
                        other.OnCollisionStay(other.parent, parent);
                    }
                    bool otherEnter = other.OnCollisionEnter != null;
                    if (otherEnter || other.OnCollisionExit != null || other.OnCollisionFirstEnter != null || other.OnCollisionAllExit != null)
                    {
                        //HashSet<Node> lastframe = other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
                        //HashSet<Node> thisframe = !other.collision.currentIsCol1 ? other.collision.collisions1 : other.collision.collisions2;
                        HashSet<Collider> lastframe = other.previousCollision;
                        HashSet<Collider> thisframe = other.currentCollision;
                        thisframe.Add(this);
                        if (!lastframe.Contains(this) && otherEnter)
                        {
                            other.OnCollisionEnter(other.parent, parent);
                        }
                    }
                }
            }
        }

        public void AfterCloning()
        {
            this.mass = this.mass;
            this.inertia = this.inertia;
        }

        public void SetShape(Shape shape)
        {
            shape.body = this;
            this.shape = shape;
            
        }

        public void ApplyForce(Vector2 f)
        {
            force += f;
        }
        public void ApplyImpulse(Vector2 impulse, Vector2 contactVector)
        {
            //
            velocity += invmass * impulse;
            //if (float.IsNaN(velocity.x)) System.Diagnostics.Debugger.Break();
            angularVelocity += invinertia * (float)VMath.Cross(contactVector, impulse);
            //if (float.IsInfinity(angularVelocity)) System.Diagnostics.Debugger.Break();
        }

        public void SetStatic(float newInerta = 0, float newMass = 0)
        {
            inertia = newInerta;
            mass = newMass;
        }

        public void SetOrientV2(Vector2 direction)
        {
            float radians = VMath.VectorToAngle(direction);
            orient = radians;
            shape.SetOrient(radians);
        }
        public Vector2 getOrientV2()
        {
            return VMath.AngleToVector(orient);
        }

        public virtual Sprite getTexture()
        {
            if (parent != null)
            {
                return parent.getSprite();
            }
            return null;
        }

    }
}
