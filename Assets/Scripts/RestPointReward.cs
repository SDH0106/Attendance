public class RestPointReward
{
    public string eventCode;
    public ItemPackage[] items;

    public RestPointReward(string eventCode, ItemPackage[] items)
    {
        this.eventCode = eventCode;
        this.items = items;
    }
}
