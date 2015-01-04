using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbItProcs
{
    /// <summary>
    /// Constantly changes the color of nodes depending on a variety of modes
    /// </summary>
    [Info(UserLevel.User, "Constantly changes the color of nodes depending on a variety of modes", CompType)]
    public class ColorChanger : Component
    {
        public const mtypes CompType = mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        public enum ColorMode
        {
            none,
            angle,
            position,
            velocity,
            scale,
            hueShifter,
            randomInitial,
            phaseBlack,
            phaseWhite,
            phaseAround,
            phaseHue,
            phaseAlpha,
            phaseAlphaOnly,
        }

        /// <summary>
        /// Determines how to change to the node's color:
        /// Angle: Hue changes acording to angle of travel. 
        /// Position: Hue changes according to position. 
        /// Velocity: Node darkens as it slows. 
        /// Scale: Hue Changes according to its Scale. 
        /// HueShifter: Hue constantly shifts;
        /// </summary>
        [Info(UserLevel.User, "Determines how to change to the node's color: \nAngle: Hue changes acording to angle of travel. \nPosition: Hue changes according to position. \nVelocity: Nodes darken as they slow. \nScale: Hue Changes according to their Scale. \nHueShifter: Hue constantly shifts;")]
        public ColorMode colormode { get; set; }

        private int _msInterval;
        /// <summary>
        /// If enabled, the color changes only this many milliseconds, otherwise, it changes every frame.
        /// </summary>
        [Info(UserLevel.User, "If enabled, the color changes only this many milliseconds, otherwise, it changes every frame.")]
        public int msInterval {
            get { return _msInterval; } 
            set 
            {
                _msInterval = value;
                if (appt != null) appt.interval = value;
            }
        }
        private bool _useMsInterval;
        public bool useMsInterval
        {
            get { return _useMsInterval; }
            set
            {
                if (appt != null)
                {
                    appt.interval = _msInterval;
                    if (!_useMsInterval && value) 
                        parent.scheduler.AddAppointment(appt);
                    if (_useMsInterval && !value) 
                        parent.scheduler.RemoveAppointment(appt);
                }
                _useMsInterval = value;
            }
        }
        /// <summary>
        /// Changes the degree by which the node shifts hue. Used by HueShifter ColorMode
        /// </summary>
        [Info(UserLevel.User, "Changes the degree by which the node shifts hue. Used by HueShifter ColorMode")]
        public float inc { get; set; }

        private float _value = 1f;
        /// <summary>
        /// The Brightness of the colors
        /// </summary>
        [Info(UserLevel.Advanced, "The Brightness of the colors")]
        public float value { get { return _value; } set { _value = value; } }

        private float _saturation = 1f;
        /// <summary>
        /// The intensity of the colors
        /// </summary>
        [Info(UserLevel.Advanced, "The intensity of the colors")]
        public float saturation { get { return _saturation; } set { _saturation = value; } }

        //private int pos = 1, sign = 1;
        private float hue = 0;
        private float permaHue = 0;
        private bool schedulerModerated;
        private Appointment appt;
        //public bool smartshifting { get; set; }
        private float currentLerpPos = 0f;
        private int lerpSign = 1;
        /// <summary>
        /// The speed at which to phase to color under the 'phase' modes.
        /// </summary>
        [Info(UserLevel.User, "The speed at which to phase to color under the 'phase' modes.")]
        public float phaseSpeed { get; set; }
        /// <summary>
        /// The percentage that the color will phase towards it's color destination.
        /// </summary>
        [Info(UserLevel.User, "The percentage that the color will phase towards it's color destination.")]
        public float phasePercent { get; set; }

        public ColorChanger() : this(null) { }
        public ColorChanger(Node parent = null)
        {
            //smartshifting = true;
            if (parent != null) this.parent = parent;
            colormode = ColorMode.hueShifter;
            inc = 1;
            phaseSpeed = 10;
            phasePercent = 100;
            schedulerModerated = false;
            msInterval = 20;
            useMsInterval = false;
            appt = new Appointment(managedUpdate, msInterval, infinite: true);
        }

        public override void OnSpawn()
        {
            if (!active) return;
            if (colormode == ColorMode.randomInitial)
            {
                //parent.body.color = Utils.IntToColor(Utils.CurrentMilliseconds());
                float num = (float)Utils.random.Next(100000) / (float)100000;
                int n = (int)(num * 16000000);
                parent.renderer.material.color = Utils.IntToColor(n);
                parent.permaColor = parent.renderer.material.color;

            }
            hue = Utils.HueFromColor(parent.permaColor);
            permaHue = hue;
        }
        private void managedUpdate(Node n)
        {
            schedulerModerated = true;
            AffectSelf();
            schedulerModerated = false;
        }
        public override void AffectSelf()
        {
            if (useMsInterval && !schedulerModerated) return;

            bool black, white;
            if (colormode == ColorMode.angle)
            {
                float angle = (float)((Math.Atan2(parent.effvelocity.y, parent.effvelocity.x) + Math.PI) * (180 / Math.PI));
                parent.material.color = getColorFromHSV(angle, saturation, value);
            }
            else if (colormode == ColorMode.position)
            {
                float r = parent.transform.position.x / (float)room.worldSize.x;
                float g = parent.transform.position.y / (float)room.worldSize.y;
                float b = (parent.transform.position.x / parent.transform.position.y) / ((float)room.worldSize.x / (float)room.worldSize.y);
                parent.material.color = new Color(r, g, b);
            }
            else if (colormode == ColorMode.velocity)
            {
                float len = Vector2.Distance(parent.rigidbody.velocity, Vector2.zero) / 25;
                parent.material.color = new Color((parent.permaColor.r.ToXNAColor() / 255f) * len, (parent.permaColor.g.ToXNAColor() / 255f) * len, (parent.permaColor.b.ToXNAColor() / 255f) * len);
                //parent.body.color = getColorFromHSV((float)Math.Min(1.0, len / 20) * 360f, (float)Math.Min(1.0, len / 20), (float)Math.Min(1.0, len / 20));
            }
            else if (colormode == ColorMode.hueShifter)
            {
                float range = 20;
                float tempinc = inc;
                if (hue % 120 > 60 - range && hue % 120 < 60 + range)
                {
                    tempinc /= 2f;
                }
                parent.material.color = getColorFromHSV(hue, saturation, value);
                hue = (hue + tempinc) % 360;
            }
            else if ((black = colormode == ColorMode.phaseBlack) || (white = colormode == ColorMode.phaseWhite))
            {
                Color c = parent.permaColor;
                Vector3 perma = new Vector3(c.r.ToXNAColor(), c.g.ToXNAColor(), c.b.ToXNAColor());
                Vector3 dest = Vector3.zero;
                if (black)
                {
                    dest = Vector3.zero;
                }
                else
                {
                    dest = Vector3.one;
                }
                dest = Vector3.Lerp(perma, dest, phasePercent / 100f);
                currentLerpPos += (phaseSpeed / 1000f) * lerpSign;
                if (currentLerpPos > 1)
                {
                    currentLerpPos = 1f;
                    lerpSign *= -1;
                }
                else if (currentLerpPos < 0)
                {
                    currentLerpPos = 0;
                    lerpSign *= -1;
                }
                parent.material.color = Vector3.Lerp(perma, dest, currentLerpPos).ToColor(parent.material.color.a);
            }
            else if (colormode == ColorMode.phaseAround)
            {
                Color c = parent.permaColor;
                Vector3 perma = new Vector3(c.r.ToXNAColor(), c.g.ToXNAColor(), c.b.ToXNAColor());
                Vector3 down = Vector3.zero;
                Vector3 up = Vector3.one;
                down = Vector3.Lerp(perma, down, phasePercent / 100f);
                up = Vector3.Lerp(perma, up, phasePercent / 100f);
                currentLerpPos += (phaseSpeed / 1000f) * lerpSign;
                if (currentLerpPos > 1)
                {
                    currentLerpPos = 1f;
                    lerpSign *= -1;
                }
                else if (currentLerpPos < 0)
                {
                    currentLerpPos = 0;
                    lerpSign *= -1;
                }
                parent.material.color = Vector3.Lerp(down, up, currentLerpPos).ToColor(parent.material.color.a);
            }
            else if (colormode == ColorMode.phaseHue)
            {
                huedest = permaHue - (phasePercent / 100f * 90 * huesign);
                huedest = GMath.Sawtooth(huedest, 360);
                float range = 20;
                float tempinc = inc / 10f;
                if (hue % 120 > 60 - range && hue % 120 < 60 + range)
                {
                    tempinc /= 2f;
                }
                float dist = GMath.CircularDistance(hue, huedest, 360);
                tempinc *= Math.Sign(dist);
                hue = GMath.Sawtooth(hue + tempinc, 360);
                if (Math.Abs(hue - huedest) < Math.Abs(tempinc))
                {
                    huesign *= -1;
                }
                //Console.WriteLine(dist + "  " + huesign);
                parent.material.color = getColorFromHSV(hue, saturation, value);
            }
        }

        private float huedest = 0;
        private float huesign = 1;

        public static void ShiftFloat(ref float f, float min, float max, float rate)
        {
            f += rate;
            if (f < min)
            {
                f = min;
                rate *= -1;
            }
            else if (f > max)
            {
                f = max;
                rate *= -1;
            }
        }
        public static Color randomColorHue()
        {
            return getColorFromHSV(Utils.random.Next(360));
        }
        public static Color getColorFromHSV(float hue, float saturation = 1f, float value = 1f, int alpha = 255)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new Color(v, t, p, alpha);
            else if (hi == 1)
                return new Color(q, v, p, alpha);
            else if (hi == 2)
                return new Color(p, v, t, alpha);
            else if (hi == 3)
                return new Color(p, q, v, alpha);
            else if (hi == 4)
                return new Color(t, p, v, alpha);
            else
                return new Color(v, p, q, alpha);
        }
        

    }
}
