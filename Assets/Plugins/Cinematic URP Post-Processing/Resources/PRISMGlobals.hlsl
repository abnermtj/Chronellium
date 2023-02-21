
struct MyCustomData
{
	half3 something;
	half3 somethingElse;
};
uniform RWStructuredBuffer<MyCustomData> _MyCustomBuffer : register(u1);

//float timeVal = max(unity_DeltaTime.b * 0.005), 0.01
