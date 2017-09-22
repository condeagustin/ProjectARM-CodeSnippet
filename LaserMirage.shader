Shader "ProjectARM/LaserMirage"
{
	Properties 
	{

		_BumpMap ("Noise text", 2D) = "bump" {}
		_Magnitude ("Magnitude", Range(0,0.01)) = 0.003
		_LaserDistance ("Laser distance", float) = 1
	}
	
	SubShader
	{
		Tags {
			"Queue"="Transparent" 
			//"IgnoreProjector"="True" 
			//"RenderType"="Opaque"
		}

		Blend One Zero
 
		GrabPass { "_LaserMirageGrabTexture" }

		Pass 
		{
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
 
			sampler2D _LaserMirageGrabTexture;
 
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			float  _Magnitude;
			float _LaserDistance;
 
			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
 
			struct VertexOutput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 uvgrab : TEXCOORD1;
			};
 
			// Vertex function 
			VertexOutput vert (VertexInput vertexIn)
			{
				VertexOutput vertexOut;
				vertexOut.vertex = mul(UNITY_MATRIX_MVP, vertexIn.vertex);
				//this is for having into account the tiling and offset
				vertexOut.texcoord = TRANSFORM_TEX(vertexIn.texcoord, _BumpMap);
				#if UNITY_UV_STARTS_AT_TOP
					vertexOut.texcoord.y = 1-vertexOut.texcoord.y;
				#endif

				vertexOut.uvgrab = ComputeGrabScreenPos(vertexOut.vertex);
				//vertexOut.uvgrab = ComputeGrabScreenPos(vertexOut.vertex) * _BumpMap_ST.xy + _BumpMap_ST.zw;
				return vertexOut;
			}
 
			// Fragment function
			half4 frag (VertexOutput vertexOut) : COLOR
			{
				float texturePortionX = _LaserDistance / 8;
				vertexOut.texcoord.x = vertexOut.texcoord.x * texturePortionX;

				vertexOut.texcoord.x -= _Time[1];

				half4 bump = tex2D(_BumpMap, vertexOut.texcoord);

				//convert from  to color [0, 1] to normal [-1, 1]
				half2 distortion = UnpackNormal(bump).rg;

				//apply the bump times a magnitude to the grab texture uv coordinates
				vertexOut.uvgrab.xy -=  distortion * _Magnitude;

				fixed4 grabScreenColor = tex2Dproj( _LaserMirageGrabTexture, UNITY_PROJ_COORD(vertexOut.uvgrab));
				//fixed4 grabScreenColor = tex2D( _LaserMirageGrabTexture, vertexOut.uvgrab);

				return grabScreenColor;

			}
		
			ENDCG
		} 
	}
}
