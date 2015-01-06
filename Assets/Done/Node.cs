using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.CSharp;
using System.Reflection;
using System.Collections.Specialized;
using System.Collections;
using UnityEngine;


namespace OrbItProcs {

    public enum nodeE {
        active,
        position,
        velocity,
        radius,
        mass,
        texture,
        name,
        lifetime,
        color,
    };

    public enum state
    {
        off,
        updateOnly,
        drawOnly,
        on,
    }
    //public delegate void CollisionDelegate(Node source, Node target);
   
    public class DataStore : Dictionary<string, object>
    {
        public DataStore() : base() { }
    }

    public class Node {
        public GameObject gameobject;
        public static float defaultNodeSize = 1f;
        public static int nodeCounter = 0;
        private Vector3 tempPosition = Vector3.zero;
        public List<string> nodeHistory = new List<string>();
        public bool spawned = false;

        private bool triggerSortComponentsUpdate = false, triggerSortComponentsDraw = false, triggerRemoveComponent = false;
        private Dictionary<Type, bool> tempCompActiveValues = new Dictionary<Type, bool>();

        private state _nodeState = state.on;
        public state nodeState { get { return _nodeState; } set { _nodeState = value; } }
        public bool IsAI { get; set; }

        private bool _active = true;
        public bool active
        {
            get { return _active; }
            set
            {
                if (_active && !value)
                {
                    foreach (Type t in comps.Keys.ToList())
                    {
                        tempCompActiveValues[t] = comps[t].active;
                        comps[t].active = false;
                    }
                }
                else if (!_active && value)
                {
                    foreach (Type t in comps.Keys.ToList())
                    {
                        if (tempCompActiveValues.ContainsKey(t)) comps[t].active = tempCompActiveValues[t];
                        else comps[t].active = true;
                    }
                }
                _active = value;
            }
        }

        private bool _IsDeleted = false;
        public bool IsDeleted
        {
            get { return _IsDeleted; }
            set
            {
                if (!_IsDeleted && value)
                {
                    OnDelete();
                }
                _IsDeleted = value;
            }
        }
        //public bool IsDefault = false;
        //public int lifetime = -1;

        private string _name = "node";
        public string name { get { return _name; } 
            set 
            { 
                _name = value;
                if (value.Equals("Group115"))
                    Console.WriteLine("Group115");
            } 
        }
        public Room room;

        private Dictionary<Type, OComponent> _comps = new Dictionary<Type, OComponent>();
        public Dictionary<Type, OComponent> comps { get { return _comps; } set { _comps = value; } }

        private List<Type> aOtherProps = new List<Type>();
        private List<Type> aSelfProps = new List<Type>();
        private List<Type> drawProps = new List<Type>();
        private List<Type> playerProps = new List<Type>();
        private List<Type> aiProps = new List<Type>();
        private List<Type> compsToRemove = new List<Type>();
        private List<Type> compsToAdd = new List<Type>();

        private HashSet<string> _tags = new HashSet<string>();
        public HashSet<string> tags { get { return _tags; } set { _tags = value; } }

        //private Body _body;
        //public Body body
        //{
        //    get { return _body; }
        //    set
        //    {
        //        _body = value;
        //    } 
        //}
        //
        private Movement _movement;
        public Movement movement
        {
            get { return _movement; }
            set
            {
                _movement = value;
                
                //if (comps != null && value != null)
                if (comps != null && !object.Equals(value, null))
                {
                    if (HasComp<Movement>())
                    {
                        comps.Remove(typeof(Movement));
                    }
                    comps.Add(typeof(Movement), value);
                }
            }
        }

