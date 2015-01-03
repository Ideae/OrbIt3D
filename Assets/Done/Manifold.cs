using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbItProcs
{
    public class Manifold
    {
        public Body a;
        public Body b;

        public double penetration = 0;                     // Depth of penetration from collision
        public Vector2 normal = new Vector2(0,0);                         // From A to B
        public Vector2[] contacts = new Vector2[2];    // Points of contact during collision
        public int contact_count = 0;                      // Number of contacts that occured during collision
        public double e;                               // Mixed restitution
        public double df;                              // Mixed dynamic friction
        public double sf;                              // Mixed static friction

        public Manifold(Body a, Body b)
        {
            this.a = a;
            this.b = b;
        }

        public bool Solve()
        {
            return Collision.Dispatch[(int)a.shape.GetShapeType(),(int)b.shape.GetShapeType()](this, a, b);
        }

        public void Initialize()
        {
            e = Math.Min(a.restitution, b.restitution);
            sf = Math.Sqrt(a.staticFriction * a.staticFriction);
            df = Math.Sqrt(a.dynamicFriction * a.dynamicFriction);

            for(int i = 0; i < contact_count; i++)
            {
                Vector2 ra = contacts[i] - a.pos;
                Vector2 rb = contacts[i] - b.pos;

                Vector2 crossprod_b = VMath.Cross(b.angularVelocity, rb);
                Vector2 crossprod_a = VMath.Cross(a.angularVelocity, ra);
                Vector2 rv = b.velocity + crossprod_b - a.velocity - crossprod_a;

                //Vector2 rv = b.velocity + VMath.Cross(b.angularVelocity, rb) -
                //             a.velocity - VMath.Cross(a.angularVelocity, ra);
                //if (b.velocity.IsFucked()) System.Diagnostics.Debugger.Break(); //removelater
                //if (a.velocity.IsFucked()) System.Diagnostics.Debugger.Break(); //removelater

                //if (rv.Length() < (dt,gravity).sqrMagnitude + EPSILON) e = 0.0f;

            }
        }
        public void ApplyImpulse()
        {
            if (GMath.Equal(a.invmass + b.invmass, 0))
            {
                InfinitMassCorrection();
                return;
            }

            for (int i = 0; i < contact_count; i++)
            {
                //calcuate radii from COM to contact
                Vector2 ra = contacts[i] - a.pos;
                Vector2 rb = contacts[i] - b.pos;
                //relative velocity
                Vector2 rv = b.velocity + VMath.Cross(b.angularVelocity, rb) -
                             a.velocity - VMath.Cross(a.angularVelocity, ra);
                //relative velocity along the normal
                double contactVel = Vector2.Dot(rv, normal);
                //do not resolve if velocities are seperating

                if (contactVel > 0)
                    return;

                double raCrossN = VMath.Cross(ra, normal);
                double rbCrossN = VMath.Cross(rb, normal);
                double invMassSum = a.invmass + b.invmass + (raCrossN * raCrossN) * a.invinertia + (rbCrossN * rbCrossN) * b.invinertia;
                //calculate impulse scalar
                double j = -(1.0 + e) * contactVel;
                j /= invMassSum;
                j /= (double)contact_count;
                //apply impulse
                Vector2 impulse = VMath.MultVectDouble(normal, j); // normal * j;
                a.ApplyImpulse(-impulse, ra);
                b.ApplyImpulse(impulse, rb);
                //friction impulse
                rv = b.velocity + VMath.Cross(b.angularVelocity, rb) -
                     a.velocity - VMath.Cross(a.angularVelocity, ra);
                Vector2 t = rv - (normal * Vector2.Dot(rv, normal));
                //t.Normalize();
                VMath.NormalizeSafe(ref t);
                //j tangent magnitude
                double jt = -Vector2.Dot(rv, t);
                jt /= invMassSum;
                jt /= (double)contact_count;
                //don't apply tiny friction impulses
                if (GMath.Equal(jt, 0.0))
                    return;
                //coulumbs law
                Vector2 tangentImpulse;
                if (Math.Abs(jt) < j * sf)
                    tangentImpulse = VMath.MultVectDouble(t, df); // t * df;
                else
                    tangentImpulse = VMath.MultVectDouble(t, -j * df); // t * -j * df
                //apply friction impulse
                a.ApplyImpulse(-tangentImpulse, ra);
                b.ApplyImpulse(tangentImpulse, rb);
            }
        }
        public void PositionalCorrection()
        {
            double k_slop = 0.05;
            double percent = 0.4;
            Vector2 correction = VMath.MultVectDouble(normal, Math.Max(penetration - k_slop, 0.0) / (a.invmass + b.invmass) * percent);
            a.pos -= VMath.MultVectDouble(correction, a.invmass);
            b.pos += VMath.MultVectDouble(correction, b.invmass);
        }
        void InfinitMassCorrection()
        {
            VMath.Set(ref a.velocity, 0, 0);//a.velocity.Set(0, 0);
            VMath.Set(ref b.velocity, 0, 0);//b.velocity.Set(0, 0);
        }
    }
}
