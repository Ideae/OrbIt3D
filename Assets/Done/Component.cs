using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using System.Reflection;
using Component = OrbItProcs.Component;

namespace OrbItProcs
{
    [Flags]
    public enum mtypes
    {
        none = 0,
        
        affectother = 2,
        affectself = 4,
        draw = 8,
        playercontrol = 256,
        aicontrol = 512,

        tracer = 128,
        essential = 64,
        initialize = 1,
        exclusiveLinker = 32,
        item = 1024,
    };

    public abstract class Component {
        public virtual mtypes compType { get; set; }
        protected bool _active = false;
        [Info(UserLevel.Developer)]
        public virtual bool active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                if (parent != null && parent.HasComp(this.GetType()))
                {
                    parent.triggerSortLists();
                }
            }
        }
        public virtual Node parent { get; set; }
        public Room room { get { if (parent != null) return parent.room; return null; } }
        private bool _CallDraw = true;
        public bool CallDraw { get { return _CallDraw; } set { _CallDraw = value; } }

        public HashSet<Node> exclusions = new HashSet<Node>();

        protected float timePassed = 0;
        protected float maxTime = -1;
        protected bool IsDecaying = false;

        public void SetDecayMaxTime(int seconds, bool isDecaying = true)
        {
            IsDecaying = isDecaying;
            maxTime = seconds * 1000;
        }
        public void CaluclateDecay()
        {
            if (!IsDecaying) return;
            timePassed += Time.deltaTime * 1000;
            if (timePassed > maxTime)
            {
                parent.RemoveComponent(this.GetType());
            }
        }
        public bool isEssential()
        {
            return (compType & mtypes.essential) == mtypes.essential;
        }
        public bool hasCompType(mtypes methodtype)
        {
            return (compType & methodtype) == methodtype;
        }
        public virtual void Initialize(Node parent) { this.parent = parent; }
        public virtual void AfterCloning() { }
        public virtual void OnSpawn() { }
        public virtual void AffectOther(Node other) {  }
        public virtual void AffectSelf() { }
        public virtual void Draw() { }
        public virtual void PlayerControl(OInput input) { }
        public virtual void AIControl(AIMode aiMode) { }
        public virtual void OnRemove(Node other) { }
        public virtual void SetupSprites() { }
        public virtual void InitializeLists() { }

        private DrawControl _draw;
        public DrawControl draw { get { return _draw; } set { _draw = value; } }
        public bool IsItem()
        {
            return (compType & mtypes.item) == mtypes.item;
        }

        public virtual Texture2D getTexture2D()
        {
            if (parent != null)
            {
                return parent.getTexture2D();
            }
            return null;
        }


        public Component()
        {
            if ((compType & mtypes.draw) == mtypes.draw)
            {
                draw = new DrawControl(this);
            }
        }
        public static Component GenerateComponent(Type t, Node par)
        {
            Component component = (Component)Activator.CreateInstance(t, par);
            return component;
        }
        
        public Component CreateClone(Node par)
        {
            Component comp = (Component)Activator.CreateInstance(this.GetType(), par);
            CloneComponent(this, comp);
            return comp;
        }


        public static void CloneComponent(Component sourceComp, Component destComp)
        {
            List<FieldInfo> fields = sourceComp.GetType().GetFields().ToList();
            fields.AddRange(sourceComp.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList());
            List<PropertyInfo> properties = sourceComp.GetType().GetProperties().ToList();
            foreach (PropertyInfo property in properties)
            {
                //if (property.PropertyType == typeof(ModifierInfo)) continue;
                if (property.PropertyType == typeof(Node))
                {
                    var cust = property.GetCustomAttributes(typeof(CopyNodeProperty), false);
                    if (cust.Length > 0)
                    {
                        Node n = (Node)property.GetValue(sourceComp, null);
                        Node nclone = n.CreateClone(sourceComp.room);
                        property.SetValue(destComp, nclone, null);
                        //Console.WriteLine("CLONING : " + property.Name);
                    }
                    continue;
                }
                if (Utils.isToggle(property.PropertyType))
                {
                    object tog = property.GetValue(sourceComp, null);
                    //dynamic newtog = tog.Clone();
                    MethodInfo m = tog.GetType().GetMethod("Clone");
                    if (m != null)
                    {
                        object newtog = m.Invoke(tog, null);
                        property.SetValue(destComp, newtog, null);
                    }
                    continue;
                }
                if (property.PropertyType.IsClass)
                {
                    if (!typeof(Delegate).IsAssignableFrom(property.PropertyType) && !(property.PropertyType == typeof(Link)))
                    {
                        //Console.WriteLine("We should be aware of this.");
                    }
                }
                if (property.GetSetMethod() != null)
                property.SetValue(destComp, property.GetValue(sourceComp, null), null);
            }
            foreach (FieldInfo field in fields)
            {
                if (field.Name.Equals("shape")) continue;

                //No tears... Only Dreams.

                //no longer checking for dictionaries, parent(Node)
                if ((field.FieldType == typeof(int))
                    || (field.FieldType == typeof(Single))
                    || (field.FieldType == typeof(bool))
                    || (field.FieldType == typeof(string)))
                {
                    field.SetValue(destComp, field.GetValue(sourceComp));
                }
                else if (field.FieldType == typeof(Vector2))
                {
                    Vector2 vect = (Vector2)field.GetValue(sourceComp);
                    Vector2 newvect = new Vector2(vect.x, vect.y);
                    field.SetValue(destComp, newvect);
                }
                else if (field.FieldType == typeof(Color))
                {
                    Color col = (Color)field.GetValue(sourceComp);
                    Color newcol = new Color(col.r, col.g, col.b, col.a);
                    field.SetValue(destComp, newcol);
                }
                else
                {
                    //this would be an object field
                    if (field.Name.Equals("room"))
                    {
                        field.SetValue(destComp, field.GetValue(sourceComp));
                    }
                }
                //field.SetValue(newobj, field.GetValue(obj));
            }
            destComp.InitializeLists();
            destComp.AfterCloning();
        }

        //this is NOT clone component
       public static void CloneObject(object sourceObject, object destObject)
       {
           List<FieldInfo> fields = sourceObject.GetType().GetFields().ToList();
           fields.AddRange(sourceObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList());
           List<PropertyInfo> properties = sourceObject.GetType().GetProperties().ToList();
           foreach (PropertyInfo property in properties)
           {
               
               if (property.PropertyType == typeof(Node)) continue;
               if (property.GetSetMethod() != null)
               {
                   property.SetValue(destObject, property.GetValue(sourceObject, null), null);
               }
           }
           foreach (FieldInfo field in fields)
           {
               if (field.Name.Equals("shape")) continue;
               //no longer checking for dictionaries, parent(Node)
               if ((field.FieldType == typeof(int))
                   || (field.FieldType == typeof(Single))
                   || (field.FieldType == typeof(bool))
                   || (field.FieldType == typeof(string)))
               {
                   field.SetValue(destObject, field.GetValue(sourceObject));
               }
               else if (field.FieldType == typeof(Vector2))
               {
                   Vector2 vect = (Vector2)field.GetValue(sourceObject);
                   Vector2 newvect = new Vector2(vect.x, vect.y);
                   field.SetValue(destObject, newvect);
               }
               else if (field.FieldType == typeof(Color))
               {
                   Color col = (Color)field.GetValue(sourceObject);
                   Color newcol = new Color(col.r, col.g, col.b, col.a);
                   field.SetValue(destObject, newcol);
               }
               else if (field.FieldType == typeof(Room))
               {
                    field.SetValue(destObject, field.GetValue(sourceObject));
               }
           }

           MethodInfo mInfo = destObject.GetType().GetMethod("InitializeLists");
           if (mInfo != null) mInfo.Invoke(destObject, null);
           mInfo = destObject.GetType().GetMethod("AfterCloning");
           if (mInfo != null) mInfo.Invoke(destObject, null);
           
           //destObject.InitializeLists();
           //destObject.AfterCloning();
       }


       public static HashSet<Type> compTypes;
       public static Dictionary<Type, Info> compInfos;
       static Component()
       {
           compTypes = AppDomain.CurrentDomain.GetAssemblies()
                      .SelectMany(assembly => assembly.GetTypes())
                      .Where(type => type.IsSubclassOf(typeof(Component))).ToHashSet();


           compInfos = new Dictionary<Type, Info>();
           foreach (Type t in compTypes)
           {
               Info info = Utils.GetInfoType(t);
               if (info == null) continue;
               compInfos[t] = info;
           }
       }

    }

    
}
