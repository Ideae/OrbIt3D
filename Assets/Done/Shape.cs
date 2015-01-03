using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UnityEngine;
namespace OrbItProcs
{
    

    public enum ShapeType
    {
        Circle,
        Polygon,
    }

    public abstract class Shape
    {
        public static int TypeCount = 2;

        public Body body ; //{ get; set; }
        public float radius { get; set; }
        public Mat22 u;// { get; set; }

        //public Body bodyP { get { return body; } set { body = value; } }
        public Mat22 uP { get { return u; } set { u = value; } }

        public Shape() { }

        public abstract Shape Clone();
        public abstract void Initialize();
        public abstract void ComputeMass(float density);
        public abstract void SetOrient(float radians);
        public abstract void Draw();
        public abstract ShapeType GetShapeType();
        
    }

    public class Circle : Shape
    {
        public Circle(float r)
        {
            radius = r;
        }
        public override Shape Clone()
        {
            return new Circle(radius);
        }
        public override void Initialize()
        {
            ComputeMass( 0.001f );
        }
        public override void ComputeMass(float density)
        {
            body.mass = (float)Math.PI * radius * radius * density;
            body.inertia = body.mass * radius * radius;
        }
        public override ShapeType GetShapeType()
        {
            return ShapeType.Circle;
        }
        public override void SetOrient(float radians)
        { }
        public override void Draw()
        { }

        
    }

    public class Polygon : Shape
    {
        public static int MaxPolyVertexCount = 64;
        public int vertexCount { get; set; }
        public Vector2[] vertices = new Vector2[MaxPolyVertexCount];
        public Vector2[] normals = new Vector2[MaxPolyVertexCount];

        public float polyReach = 0;

        public float LineThickness { get; set; }
        public bool RecurseDrawEnabled { get; set; }
        public int RecurseCount { get; set; }
        public float RecurseScaleReduction { get; set; }
        public bool FillEnabled { get; set; }


        public float[,] verticesP
        {
            get
            {
                float[,] result = new float[MaxPolyVertexCount, 2];
                for (int i = 0; i < MaxPolyVertexCount; i++)
                {
                    result[i, 0] = vertices[i].x;
                    result[i, 1] = vertices[i].y;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < MaxPolyVertexCount; i++)
                {
                    vertices[i].x = value[i, 0];
                    vertices[i].y = value[i, 1];
                }
            }
        }
        public float[,] normalsP
        {
            get
            {
                float[,] result = new float[MaxPolyVertexCount, 2];
                for (int i = 0; i < MaxPolyVertexCount; i++)
                {
                    result[i, 0] = normals[i].x;
                    result[i, 1] = normals[i].y;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < MaxPolyVertexCount; i++)
                {
                    normals[i].x = value[i, 0];
                    normals[i].y = value[i, 1];
                }
            }
        }


        public Polygon() {
            LineThickness = 1f;
            RecurseDrawEnabled = false;
            RecurseCount = 1;
            RecurseScaleReduction = 0.2f;
            FillEnabled = false;
            if (body != null)
            {
                body.orient = body.orient;
            }
        }
        