        //private Collision _collision;
        //public Collision collision
        //{
        //    get { return _collision; }
        //    set
        //    {
        //        _collision = value;
        //        if (comps != null && value != null)
        //        {
        //            if (HasComp<Collision>())
        //            {
        //                comps.Remove(typeof(Collision));
        //            }
        //            comps.Add(typeof(Collision), value);
        //        }
        //    }
        //}
        private BasicDraw _basicdraw;
        public BasicDraw basicdraw
        {
            get { return _basicdraw; }
            set
            {
                _basicdraw = value;
                if (comps != null && value != null)
                {
                    if (HasComp<BasicDraw>())
                    {
                        comps.Remove(typeof(BasicDraw));
                    }
                    comps.Add(typeof(BasicDraw), value);
                }
            }
        }
        private Meta _meta;
        public Meta meta
        {
            get { return _meta; }
            set
            {
                _meta = value;
                if (comps != null && value != null)
                {
                    if (HasComp<Meta>())
                    {
                        comps.Remove(typeof(Meta));
                    }
                    comps.Add(typeof(Meta), value);
                }
            }
        }
        private textures _texture = textures.cage;
        public textures texture
        {
            get { return _texture; }
            set 
            { 
                _texture = value;
                material.SetTexture("_MainTex", getTexture2D());
            }
        }
        private Player _player;
        public Player player { get { return _player; } set { _player = value; if (value != null) SortComponentListsUpdate(); } }
        [Info(UserLevel.Never)]
        public bool IsPlayer { get { return player != null; } }

        private ObservableHashSet<Link> _SourceLinks = new ObservableHashSet<Link>();
        public ObservableHashSet<Link> SourceLinks { get { return _SourceLinks; } set { _SourceLinks = value; } }
        private ObservableHashSet<Link> _TargetLinks = new ObservableHashSet<Link>();
        public ObservableHashSet<Link> TargetLinks { get { return _TargetLinks; } set { _TargetLinks = value; } }
        private Group _group;
        public Group group { get { return _group; } set { _group = value; } }

        
        
        [Info(UserLevel.Never)]
        public Scheduler scheduler
        {
            get
            {
                if (!HasComp<Scheduler>())
                {
                    addComponent(typeof(Scheduler), true);
                }
                return Comp<Scheduler>();
            }
            set
            {
                comps[typeof(Scheduler)] = value;
            }
        }

        public bool IsDefault
        {
            get
            {
                if (group != null && this == group.defaultNode) return true;
                return false;
            }
        }

        [Info(UserLevel.Never)]
        public DataStore dataStore = new DataStore();

        public event EventHandler OnAffectOthers;

        public void storeInInstance(nodeE val, Dictionary<object, object> dict)
        {
            if (val == nodeE.active) active = (bool)dict[val];
            if (val == nodeE.name) name = (string)dict[val];
            if (val == nodeE.position) transform.position = (Vector3)dict[val];
            if (val == nodeE.velocity) rigidbody.velocity = (Vector3)dict[val];
            if (val == nodeE.radius) radius = (float)dict[val];
            if (val == nodeE.mass) rigidbody.mass = (float)dict[val];
            if (val == nodeE.texture) texture = (textures)dict[val];
            if (val == nodeE.color) renderer.material.color = (Color)dict[val];
            
        }
        public float radius
        {
            get { return transform.localScale.x; }
            set { 
                transform.localScale = new Vector3(value, value, value); }
        }
        //lower performance because of check for null
        public Transform transform { get { return gameobject != null ? gameobject.transform : null; } }
        public Collider collider { get { return gameobject != null ? gameobject.collider : null; } }
        public Rigidbody rigidbody { get { return gameobject != null ? gameobject.rigidbody : null; } }
        public Renderer renderer { get { return gameobject != null ? gameobject.renderer : null; } }
        public Material material { get { return gameobject != null ? gameobject.renderer.material : null; } }
        public Color permaColor;

