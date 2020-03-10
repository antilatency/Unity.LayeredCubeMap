Shader "Antilatency/Skybox/CubemapProjection"
{
    Properties
    {
		_MainTex("Cubemap", CUBE) = "" {}
        _ShowHorizon("Show Horizon", Float) = 0
		
		_HorizonAdjustment("Horizon Adjustment", Float) = 0
		_MatrixX("MatrixX", vector) = (1,0,0,0)
		_MatrixY("MatrixY", vector) = (0,1,0,0)
		_MatrixZ("MatrixZ", vector) = (0,0,1,0)
		_Origin("Origin", vector) = (0,0,1,0)
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
				float3 worldPosition : worldPosition;
				float3 skyboxPosition : skyboxPosition;
			};

			float3 _MatrixX;
			float3 _MatrixY;
			float3 _MatrixZ;
			float3 _Origin;
			v2f vert(appdata v)
			{
				
				float3x3 SkyMatrix = float3x3(_MatrixX, _MatrixY, _MatrixZ);
				
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.worldPosition = mul(unity_ObjectToWorld, v.vertex) - _Origin;
				o.skyboxPosition = mul(SkyMatrix, o.worldPosition);
				return o;
			}

			samplerCUBE _MainTex;
			float _ShowHorizon;
			float _HorizonAdjustment;
			float4 frag(v2f i) : SV_Target
			{
				
				float3 skyboxPosition = normalize(i.skyboxPosition);

				float r = length(skyboxPosition.xz);
				skyboxPosition.y += _HorizonAdjustment * r;

				float4 color = texCUBE(_MainTex, skyboxPosition);

				float3 worldPosition = normalize(i.worldPosition);
				
				color = lerp(color,1,abs(worldPosition.y) < _ShowHorizon);

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
