Shader "Skybox/DayNightBlend"
{
    Properties
    {
        _NightTex ("Day Sky (Panoramic)", 2D) = "white" {}
        _DayTex ("Night Sky (Panoramic)", 2D) = "black" {}
        _Blend ("Day → Night Blend", Range(0,1)) = 0
        _Exposure ("Exposure", Range(0,8)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Background"
            "RenderType"="Background"
            "PreviewType"="Skybox"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _DayTex;
            sampler2D _NightTex;
            float _Blend;
            float _Exposure;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = normalize(mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            float2 DirectionToPanoramicUV(float3 dir)
            {
                float2 uv;
                uv.x = atan2(dir.z, dir.x) / (2 * UNITY_PI) + 0.5;
                uv.y = asin(dir.y) / UNITY_PI + 0.5;
                return uv;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = DirectionToPanoramicUV(i.dir);

                fixed4 day = tex2D(_DayTex, uv);
                fixed4 night = tex2D(_NightTex, uv);

                fixed4 sky = lerp(day, night, _Blend);
                return sky * _Exposure;
            }
            ENDCG
        }
    }
}