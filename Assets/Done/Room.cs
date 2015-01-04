using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Component = OrbItProcs.Component;
using System.Collections.ObjectModel;
using OrbItProcs;
using UnityEngine;

namespace OrbItProcs {

    public class Room : MonoBehaviour
    {
        ////Room
        //consts
        public const float WallWidth = 10;
        //Fields
        public static long totalElapsedMilliseconds = 0;
        public Node targetNodeGraphic = null;
        //Components
        public ProcessManager processManager { get; set; }
        //public GridSystem gridsystemAffect { get; set; }
        //public Level level { get; set; }
        //public RenderTarget2D roomRenderTarget { get; set; }
        //public ThreadedCamera camera { get; set; }
        public Scheduler scheduler { get; set; }
        //Entities
        public Group masterGroup { get; set; }
        public RoomGroups groups { get; private set; }
        public Group activeGroup { get; set; }
        public Node defaultNode { get; set; }
        public Node activeDefaultNode { get { return activeGroup != null ? activeGroup.defaultNode : null; } }
        public HashSet<Player> players { get; set; }
        [Info(UserLevel.Never)]
        public HashSet<Node> playerNodes { get { return players.Select(p => p.node).ToHashSet(); } }
        public ObservableHashSet<Link> AllActiveLinks { get; set; }
        public ObservableHashSet<Link> AllInactiveLinks { get; set; }
        public List<Rect> linesToDraw = new List<Rect>();
        //Values
        public Vector3 worldSize = Vector3.one;
        public float worldWidth { get { return worldSize.x; } set { worldSize.x = value; } }
        public float worldHeight { get { return worldSize.y; } set { worldSize.y = value; } }
        public float worldDepth { get { return worldSize.z; } set { worldSize.z = value; } }
        public bool DrawLinks { get; set; }
        public Node targetNode { get; set; }
        public Color borderColor { get; set; }
        //public bool DrawAffectGrid { get; set; }
        //public bool DrawCollisionGrid { get; set; }
        //Events
        public event EventHandler AfterIteration;
        private Action PendingRoomResize;
        public Room(OrbIt game, Vector3 worldSize, bool Groups = true)
        {
            groups = new RoomGroups(this);
            AllActiveLinks = new ObservableHashSet<Link>();
            AllInactiveLinks = new ObservableHashSet<Link>();

            this.worldSize = worldSize;
            
            scheduler = new Scheduler();
            borderColor = Color.grey; //DarkGray #unity
            
            // grid System
            //gridsystemAffect = new GridSystem(this, 40, new Vector2(-worldWidth/2, -worldHeight/2), worldWidth, worldHeight);
            //collisionManager = new CollisionManager(this);
            //level = new Level(this, 40, 40, gridsystemAffect.cellWidth, gridsystemAffect.cellHeight);
            //roomRenderTarget = new RenderTarget2D(game.GraphicsDevice, OrbIt.ScreenWidth, OrbIt.ScreenHeight);
            //gridsystemCollision = new GridSystem(this, gridsystemAffect.cellsX, new Vector2(0, worldHeight - OrbIt.Height), worldWidth, OrbIt.Height);
            //camera = new ThreadedCamera(this, 1f);
            DrawLinks = true;
            scheduler = new Scheduler();

            players = new HashSet<Player>();

            #region ///Default User props///
            Dictionary<object, object> userPr = new Dictionary<object, object>() {
                { nodeE.position, Vector3.zero },
                { nodeE.texture, textures.cage },
            };
            #endregion


            defaultNode = new Node(this, userPr);
            defaultNode.name = "master";
            
            //defaultNode.IsDefault = true;

            foreach(Component c in defaultNode.comps.Values)
            {
                c.AfterCloning();
            }

            Node firstdefault = new Node(this);
            //firstdefault.addComponent(comp.itempayload, true);
            Node.cloneNode(defaultNode, firstdefault);
            firstdefault.name = "[G0]0";
            //firstdefault.IsDefault = true;

            masterGroup = new Group(this, defaultNode, null, defaultNode.name, false);
            if (Groups)
            {
                new Group(this, defaultNode, masterGroup, "General Groups", false);
                new Group(this, defaultNode, masterGroup, "Preset Groups", false);
                new Group(this, defaultNode.CreateClone(this), masterGroup, "Player Group", true);
                new Group(this, defaultNode, masterGroup, "Item Group", false);
                new Group(this, defaultNode, masterGroup, "Link Groups", false);
                new Group(this, defaultNode.CreateClone(this), masterGroup, "Bullet Group", true);
                new Group(this, defaultNode, masterGroup, "Wall Group", true);
                Group g = new Group(this, firstdefault, groups.general, "Group1");
                activeGroup = g;
            }
            Dictionary<object, object> userPropsTarget = new Dictionary<object, object>() {
                    { typeof(ColorChanger), true }, 
                    { nodeE.texture, textures.blackorb } 
            };

            targetNodeGraphic = new Node(this,userPropsTarget);
            
            targetNodeGraphic.name = "TargetNodeGraphic";

            //MakeWalls(WallWidth);

            MakePresetGroups();
            MakeItemGroups();
        }

