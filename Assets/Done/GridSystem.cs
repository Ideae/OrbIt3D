using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using UnityEngine;
namespace OrbItProcs {
    public class GridSystem {
        public Room room;
        public Vector2 position;
        public int cellsX { get; set; }
        public int cellsY { get; set; }

        public int cellWidth { get; set; }
        public int cellHeight { get; set; }
        public int gridWidth { get; set; }
        public int gridHeight { get; set; }

        public List<Collider>[,] grid;
        public HashSet<Collider> alreadyVisited;
        public List<Rect> linesToDraw = new List<Rect>();
        public GridSystem(Room room, int cellsX, Vector2 position, int? GridWidth = null, int? GridHeight = null)
        {
            linesToDraw = new List<Rect>();
            this.position = position;
            this.room = room;
            
            this.gridWidth = GridWidth ?? room.worldWidth;
            this.gridHeight = GridHeight ?? room.worldHeight;
            this.cellsX = cellsX;
            
            //this.cellReach = cellReach;
            cellWidth = (int)Math.Ceiling((double)gridWidth / (double)cellsX);
            cellHeight = cellWidth;
            this.cellsY = (gridHeight - 1)/ cellHeight + 1;
            //cellheight = gridheight / cellsY;
            grid = new List<Collider>[cellsX, cellsY];
            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
                {
                    grid[i, j] = new List<Collider>();
                }
            }
            
            //
            arrayGrid = new IndexArray<Collider>[cellsX][];
            for(int i = 0; i < cellsX; i++)
            {
                arrayGrid[i] = new IndexArray<Collider>[cellsY];
                for(int j = 0; j < cellsY; j++)
                {
                    arrayGrid[i][j] = new IndexArray<Collider>(100);
                }
            }
            bucketBags = new IndexArray<IndexArray<Collider>>[cellsX][];
            for (int i = 0; i < cellsX; i++)
            {
                bucketBags[i] = new IndexArray<IndexArray<Collider>>[cellsY];
                for (int j = 0; j < cellsY; j++)
                {
                    bucketBags[i][j] = new IndexArray<IndexArray<Collider>>(20);
                }
            }
            //
            distToOffsets = GenerateReachOffsetsDictionary();

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            int generateReach = gridWidth / 3;
            //GenerateAllReachOffsetsPerCoord(generateReach); //takes a shitload of time.. but only for the temproom?
            //stopwatch.Stop();
            //string taken = stopwatch.Elapsed.ToString();
            //Console.WriteLine("gridsystem generation time taken: " + taken);

            bucketLists = new List<List<Collider>>[cellsX, cellsY];

            ///new attempt
            GenerateReachOffsetsArray();
            

        }
        public IndexArray<Collider>[][] arrayGrid;
        //public IndexArray<Node>[][][] bucketBags;
        public IndexArray<IndexArray<Collider>>[][] bucketBags;


        public IndexArray<IndexArray<Collider>> retrieveBucketBags(Collider collider)
        {
            int effectiveX = (int)(collider.pos.x - position.x);
            int effectiveY = (int)(collider.pos.y - position.y);

            if (effectiveX < 0 || effectiveY < 0) return null;
            int x = effectiveX / cellWidth;
            int y = effectiveY / cellHeight;

            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return null;
            return bucketBags[x][y];
        }
        public void insertToBuckets(Collider collider, bool forceIntoGrid = true)
        {
            int effectiveX = (int)(collider.pos.x - position.x);
            int effectiveY = (int)(collider.pos.y - position.y);

            int x = effectiveX / cellWidth;
            int y = effectiveY / cellHeight;
            //if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return;
            if (x < 0)
            {
                if (forceIntoGrid) x = 0;
                else 
                    return;
            }
            else if (x >= cellsX)
            {
                if (forceIntoGrid) x = cellsX - 1;
                else 
                    return;
            }
            if (y < 0)
            {
                if (forceIntoGrid) y = 0;
                else 
                    return;
            }
            else if (y >= cellsY)
            {
                if (forceIntoGrid) y = cellsY - 1;
                else 
                    return;
            }
            arrayGrid[x][y].AddItem(collider);
        }
        public void clearBuckets()
        {
            for(int x = 0; x < cellsX; x++)
            {
                for(int y = 0; y < cellsY; y++)
                {
                    arrayGrid[x][y].index = 0;
                }
            }
        }
        //static int largest = 0;
        public void insert(Collider collider)
        {
            Tuple<int, int> indexs = getIndexs(collider);
            //if (node == room.game.targetNode) Console.WriteLine("target indexs: {0} {1}",indexs.Item1,indexs.Item2);
            grid[indexs.Item1, indexs.Item2].Add(collider);
            //if (grid[indexs.Item1, indexs.Item2].Count > largest)
            //{
            //    largest = grid[indexs.Item1, indexs.Item2].Count;
            //    //Console.WriteLine(largest);
            //}
            //grid[indexs.Item1, indexs.Item2].ToArray();
        }




