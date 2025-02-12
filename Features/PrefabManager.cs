using AdminToys;
using Mirror;
using UnityEngine;

namespace ProjectMER.Features;

public static class PrefabManager
{
	public static PrimitiveObjectToy PrimitiveObjectPrefab { get; private set; }

	public static LightSourceToy LightSourcePrefab { get; private set; }

	public static void RegisterPrefabs()
	{
		foreach (GameObject gameObject in NetworkClient.prefabs.Values)
		{
			if (gameObject.TryGetComponent(out PrimitiveObjectToy primitiveObjectToy))
			{
				PrimitiveObjectPrefab = primitiveObjectToy;
				continue;
			}

			if (gameObject.TryGetComponent(out LightSourceToy lightSourceToy))
			{
				LightSourcePrefab = lightSourceToy;
				continue;
			}
		}
	}
}
