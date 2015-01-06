using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
namespace OrbItProcs
{
    public enum GroupState { off, drawingOnly, updatingOnly, on };

    public class Group
    {
        public static int GroupNumber = 2;
        public static Dictionary<int, Color> IntToColor = new Dictionary<int, Color>()
        {
            { 0, Color.white },
            { 1, Color.green },
            { 2, Color.red },
            { 3, Color.blue },
            { 4, Color.magenta }
        };

        public int GroupId { get; set; }
        public Group parentGroup { get; set; }
        //
        public ObservableHashSet<Node> fullSet { get; set; }
        public ObservableHashSet<Node> entities { get; set; }
        public ObservableHashSet<Node> inherited { get; set; }
        private Dictionary<string, Group> _childGroups;
        public Dictionary<string, Group> childGroups
        {
            get
            {
                return _childGroups;
            }
            set
            {
                _childGroups = value;
                foreach(Group g in _childGroups.Values)
                {
                    g.parentGroup = this;
                }
            }
        }
        private Node _defaultNode = null;
        public Node defaultNode { get { return _defaultNode; } 
            set 
            { 
                _defaultNode = value;
                if (value != null)
                {
                    value.group = this;
                }
                if (nodescript != null)
                {
                    nodescript.node = value;
                }
            } 
        }
        public Room room;
        private string _Name;
        public string Name { get { return _Name; } set { if (_Name != null && _Name.Equals("master")) return; _Name = value; } } //cannot rename main group
        public bool Spawnable { get; set; }
        //public GroupState groupState { get; set; }

        private bool _Disabled = false;
        public bool Disabled
        {
            get { return _Disabled; }
            set
            {
                _Disabled = value;
                if (value)
                {
                    if (parentGroup != null)
                    {
                        
                        foreach (Node n in fullSet)
                        {
                            if (parentGroup.inherited.Contains(n)) parentGroup.inherited.Remove(n);
                        }
                        Spawnable = false;
                    }
                }
                else
                {
                    if (parentGroup != null)
                    {
                        foreach (Node n in fullSet)
                        {
                            parentGroup.inherited.Add(n);
                        }
                        Spawnable = true;
                    }
                }
            }
        }

        private ObservableHashSet<Link> _SourceLinks = new ObservableHashSet<Link>();
        public ObservableHashSet<Link> SourceLinks { get { return _SourceLinks; } set { _SourceLinks = value; } }

        private ObservableHashSet<Link> _TargetLinks = new ObservableHashSet<Link>();
        public ObservableHashSet<Link> TargetLinks { get { return _TargetLinks; } set { _TargetLinks = value; } }
        public List<Group> groupPath { get; set; }
        public GameObject gameObject { get; set; }
        public NodeScript nodescript { get; set; }
        public Group()
            : this(null)
        {
        }
        public Group(Room room, Node defaultNode = null, Group parentGroup = null, string Name = "", bool Spawnable = true, ObservableHashSet<Node> entities = null)
        {
            if (parentGroup != null) room = parentGroup.room;
            this.room = room ?? OrbIt.game.room;

            GroupId = -1;
            this.defaultNode = defaultNode ?? this.room.defaultNode;
            this.entities = entities ?? new ObservableHashSet<Node>();
            this.inherited = new ObservableHashSet<Node>();
            this.fullSet = new ObservableHashSet<Node>();
            if (entities != null)
            {
                foreach (Node e in entities)
                {
                    fullSet.Add(e);
                }
            }
            this.parentGroup = parentGroup;
            //this.groupState = groupState;
            this.Spawnable = Spawnable;
            this.childGroups = new Dictionary<string, Group>();
            this.entities.CollectionChanged += entities_CollectionChanged;
            this.inherited.CollectionChanged += entities_CollectionChanged;

            if (Name.Equals("")) 
            {
                this.GroupId = GroupNumber;
                Name = "Group" + GroupNumber; //maybe a check that the name is unique
                GroupNumber++; 
            }
            this.Name = Name;
            groupPath = new List<Group>();

            gameObject = new GameObject(Name);
            nodescript = gameObject.AddComponent<NodeScript>();
            nodescript.node = defaultNode;

            if (parentGroup != null)
            {
                parentGroup.AddGroup(this.Name, this);
                Group g = parentGroup;

                while (g != null)
                {
                    groupPath.Add(g);
                    g = g.parentGroup;
                }
                gameObject.transform.parent = parentGroup.gameObject.transform;
            }
        }