        public void attatchToSidebar() //UserInterface ui)
        {
            //We put the Procs In OrbItProcs
            processManager = new ProcessManager();
            processManager.SetProcessKeybinds();
            //ui.sidebar.UpdateGroupComboBoxes();
        }
        
        public void MakePresetGroups()
        {
            var infos = Component.compInfos;
            int runenum = 0;
            foreach(Type t in infos.Keys)
            {
                Info info = infos[t];
                if ((info.compType & mtypes.essential) == mtypes.essential) continue;
                if ((info.compType & mtypes.exclusiveLinker) == mtypes.exclusiveLinker) continue;
                if ((info.compType & mtypes.item) == mtypes.item) continue;
                if (info.userLevel == UserLevel.Developer || info.userLevel == UserLevel.Advanced) continue;
                if (t == typeof(Lifetime)) continue;
                if (t == typeof(Rune)) continue;
                Node nodeDef = defaultNode.CreateClone(this);
                nodeDef.SetColor(Utils.randomColor());
                nodeDef.addComponent(t, true);
                nodeDef.addComponent(typeof(Rune), true);
                nodeDef.Comp<Rune>().runeTexture = (textures)runenum++;
                Group presetgroup = new Group(this, nodeDef, groups.preset, t.ToString().LastWord('.') + " Group");
            }
        }

        public void MakeItemGroups()
        {
            Node itemDef = defaultNode.CreateClone(this);
            itemDef.addComponent(typeof(ItemPayload), true);
            itemDef.movement.active = false;

            var infos = Component.compInfos;
            foreach (Type t in infos.Keys)
            {
                Info info = infos[t];
                if ((info.compType & mtypes.item) != mtypes.item) continue;
                if (t == typeof(ItemPayload)) continue;
                //if (info.userLevel == UserLevel.Developer || info.userLevel == UserLevel.Advanced) continue;
                Node nodeDef = itemDef.CreateClone(this); ///
                //nodeDef.addComponent(t, true);
                Component c = Node.MakeComponent(t, true, nodeDef);
                nodeDef.Comp<ItemPayload>().AddComponentItem(c);
                new Group(this, nodeDef, groups.items, t.ToString().LastWord('.') + " Item");
            }
        }
        
        
        public int affectAlgorithm = 2;
        public bool ColorNodesInReach = false;

        public float scrollRate = 1.5f;//0.5f;
        public bool skipOutsideGrid = false;
        public bool scroll = false; //#tojam
        public int waitTime = 5000;
        public int waitTimeCounter = 0;
        public void Update()
        {

            Player.CheckForPlayers(this);

            long elapsed = 0;
            elapsed = (long)(Time.deltaTime * 1000);
            totalElapsedMilliseconds += elapsed;

            foreach(Node n in masterGroup.fullSet.ToList())
            {
                if (n.active)
                {
                    n.Update();
                }
            }
            if (AfterIteration != null) AfterIteration(this, null);
            //addBorderLines();

            updateTargetNodeGraphic();

            scheduler.AffectSelf();

            if (PendingRoomResize != null)
            {
                PendingRoomResize();
                PendingRoomResize = null;
            }

            //Draw();
            
        }

        public void GroupSelectDraw() //todo: make this the draw method in groupselect class
        {
            if (processManager.processDict.ContainsKey(typeof(GroupSelect)))
            {
                HashSet<Node> groupset = processManager.GetProcess<GroupSelect>().groupSelectSet;
                if (groupset != null)
                {
                    targetNodeGraphic.renderer.material.color = Color.green;
                    foreach (Node n in groupset.ToList())
                    {
                        targetNodeGraphic.transform.position = n.transform.position;
                        targetNodeGraphic.radius = n.radius * 1.5f;
                        targetNodeGraphic.Draw();
                    }
                }
            }
        }
        public void Draw()
        {
            //spritebatch.Draw(game.textureDict[textures.whitepixel], new Vector2(300, 300), null, Color.Black, 0f, Vector2.Zero, 100f, 0);
            if (targetNode != null)
            {
                updateTargetNodeGraphic();
                targetNodeGraphic.Draw();
            }
            GroupSelectDraw();
            foreach(var n in masterGroup.fullSet.ToList()) //todo:wtfuck threading?
            {
                //if (skipOutsideGrid && (n.transform.position.y < (gridsystemAffect.position.y - gridsystemAffect.gridHeight /2) || n.transform.position.y > gridsystemAffect.position.y + gridsystemAffect.gridHeight)) continue;
                //Node n = (Node)o;
                n.Draw();
            }

            //camera.drawGrid(linesToDraw, borderColor);
            linesToDraw = new List<Rect>();


            //if (DrawAffectGrid) gridsystemAffect.DrawGrid(this, Color.blue);//LightBlue);

            if (DrawLinks)
            {
                foreach (Link link in AllActiveLinks)
                {
                    link.GenericDraw();
                }
            }
            OrbIt.globalGameMode.Draw();
            processManager.Draw(); //find out why we needed this and generalize later

        }
        public void updateTargetNodeGraphic()
        {
            if (targetNode != null)
            {
                targetNodeGraphic.Comp<ColorChanger>().AffectSelf();
                targetNodeGraphic.transform.position = targetNode.transform.position;
                targetNodeGraphic.radius = targetNode.radius * 1.5f;
            }
        }

