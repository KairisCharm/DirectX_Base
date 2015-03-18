//===================================================
// constants
//===================================================
cbuffer ViewBuffer
{
    matrix gViewMatrix;
	matrix gProjectionMatrix;
	float2 gScreenSize;
};



Texture2D gTextTexture : register(t0);
Texture2D gSpriteTexture : register(t1);
Texture2D gOtherTexture : register(t2);
SamplerState gSamplerType;



//===================================================
// utilities
//===================================================
float4 ScaleVert(float4 inVert)
{
	float halfScreenWidth = gScreenSize.x / 2;
	float halfScreenHeight = gScreenSize.y / 2;

	inVert.x = (inVert.x - halfScreenWidth) / halfScreenWidth;
	inVert.y = (inVert.y - halfScreenHeight) / halfScreenHeight;

	return inVert;
}



float2 RotatePoint(float2 inPoint, float inRadius, float inRotation)
{
	inPoint.x = inPoint.x + inRadius * sin(inRotation);
	inPoint.y = inPoint.y + inRadius * cos(inRotation);

	return inPoint;
}



float4 ProjectVert(float4 inVert)
{
	inVert = mul(gViewMatrix, inVert);
    inVert = mul(gProjectionMatrix, inVert);

	return inVert;
}



//===================================================
// line and polygon layout
//===================================================
struct VS_Line_Polygon_Input
{
    float2 mPosition : POSITION;
    float4 mColor : COLOR;
};



struct PS_Line_Polygon_Input
{
    float4 mPosition : SV_POSITION;
    float4 mColor : COLOR;
};



PS_Line_Polygon_Input VS_Line_Polygon(VS_Line_Polygon_Input input)
{
	PS_Line_Polygon_Input output;

    output.mPosition = ProjectVert(float4(input.mPosition.x, input.mPosition.y, 0, 1));
	output.mColor = input.mColor;

	return output;
}



float4 PS_Line_Polygon(PS_Line_Polygon_Input input) : SV_Target
{
	return input.mColor;
}


//===================================================
// texture layout
//===================================================
struct VS_Texture_Input
{
	float2 mPosition : POSITION;
	float2 mTexCoord : TEXCOORD;
};



struct PS_Texture_Input
{
	float4 mPosition : SV_POSITION;
	float2 mTexCoord : TEXCOORD;
};



PS_Texture_Input VS_Texture(VS_Texture_Input input)
{
	PS_Texture_Input output;

	output.mPosition = ProjectVert(float4(input.mPosition.x, input.mPosition.y, 0, 1));
	output.mTexCoord = input.mTexCoord;

	return output;
}



float4 PS_Texture(PS_Texture_Input input) : SV_Target
{
	return gOtherTexture.Sample(gSamplerType, float2(input.mTexCoord.x, input.mTexCoord.y));
}



//===================================================
// sprite layout
//===================================================
struct VS_Sprite_Input
{
	float2 mPosition : POSITION;
	float mSizePercent : SIZEPERCENT;
	float mImageWidth : IMAGEWIDTH;
	int mTextureID : TEXID;
	float mTextureStart : INDEX;
	float mRotation : ROTATION;
	float mXOffset : XOFFSET;
};



struct PS_Sprite_Input
{
	float4 mPosition : SV_POSITION;
	float2 mTexCoord : TEXCOORD;
	int mTextureID : TEXID;
};



VS_Sprite_Input VS_Sprite(VS_Sprite_Input input)
{
	return input;
}



[maxvertexcount(4)]
void GS_Sprite(point VS_Sprite_Input input[1], inout TriangleStream<PS_Sprite_Input> OutputStream)
{
	uint mipLevel = 0, width = 0, height = 0, levelCount = 0;

	switch(input[0].mTextureID)
	{
	case 0:
		gTextTexture.GetDimensions(mipLevel, width, height, levelCount);
		break;
	case 1:
		gSpriteTexture.GetDimensions(mipLevel, width, height, levelCount);
		break;
	}

	float halfScreenWidth = gScreenSize.x / 2;
	float halfScreenHeight = gScreenSize.y / 2;
	float halfImageWidth = (input[0].mImageWidth / 2) * input[0].mSizePercent;
	float halfImageHeight = (height / 2) * input[0].mSizePercent;
	
	float4 mPosition = ProjectVert(float4(input[0].mPosition.x, input[0].mPosition.y, 0, 1));

	mPosition.x /= mPosition.w;
	mPosition.y /= mPosition.w;

	mPosition = float4(mPosition.x * halfScreenWidth + halfScreenWidth + input[0].mXOffset, mPosition.y * halfScreenHeight + halfScreenHeight, 0, 1);

	float4 vert[4];

	float rotation = input[0].mRotation;

	float2 newPoint = RotatePoint(float2(mPosition.x, mPosition.y), halfImageHeight, rotation);
	vert[0] = float4(newPoint.x, newPoint.y, 0, 1);

	newPoint = RotatePoint(float2(mPosition.x, mPosition.y), -halfImageHeight, rotation);
	vert[2] = float4(newPoint.x, newPoint.y, 0, 1);

	rotation += radians(90);

	newPoint = RotatePoint(float2(vert[0].x, vert[0].y), halfImageWidth, rotation);
	vert[1] = float4(newPoint.x, newPoint.y, 0, 1);

	newPoint = RotatePoint(float2(vert[0].x, vert[0].y), -halfImageWidth, rotation);
	vert[0] = float4(newPoint.x, newPoint.y, 0, 1);

	newPoint = RotatePoint(float2(vert[2].x, vert[2].y), halfImageWidth, rotation);
	vert[3] = float4(newPoint.x, newPoint.y, 0, 1);

	newPoint = RotatePoint(float2(vert[2].x, vert[2].y), -halfImageWidth, rotation);
	vert[2] = float4(newPoint.x, newPoint.y, 0, 1);

	for(int i = 0; i < 4; i++)
		vert[i] = ScaleVert(vert[i]);

	float left = (input[0].mTextureStart / width);
	float right = ((input[0].mTextureStart + input[0].mImageWidth) / width);

	// Get billboard's texture coordinates
	float2 texCoord[4];
	texCoord[0] = float2(left, 0);
	texCoord[1] = float2(right, 0);
	texCoord[2] = float2(left, 1);
	texCoord[3] = float2(right, 1);

	PS_Sprite_Input outputVert;

	for(int i = 0; i < 4; i++)
	{
		outputVert.mPosition = vert[i];
		outputVert.mTexCoord = texCoord[i];
		outputVert.mTextureID = input[0].mTextureID;

		OutputStream.Append(outputVert);
	}

	OutputStream.RestartStrip();
}



float4 PS_Sprite(PS_Sprite_Input input) : SV_Target
{
	float4 textureSample = 0;
	
	switch(input.mTextureID)
	{
	case 0:
		textureSample = gTextTexture.Sample(gSamplerType, float2(input.mTexCoord.x, input.mTexCoord.y));
		break;
	case 1:
		textureSample = gSpriteTexture.Sample(gSamplerType, float2(input.mTexCoord.x, input.mTexCoord.y));
		break;
	}

	return float4(textureSample.x, textureSample.y, textureSample.z, textureSample.w);
}