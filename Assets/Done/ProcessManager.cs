using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbItProcs
{
    public class ProcessManager
    {
        public Dictionary<Type, Process> processDict { get; set; }
        public HashSet<Process> activeProcesses;

        public ProcessManager()
        {
            this.processDict = new Dictionary<Type, Process>();
            this.activeProcesses = new HashSet<Process>();
            //OrbIt.ui.groupSelectSet = GetProcess<GroupSelect>().groupSelectSet;
        }

        public T GetProcess<T>() where T : Process, new()
        {
            Type t = typeof(T);
            if (processDict.ContainsKey(t))
            {
                return (T)processDict[t];
            }
            else
            {
                //Process p = (Process)Activator.CreateInstance(t);
                Process p = new T();
                //p.Create();
                p.InvokeOnCreate();
                processDict.Add(t, p);
                return (T)p;
            }
        }



        Action enableKeyBinds<T>() where T : Process, new()
        {
            return delegate
            {
                OrbIt.game.keyManager.AddProcess(this, GetProcess<T>());
            };
        }

        public void SetProcessKeybinds()
        {
            //ToolWindow toolbar = OrbIt.ui.sidebar.toolWindow;
            KeyManager Keybindset = OrbIt.game.keyManager;

            Keybindset.Add("spawnnodes", new KeyBundle(KeyCode.Alpha1, KeyCode.LeftShift), enableKeyBinds<SpawnNodes>());
            //toolbar.AddButton("spawn", enableKeyBinds<SpawnNodes>(), "Spawn node of selected group. RightClick to spawn many");
            //toolbar.AddButton("remove", enableKeyBinds<RemoveNodes>(), "Remove nodes: leftclick single, rightclick drag, middleclick remove all.");
            //
            //Keybindset.Add("groupselect", new KeyBundle(KeyCodes.D3, KeyCodes.LeftShift), enableKeyBinds<GroupSelect>());
            //Keybindset.Add("singleselect", new KeyBundle(KeyCodes.D4, KeyCodes.LeftShift), enableKeyBinds<SingleSelect>());
            //toolbar.AddButton("select", enableKeyBinds<SingleSelect>(), "Click to select a node, drag to select many");
            //
            //Keybindset.Add("mapeditor", new KeyBundle(KeyCodes.D5, KeyCodes.LeftShift), enableKeyBinds<MapEditor>());
            //toolbar.AddButton("level", enableKeyBinds<MapEditor>(), "Click to set static colidable polygons.");
            //
            //Keybindset.Add("randomizer", new KeyBundle(KeyCodes.D2, KeyCodes.LeftShift), enableKeyBinds<Randomizer>());
            //toolbar.AddButton("random", enableKeyBinds<Randomizer>(), "Click to spawn a random node, right click to spawn a copy of the previous random node.");
            //
            //toolbar.AddButton("forceSpawn", enableKeyBinds<DirectedSpawn>(), "Spawn nodes in a direction using left and right click.");
            //toolbar.AddButton("forcePush", enableKeyBinds<FloodFill>(), "Take a hike.");
            ////toolbar.AddButton("control", enableKeyBinds<DiodeSpawner>(), "Spawn things in a confusing way");
            //toolbar.AddButton("control", enableKeyBinds<PlayerProcess>(), "Create a player who will use the keyboard and mouse.");
            //toolbar.AddButton("static", enableKeyBinds<GridSpawn>(), "Spawn nodes statically to the grid.");
            //toolbar.AddButton("resize", enableKeyBinds<ResizeRoom>(), "Change the size of the Room");
            //
            //Keybindset.Add("resetplayers", new KeyBundle(KeyCodes.Home), delegate { Player.ResetPlayers(OrbIt.game.room); });
            //
            //Keybindset.Add("pausegame", new KeyBundle(KeyCodes.F, KeyCodes.LeftShift), delegate { OrbIt.ui.IsPaused = !OrbIt.ui.IsPaused; });
            //
            //
            //Keybindset.Add("graphdata", new KeyBundle(KeyCodes.D6, KeyCodes.LeftShift), enableKeyBinds<GraphData>());
            //
            //Keybindset.Add("polygonspawner", new KeyBundle(KeyCodes.D9, KeyCodes.LeftShift), enableKeyBinds<PolygonSpawner>());
            ////Keybindset.Add("diodespawner", new KeyBundle(KeyCodes.D8, KeyCodes.LeftShift), enableKeyBinds(proc.diodeSpawner));
            //Keybindset.Add("screenshot", new KeyBundle(KeyCodes.PrintScreen), () => { OrbIt.game.room.camera.TakeScreenshot = true; });



            //Keybindset.AddProcess(this, GetProcess<CameraControl>(), false);
            Keybindset.AddProcess(this, GetProcess<SpawnNodes>());

            //OrbIt.ui.sidebar.UpdateProcessView();
        }
        
        public void Update()
        {
            /*foreach (Process p in processes)
            {
                if (p.active)
                {
                    p.OnUpdate();
                }
            }
            if (activeInputProcess != null)
                activeInputProcess.OnUpdate();*/
        }
        
        public void Draw()
        {
            foreach (Process p in activeProcesses)
            {
                if (p.active)
                {
                    p.Draw();
                }
            }
        }


        //this will start it.... for now...
        public void Add(Process p)
        {
            if (!activeProcesses.Contains(p))
            {
                bool isIn = activeProcesses.Any((x) => x.GetType() == p.GetType());
                if (isIn) return;

                activeProcesses.Add(p);
                p.InvokeOnCreate();
            }
            //System.Console.WriteLine("heyo pre-emptive strike");
        }

        public void Remove(Process p)
        {
            p.InvokeOnDestroy();
            activeProcesses.Remove(p);
        }
    }
}
