Shader "UI/SpotlightDarkness"
{
    Properties
    {
        _DarknessColor ("Darkness Color", Color) = (0, 0, 0, 1)
        _Player1Pos ("Player 1 Screen Pos", Vector) = (0.5, 0.5, 0, 0)
        _Player2Pos ("Player 2 Screen Pos", Vector) = (0.5, 0.5, 0, 0)
        _LightRadius ("Light Radius", Float) = 0.15
        _EdgeSoftness ("Edge Softness", Float) = 0.05
        _DarknessAlpha ("Darkness Alpha", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            fixed4 _DarknessColor;
            float4 _Player1Pos;
            float4 _Player2Pos;
            float _LightRadius;
            float _EdgeSoftness;
            float _DarknessAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist1 = distance(i.uv, _Player1Pos.xy);
                float dist2 = distance(i.uv, _Player2Pos.xy);
                float minDist = min(dist1, dist2);

                // smoothstep: 0 inside radius, 1 outside (radius + softness)
                float lightFactor = smoothstep(_LightRadius, _LightRadius + _EdgeSoftness, minDist);

                float alpha = _DarknessAlpha * lightFactor * i.color.a;

                return fixed4(_DarknessColor.rgb, alpha);
            }
            ENDCG
        }
    }
}
