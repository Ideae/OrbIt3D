using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;

namespace OrbItProcs {

    public static class Utils {
        public static float pixelsToUnits = 5;
        public static Sprite ToSprite(this Texture2D tex)
        {
            Vector2 pivot = new Vector2(0.5f, 0.5f);// / 2 / pixelsToUnits;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, pixelsToUnits);
            return sprite;
        }

        public static System.Random random = new System.Random((int)DateTime.Now.Millisecond);
        public static Color ContrastColor(this Color c)
        {
            int r = (c.r.ToXNAColor() + 128) % 255;
            int g = (c.g.ToXNAColor() + 128) % 255;
            int b = (c.b.ToXNAColor() + 128) % 255;
            return new Color(r, g, b);
        }
        public static int ToXNAColor(this float f)
        {
            return (int)(f * 255);
        }
        public static bool AsBool(this int i)
        {
            return i == 0 ? false : true;
        }
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
                                            TKey key, Func<TValue> valueCreator)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = valueCreator();
                dictionary.Add(key, value);
            }
            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
                                                    TKey key) where TValue : new()
        {
            return dictionary.GetOrAdd(key, () => new TValue());
        }
        public static Rect contract(this Rect source, int amount){

            return new Rect(source.x + amount / 2, source.y + amount / 2, source.width - amount, source.height - amount);
        }
        public static string RandomName(int tries = 0)
        {
            var dict = Component.compTypes;
            int depth = Utils.random.Next(dict.Count);
            Type t = dict.ElementAt(depth);
            var props = t.GetProperties();
            int i = Utils.random.Next(props.Length);
            var pinfo = props.ElementAt(i);
            if (tries < 10 && typeof(Component).GetProperty(pinfo.Name) != null)
            {
                return RandomName(++tries);
            }
            return pinfo.Name;
        }
        public static Color ToColor(this Vector4 v)
        {
            return new Color(v.x, v.y, v.z, v.w);
        }
        public static Color ToColor(this Vector3 v, float alpha)
        {
            return new Color(v.x, v.y, v.z, alpha);
        }
        public static string Name(this Type t)
        {
            return t.ToString().LastWord('.');
        }
        //todo: realize self
        //public static Texture2D Crop(this Texture2D image, Rect source)
        //{
        //    var graphics = image.GraphicsDevice;
        //    var ret = new RenderTarget2D(graphics, source.Width, source.Height);
        //    var sb = new SpriteBatch(graphics);
        //
        //    graphics.SetRenderTarget(ret); // draw to image
        //    graphics.Clear(new Color(0, 0, 0, 0));
        //
        //    sb.Begin();
        //    sb.Draw(image, Vector2.Zero, source, Color.White);
        //    sb.End();
        //
        //    graphics.SetRenderTarget(null); // set back to main window
        //    Texture2D ret2 = new Texture2D(graphics, source.Width, source.Height);
        //    Color[] q = new Color[source.Width * source.Height]; 
        //    ret.GetData(q);
        //
        //    ret2.SetData<Color>(q);
        //
        //    return (Texture2D)ret2;
        //}
        //public static Texture2D[,] sliceSpriteSheet(this Texture2D spritesheet, int columnsX, int rowsY)
        //{
        //   Texture2D[,] result = new Texture2D[columnsX,rowsY];
        //    int width = spritesheet.Bounds.Width / columnsX;
        //    int height = spritesheet.Bounds.Height / rowsY;
        //   for (int x = 0; x < columnsX; x++)
        //   {
        //       for (int y = 0; y < rowsY; y++)
        //       {
        //           result[x, y] = spritesheet.Crop(new Rect(x * width, y * height, width, height));
        //       }
        //   }
        //   return result;
        //}
        public static void notImplementedException() 
        { 
            //PopUp.Toast("Zack and Dante are lazy.", "NotImplementedException"); 
            Debug.Log("Zack and Dante are lazy. : NotImplementedException"); 
        }
        public static object parsePrimitive(Type primitiveType, String value)
        {

            string s = value.ToString().Trim();

            if (primitiveType == typeof(int))
            {
                int v;
                if (Int32.TryParse(s, out v))
                {
                    //fpinfo.SetValue(v, parentItem.obj);
                    return v;
                }
                else return null;
            }
            else if (primitiveType == typeof(float))
            {
                float v;
                if (float.TryParse(s, out v))
                {
                    //fpinfo.SetValue(v, parentItem.obj);
                    return v;
                }
                else return null;
            }
            else if (primitiveType == typeof(double))
            {
                double v;
                if (double.TryParse(s, out v))
                {
                    //fpinfo.SetValue(v, parentItem.obj);
                    return v;
                }
                else return null;
            }
            else if (primitiveType == typeof(byte))
            {
                byte v;
                if (byte.TryParse(s, out v))
                {
                    //fpinfo.SetValue(v, parentItem.obj);
                    return v;
                }
                else return null;
            }
            else if (primitiveType.IsEnum)
            {
                foreach (var val in Enum.GetValues(primitiveType))
                {
                    if (val.ToString().ToLower().Equals(s.ToLower()))
                    {
                        return val;
                    }
                }
            }
            return null;
        }
        public static mtypes GetCompTypes(Type t)
        {
            FieldInfo pinfo = t.GetField("CompType");
            if (pinfo == null || pinfo.FieldType != typeof(mtypes)) return mtypes.none;
            return (mtypes)pinfo.GetValue(null);
        }
        public static Info GetInfoType(Type t)
        {
            var infos = t.GetCustomAttributes(typeof(Info), false);
            if (infos != null && infos.Length > 0)
            {
                return (Info)infos.ElementAt(0);
            }
            return null;
        }
        public static Info GetInfoClass(object o)
        {
            return GetInfoType(o.GetType());
        }
        public static Info GetInfoProperty(PropertyInfo pinfo)
        {
            var infos = pinfo.GetCustomAttributes(typeof(Info), false);
            if (infos != null && infos.Length > 0)
            {
                return (Info)infos.ElementAt(0);
            }
            return null;
        }

        public static bool isGenericType(Type genericType, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }
            return false;
        }
        public static bool isToggle(object o)
        {
            return isGenericType(typeof(Toggle<>), o.GetType());
        }
        public static bool isToggle(Type t)
        {
            return isGenericType(typeof(Toggle<>), t);
        }
        //thanks, skeet!
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
        public static int Sign(int i)
        {
            return (i > 0).ToInt() - (i < 0).ToInt();
        }

        public static int ToInt(this bool b)
        {
            return b ? 1 : 0;
        }

        public static string LastWord(this string s, char delim)
        {
            return s.Substring(s.LastIndexOf(delim) + 1);
        }

        public static void Break()
        {
            System.Diagnostics.Debugger.Break();
        }

        public static string uniqueString(ObservableHashSet<string> hashSet = null)
        {
            string GuidString = randomString();

            if (hashSet != null)
            {
                while (hashSet.Contains(GuidString))
                {
                    GuidString = randomString();
                }
            }
            return GuidString;
        }
        public static string randomString()
        {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            return GuidString;
        }
        ///dontdelete. sorry
        public static bool IsFucked(this Vector2 v)
        {
            if (float.IsInfinity(v.x) || float.IsNaN(v.x) || float.IsInfinity(v.y) || float.IsNaN(v.y)) return true;
            return false;
        }
        public static string wordWrap(this string message, int maxCharsPerLine)
        {
            int chars = maxCharsPerLine;
                for (int i = 1; i <= 4; i++)
                    if (message.Length > chars * i)
                        for (int j = chars * i; j > (chars * i) - chars; j--)
                            if (message.ElementAt(j).Equals(' ') || message.ElementAt(j).Equals('/'))
                            {
                                message = message.Insert(j + 1, "\n");
                                break;
                            };
            return message;
        }
        public static float[] toFloatArray(this Vector2 v2)
        {
            float[] result = new float[2];
            result[0] = v2.x; result[1] = v2.y;
            return result;
        }
        //even distribution of colors between 0 and 16.5 million (total number of possible colors, excluding alphas)
        public static Color IntToColor(int i, int alpha = 255)
        {
            int r = (i / (255 * 255)) % 255;
            int g = (i / 255) % 255;
            int b = i % 255;

            string s = string.Format("{0}\t{1}\t{2}", r, g, b);
            //Console.WriteLine(s);
            //Console.WriteLine(i);
            return new Color(r, g, b, alpha);
        }

        public static int CurrentMilliseconds()
        {
            DateTime dt = DateTime.Now;
            int total = dt.Millisecond + (dt.Second * 1000) + (dt.Minute * 60 * 1000);
            return total;
        }

        public static bool In<T>(this T x, params T[] args) where T : struct, IConvertible {return args.Contains(x);}
        public static T Pop<T>(this List<T> list)
        {
            //error's tomb
            T item = list.ElementAt(list.Count-1);
            list.Remove(item);
            return item;
        }
        public static float HueFromColor(Color c)
        {
            //180/pi*atan2( sqrt(3)*(G-B) , 2*R-G-B )
            return (float)(180 / Math.PI * Math.Atan2(Math.Sqrt(3) * (c.g.ToXNAColor() - c.b.ToXNAColor()), 2 * c.r.ToXNAColor() - c.g.ToXNAColor() - c.b.ToXNAColor()));
        }
        
        /*
        public static string SelectedItem(this TomShane.Neoforce.Controls.ComboBox cb)
        {
            if (cb == null || cb.ItemIndex == -1) return null;
            return cb.Items.ElementAt(cb.ItemIndex).ToString();
        }
        public static object selected(this TomShane.Neoforce.Controls.ListBox c) { return c.Items.ElementAt(c.ItemIndex); }

        public static object selected(this TomShane.Neoforce.Controls.ComboBox c) { return c.Items.ElementAt(c.ItemIndex); }

        public static void syncToOCDelegate(this ICollection<object> lst, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (object o in e.NewItems)
                    lst.Add(o);
            if (e.Action == NotifyCollectionChangedAction.Remove)
                foreach (object o in e.OldItems)
                    lst.Remove(o);
        }
        public static void syncTo(this ObservableCollection<object> oc, ICollection<object> from)
        {
            oc.CollectionChanged += delegate(object s, NotifyCollectionChangedEventArgs e) { (from as ObservableCollection<object>).syncToOCDelegate(e); };
        }
        public static void reset(this ObservableCollection<object> oc)
        {
            foreach (object o in oc) oc.Remove(o);
        }
        public static void AddRange(this ObservableCollection<object> oc, ICollection<object> from)
        { foreach (object o in from) oc.Add(o); }
        public static void RemoveRange(this ObservableCollection<object> oc, ICollection<object> from)
        { foreach (object o in from) if (oc.Contains(o)) oc.Remove(o); }
        */
        public static Color randomColor()
        {
            return new Color((float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255);
        }
        public static void printDictionary(Dictionary<object, object> dict, string s = "")
        {
            if (dict == null)
            { //Console.WriteLine("Dict is null"); return; }
            }
            Console.WriteLine(s);
            foreach (KeyValuePair<object, object> kvp in dict)
            {
                //Console.WriteLine("Key = {0}, Value = {1}",
                //    kvp.Key, kvp.Value);
            }
        }
        //public static void DrawLine(Room room, Vector2 start, Vector2 end, float thickness, Color color, Layers Layer)
        //{
        //    if (thickness * room.zoom < 1) thickness = 1 / room.zoom;
        //    Vector2 diff = (end - start);// *mapzoom;
        //    Vector2 centerpoint = (end + start) / 2;
        //    //centerpoint *= mapzoom;
        //    float len = diff.Length();
        //    //thickness *= 2f * mapzoom;
        //    Vector2 scalevect = new Vector2(len, thickness);
        //    float angle = (float)(Math.Atan2(diff.y, diff.x));
        //    room.camera.Draw(textures.whitepixel, centerpoint, null, color, angle, Assets.textureCenters[textures.whitepixel], scalevect, Layer);
        //}

        public static bool checkCollision(Node o1, Node o2)
        {
            if (Vector2.Distance(o1.body.pos, o2.body.pos) <= o1.body.radius + o2.body.radius)
            {
                return true;
            }
            return false;
        }

        public static void resolveCollision(Node o1, Node o2)
        {
            float distanceOrbs = (float)Vector2.Distance(o1.body.pos, o2.body.pos);
            if (distanceOrbs < 10) distanceOrbs = 10; //prevent /0 error
            Vector2 normal = (o2.body.pos - o1.body.pos) / distanceOrbs;
            float pvalue = 2 * (o1.body.velocity.x * normal.x + o1.body.velocity.y * normal.y - o2.body.velocity.x * normal.x - o2.body.velocity.y * normal.y) / (o1.body.mass + o2.body.mass);

            o1.body.velocity.x = o1.body.velocity.x - pvalue * normal.x * o2.body.mass;
            o1.body.velocity.y = o1.body.velocity.y - pvalue * normal.y * o2.body.mass;
            o2.body.velocity.x = o2.body.velocity.x + pvalue * normal.x * o1.body.mass;
            o2.body.velocity.y = o2.body.velocity.y + pvalue * normal.y * o1.body.mass;
            //float loss1 = 0.98f;
            //float loss2 = 0.98f;
            //o1.transform.velocity *= loss1;
            //o2.transform.velocity *= loss2;
            fixCollision(o1, o2);
        }

        //make sure that if the orbs are stuck together, they are separated.
        public static void fixCollision(Node o1, Node o2)
        {
            //float orbRadius = 25.0f; //integrate this into the orb class
            //if the orbs are still within colliding distance after moving away (fix radius variables)
            //if (Vector2.DistanceSquared(o1.transform.position + o1.transform.velocity, o2.transform.position + o2.transform.velocity) <= ((o1.transform.radius * 2) * (o2.transform.radius * 2)))
            if (Vector2.Distance(o1.body.pos + o1.body.velocity, o2.body.pos + o2.body.velocity) <= o1.body.radius + o2.body.radius)
            {

                Vector2 difference = o1.body.pos - o2.body.pos; //get the vector between the two orbs
                float length = Vector2.Distance(o1.body.pos, o2.body.pos);//get the length of that vector
                difference = difference / length;//get the unit vector
                //fix the below statement to get the radius' from the orb objects
                length = (o1.body.radius + o2.body.radius) - length; //get the length that the two orbs must be moved away from eachother
                difference = difference * length; // produce the vector from the length and the unit vector
                if (o1.movement.active && o1.movement.pushable
                    && o2.movement.active && o2.movement.pushable)
                {
                    o1.body.pos += difference / 2;
                    o2.body.pos -= difference / 2;
                }
                else if (o1.movement.active && !o1.movement.pushable)
                {
                    o2.body.pos -= difference;
                }
                else if (o2.movement.active && !o2.movement.pushable)
                {
                    o1.body.pos += difference;
                }
            }
            else return;
        }

        public static void Infect(Node newNode)
        {
            if (Utils.random.Next(50000) == 0)
            {
                newNode.body.color = Color.red;
                Action<Node, Node> evil = null;
                Action<Node> doAfter = delegate(Node n)
                {
                    n.body.color = Color.red;
                    n.body.OnCollisionStay += evil;
                };


                evil = delegate(Node source, Node target)
                {
                    if (target == null) return;
                    if (target.CheckData<bool>("infected")) return;
                    if (target.HasComp<Scheduler>())
                    {
                        target.Comp<Scheduler>().doAfterXMilliseconds(doAfter, Utils.random.Next(5000));
                        target.SetData("infected", true);
                    }
                };
                newNode.body.OnCollisionStay += evil;
            }
        }


        
    } // end of class.
}
