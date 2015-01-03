using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace OrbItProcs
{
    public class Level
    {
        public Room room;
        public int cellsX { get; set; }
        public int cellsY { get; set; }

        public int cellWidth { get; set; }
        public int cellHeight { get; set; }

        public int gridWidth { get { return cellsX * cellWidth; } }
        public int gridHeight { get { return cellsY * cellWidth; } }

        public List<Rect> linesToDraw = new List<Rect>();

        public Level() { }

        public Level(Room room, int cellsX, int cellsY, int cellWidth, int? cellHeight = null)
        {
            this.room = room;
            this.cellsX = cellsX;
            this.cellsY = cellsY;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight ?? cellWidth;


        }


        public void addLevelLines()
        {
            for (int i = 0; i <= cellsX; i++)
            {
                int x = i * cellWidth;
                linesToDraw.Add(new Rect(x, 0, x, cellHeight*cellsY));
            }
            for (int i = 0; i <= cellsY; i++)
            {
                int y = i * cellHeight;
                linesToDraw.Add(new Rect(0, y, cellWidth*cellsX, y));
            }
        }
    }
}
