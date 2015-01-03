using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbItProcs
{
    public struct KeyBundle
    {
        //public static KeyMouse km(KeyCodes k ) { return new KeyMouse(k); } //to be deleted
        public KeyCode? effectiveKey;
        public KeyCode? mod1;
        public KeyCode? mod2;

        public KeyBundle(KeyCode effectiveKey, KeyCode? mod1 = null, KeyCode? mod2 = null)
        {
            this.effectiveKey = effectiveKey;

            this.mod1 = mod1;
            this.mod2 = mod2;

            if (mod1 == null)
            {
                if (mod2 != null) { this.mod1 = mod1; this.mod2 = mod2; throw new ArgumentException(); }
                
            }
            else
            {
                if (mod2 != null)
                {
                    //this.mod2 = mod2;
                    if (mod1 > mod2)
                    {
                        this.mod1 = mod2;
                        this.mod2 = mod1;
                    }
                }
            }
        }

        public KeyBundle(List<KeyCode> list)
        {
            effectiveKey = mod1 = mod2 = null;
            for (int i = list.Count - 1; i >= 0; i--)
            //for (int i = 0; i < list.Count; i++)
            {
                if (effectiveKey == null) effectiveKey = list.ElementAt(i);
                else if (mod1 == null) mod1 = list.ElementAt(i);
                else if (mod2 == null) mod2 = list.ElementAt(i);
            }
        }

        public bool isKey()
        {
            return (int)effectiveKey < 255;
        }
    }

    public class KeyAction
    {
        public string name = "Missing Name";
        public Action pressAction;
        public Action releaseAction;
        public Action holdAction;

        public HashSet<KeyBundle> bundles;

        public KeyAction(string name, Action action, HashSet<KeyBundle> bundles)
        {
            this.name = name;
            this.pressAction = action;
            if (bundles == null) throw new ArgumentException("bundles was null - {0}", name);
            this.bundles = bundles;
        }

        //public KeyAction(string name, Action action, KeyBundle bundle)
        //{
        //    this.name = name;
        //    this.pressAction = action;
        //    this.bundles = new HashSet<KeyBundle>();
        //    this.bundles.Add(bundle);
        //}
        //public KeyAction(HashSet<KeyBundle> bundles, string name, Action pressAction = null, Action releaseAction = null, Action holdAction = null)
        //{
        //    this.name = name;
        //    this.pressAction = pressAction;
        //    this.releaseAction = releaseAction;
        //    this.holdAction = holdAction;
        //    if (bundles == null) throw new ArgumentException("bundles was null - {0}", name);
        //    this.bundles = bundles;
        //}
        //public KeyAction(KeyBundle bundles, string name, Action pressAction = null, Action releaseAction = null, Action holdAction = null)
        //{
        //    this.name = name;
        //    this.pressAction = pressAction;
        //    this.releaseAction = releaseAction;
        //    this.holdAction = holdAction;
        //    this.bundles = new HashSet<KeyBundle>();
        //    this.bundles.Add(bundles);
        //}


    }

    public enum KeySwitchMethod
    {
        Overwrite,
        Switch,
        TempSwitch,
        TempOverwrite,
    }

    //================================================== KEY MANAGER ==========================================
    public class KeyManager
    {
        public static int HoldCounter = 0;

        public Dictionary<KeyBundle, KeyAction> PressedBundles = new Dictionary<KeyBundle, KeyAction>();

        //public UserInterface ui;

        public List<Process> PermanentProcesses = new List<Process>();
        public Process TemporaryProcess = null;

        public Dictionary<KeyBundle, KeyAction> _Keybinds = new Dictionary<KeyBundle, KeyAction>();
        //20 references
        public Dictionary<KeyBundle, KeyAction> Keybinds { get { return _Keybinds; } set { _Keybinds = value; } }

        public Stack<KeyCode> PressedKeysS = new Stack<KeyCode>(); //max of three KeyMouse detected at a time
        public List<KeyCode> PressedKeys = new List<KeyCode>();

        public bool MouseInGameBox = false;

        //don't call me generic
        public Dictionary<Process, List<Tuple<KeyBundle, KeyAction>>> ReplacedBundles = new Dictionary<Process, List<Tuple<KeyBundle, KeyAction>>>();

        public KeyManager(Dictionary<KeyBundle, KeyAction> Keybinds = null)
        {
            if (Keybinds != null)
            {
                this.Keybinds = Keybinds;
            }
        }

        public void Add(string name, KeyBundle bundle, Action action, Action holdAction = null)
        {
            KeyAction ka = new KeyAction(name, action, new HashSet<KeyBundle>() { bundle });
            Keybinds.Add(bundle, ka);
        }
        public void Add(string name, HashSet<KeyBundle> bundles, Action action)
        {
            KeyAction ka = new KeyAction(name, action, bundles);
            
            foreach(KeyBundle b in bundles)
            {
                Keybinds.Add(b, ka);
            }
        }

        public void AddProcess(Process p, KeySwitchMethod switchmethod)
        {
            if (p != null && p.processKeyActions != null)
            {
                
                if (switchmethod.In(KeySwitchMethod.Switch, KeySwitchMethod.TempOverwrite, KeySwitchMethod.TempSwitch))
                {
                    throw new NotImplementedException("Don't make a fus, implement your bus.");
                }

                foreach (KeyAction a in p.processKeyActions.Keys)
                {
                    Keybinds[p.processKeyActions[a]] = a;
                }
            }

            Input.GetKeyDown(KeyCode.F);

        }
        public void AddProcess(ProcessManager pm, Process p, bool Temporary = true)
        {
            if (p == null) throw new SystemException("Process parameter was null"); //well ya see kids...
            if (p.processKeyActions == null) throw new SystemException("Process parameter had no keyactions");
            p.active = true;
            if (Temporary)
            {
                if (TemporaryProcess != p)
                {
                    //remove current temporary process and reinstate the keybinds it had replaced when it was added
                    RemoveTemporaryProcess(pm);

                    ReplacedBundles[p] = new List<Tuple<KeyBundle, KeyAction>>();
                    foreach (KeyAction a in p.processKeyActions.Keys)
                    {

                        //store previous keyaction in replacedbundles
                        if (Keybinds.ContainsKey(p.processKeyActions[a]))
                        {
                            ReplacedBundles[p].Add(new Tuple<KeyBundle, KeyAction>(p.processKeyActions[a], Keybinds[p.processKeyActions[a]]));
                        }
                        //insert new keybind
                        Keybinds[p.processKeyActions[a]] = a;
                    }
                    TemporaryProcess = p;
                    if (!pm.activeProcesses.Contains(p))
                        pm.activeProcesses.Add(p);
                }
            }
            else //permanent process
            {
                if (!PermanentProcesses.Contains(p))
                {
                    PermanentProcesses.Add(p);
                    foreach (KeyAction a in p.processKeyActions.Keys)
                    {
                        //insert new keybind
                        Keybinds[p.processKeyActions[a]] = a;
                    }
                }
            }
            //if (ui.sidebar != null) ui.sidebar.UpdateProcessView();
        }

        public void RemoveTemporaryProcess(ProcessManager pm)
        {
            if (TemporaryProcess == null) return;//throw new SystemException("Temporary process was null");
            TemporaryProcess.active = false;
            foreach(KeyAction ka in TemporaryProcess.processKeyActions.Keys)
            {
                KeyBundle kb = TemporaryProcess.processKeyActions[ka];
                if (Keybinds.ContainsKey(kb) && ka == Keybinds[kb])
                {
                    Keybinds.Remove(TemporaryProcess.processKeyActions[ka]);
                }
            }
            if (ReplacedBundles.ContainsKey(TemporaryProcess))
            {
                List<Tuple<KeyBundle, KeyAction>> list = ReplacedBundles[TemporaryProcess];
                foreach(Tuple<KeyBundle, KeyAction> tup in list)
                {
                    if (Keybinds.ContainsKey(tup.Item1)) continue; //maybe shouldn't do this; doesn't replace if something took the slot
                    Keybinds.Add(tup.Item1, tup.Item2);
                }
            }
            pm.activeProcesses.Remove(TemporaryProcess);

            ReplacedBundles.Remove(TemporaryProcess);
            TemporaryProcess = null; //maybe disable temporary process? or should we let caller do that
            
        }


        // processes can now spawn their own keyactions
        //public void Add(KeyBundle keyarr, Process process, bool AddBothCombinations = false)
        //{
        //    Keybinds.Add(keyarr, process);
        //    if (AddBothCombinations && keyarr.mod2 != null) //adds the process to both combinations (123, 213)
        //    {
        //        KeyBundle kb = new KeyBundle(keyarr.mod1, keyarr.effectiveKey, keyarr.mod2);
        //        Keybinds.Add(kb, process);
        //    }
        //}

        //public void Add(KeyBundle keyarr, Action action, bool AddBothCombinations = false)
        //{
        //    Keybinds.Add(keyarr, action);
        //    if (AddBothCombinations && keyarr.mod2 != null) //adds the action to both combinations (123, 213)
        //    {
        //        KeyBundle kb = new KeyBundle(keyarr.mod1, keyarr.effectiveKey, keyarr.mod2);
        //        Keybinds.Add(kb, action);
        //    }
        //}
        public void Update()
        {
            //if (!ui.SidebarActive)
            //{
            //    MouseInGameBox = true;
            //}
            //else
            //{
            //    MouseInGameBox = newMouseState.X > OrbIt.game.room.camera.CameraOffset;
            //}
            MouseInGameBox = Input.mousePresent;

            if (MouseInGameBox) //todo:check that the game window is active
            {
                ProcessInputs();
                ProcessHolds();
            }

            foreach(var p in PermanentProcesses)
            {
                //if (p.active)
                    p.Update();
            }
            if (TemporaryProcess != null) // && TemporaryProcess.active
            {
                TemporaryProcess.Update();
            }
        }

        public void ProcessHolds()
        {
            foreach (KeyBundle kb in PressedBundles.Keys.ToList())
            {
                if (Input.GetKey((KeyCode)kb.effectiveKey))
                {
                    if (PressedBundles[kb].holdAction != null) PressedBundles[kb].holdAction();
                }
                else
                {
                    if (PressedBundles[kb].releaseAction != null) PressedBundles[kb].releaseAction();
                    PressedBundles.Remove(kb);
                }
            }
        }
        public void ProcessInputs()
        {
            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(k))
                {
                    KeyCode kc = (KeyCode)k;
                    //Console.WriteLine(kc); //======================//======================//======================//======================//======================
                    if (!PressedKeys.Contains(kc) && PressedKeys.Count < 3)
                    {
                        //PressedKeys.Push(kc);
                        PressedKeys.Add(kc);
                        TryAction();
                    }
                    //PrintPressedKeys();
                    
                }
            }

            for (int i = PressedKeys.Count - 1; i >= 0; i--)
            {
                KeyCode key = PressedKeys.ElementAt(i);
                if (Input.GetKeyUp((KeyCode)key))
                {
                    //while (PressedKeys.ElementAt(PressedKeys.Count) != key) PressedKeys.Pop();
                    //PressedKeys.Pop();
                     PressedKeys.Remove(key);
                    //PrintPressedKeys(true);
                    break;
                }
            }

            
        }

        public void TryAction()
        {
            
            KeyBundle kb = new KeyBundle(PressedKeys);
            string m1 = "none"; string m2 = "none";
            if (kb.mod1 != null) m1 = ((KeyCode)kb.mod1).ToString();
            if (kb.mod2 != null) m2 = ((KeyCode)kb.mod2).ToString();
            //Console.WriteLine("ex: {0}\t\tmod1: {1}\t\tmod2: {2}", kb.effectiveKey, m1, m2);
            //Console.WriteLine("ex: {0}\t\tmod1: {1}\t\tmod2: {2} \t\t extra: {3}", PressedKeys.ElementAt(0), PressedKeys.ElementAt(1), PressedKeys.ElementAt(2), PressedKeys.ElementAt(3));
            //Console.WriteLine("ex: {0}", PressedKeys.Count);
            if (!Keybinds.ContainsKey(kb))
            {
                KeyCode? temp = null;
                
                if (kb.mod2 != null) 
                {
                    temp = kb.mod2;
                    kb.mod2 = null;
                }

                if (!Keybinds.ContainsKey(kb))
                {
                    if (kb.mod1 != null) kb.mod1 = temp;
                    if (!Keybinds.ContainsKey(kb))
                    {
                        kb.mod1 = null;
                        if (!Keybinds.ContainsKey(kb))
                        {
                            return;
                        }
                    }
                }
            }
            
            KeyAction ka = Keybinds[kb];
            if (ka != null)
            {

                if (MouseInGameBox || (kb.effectiveKey != KeyCode.Mouse0 && kb.effectiveKey != KeyCode.Mouse1 && kb.effectiveKey != KeyCode.Mouse2))
                {
                    
                    if (ka.pressAction != null)
                    {
                        ka.pressAction();
                    }
                    //if (!PressedBundles.ContainsKey(kb)) 
                    PressedBundles.Add(kb, ka); //exception
                }
                
            }
        }

        public void PrintPressedKeys(bool released = false)
        {
            string s = "";
            if (released) s += "Release: "; else s += "Pressed: ";

            //for (int i = PressedKeys.Count - 1; i >= 0; i--)
            for (int i = 0; i < PressedKeys.Count; i++)
            {
                s += PressedKeys.ElementAt(i) + " ";
            }

            Console.WriteLine(s);
        }
        public void addGlobalKeyAction(String name, KeyCode k1, KeyCode? k2 = null, KeyCode? k3 = null, Action OnPress = null, Action OnRelease = null, Action OnHold = null)
        {
            KeyBundle keyBundle;
            if (k2 == null) keyBundle = new KeyBundle(k1);
            else if (k3 == null) keyBundle = new KeyBundle((KeyCode)k2, k1);
            else keyBundle = new KeyBundle((KeyCode)k3, (KeyCode)k2, k1);

            var keyAction = new KeyAction(name, OnPress, new HashSet<KeyBundle>() { keyBundle });

            keyAction.releaseAction = OnRelease;
            keyAction.holdAction = OnHold;

            Keybinds.Add(keyBundle, keyAction);
        }

    }
}
