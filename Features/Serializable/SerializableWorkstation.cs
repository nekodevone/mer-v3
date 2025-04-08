using InventorySystem.Items.Firearms.Attachments;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Extensions;
using UnityEngine;

namespace ProjectMER.Features.Serializable;

public class SerializableWorkstation : SerializableObject
{
	/// <summary>
	/// Gets or sets a value indicating whether the player can interact with the workstation.
	/// </summary>
	public bool IsInteractable { get; set; } = true;

	public override GameObject SpawnOrUpdateObject(Room room, GameObject? instance = null)
	{
		WorkstationController workstation;
		Vector3 position = room.GetRelativePosition(Position);
		Quaternion rotation = room.GetRelativeRotation(Rotation);

		if (instance == null)
			workstation = UnityEngine.Object.Instantiate(PrefabManager.WorkstationPrefab);
		else
		{
			workstation = instance.GetComponent<WorkstationController>();
		}

		workstation.transform.SetPositionAndRotation(position, rotation);
		workstation.transform.localScale = Scale.ToVector3();

		workstation.NetworkStatus = (byte)(IsInteractable ? 0 : 4);

		return workstation.gameObject;
	}
}
