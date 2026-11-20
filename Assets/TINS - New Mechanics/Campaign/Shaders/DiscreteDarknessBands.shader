Shader "TINS/Campaign/Discrete Darkness Bands"
{
    Properties
    {
        _Color ("Darkness Color", Color) = (0,0,0,1)
        _Center ("Light Center", Vector) = (0,0,0,0)
        _LightRadius ("Illuminated Radius", Float) = 2
        _PenumbraWidth ("Penumbra Width", Float) = 2
        _PenumbraOpacity ("Penumbra Darkness", Range(0,1)) = 0.62
        _BandSize ("Pixel Step", Float) = 0.0625
        _PurificationWave ("Purification Wave", Vector) = (0,0,0,0)
        _FlashEnabled ("Flash Active", Float) = 0
        _FlashOrigin ("Flash Origin", Vector) = (0,0,0,0)
        _FlashDirection ("Flash Direction", Vector) = (1,0,0,0)
        _FlashRange ("Flash Range", Float) = 0
        _FlashCosAngle ("Flash Half Angle Cosine", Float) = 0.9
    }
    SubShader
    {
        Tags { "Queue"="Transparent+500" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float4 _Center;
            float _LightRadius;
            float _PenumbraWidth;
            float _PenumbraOpacity;
            float _BandSize;
            float4 _PurificationWave;
            float _FlashEnabled;
            float4 _FlashOrigin;
            float4 _FlashDirection;
            float _FlashRange;
            float _FlashCosAngle;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float2 world : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                float4 world = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = world.xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float stepSize = max(0.01, _BandSize);
                float distanceFromLight = length(i.world - _Center.xy);
                float quantizedDistance = floor(distanceFromLight / stepSize) * stepSize;
                float alpha = 0.0;

                if (quantizedDistance >= _LightRadius + _PenumbraWidth)
                    alpha = 1.0;
                else if (quantizedDistance >= _LightRadius)
                    alpha = _PenumbraOpacity;

                if (_FlashEnabled > 0.5)
                {
                    float2 toPixel = i.world - _FlashOrigin.xy;
                    float distanceFromFlash = length(toPixel);
                    float insideCone = dot(normalize(toPixel), normalize(_FlashDirection.xy));
                    if (distanceFromFlash <= _FlashRange && insideCone >= _FlashCosAngle)
                        alpha = 0.0;
                }

                if (_PurificationWave.z > 0.0)
                {
                    float fromWave = abs(quantizedDistance - _PurificationWave.x);
                    float ring = saturate(1.0 - fromWave / max(0.01, _PurificationWave.y));
                    float glow = quantizedDistance <= _PurificationWave.x ? 0.22 : 0.0;
                    if (ring > 0.0 || glow > 0.0)
                        return fixed4(1.0, 0.83, 0.48, max(ring, glow) * _PurificationWave.z);
                }

                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
