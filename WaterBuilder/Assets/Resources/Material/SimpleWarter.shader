Shader "Custom/SimpleWarter"
{
	Properties
	{
		[NoScaleOffset] _BumpMap ("Waves normalmap" , 2D) = ""{}
		[NoScaleOffset] _ColorControl("Reflective Color(RGB) frensnel(A)", 2D) = ""{}
		_HorizonColor("Horizon color", color) = (1, 1, 1, 1)
		_WaveSpeed("Wave speed, time / 20, (map1:x,y; map2:x,y)", vector) = (19,9,-16,-7)
		_WaveScale("Wave scale, the value small,the wave large (map1:x,y; map2:x,y)", vector) = (1, 1, 1, 1)

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 bumpuv[2] : TEXCOORD0;
				float3 viewDir : TEXCOORD2;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _BumpMap;
			sampler2D _ColorControl;
			float4 _WaveSpeed;
			float4 _WaveScale;
			float4 _HorizonColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float4 temp = (worldPos.xzxz + _WaveSpeed * _Time.x) * _WaveScale;
				o.bumpuv[0] = temp.xy;
				o.bumpuv[1] = temp.wz;
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex)).xyz;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half3 bump1 = UnpackNormal(tex2D(_BumpMap, i.bumpuv[0])).rgb;
				half3 bump2 = UnpackNormal(tex2D(_BumpMap, i.bumpuv[1])).rgb;
				half3 bump = (bump1 + bump2) * 0.5;
				//bump是切换空间下的法线方向，z轴对应的是物体空间中的上方(y轴)，这里为了节省运算计算，因为水的y轴在世界空间是一定向上的，这里直接调换法线的z和y
				half fresnel = dot(i.viewDir, bump.xzy);

				half4 water = tex2D(_ColorControl, float2(fresnel, fresnel));

				float4 col;
				col.rgb = lerp(water.rgb, _HorizonColor.rgb, water.a);
				col.a = _HorizonColor.a;

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
