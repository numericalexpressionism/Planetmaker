// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MySkybox" {
   Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _Scale ("Scale", float) = 1.0 
   }

   SubShader {
      Tags { "Queue"="Background"  }

      Pass {
         ZWrite Off 
         Cull Off

         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         #include "UnityCG.cginc"

         struct vertexInput {
            float4 vertex : POSITION;
            float4 normal : NORMAL;
            float3 texcoord : TEXCOORD0;
         };

         struct vertexOutput {
            float4 vertex : SV_POSITION;
            float4 normal : NORMAL;
            float3 texcoord : TEXCOORD0;
         };

         sampler2D _MainTex;
         float _Scale;

         vertexOutput vert(vertexInput input)
         {
            vertexOutput output;
            //output.vertex = UnityObjectToClipPos(input.vertex); //natural skybox

            float ratio = _ScreenParams.x/_ScreenParams.y; 
            float v = min(1,ratio) * _Scale;
            float h = v/ratio;
            

            float4x4 ViewRotationOnly = UNITY_MATRIX_V;
            ViewRotationOnly._m03 = 0;
            ViewRotationOnly._m13 = 0;
            ViewRotationOnly._m23 = 1.5;
            ViewRotationOnly._m30 = 0;
            ViewRotationOnly._m31 = 0;
            ViewRotationOnly._m32 = 0;
           
            output.vertex = mul(ViewRotationOnly, input.vertex);
            output.vertex *= float4(h,v,h,1);

            //output.vertex.y = sign(input.vertex.y);//sky-cylinder
            output.texcoord = input.texcoord;
            output.normal = -input.normal;
            return output;
         }

         fixed3 frag (vertexOutput input) : COLOR
         {  
            fixed lat = (atan2(input.texcoord.x, input.texcoord.z)/3.14159265359/2)+0.5f;
            fixed lng = acos(input.texcoord.y)/3.14159265359 * 1;

            float4 ScreenPos = ComputeScreenPos (input.vertex);
            return tex2D(_MainTex, fixed2(lat, lng));
            //return tex2D(_MainTex, fixed2(ScreenPos.x/_ScreenParams.x*2, -ScreenPos.y/_ScreenParams.y*2));
         }
         ENDCG 
      }
   } 	
}