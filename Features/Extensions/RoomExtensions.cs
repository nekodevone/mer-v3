using LabApi.Features.Wrappers;
using MapGeneration;
using NorthwoodLib.Pools;
using ProjectMER.Features.Serializable;
using UnityEngine;

namespace ProjectMER.Features.Extensions;

public static class RoomExtensions
{
	public static string GetRoomStringId(this Room room) => $"{room.Zone}_{room.Shape}_{room.Name}";

	public static List<Room> GetRooms(this SerializableObject serializableObject)
	{
		string[] split = serializableObject.Room.Split('_');
		if (split.Length != 3)
			return ListPool<Room>.Shared.Rent(Room.List.Where(x => x.Name == RoomName.Outside));

		FacilityZone facilityZone = (FacilityZone)Enum.Parse(typeof(FacilityZone), split[0], true);
		RoomShape roomShape = (RoomShape)Enum.Parse(typeof(RoomShape), split[1], true);
		RoomName roomName = (RoomName)Enum.Parse(typeof(RoomName), split[2], true);

		return ListPool<Room>.Shared.Rent(Room.List.Where(x => x.Zone == facilityZone && x.Shape == roomShape && x.Name == roomName));
	}

	public static int GetRoomIndex(this Room room)
	{
        List<Room> list = ListPool<Room>.Shared.Rent(Room.List.Where(x => x.Zone == room.Zone && x.Shape == room.Shape && x.Name == room.Name));
		int index = list.IndexOf(room);
		ListPool<Room>.Shared.Return(list);
		return index;
	}

    public static Vector3 GetRelativePosition(this Room room, string position) => GetRelativePosition(room, position.ToVector3());

    public static Vector3 GetRelativePosition(this Room room, Vector3 position)
	{
		if (room.Name == RoomName.Outside)
			return position;

		return room.Transform.TransformPoint(position);
	}

	public static Quaternion GetRelativeRotation(this Room room, string rotation)
	{
		if (room.Name == RoomName.Outside)
			return Quaternion.Euler(rotation.ToVector3());

		return room.Transform.rotation * Quaternion.Euler(rotation.ToVector3());
	}
}
