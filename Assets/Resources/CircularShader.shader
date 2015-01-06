Shader "Circular Shader" {
   Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
      _MainTex ("Texture Image", 2D) = "white" {} 
         // a 2D texture property that we call "_MainTex", which should
         // be labeled "Texture Image" in Unity's user interface.
         // By default we use the built-in texture "white"  
         // (alternatives: "black", "gray" and "bump").
   }
   SubShader {
      Pass {	
         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
 //#include "UnityCG.cginc"
		uniform float4 _Color; 
         uniform sampler2D _MainTex;	
            // a uniform variable refering to the property above
            // (in fact, this is just a small integer specifying a 
            // "texture unit", which has the texture image "bound" 
            // to it; Unity takes care of this).
 
		

         struct vertexInput {
            float4 vertex : POSITION;
            float4 texcoord : TEXCOORD0;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 tex : TEXCOORD0;
         };

		 float tri(float num, float mod)
		{
			//float a = mod - (abs(fmod(abs(num), (2 * mod)) - mod));
		    float a = fmod(abs(num), (2 * mod));
		    float b = a - mod;
		    float c = abs(b);
		    float d = mod - c;
		    return d;
		}
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
 
            output.tex = input.texcoord;
               // Unity provides default longitude-latitude-like 
               // texture coordinates at all vertices of a 
               // sphere mesh as the input parameter 
               // "input.texcoord" with semantic "TEXCOORD0".
            output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
            return output;
         }
         float4 frag(vertexOutput input) : COLOR
         {
            //return tex2D(_MainTex, input.tex.xy);
			fixed4 c = tex2D(_MainTex, input.tex.xy);// * IN.color;
			
			half random = frac(_Time);
			half2 center = float2(0.25, 0.5);
			half2 dir = input.tex.xy - center;
			half len = tri(length(dir) - _Time.x * 10, 1);
			//half len = 1;
			c.rgba = float4(len, len,len, 1) * _Color;
			return c;
         }
 
         ENDCG
      }
   }
   // The definition of a fallback shader should be commented out 
   // during development:
   // Fallback "Unlit/Texture"
}