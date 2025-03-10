public class AttendanceReward
{
    public int month;
    public int day;
    public ItemPackage[] items;

    public AttendanceReward(int month, int day, ItemPackage[] items)
    {
        this.month = month;
        this.day = day;
        this.items = items;
    }
}
