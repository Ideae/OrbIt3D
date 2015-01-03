using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbItProcs
{
    public static class GMath
    {
        public static float EPSILON = 0.0001f;
        public const float PI = (float)Math.PI;
        public const float TwoPI = (float)(Math.PI * 2);
        public const float PIbyTwo = (float)(Math.PI / 2);
        public const float rootOfTwo = 1.41421356237f;
        public const float invRootOfTwo = 0.70710678118f;
        public static float between0and2pi(this float value)
        {
            //sawtooth?
            if (value > 2 * GMath.PI) value = value % (2 * GMath.PI);
            if (value < 0) value = 2 * GMath.PI + value;
            return value;
        }
        public static float Sawtooth(float x, float mod)
        {
            float ret = x % mod;
            if (x >= 0 || ret == 0)
            {
                return ret;
            }
            else
            {
                return mod - (float)Math.Abs(ret);
            }
        }
        public static float Triangle(float num, float mod)
        {
            //return (mod - (float)Math.Abs(((int)(num) % (2 * mod) - mod))) / (mod / 5) + 0.5f;

            float a = Math.Abs(num) % (2 * mod);
            float b = a - mod;
            float c = Math.Abs(b);
            float d = mod - c;
            return d;

            //x = m - abs(i % (2*m) - m)
            //return mod - Math.Abs(num % (2 * mod) - mod);
        }
        public static float AngleLerp(float source, float dest, float amount)
        {
            float result = 0f;
            float pi = PI;

            if (source < pi && dest > source + pi)
            {

                result = Mathf.Lerp(source, dest - (2 * pi), amount);
            }
            else if (source > pi && dest < source - pi)
            {
                result = Mathf.Lerp(source, dest + (2 * pi), amount);
            }
            else
            {
                result = Mathf.Lerp(source, dest, amount);
            }
            return result;
        }
        public static float Lerp(float start, float end, float amount)
        {
            if (amount > 1f) amount = 1f;
            else if (amount < 0f) amount = 0f;
            if (start > end)
            {
                float temp = start;
                start = end;
                end = start;
            }
            return start + (end - start) * amount;
        }
        public static bool Equal(double a, double b)
        {
            return Math.Abs(a - b) <= EPSILON;
        }
        public static bool BiasGreaterThan(double a, double b)
        {
            double k_biasRelative = 0.95;
            double k_biasAbsolute = 0.01;
            return a >= b * k_biasRelative + a * k_biasAbsolute;
        }
        public static float CircularDistance(float x, float v, int t)
        {
            int half = t / 2;
            if (x == v) return 0;
            if (x > v)
            {
                if (v > x - half)
                {
                    return v - x; //negative
                }
                else
                {
                    return t - x + v;
                }
            }
            else
            {
                if (v < x + half)
                {
                    return v - x;
                }
                else
                {
                    return v - t - x; //negative
                }
            }
        }
    }
    public static class VMath
    {
        
        #region /// Existing Methods ///
        public static void Test()
        {
            //Vector2.Add;
            //Vector2.Barycentric;
            //Vector2.CatmullRom;
            //Vector2.Clamp;
            //Vector2.Distance;
            //Vector2.DistanceSquared;
            //Vector2.Divide;
            //Vector2.Dot;
            //Vector2.Equals;
            //Vector2.Hermite;
            //Vector2.Lerp;
            //Vector2.Max;
            //Vector2.Min;
            //Vector2.Multiply;
            //Vector2.Negate;
            //Vector2.Normalize;
            //Vector2.One;
            //Vector2.Reflect;
            //Vector2.SmoothStep;
            //Vector2.Subtract;
            //Vector2.Transform;
            //Vector2.TransformNormal;
            //Vector2.UnitX;
            //Vector2.UnitY;
            //Vector2.Zero;
        }
        #endregion

        public static Vector3 SetX(this Vector3 vector3, float x)
        {
            return new Vector3(x, vector3.y, vector3.z);
        }
        public static Vector3 SetY(this Vector3 vector3, float y)
        {
            return new Vector3(vector3.x, y, vector3.z);
        }
        public static Vector3 SetZ(this Vector3 vector3, float z)
        {
            return new Vector3(vector3.x, vector3.y, z);
        }
        public static void Set(ref Vector2 v, float x, float y)
        {
            v.x = x; v.y = y;
        }
        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }

        public static float VectorToAngle(Vector2 vector)
        {
            float value = (float)Math.Atan2(vector.x, -vector.y);//should the components be swapped here?
            if (value > GMath.TwoPI)//should this be a sawtooth?
                value = value % GMath.TwoPI;
            if (value < 0)
                value = GMath.TwoPI + value;
            return value;
        }
        public static Vector2 VectorRotateLerp(Vector2 source, Vector2 direction, float amount)
        {
            float oldAngle = VMath.VectorToAngle(source);
            float newAngle = VMath.VectorToAngle(direction);
            float lerpedAngle = GMath.AngleLerp(oldAngle, newAngle, amount);
            //Vector2 finalDir = VMath.AngleToVector(lerpedAngle);
            return VMath.Redirect(source, VMath.AngleToVector(lerpedAngle));
        }

        public static bool isWithin(this Vector2 v, Rect r)
        {
            return v.x >= r.xMin && v.x <= r.xMax && v.y >= r.yMin && v.y <= r.yMax;
        }
        public static bool isWithin(this Vector2 v, Vector2 TopLeft, Vector2 BottomRight)
        {
            return (v.x >= TopLeft.x && v.y >= TopLeft.y && v.x <= BottomRight.x && v.y <= BottomRight.y);
        }
        public static Vector2 Rotate(this Vector2 v, float radians)
        {
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);
            double xp = v.x * c - v.y * s;
            double yp = v.x * s + v.y * c;
            v.x = (float)xp;
            v.y = (float)yp;
            return v;
        }
        public static Vector2 ProjectOnto(this Vector2 source, Vector2 target)
        {
            return (Vector2.Dot(source, target) / target.sqrMagnitude) * target;
        }

        public static Vector2 Cross(Vector2 v, double a)
        {
            return new Vector2((float)a * v.y, -(float)a * v.x);
        }
        public static Vector2 Cross(double a, Vector2 v)
        {
            return new Vector2(-(float)a * v.y, (float)a * v.x);
        }
        public static double Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
        public static Vector2 MultVectDouble(Vector2 v, double d)
        {
            return new Vector2(v.x * (float)d, v.y * (float)d);
        }
        //todo: test resize and redirect
        public static Vector2 Resize(Vector2 v, float length)
        {
            return v *= length / v.magnitude; 
        }
        public static Vector2 Redirect(Vector2 source, Vector2 direction)
        {
            return direction *= source.magnitude / direction.magnitude;
            //return Resize(direction, source.Length());
        }

        public static Vector2 NormalizeSafe(this Vector2 v)
        {
            if (v.x == 0 && v.y == 0) return v;
            float len = v.magnitude;
            if (len == 0) return v;
            float invLen = 1.0f / len;
            v.x *= invLen;
            v.y *= invLen;
            return v;
        }

        public static void NormalizeSafe(ref Vector2 v)
        {
            if (v.x != 0 || v.y != 0)
            {
                float len = v.magnitude;
                if (len == 0) return;
                float invLen = 1.0f / len;
                v.x *= invLen;
                v.y *= invLen;
            }
        }
        //public static Vector2 ToVector2(this Point point)
        //{
        //    return new Vector2(point.x, point.y);
        //}
    }
}
