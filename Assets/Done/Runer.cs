using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbItProcs
{
    /// <summary>
    /// Draws a rune image after the node's basic draw.
    /// </summary>
    [Info(UserLevel.User, "Draws a rune image after the node's basic draw.", CompType)]
    public class Rune : OComponent
    {
        public const mtypes CompType = mtypes.draw;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// The rune texture to draw.
        /// </summary>
        [Info(UserLevel.User, "The rune texture to draw.")]
        public textures runeTexture { get; set; }

        /// <summary>
        /// Toggles whether runes are randomly generated upon spawning.
        /// </summary>
        [Info(UserLevel.User, "Toggles whether runes are randomly generated upon spawning.")]
        public bool randomRune { get; set; }

        public Rune() : this(null) { }
        public Rune(Node parent)
        {
            this.parent = parent;
            randomRune = false; 
            runeTexture = textures.blackorb;
        }

        public override void OnSpawn()
        {
            if (!randomRune) return;
            int r = Utils.random.Next(16);
            runeTexture = (textures)r;
        }
        public override void Draw()
        {
            //Color col = parent.material.color.ContrastColor();
            //float scale = 1f;
            //room.camera.Draw(runeTexture, parent.transform.position, col, scale, parent.body.orient, Layers.Over1);
        }
    }
}
