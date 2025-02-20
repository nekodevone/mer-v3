using LabApi.Features.Wrappers;
using PlayerRoles;
using ProjectMER.Features.Extensions;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ProjectMER.Features.Serializable;

public class SerializablePlayerSpawnpoint : SerializableObject
{
	public List<RoleTypeId> Roles { get; set; } = [];

	[YamlIgnore]
	public override string Scale { get; set; }

	public override GameObject SpawnOrUpdateObject(Room room, GameObject? instance = null)
	{
		GameObject teleport;
		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);

		if (instance == null)
			teleport = new GameObject("PlayerSpawnPoint");
		else
		{
			teleport = instance;
		}

		teleport.transform.position = position;
		teleport.transform.rotation = rotation;

		return teleport.gameObject;
	}
}