        public override void Initialize()
        {
            ComputeMass(0.001f);
        }
        public override Shape Clone ()
        {
            Polygon poly = new Polygon();
            poly.u = u;
            poly.RecurseCount = RecurseCount;
            poly.RecurseDrawEnabled = RecurseDrawEnabled;
            poly.RecurseScaleReduction = RecurseScaleReduction;
            poly.LineThickness = LineThickness;
            poly.FillEnabled = FillEnabled;


            for (int i = 0; i < vertexCount; i++)
            {
                poly.vertices[i] = vertices[i];
                poly.normals[i] = normals[i];
            }
            poly.vertexCount = vertexCount;
            poly.polyReach = polyReach;
            return poly;
        }
        public override void ComputeMass(float density)
        {
            //calculate centroid and moment of inertia
            Vector2 c = new Vector2(0, 0); // centroid
            double area = 0;
            double I = 0;
            double k_inv3 = 1.0 / 3.0;

            for (int i1 = 0; i1 < vertexCount; i1++)
            {
                Vector2 p1 = vertices[i1];
                int i2 = i1 + 1 < vertexCount ? i1 + 1 : 0;
                Vector2 p2 = vertices[i2];

                double D = VMath.Cross(p1, p2);
                double triangleArea = 0.5 * D;

                area += triangleArea;

                //use area to weight the centroid average, not just the vertex position
                c += VMath.MultVectDouble(p1 + p2, triangleArea * k_inv3); // triangleArea * k_inv3 * (p1 + p2);

                double intx2 = p1.x * p1.x + p2.x * p1.x + p2.x * p2.x;
                double inty2 = p1.y * p1.y + p2.y * p1.y + p2.y * p2.y;
                I += (0.25 * k_inv3 * D) * (intx2 + inty2);
            }
            c = VMath.MultVectDouble(c, 1.0 / area);

            //translate verticies to centroid (make centroid (0,0)
            //for the polygon in model space)
            //Not really necessary but I like doing this anyway
            for(int i = 0; i < vertexCount; i++)
            {
                vertices[i] -= c;
            }

            body.mass = density * (float)area;
            body.inertia = (float)I * density;
        }

        public override void SetOrient(float radians)
        {
            u.Set(radians);
        }

        public override void Draw()
        {
            DrawPolygon(body.pos, body.color);
            if (testTexture != null)
                body.room.camera.Draw(testTexture, body.pos + (trueOffset.Rotate(body.orient)) + (offset.Rotate(body.orient)), body.color, 1f, body.orient, Layers.Over1);
        }

        public void DrawPolygon(Vector2 position, Color color)
        {
            //Vector2[] vertIncrements = new Vector2[vertexCount];
            //for (int i = 0; i < vertexCount; i++)
            //{
            //    vertIncrements[i] = vertices[i];
            //    vertIncrements[i].Normalize();
            //    vertIncrements[i] *= -LineThickness;
            //
            //}

            //could optimize to use the last vertex on the next iteration
            for (int i = 0; i < vertexCount; i++)
            {
                Vector2 a1 = (u /** (RecurseCount)*/) * vertices[i];
                Vector2 a2 = (u /** (RecurseCount)*/) * vertices[(i + 1) % vertexCount];

                Vector2 v1 = position + a1;
                Vector2 v2 = position + a2;
                body.room.camera.DrawLine(v1, v2, LineThickness, color, Layers.Over2);

                if (RecurseDrawEnabled)
                {
                    DrawRecurse(body.pos + a1, RecurseCount, 1f);
                }

            }


            //DrawFill(body.pos, 1f);
        }

        public void DrawFill(Vector2 pos, float scale)
        {
            //could optimize to use the last vertex on the next iteration
            scale -= RecurseScaleReduction;
            if (scale < 0.3f) return;

            for (int i = 0; i < vertexCount; i++)
            {
                Vector2 a1 = u * vertices[i] * scale;
                Vector2 a2 = u * vertices[(i + 1) % vertexCount] * scale;

                Vector2 v1 = pos + a1;
                Vector2 v2 = pos + a2;
                body.room.camera.DrawLine(v1, v2, LineThickness, body.color, Layers.Under5);

                //Draw(pos, count, scale, scalediff);
            }
            DrawFill(pos, scale);
        }

        public void DrawRecurse(Vector2 pos, int count, float scale)
        {
            //could optimize to use the last vertex on the next iteration
            scale -= RecurseScaleReduction;
            count--;
            if (scale < 0 || count < 0) return;

            for (int i = 0; i < vertexCount; i++)
            {
                Vector2 a1 = (u * (i + 1)) * vertices[i] * scale;
                Vector2 a2 = (u * (i + 1)) * vertices[(i + 1) % vertexCount] * scale;

                Vector2 v1 = pos + a1;
                Vector2 v2 = pos + a2;
                body.room.camera.DrawLine(v1, v2, 1f, body.color, Layers.Under5);

                DrawRecurse(pos + a1, count, scale);
            }
        }

        public override ShapeType GetShapeType()
        {
            return ShapeType.Polygon;
        }

