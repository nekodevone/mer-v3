using Mirror;
using ProjectMER.Features.Interfaces;
using ProjectMER.Features.Serializable;
using UnityEngine;

namespace ProjectMER.Features.Objects;

public class IndicatorObject : MapEditorObject
{
	public static Dictionary<IndicatorObject, MapEditorObject> Dictionary = [];

	public static void ClearIndicators()
	{
		foreach (IndicatorObject indicator in Dictionary.Keys)
			indicator.Destroy();

		Dictionary.Clear();
	}

	public static void RefreshIndicators()
	{
		ClearIndicators();

		foreach (MapSchematic map in MapUtils.LoadedMaps.Values)
		{
			foreach (MapEditorObject mapEditorObject in map.SpawnedObjects)
			{
				if (mapEditorObject.Base is not IIndicatorDefinition indicatorDefinition)
					continue;

				GameObject indicator = indicatorDefinition.SpawnOrUpdateIndicator(mapEditorObject.Room);
                BoxCollider collider = indicator.AddComponent<BoxCollider>();
				collider.isTrigger = true;
				indicator.GetComponentsInChildren<NetworkIdentity>().ForEach(x => NetworkServer.Spawn(x.gameObject));
				Dictionary.Add(indicator.AddComponent<IndicatorObject>(), mapEditorObject);
			}
		}
	}
}
