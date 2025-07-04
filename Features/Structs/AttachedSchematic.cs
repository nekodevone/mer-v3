using LabApi.Features.Wrappers;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace ProjectMER.Features.Structs;

public class AttachedSchematic
{
    public SchematicObject Schematic { get; set; }

    public Player Player { get; set; }

    public Transform OriginalTransform { get; set; }
}