using System;

public partial class User // Attendance
{
    DateTime _lastAttendanceReceivedTime;
    int _attendanceLevel;
    double _restPoint;

    public DateTime LastAttendanceReceivedTime
    {
        get { return _lastAttendanceReceivedTime; }
    }

    public int AttendanceLevel
    {
        get { return _attendanceLevel; }
    }

    public double RestPoint
    {
        get { return _restPoint; }
        set
        {
            _restPoint = value;
        }
    }

    public bool IsReceivedAttendanceToday()
    {
        var lastAttendanceReceivedDateTime = _lastAttendanceReceivedTime;

        if (lastAttendanceReceivedDateTime.Year != DateTime.Now.Year)
            return false;
        if (lastAttendanceReceivedDateTime.Month != DateTime.Now.Month)
            return false;

        return lastAttendanceReceivedDateTime.Day >= DateTime.Now.Day;
    }

    public void RecordAttendanceRecevied()
    {
        _lastAttendanceReceivedTime = DateTime.Now;
        _attendanceLevel++;

        Save();

        NotifyValuesChanged();
    }

    public void ResetAttendanceIfMonthChanged()
    {
        if (_lastAttendanceReceivedTime.Year == DateTime.Now.Year &&
            _lastAttendanceReceivedTime.Month == DateTime.Now.Month)
            return;

        _attendanceLevel = 0;
        _lastAttendanceReceivedTime = default(DateTime);
    }
}
