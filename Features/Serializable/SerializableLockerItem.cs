﻿using InventorySystem.Items.Firearms.Attachments;

namespace ProjectMER.Features.Serializable
{
    public class SerializableLockerItem
    {
        public SerializableLockerItem()
        {
        }

        public SerializableLockerItem(ItemType item, uint count, List<AttachmentName> attachments, int chance)
        {
            Item = item;
            Count = count;
            Attachments = attachments;
            Chance = chance;
        }

        public ItemType Item { get; set; } = ItemType.None;

        public uint Count { get; set; } = 1;

        public List<AttachmentName> Attachments { get; set; }

        public int Chance { get; set; } = 100;

    }
}