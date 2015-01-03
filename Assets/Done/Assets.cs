using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public enum textures
    {
        whitecircle,
        orientedcircle,
        blackorb,
        whitesphere,
        ring,
        whiteorb,
        blueorb,
        colororb,
        whitepixel,
        whitepixeltrans,
        sword,
        randompixels,
        innerL,
        innerR,
        outerL,
        outerR,
        pointer,
        itemLight,
        itemWhisper,
        fist,
        cage,
        robot1,
        shoveltip,
        spiderhead,
        spiderleg1,
        rock1,
        boulder1,
        goat,
        gradient1,
        gradient2,
        ridgesR,
        ridgesL,
        boulderShine,
        endLight,
        leaf,
        whiteorb2,
    }

    public class Assets : MonoBehaviour
    {
        public static Dictionary<textures, Texture2D> textureDict;
        public static Dictionary<textures, Vector2> textureCenters;
        public static Dictionary<textures, Rect> textureRects;
        //public GameObject DefaultNode;
        //public static GameObject defaultNode;

        public void Awake()
        {
            OrbIt.assets = this;
            //Assets.defaultNode = this.DefaultNode;
            textureDict = new Dictionary<textures, Texture2D>(){
            { textures.blueorb, Resources.Load<Texture2D>("Textures/bluesphere"               )},
            { textures.whiteorb, Resources.Load<Texture2D>("Textures/whiteorb"                )},
            { textures.colororb, Resources.Load<Texture2D>("Textures/colororb"                )},
            { textures.whitepixel, Resources.Load<Texture2D>("Textures/whitepixel"            )},
            { textures.whitepixeltrans, Resources.Load<Texture2D>("Textures/whitepixeltrans"  )},
            { textures.whitecircle, Resources.Load<Texture2D>("Textures/whitecircle"          )},
            { textures.whitesphere, Resources.Load<Texture2D>("Textures/whitesphere"          )},
            { textures.blackorb, Resources.Load<Texture2D>("Textures/blackorb"                )},
            { textures.ring, Resources.Load<Texture2D>("Textures/ring"                        )},
            { textures.orientedcircle, Resources.Load<Texture2D>("Textures/orientedcircle"    )},
            { textures.sword, Resources.Load<Texture2D>("Textures/sword"                      )},
            { textures.randompixels, Resources.Load<Texture2D>("Textures/randompixels"        )},
            { textures.innerL, Resources.Load<Texture2D>("Textures/innerL"                    )},
            { textures.innerR, Resources.Load<Texture2D>("Textures/innerR"                    )},
            { textures.outerL, Resources.Load<Texture2D>("Textures/outerL"                    )},
            { textures.outerR, Resources.Load<Texture2D>("Textures/outerR"                    )},
            { textures.pointer, Resources.Load<Texture2D>("Textures/pointer"                  )},
            { textures.itemLight, Resources.Load<Texture2D>("Textures/itemLight"              )},
            { textures.itemWhisper, Resources.Load<Texture2D>("Textures/itemWhisper"          )},
            { textures.cage, Resources.Load<Texture2D>("Textures/cage"                        )},
            { textures.fist, Resources.Load<Texture2D>("Textures/fist"                        )},
            { textures.goat, Resources.Load<Texture2D>("Textures/Boulder_3"                   )},
            { textures.robot1, Resources.Load<Texture2D>("Textures/Robot1"                    )},
            { textures.shoveltip, Resources.Load<Texture2D>("Textures/ShovelTip"              )},
            { textures.spiderhead, Resources.Load<Texture2D>("Textures/SpiderHead"            )},
            { textures.spiderleg1, Resources.Load<Texture2D>("Textures/SpiderLeg1"            )},
            { textures.rock1, Resources.Load<Texture2D>("Textures/RockTexture1"               )},
            { textures.boulder1, Resources.Load<Texture2D>("Textures/Bolders"                 )},
            { textures.gradient1, Resources.Load<Texture2D>("Textures/gradient"               )},
            { textures.gradient2, Resources.Load<Texture2D>("Textures/gradient2"              )},
            { textures.ridgesL, Resources.Load<Texture2D>("Textures/RidgesL"                  )},
            { textures.ridgesR, Resources.Load<Texture2D>("Textures/RidgesR"                  )},
            { textures.boulderShine, Resources.Load<Texture2D>("Textures/boulderShine"        )},
            { textures.endLight, Resources.Load<Texture2D>("Textures/endLight"                )},
            { textures.leaf, Resources.Load<Texture2D>("Textures/leaf"                        )},
            { textures.whiteorb2, Resources.Load<Texture2D>("Textures/whiteorb2"              )},
            };

            textureCenters = new Dictionary<textures, Vector2>();
            foreach (var tex in textureDict.Keys)
            {
                Texture2D t = textureDict[tex];
                textureCenters[tex] = new Vector2(t.width / 2f, t.height / 2f);
                if (!t) throw new FileNotFoundException("Asset Missing");
            }
        }
    }
}