        void entities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //bool ui = OrbIt.ui != null && OrbIt.ui.sidebar.cbListPicker != null;
                foreach (Node n in e.NewItems)
                {
                    if (parentGroup != null && !parentGroup.entities.Contains(n))
                    {
                        parentGroup.inherited.Add(n);
                    }
                    fullSet.Add(n);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    if (sender != fullSet)
                    {
                        if (!entities.Contains(n) && !inherited.Contains(n))
                            fullSet.Remove(n);
                    }
                    if (parentGroup != null && parentGroup.inherited.Contains(n))
                    {
                        parentGroup.inherited.Remove(n);
                    }
                    if (n.group == this) n.group = null;
                }
            }
        }

        public void EmptyGroup()
        {
            foreach(Node n in fullSet.ToList())
            {
                DeleteEntity(n);
            }
            
        }

        public void ForEachFullSet(Action<Node> action)
        {
            //fullSet.ToList().ForEach(action);
            foreach(var n in fullSet) // ToList()
            {
                action(n);
            }
        }
        //adds entity to current group and all parent groups
        public void IncludeEntity(Node entity)
        {
            entities.Add(entity);
            entity.group = this;
            entity.gameobject.transform.parent = gameObject.transform;
//             if (OrbIt.ui.sidebar.cbListPicker.Text.Equals(Name)) // if there is a crash in this line, we removed (room.game.IsOldUI && 
//             {
//                 OrbIt.ui.sidebar.lstMain.Items.Add(entity);
//                 OrbIt.ui.sidebar.SyncTitleNumber(this);
//             }
            //if (parentGroup != null)
            //    parentGroup.IncludeEntity(entity);
        }
        //removes entity from current group and all child groups
        public void DiscludeEntity(Node entity)
        {
            entities.Remove(entity);
            if (inherited.Contains(entity))
                inherited.Remove(entity);
            //fullSet.Remove(entity);
            foreach(Group g in groupPath)
            {
                g.inherited.Remove(entity);
                //if (g.fullSet.Contains(entity))
                //    System.Diagnostics.Debugger.Break();
            }
            //if (childGroups.Count > 0)
            //{
            //    foreach (Group childgroup in childGroups.Values.ToList())
            //    {
            //        if (childgroup.fullSet.Contains(entity))
            //            childgroup.DiscludeEntity(entity);
            //    }
            //}
        }
        //removes entity from all groups, starting from the highest root
        public void DeleteEntity(Node entity)
        {
            //entity.active = false;
            //Group root = this;
            //while (root.parentGroup != null)
            //{
            //    root = root.parentGroup;
            //}
            DiscludeEntity(entity);
            //DiscludeEntity(entity);
            //entities.Remove(entity);
            entity.OnDelete();
            entity.group = null;
        }

        public Group FindGroup(string name)
        {
            Group root = this;
            while (root.parentGroup != null)
            {
                root = root.parentGroup;
            }
            Group result = root.FindGroupRecurse(name);
            if (result != null) return result;
            return root;
        }

        private Group FindGroupRecurse(string name)
        {
            if (Name.Equals(name)) return this;
            if (childGroups.Count == 0) return null;

            foreach (Group g in childGroups.Values)
            {
                Group result = g.FindGroupRecurse(name);
                if (result != null) return result;
            }
            return null;
        }

        
        public void ForEachAllSets(Action<Node> action)
        {
            entities.ToList().ForEach(action);
            inherited.ToList().ForEach(action);
        }

        public void TraverseGroups()
        {
            foreach (Group g in childGroups.Values.ToList())
            {
                g.TraverseGroups();
            }
        }

        //dunno about this
        public void RemoveFromChildrenDeep(Node toremove)
        {
            if (entities.Contains(toremove)) entities.Remove(toremove);
            if (childGroups.Count == 0) return;

            foreach(string s in childGroups.Keys.ToList())
            {
                childGroups[s].RemoveFromChildrenDeep(toremove);
            }
        }

        //public void Update(GameTime gametime)
        //{
        //    if (groupState.In(GroupState.on, GroupState.updatingOnly))
        //    {
        //        entities.ToList().ForEach(delegate(Node n) { ((Node)n).Update(gametime); });
        //    }
        //}

        //public void Draw(SpriteBatch spritebatch)
        //{
        //    if (groupState.In(GroupState.on, GroupState.drawingOnly))
        //    {
        //        entities.ToList().ForEach(delegate(Node n) { ((Node)n).Draw(); });
        //    }
        //}

        public override string ToString()
        {
            return Name;
        }
        /*
        public Group FindGroup(string name)
        {
            if (name.Equals(Name)) return this;

            foreach (string s in childGroups.Keys.ToList())
            {
                if (name.Equals(s)) return childGroups[s];
            }
            if (parentGroup != null) return parentGroup;
            return this;
        }
        */
        public void AddGroup(string name, Group group)
        {
            if (childGroups.ContainsKey(name))
            {
                return;
                //throw new SystemException("Error: One of the childGroups with the same key was already present in this Group.");
            }
            childGroups.Add(name, group);
            group.parentGroup = this;
            foreach(Node n in group.fullSet)
            {
                inherited.Add(n);
            }
        }

        public void DetatchFromParent()
        {
            if (parentGroup == null) return;
            foreach(Node n in fullSet)
            {
                parentGroup.inherited.Remove(n);
            }
            if (parentGroup.childGroups.ContainsKey(Name))
            {
                parentGroup.childGroups.Remove(Name);
            }
            parentGroup = null;
        }

        public void GroupNamesToList(List<object> list, bool addSelf = true)
        {
            if (addSelf) list.Add(Name);
            foreach (Group g in childGroups.Values)
            {
                g.GroupNamesToList(list);
            }
        }

        public void DeleteGroup()
        {
            foreach (Group g in childGroups.Values)
                g.DeleteGroup();

            foreach (Node n in entities.ToList())
            {
                n.group = null;
                DeleteEntity(n);
            }
            if (parentGroup == null) throw new SystemException("Don't delete orphans");
            parentGroup.childGroups.Remove(Name);
        }

        /*
        public void UpdateComboBox()
        {
            Game1.ui.sidebar.cbListPicker.ItemIndex = 0;
            List<object> list = Game1.ui.sidebar.cbListPicker.Items;
            list.ToList().ForEach((o) => list.Remove(o));

            GroupNamesToList(list);
            list.Add("Other Objects");
        }
        */
        //unfortunately I'm not sure it makes sense to use this awesome method
        public static void ForEachDictionary (Dictionary<string,Group> dict, Action<object> action)
        {
            //dict.Values.ToList().Select(x => x.entities).Aggregate((x, y) => (ObservableCollection<object>)x.Union(y)).ToList().ForEach(action);
            HashSet<object> hashset = new HashSet<object>();
            dict.Keys.ToList().ForEach(delegate(string key) {
                Group g = dict[key];

                g.entities.ToList().ForEach(delegate(Node o)
                { 
                    if (!hashset.Contains(o))
                    {
                        hashset.Add(o);
                        action(o);
                    }
                });
            });
        }
    }
}