        public Vector3 effvelocity = Vector3.zero;
        public bool isSolid { get { return !collider.isTrigger; } set { collider.isTrigger = !value; } }
        //public Node(Room room) : this(room, ShapeType.Circle) { }
        public Node(Room room)//, ShapeType shapetype)
        {
            this.room = room;
            //("Everyone else must use the Parameterized constructor and pass a room reference.");
            name = name + nodeCounter;
            nodeCounter++;
            GameObject prefab = (GameObject)Resources.Load("NodePrefab");
            gameobject = (GameObject)GameObject.Instantiate(prefab);
            gameobject.SetActive(false);
            gameobject.GetComponent<NodeScript>().InitNodeScript(this);
            texture = _texture;

            meta = new Meta(this);
            movement = new Movement(this);


            radius = defaultNodeSize;
            basicdraw = new BasicDraw(this);
            movement.active = true; 
            basicdraw.active = true;
            IsAI = false;
            affectAction = (source, other) =>
            {
                //todo: extend to check for every component for finer control if necessary
                if (source.AffectExclusionCheck != null && source.AffectExclusionCheck(other)) return;
                foreach (Type t in source.aOtherProps)
                {
                    if (!source.comps[t].active) continue;
                    source.comps[t].AffectOther(other);
                }
            };
        }
        Action<Node, Node> affectAction;
        public Node(Room room, Dictionary<object, object> userProps)//, ShapeType shapetype = ShapeType.Circle)
            : this(room)//, shapetype)
        {
            if (userProps != null)
            {
                // add the userProps to the props
                foreach (object p in userProps.Keys)
                {
                    // if the key is a Type, we need to add the component to comps dict
                    if (p is Type)
                    {
                        Type t = (Type)p;
                        fetchComponent(t, (bool)userProps[t]);
                    }
                    // if the key is a nodeE, we need to update the instance variable value
                    else if (p is nodeE)
                    {
                        nodeE nn = (nodeE)p;
                        storeInInstance(nn, userProps);
                    }
                }
            }
            SortComponentLists();
        }

        //public static Node ContructLineWall(Room room, Vector2 start, Vector2 end, int thickness, Dictionary<object, object> props = null, bool addToWallGroup = true)
        //{
        //    float dist = Vector2.Distance(start, end);
        //    int halfheight = (int)(dist / 2);
        //    int halfwidth = thickness / 2;
        //    float angle = VMath.VectorToAngle(start - end);

        //    Node n = new Node(room, props, ShapeType.Polygon);
        //    Polygon p = (Polygon)n.body.shape;
        //    n.body.orient = angle;
        //    p.SetBox(halfwidth, halfheight, false);
        //    n.transform.position = (start + end) / 2;
        //    n.body.DrawPolygonCenter = false;


        //    n.body.SetStatic();
        //    if (addToWallGroup)
        //    {
        //        room.masterGroup.childGroups["Wall Group"].IncludeEntity(n);
        //        n.OnSpawn();
        //    }
        //    return n;
        //}

        public T Comp<T>() where T : OComponent
        {
            return (T)comps[typeof(T)];
        }
        public bool HasComp<T>() where T : OComponent
        {
            return comps.ContainsKey(typeof(T));
        }
        public bool HasComp(Type componentType)
        {
            return comps.ContainsKey(componentType);
        }

        public void EnsureContains<T>(bool active = true) where T : OComponent
        {
            if (!HasComp<T>())
            {
                addComponent<T>(active);
            }
        }
        public bool HasActiveComponent<T>()
        {
            return comps.ContainsKey(typeof(T)) && comps[typeof(T)].active;
        }

        public T CheckData<T>(string key)
        {
            if (dataStore.ContainsKey(key))
            {
                return (T)dataStore[key];
            }
            else
            {
                return default(T);
            }
        }

        public void SetData(string key, object data)
        {
            dataStore[key] = data;
        }

