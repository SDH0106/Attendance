using System;

public class Item
{
    public enum ItemCategory
    {
        Normal, Event, Key, Dummy
    }

    public ulong ItemId { get; set; }
    public ulong IconId { get; set; }
    public string Name;
    public string Description;
    public int QuantityGroupPer { get; set; }
    public ItemCategory Category { get; set; }

    public Item() { }

    public Item(ulong id,int category, ulong icon, string name, string description, int quantityGroupPer)
    {
        this.ItemId = id;
        this.Category = (ItemCategory)category;
        this.IconId = icon;
        this.Name = name;
        this.Description = description;
        this.QuantityGroupPer = quantityGroupPer;
    }
}