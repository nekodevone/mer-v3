using System;
using AdminToys;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Extensions;
using UnityEngine;

namespace ProjectMER.Features.Serializable;

public class SerializableCapybara : SerializableObject
{
	public override GameObject SpawnOrUpdateObject(Room room, GameObject? instance = null)
	{
		CapybaraToy capybara;
		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);

		if (instance == null)
			capybara = UnityEngine.Object.Instantiate(PrefabManager.CapybaraPrefab);
		else
		{
			capybara = instance.GetComponent<CapybaraToy>();
		}

		capybara.transform.SetPositionAndRotation(position, rotation);
		capybara.transform.localScale = Scale.ToVector3();

		capybara.Network_collisionsEnabled = true;

		return capybara.gameObject;
	}
}
