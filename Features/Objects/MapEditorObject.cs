using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Serializable;
using UnityEngine;

namespace ProjectMER.Features.Objects;

public class MapEditorObject : MonoBehaviour
{
	private int _prevIndex;
	public SerializableObject Base;

	public string MapName { get; protected set; }

	public string Id { get; protected set; }

	public Room Room { get; protected set; }

	public MapEditorObject Init(SerializableObject serializableObject, string mapName, string id, Room room)
	{
		Base = serializableObject;
		_prevIndex = Base.Index;
		MapName = mapName;
		Id = id;
		Room = room;

		NetworkServer.Spawn(gameObject);
		return this;
	}

	public virtual void UpdateObjectAndCopies()
	{
		if (Base is SerializableDoor serializableDoor && serializableDoor._prevType != serializableDoor.Type)
		{
			if (MapUtils.LoadedMaps.TryGetValue(MapName, out MapSchematic map))
			{
				map.DestroyObject(Id);
			}

			Timing.CallDelayed(0.1f, () => map.SpawnObject(Id, Base));
			return;
		}

		if (_prevIndex != Base.Index)
		{
			if (MapUtils.LoadedMaps.TryGetValue(MapName, out MapSchematic map))
			{
				map.DestroyObject(Id);
			}

			Timing.CallDelayed(0.1f, () => map.SpawnObject(Id, Base));
			return;
		}

		foreach (MapEditorObject copy in MapUtils.LoadedMaps[MapName].SpawnedObjects.ToList())
		{
			if (copy.Id != Id)
				continue;

			copy.UpdateCopy();
		}

		_prevIndex = Base.Index;
	}

	private void UpdateCopy()
	{
		IndicatorObject.TrySpawnOrUpdateIndicator(this);
		Base.SpawnOrUpdateObject(Room, gameObject);
	}

	public Vector3 RelativePosition
	{
		get => Base.Position.ToVector3();
		set => Base.Position = value.ToString("F3");
	}

	/// <summary>
	/// Destroys the object.
	/// </summary>
	public void Destroy()
	{
		IndicatorObject.TryDestroyIndicator(this);
		Destroy(gameObject);
	}
}
