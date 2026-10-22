using System;

namespace Practice8
{
    public class PlannerItem
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public DateTime Deadline { get; set; }

        public bool IsCompleted { get; set; }

        public bool HasReminder { get; set; }

        public int ReminderMinutesBefore { get; set; }

        public bool ReminderShown { get; set; }
    }
}