Shader "Custom/WinAnimationEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_AnimationTex("Albedo (RGB)", 2D) = "white" {}
		_AnimationTime("Animation Time used to make disolve effect", Range(0.0 , 1.0)) = 0.0
    }
    SubShader
    {
        Tags 
		{	
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"CanUseSpriteAtlas" = "True"
		}

		Blend SrcAlpha OneMinusSrcAlpha

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
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			sampler2D _AnimationTex;
            float4 _MainTex_ST;
			float _AnimationTime;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			float4 SampleSpriteTexture(sampler2D text, float2 uv)
			{
				half4 color = tex2D(text, uv);

				#if ETC1_EXTERNAL_ALPHA
					// get the color from an external texture (usecase: Alpha support for ETC1 on android)
					color.a = tex2D(_AlphaTex, uv).r;
				#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = SampleSpriteTexture(_MainTex, i.uv);
				float3 mask = tex2D(_AnimationTex, float2(_AnimationTime * 0.5, 0.5)).rgb;

				col.rgb -= mask.r * 1.25;
				col.a = col.a;

                return col;
            }
            ENDCG
        }
    }
}
