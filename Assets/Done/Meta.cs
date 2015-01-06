using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbItProcs
{
    public enum AIMode
    {
        Agro,
        Friendly,
        Neutral,
        Player,
        None,
    }
    [Flags]
    public enum ItemSlots
    {
        None = 0,
        A_Green = 1,
        B_Red = 2,
        X_Blue = 4,
        Y_Yellow = 8,
        All = 15,
    }
    /// <summary>
    /// The Meta component hold information about the node such as health, shields, and player/ai modes.
    /// </summary>
    [Info(UserLevel.User, "The Meta component hold information about the node such as health, shields, and player/ai modes.", CompType)]
    public class Meta : OComponent
    {
        public enum DamageMode
        {
            OnlyPlayers,
            OnlyNonPlayers,
            Everything,
            Nothing,
        }
        public enum HealthBarMode
        {
            Fade,
            Bar,
            none,
        }
        public const mtypes CompType = mtypes.essential | mtypes.draw | mtypes.playercontrol;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// The score of the player. If enabled and the player reaches the max score, the game ends.
        /// </summary>
        [Info(UserLevel.User, "The score of the player. If enabled and the player reaches the max score, the game ends.")]
        public Toggle<float> score { get; set; }
        /// <summary>
        /// The maximum health the node. When it reaches 0, the node dies.
        /// </summary>
        [Info(UserLevel.User, "The maximum health the node. When it reaches 0, the node dies.")]
        public Toggle<float> maxHealth { get; set; }
        /// <summary>
        /// The current health of the node.
        /// </summary>
        [Info(UserLevel.User, "The current health of the node.")]
        public float currentHealth { get; set; }
        /// <summary>
        /// The worth of the node represents how likely it is that the node will drop valuable loot upon death.
        /// </summary>
        [Info(UserLevel.User, "The worth of the node represents how likely it is that the node will drop valuable loot upon death.")]
        public Toggle<float> worth { get; set; }
        /// <summary>
        /// Represents the amount of damage mitigation upon losing health.
        /// </summary>
        [Info(UserLevel.User, "Represents the amount of damage mitigation upon losing health.")]
        public Toggle<float> armour { get; set; }
        /// <summary>
        /// The damageMode describes who this node can hurt.
        /// </summary>
        [Info(UserLevel.User, "The damageMode describes who this node can hurt.")]
        public DamageMode damageMode { get; set; }
        /// <summary>
        /// Select how to indicate health
        /// </summary>
        [Info(UserLevel.User, "Select how to indicate health")]
        public HealthBarMode healthBar { get; set; }
        //[Info(UserLevel.Developer)]
        //public ItemSlots itemSlots { get; set; }
        /// <summary>
        /// If enabled, the health bar will rotate.
        /// </summary>
        [Info(UserLevel.Advanced, "If enabled, the health bar will rotate.")]
        public bool rotateHealthBar { get; set; }
        /// <summary>
        /// If this node is controlled by an AI, this will determine it's behaviour.
        /// </summary>
        [Info(UserLevel.User, "If this node is controlled by an AI, this will determine it's behaviour.")]
        public AIMode AImode { get; set; }
        /// <summary>
        /// If the node is deadly, this node will kill other nodes on contact.
        /// </summary>
        [Info(UserLevel.User, "If the node is deadly, this node will kill other nodes on contact.")]
        public bool deadly { get; set; }
        /// <summary>
        /// This affects the amount of damage this node will do when attacking other nodes.
        /// </summary>
        [Info(UserLevel.User, "This affects the amount of damage this node will do when attacking other nodes.")]
        public float damageMultiplier { get; set; }
        /// <summary>
        /// If enabled, the node will ignore the grid system and iterate over every node in the room.
        /// </summary>
        [Info(UserLevel.User, "If enabled, the node will ignore the grid system and iterate over every node in the room.")]
        public bool IgnoreAffectGrid { get; set; }

        /// <summary>
        /// The radius of the node.
        /// </summary>
        [Info(UserLevel.User, "The radius of the node.")]
        public float radius { get { return parent != null ? parent.radius : 25; } set { if (parent != null) parent.radius = value; } }
        private float lightRotation = 0f;
        public Action<Node, Node> OnDeath { get; set; }
        public Action<Node, Node, int> OnTakeDamage { get; set; }
        public bool itemSwitching { get; set; }
        public Meta() : this(null) { }
        public Meta(Node parent)
        {
            this.parent = parent;
            score = new Toggle<float>(0, false);
            maxHealth = new Toggle<float>(100, true);
            currentHealth = maxHealth.value;
            worth = new Toggle<float>(1, false);
            armour = new Toggle<float>(1, false);
            damageMode = DamageMode.OnlyPlayers;
            AImode = AIMode.None;
            deadly = false;
            active = true;
            IgnoreAffectGrid = false;
            damageMultiplier = 1f;
            itemSwitching = true;
        }
        public override void PlayerControl(OInput input)
        {
            if (itemSwitching)
            {
                if (input.BtnClicked(InputButtons.A)) parent.player.currentItem = ItemSlots.A_Green;
                if (input.BtnClicked(InputButtons.B)) parent.player.currentItem = ItemSlots.B_Red;
                if (input.BtnClicked(InputButtons.X)) parent.player.currentItem = ItemSlots.X_Blue;
                if (input.BtnClicked(InputButtons.Y)) parent.player.currentItem = ItemSlots.Y_Yellow;
            }
        }
        public void CalculateDamage(Node other, float damage)
        {
            if (maxHealth.enabled)
            {
                float resultingDamage = OrbIt.globalGameMode.DetermineDamage(other, parent, damage) * damageMultiplier;
                currentHealth = (float)Math.Max(currentHealth - resultingDamage, 0);
                currentHealth = (float)Math.Min(currentHealth, maxHealth.value);
                float percent = 0.65f;
                if (healthBar == HealthBarMode.Fade) parent.material.color = parent.permaColor * ((currentHealth / maxHealth.value) * percent + (1f - percent));
                if (currentHealth <= 0)
                {
                    //Die(other);
                    parent.OnDeath(other);
                }
            }
        }

        public override void SetupSprites()
        {
            //if (parent.IsPlayer)
            //{
            //    draw.addSprite("Pointer").SetLayer(Layers.Over4).SetTexture(textures.pointer);
            //    draw.addSprite("A").SetColor(Color.green).SetLayer(Layers.Under2).SetScale(1.7f);
            //    draw.addSprite("B").SetColor(Color.red).SetLayer(Layers.Under2).SetScale(1.7f);
            //    draw.addSprite("X").SetColor(Color.blue).SetLayer(Layers.Under2).SetScale(1.7f);
            //    draw.addSprite("Y").SetColor(Color.yellow).SetLayer(Layers.Under2).SetScale(1.7f);
            //}
        }

        public override void Draw()
        {
            //if (healthBar == HealthBarMode.Bar) {
            //    float healthRatio = (float)currentHealth / (float)maxHealth;
            //    drawBar(parent, 1, healthRatio, rotateHealthBar, Color.cyan, Color.green, Color.yellow, Color.red);
            //
            //}
            //if (parent.IsPlayer)
            //{
            //    Color Q;
            //    switch (parent.player.currentItem)
            //    {
            //        case ItemSlots.A_Green: Q = Color.green; break;
            //        case ItemSlots.B_Red: Q = Color.red; break;
            //        case ItemSlots.X_Blue: Q = Color.blue; break;
            //        case ItemSlots.Y_Yellow: Q = Color.yellow; break;
            //        default: Q = Color.black; break;
            //    }
            //    //room.camera.Draw(textures.pointer, parent.transform.position, Q, parent.body.scale, parent.body.orient, Layers.Over4);
            //    draw.getSprite("Pointer").SetColor(Q);
            //    ItemSlots itemSlots = parent.player.occupiedSlots;
            //    textures A, B, X, Y;
            //    A = (itemSlots & ItemSlots.A_Green) == ItemSlots.A_Green ? textures.itemLight : textures.itemWhisper;
            //    B = (itemSlots & ItemSlots.B_Red) == ItemSlots.B_Red ? textures.itemLight : textures.itemWhisper;
            //    X = (itemSlots & ItemSlots.X_Blue) == ItemSlots.X_Blue ? textures.itemLight : textures.itemWhisper;
            //    Y = (itemSlots & ItemSlots.Y_Yellow) == ItemSlots.Y_Yellow ? textures.itemLight : textures.itemWhisper;
            //
            //    //room.camera.Draw(A, parent.transform.position, Color.green, parent.body.scale * 1.7f, lightRotation, Layers.Under2);
            //    //room.camera.Draw(B, parent.transform.position, Color.red, parent.body.scale * 1.7f, lightRotation + GMath.PIbyTwo, Layers.Under2);
            //    //room.camera.Draw(X, parent.transform.position, Color.blue, parent.body.scale * 1.7f, lightRotation + GMath.PI, Layers.Under2);
            //    //room.camera.Draw(Y, parent.transform.position, Color.yellow, parent.body.scale * 1.7f, lightRotation + GMath.PI + GMath.PIbyTwo, Layers.Under2);
            //    draw.getSprite("A").SetTexture(A).SetRotation(lightRotation);
            //    draw.getSprite("B").SetTexture(B).SetRotation(lightRotation + GMath.PIbyTwo);
            //    draw.getSprite("X").SetTexture(X).SetRotation(lightRotation + GMath.PI);
            //    draw.getSprite("Y").SetTexture(Y).SetRotation(lightRotation + GMath.PI + GMath.PIbyTwo);
            //    
            //    lightRotation += 0.1f;
            //}

        }

        public static void drawBar(Node node, float scale, float Ratio, bool Rotate, Color full, Color? threeQuarters = null, Color? half = null, Color? oneQuarter = null)
        {
            //float baseRotation = Rotate ? node.transform.localEulerAngles.x : 0f;
            //float rotation = baseRotation + (1f - Ratio) * GMath.TwoPI;
            //float rotation2 = baseRotation;
            //Color c;
            //Layers hideLayer = Layers.Over1;
            //if (Ratio > 0.75f) 
            //    c = full;
            //else if (Ratio > 0.5f) 
            //    c = threeQuarters ?? full;
            //else if (Ratio > 0.25f) 
            //{
            //    c = half ?? threeQuarters ?? full;
            //    hideLayer = Layers.Over3;
            //    rotation2 = rotation - GMath.PI; 
            //}
            //else 
            //{
            //    c = oneQuarter ?? half ?? threeQuarters ?? full;
            //    hideLayer = Layers.Over3;
            //    rotation2 = rotation - GMath.PI;
            //}
            ////float tempscale = node.body.scale;
            //float tempscale = 1f;
            //node.room.camera.Draw(textures.outerL, node.transform.position, Color.black, tempscale * scale, baseRotation, hideLayer);
            //node.room.camera.Draw(textures.outerR, node.transform.position, Color.black, tempscale * scale, baseRotation, Layers.Over1);
            //node.room.camera.Draw(textures.innerL, node.transform.position, c, tempscale * scale, rotation, Layers.Over2);
            //node.room.camera.Draw(textures.innerR, node.transform.position, c, tempscale * scale, rotation2, Layers.Over2);
        }
        public override void OnRemove(Node other)
        {
            if (OnDeath != null) OnDeath(parent, other);
        }


    }
}

