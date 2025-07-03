Shader "Unlit/TerrainGen"
{
    Properties
    {
        _MapTex ("Map", 2D) = "white" {}
        _HeightMap ("Height Map", 2D) = "white" {}
        [Header(Red Texture Settings)]
        [Space(10)]
        _Tex1 ("Albedo", 2D) = "white" {}
        [Header(Green Texture Settings)]
        [Space(10)]
        _Tex2 ("Albedo", 2D) = "white" {}
        [Header(Blue Texture Settings)]
        [Space(10)]
        _Tex3 ("Albedo", 2D) = "white" {}
        [Header(Yellow Texture Settings)]
        [Space(10)]
        _Tex4 ("Albedo", 2D) = "white" {}
        [Header(Cyan Texture Settings)]
        [Space(10)]
        _Tex5 ("Albedo", 2D) = "white" {}
        [Header(Magenta Texture Settings)]
        [Space(10)]
        _Tex6 ("Albedo", 2D) = "white" {}
        [Header(White Texture Settings)]
        [Space(10)]
        _Tex7 ("Albedo", 2D) = "white" {}
        [Header(Black Texture Settings)]
        [Space(10)]
        _Tex8 ("Albedo", 2D) = "white" {}
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MapTex;
            sampler2D _Tex1;
            float4 _Tex1_ST;
            sampler2D _Tex2;
            float4 _Tex2_ST;
            sampler2D _Tex3;
            float4 _Tex3_ST;
            sampler2D _Tex4;
            float4 _Tex4_ST;
            sampler2D _Tex5;
            float4 _Tex5_ST;
            sampler2D _Tex6;
            float4 _Tex6_ST;
            sampler2D _Tex7;
            float4 _Tex7_ST;
            sampler2D _Tex8;
            float4 _Tex8_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 targetCol = tex2D(_MapTex, i.uv);
                float r = targetCol.r;
                float g = targetCol.g;
                float b = targetCol.b;
                float colMax = max(max(r,g), b);
                float condRl = step(r, 0.4);
                float condGl = step(g, 0.4);
                float condBl = step(b, 0.4);
                float condRm = step(0.1, r);
                float condGm = step(0.1, g);
                float condBm = step(0.1, b);
                fixed4 tex1 = tex2D(_Tex1, i.uv * _Tex1_ST.xy + _Tex1_ST.zw);
                fixed4 tex2 = tex2D(_Tex2, i.uv * _Tex2_ST.xy + _Tex2_ST.zw);
                fixed4 tex3 = tex2D(_Tex3, i.uv * _Tex3_ST.xy + _Tex3_ST.zw);
                fixed4 tex4 = tex2D(_Tex4, i.uv * _Tex4_ST.xy + _Tex4_ST.zw);
                fixed4 tex5 = tex2D(_Tex5, i.uv * _Tex5_ST.xy + _Tex5_ST.zw);
                fixed4 tex6 = tex2D(_Tex6, i.uv * _Tex6_ST.xy + _Tex6_ST.zw);
                fixed4 tex7 = tex2D(_Tex7, i.uv * _Tex7_ST.xy + _Tex7_ST.zw);
                fixed4 tex8 = tex2D(_Tex8, i.uv * _Tex8_ST.xy + _Tex8_ST.zw);
                
                fixed4 colR = lerp(0, colMax * tex1, condRm * condGl * condBl); //TODO: USE VALUE OF THE CHANNEL
                fixed4 colG = lerp(0, colMax * tex2, condRl * condGm * condBl);
                fixed4 colB = lerp(0, colMax * tex3, condRl * condGl * condBm);
                fixed4 colY = lerp(0, colMax * tex4, condRm *  condGm * condBl); //TODO: USE MAX OF THE MIX
                fixed4 colC = lerp(0, colMax * tex5, condRl * condGm * condBm);
                fixed4 colM = lerp(0, colMax * tex6, condRm * condGl * condBm);
                fixed4 colW = lerp(0, colMax/20 * (tex1 + tex2 + tex3), condRm * condGm * condBm);
                fixed4 colBl = lerp(0, (1 - colMax) * tex8, condRl * condGl * condBl);
                
                fixed4 col = colR + colG + colB + colY + colC + colM + colW + colBl;
                return col;
            }
            ENDCG
        }
    }
}
