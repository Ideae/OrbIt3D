using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using UnityEngine;
namespace OrbItProcs
{
    public enum movemode
    {
        free,
        wallbounce,
        screenwrap,
        falloff,
        forceback,
    };
    /// <summary>
    /// Basic Movement Component
    /// </summary>
    [Info(UserLevel.User, "Basic Movement Component", CompType)]
    public class Movement : OComponent {

        public const mtypes CompType = mtypes.essential | mtypes.playercontrol;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        public bool pushable { get; set; }

        public bool lockZAxis { get; set; }

        private Toggle<float> _maxVelocity = new Toggle<float>(30f, true);
        private Toggle<float> _minVelocity = new Toggle<float>(0f, false);
        /// <summary>
        /// If enabled, this limits the node's speed to stay below the specified velocity.
        /// </summary>
        [Info(UserLevel.User, "If enabled, this limits the node's velocity to stay below the specified velocity.")]
        public Toggle<float> maxVelocity { get { return _maxVelocity; } set { _maxVelocity = value; if (maxVelocity < _minVelocity) _maxVelocity = _minVelocity; } }
        /// <summary>
        /// If enabled, this limits the node's velocity to stay above the specified velocity.
        /// </summary>
        [Info(UserLevel.User, "If enabled, this limits the node's velocity to stay above the specified velocity.")]
        public Toggle<float> minVelocity { get { return _minVelocity; } set { _minVelocity = value; if (_minVelocity > _maxVelocity) _minVelocity = _maxVelocity; } }
        /// <summary>
        /// Gives the node this velocity in a random direction when spawned.
        /// </summary>
        [Info(UserLevel.User, "If enabled, gives the node this velocity in a random direction when spawned.")]
        public Toggle<float> randInitialVel { get; set; }



        private movemode _mode;
        /// <summary>
        /// How the screen's Limits affect this wall:
        /// Free: the restraints won't affect the wall.
        /// Wallbounce: the node's velocity is inverted when they hit the wall,
        /// Screenwrap: the node appears at the opposite wall,
        /// Falloff: the node is deleted when it exits the screen,
        /// Halt the node stops upon exiting the screen,
        /// </summary>
        [Info(UserLevel.User, "How the screen's Limits affect this wall: \n Free: the restraints won't affect the wall. \n Wallbounce: the node's velocity is inverted when they hit the wall, \n Screenwrap: the node appears at the opposite wall, \n Falloff: the node is deleted when it exits the screen, \n Halt the node stops upon exiting the screen,")]
        public movemode mode { get { return _mode; } set { _mode = value; } }

        public bool effvelocityMode { get; set; }

        public Movement() : this(null) { }
        public Movement(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            randInitialVel = new Toggle<float>(8f);
            pushable = true;
            mode = movemode.wallbounce;
            effvelocityMode = false;
            lockZAxis = false;
        }
        public override void OnSpawn()
        {
            if (randInitialVel) RandomizeVelocity();
            moderateVelocity();
        }

        [Clickable]
        public void moderateVelocity()
        {
            double velSquared = parent.rigidbody.velocity.x * parent.rigidbody.velocity.x + parent.rigidbody.velocity.y * parent.rigidbody.velocity.y;

            if (minVelocity.enabled && velSquared < minVelocity * minVelocity)
            {
                //VMath.NormalizeSafe(ref parent.rigidbody.velocity);
                parent.rigidbody.velocity.Normalize();
                parent.rigidbody.velocity *= minVelocity;
            }
            if (maxVelocity.enabled && velSquared > maxVelocity * maxVelocity)
            {
                //VMath.NormalizeSafe(ref parent.rigidbody.velocity);
                parent.rigidbody.velocity.Normalize();
                parent.rigidbody.velocity *= maxVelocity;
            }
        }
        public void RandomizeVelocity()
        {
            float x = ((float)Utils.random.NextDouble() * 100) - 50;
            float y = ((float)Utils.random.NextDouble() * 100) - 50;
            float z = ((float)Utils.random.NextDouble() * 100) - 50;
            Vector3 vel = new Vector3(x, y, z);
            VMath.NormalizeSafe(ref vel);
            //vel.Normalize();
            vel = vel * randInitialVel;
            parent.rigidbody.velocity = vel;
        }
        [Clickable]
        public void scaleVelocity()
        {
            if (parent.rigidbody.velocity.x != 0 && parent.rigidbody.velocity.y != 0)
            {
                //VMath.NormalizeSafe(ref parent.rigidbody.velocity);
                parent.rigidbody.velocity.Normalize();
                parent.rigidbody.velocity *= randInitialVel;
            }
        }

