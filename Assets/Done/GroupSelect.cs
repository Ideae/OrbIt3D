using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class GroupSelect : Process
    {
        private Vector2 groupSelectionBoxOrigin;
        public HashSet<Node> groupSelectSet;

        public GroupSelect() : base()
        {
            //LeftHold += LeftH;
            //LeftClick += LeftC;
            addProcessKeyAction("grouph", KeyCode.Mouse0, OnHold: UpdateBox, OnPress: SetOrigin, OnRelease: SelectGroup);

        }

        public void UpdateBox()
        {
            Vector2 mousePos = OInput.WorldMousePos;

            float lowerx = Math.Min(mousePos.x, groupSelectionBoxOrigin.x);
            float upperx = Math.Max(mousePos.x, groupSelectionBoxOrigin.x);
            float lowery = Math.Min(mousePos.y, groupSelectionBoxOrigin.y);
            float uppery = Math.Max(mousePos.y, groupSelectionBoxOrigin.y);

            room.addRectangleLines(lowerx, lowery, upperx, uppery);
            //Console.WriteLine(mousePos.x + " " + glob.x);
        }

        public void SetOrigin()
        {
            groupSelectionBoxOrigin = OInput.WorldMousePos;
        }

        public void SelectGroup()
        {
            
            bool ctrlDown = UnityEngine.Input.GetKeyDown(KeyCode.LeftControl);
            bool altDown = UnityEngine.Input.GetKeyDown(KeyCode.LeftAlt);
            if (altDown) ctrlDown = false;

            Vector2 mousePos = OInput.WorldMousePos;

            float lowerx = Math.Min(mousePos.x, groupSelectionBoxOrigin.x);
            float upperx = Math.Max(mousePos.x, groupSelectionBoxOrigin.x);
            float lowery = Math.Min(mousePos.y, groupSelectionBoxOrigin.y);
            float uppery = Math.Max(mousePos.y, groupSelectionBoxOrigin.y);

            if (!ctrlDown && !altDown) groupSelectSet = new HashSet<Node>();
                

            foreach (Node n in room.masterGroup.fullSet.ToList())
            {
                float xx = n.transform.position.x;
                float yy = n.transform.position.y;

                if (xx >= lowerx && xx <= upperx
                    && yy >= lowery && yy <= uppery)
                {
                    if (altDown)
                    {
                        if (groupSelectSet.Contains(n)) groupSelectSet.Remove(n);
                        else groupSelectSet.Add(n);
                    }
                    else
                    {
                        groupSelectSet.Add(n);
                    }
                }
            }
            //System.Console.WriteLine(groupSelectSet.Count);

            room.addRectangleLines(lowerx, lowery, upperx, uppery);

        }
    }
}