        public void AddTag(string tag)
        {
            tags.Add(tag);
        }
        public void RemoveTag(string tag)
        {
            tags.Remove(tag);
        }
        public Func<Node, bool> AffectExclusionCheck = null;
        public int affectionReach = 180;
        public Vector3 previousFramePosition = new Vector3();
        public virtual void Update()
        {
            if (IsPlayer)
            {
                rigidbody.angularVelocity = Vector3.zero;
            }

            if (!movement.pushable && tempPosition != Vector3.zero)
            {
                transform.position = tempPosition;
                rigidbody.velocity = Vector3.zero;
            }
            previousFramePosition = tempPosition;
            effvelocity = transform.position - tempPosition;
            tempPosition = transform.position;
            
            if (nodeState == state.off || nodeState == state.drawOnly) return;

            if (aOtherProps.Count > 0)
            {
                //if (meta.IgnoreAffectGrid) //always ignore gridsystem for now
                foreach(Node n in room.masterGroup.fullSet)
                {
                    affectAction(this, n);
                }
            }
            if (OnAffectOthers != null) OnAffectOthers.Invoke(this, null);

            foreach (OComponent component in comps.Values)
            {
                component.CaluclateDecay();
                Type t = component.GetType();
                if (aSelfProps.Contains(t))
                    component.AffectSelf();
            }
            if (IsPlayer)
            {
                //player.controller.UpdateNewState();
                player.input.SetNewState();
                foreach (Type c in playerProps)
                {
                    comps[c].PlayerControl(player.input);
                }
                //player.controller.UpdateOldState();
                player.input.SetOldState();
            }
            //AI execution
            if (IsAI)
            {
                foreach (Type c in aiProps)
                {
                    comps[c].AIControl(AIMode.Agro);
                }
            }

            if (movement.active) movement.AffectSelf(); //temporary until make movement list to update at the correct time

            if (triggerSortComponentsUpdate)
            {
                SortComponentListsUpdate();
                triggerSortComponentsUpdate = false;
            }
            if (triggerRemoveComponent)
            {
                RemoveComponentTriggered();
            }
            //gameobject.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            //gameobject.transform.eulerAngles = new Vector3(0, 0, body.orient * Mathf.Rad2Deg);
            //Debug.Log(body.orient);
        }
        //may implement this optimization if there are more affect others compoenents based on grabbing surrounding nodes from buckets
        //public void ReorderAffectOthersList()
        //{
        //    List<Type> affectOthers = aOtherProps.ToList();
        //    //affectOthers.Sort((a, b) => )
        //
        //}

        public void Draw()
        {
            if (nodeState == state.off || nodeState == state.updateOnly) return;
            foreach (Type c in drawProps)
            {
                if (!comps[c].CallDraw) continue;
                if (!comps[c].active) continue;
                comps[c].Draw();
            }
            if (triggerSortComponentsDraw)
            {
                SortComponentListsDraw();
                triggerSortComponentsDraw = false;
            }
            if (triggerRemoveComponent)
            {
                RemoveComponentTriggered();
            }
        }
        public void DrawSlow()
        {
            foreach(OComponent c in comps.Values)
            {
                if (!c.active) continue;
                if (c.hasCompType(mtypes.draw))
                {
                    c.Draw();
                }
            }
        }
        public override string ToString()
        {
            //return base.ToString();
            string ret = name;
            if (IsDefault) ret += "(DEF)";
            return ret;
        }
        public void setCompActive(Type c, bool Active)
        {
            if (comps.ContainsKey(c))
            {
                comps[c].active = Active;
            }
            else
            {
                Console.WriteLine("Component not found in dictionary");
            }
        }

        //assuming caller knows that c is contained in comps (to prevent a very frequent comparison) 
        //(probably called from a foreach of comps.keys anyway)
        public bool isCompActive(Type c)
        {
            return comps[c].active;
        }

        //lists will be sorted once at a safe place, and then these will be set to false;
        public void triggerSortLists()
        {
            triggerSortComponentsUpdate = true;
            triggerSortComponentsDraw = true;
        }

        public void acceptUserProps(Dictionary<object, object> userProps)
        {
            foreach (object p in userProps.Keys)
            {
                // if the key is a node type, (and not a bool) we need to update the instance variable value
                if (p is nodeE)// && !(userProps[p] is bool))
                    storeInInstance((nodeE)p, userProps);
                // if the key is a comp type, we need to add the component to comps dict
                if (p is Type)
                {
                    Type t = (Type)p;
                    fetchComponent(t, (bool)userProps[p]);
                    if (HasComp(t)) comps[t].active = (bool)userProps[t];
                }
            }
            SortComponentLists();
        }