        //Highlight something and then use [Shift *] to put it in a comment block!---------------------------------

        public void SetCenterOfMass(Vector2[] verts)
        {
            int len = verts.Length;
            if (len < 3) return;

            Set(verts, len);

            Vector2 centroid = FindCentroid(vertices, vertexCount);

            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i] = new Vector2(vertices[i].x - centroid.x, vertices[i].y - centroid.y);
            }
            //body.pos = new Vector2(x, y);
            Set(vertices, vertexCount);

            CalibrateTexture();
            //Vector2 newCentroid = FindCentroid(vertices, vertexCount);
            body.pos = centroid;// +newCentroid;
            
        }

        public Vector2 FindCentroid(Vector2[] verts, int? length = null)
        {
            int len;
            if (length == null)
            {
                len = verts.Length;
            }
            else
            {
                len = (int)length;
            }

            float x = 0, y = 0, area = 0;
            for (int i = 0; i < len; i++)
            {
                Vector2 next = verts[(i + 1) % len];
                Vector2 current = verts[i];
                float factor = current.x * next.y - next.x * current.y;
                x += (current.x + next.x) * factor;
                y += (current.y + next.y) * factor;

                area += factor;
            }
            area /= 2;
            x /= 6 * area;
            y /= 6 * area;
            //if (x < 0 || y < 0) System.Diagnostics.Debugger.Break();

            return new Vector2(x, y);
        }

        // half width and half height
        public void SetBox(float hw, float hh, bool fill = true)
        {
            vertexCount = 4;
            VMath.Set(ref vertices[0], -hw, -hh);//vertices[0].Set(-hw, -hh);
            VMath.Set(ref vertices[1], hw, -hh);//vertices[1].Set(hw, -hh);
            VMath.Set(ref vertices[2], hw, hh);//vertices[2].Set(hw, hh);
            VMath.Set(ref vertices[3], -hw, hh);//vertices[3].Set(-hw, hh);
            VMath.Set(ref normals[0], 0, -1);//normals[0].Set(0, -1);
            VMath.Set(ref normals[1], 1, 0);//normals[1].Set(1, 0);
            VMath.Set(ref normals[2], 0, 1);//normals[2].Set(0, 1);
            VMath.Set(ref normals[3], -1, 0);//normals[3].Set(-1, 0);
            polyReach = Vector2.Distance(Vector2.zero, new Vector2(hw, hh)) * 2;
            if (fill)
            {
                testTexture = CreateClippedTexture(Assets.textureDict[body.texture].texture, vertices, vertexCount, out this.offset);
                this.trueOffset = this.offset * -1f;
            }
        }

        public void CalibrateTexture()
        {
            float minX = vertices.Min(x => x.x);
            float maxX = vertices.Max(x => x.x);
            float minY = vertices.Min(x => x.y);
            float maxY = vertices.Max(x => x.y);
            this.trueOffset = new Vector2((maxX - minX) / 2, (maxY - minY) / 2);
            this.testTexture = CreateClippedTexture(Assets.textureDict[body.texture].texture, vertices, vertexCount, out offset);
            this.offset = new Vector2(offset.x, offset.y);
        }

        public void Set(Vector2[] verts, int count)
        {
            //no hulls with less than 3 verticies (ensure actual polygon)
            //Debug.Assert(count > 2 && count < MaxPolyVertexCount);
            count = Math.Min(count, MaxPolyVertexCount);

            //find the right most point in the hull
            int rightMost = 0;
            double highestXCoord = verts[0].x;
            for (int i = 1; i < count; i++)
            {
                double x = verts[0].x;
                if (x > highestXCoord)
                {
                    highestXCoord = x;
                    rightMost = i;
                }
                //if matching x then take farthest negative y
                else if (x == highestXCoord && verts[i].y < verts[rightMost].y)
                {
                    rightMost = i;
                }
            }

            int[] hull = new int[MaxPolyVertexCount];
            int outCount = 0;
            int indexHull = rightMost;

            for (; ; )
            {
                hull[outCount] = indexHull;
                // search for next index that wraps around the hull
                // by computing cross products to find the most counter-clockwise
                // vertex in the set, given the previous hull index
                int nextHullIndex = 0;
                for (int i = 1; i < count; i++)
                {
                    //skip if same coordinate as we need three unique
                    //points in the set to perform a cross product
                    if (nextHullIndex == indexHull)
                    {
                        nextHullIndex = i;
                        continue;
                    }
                    // cross every set of three unquie verticies
                    // record each counter clockwise third vertex and add
                    // to the output hull
                    Vector2 e1 = verts[nextHullIndex] - verts[hull[outCount]];
                    Vector2 e2 = verts[i] - verts[hull[outCount]];
                    double c = VMath.Cross(e1, e2);
                    if (c < 0.0f)
                        nextHullIndex = i;

                    // Cross product is zero then e vectors are on same line
                    // therefor want to record vertex farthest along that line
                    if (c == 0.0f && e2.sqrMagnitude > e1.sqrMagnitude)
                        nextHullIndex = i;
                }
                outCount++;
                indexHull = nextHullIndex;
                //conclude algorithm upon wraparound
                if (nextHullIndex == rightMost)
                {
                    vertexCount = outCount;
                    break;
                }
            }
            float maxDist = 0;
            
            // Copy vertices into shape's vertices
            for (int i = 0; i < vertexCount; ++i)
            {
                vertices[i] = verts[hull[i]];
                float dist = Vector2.Distance(Vector2.zero, vertices[i]);
                if (dist > maxDist) maxDist = dist;
            }
            polyReach = maxDist * 2;

            ComputeNormals();
        }
        public Texture2D CreateClippedTexture(Texture2D tex, Vector2[] verts, int count, out Vector2 offset)
        {
            //Point offsetP = new Point();
            //Point[] points = new Point[count];
            //for(int i = 0; i < count; i++)
            //{
            //    points[i] = new Point((int)verts[i].x, (int)verts[i].y);
            //}
            //
            //Texture2D ret = Assets.ClippedBitmap(tex, points, out offsetP);
            //offset.x = offsetP.x; offset.y = offsetP.y;
            //return ret;
            offset = Vector2.zero; //todo: fix with unity
            return tex;
        }
        public Texture2D testTexture;
        private Vector2 offset;
        private Vector2 trueOffset;

        public void ComputeNormals()
        {
            // Compute face normals
            for (int i1 = 0; i1 < vertexCount; ++i1)
            {
                int i2 = i1 + 1 < vertexCount ? i1 + 1 : 0;
                Vector2 face = vertices[i2] - vertices[i1];

                // Ensure no zero-length edges, because that's bad
                System.Diagnostics.Debug.Assert(face.sqrMagnitude > GMath.EPSILON * GMath.EPSILON);

                // Calculate normal with 2D cross product between vector and scalar
                normals[i1] = new Vector2(face.y, -face.x);
                VMath.NormalizeSafe(ref normals[i1]);
            }
        }
        // The extreme point along a direction within a polygon
        public Vector2 GetSupport(Vector2 dir)
        {
            double bestProjection = -float.MaxValue; //-FLT_MAX;
            Vector2 bestVertex = new Vector2(0,0);

            for(int i = 0; i < vertexCount; ++i)
            {
                Vector2 v = vertices[i];
                double projection = Vector2.Dot( v, dir );

                if(projection > bestProjection)
                {
                    bestVertex = v;
                    bestProjection = projection;
                }
            }
            return bestVertex;
        }
    }



    public struct Mat22
    {
        //public Vector2 xCol;
        //public Vector2 yCol;
        public Vector2 Col1, Col2;

        /// <summary>
        /// Construct this matrix using columns.
        /// </summary>
        public Mat22(Vector2 c1, Vector2 c2)
        {
            Col1 = c1;
            Col2 = c2;
        }

        /// <summary>
        /// Construct this matrix using scalars.
        /// </summary>
        public Mat22(float a11, float a12, float a21, float a22)
        {
            Col1.x = a11; Col1.y = a21;
            Col2.x = a12; Col2.y = a22;
        }

        /// <summary>
        /// Construct this matrix using an angle. 
        /// This matrix becomes an orthonormal rotation matrix.
        /// </summary>
        public Mat22(float angle)
        {
            float c = (float)System.Math.Cos(angle), s = (float)System.Math.Sin(angle);
            Col1.x = c; Col2.x = -s;
            Col1.y = s; Col2.y = c;
        }

        /// <summary>
        /// Initialize this matrix using columns.
        /// </summary>
        public void Set(Vector2 c1, Vector2 c2)
        {
            Col1 = c1;
            Col2 = c2;
        }

        /// <summary>
        /// Initialize this matrix using an angle.
        /// This matrix becomes an orthonormal rotation matrix.
        /// </summary>
        public void Set(float angle)
        {
            float c = (float)System.Math.Cos(angle), s = (float)System.Math.Sin(angle);
            Col1.x = c; Col2.x = -s;
            Col1.y = s; Col2.y = c;
        }

        /// <summary>
        /// Set this to the identity matrix.
        /// </summary>
        public void SetIdentity()
        {
            Col1.x = 1.0f; Col2.x = 0.0f;
            Col1.y = 0.0f; Col2.y = 1.0f;
        }

        /// <summary>
        /// Set this matrix to all zeros.
        /// </summary>
        public void SetZero()
        {
            Col1.x = 0.0f; Col2.x = 0.0f;
            Col1.y = 0.0f; Col2.y = 0.0f;
        }

        /// <summary>
        /// Extract the angle from this matrix (assumed to be a rotation matrix).
        /// </summary>
        public float GetAngle()
        {
            return (float)System.Math.Atan2(Col1.y, Col1.x);
        }

        /// <summary>
        /// Compute the inverse of this matrix, such that inv(A) * A = identity.
        /// </summary>
        public Mat22 GetInverse()
        {
            float a = Col1.x, b = Col2.x, c = Col1.y, d = Col2.y;
            Mat22 B = new Mat22();
            float det = a * d - b * c;
            //Box2DXDebug.Assert(det != 0.0f);
            System.Diagnostics.Debug.Assert(det != 0.0f);
            det = 1.0f / det;
            B.Col1.x = det * d; B.Col2.x = -det * b;
            B.Col1.y = -det * c; B.Col2.y = det * a;
            return B;
        }

        /// <summary>
        /// Solve A * x = b, where b is a column vector. This is more efficient
        /// than computing the inverse in one-shot cases.
        /// </summary>
        public Vector2 Solve(Vector2 b)
        {
            float a11 = Col1.x, a12 = Col2.x, a21 = Col1.y, a22 = Col2.y;
            float det = a11 * a22 - a12 * a21;
            //Box2DXDebug.Assert(det != 0.0f);
            System.Diagnostics.Debug.Assert(det != 0.0f);
            det = 1.0f / det;
            Vector2 x = new Vector2();
            x.x = det * (a22 * b.x - a12 * b.y);
            x.y = det * (a11 * b.y - a21 * b.x);
            return x;
        }

        public static Mat22 Identity { get { return new Mat22(1, 0, 0, 1); } }

        public static Mat22 operator +(Mat22 A, Mat22 B)
        {
            Mat22 C = new Mat22();
            C.Set(A.Col1 + B.Col1, A.Col2 + B.Col2);
            return C;
        }
        // switched them and collision is working properly
        public static Vector2 operator *(Mat22 m, Vector2 v)
        {
            //return new Vector2(m.Col1.x * v.x + m.Col1.y * v.y, m.Col2.x * v.x + m.Col2.y * v.y);
            return new Vector2(m.Col1.x * v.x + m.Col2.x * v.y, m.Col1.y * v.x + m.Col2.y * v.y);
        }

        public static Mat22 operator *(Mat22 m, int i)
        {
            //return new Vector2(m.Col1.x * v.x + m.Col1.y * v.y, m.Col2.x * v.x + m.Col2.y * v.y);
            return new Mat22(m.GetAngle() * i);
        }

        public Mat22 Transpose()
        {
            return new Mat22(Col1.x, Col1.y, Col2.x, Col2.y);
        }

    }
}
