using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbItProcs
{
    public enum MouseButton { Left = 0, Right = 1, Middle = 2, None = 3 }
    public enum InputButtons
    {
        A, 
        X, 
        B, 
        Y,
        Dpad_UpArrow, 
        Dpad_DownArrow,
        Dpad_RightArrow, 
        Dpad_LeftArrow,
        Back, 
        Start,
        LB, 
        RB,
        TriggersL, 
        TriggersR,
        LS, 
        RS,
    }
    public struct InputState
    {
        public readonly Stick LeftStick_WASD, RightStick_Mouse;
        public readonly bool A_1, X_2, B_3, Y_4;
        public readonly bool Dpad_UpArrow, Dpad_DownArrow, Dpad_RightArrow, Dpad_LeftArrow;
        public readonly bool Select_TAB, Start_ESC;
        public readonly bool LeftBumper_Q, RightBumper_E;
        public readonly bool LeftTrigger_Mouse2, RightTrigger_Mouse1;
        public readonly float LeftTriggerAnalog, RightTriggerAnalog;
        //keyboard/mouse
        public InputState(Stick LeftStick_WASD, Stick RightStick_Mouse,
                          bool LeftTrigger_Mouse2, bool RightTrigger_Mouse1,
                          bool A_1, bool X_2, bool B_3, bool Y_4,
                          bool Dpad_UpArrow, bool Dpad_DownArrow, bool Dpad_RightArrow, bool Dpad_LeftArrow,
                          bool Select_TAB, bool Start_ESC,
                          bool LeftBumper_Q, bool RightBumper_E,
                          float LeftTriggerAnalog = 0, float RightTriggerAnalog = 0)
        {
            this.LeftStick_WASD = LeftStick_WASD;
            this.RightStick_Mouse = RightStick_Mouse;
            this.LeftTrigger_Mouse2 = LeftTrigger_Mouse2;
            this.RightTrigger_Mouse1 = RightTrigger_Mouse1;
            this.A_1 = A_1;
            this.X_2 = X_2;
            this.B_3 = B_3;
            this.Y_4 = Y_4;
            this.Dpad_UpArrow = Dpad_UpArrow;
            this.Dpad_DownArrow = Dpad_DownArrow;
            this.Dpad_RightArrow = Dpad_RightArrow;
            this.Dpad_LeftArrow = Dpad_LeftArrow;
            this.Select_TAB = Select_TAB;
            this.Start_ESC = Start_ESC;
            this.LeftBumper_Q = LeftBumper_Q;
            this.RightBumper_E = RightBumper_E;
            this.LeftTriggerAnalog = LeftTriggerAnalog;
            this.RightTriggerAnalog = RightTriggerAnalog;
        }
        //controller
        //public InputState(ref GamePadState state, float triggerDeadZone)
        //{
        //    this.LeftStick_WASD = new Stick(state.ThumbSticks.Left);
        //    this.LeftStick_WASD.v2.y *= -1; //todo: fix directional bools?
        //    this.RightStick_Mouse = new Stick(state.ThumbSticks.Right);
        //    this.RightStick_Mouse.v2.y *= -1;
        //    this.LeftTrigger_Mouse2 = state.Triggers.Left > triggerDeadZone;
        //    this.RightTrigger_Mouse1 = state.Triggers.Right > triggerDeadZone;
        //    this.A_1 = state.Buttons.A;
        //    this.X_2 = state.Buttons.x;
        //    this.B_3 = state.Buttons.b.ToXNAColor();
        //    this.Y_4 = state.Buttons.y;
        //    this.Dpad_UpArrow = state.DPad.Up;
        //    this.Dpad_DownArrow = state.DPad.Down;
        //    this.Dpad_RightArrow = state.DPad.Right;
        //    this.Dpad_LeftArrow = state.DPad.Left;
        //    this.Select_TAB = state.Buttons.Back;
        //    this.Start_ESC = state.Buttons.Start;
        //    this.LeftBumper_Q = state.Buttons.LeftShoulder;
        //    this.RightBumper_E = state.Buttons.RightShoulder;
        //    this.LeftTriggerAnalog = state.Triggers.Left;
        //    this.RightTriggerAnalog = state.Triggers.Right;
        //}
        public bool IsButtonDown(InputButtons button)
        {
            switch(button)
            {
                case InputButtons.A:    return A_1;
                case InputButtons.X:    return X_2;
                case InputButtons.B:    return B_3;
                case InputButtons.Y:   return Y_4;
                case InputButtons.Dpad_UpArrow:    return Dpad_UpArrow;
                case InputButtons.Dpad_DownArrow:   return Dpad_DownArrow;
                case InputButtons.Dpad_RightArrow:    return Dpad_RightArrow;
                case InputButtons.Dpad_LeftArrow:   return Dpad_LeftArrow;
                case InputButtons.Back:    return Select_TAB;
                case InputButtons.Start:   return Start_ESC;
                case InputButtons.LB:    return LeftBumper_Q;
                case InputButtons.RB:   return RightBumper_E;
                case InputButtons.TriggersL: return LeftTrigger_Mouse2;
                case InputButtons.TriggersR: return RightTrigger_Mouse1;
                default: return false;
            }
        }
    }


    public abstract class OInput
    {
        public InputState newInputState, oldInputState;
        public Player player;
        public abstract InputState GetState();
        public virtual Vector2 GetLeftStick()
        {
            return newInputState.LeftStick_WASD.v2;
        }
        public virtual Vector2 GetRightStick()
        {
            return newInputState.RightStick_Mouse.v2;
        }
        /// <summary> Returns a non-unit vector up to the radius specified.</summary>
        public virtual Vector2 GetRightStick(float range, bool drawRing = false)
        {
            return newInputState.RightStick_Mouse.v2;
        }
        public virtual void SetNewState()
        {
            newInputState = GetState();
        }
        public virtual void SetOldState()
        {
            oldInputState = newInputState;
        }
        public bool BtnDown(InputButtons button)
        {
            return newInputState.IsButtonDown(button);
        }
        public bool BtnUp(InputButtons button)
        {
            return !newInputState.IsButtonDown(button);
        }
        public bool BtnClicked(InputButtons button)
        {
            return newInputState.IsButtonDown(button) && !oldInputState.IsButtonDown(button);
        }
        public bool BtnReleased(InputButtons button)
        {
            return !newInputState.IsButtonDown(button) && oldInputState.IsButtonDown(button);
        }

        public static Vector3 mousePosition { get { return UnityEngine.Input.mousePosition; } }

        public static Vector2 WorldMousePos { get { return Camera.main.ScreenToWorldPoint(OInput.mousePosition); } }
    }
    public class PcFullInput : OInput
    {
        //public KeyboardState oldKeyState, newKeyState;
        //public MouseState oldMouseState, newMouseState;
        
        public float mouseStickRadius;

        public PcFullInput(Player player)//, float mouseStickRadius)
        {
            this.player = player;
            this.mouseStickRadius = 50f;//mouseStickRadius;
        }
        public override InputState GetState()
        {
            //newKeyState = Keyboard.GetState();
            //newMouseState = Mouse.GetState();
            Stick LeftStick_WASD = new Stick(Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D));
            Stick RightStick_Mouse = new Stick(GetRightStick(mouseStickRadius));
            newInputState = new InputState(LeftStick_WASD, RightStick_Mouse, Input.GetKey(KeyCode.Mouse1), Input.GetKey(KeyCode.Mouse0),
                                           Input.GetKey(KeyCode.Alpha1), Input.GetKey(KeyCode.Alpha2), Input.GetKey(KeyCode.Alpha3), Input.GetKey(KeyCode.Alpha4),
                                           Input.GetKey(KeyCode.UpArrow), Input.GetKey(KeyCode.DownArrow), Input.GetKey(KeyCode.RightArrow), Input.GetKey(KeyCode.LeftArrow),
                                           Input.GetKey(KeyCode.Tab), Input.GetKey(KeyCode.Escape), Input.GetKey(KeyCode.Q), Input.GetKey(KeyCode.E));
            return newInputState;
        }
        public override void SetOldState()
        {
            base.SetOldState();
        }
        /// <summary> Returns a non-unit vector up to the radius specified.</summary>
        public override Vector2 GetRightStick(float radius, bool drawRing = false)
        {
            Vector2 mousePos = OInput.WorldMousePos;// Input.mousePosition;
            Vector2 playerPos = player.node.transform.position;//(player.node.transform.position - player.room.camera.virtualTopLeft) * player.room.camera.zoom + player.room.camera.CameraOffsetVect;
            Vector2 dir = mousePos - playerPos;
            float lensqr = dir.sqrMagnitude;
            if (lensqr > radius * radius)
            {
                VMath.NormalizeSafe(ref dir);
                //dir = dir.NormalizeSafe() * radius;
            }
            else
            {
                dir /= radius;
            }
            if (drawRing)
            {
                float scale = (radius * 2f) / Assets.textureDict[textures.ring].width;
                float alpha = (((float)Math.Sin(Time.timeSinceLevelLoad * 1000 / 300f) + 1f) / 4f) + 0.25f;
                player.room.camera.Draw(textures.ring, player.node.transform.position, player.pColor * alpha, scale, Layers.Under2);
            }
            return dir;
        }
        
    }
    //public class ControllerFullInput : OInput
    //{
    //    public GamePadState newGamePadState, oldGamePadState;
    //    public PlayerIndex playerIndex;
    //    public float triggerDeadZone;
    //    public ControllerFullInput(Player player, PlayerIndex playerIndex)//, float triggerDeadZone)
    //    {
    //        this.player = player;
    //        this.playerIndex = playerIndex;
    //        this.triggerDeadZone = 0.5f;
    //    }
    //    public override InputState GetState()
    //    {
    //        newGamePadState = GamePad.GetState(playerIndex, GamePadDeadZone.Circular);
    //        
    //        newInputState = new InputState(ref newGamePadState, triggerDeadZone);
    //        return newInputState;
    //    }
    //    public override void SetOldState()
    //    {
    //        base.SetOldState();
    //        oldGamePadState = newGamePadState;
    //    }
    //}
    public class UnityInput : OInput
    {
        public int playerIndex;
        public float triggerDeadZone;
        public UnityInput(Player player, int playerIndex)//, float triggerDeadZone)
        {
            this.player = player;
            this.playerIndex = playerIndex;
            this.triggerDeadZone = 0.5f;
        }
        public override InputState GetState()
        {
            Stick left = new Stick(Input.GetAxis("L_XAxis_" + playerIndex), Input.GetAxis("L_YAxis_" + playerIndex));
            Stick right = new Stick(Input.GetAxis("R_XAxis_" + playerIndex), Input.GetAxis("R_YAxis_" + playerIndex));

            //float triggeraxis = Input.GetAxis("TriggersL_" + playerIndex);
            //float rightTrigger = Mathf.Min(triggeraxis, 0) * -1f;
            //float leftTrigger = Mathf.Max(triggeraxis, 0);
            //Debug.Log(playerIndex + " : " + player.playerIndex + " rightTrigger: " + rightTrigger);
            //Debug.Log(playerIndex + " : " + player.playerIndex + " leftTrigger: " + leftTrigger);
            
            newInputState = new InputState(left, right,
                Input.GetAxis("TriggersL_" + playerIndex) > 0.5f,
                Input.GetAxis("TriggersR_" + playerIndex) > 0.5f,
                Input.GetButton("A_" + playerIndex),
                Input.GetButton("X_" + playerIndex),
                Input.GetButton("B_" + playerIndex),
                Input.GetButton("Y_" + playerIndex),
                Input.GetAxis("DPad_YAxis_" + playerIndex) < 0,
                Input.GetAxis("DPad_YAxis_" + playerIndex) > 0,
                Input.GetAxis("DPad_XAxis_" + playerIndex) > 0,
                Input.GetAxis("DPad_XAxis_" + playerIndex) < 0,
                Input.GetButton("Back_" + playerIndex),
                Input.GetButton("Start_" + playerIndex),
                Input.GetButton("LB_" + playerIndex),
                Input.GetButton("RB_" + playerIndex),
                Input.GetAxis("TriggersL_" + playerIndex),
                Input.GetAxis("TriggersR_" + playerIndex));
            return newInputState;
        }
    }

    public struct Stick
    {
        public Vector2 v2;
        public bool up;
        public bool down;
        public bool left;
        public bool right;
        public float AsRadians { get { return VMath.VectorToAngle(v2); } }
        public int AsDegrees { get { return (int)(AsRadians * (180 / GMath.PI)); } }

        public Stick(float axisX, float axisY) : this(new Vector2(axisX, axisY)) { }
        public Stick(Vector2 sourceStick, float deadzone = 0.5f)
        {
            //v2 = Vector2.Zero;
            up = false;
            down = false;
            left = false;
            right = false;

            v2 = sourceStick;//multiply by -1?
            v2.y *= -1;
            if (v2.sqrMagnitude < deadzone * deadzone) return;

            double angle = Math.Atan2(sourceStick.y, sourceStick.x);
            int octant = ((int)Math.Round(8 * angle / (2 * Math.PI) + 9)) % 8; // TODO: test & clarify

            switch (octant)
            {
                case 0: up = true; right = true; break;
                case 1: up = true; break;
                case 2: left = true; up = true; break;
                case 3: left = true; break;
                case 4: down = true; left = true; break;
                case 5: down = true; break;
                case 6: right = true; down = true; break;
                case 7: right = true; break;
            }
        }
        public Stick(bool up, bool down, bool left, bool right)
        {
            float x = 0, y = 0;
            if (up)
            {
                y -= 1;
                this.up = true;
            }
            else this.up = false;
            if (down)
            {
                y += 1;
                this.down = true;
            }
            else this.down = false;
            if (left)
            {
                x -= 1;
                this.left = true;
            }
            else this.left = false;
            if (right)
            {
                x += 1;
                this.right = true;
            }
            else this.right = false;

            Vector2 v = new Vector2(x, y);

            if (x != 0 && y != 0)
            {
                v *= GMath.invRootOfTwo;
            }
            this.v2 = v;
        }

        public static implicit operator Vector2(Stick s) { return s.v2; }
        public bool isCentered() //account for v2?
        {
            if (up == false &&
                down == false &&
                left == false &&
                right == false) return true;
            else return false;
        }
    }
}
