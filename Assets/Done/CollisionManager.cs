 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace OrbItProcs
{
    public class CollisionManager : RoomComponent
    {

        public int colIterations { get; set; }
        public HashSet<Collider> CollisionSetCircle { get; set; }
        public HashSet<Collider> CollisionSetPolygon { get; set; }
        private List<Manifold> contacts = new List<Manifold>();
        public GridSystem gridsystemCollision { get; set; }

        Action<Collider, Collider> collideAction;
        Room room;

        public CollisionManager(Room room)
        {
            this.room = room;
            CollisionSetCircle = new HashSet<Collider>();
            CollisionSetPolygon = new HashSet<Collider>();
            colIterations = 1;

            collideAction = (c1, c2) =>
            {
                if (c1.parent == c2.parent) return;
                if (c1 is Body)
                {
                    Body b = (Body)c1;

                    if (gridsystemCollision.alreadyVisited.Contains(c2)) return;
                    if (c2 is Body)
                    {
                        Body bb = (Body)c2;
                        //if (!b.exclusionList.Contains(bb)) 
                        b.CheckCollisionBody(bb);
                    }
                    else
                    {
                        b.CheckCollisionCollider(c2);
                    }
                }
                else
                {
                    if (gridsystemCollision.alreadyVisited.Contains(c2)) return;
                    if (c2 is Body)
                    {
                        Body bb = (Body)c2;
                        //if (!c1.exclusionList.Contains(bb)) 
                        c1.CheckCollisionBody(bb);
                    }
                }
            };

            //gridsystemCollision = new GridSystem(room, room.gridsystemAffect.cellsX, new Vector2(0, room.worldHeight - OrbIt.ScreenHeight), room.worldWidth, OrbIt.ScreenHeight);
            Vector2 position = new Vector2(OrbIt.origin.x - room.worldWidth/2 , OrbIt.origin.y - room.worldHeight/2);
            gridsystemCollision = new GridSystem(room, room.gridsystemAffect.cellsX, position, room.worldWidth, room.worldHeight);


        }

        
        public void AddManifold(Manifold m)
        {
            contacts.Add(m);
        }


        public void AddCollider(Collider collider)
        {
            if (collider.shape is Circle)
                CollisionSetCircle.Add(collider);
            else if (collider.shape is Polygon)
                CollisionSetPolygon.Add(collider);
        }
        public void RemoveCollider(Collider collider)
        {
            if (CollisionSetCircle.Contains(collider))
                CollisionSetCircle.Remove(collider);
            if (CollisionSetPolygon.Contains(collider))
                CollisionSetPolygon.Remove(collider);
        }

        static int algorithm = 7;

        public override void Update()
        {
            //Testing.modInc();
            if (algorithm >= 5)
            {
                gridsystemCollision.clearBuckets();
                foreach (var c in CollisionSetCircle) //.ToList()
                {
                    //if (skipOutsideGrid && (c.pos.y < gridsystemCollision.position.y || c.pos.y > gridsystemCollision.position.y + gridsystemCollision.gridHeight)) continue;

                    //if (ColorNodesInReach) c.parent.body.color = Color.White;
                    if (!c.parent.active) continue;
                    gridsystemCollision.insertToBuckets(c);
                }
            }

            //Testing.PrintTimer("insertion");//oh, mama.

            gridsystemCollision.alreadyVisited = new HashSet<Collider>();

            foreach (var c in CollisionSetPolygon.ToList())
            {
                if (c.parent.active)
                {
                    gridsystemCollision.alreadyVisited.Add(c);
                    int reach = (int)c.radius * 2;
                    if (c.shape is Circle)//todo fix the fact that circles are in the polygons list
                    {
                        reach = (int)(c.shape as Polygon).polyReach;
                    }
                    foreach (var otherCol in CollisionSetPolygon.ToList())
                    {
                        collideAction(c, otherCol);
                    }
                    foreach (var otherCol in CollisionSetCircle.ToList())
                    {
                        collideAction(c, otherCol);
                    }
                }
            }

            foreach (var c in CollisionSetCircle.ToList()) //.ToList() 
            {
                //if (skipOutsideGrid && (c.pos.y < gridsystemCollision.position.y || c.pos.y > gridsystemCollision.position.y + gridsystemCollision.gridHeight)) continue;
                if (c.parent.active)
                {
                    gridsystemCollision.alreadyVisited.Add(c);
                    int reach = (int)c.radius * 2;
                    if (c.shape is Polygon)
                    {
                        reach = (int)(c.shape as Polygon).polyReach; //shouldnt have a polygon in the circle list
                        foreach (var otherCol in CollisionSetCircle.ToList())
                        {
                            collideAction(c, otherCol);
                        }
                    }
                    else if (algorithm == 7)
                    {
                        gridsystemCollision.retrieveOffsetArraysCollision(c, collideAction, c.radius * 2);
                    }
                    //else if (algorithm == 6)
                    //{
                    //    gridsystemCollision.retrieveFromAllOffsets(c, reach, collideAction);
                    //}
                }
            }
            //Testing.PrintTimer("insertion");
            //Testing.PrintTimer("retrieve");
            //Testing.PrintTimer("manifolds");
            //COLLISION
            foreach (Manifold m in contacts)
            {
                m.Initialize();
            }
            // \COLLISION
            //update velocity
            for (int ii = 0; ii < colIterations; ii++)
            {
                foreach (Manifold m in contacts)
                {
                    m.ApplyImpulse();
                    //m.a.parent.SetColor(Color.Green);
                    //m.b.parent.SetColor(Color.Yellow);
                }
            }
            foreach (Node n in room.masterGroup.fullSet.ToList())
            {
                if (room.skipOutsideGrid && n.body.pos.isWithin(gridsystemCollision.position, gridsystemCollision.position + new Vector2(gridsystemCollision.gridWidth, gridsystemCollision.gridHeight))) continue;
                n.movement.IntegrateVelocity();
                VMath.Set(ref n.body.force, 0, 0);
                n.body.torque = 0;
            }
            foreach (Manifold m in contacts)
                m.PositionalCorrection();

            if (contacts.Count > 0) contacts = new List<Manifold>();
        }

        public override void Draw(){
            gridsystemCollision.DrawGrid(room, Color.blue);
        }
    }
}