        public void addComponent(Type t, bool active, bool overwrite = false)
        {
            bool fetch = fetchComponent(t, active, overwrite);
            if (fetch) SortComponentLists();
        }
        public void addComponent<T>(bool active, bool overwrite = false) where T : OComponent
        {
            addComponent(typeof(T), active, overwrite);
        }
        public void addComponent(OComponent component, bool active, bool overwrite = false)
        {
            component.parent = this;
            if (comps.ContainsKey(component.GetType()) && !overwrite) return;
            comps[component.GetType()] = component;
            component.active = active;

            component.Initialize(this);
            SortComponentLists();
            if (IsPlayer && component.IsItem())
            {
                player.AddItem(component);
            }
            if (spawned) component.OnSpawn();
        }

        public bool fetchComponent(Type t, bool active, bool overwrite = false)
        {
            if (t == typeof(Movement))//todo: add more essentials here
            {
                movement.active = active;
                return false;
            }
            else if (t == typeof(BasicDraw))
            {
                basicdraw.active = active;
                return false;
            }
            else if (t == typeof(Meta))
            {
                meta.active = active;
                return false;
            }
            if (overwrite)
            {
                OComponent component = MakeComponent(t, active, this);
                if (HasComp(t))
                {
                    comps.Remove(t);
                }
                comps.Add(t, component);
                if (IsPlayer && component.IsItem())
                {
                    player.AddItem(component);
                }
                if (spawned) component.OnSpawn();
            }
            else
            {
                if (!HasComp(t))
                {
                    OComponent component = MakeComponent(t, active, this);
                    comps.Add(t, component);
                    if (IsPlayer && component.IsItem())
                    {
                        player.AddItem(component);
                    }
                    if (spawned) component.OnSpawn();
                }
                else
                {
                    return false;
                }
            }
            return true;
            
        }

        public static OComponent MakeComponent(Type t, bool active, Node parent)
        {
            OComponent component;

            component = OComponent.GenerateComponent(t, parent);
            //component.parent = this;
            component.active = active;
            component.AfterCloning();

            return component;
        }

        public void RemoveComponent(Type t)
        {
            if (!comps.ContainsKey(t))
            {
                Console.WriteLine("Component already removed or doesn't exist.");
                return;
            }
            comps[t].active = false;
            compsToRemove.Add(t);
            if (!room.masterGroup.fullSet.Contains(this))
            {
                SortComponentLists();
                RemoveComponentTriggered();
            }
            else
            {
                triggerSortLists();
                triggerRemoveComponent = true;
            }
        }

        public void RemoveComponentTriggered()
        {
            List<Type> toremove = new List<Type>();
            List<Type> toaddremove = new List<Type>();
            foreach (Type c in compsToRemove)
            {
                if (comps.ContainsKey(c))
                {
                    if (!drawProps.Contains(c) && !aSelfProps.Contains(c) && !aOtherProps.Contains(c))
                    {
                        //we should call a 'destroy component' method here, instead of just hoping it gets garabage collected
                        if (IsPlayer)
                        {
                            player.RemoveItem(comps[c]);
                        }
                        comps[c].OnRemove(null);
                        comps.Remove(c);
                        toremove.Add(c);
                        triggerRemoveComponent = false;
                        triggerSortLists();
                    }
                    else
                    {
                        triggerSortLists();
                    }
                }
            }
            foreach (Type c in compsToAdd)
            {
                if (comps.ContainsKey(c)) continue;

                addComponent(c, true);
                toaddremove.Add(c);
            }
            int cc = toremove.Count;
            for (int i = 0; i < cc; i++)
            {
                compsToRemove.Remove(toremove.ElementAt(0));
            }
            cc = toaddremove.Count;
            for (int i = 0; i < cc; i++)
            {
                compsToAdd.Remove(toaddremove.ElementAt(0));
            }
            
        }

        public void SortComponentLists()
        {
            SortComponentListsUpdate();
            SortComponentListsDraw();
        }

