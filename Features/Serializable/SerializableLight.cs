using AdminToys;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Extensions;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ProjectMER.Features.Serializable;

public class SerializableLight : SerializableObject
{
	public string Color { get; set; } = "white";

	public float Intensity { get; set; } = 1f;

	public float Range { get; set; } = 1f;

	public LightShadows Shadows { get; set; } = LightShadows.Hard;
	
	public float Strength { get; set; } = 0f;

	public LightType Type { get; set; } = LightType.Point;

	public LightShape Shape { get; set; } = LightShape.Cone;

	public float SpotAngle { get; set; } = 30f;

	public float InnerSpotAngle { get; set; } = 0f;

	[YamlIgnore]
	public override string Scale { get; set; }

	public override GameObject SpawnOrUpdateObject(Room room, GameObject? instance = null)
	{
		LightSourceToy light;
		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);

		if (instance == null)
			light = UnityEngine.Object.Instantiate(PrefabManager.LightSourcePrefab, position, rotation);
		else
		{
			light = instance.GetComponent<LightSourceToy>();
			light.transform.position = position;
			light.transform.rotation = rotation;
		}

		if (ColorUtility.TryParseHtmlString(Color, out Color color))
			light.NetworkLightColor = color;

		light.NetworkLightIntensity = Intensity;
		light.NetworkLightRange = Range;
		light.NetworkShadowType = Shadows;
		light.NetworkShadowStrength = Strength;
		light.NetworkLightType = Type;
		light.NetworkLightShape = Shape;
		light.NetworkSpotAngle = SpotAngle;
		light.NetworkInnerSpotAngle = InnerSpotAngle;

		return light.gameObject;
	}
}
