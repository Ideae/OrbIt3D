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

        public bool isSolid { get { return parent != null && parent.body.isSolid; } set { if (parent != null) parent.body.isSolid = value; } }
        
        private bool _AllHandlersEnabled = true;
        [Info(UserLevel.Developer)]
        public bool AllHandlersEnabled
        {
            get { return _AllHandlersEnabled; }
            set
            {
                _AllHandlersEnabled = value;
            }
        }
        [Info(UserLevel.Developer)]
        public override bool active
        {
            get { return _active; }
            set
            {
                _active = value;
            }
        }


        public Collision() : this(null) { }
        public Collision(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            _AllHandlersEnabled = true;
        }

        public override void OnSpawn()
        {
        }
        public override void AffectSelf()
        {
        }

        public override void Draw()
        {
        }

    }
}
