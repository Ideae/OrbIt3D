using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;


namespace OrbItProcs
{
    public enum Layers
    {
        Under5 = 5,
        Under4 = 4,
        Under3 = 3,
        Under2 = 2,
        Under1 = 1,
        Player = 0,
        Over1 = -1,
        Over2 = -2,
        Over3 = -3,
        Over4 = -4,
        Over5 = -5
    }

    public class DrawCommand
    {
        public enum DrawType
        {
            standard,
            vectScaled,
            drawString,
            direct
        }

        private DrawType type;
        private textures texture;
        private Vector2 position;
        public Color color;
        private Color permColor;
        private float scale;
        private Vector2 scalevect;
        private float rotation;
        private Rect? sourceRect;
        private Vector2 origin;
        private float layerDepth;
        private string text;
        public float life;
        public float maxlife;

        public DrawCommand(textures texture, Vector2 position, Rect? sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth = 0, int maxlife = -1)
        {
            this.type = DrawType.standard;
            this.texture = texture;
            this.position = position;
            this.color = color;
            this.permColor = color;
            this.scale = scale;
            this.rotation = rotation;
            this.sourceRect = sourceRect;
            this.origin = origin;
            this.layerDepth = layerDepth;
            this.maxlife = maxlife;
            this.life = maxlife;
        }
        public DrawCommand(Texture2D texture, Vector2 position, Rect? sourceRect, Color color, float rotation, Vector2 origin, float scale, float layerDepth = 0, int maxlife = -1)
        {
            this.type = DrawType.direct;
            this.texture2d = texture;
            this.position = position;
            this.color = color;
            this.permColor = color;
            this.scale = scale;
            this.rotation = rotation;
            this.sourceRect = sourceRect;
            this.origin = origin;
            this.layerDepth = layerDepth;
            this.maxlife = maxlife;
            this.life = maxlife;
        }
        public DrawCommand(textures texture, Vector2 position, Rect? sourceRect, Color color, float rotation, Vector2 origin, Vector2 scalevect, float layerDepth = 0, int maxlife = -1)
        {
            this.type = DrawType.vectScaled;
            this.texture = texture;
            this.position = position;
            this.color = color;
            this.permColor = color;
            this.scalevect = scalevect;
            this.rotation = rotation;
            this.sourceRect = sourceRect;
            this.origin = origin;
            this.layerDepth = layerDepth;
            this.maxlife = maxlife;
            this.life = maxlife;
        }
        public DrawCommand(string text, Vector2 position, Color color, float scale = 0.5f, int maxlife = -1, float layerDepth = 0)
        {
            this.type = DrawType.drawString;
            this.position = position;
            this.color = color;
            this.permColor = color;
            this.layerDepth = layerDepth;
            this.scale = scale;
            this.text = text;
            this.maxlife = maxlife;
            this.life = maxlife;
        }

        public void Draw()
        {
           
        }


        public Texture2D texture2d { get; set; }
    }
    public class ThreadedCamera
    {


        Queue<DrawCommand> thisFrame = new Queue<DrawCommand>();

        List<DrawCommand> permanents = new List<DrawCommand>();
        Queue<DrawCommand> addPerm = new Queue<DrawCommand>();
        Queue<DrawCommand> removePerm = new Queue<DrawCommand>();

        private int _CameraOffset = 0;
        public float backgroundHue = 180;
        public int CameraOffset { get { return _CameraOffset; } set { _CameraOffset = value; CameraOffsetVect = new Vector2(value + 10, 0); } }
        public Vector2 CameraOffsetVect = new Vector2(0, 0);
        public Room room;
        public float zoom;

        public float vWidth { get { return OrbIt.Width - CameraOffsetVect.x; } }
        public float vHeight { get { return OrbIt.Height - CameraOffsetVect.y; } }

        public Vector2 virtualTopLeft { get { return pos - new Vector2(room.gridsystemAffect.gridWidth / 2, room.gridsystemAffect.gridHeight / 2) * 1 / zoom; } }// + CameraOffsetVect; } }

