using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace OrbItProcs
{
    public class SpawnNodes : Process
    {
        private Vector2 spawnPos;
        int rightClickCount = 0;//
        int rightClickMax = 1;//
        public int batchSpawnNum { get; set; }

        public Toggle<float> radiusRange { get; set; }
        public float radiusCenter { get; set; }

        public SpawnNodes() : base()
        {
            batchSpawnNum = 2;
            radiusRange = new Toggle<float>(10f, true);
            radiusCenter = 15f;

            addProcessKeyAction("SpawnNode", KeyCode.Mouse0, OnPress: SpawnNode);
            //addProcessKeyAction("SetSpawnPosition", KeyCodes.LeftShift, OnPress: SetSpawnPosition);
            addProcessKeyAction("BatchSpawn", KeyCode.Mouse1, OnHold: BatchSpawn);
            //addProcessKeyAction("DirectionalLaunch", KeyCodes.LeftShift, KeyCodes.RightClick, OnHold: DirectionalLaunch);
            addProcessKeyAction("testing", KeyCode.Slash, OnPress: TestingStuff);
            addProcessKeyAction("removeall", KeyCode.Delete, OnPress: RemoveAll);
            addProcessKeyAction("removeall", KeyCode.Escape, OnPress: RemoveAll2);
        }

        public void SetRadius(Node n)
        {
            n.radius = radiusCenter;
            if (radiusRange.enabled)
            {
                n.radius = (float)Utils.random.NextDouble() * radiusRange.value - (radiusRange.value / 2) + radiusCenter;
            }
            //
            //n.body.angularVelocity = 2;
            //n.collision.active = false;
            n.addComponent<Gravity>(true);
        }
        public void RemoveAll()
        {
            if (room.activeGroup == null) return;
            if (room.targetNode != null)
                if (room.activeGroup.fullSet.Contains(room.targetNode)) room.targetNode = null;
            //int size = g.fullSet.Count, count = 0;
            room.activeGroup.EmptyGroup();
        }
        public void RemoveAll2()
        {
            
            if (room.activeGroup.fullSet.Count == 0)
            {
                Application.Quit();
            }
            RemoveAll();
        }

        public void SpawnNode()
        {
            Node n = room.spawnNode((int)OInput.WorldMousePos.x, (int)OInput.WorldMousePos.y);
            SetRadius(n);
        }
        #region Testing Region
        public void TestingStuff()
        {
            //
            //room.game.testing.TestOnClick();
            //room.game.testing.TestHashSet();
            //room.game.testing.WhereTest();
            //room.game.testing.ForLoops();
            //room.game.testing.ColorsTest();
            //room.game.testing.NormalizeTest();
            //DateTime before = DateTime.Now;
            //room.game.testing.LoopTesting();
            //
            //int diff = DateTime.Now.Millisecond - before.Millisecond;
            //if (diff < 0)  diff += 1000;
            //Console.WriteLine("DIFF:" + diff);
            //

            //testing.TriangleTest2();
            //testing.TestRedirect();
            //room.game.testing.Gridsystem();
            //room.gridsystem.GenerateReachOffsets();
            //room.gridsystem.PrintOffsets(10);
            //long totalMemory = GC.GetTotalMemory(true);
            //Console.WriteLine(totalMemory);
            //room.gridsystem.GenerateAllReachOffsetsPerCoord(300);
            //long totalMemoryNew = GC.GetTotalMemory(true);
            //Console.WriteLine(totalMemoryNew);
            //
            //float m1 = Testing.ByteToMegabyte((int)totalMemory);
            //float m2 = Testing.ByteToMegabyte((int)totalMemoryNew);
            //Console.WriteLine("{0} - {1} = {2}", m2, m1, m2 - m1);
            //room.gridsystem.PrintOffsets(max: 300, x: 0, y: 0);
            //Testing.IndexsGridsystem();
            //Testing.TestCountArray();
        }
        #endregion

        public void SetSpawnPosition()
        {
            spawnPos = OInput.WorldMousePos;
        }
        public void BatchSpawn()
        {
            int rad = 100;
            for (int i = 0; i < batchSpawnNum; i++)
            {
                int rx = Utils.random.Next(rad * 2) - rad;
                int ry = Utils.random.Next(rad * 2) - rad;
                SetRadius(room.spawnNode((int)OInput.WorldMousePos.x + rx, (int)OInput.WorldMousePos.y + ry));
            }
        }

        public void DirectionalLaunch()
        {
            rightClickCount++;
            if (rightClickCount % rightClickMax == 0)
            {
                //Vector2 positionToSpawn = new Vector2(Game1.sWidth, Game1.sHeight);
                Vector3 positionToSpawn = spawnPos;
                //positionToSpawn /= (game.room.mapzoom * 2);
                //positionToSpawn /= (2);
                Vector3 diff = OInput.WorldMousePos;
                //diff *= room.zoom;
                diff = diff - positionToSpawn;
                //diff.Normalize();

                //new node(s)
                Dictionary<object, object> userP = new Dictionary<object, object>() {
                            { typeof(Lifetime), true },
                            { nodeE.position, positionToSpawn },
                            { nodeE.velocity, diff },
                    };

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    Action<Node> after = delegate(Node n) { 
                        n.rigidbody.velocity = diff;
                        if (n.rigidbody.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
                    
                    }; 
                    SetRadius(room.spawnNode(userP, after));
                }
                else
                {
                    SetRadius(room.spawnNode(userP));
                }
                rightClickCount = 0;
            }
        }
    }
}
