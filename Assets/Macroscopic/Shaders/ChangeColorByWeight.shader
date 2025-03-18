Shader "Traffic/ChangeColorByWeight"
{
    Properties
    {
        _ColorTex ("Color Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _ColorTex;
            float4 _ColorTex_ST;
            float _NodeWeight1;
            float _NodeWeight2;
            float4 _NodePos1;
            float4 _NodePos2;

            v2f vert (appdata v)
            {
                v2f vOut;
                vOut.pos = UnityObjectToClipPos(v.vertex);
                float3 posW = mul(unity_ObjectToWorld, v.vertex).xyz;
                if(_NodeWeight1 == _NodeWeight1 && _NodeWeight2 == _NodeWeight2){
                    float2 roadDir = _NodePos2.xy - _NodePos1.xy;
                    float2 posDir = posW.xy - _NodePos1.xy;
                    float s = length(posDir) / length(roadDir);

                    vOut.uv.x = (_NodePos1.x == _NodePos2.x && _NodePos1.y == _NodePos2.y) ? 
                                    _NodeWeight1 : lerp(_NodeWeight1, _NodeWeight2, s);
                }
                else{
                    vOut.uv.x = 0.01;
                }
                vOut.uv.y = v.texcoord.y;

                if(vOut.uv.x < 0.01)
                    vOut.uv.x = 0.01;
                if(vOut.uv.x > 0.99)
                    vOut.uv.x = 0.99;

                return vOut;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(tex2D(_ColorTex, i.uv.xy).xyz, 1);
            }
            ENDCG
        }
    }
}