        static double x = 0;
        static bool phaseBackgroundColor = false;
        public Vector2 pos;

        public ThreadedCamera(Room room, float zoom = 0.5f, Vector2? pos = null)
        {
            this.room = room;
            this.zoom = zoom;
            this.pos = pos ?? new Vector2(room.gridsystemAffect.position.x + room.gridsystemAffect.gridWidth / 2, 10 + room.gridsystemAffect.position.y + room.gridsystemAffect.gridHeight / 2);
            //Game1.ui.keyManager.addProcessKeyAction("screenshot", KeyCodes.PrintScreen, OnPress: delegate { TakeScreenshot = true; });
        }


        public void AddPermanentDraw(textures texture, Vector2 position, Color color, float scale, float rotation, int life)
        {
            permanents.Add(new DrawCommand(texture, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, null, color, rotation, Assets.textureCenters[texture], scale * zoom, life));
        }
        public void AddPermanentDraw(textures texture, Vector2 position, Color color, Vector2 scalevect, float rotation, int life)
        {
            permanents.Add(new DrawCommand(texture, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, null, color, rotation, Assets.textureCenters[texture], scalevect * zoom, life));
        }

        public void removePermanentDraw(textures texture, Vector2 position, Color color, float scale)
        {
            permanents.Remove(new DrawCommand(texture, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, null, color, 0, Assets.textureCenters[texture], scale * zoom));
        }

        public void Work()
        {
            
            //Color bg = Color.Black;
            //    if (phaseBackgroundColor)
            //    {
            //        x += Math.PI / 360.0;
            //        backgroundHue = (backgroundHue + ((float)Math.Sin(x) + 1) / 10f) % 360;
            //        bg = ColorChanger.getColorFromHSV(backgroundHue, value: 0.2f);
            //    }
            //

            //batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, Game1.shaderEffect); //tran

                int count = thisFrame.Count;
                for (int j = 0; j < count; j++)
                {
                    DrawCommand gg = thisFrame.Dequeue();
                    gg.Draw();
                }
                int permCount = permanents.Count;
                //Console.WriteLine("1: " + permCount);
                for (int j = 0; j < permCount; j++)//todo:proper queue iteration/remove logic
                {
                    DrawCommand command = permanents[j];
                    if (command.life-- < 0)
                    {
                        permanents.Remove(command);
                        j--;
                        permCount--;
                    }
                    else
                    {
                        command.Draw();
                    }
                }
            


        }
        public void Draw(textures texture, Vector2 position, Color color, float scale, Layers Layer, bool center = true)
        {
            thisFrame.Enqueue(new DrawCommand(texture, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, null, color, 0, center ? Assets.textureCenters[texture] : Vector2.zero, scale * zoom, (((float)Layer) / 10), -1));
        }
        public void Draw(Texture2D texture, Vector2 position, Color color, float scale, Layers Layer, bool center = true)
        {
            thisFrame.Enqueue(new DrawCommand(texture, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, null, color, 0f, center ? new Vector2(texture.width / 2, texture.height / 2) : Vector2.zero, scale * zoom, (((float)Layer) / 10f), -1));
        }

        public void Draw(textures texture, Vector2 position, Color color, float scale, float rotation, Layers Layer)
        {
            thisFrame.Enqueue(new DrawCommand(texture, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, null, color, rotation, Assets.textureCenters[texture], scale * zoom, (((float)Layer) / 10), -1));
        }
        public void Draw(Texture2D texture, Vector2 position, Color color, float scale, float rotation, Layers Layer)
        {
            thisFrame.Enqueue(new DrawCommand(texture, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, null, color, rotation, new Vector2(texture.width / 2, texture.height / 2), scale * zoom, (((float)Layer) / 10), -1));
        }
        public void Draw(textures texture, Vector2 position, Color color, Vector2 scalevect, float rotation, Layers Layer)
        {
            thisFrame.Enqueue(new DrawCommand(texture, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, null, color, rotation, Assets.textureCenters[texture], scalevect * zoom, (((float)Layer) / 10), -1));
        }
        public void Draw(textures texture, Vector2 position, Rect? sourceRect, Color color, float rotation, Vector2 origin, float scale, Layers Layer)
        {
            thisFrame.Enqueue(new DrawCommand(texture, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, sourceRect, color, rotation, origin, scale * zoom, (((float)Layer) / 10), -1));
        }
        public void Draw(textures texture, Vector2 position, Rect? sourceRect, Color color, float rotation, Vector2 origin, Vector2 scalevect, Layers Layer)
        {
            thisFrame.Enqueue(new DrawCommand(texture, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, sourceRect, color, rotation, origin, scalevect * zoom, (((float)Layer) / 10), -1));
        }


