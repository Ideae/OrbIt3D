using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public interface ILinkable
    {
        Link link { get; set; }
        Node parent { get; set; }
        bool active { get; set; }
        //void AffectSelf();
        void AffectOther(Node other);
        void Draw();
    }
}