        public void SortComponentListsUpdate()
        {
            aOtherProps = new List<Type>();
            aSelfProps = new List<Type>();
            playerProps = new List<Type>();
            aiProps = new List<Type>();

            var clist = comps.Keys.ToList();
            Comparison<Type> typeComparer = delegate(Type t1, Type t2)
            {
                string s1 = t1.ToString().LastWord('.');
                string s2 = t2.ToString().LastWord('.');
                return s1.CompareTo(s2);
            };
            clist.Sort(typeComparer);

            foreach (Type c in clist)
            {
                if (c == typeof(Movement) || c == typeof(Collision)) continue;
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.affectother) == mtypes.affectother))
                {
                    aOtherProps.Add(c);
                }
            }
            foreach (Type c in clist)
            {
                if (c == typeof(Movement)) continue;
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.affectself) == mtypes.affectself))
                {
                    aSelfProps.Add(c);
                }
            }
            if (IsPlayer)
            {
                foreach (Type c in clist)
                {
                    if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.playercontrol) == mtypes.playercontrol))
                    {
                        playerProps.Add(c);
                    }
                }
            }
            //if (meta.AImode != AIMode.None && meta.AImode != AIMode.Player)
            //{
                foreach (Type c in clist)
                {
                    if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.aicontrol) == mtypes.aicontrol))
                    {
                        aiProps.Add(c);
                    }
                }
            //}
        }

        public void SortComponentListsDraw()
        {
            drawProps = new List<Type>();

            var clist = comps.Keys.ToList();
            Comparison<Type> typeComparer = delegate(Type t1, Type t2)
            {
                string s1 = t1.ToString().LastWord('.');
                string s2 = t2.ToString().LastWord('.');
                return s1.CompareTo(s2);
            };
            clist.Sort(typeComparer);

            foreach (Type c in clist)
            {
                if (comps.ContainsKey(c) && isCompActive(c) && comps[c].hasCompType(mtypes.draw))
                {
                    drawProps.Add(c);
                }
            }
        }
        public Texture2D getTexture2D()
        {
            try { return Assets.textureDict[texture]; }
            catch { Debug.Log(texture + " exception"); return null; }
        }
        public Vector2 TextureCenter()
        { 
            Texture2D tx = Assets.textureDict[texture];
            return new Vector2(tx.width / 2f, tx.height / 2f); // TODO: maybe cast to floats to make sure it's the exact center.
        }
        
        public void SetColor(Color c)
        {
            renderer.material.color = c;
            permaColor = c;
            basicdraw.UpdateColor();
        }
        public float diameter()
        {
            return radius * 2;
        }

        public void OnSpawn()
        {
            //gameobject = new GameObject(name);//, typeof(SpriteRenderer));
            //gameobject.GetComponent<SpriteRenderer>().sprite = Assets.textureDict[body.texture];
            gameobject.transform.parent = OrbIt.game.transform;
            //NodeScript.AddNodeScript(gameobject, this);
            gameobject.SetActive(true);

            texture = _texture;

            foreach (Type key in comps.Keys.ToList())
            {
                OComponent component = comps[key];
                component.OnSpawn();
                //if ((component.compType & mtypes.draw) == mtypes.draw)
                //{
                //    component.draw = new DrawControl(component);
                //    component.SetupSprites();
                //}
            }
        }

        public void OnDeath(Node other, bool delete = true)
        {
            foreach (Type key in comps.Keys.ToList())
            {
                if (key == typeof(Meta)) continue;
                OComponent component = comps[key];
                //MethodInfo mInfo = component.GetType().GetMethod("OnRemove");
                //if (mInfo != null
                //    && mInfo.DeclaringType == component.GetType())
                //{
                    component.OnRemove(other);
                //}
            }
            meta.OnRemove(other);
            if (group != null && delete)
            {
                group.DeleteEntity(this);
            }
            UnityEngine.Object.Destroy(gameobject);
        }

        public void OnDelete()
        {
            //active = false;

            //if (this == room.targetNode) room.targetNode = null;
            //if (this == Game1.ui.sidebar.inspectorArea.editNode) Game1.ui.sidebar.inspectorArea.editNode = null; //todo: social design pattern
            //if (this == Game1.ui.spawnerNode) Game1.ui.spawnerNode = null;

            //if (room.masterGroup != null && room.masterGroup.fullSet.Contains(this))
            //{
            //    room.masterGroup.DiscludeEntity(this);
            //}
            OnDeath(null, false);
        }

        public Node CreateClone(Room room = null)
        {
            Room r = room ?? this.room;
            Node newNode = new Node(r);
            cloneNode(this, newNode);
            return newNode;
        }

        public static void cloneNode(Node sourceNode, Node destNode)
        {
            List<FieldInfo> fields = sourceNode.GetType().GetFields().ToList();
            fields.AddRange(sourceNode.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList());
            List<PropertyInfo> properties = sourceNode.GetType().GetProperties().ToList();

            OComponent.CloneObject(sourceNode.transform, destNode.transform);
            OComponent.CloneObject(sourceNode.rigidbody, destNode.rigidbody);
            OComponent.CloneObject(sourceNode.collider, destNode.collider, 0, true);
            OComponent.CloneObject(sourceNode.renderer, destNode.renderer);
            /*
            foreach (PropertyInfo property in properties)
            {
                //if (property.Name.Equals("compsProp")) continue;
                property.SetValue(destNode, property.GetValue(sourceNode, null), null);
            
            }
            //*/
            //do not copy parent field
            foreach (FieldInfo field in fields)
            {
                if (field.Name.Equals("_comps"))
                {
                    Dictionary<Type, OComponent> dict = sourceNode.comps;
                    foreach (Type key in dict.Keys)
                    {
                        if (key == typeof(Movement) || key == typeof(Collision)) continue;
                        destNode.addComponent(key, sourceNode.comps[key].active);
                        OComponent.CloneComponent(dict[key], destNode.comps[key]);
                        destNode.comps[key].Initialize(destNode);
                    }
                    foreach (Type key in destNode.comps.Keys.ToList())
                    {
                        if (key == typeof(Movement) || key == typeof(Collision)) continue;
                        OComponent component = destNode.comps[key];
                        MethodInfo mInfo = component.GetType().GetMethod("AfterCloning");
                        if (mInfo != null
                            && mInfo.DeclaringType == component.GetType())
                        {
                            component.AfterCloning();
                        }

                    }
                }
                else if ((field.FieldType == typeof(int))
                   || (field.FieldType == typeof(Single))
                   || (field.FieldType == typeof(bool))
                   || (field.FieldType == typeof(string))
                   || (field.FieldType.IsEnum))
                {
                    if (!field.Name.Equals("IsDefault"))
                        field.SetValue(destNode, field.GetValue(sourceNode));
                }
                else if (field.FieldType == typeof(Vector2))
                {
                    Vector2 vect = (Vector2)field.GetValue(sourceNode);
                    Vector2 newvect = new Vector2(vect.x, vect.y);
                    field.SetValue(destNode, newvect);
                }
                else if (field.FieldType == typeof(Vector3))
                {
                    Vector3 vect = (Vector3)field.GetValue(sourceNode);
                    Vector3 newvect = new Vector3(vect.x, vect.y, vect.z);
                    field.SetValue(destNode, newvect);
                }
                else if (field.FieldType == typeof(Color))
                {
                    Color col = (Color)field.GetValue(sourceNode);
                    Color newcol = new Color(col.r, col.g, col.b, col.a);
                    field.SetValue(destNode, newcol);
                }
                else if (field.FieldType == (typeof(Movement)))
                {
                    OComponent.CloneComponent(sourceNode.movement, destNode.movement);
                    destNode.movement.parent = destNode;
                    destNode.movement.AfterCloning();
                }
            }
        }

        internal void clearData(string p)
        {
            dataStore.Remove(p);
        }
    }
}