using Defective.JSON;

public struct ItemPackage
{    
    public ulong itemId;
    public double quantity;
    public string contentDetailId;

    public ItemPackage(ulong itemId, double quantity, string contentDetailId)
    {
        this.itemId = itemId;
        this.quantity = quantity;
        this.contentDetailId = contentDetailId;
    }

    public ItemPackage(JSONObject json)
    {   
        string idString = json.GetField("id").stringValue;
        itemId = ulong.Parse(idString);
        string detailString = json.GetField("ContentDescriptIds")?.stringValue;
        contentDetailId = detailString;
        quantity = 0;
        var quantityJson = json.GetField("quantity");
        if (quantityJson.isString)
        {
            quantity = quantityJson.doubleValue;
        }
        else if (quantityJson.isNumber)
        {
            quantity = quantityJson.intValue;
        }
    }

    public JSONObject ToJsonObject()
    {
        var json = new JSONObject();
        json.AddField("id", itemId.ToString("D10"));
        json.AddField("quantity", quantity);
        return json;
    }
}
