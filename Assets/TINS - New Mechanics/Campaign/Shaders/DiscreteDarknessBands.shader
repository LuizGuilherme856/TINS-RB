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
        [NoScaleOffset] _TerrainTiles ("16 px Terrain Tiles", 2D) = "white" {}
        [HideInInspector] _TerrainAtlasSize ("Terrain Atlas Size", Vector) = (112,64,0,0)
        [HideInInspector] _TerrainEnabled ("Terrain Enabled", Float) = 0
        [HideInInspector] _TerrainOnly ("Terrain Pass", Float) = 0
        [HideInInspector] _PurifiedRadius ("Purified Grass Radius", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+500" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode"="Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightVariables.hlsl"

            half4 _Color;
            float4 _Center;
            float _LightRadius;
            float _PenumbraWidth;
            float _PenumbraOpacity;
            float _BandSize;
            float4 _PurificationWave;
            sampler2D _TerrainTiles;
            float4 _TerrainAtlasSize;
            float _TerrainEnabled;
            float _TerrainOnly;
            float _PurifiedRadius;

            struct appdata { float3 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float2 world : TEXCOORD0; half2 lightingUV : TEXCOORD1; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.world = TransformObjectToWorld(v.vertex).xy;
                o.lightingUV = ComputeScreenPos(o.vertex / o.vertex.w).xy;
                return o;
            }

            half3 SampleTerrain(float2 world, float band)
            {
                // Each world unit is one 16×16 tile. All four palettes share its variant.
                float2 cell = floor(world);
                float columns = _TerrainAtlasSize.x / 16.0;
                float column = floor(frac(sin(dot(cell, float2(127.1, 311.7))) * 43758.5453) * columns);
                float2 texel = float2(column, 3.0 - band) * 16.0 + floor(frac(world) * 16.0);
                return tex2D(_TerrainTiles, (texel + 0.5) / _TerrainAtlasSize.xy).rgb;
            }

            half4 frag(v2f i) : SV_Target
            {
                float stepSize = max(0.01, _BandSize);
                float distanceFromLight = length(i.world - _Center.xy);
                float quantizedDistance = floor(distanceFromLight / stepSize) * stepSize;
                float band = quantizedDistance < _PurifiedRadius ? 0.0 : 1.0;
                if (quantizedDistance >= _LightRadius)
                    band = quantizedDistance >= _LightRadius + _PenumbraWidth ? 3.0 : 2.0;
                float alpha = band == 3.0 ? 1.0 : (band == 2.0 ? _PenumbraOpacity : 0.0);
                // The Flash uses the renderer's Additive blend style. Its Light2D
                // buffer already includes the shadows from the scene's ShadowCaster2D components.
                half3 light = 0.0;
                #if USE_SHAPE_LIGHT_TYPE_1
                    light = SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, i.lightingUV).rgb * _HDREmulationScale;
                #endif
                half lightStrength = max(light.r, max(light.g, light.b));
                // Lift the existing darkness gradually as the light grows.
                alpha *= exp2(-lightStrength * 3.4);

                if (_TerrainOnly > 0.5)
                {
                    half3 ground = SampleTerrain(i.world, band);
                    // The same light lifts pale blades more than dark soil.
                    half3 reflectedLight = light * (0.34 + 0.90 * ground);
                    return half4(ground + reflectedLight, 1.0);
                }

                half3 darknessColor = _Color.rgb;
                if (_TerrainEnabled > 0.5 && alpha > 0.0)
                    darknessColor = SampleTerrain(i.world, band);
                half4 overlay = half4(darknessColor, alpha * _Color.a);

                if (_PurificationWave.z > 0.0)
                {
                    float fromWave = abs(quantizedDistance - _PurificationWave.x);
                    float ring = saturate(1.0 - fromWave / max(0.01, _PurificationWave.y));
                    float glow = quantizedDistance <= _PurificationWave.x ? 0.22 : 0.0;
                    if (ring > 0.0 || glow > 0.0)
                        overlay = half4(1.0, 0.83, 0.48, max(ring, glow) * _PurificationWave.z);
                }

                return overlay;
            }
            ENDHLSL
        }
    }
}
