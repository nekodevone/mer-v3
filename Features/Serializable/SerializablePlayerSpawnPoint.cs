using AdminToys;
using LabApi.Features.Wrappers;
using PlayerRoles;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using Respawning.Objectives;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ProjectMER.Features.Serializable;

public class SerializablePlayerSpawnpoint : SerializableObject, IIndicatorDefinition
{
	public List<RoleTypeId> Roles { get; set; } = [];

	[YamlIgnore]
	public override string Scale { get; set; }

	public override GameObject SpawnOrUpdateObject(Room room, GameObject? instance = null)
	{
		GameObject spawnpoint;
		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);

		if (instance == null)
			spawnpoint = new GameObject("PlayerSpawnPoint");
		else
		{
			spawnpoint = instance;
		}

		spawnpoint.transform.position = position;
		spawnpoint.transform.rotation = rotation;

		return spawnpoint.gameObject;
	}

	public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
	{
		Transform root;
		PrimitiveObjectToy cylinder;
		Transform arrowY;
		Transform arrowX;
		PrimitiveObjectToy arrow;

		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);

		if (instance == null)
		{
			root = new GameObject("Indicator").transform;
			root.position = position;

			cylinder = GameObject.Instantiate(PrefabManager.PrimitiveObjectPrefab, root.transform);
			cylinder.transform.localPosition = Vector3.zero;
			cylinder.NetworkPrimitiveType = PrimitiveType.Cylinder;
			cylinder.NetworkPrimitiveFlags = PrimitiveFlags.Visible;
			cylinder.transform.localScale = new Vector3(1f, 0.001f, 1f);

			arrowY = new GameObject("Arrow Y Axis").transform;
			arrowY.parent = root.transform;
			// arrowY.transform.localPosition = Vector3.up;

			arrowX = new GameObject("Arrow X Axis").transform;
			arrowX.parent = arrowY.transform;
			// arrowX.transform.localPosition = Vector3.zero;

			arrow = GameObject.Instantiate(PrefabManager.PrimitiveObjectPrefab, arrowX.transform);
			arrow.transform.localPosition = root.forward;
			arrow.NetworkPrimitiveType = PrimitiveType.Cube;
			arrow.NetworkPrimitiveFlags = PrimitiveFlags.Visible;
			arrow.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
		}
		// primitive = UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObjectPrefab, position, rotation);
		else
		{
			root = instance.transform;
			
			arrowY = root.Find("Arrow Y Axis");
			arrowX = arrowY.Find("Arrow X Axis");

			// root.transform.rotation = rotation;
		}

		root.position = position;
		arrowY.localPosition = Vector3.up * 1.6f;
		arrowY.localEulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
		arrowX.localPosition = Vector3.zero;
		arrowX.localEulerAngles = new Vector3(-rotation.eulerAngles.x, 0f, 0f);

		foreach (PrimitiveObjectToy primitive in root.GetComponentsInChildren<PrimitiveObjectToy>())
		{
			if (Roles.Count > 0)
			{
				Color colorSum = new(0f, 0f, 0f, 1f);
				foreach (RoleTypeId roleType in Roles)
				{
					Color roleColor = roleType.GetRoleColor();
					colorSum.r += roleColor.r;
					colorSum.g += roleColor.g;
					colorSum.b += roleColor.b;
				}

				primitive.NetworkMaterialColor = new Color(colorSum.r / Roles.Count, colorSum.g / Roles.Count, colorSum.b / Roles.Count, colorSum.a);
			}
			else
			{
				primitive.NetworkMaterialColor = new Color(1f, 1f, 1f, 0.25f);
			}
		}

		return root.gameObject;
	}
}
