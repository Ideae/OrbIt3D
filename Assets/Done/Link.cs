using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine; 

namespace OrbItProcs
{
    public enum linktype
    {
        none,
        NodeToNode,
        NodeToGroup,
        GroupToGroup,

    }
    public enum updatetime
    {
        SourceUpdate,
        RoomUpdate,
    }
    public class Link
    {
        private bool _active = true;
        public bool active { get { return _active; } 
            set 
            {
                if (value)
                {
                    if (formation != null)
                    {
                        formation.UpdateFormation();
                    }
                    if (room != null)
                    {
                        room.AllInactiveLinks.Remove(this);
                        room.AllActiveLinks.Add(this);
                    }
                }
                else
                {
                    if (room != null)
                    {
                        room.AllActiveLinks.Remove(this);
                        room.AllInactiveLinks.Add(this);
                    }
                }
                _active = value; 
            } 
        }
        public Room room;
        //public ObservableHashSet<ILinkable> components { get; set; }
        public Dictionary<Type, ILinkable> components { get; set; }

        public linktype ltype { get; set; }
        public updatetime _UpdateTime = updatetime.SourceUpdate;
        public updatetime UpdateTime { get { return _UpdateTime; } set { _UpdateTime = value; } }
        public Formation formation { get; set; }
        private formationtype _FormationType;
        public formationtype FormationType { get { return _FormationType; }
            set {
                if (_FormationType != value && formation != null)
                {
                    _FormationType = value;
                    formation.UpdateFormation();
                }
                else
                {
                    _FormationType = value; 
                }
                
            }
        }
        public bool DrawLinkLines { get; set; }
        public bool _Reversed = false;
        public bool Reversed { get { return _Reversed; } set { _Reversed = value; } }
        public bool _DrawTips = false;
        public bool DrawTips { get { return _DrawTips; } set { _DrawTips = value; } }
        public float _AngleInc = 0.02f;
        public float AngleInc { get { return _AngleInc; } set { _AngleInc = value; } }
        public Node sourceNode { get; set; }
        public Node targetNode { get; set; }
        public ObservableHashSet<Node> sources { get; set; }
        public ObservableHashSet<Node> targets { get; set; }
        public ObservableHashSet<Node> exclusions { get; set; }
        public Group sourceGroup { get; set; }
        public Group targetGroup { get; set; }
        public bool _IsEntangled = false;
        public bool IsEntangled
        {
            get { return _IsEntangled; }
            set
            {
                _IsEntangled = value;
            }
        }
        private float anglestep = 0;
        public Link()
        {
            //..
            this.room = OrbIt.game.room;
            this.components = new Dictionary<Type, ILinkable>();
            this.sources = new ObservableHashSet<Node>();
            this.targets = new ObservableHashSet<Node>();
        }

        //blank link (for the palette)
        public Link(ILinkable linkComponent, formationtype ftype = formationtype.AllToAll)
        {
            this.room = OrbIt.game.room;
            this.components = new Dictionary<Type, ILinkable>();
            this.components[linkComponent.GetType()] = linkComponent;
            this._FormationType = ftype;
            this.formation = new Formation(this, ftype, InitializeFormation: false);
        }

        public Link(Link link, object source, object target)
        {
            this.room = OrbIt.game.room;
            this.components = new Dictionary<Type, ILinkable>();

            this.UpdateTime = link.UpdateTime;
            this.IsEntangled = link.IsEntangled;
            this.Reversed = link.Reversed;
            this.AngleInc = link.AngleInc;
            this.DrawTips = link.DrawTips;

            foreach (ILinkable component in link.components.Values)
            {
                OComponent newComponent = (OComponent)Activator.CreateInstance(component.GetType());
                OComponent.CloneComponent((OComponent)component, newComponent);
                newComponent.active = true;
                ((ILinkable)newComponent).link = this;
                if (newComponent.GetType().GetProperty("activated") != null) newComponent.GetType().GetProperty("activated").SetValue(newComponent, true, null);

                this.components[newComponent.GetType()] = ((ILinkable)newComponent);
            }

            Initialize(source, target, null, link.formation);

            

        }