        public void DrawLine(Vector2 start, Vector2 end, float thickness, Color color, Layers Layer)
        {
            if (thickness * zoom < 1) thickness = 1 / zoom;
            Vector2 diff = (end - start);// *mapzoom;
            Vector2 centerpoint = (end + start) / 2;
            //centerpoint *= mapzoom;
            float len = diff.magnitude;
            //thickness *= 2f * mapzoom;
            Vector2 scalevect = new Vector2(len, thickness);
            float angle = (float)(Math.Atan2(diff.y, diff.x));
            Draw(textures.whitepixel, centerpoint, null, color, angle, Assets.textureCenters[textures.whitepixel], scalevect, Layer);
        }

        public void DrawLinePermanent(Vector2 start, Vector2 end, float thickness, Color color, int life)//, Layers Layer)
        {
            if (thickness * zoom < 1) thickness = 1 / zoom;
            Vector2 diff = (end - start);// *mapzoom;
            Vector2 centerpoint = (end + start) / 2;
            //centerpoint *= mapzoom;
            float len = diff.magnitude;
            //thickness *= 2f * mapzoom;
            Vector2 scalevect = new Vector2(len, thickness);
            float angle = (float)(Math.Atan2(diff.y, diff.x));
            //Draw(textures.whitepixel, centerpoint, null, color, angle, Assets.textureCenters[textures.whitepixel], scalevect, Layer);
            AddPermanentDraw(textures.whitepixel, centerpoint, color, scalevect, angle, life);
        }

        public void DrawStringWorld(string text, Vector2 position, Color color, Color? color2 = null, float scale = 0.5f, bool offset = true, Layers Layer = Layers.Over5)
        {
            Color c2 = Color.red;
            if (color2 != null) c2 = (Color)color2;
            Vector2 pos = position * zoom;
            if (offset) pos += CameraOffsetVect;
            thisFrame.Enqueue(new DrawCommand(text, ((position - virtualTopLeft) * zoom) + CameraOffsetVect, c2, scale, layerDepth: (((float)Layer) / 10)));
            thisFrame.Enqueue(new DrawCommand(text, ((position - virtualTopLeft) * zoom) + CameraOffsetVect + new Vector2(1, -1), color, scale, layerDepth: (((float)Layer) / 10)));
        }
        public void DrawStringScreen(string text, Vector2 position, Color color, Color? color2 = null, float scale = 0.5f, bool offset = true, Layers Layer = Layers.Over5)
        {
            Color c2 = Color.white;
            if (color2 != null) c2 = (Color)color2;
            Vector2 pos = position;
            if (offset) pos += CameraOffsetVect;
            thisFrame.Enqueue(new DrawCommand(text, pos, c2, scale, layerDepth: (((float)Layer) / 10)));
            thisFrame.Enqueue(new DrawCommand(text, pos + new Vector2(1, -1), color, scale, layerDepth: (((float)Layer) / 10)));
        }

        public void drawGrid(List<Rect> linesToDraw, Color color)
        {
            foreach (Rect rect in linesToDraw)
            {
                Rect maprect = new Rect(rect.x, rect.y, rect.width, rect.height);
                DrawLine(new Vector2(maprect.x, maprect.y), new Vector2(maprect.width, maprect.height), 2, color, Layers.Under5);
            }
        }
    }
}