        //new attempt : april 14 2014   START
        public int[] offsetArray;
        public int[] reachIndexs;

        public void GenerateReachOffsetsArray()
        {
            SortedDictionary<float, List<Tuple<int, int>>> offsets = new SortedDictionary<float, List<Tuple<int, int>>>();
            int fullLength = 0;
            for (int x = 0; x < cellsX; x++)
            {
                for (int y = 0; y < cellsY; y++)
                {
                    float dist = (float)Math.Sqrt(x * x + y * y) * cellWidth;
                    if (!offsets.ContainsKey(dist)) offsets.Add(dist, new List<Tuple<int, int>>());

                    offsets[dist].Add(new Tuple<int, int>(x, y));
                    fullLength++;

                    if (x == 0 && y > 0)
                    {
                        offsets[dist].Add(new Tuple<int, int>(x, -y));
                        fullLength++;
                    }
                    else if (x > 0 && y == 0)
                    {
                        offsets[dist].Add(new Tuple<int, int>(-x, y));
                        fullLength++;
                    }
                    else if (x > 0 && y > 0)
                    {
                        offsets[dist].Add(new Tuple<int, int>(-x, y));
                        offsets[dist].Add(new Tuple<int, int>(x, -y));
                        offsets[dist].Add(new Tuple<int, int>(-x, -y));
                        fullLength += 3;
                    }
                }
            }

            offsetArray = new int[fullLength * 2]; // + 10 for... safety?
            reachIndexs = new int[offsets.Keys.Count];
            int indexCount = 0;
            int reachIndexCount = 0;
            foreach(float dist in offsets.Keys)
            {
                foreach(Tuple<int, int> tup in offsets[dist])
                {
                    offsetArray[indexCount++] = tup.Item1;
                    offsetArray[indexCount++] = tup.Item2;
                }
                reachIndexs[reachIndexCount++] = indexCount - 2;
            }
        }
        public void retrieveOffsetArraysCollision(Collider collider, Action<Collider, Collider> action, float distance)
        {

            int effectiveX = (int)(collider.pos.x - position.x);
            int effectiveY = (int)(collider.pos.y - position.y);

            int x = effectiveX / cellWidth;
            int y = effectiveY / cellHeight;
            if (x < 0 || x > cellsX || y < 0 || y > cellsY) return;
            int findcount = FindCount(distance);
            int lastIndex;
            //lastIndex = reachIndexs[reachCount];
            lastIndex = reachIndexs[findcount];
            for (int coordPointer = 0; coordPointer <= lastIndex; coordPointer += 2)
            {
                int xx = offsetArray[coordPointer] + x; 
                int yy = offsetArray[coordPointer + 1] + y;
                if (xx < 0 || xx >= cellsX || yy < 0 || yy >= cellsY) continue;
                IndexArray<Collider> buck = arrayGrid[xx][yy];
                int count = buck.index;
                for (int j = 0; j < count; j++)
                {
                    Collider c = arrayGrid[xx][yy].array[j];
                    //if (room.ColorNodesInReach && collider.parent == room.targetNode) c.parent.body.color = Color.Purple;
                    if (alreadyVisited.Contains(c) || collider == c) continue;
                    action(collider, c);
                }
            }
        }