        public override void AffectSelf()
        {
            //parent.body.position.x += parent.rigidbody.velocity.x * VelocityModifier;
            //parent.body.position.y += parent.rigidbody.velocity.y * VelocityModifier;
            //return;
            if (mode == movemode.wallbounce) wallBounce();
            else if (mode == movemode.forceback) forceBack();
            else if (mode == movemode.screenwrap) screenWrap();
            else if (mode == movemode.falloff) fallOff();

            //GraphData.AddFloat(parent.transform.position.x);
            if (lockZAxis)
            {
                parent.transform.SetPosZ(0);
            }
            //Trippy();
        }

        //public void Trippy()
        //{
        //    //test (holy SHIT that looks cool)
        //    PropertyInfo pi = parent.body.GetType().GetProperty("scale");
        //    pi.SetValue(parent.body, parent.transform.position.x % 4.0f, null);
        //}
        public float absaccel = 0.2f;
        public float friction = 0.01f;
        private float v = 0.0f;

        public override void PlayerControl(OInput input)
        {

            Vector2 stick = input.GetLeftStick();
            Vector2 stick2 = input.GetRightStick();
            
            
            if (stick2.sqrMagnitude > 0.6f * 0.6f)
            {
                v = VMath.VectorToAngle(stick2).between0and2pi();
                if (v == 0f) v = 0.00001f;
            }
            else if (stick.sqrMagnitude > 0.6f * 0.6f)
            {
                v = VMath.VectorToAngle(stick).between0and2pi();
                if (v == 0f) v = 0.00001f;
            }
            float result = GMath.AngleLerp(parent.transform.eulerAngles.x, v, 0.1f);

            parent.transform.eulerAngles.SetX(result);

            stick *= 0.4f;
            stick *= absaccel;
            if ((parent.rigidbody.velocity.x != 0 || parent.rigidbody.velocity.y != 0))
            {
                stick += (Vector2)parent.rigidbody.velocity * -friction;
            }
            stick *= parent.rigidbody.mass;
            //todo: update maxvel?
            parent.rigidbody.AddForce(stick);


        }
        public void forceBack()
        {
            Vector3 pos = parent.transform.position;
            float x = Mathf.Max(0, Mathf.Abs(pos.x - OrbIt.origin.x) + parent.radius - room.worldSize.x);
            float y = Mathf.Max(0, Mathf.Abs(pos.y - OrbIt.origin.y) + parent.radius - room.worldSize.y);
            float z = Mathf.Max(0, Mathf.Abs(pos.z - OrbIt.origin.z) + parent.radius - room.worldSize.z);
            float force = (new Vector3(x, y, z)).magnitude;
            Vector3 v = (OrbIt.origin - pos).normalized * force;
            parent.rigidbody.AddForce(v);
            //OVRManager.display.isDirectMode
        }


        //reminder: make a vocal recognition extension for visual studio to take you where you want ("Class: Movement. Method: fallOff.")
        public void fallOff()
        {
            float levelwidth = room.worldSize.x;
            float levelheight = room.worldSize.y;

            Vector2 pos = parent.transform.position;

            //if (parent.HasComp<Queuer>() && (parent.Comp<Queuer>().qs & queues.position) == queues.position)
            //{
            //    Queuer q = parent.Comp<Queuer>();
            //    Queue<Vector2> positions = ((Queue<Vector2>)(parent.Comp<Queuer>().positions));
            //    pos = positions.ElementAt(0);
            //}

            if (pos.x >= (levelwidth + parent.radius))
            {
                parent.IsDeleted = true;
            }
            else if (pos.x < parent.radius * -1)
            {
                parent.IsDeleted = true;
            }

            if (pos.y >= (levelheight + parent.radius))
            {
                parent.IsDeleted = true;
            }
            else if (pos.y < parent.radius * -1)
            {
                parent.IsDeleted = true;
            }
        }

