using System;

public class UserItem
{
    Guid _guId;

    public ulong ItemId { get; set; }
    public double Quantity { get; set; }

    public int QuantityGroupPer
    {
        get
        {
            return GameData.Instance.GetItem(ItemId).QuantityGroupPer;
        }
    }

    public Item.ItemCategory Category
    {
        get
        {
            return GameData.Instance.GetItem(ItemId).Category;
        }
    }

    public UserItem(ulong itemId, int quantity)
    {
        this.ItemId = itemId;
        this.Quantity = quantity;

        _guId = Guid.NewGuid();
    }

    public double AddQuantity(double count)
    {
        if (count < 0)
        {
            return -1;
        }
        if (Quantity + count > QuantityGroupPer)
        {
            double tempValue = Quantity + count - QuantityGroupPer;
            Quantity = QuantityGroupPer;
            return tempValue;
        }
        else
            Quantity += count;

        return 0;

    }

    public double SubQuantity(double count)
    {
        if (count < 0)
        {
            return -1;
        }
        Quantity -= count;
        if (Quantity < 0)
        {
            double tempValue = Quantity;
            Quantity = 0;
            return tempValue * -1;
        }
        return 0;
    }

}
