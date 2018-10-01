// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/RGBGlitchShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _GlitchAmount;

			float rand() 
			{
				return frac(sin(_Time.y)*1e4);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv.xy;

				float2 uvR = uv;
				float2 uvB = uv;

				uvR.x = uv.x * 1.0 - rand() * 0.02 * _GlitchAmount;
				uvB.y = uv.y * 1.0 + rand() * 0.02 * _GlitchAmount;

				// 
				if (uv.y < rand() && uv.y > rand() - 0.1 && rand() < _GlitchAmount && sin(_Time.y) < 0.0)
				{
					uv.x = (uv + 0.02 * rand()).x;
				}

				float4 original = tex2D(_MainTex, uv);
				float4 c = float4(tex2D(_MainTex, uvR).r, original.g, tex2D(_MainTex, uvB).b, original.a);

				//
				float scanline = sin(uv.y * 800.0 * rand()) / 30.0;
				c *= 1.0 - scanline * _GlitchAmount;

				float vegDist = length(float2(0.5 , 0.5) - uv);
				c *= 1.0 - vegDist * 0.6;

				return c;
			}
			ENDCG
		}
	}
}