        public void retrieveOffsetArraysAffect(Collider collider, Action<Collider, Collider> action, float distance)
        {
            int effectiveX = (int)(collider.pos.x - position.x);
            int effectiveY = (int)(collider.pos.y - position.y);
            int x = effectiveX / cellWidth;
            int y = effectiveY / cellHeight;
            if (x < 0 || x > cellsX || y < 0 || y > cellsY) return;
            int findcount = FindCount(distance);
            int lastIndex;
            //lastIndex = reachIndexs[reachCount];
            lastIndex = reachIndexs[findcount];
            for (int coordPointer = 0; coordPointer <= lastIndex; coordPointer += 2)
            {
                int xx = offsetArray[coordPointer] + x;
                int yy = offsetArray[coordPointer + 1] + y;
                if (xx < 0 || xx >= cellsX || yy < 0 || yy >= cellsY) continue;
                IndexArray<Collider> buck = arrayGrid[xx][yy];
                int count = buck.index;
                for (int j = 0; j < count; j++)
                {
                    Collider c = buck.array[j];// = arrayGrid[xx][yy].array[j];
                    //if (room.ColorNodesInReach && collider.parent == room.targetNode) buck.array[j].parent.body.color = Color.Purple;
                    //if (alreadyVisited.Contains(c) || collider == c) continue;
                    if (c == collider) continue;
                    action(collider, c);
                }
            }
        }

        // /new attempt april 14 2014     END

        //todo: save indexs of nodes upon insertion (duh.)
        // save 3d lookup table of retrieveBuckets lists (per x,y,reach)
        // 
        SortedDictionary<float, List<Tuple<int, int>>> distToOffsets;
        public SortedDictionary<float, List<Tuple<int, int>>> GenerateReachOffsetsDictionary()
        {
            SortedDictionary<float, List<Tuple<int, int>>> offsets = new SortedDictionary<float, List<Tuple<int, int>>>();
            for (int x = 0; x < cellsX; x++)
            {
                for (int y = 0; y < cellsY; y++)
                {
                    float dist = (float)Math.Sqrt(x * x + y * y) * cellWidth;
                    if (!offsets.ContainsKey(dist)) offsets.Add(dist, new List<Tuple<int, int>>());

                    offsets[dist].Add(new Tuple<int, int>(x, y));

                    if (x == 0 && y > 0)
                    {
                        offsets[dist].Add(new Tuple<int, int>(x, -y));
                    }
                    else if (x > 0 && y == 0)
                    {
                        offsets[dist].Add(new Tuple<int, int>(-x, y));
                    }
                    else if (x > 0 && y > 0)
                    {
                        offsets[dist].Add(new Tuple<int, int>(-x, y));
                        offsets[dist].Add(new Tuple<int, int>(x, -y));
                        offsets[dist].Add(new Tuple<int, int>(-x, -y));
                    }
                }
            }
            return offsets;
        }
        public SortedDictionary<float, List<Tuple<int, int>>>[,] offsetsArrayCollection;