        private void Initialize(object src, object trg, ILinkable linkComponent, object formation)
        {
            this.room = OrbIt.game.room;
            this.DrawLinkLines = true;
            
            if (components == null)
            {
                this.components = new Dictionary<Type, ILinkable>();
            }
            if (linkComponent != null)
            {
                linkComponent.link = this;
                linkComponent.active = true;
                this.components[linkComponent.GetType()] = linkComponent;
            }


            if (src is Node && trg is Node) this.ltype = linktype.NodeToNode;
            else if (src is Node && (trg is HashSet<Node> || trg is Group)) this.ltype = linktype.NodeToGroup;
            else this.ltype = linktype.GroupToGroup;

            bool EqualSets = false;

            //source
            if (src is Node)
            {
                this.sourceNode = (Node)src;
                this.sources = new ObservableHashSet<Node>() { sourceNode };
                sourceNode.SourceLinks.Add(this);
                //linkComponent.parent = sourceNode;
                if (trg is Node)
                {
                    sourceNode.OnAffectOthers += NodeToNodeHandler;
                }
                else
                {
                    sourceNode.OnAffectOthers += NodeToGroupHandler;
                }

            }
            else if (src is HashSet<Node>)
            {
                Group ss = new Group();
                foreach (Node s in (HashSet<Node>)src)
                {
                    ss.entities.Add(s);
                }

                this.sourceGroup = ss;
                this.sourceGroup.SourceLinks.Add(this);
                this.sources = this.sourceGroup.fullSet;

                room.masterGroup.childGroups["Link Groups"].AddGroup(ss.Name, ss);

                foreach (Node s in this.sources)
                {
                    s.OnAffectOthers += NodeToGroupHandler;
                    s.SourceLinks.Add(this);
                }
                this.sources.CollectionChanged += sourceGroup_CollectionChanged;

                if (trg is HashSet<Node> && src == trg)
                {
                    EqualSets = true;
                    this.targetGroup = this.sourceGroup;
                    this.targetGroup.TargetLinks.Add(this);
                    this.targets = this.targetGroup.fullSet;
                    foreach (Node t in this.targets)
                    {
                        t.TargetLinks.Add(this);
                    }
                }
            }
            else if (src is Group)
            {
                this.sourceGroup = (Group)src;
                this.sources = this.sourceGroup.fullSet;

                this.sourceGroup.SourceLinks.Add(this);

                foreach (Node s in sources)
                {
                    s.OnAffectOthers += NodeToGroupHandler;
                    s.SourceLinks.Add(this);

                }
                this.sourceGroup.fullSet.CollectionChanged += sourceGroup_CollectionChanged;
            }
            else
            {
                Console.WriteLine("Unrecongized source type when creating link");
            }

            //target
            if (trg is Node)
            {
                this.targetNode = (Node)trg;
                this.targets = new ObservableHashSet<Node>() { targetNode };
                targetNode.TargetLinks.Add(this);
            }
            else if (trg is HashSet<Node> && !EqualSets)
            {
                Group ts = new Group();
                foreach (Node t in (HashSet<Node>)trg)
                {
                    ts.entities.Add(t);
                }
                this.targetGroup = ts;
                this.targets = this.targetGroup.fullSet;
                this.targetGroup.TargetLinks.Add(this);

                room.masterGroup.childGroups["Link Groups"].AddGroup(ts.Name, ts);
                //if (OrbIt.ui != null) OrbIt.ui.sidebar.UpdateGroupComboBoxes();

                foreach (Node t in this.targets)
                {
                    t.TargetLinks.Add(this);
                }
                this.targets.CollectionChanged += targetGroup_CollectionChanged;
            }
            else if (trg is Group)
            {
                this.targetGroup = (Group)trg;
                this.targets = this.targetGroup.fullSet;
                this.targetGroup.TargetLinks.Add(this);

                foreach (Node t in targets)
                {
                    t.TargetLinks.Add(this);
                }
                this.targetGroup.fullSet.CollectionChanged += targetGroup_CollectionChanged;
            }
            else
            {
                Console.WriteLine("Unrecongized target type when creating link");
            }

            if (formation == null)
            {
                this._FormationType = formationtype.AllToAll;
                this.formation = new Formation(this, formationtype.AllToAll);
            }
            else if (formation is formationtype)
            {
                this._FormationType = (formationtype)formation;
                this.formation = new Formation(this, (formationtype)formation);
            }
            else if (formation is Formation)
            {
                this._FormationType = ((Formation)formation).FormationType;
                this.formation = new Formation(this, (Formation)formation);
            }
        }
        //constructors
        public Link(Node sourceNode, Node targetNode, ILinkable linkComponent = null, object formation = null)
        {
            Initialize(sourceNode, targetNode, linkComponent, formation);
        }
        public Link(Node sourceNode, HashSet<Node> targets, ILinkable linkComponent = null, object formation = null)
        {
            Initialize(sourceNode, targets, linkComponent, formation);
        }
        public Link(HashSet<Node> sources, Node targetNode, ILinkable linkComponent = null, object formation = null)
        {
            Initialize(sources, targetNode, linkComponent, formation);
        }
        public Link(HashSet<Node> sources, HashSet<Node> targets, ILinkable linkComponent = null, object formation = null)
        {
            Initialize(sources, targets, linkComponent, formation);
        }
        public Link(HashSet<Node> sources, Group targetGroup, ILinkable linkComponent = null, object formation = null)
        {
            Initialize(sources, targetGroup, linkComponent, formation);
        }
        public Link(Group sourceGroup, HashSet<Node> targets, ILinkable linkComponent = null, object formation = null)
        {
            Initialize(sourceGroup, targets, linkComponent, formation);
        }
        public Link(Node sourceNode, Group targetGroup, ILinkable linkComponent = null, object formation = null)
        {
            Initialize(sourceNode, targetGroup, linkComponent, formation);
        }
        public Link(Group sourceGroup, Node targetNode, ILinkable linkComponent = null, object formation = null)
        {
            Initialize(sourceGroup, targetNode, linkComponent, formation);
        }
        public Link(Group sourceGroup, Group targetGroup, ILinkable linkComponent = null, object formation = null)
        {
            Initialize(sourceGroup, targetGroup, linkComponent, formation);
        }

