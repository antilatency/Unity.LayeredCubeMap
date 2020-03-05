Shader "Antilatency/Skybox/CubemapProjection"
{
    Properties
    {
		_MainTex("Cubemap", CUBE) = "" {}
        Horizon("Horizon", Float) = 0
		
		_MatrixX("MatrixX", vector) = (1,0,0,0)
		_MatrixY("MatrixY", vector) = (0,1,0,0)
		_MatrixZ("MatrixZ", vector) = (0,0,1,0)
    }
    SubShader
    {
		Tags { "RenderType" = "Opaque" "PerformanceChecks" = "False" }

        //Cull Off
        //ZWrite Off
        //ZTest Always
		CGINCLUDE
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 position : P;
			};

			float3 _MatrixX;
			float3 _MatrixY;
			float3 _MatrixZ;

			v2f vert(appdata v)
			{
				
				float3x3 SkyMatrix = float3x3(_MatrixX, _MatrixY, _MatrixZ);
				
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.position = mul(SkyMatrix, mul(unity_ObjectToWorld, v.vertex));
				return o;
			}

			samplerCUBE _MainTex;
			float Horizon;
			float4 frag(v2f i) : SV_Target
			{

				float3 n = normalize(i.position);
				float4 color = texCUBE(_MainTex, n);

				color = lerp(color,1,abs(n.y) < Horizon);

				return color;
			}
		ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag            
            ENDCG
        }

    }
}
