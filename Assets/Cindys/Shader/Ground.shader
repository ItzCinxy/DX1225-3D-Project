Shader "Custom/ChannelMappingShader"
{
    Properties
    {
        _Pixel("Pixel", float) = 10
        _MainTex("Base Texture", 2D) = "white" { }
        _Tex1("Texture 1 (Red)", 2D) = "white" { }
        _Tex2("Texture 2 (Blue)", 2D) = "white" { }
        _Tex3("Texture 3 (Green)", 2D) = "white" { }
        
        _MainTex_ST("Main Texture Tiling", Vector) = (1, 1, 0, 0)
        _Tex1_ST("Texture 1 Tiling", Vector) = (1, 1, 0, 0)
        _Tex2_ST("Texture 2 Tiling", Vector) = (1, 1, 0, 0)
        _Tex3_ST("Texture 3 Tiling", Vector) = (1, 1, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _Tex1;
            sampler2D _Tex2;
            sampler2D _Tex3;
            float _Pixel;
            float4 _MainTex_ST;
            float4 _Tex1_ST;
            float4 _Tex2_ST;
            float4 _Tex3_ST;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Apply tiling to the UV coordinates
                float2 mainTexUV = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                float2 tex1UV = i.uv * _Tex1_ST.xy + _Tex1_ST.zw;
                float2 tex2UV = i.uv * _Tex2_ST.xy + _Tex2_ST.zw;
                float2 tex3UV = i.uv * _Tex3_ST.xy + _Tex3_ST.zw;

                // Sample the main texture (which has RGB channels)
                float4 mainTexColor = tex2D(_MainTex, mainTexUV);

                // Create colors for each texture based on the main texture's color channels
                float4 tex1Color = tex2D(_Tex1, tex1UV);
                tex1Color.rgb = mainTexColor.r * tex1Color.rgb; // Use red channel from _MainTex

                float4 tex2Color = tex2D(_Tex2, tex2UV);
                tex2Color.rgb = mainTexColor.b * tex2Color.rgb; // Use blue channel from _MainTex

                float4 tex3Color = tex2D(_Tex3, tex3UV);
                tex3Color.rgb = mainTexColor.g * tex3Color.rgb; // Use green channel from _MainTex

                // Combine the textures (add them or use another blending method)
                float4 finalColor = tex1Color * 0.33 + tex2Color * 0.33 + tex3Color * 0.33;

                return finalColor;
            }
            ENDCG
        }
    }
}
