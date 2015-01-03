﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using UnityEngine;
namespace OrbItProcs
{
    /// <summary>
    /// Basic Draw Component, ensures that you can see the node.
    /// </summary>
    [Info(UserLevel.User, "Basic Draw Component, ensures that you can see the node.", CompType)]
    public class BasicDraw : Component
    {
        public enum Initial
        {
            Deviant,
            Random,
            Managed,
            CloseToPermanent,
        }
        public const mtypes CompType = mtypes.essential | mtypes.draw;
        public override mtypes compType { get { return CompType; } set { } }

        private Initial _InitialColor = Initial.Managed;
        /// <summary>
        /// Determines whether the color will be random or set by the Red, Green and Blue properties initially.
        /// </summary>
        [Info(UserLevel.User, "Determines whether the color will be random or set by the Red, Green and Blue properties initially.")]
        public Initial InitialColor { get { return _InitialColor; } set { _InitialColor = value; Colorize(); } }
        /// <summary>
        /// Red color component
        /// </summary>
        [Info(UserLevel.User, "Red color component")]
        public float Red { get; set; }
        /// <summary>
        /// Green color component
        /// </summary>
        [Info(UserLevel.User, "Green color component")]
        public float Green { get; set; }
        /// <summary>
        /// Blue color component
        /// </summary>
        [Info(UserLevel.User, "Blue color component")]
        public float Blue { get; set; }
        /// <summary>
        /// Alpha color component
        /// </summary>
        [Info(UserLevel.User, "Alpha color component")]
        public float AlphaPercent { get; set; }
        /// <summary>
        /// The layer that the node will draw on.
        /// </summary>
        [Info(UserLevel.User, "The layer that the node will draw on.")]
        public Layers DrawLayer { get; set; }
        public BasicDraw() : this(null) { }
        public BasicDraw(Node parent = null) 
        {
            if (parent != null) this.parent = parent;
            UpdateColor();
            AlphaPercent = 100f;
            //DrawLayer = Layers.Under1;
            DrawSparkles = true;
            threshold = 20;
        }
        public override void SetupSprites()
        {
            Layers layer = parent.IsPlayer ? Layers.Player : DrawLayer;
            draw.addSprite("sprite", parent.body.texture, layer).SetColor(parent.body.color);
        }
        public void UpdateColor()
        {
            if (parent == null) return;
            Color c = parent.body.color;
            Red = c.r;
            Green = c.g;
            Blue = c.b;
            
            if (draw != null)
            {
                var sprite = draw.getSprite("sprite");
                if (sprite != null)
                {
                    sprite.SetColor(c);
                }
            }
        }

        public override void OnSpawn()
        {
            Colorize();
        }

        public void Colorize()
        {
            if (InitialColor == Initial.Random)
            {
                RandomizeColor();
            }
            else if (InitialColor == Initial.Managed)
            {
                SetColor();
            }
            else if (InitialColor == Initial.Deviant)
            {
                Deviate();
            }
            else if (InitialColor == Initial.CloseToPermanent)
            {
                CloseToPermanent();
            }
        }
        public void SetColor()
        {
            if (parent != null)
            {
                parent.body.color = new Color(Red, Green, Blue);
                parent.body.permaColor = parent.body.color;
            }
        }
        [Clickable]
        public void RandomizeColor()
        {
            if (parent != null)
            {
                parent.body.color = Utils.randomColor();
                parent.body.permaColor = parent.body.color;
            }
        }
        public void Deviate()
        {
            if (parent != null)
            {
                if (OrbIt.globalGameMode != null)
                {
                    parent.body.color = Color.Lerp(OrbIt.globalGameMode.globalColor, Utils.randomColor(), 0.1f);
                    parent.body.permaColor = parent.body.color;
                    OrbIt.globalGameMode.globalColor = parent.body.color;
                }
            }
            else SetColor();
        }
        public int threshold { get; set; }
        public void CloseToPermanent()
        {
            if (parent != null)
            {
                SetColor();
                Color c = parent.body.permaColor;
                int r = Utils.random.Next(threshold) - (threshold / 2);
                int g = Utils.random.Next(threshold) - (threshold / 2);
                int b = Utils.random.Next(threshold) - (threshold / 2);
                parent.body.color = new Color(c.r + r, c.g + g, c.b + b);
                parent.body.permaColor = parent.body.color;
                UpdateColor();
            }
        }
        public bool DrawSparkles { get; set; }
        public override void Draw()
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            //if (parent.body.shape is Polygon)
            //{
            //    parent.body.shape.Draw();
            //    if (!parent.body.DrawPolygonCenter) return;
            //}

            
            //room.camera.Draw(parent.body.texture, parent.body.pos, parent.body.color * (AlphaPercent / 100f), parent.body.scale, parent.body.orient, layer);
            draw.getSprite("sprite").SetTexture(parent.body.texture).SetPosition(parent.body.pos).SetColor(parent.body.color * (AlphaPercent / 100f)).SetRotation(parent.body.orient);

            //if(parent.body.texture == textures.boulder1 && DrawSparkles) room.camera.Draw(textures.boulderShine, parent.body.pos, Utils.randomColor(), parent.body.scale, parent.body.orient, layer);
        }
    }
}