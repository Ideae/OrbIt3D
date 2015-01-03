using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;
namespace OrbItProcs
{
    public enum movemode
    {
        free,
        wallbounce,
        screenwrap,
        falloff,
        halt,
    };
    /// <summary>
    /// Basic Movement Component
    /// </summary>
    [Info(UserLevel.User, "Basic Movement Component", CompType)]
    public class Movement : Component {

        public const mtypes CompType = mtypes.essential | mtypes.playercontrol;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        public bool pushable { get; set; }

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
                VMath.NormalizeSafe(ref parent.rigidbody.velocity);
                parent.rigidbody.velocity *= minVelocity;
            }
            if (maxVelocity.enabled && velSquared > maxVelocity * maxVelocity)
            {
                VMath.NormalizeSafe(ref parent.rigidbody.velocity);
                parent.rigidbody.velocity *= maxVelocity;
            }
        }
        public void RandomizeVelocity()
        {
            float x = ((float)Utils.random.NextDouble() * 100) - 50;
            float y = ((float)Utils.random.NextDouble() * 100) - 50;
            Vector2 vel = new Vector2(x, y);
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
                VMath.NormalizeSafe(ref parent.rigidbody.velocity);
                parent.rigidbody.velocity *= randInitialVel;
            }
        }

        public override void AffectSelf()
        {
            //parent.body.position.x += parent.rigidbody.velocity.x * VelocityModifier;
            //parent.body.position.y += parent.rigidbody.velocity.y * VelocityModifier;
            //return;
            if (mode == movemode.screenwrap) screenWrap();
            if (mode == movemode.wallbounce) wallBounce();
            if (mode == movemode.falloff)    fallOff();
            if (mode == movemode.halt) halt();

            //GraphData.AddFloat(parent.transform.position.x);

            //Trippy();
        }

        public void Trippy()
        {
            //test (holy SHIT that looks cool)
            PropertyInfo pi = parent.body.GetType().GetProperty("scale");
            pi.SetValue(parent.body, parent.transform.position.x % 4.0f, null);
        }
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
            float result = GMath.AngleLerp(parent.body.orient, v, 0.1f);

            parent.body.orient =(result);

            stick *= 0.4f;
            stick *= absaccel;
            if ((parent.rigidbody.velocity.x != 0 || parent.rigidbody.velocity.y != 0))
            {
                stick += parent.rigidbody.velocity * -friction;
            }
            stick *= parent.body.mass;
            //todo: update maxvel?
            parent.body.ApplyForce(stick);


        }
        //reminder: make a vocal recognition extension for visual studio to take you where you want ("Class: Movement. Method: fallOff.")
        public void fallOff()
        {
            int levelwidth = room.worldWidth;
            int levelheight = room.worldHeight;

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
            int levelwidth = room.worldWidth;
            int levelheight = room.worldHeight;
            float halfwidth = levelwidth / 2f;
            float halfheight = levelheight / 2f;

            int maxX = (int)(OrbIt.origin.x + halfwidth);
            int minX = (int)(OrbIt.origin.x - halfwidth);
            int maxY = (int)(OrbIt.origin.y + halfheight);
            int minY = (int)(OrbIt.origin.y - halfheight);

            if (parent.transform.position.x >= (maxX - parent.radius))
            {
                //float off = parent.transform.position.x - (levelwidth - parent.radius);
                //parent.transform.position.x = (levelwidth - parent.radius - off) % room.worldWidth;
                parent.transform.position.x = GMath.Triangle(parent.transform.position.x, maxX - (int)parent.radius);
                if (parent.rigidbody.velocity.x > 0)
                    parent.rigidbody.velocity.x *= -1;
                //parent.body.InvokeOnCollisionStay(null); //todo: find out why we needed null, fix this

            }
            if (parent.transform.position.x < minX + parent.radius)
            {
                //float off = parent.radius - parent.transform.position.x;
                //parent.transform.position.x = (parent.radius + off) % room.worldWidth;
                parent.transform.position.x = GMath.Triangle(parent.transform.position.x - parent.radius + halfwidth, room.worldWidth) + parent.radius - halfwidth;
                if (parent.rigidbody.velocity.x < 0)
                    parent.rigidbody.velocity.x *= -1;
                //parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.transform.position.y >= (maxY - parent.radius))
            {
                //float off = parent.transform.position.y - (levelheight - parent.radius);
                //parent.transform.position.y = (levelheight - parent.radius - off) % room.worldHeight;
                parent.transform.position.y = GMath.Triangle(parent.transform.position.y, maxY - (int)parent.radius);
                if (parent.rigidbody.velocity.y > 0)
                    parent.rigidbody.velocity.y *= -1;
                //parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.transform.position.y < minY + parent.radius)
            {
                //float off = parent.radius - parent.transform.position.y;
                //parent.transform.position.y = (parent.radius + off) % room.worldHeight;
                parent.transform.position.y = GMath.Triangle(parent.transform.position.y - parent.radius + halfheight, room.worldHeight) + parent.radius - halfheight;
                if (parent.rigidbody.velocity.y < 0)
                    parent.rigidbody.velocity.y *= -1;
                //parent.body.InvokeOnCollisionStay(null);
            }
        }

        public void halt()
        {
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game...;
            int levelwidth = room.worldWidth;
            int levelheight = room.worldHeight;

            if (parent.transform.position.x >= (levelwidth - parent.radius))
            {
                parent.transform.position.x = levelwidth - parent.radius;
                parent.rigidbody.velocity.x *= 0;
                parent.body.InvokeOnCollisionStay(null);

            }
            if (parent.transform.position.x < parent.radius)
            {
                parent.transform.position.x = parent.radius;
                parent.rigidbody.velocity.x *= 0;
                parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.transform.position.y >= (levelheight - parent.radius))
            {
                parent.transform.position.y = levelheight - parent.radius;
                parent.rigidbody.velocity.y *= 0;
                parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.transform.position.y < parent.radius)
            {
                parent.transform.position.y = parent.radius;
                parent.rigidbody.velocity.y *= 0;
                parent.body.InvokeOnCollisionStay(null);
            }


        }

        
        public void screenWrap()
        {
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game...;
            int levelwidth = room.worldWidth;
            int levelheight = room.worldHeight;

            //todo: modulus screen width
            //hitting top/bottom of screen
            //teleport node
            if (parent.transform.position.x >= levelwidth)
            {
                parent.transform.position.x = parent.transform.position.x - levelwidth;//1;
            }
            else if (parent.transform.position.x < 0)
            {
                parent.transform.position.x = levelwidth - parent.transform.position.x;//1;
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
                parent.transform.position.y = parent.transform.position.y - levelheight;//1;
            }
            else if (parent.transform.position.y < 0)
            {
                parent.transform.position.y = levelheight - parent.transform.position.y;//1;
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