        public void addRectangleLines(float x, float y, float width, float height)
        {
            addRectangleLines((int)x, (int)y, (int)width, (int)height);
        }
        public Node SelectNodeAt(Vector2 pos)
        {
            Node found = null;
            float shortedDistance = float.MaxValue;
            for (int i = masterGroup.fullSet.Count - 1; i >= 0; i--)
            {
                Node n = (Node)masterGroup.fullSet.ElementAt(i);
                // find node that has been clicked, starting from the most recently placed nodes
                float distsquared = Vector2.Distance(n.transform.position, pos);
                
                if (distsquared < n.radius)
                {
                    if (distsquared < shortedDistance)
                    {
                        found = n;
                        shortedDistance = distsquared;
                    }
                }
            }
            return found;
        }
        public Node spawnNode(int worldMouseX, int worldMouseY)
        {
            Dictionary<object, object> userP = new Dictionary<object, object>() {
                                { nodeE.position, new Vector3(worldMouseX,worldMouseY, 0f) },
            };
            return spawnNode(userP);
        }
        public Node spawnNode(Node newNode, Action<Node> afterSpawnAction = null, int lifetime = -1, Group g = null)
        {
            Group spawngroup = g ?? activeGroup;
            if (spawngroup == null || !spawngroup.Spawnable) return null;
            //newNode.name = "bullet" + Node.nodeCounter;
            return SpawnNodeHelper(newNode, afterSpawnAction, spawngroup, lifetime);
        }
        public Node spawnNode(Dictionary<object, object> userProperties, Action<Node> afterSpawnAction = null, bool blank = false, int lifetime = -1)
        {
            if (activeGroup == null || !activeGroup.Spawnable) return null;

            Node newNode = new Node(this);
            if (!blank)
            {
                Node.cloneNode(activeDefaultNode, newNode);
            }
            newNode.group = activeGroup;
            newNode.name = activeGroup.Name + Node.nodeCounter;
            newNode.acceptUserProps(userProperties);

            return SpawnNodeHelper(newNode, afterSpawnAction, activeGroup, lifetime);
        }
        public Node spawnNode(Group group, Dictionary<object, object> userProperties = null)
        {
            if (group == null) return null;
            Node newNode = group.defaultNode.CreateClone(this);
            newNode.group = group;
            newNode.name = group.Name + Node.nodeCounter;
            if (userProperties != null) newNode.acceptUserProps(userProperties);
            return SpawnNodeHelper(newNode, null, group, -1);
        }
        private Node SpawnNodeHelper(Node newNode, Action<Node> afterSpawnAction = null, Group g = null, int lifetime = -1)
        {
            //newNode.addComponent(comp.itempayload, true);
            newNode.OnSpawn();
            if (afterSpawnAction != null) afterSpawnAction(newNode);
            if (lifetime != -1)
            {
                newNode.addComponent<Lifetime>(true);
                newNode.Comp<Lifetime>().timeUntilDeath.value = lifetime;
                newNode.Comp<Lifetime>().timeUntilDeath.enabled = true;
            }
            
            g.IncludeEntity(newNode);
            newNode.spawned = true;
            return newNode;
        }

        public class RoomGroups
        {
            private Room room;
            public Group general { get { if (room.masterGroup == null) return null; return room.masterGroup.childGroups["General Groups"]; } }
            public Group preset { get { if (room.masterGroup == null) return null; return room.masterGroup.childGroups["Preset Groups"]; } }
            public Group player { get { if (room.masterGroup == null) return null; return room.masterGroup.childGroups["Player Group"]; } }
            public Group items { get { if (room.masterGroup == null) return null; return room.masterGroup.childGroups["Item Group"]; } }
            public Group bullets { get { if (room.masterGroup == null) return null; return room.masterGroup.childGroups["Bullet Group"]; } }
            public Group walls { get { if (room.masterGroup == null) return null; return room.masterGroup.childGroups["Wall Group"]; } }
            public RoomGroups(Room room) { this.room = room; }
        }


    }

}
