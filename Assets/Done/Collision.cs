using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbItProcs
{
    /// <summary>
    /// Nodes with this component bounce away from each other upon contact
    /// </summary>
    [Info(UserLevel.User, "Nodes with this component bounce away from each other upon contact", CompType)]
    public class Collision : Component, ILinkable
    {
        public const mtypes CompType = mtypes.draw | mtypes.affectself | mtypes.essential;
        public override mtypes compType { get { return CompType; } set { } }
        public Link link { get; set; }

        /// <summary>
        /// When enabled, draws the ring circle around the node while collision is active.
        /// </summary>
        [Info(UserLevel.User, "When enabled, draws the ring circle around the node while collision is active.")]
        public bool DrawRing { get; set; }

        public static Func<Manifold, Collider, Collider, bool>[,] Dispatch = new Func<Manifold, Collider, Collider, bool>[2, 2]
        {
            {CircletoCircle, CircletoPolygon},
            {PolygontoCircle, PolygontoPolygon},
        };
        public static Func<Collider, Collider, bool>[,] CheckDispatch = new Func<Collider, Collider, bool>[2, 2]
        {
            {CircletoCircleCheck, CircletoPolygonCheck},
            {PolygontoCircleCheck, PolygontoPolygonCheck},
        };

        public static bool CheckCollision(Collider aa, Collider bb)
        {
            return Collision.CheckDispatch[(int)aa.shape.GetShapeType(), (int)bb.shape.GetShapeType()](aa, bb);
        }



        [Info(UserLevel.Developer)]
        public Dictionary<string, Collider> colliders = new Dictionary<string, Collider>();

        public void AddCollider(string key, Collider col)
        {
            int i = 0;
            while(colliders.ContainsKey(key))
            {
                key = key + i++;
            }
            colliders.Add(key, col);
            col.parent = parent;
            if (active && parent != null && parent.group != null)
            {
                UpdateCollisionSet();
            }
        }

        public Collider GetCollider(string key, bool remove = false)
        {
            if (colliders.ContainsKey(key))
            {
                Collider col = colliders[key];
                if (remove)
                {
                    colliders.Remove(key);
                }
                return col;
            }
            return null;
        }
        // Beware of undefined behaviour from the delegates on the colliders, they may contain context based references
        public void SwapCollider(Node other, string key)
        {
            Collider temp = GetCollider(key, true);
            Collider temp2 = other.collision.GetCollider(key, true);
            if (temp != null)
            {
                other.collision.AddCollider(key, temp);
            }
            if (temp2 != null)
            {
                AddCollider(key, temp2);
            }
        }

        public void SwapAllColliders(Node other)
        {
            foreach(string key in colliders.Keys)
            {
                SwapCollider(other, key);
            }
        }

        public bool isSolid { get { return parent != null && parent.body.isSolid; } set { if (parent != null) parent.body.isSolid = value; } }
        
        private bool _AllHandlersEnabled = true;
        [Info(UserLevel.Developer)]
        public bool AllHandlersEnabled
        {
            get { return _AllHandlersEnabled; }
            set
            {
                //if (_active && parent != null && parent.active && !parent.IsDefault)
                //{
                //    if (value)
                //    {
                //        if (parent.body.HandlersEnabled) 
                //            room.AddCollider(parent.body);
                //        foreach (Collider col in colliders.Values)
                //        {
                //            if (col.HandlersEnabled) room.AddCollider(col);
                //        }
                //    }
                //    else
                //    {
                //        if (!parent.body.isSolid) 
                //            room.RemoveCollider(parent.body);
                //        foreach (Collider col in colliders.Values)
                //        {
                //            room.RemoveCollider(col);
                //        }
                //    }
                //}

                _AllHandlersEnabled = value;
                UpdateCollisionSet();
            }
        }
        [Info(UserLevel.Developer)]
        public override bool active
        {
            get { return _active; }
            set
            {
                //if (parent != null && OrbIt.ui != null && !parent.IsDefault)
                //{
                //    if (value)
                //    {
                //        if (parent.body.isSolid || parent.body.HandlersEnabled) 
                //            room.AddCollider(parent.body);
                //        foreach (Collider col in colliders.Values)
                //        {
                //            if (col.HandlersEnabled) room.AddCollider(col);
                //        }
                //    }
                //    else
                //    {
                //        RemoveCollidersFromSet();
                //    }
                //}
                _active = value;
                //if (OrbIt.ui != null)
                    UpdateCollisionSet();
            }
        }

        public void UpdateCollisionSet()
        {
            if (parent != null && !parent.IsDefault)
            {
                if (room.masterGroup == null || !room.masterGroup.fullSet.Contains(parent)) return;
                if (active && AllHandlersEnabled)
                {
                    if (parent.body.isSolid || parent.body.HandlersEnabled)
                        room.collisionManager.AddCollider(parent.body);
                    else
                        room.collisionManager.RemoveCollider(parent.body);

                    foreach (Collider col in colliders.Values)
                    {
                        if (col.HandlersEnabled) room.collisionManager.AddCollider(col);
                        else room.collisionManager.RemoveCollider(col);
                    }
                }
                else
                {
                    RemoveCollidersFromSet();
                }
            }
        }

        public void RemoveCollidersFromSet()
        {
            room.collisionManager.RemoveCollider(parent.body);
            foreach (Collider col in colliders.Values)
            {
                room.collisionManager.RemoveCollider(col);
            }
        }

        public Collision() : this(null) { }
        public Collision(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            _AllHandlersEnabled = true;
            UpdateCollisionSet();
        }

        public override void OnSpawn()
        {
            UpdateCollisionSet();
        }

        public override void AffectOther(Node other) //only for links (no handlers)
        {
            if (!active) return;
            Manifold m = new Manifold(parent.body, other.body);
            m.Solve();
            if (m.contact_count > 0)
            {
                room.collisionManager.AddManifold(m);
            }
        }

        public override void AffectSelf()
        {
            if (AllHandlersEnabled)
            {
                foreach (Collider collider in colliders.Values)
                {
                    if (collider.HandlersEnabled)
                    {
                        collider.pos = parent.body.pos + collider.objectSpacePos;
                    }
                }
            }
        }

        public override void Draw()
        {
            if (!DrawRing) return;
            //Console.WriteLine(Utils.random.Next(10));
            //room.camera.Draw(textures.ring, parent.body.pos, Color.red, parent.body.scale, Layers.Under2);

            foreach (Collider cc in colliders.Values)
            {
                if (cc.HandlersEnabled)
                {
                    float scale = cc.radius / parent.getSprite().texture.width * 2;
                    room.camera.Draw(textures.ring, parent.body.pos, parent.body.color, scale, Layers.Under2);
                }
            }
        }
        public void ClearCollisionLists()
        {
            if (!active) return;
            parent.body.ClearCollisionList();
            foreach (Collider c in colliders.Values)
            {
                c.ClearCollisionList();
            }
        }


        public IEnumerable<List<Node>> CollisionHelper(List<Node> list)
        {
            List<Node> list1 = new List<Node>();
            List<Node> list2 = new List<Node>();
            while(true)
            {
                list1 = list;
                yield return list2;
                list2 = list;
                yield return list1;
            }
        }



        public static bool CircletoCircle(Manifold m, Collider a, Collider b)
        {
            Circle ca = (Circle)a.shape;
            Circle cb = (Circle)b.shape;
            Vector2 normal = b.pos - a.pos;
            float distSquared = normal.sqrMagnitude;
            double radius = a.radius + b.radius;
            
            if (distSquared >= (float)(radius * radius))
            {
                m.contact_count = 0;
                return false;
            }

            double distance = Math.Sqrt(distSquared);
            m.contact_count = 1;

            if (distance == 0)
            {
                m.penetration = ca.radius;
                m.normal = new Vector2(1, 0);
                m.contacts[0] = a.pos;
            }
            else
            {
                m.penetration = radius - distance;
                m.normal = VMath.MultVectDouble(normal, 1.0 / distance); //normal / distance;
                m.contacts[0] = VMath.MultVectDouble(m.normal, ca.radius) + a.pos; //m.normal * ca.radius + a.body.position;
            }
            return true;
        }

        public static bool CircletoPolygon(Manifold m, Collider a, Collider b)
        {
            Circle A = (Circle)a.shape;
            Polygon B = (Polygon)b.shape;
            m.contact_count = 0;

            // Transform circle center to Polygon model space
            Vector2 center = a.pos;
            center = B.u.Transpose() * (center - b.pos);

            // Find edge with minimum penetration
            // Exact concept as using support points in Polygon vs Polygon
            double separation = -float.MaxValue;
            int faceNormal = 0;
            for (int i = 0; i < B.vertexCount; ++i)
            {
                double s = Vector2.Dot(B.normals[i], center - B.vertices[i]);

                if (s > A.radius)
                {
                    return false;
                }

                if (s > separation)
                {
                    separation = s;
                    faceNormal = i;
                }
            }

            // Grab face's vertices
            Vector2 v1 = B.vertices[faceNormal];
            int i2 = faceNormal + 1 < B.vertexCount ? faceNormal + 1 : 0;
            Vector2 v2 = B.vertices[i2];

            // Check to see if center is within polygon
            if (separation < GMath.EPSILON)
            {
                m.contact_count = 1;
                m.normal = -(B.u * B.normals[faceNormal]);
                m.contacts[0] = VMath.MultVectDouble(m.normal, A.radius) + a.pos;
                m.penetration = A.radius;
                return true;
            }

            // Determine which voronoi region of the edge center of circle lies within
            double dot1 = Vector2.Dot(center - v1, v2 - v1);
            double dot2 = Vector2.Dot(center - v2, v1 - v2);
            m.penetration = A.radius - separation;

            // Closest to v1
            if (dot1 <= 0.0f)
            {
                if (Vector2.Distance(center, v1) > A.radius)
                {
                    return false;
                }
                m.contact_count = 1;
                Vector2 n = v1 - center;
                n = B.u * n;
                VMath.NormalizeSafe(ref n);
                m.normal = n;
                v1 = B.u * v1 + b.pos;
                m.contacts[0] = v1;
            }
            // Closest to v2
            else if (dot2 <= 0.0f)
            {
                if (Vector2.Distance(center, v2) > A.radius)
                {
                    return false;
                }

                m.contact_count = 1;
                Vector2 n = v2 - center;
                v2 = B.u * v2 + b.pos;
                m.contacts[0] = v2;
                n = B.u * n;
                VMath.NormalizeSafe(ref n);
                m.normal = n;
            }
            // Closest to face
            else
            {
                Vector2 n = B.normals[faceNormal];
                if (Vector2.Dot(center - v1, n) > A.radius)
                {
                    return false;
                }

                n = B.u * n;
                m.normal = -n;
                m.contacts[0] = VMath.MultVectDouble(m.normal, A.radius) + a.pos;
                m.contact_count = 1;
            }
            return true;
        }

        public static bool PolygontoCircle(Manifold m, Collider a, Collider b)
        {
            bool ret = CircletoPolygon(m, b, a);
            m.normal = -m.normal;
            return ret;
        }

        public static double FindAxisLeastPenetration(ref int faceIndex, Polygon A, Polygon B)
        {
            double bestDistance = -float.MaxValue;
            int bestIndex = 0;

            for (int i = 0; i < A.vertexCount; ++i)
            {
                // Retrieve a face normal from A
                Vector2 n = A.normals[i];
                Vector2 nw = A.u * n;

                // Transform face normal into B's model space
                Mat22 buT = B.u.Transpose();
                n = buT * nw;

                // Retrieve support point from B along -n
                Vector2 s = B.GetSupport(-n);

                // Retrieve vertex on face from A, transform into
                // B's model space
                Vector2 v = A.vertices[i];
                v = A.u * v + A.body.pos;
                v -= B.body.pos;
                v = buT * v;

                // Compute penetration distance (in B's model space)
                double d = Vector2.Dot(n, s - v);

                // Store greatest distance
                if (d > bestDistance)
                {
                    bestDistance = d;
                    bestIndex = i;
                }
            }

            faceIndex = bestIndex;
            return bestDistance;
        }

        public static void FindIncidentFace(ref Vector2[] v, Polygon RefPoly, Polygon IncPoly, int referenceIndex)
        {
            Vector2 referenceNormal = RefPoly.normals[referenceIndex];

            // Calculate normal in incident's frame of reference
            referenceNormal = RefPoly.u * referenceNormal; // To world space
            referenceNormal = IncPoly.u.Transpose() * referenceNormal; // To incident's model space

            // Find most anti-normal face on incident polygon
            int incidentFace = 0;
            double minDot = float.MaxValue;
            for (int i = 0; i < IncPoly.vertexCount; ++i)
            {
                double dot = Vector2.Dot(referenceNormal, IncPoly.normals[i]);
                if (dot < minDot)
                {
                    minDot = dot;
                    incidentFace = i;
                }
            }

            // Assign face vertices for incidentFace
            v[0] = IncPoly.u * IncPoly.vertices[incidentFace] + IncPoly.body.pos;
            incidentFace = incidentFace + 1 >= IncPoly.vertexCount ? 0 : incidentFace + 1;
            v[1] = IncPoly.u * IncPoly.vertices[incidentFace] + IncPoly.body.pos;
        }

        public static int Clip(Vector2 n, double c, ref Vector2[] face)
        {
            int sp = 0;
            Vector2[] outV = new Vector2[2]{
                face[0],
                face[1],
            };

            // Retrieve distances from each endpoint to the line
            // d = ax + by - c
            double d1 = Vector2.Dot(n, face[0]) - c;
            double d2 = Vector2.Dot(n, face[1]) - c;

            // If negative (behind plane) clip
            if (d1 <= 0.0f) outV[sp++] = face[0];
            if (d2 <= 0.0f) outV[sp++] = face[1];

            // If the points are on different sides of the plane
            if (d1 * d2 < 0.0f) // less than to ignore -0.0f
            {
                // Push interesection point
                double alpha = d1 / (d1 - d2);
                outV[sp] = face[0] + VMath.MultVectDouble((face[1] - face[0]), alpha);
                ++sp;
            }

            // Assign our new converted values
            face[0] = outV[0];
            face[1] = outV[1];

            //assert( sp != 3 );
            System.Diagnostics.Debug.Assert(sp != 3);


            return sp;
        }

        public static bool PolygontoPolygon(Manifold m, Collider a, Collider b)
        {
            Polygon A = (Polygon)a.shape;
            Polygon B = (Polygon)b.shape;
            m.contact_count = 0;

            // Check for a separating axis with A's face planes
            int faceA = 0;
            double penetrationA = FindAxisLeastPenetration(ref faceA, A, B);
            if (penetrationA >= 0.0f)
                return false;

            // Check for a separating axis with B's face planes
            int faceB = 0;
            double penetrationB = FindAxisLeastPenetration(ref faceB, B, A);
            if (penetrationB >= 0.0f)
                return false;

            int referenceIndex;
            bool flip; // Always point from a to b

            Polygon RefPoly; // Reference
            Polygon IncPoly; // Incident

            // Determine which shape contains reference face
            if (GMath.BiasGreaterThan(penetrationA, penetrationB))
            {
                RefPoly = A;
                IncPoly = B;
                referenceIndex = faceA;
                flip = false;
            }

            else
            {
                RefPoly = B;
                IncPoly = A;
                referenceIndex = faceB;
                flip = true;
            }

            // World space incident face
            Vector2[] incidentFace = new Vector2[2];
            FindIncidentFace(ref incidentFace, RefPoly, IncPoly, referenceIndex);

            //        y
            //        ^  .n       ^
            //      +---c ------posPlane--
            //  x < | i |\
            //      +---+ c-----negPlane--
            //             \       v
            //              r
            //
            //  r : reference face
            //  i : incident poly
            //  c : clipped point
            //  n : incident normal

            // Setup reference face vertices
            Vector2 v1 = RefPoly.vertices[referenceIndex];
            referenceIndex = referenceIndex + 1 == RefPoly.vertexCount ? 0 : referenceIndex + 1;
            Vector2 v2 = RefPoly.vertices[referenceIndex];

            // Transform vertices to world space
            v1 = RefPoly.u * v1 + RefPoly.body.pos;
            v2 = RefPoly.u * v2 + RefPoly.body.pos;

            // Calculate reference face side normal in world space
            Vector2 sidePlaneNormal = (v2 - v1);
            VMath.NormalizeSafe(ref sidePlaneNormal);

            // Orthogonalize
            Vector2 refFaceNormal = new Vector2(sidePlaneNormal.y, -sidePlaneNormal.x);

            // ax + by = c
            // c is distance from origin
            double refC = Vector2.Dot(refFaceNormal, v1);
            double negSide = -Vector2.Dot(sidePlaneNormal, v1);
            double posSide = Vector2.Dot(sidePlaneNormal, v2);

            // Clip incident face to reference face side planes
            if (Clip(-sidePlaneNormal, negSide, ref incidentFace) < 2)
                return false; // Due to floating point error, possible to not have required points

            if (Clip(sidePlaneNormal, posSide, ref incidentFace) < 2)
                return false; // Due to floating point error, possible to not have required points

            // Flip
            m.normal = flip ? -refFaceNormal : refFaceNormal;

            // Keep points behind reference face
            int cp = 0; // clipped points behind reference face
            double separation = Vector2.Dot(refFaceNormal, incidentFace[0]) - refC;
            if (separation <= 0.0f)
            {
                m.contacts[cp] = incidentFace[0];
                m.penetration = -separation;
                ++cp;
            }
            else
                m.penetration = 0;

            separation = Vector2.Dot(refFaceNormal, incidentFace[1]) - refC;
            if (separation <= 0.0f)
            {
                m.contacts[cp] = incidentFace[1];

                m.penetration += -separation;
                ++cp;

                // Average penetration
                m.penetration /= (double)cp;
            }
            
            m.contact_count = cp;
            return cp > 0;
        }


        ////////FUCKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK
        public static bool CircletoCircleCheck(Collider a, Collider b)
        {
            Vector2 normal = b.pos - a.pos;
            float distSquared = normal.sqrMagnitude;
            double radius = a.radius + b.radius;
            return distSquared < radius * radius;
        }

        public static bool CircletoPolygonCheck(Collider a, Collider b)
        {
            Circle A = (Circle)a.shape;
            Polygon B = (Polygon)b.shape;

            //m.contact_count = 0;

            // Transform circle center to Polygon model space
            Vector2 center = a.pos;
            center = B.u.Transpose() * (center - b.pos);

            // Find edge with minimum penetration
            // Exact concept as using support points in Polygon vs Polygon
            double separation = -float.MaxValue;
            int faceNormal = 0;
            for (int i = 0; i < B.vertexCount; ++i)
            {
                double s = Vector2.Dot(B.normals[i], center - B.vertices[i]);

                if (s > A.radius)
                {
                    return false;
                }

                if (s > separation)
                {
                    separation = s;
                    faceNormal = i;
                }
            }

            // Grab face's vertices
            Vector2 v1 = B.vertices[faceNormal];
            int i2 = faceNormal + 1 < B.vertexCount ? faceNormal + 1 : 0;
            Vector2 v2 = B.vertices[i2];

            // Check to see if center is within polygon
            if (separation < GMath.EPSILON)
            {
                //m.contact_count = 1;
                //m.normal = -(B.u * B.normals[faceNormal]);
                //m.contacts[0] = VMath.MultVectDouble(m.normal, A.radius) + a.pos;
                //m.penetration = A.radius;
                return true;
            }

            // Determine which voronoi region of the edge center of circle lies within
            double dot1 = Vector2.Dot(center - v1, v2 - v1);
            double dot2 = Vector2.Dot(center - v2, v1 - v2);
            //m.penetration = A.radius - separation;

            // Closest to v1
            if (dot1 <= 0.0f)
            {
                if (Vector2.Distance(center, v1) > A.radius)
                {
                    return false;
                }
                //m.contact_count = 1;
                //Vector2 n = v1 - center;
                //n = B.u * n;
                //n.Normalize();
                //m.normal = n;
                //v1 = B.u * v1 + b.pos;
                //m.contacts[0] = v1;
            }
            // Closest to v2
            else if (dot2 <= 0.0f)
            {
                if (Vector2.Distance(center, v2) > A.radius)
                {
                    return false;
                }

                //m.contact_count = 1;
                //Vector2 n = v2 - center;
                //v2 = B.u * v2 + b.pos;
                //m.contacts[0] = v2;
                //n = B.u * n;
                //n.Normalize();
                //m.normal = n;
            }
            // Closest to face
            else
            {
                Vector2 n = B.normals[faceNormal];
                if (Vector2.Dot(center - v1, n) > A.radius)
                {
                    return false;
                }

                //n = B.u * n;
                //m.normal = -n;
                //m.contacts[0] = VMath.MultVectDouble(m.normal, A.radius) + a.pos;
                //m.contact_count = 1;
            }
            return true;
        }

        public static bool PolygontoCircleCheck(Collider a, Collider b)
        {
            return CircletoPolygonCheck(b, a);
            //m.normal = -m.normal;
            //return ret;
        }

        public static bool PolygontoPolygonCheck(Collider a, Collider b)
        {
            Polygon A = (Polygon)a.shape;
            Polygon B = (Polygon)b.shape;
            //m.contact_count = 0;

            // Check for a separating axis with A's face planes
            int faceA = 0;
            double penetrationA = FindAxisLeastPenetration(ref faceA, A, B);
            if (penetrationA >= 0.0f)
                return false;

            // Check for a separating axis with B's face planes
            int faceB = 0;
            double penetrationB = FindAxisLeastPenetration(ref faceB, B, A);
            if (penetrationB >= 0.0f)
                return false;

            int referenceIndex;
            //bool flip; // Always point from a to b

            Polygon RefPoly; // Reference
            Polygon IncPoly; // Incident

            // Determine which shape contains reference face
            if (GMath.BiasGreaterThan(penetrationA, penetrationB))
            {
                RefPoly = A;
                IncPoly = B;
                referenceIndex = faceA;
                //flip = false;
            }
            else
            {
                RefPoly = B;
                IncPoly = A;
                referenceIndex = faceB;
                //flip = true;
            }
            // World space incident face
            Vector2[] incidentFace = new Vector2[2];
            FindIncidentFace(ref incidentFace, RefPoly, IncPoly, referenceIndex);
            // Setup reference face vertices
            Vector2 v1 = RefPoly.vertices[referenceIndex];
            referenceIndex = referenceIndex + 1 == RefPoly.vertexCount ? 0 : referenceIndex + 1;
            Vector2 v2 = RefPoly.vertices[referenceIndex];

            // Transform vertices to world space
            v1 = RefPoly.u * v1 + RefPoly.body.pos;
            v2 = RefPoly.u * v2 + RefPoly.body.pos;

            // Calculate reference face side normal in world space
            Vector2 sidePlaneNormal = (v2 - v1);
            VMath.NormalizeSafe(ref sidePlaneNormal);

            // Orthogonalize
            Vector2 refFaceNormal = new Vector2(sidePlaneNormal.y, -sidePlaneNormal.x);

            // ax + by = c
            // c is distance from origin
            double refC = Vector2.Dot(refFaceNormal, v1);
            double negSide = -Vector2.Dot(sidePlaneNormal, v1);
            double posSide = Vector2.Dot(sidePlaneNormal, v2);

            // Clip incident face to reference face side planes
            if (Clip(-sidePlaneNormal, negSide, ref incidentFace) < 2)
                return false; // Due to floating point error, possible to not have required points

            if (Clip(sidePlaneNormal, posSide, ref incidentFace) < 2)
                return false; // Due to floating point error, possible to not have required points

            // Flip
            //m.normal = flip ? -refFaceNormal : refFaceNormal;

            // Keep points behind reference face
            int cp = 0; // clipped points behind reference face
            double separation = Vector2.Dot(refFaceNormal, incidentFace[0]) - refC;
            if (separation <= 0.0f)
            {
                //m.contacts[cp] = incidentFace[0];
                //m.penetration = -separation;
                ++cp;
            }
            //else
            //    m.penetration = 0;

            separation = Vector2.Dot(refFaceNormal, incidentFace[1]) - refC;
            if (separation <= 0.0f)
            {
                //m.contacts[cp] = incidentFace[1];
                //
                //m.penetration += -separation;
                ++cp;

                // Average penetration
                //m.penetration /= (double)cp;
            }

            //m.contact_count = cp;
            return cp > 0;
        }

    }
}
