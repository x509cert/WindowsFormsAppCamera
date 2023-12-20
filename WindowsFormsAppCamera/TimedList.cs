using System;
using System.Collections.Generic;
using System.Timers;

namespace WindowsFormsAppCamera
{
    public class TimedItem
    {
        public DateTime Timestamp { get; set; }
        public string SomeString { get; set; }
        public int SomeInteger { get; set; }

        public TimedItem(DateTime timestamp, string someString, int someInteger)
        {
            Timestamp = timestamp;
            SomeString = someString;
            SomeInteger = someInteger;
        }
    }
    public class TimedList
    {
        private List<TimedItem> _items = new List<TimedItem>();
        private Timer           _cleanupTimer;
        private const int       _maxTime = 5; // 5 minutes
        public int              Count { get { return _items.Count; } }

        public TimedList()
        {
            // Set up a timer to age out the list
            _cleanupTimer = new Timer(60000); // 60 seconds
            _cleanupTimer.Elapsed += CleanupTimer_Elapsed;
            _cleanupTimer.Start();
        }

        private void CleanupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime threshold = DateTime.Now.AddMinutes(-_maxTime);
            _items.RemoveAll(item => item.Timestamp < threshold);
        }

        public void Add(TimedItem item)
        {
            _items.Add(item);
        }
    }
}
