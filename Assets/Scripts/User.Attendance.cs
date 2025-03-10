using System;
using System.Diagnostics;

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

        UnityEngine.Debug.Log(lastAttendanceReceivedDateTime);

        if (lastAttendanceReceivedDateTime.Year != GameData.DummyTime.Year)
            return false;
        if (lastAttendanceReceivedDateTime.Month != GameData.DummyTime.Month)
            return false;

        return lastAttendanceReceivedDateTime.Day >= GameData.DummyTime.Day;
    }

    public void RecordAttendanceRecevied()
    {
        _lastAttendanceReceivedTime = GameData.DummyTime;
        _attendanceLevel++;

        Save();

        NotifyValuesChanged();
    }

    public void ResetAttendanceIfMonthChanged()
    {
        if (_lastAttendanceReceivedTime.Year == GameData.DummyTime.Year &&
            _lastAttendanceReceivedTime.Month == GameData.DummyTime.Month)
            return;

        _attendanceLevel = 0;
        _lastAttendanceReceivedTime = default(DateTime);
    }
}