        public void wallBounce()
        {
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game...;
            float levelwidth = room.worldWidth;
            float levelheight = room.worldHeight;
            float leveldepth = room.worldDepth;
            float halfwidth = levelwidth / 2f;
            float halfheight = levelheight / 2f;
            float halfdepth = leveldepth / 2f;

            float maxX = (OrbIt.origin.x + halfwidth);
            float minX = (OrbIt.origin.x - halfwidth);
            float maxY = (OrbIt.origin.y + halfheight);
            float minY = (OrbIt.origin.y - halfheight);
            float maxZ = (OrbIt.origin.z + halfdepth);
            float minZ = (OrbIt.origin.z - halfdepth);

            if (parent.transform.position.x >= (maxX - parent.radius))
            {
                //parent.transform.position.SetX(GMath.Triangle(parent.transform.position.x, maxX - (int)parent.radius));
                parent.transform.SetPosX(maxX - parent.radius);
                if (parent.rigidbody.velocity.x > 0)
                    parent.rigidbody.SetVelX(parent.rigidbody.velocity.x * -1);
                //parent.body.InvokeOnCollisionStay(null); //todo: find out why we needed null, fix this

            }
            if (parent.transform.position.x < minX + parent.radius)
            {
                //parent.transform.position.SetX(GMath.Triangle(parent.transform.position.x - parent.radius + halfwidth, room.worldSize.x) + parent.radius - halfwidth);

                parent.transform.SetPosX(minX + parent.radius);
                if (parent.rigidbody.velocity.x < 0)
                    parent.rigidbody.SetVelX(parent.rigidbody.velocity.x * -1);
                //parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.transform.position.y >= (maxY - parent.radius))
            {
                //parent.transform.position.SetY(GMath.Triangle(parent.transform.position.y, maxY - (int)parent.radius));
                parent.transform.SetPosY(maxY - parent.radius);
                if (parent.rigidbody.velocity.y > 0)
                    parent.rigidbody.SetVelY(parent.rigidbody.velocity.y * -1);
                //parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.transform.position.y < minY + parent.radius)
            {
                //parent.transform.position.SetY(GMath.Triangle(parent.transform.position.y - parent.radius + halfheight, room.worldSize.y) + parent.radius - halfheight);
                parent.transform.SetPosY(minY + parent.radius);
                if (parent.rigidbody.velocity.y < 0)
                    parent.rigidbody.SetVelY(parent.rigidbody.velocity.y * -1);
                //parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.transform.position.z >= (maxZ - parent.radius))
            {
                //parent.transform.position.SetZ(GMath.Triangle(parent.transform.position.z, maxZ - (int)parent.radius));
                parent.transform.SetPosZ(maxZ - parent.radius);
                if (parent.rigidbody.velocity.z > 0)
                    parent.rigidbody.SetVelZ(parent.rigidbody.velocity.z * -1);
            }
            if (parent.transform.position.z < minZ + parent.radius)
            {
                //parent.transform.position.SetZ(GMath.Triangle(parent.transform.position.z - parent.radius + halfdepth, room.worldSize.z) + parent.radius - halfdepth);
                parent.transform.SetPosZ(minZ + parent.radius);
                if (parent.rigidbody.velocity.z < 0)
                    parent.rigidbody.SetVelZ(parent.rigidbody.velocity.z * -1);
                
            }
        }

        public void screenWrap()
        {
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game...;
            float levelwidth = room.worldSize.x;
            float levelheight = room.worldSize.y;

            //todo: modulus screen width
            //hitting top/bottom of screen
            //teleport node
            if (parent.transform.position.x >= levelwidth)
            {
                parent.transform.SetPosX(parent.transform.position.x - levelwidth);//1;
            }
            else if (parent.transform.position.x < 0)
            {
                parent.transform.SetPosX(levelwidth - parent.transform.position.x);//1;
            }
            //show half texture on other side
            if (parent.transform.position.x >= (levelwidth - parent.radius))
            {
                //
            }
            else if (parent.transform.position.x < parent.radius)
            {
                //
            }

            //hitting sides
            //teleport node
            if (parent.transform.position.y >= levelheight)
            {
                parent.transform.SetPosY(parent.transform.position.y - levelheight);//1;
            }
            else if (parent.transform.position.y < 0)
            {
                parent.transform.SetPosY(levelheight - parent.transform.position.y);//1;
            }
            //show half texture on other side
            if (parent.transform.position.y >= (levelheight - parent.radius))
            {
                //
            }
            else if (parent.transform.position.y < parent.radius)
            {
                //
            }



        }

    }
}