        public void GenerateAllReachOffsetsPerCoord(float maxdist = float.MaxValue)
        {
            //GC.GetTotalMemory(true);
            offsetsArrayCollection = new SortedDictionary<float, List<Tuple<int, int>>>[cellsX, cellsY];
            for (int x = 0; x < cellsX; x++)
            {
                for (int y = 0; y < cellsY; y++)
                {
                    offsetsArrayCollection[x,y] = new SortedDictionary<float, List<Tuple<int, int>>>();
                    foreach(float dist in distToOffsets.Keys)
                    {
                        if (dist > maxdist) break;
                        foreach(Tuple<int,int> tuple in distToOffsets[dist])
                        {
                            if (tuple.Item1 + x < 0 || tuple.Item1 + x >= cellsX || tuple.Item2 + y < 0 || tuple.Item2 + y >= cellsY) continue;
                            if (!offsetsArrayCollection[x, y].ContainsKey(dist)) offsetsArrayCollection[x, y][dist] = new List<Tuple<int, int>>();
                            offsetsArrayCollection[x, y][dist].Add(tuple);
                            //add bucket to bucketBag
                            bucketBags[x][y].AddItem(arrayGrid[x + tuple.Item1][y + tuple.Item2]);
                        }
                    }
                    bucketBags[x][y].index = 15; //determins global reach
                }
            }
        }
        public void retrieveFromAllOffsets(Collider collider, float reachDistance, Action<Collider, Collider> action)
        {
            int x = (int)collider.pos.x / cellWidth;
            int y = (int)collider.pos.y / cellHeight;
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return;
            int findcount = FindCount(reachDistance);
            for (int i = 0; i < findcount; i++)
            {
                foreach (var tuple in distToOffsets.ElementAt(i).Value)
                {
                    int xx = tuple.Item1 + x; int yy = tuple.Item2 + y;
                    if (xx < 0 || xx >= cellsX || yy < 0 || yy >= cellsY) continue;
                    //foreach (Collider c in grid[tuple.Item1, tuple.Item2])
                    //{
                    //    action(collider, c);
                    //}
                    IndexArray<Collider> buck = arrayGrid[xx][yy];
                    int count = buck.index;
                    //if (count > 0) Console.WriteLine(count);
                    for (int j = 0; j < count; j++)
                    {
                        Collider c = arrayGrid[xx][yy].array[j];
                        if (alreadyVisited.Contains(c) || collider == c) continue;
                        action(collider, c);
                    }
                }
            }
        }
        public List<List<Collider>>[,] bucketLists;
        public int preexistingCounter = 0;
        public List<List<Collider>> retrieveBuckets(Collider collider, float reachDistance)
        {
            int x = (int)collider.pos.x / cellWidth;
            int y = (int)collider.pos.y / cellHeight;
            
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY)
            {
                return null;
            }
            else
            {
                if (bucketLists[x, y] != null)
                {
                    preexistingCounter++;
                    return bucketLists[x, y];
                }
                bucketLists[x, y] = new List<List<Collider>>();

                int count = FindCount(reachDistance);
                var dict = offsetsArrayCollection[x, y];
                if (dict.Count <= count)
                {
                    throw new SystemException("Count too big exception");
                }
                //int cellsHit = 0;
                for (int i = 0; i < count; i++)
                {
                    List<Tuple<int, int>> tuples = dict.ElementAt(i).Value;
                    foreach (var tuple in tuples)
                    {
                        bucketLists[x, y].Add(grid[tuple.Item1 + x, tuple.Item2 + y]);
                    }
                }
                //Console.WriteLine(cellsHit);
                return bucketLists[x, y];
            }
        }

        public void retrieveFromOptimizedOffsets(Collider collider, float reachDistance, Action<Node> action)
        {
            int x = (int)collider.pos.x / cellWidth;
            int y = (int)collider.pos.y / cellHeight;
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return;
            int count = FindCount(reachDistance);
            var dict = offsetsArrayCollection[x, y];
            if (dict.Count <= count)
            {
                throw new SystemException("Count too big exception");
            }
            //int cellsHit = 0;
            for (int i = 0; i < count; i++)
            {
                foreach (var tuple in dict.ElementAt(i).Value)
                {
                    foreach (Collider c in grid[tuple.Item1 + x, tuple.Item2 + y])
                    {
                        action(c.parent);
                    }
                    //cellsHit++;
                }
            }
            //Console.WriteLine(cellsHit);
        }

        public Dictionary<float, int> distToCount = new Dictionary<float, int>();
        public void retrieveNew(Collider collider, float reachDistance, Action<Node> action)
        {
            int x = (int)collider.pos.x / cellWidth;
            int y = (int)collider.pos.y / cellHeight;
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return;

            foreach(float dist in distToOffsets.Keys)
            {
                if (dist > reachDistance) break;
                foreach(var tuple in distToOffsets[dist])
                {
                    foreach (Collider c in grid[x + tuple.Item1, y + tuple.Item2])
                    {
                        action(c.parent);
                        
                    }
                }
            }
        }

        public int FindCount(float dist)
        {
            if (distToCount.ContainsKey(dist)) return distToCount[dist];
            int count = 0;
            foreach(float f in distToOffsets.Keys)
            {
                count++;
                if (f > dist)
                {
                    distToCount[dist] = count;
                    return count;
                }

            }
            return count;
        }


        public void PrintOffsets(int max = int.MaxValue, bool printOffsets = true, int x = -1, int y = -1)
        {
            var offsets = distToOffsets;
            if (x >= 0 && y >= 0)
            {
                offsets = offsetsArrayCollection[x, y];
            }
            Console.WriteLine(" ::::" + cellsX + "," + cellsY + "\t");
            int c = 0;
            foreach (var f in offsets.Keys)
            {
                if (c++ > max) break;
                Console.Write("{0,10} ", f);
                if (!printOffsets)
                {
                    Console.WriteLine();
                    continue;
                }
                foreach (var t in offsets[f])
                {
                    string s = t.Item1 + "," + t.Item2;
                    Console.Write("{0,10} ", s);
                }
                Console.WriteLine();
            }
        }

        

        // gets the index of the node in the gridsystem, without correcting out of bounds nodes.
        public Tuple<int, int> getIndexsNew(Collider collider)
        {
            //int a = (int)node.transform.position.x / cellWidth;
            return new Tuple<int, int>((int)collider.pos.x / cellWidth, (int)collider.pos.y / cellHeight);
        }

        public void testRetrieve(int x, int y, int reach)
        {
            Console.WriteLine("Retrieve at: {0},{1}\tReach: {2}", x, y, reach);
            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
                {
                    // Wow. Never use Math class >.<
                    //double dist = Math.Pow(Math.Abs(x - i), 2) + Math.Pow(Math.Abs(y - j), 2);
                    int xd = (x - i) * (x - i);
                    if (xd < 0) xd *= -1;
                    int yd = (y - j) * (y - j);
                    if (yd < 0) yd *= -1;
                    int dist = xd + yd;

                    if (dist <= reach * reach)
                    {
                        Console.Write(i + "," + j + "\t");
                    }
                }
            }
            Console.WriteLine();
        }

        public List<Collider> retrieve(Collider collider, int reach = -1)
        {
            //CountArray<Node>[,] nodes;

            //if (reach == -1) reach = cellReach;
            if (reach == -1) reach = 5;
            List<Collider> returnList = new List<Collider>();
            Tuple<int, int> indexs = getIndexs(collider);
            int x = indexs.Item1;
            int y = indexs.Item2;
            //grid[indexs.Item1, indexs.Item2].Add(node);
            int xbegin, xend, ybegin, yend;
            xbegin = x - reach;
            xend = x + reach + 1;
            if (xbegin < 0) xbegin = 0;
            if (xend > cellsX) xend = cellsX;

            ybegin = y - reach;
            yend = y + reach + 1;
            if (ybegin < 0) ybegin = 0;
            if (yend > cellsY) yend = cellsY;
            //return box of slots
            /*
            for (int i = xbegin; i < xend; i++)
            {
                for (int j = ybegin; j < yend; j++)
                {
                    //grid[i, j] = new List<Node>();
                    //returnList.AddRange(grid[i, j]);
                    foreach (Node n in grid[i, j])
                    {
                        returnList.Add(n);
                    }
                }
            }
            */
            //return circle of slots
            ///*
            //int cellsHit = 0;
            for (int i = xbegin; i < xend; i++)
            {
                for (int j = ybegin; j < yend; j++)
                {
                    // Wow. Never use Math class >.<
                    //double dist = Math.Pow(Math.Abs(x - i), 2) + Math.Pow(Math.Abs(y - j), 2);
                    int xd = (x - i) * (x - i);
                    if (xd < 0) xd *= -1;
                    int yd = (y - j) * (y - j);
                    if (yd < 0) yd *= -1;
                    int dist = xd + yd;

                    if (dist <= reach * reach)
                    {
                        returnList.AddRange(grid[i, j]);
                        //foreach (Node n in grid[i, j])
                        //{
                        //    returnList.Add(n);
                        //}
                        //cellsHit++;
                    }

                }
                
            }
            //Console.WriteLine("Cells hit: {0}", cellsHit);
            ///*
            //Console.WriteLine("xbegin:{0} + xend:{1} + ybegin:{2} + yend:{3}", xbegin, xend, ybegin, yend);
            return returnList;

        }

        public Tuple<int, int> getIndexs(Collider collider)
        {
            Vector2 pos = new Vector2(collider.pos.x, collider.pos.y);
            int x = (int)collider.pos.x;
            int y = (int)collider.pos.y;
            int gridx = (int)pos.x - ((int)pos.x % cellWidth);
            x = gridx / cellWidth;
            //if ((int)pos.x - gridx > gridx + cellwidth - (int)node.transform.radius) x++;
            int gridy = (int)pos.y - ((int)pos.y % cellHeight);
            y = gridy / cellHeight;
            //if ((int)pos.y - gridy > gridy + cellheight - (int)node.transform.radius) y++;

            if (x > cellsX - 1) x = cellsX - 1;
            if (y > cellsY - 1) y = cellsY - 1;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            //Console.WriteLine("{0} + {1}", x, y);
            return new Tuple<int, int>(x, y);
        }

        public void clear()
        {
            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
                {
                    grid[i, j] = new List<Collider>();
                    //grid[i, j].RemoveRange(0, grid[i, j].Count);
                    bucketLists[i, j] = null;
                }
            }
            //Console.WriteLine(preexistingCounter);
            preexistingCounter = 0;
        }

        public bool ContainsCollider(Collider collider)
        {
            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
                {
                    if (grid[i, j].Contains(collider)) return true;
                }
            }
            return false;
        }

        // color the nodes that targetnode is affecting
        public void colorEffectedNodes()
        {
            // coloring the nodes
            if (room.targetNode != null)
            {
                List<Collider> returnObjectsGridSystem = retrieve(room.targetNode.body);

                foreach (Node _node in room.masterGroup.fullSet)
                {
                    if (_node.body.color != Color.black)
                    {
                        if (returnObjectsGridSystem.Contains(_node.body))
                            _node.body.color = Color.magenta;
                        else
                            _node.body.color = Color.white;
                    }
                }
                room.targetNode.body.color = Color.red;
            }
        }


        public void DrawGrid(Room room, Color color)
        {
            room.camera.drawGrid(linesToDraw, color);
            linesToDraw = new List<Rect>();
        }

        //draw grid lines
        public void addGridSystemLines(GridSystem gs)
        {
            for (int i = 0; i <= gs.cellsX; i++)
            {
                int x = i * gs.cellWidth + (int)gs.position.x;
                linesToDraw.Add(new Rect(x, (int)gs.position.y, x, gs.gridHeight + (int)gs.position.y));
            }
            for (int i = 0; i <= gs.cellsY; i++)
            {
                int y = i * gs.cellHeight + (int)gs.position.y;
                linesToDraw.Add(new Rect((int)gs.position.x, y, gs.gridWidth + (int)gs.position.x, y));
            }
        }

        

        public void addRectangleLines(int x, int y, int width, int height)
        {
            linesToDraw.Add(new Rect(x, y, width, y));
            linesToDraw.Add(new Rect(x, y, x, height));
            linesToDraw.Add(new Rect(x, height, width, height));
            linesToDraw.Add(new Rect(width, y, width, height));
        }
    }

}