        //handlers
        public void NodeToNodeHandler(object sender, EventArgs e)
        {
            UpdateNodeToNode();
        }
        public void NodeToGroupHandler(object sender, EventArgs e)
        {
            UpdateNodeToGroup((Node)sender);
        }
        

        void sourceGroup_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Node n in e.NewItems)
                {
                    //n.OnAffectOthers += (o, ee) => UpdateNodeToGroup((Node)o);
                    n.OnAffectOthers += NodeToGroupHandler;
                    n.SourceLinks.Add(this);
                    if (_FormationType == formationtype.AllToAll && formation != null && !formation.AffectionSets.ContainsKey(n))
                    {
                        formation.AffectionSets[n] = targets;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    //n.OnAffectOthers -= (o, ee) => UpdateNodeToGroup((Node)o);
                    n.OnAffectOthers -= NodeToGroupHandler;
                    n.SourceLinks.Remove(this);
                    if (formation != null && formation.AffectionSets.ContainsKey(n))
                    {
                        formation.AffectionSets.Remove(n);
                    }
                }
            }
        }

        void targetGroup_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                
                foreach (Node n in e.NewItems)
                {
                    n.TargetLinks.Add(this);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    n.TargetLinks.Remove(this);
                }
            }
        }

        public void EmptySources()
        {
            foreach(Node n in sources.ToList())
            {
                sources.Remove(sources.ElementAt(0));
            }
        }
        public void EmptyTargets()
        {
            foreach (Node n in targets.ToList())
            {
                targets.Remove(targets.ElementAt(0));
            }
        }

        public void AddLinkComponent(ILinkable linkC, bool active = true)
        {
            linkC.link = this;
            components[linkC.GetType()] = linkC;
            linkC.active = active;
        }

        public T Comp<T>() where T : ILinkable
        {
            if (components.ContainsKey(typeof(T))) return (T)components[typeof(T)];
            return default(T);
        }
        public bool HasComp<T>() where T : ILinkable
        {
            return components.ContainsKey(typeof(T));
        }

        public void UpdateNodeToNode()
        {
            if (!active) return;

            if (IsEntangled)
            {
                foreach (ILinkable link in components.Values)
                {
                    link.parent = sourceNode;
                    link.AffectOther(targetNode);
                    link.parent = targetNode;
                    link.AffectOther(sourceNode);
                }
            }
            else
            {
                foreach (ILinkable link in components.Values)
                {
                    link.parent = sourceNode;
                    link.AffectOther(targetNode);
                }
            }
        }
        
        public void UpdateNodeToGroup(Node source)
        {
            if (!active) return;
            if (IsEntangled)
            {
                if (!formation.AffectionSets.ContainsKey(source)) return;
                foreach (ILinkable link in components.Values)
                {
                    foreach (Node target in formation.AffectionSets[source])
                    {
                        if (source == target) continue;

                        link.parent = source;
                        link.AffectOther(target);
                        link.parent = target;
                        link.AffectOther(source);
                    }
                }
            }
            else
            {
                if (!formation.AffectionSets.ContainsKey(source)) return;

                foreach (ILinkable link in components.Values)
                {
                    link.parent = source;
                    foreach (Node target in formation.AffectionSets[source])
                    {
                        if (source == target) continue;
                        link.AffectOther(target);
                    }
                }
            }
        }
        
        public void GenericDraw()
        {
            if (!active) return;
            if (!DrawLinkLines) return;
            //if (!linkComponent.active)
                //return;

            //float mapzoom = room.zoom;


            Color col;
            /*
            if (linkComponent.activated)
                col = Color.Blue;
            else
                col = Color.White;
            */
            col = Color.blue;

            //float increment = 0.5f;
            anglestep = 0;
            
            //Component lcomp = (Component)linkComponent;
            //col = Group.IntToColor[(int)lcomp.com % Group.IntToColor.Count];

            foreach (Node source in sources)
            {
                //col = source.transform.color;

                //spritebatch.Draw(source.getTexture(), source.transform.position / mapzoom, null, col, 0, source.TextureCenter(), (source.transform.scale / mapzoom) * 1.2f, 0);

                if (!formation.AffectionSets.ContainsKey(source)) continue;
                foreach (Node target in formation.AffectionSets[source])
                {
                    anglestep += AngleInc;


                    Color color1 = Color.white;
                    if (sourceNode != null && sourceNode.IsPlayer)
                    {
                        color1 = sourceNode.material.color;
                    }

                    Vector2 diff = target.transform.position - source.transform.position;
                    Vector2 perp = new Vector2(diff.y, -diff.x);
                    VMath.NormalizeSafe(ref perp);
                    perp *= 2;

                    //room.camera.DrawLine(source.transform.position, target.transform.position, 2f, color1, Layers.Under3);


                    //Utils.DrawLine(spritebatch, source.transform.position + perp, target.transform.position + perp, 2f, col, room);
                    //Utils.DrawLine(spritebatch, source.transform.position - perp, target.transform.position - perp, 2f, col, room);

                    //if (!DrawTips) continue;
                    //perp *= 20;
                    //
                    //Vector2 center = (target.transform.position + source.transform.position) / 2;
                    //
                    //Vector2 point = target.transform.position - (diff / 5);
                    //room.camera.DrawLine(point + perp, target.transform.position, 2f, color1, Layers.Under3);
                    //room.camera.DrawLine(point - perp, target.transform.position, 2f, color1, Layers.Under3);


                }
            }

        }

        public override string ToString()
        {
            string result = "[L]";
            if (components != null)
            {
                foreach(ILinkable link in components.Values)
                {
                    OComponent c = (OComponent)link;
                    result += c.GetType().Name.Substring(0, 4) + "|";
                }
            }
            result += "[" + FormationType + "]";
            return result;
        }

        public void DeleteLink()
        {

            if (sourceNode != null)
            { 
                sourceNode.SourceLinks.Remove(this);
                sourceNode.OnAffectOthers -= NodeToNodeHandler;
            }
            if (targetNode != null) targetNode.TargetLinks.Remove(this);

            if (sources != null)
            {
                sources.CollectionChanged -= sourceGroup_CollectionChanged;
                foreach (Node n in sources)
                {
                    n.OnAffectOthers -= NodeToGroupHandler;
                    n.SourceLinks.Remove(this);
                }
            }
            if (targets != null)
            {
                targets.CollectionChanged -= targetGroup_CollectionChanged;
                foreach (Node n in targets)
                {
                    n.TargetLinks.Remove(this);
                }
            }
            if (sourceGroup != null) sourceGroup.SourceLinks.Remove(this);
            if (targetGroup != null) targetGroup.TargetLinks.Remove(this);

            

            room.AllActiveLinks.Remove(this);
        }

        public static void GetILinkableEnumVals(List<object> list)
        {
            foreach (Type compType in OComponent.compTypes)
            {

                if (!typeof(ILinkable).IsAssignableFrom(compType)) continue;

                list.Add(compType);

            }
        }


    }
}
