using System;

namespace Template.Service.Tasks
{
    [AttributeUsage(AttributeTargets.Class)]
    class TaskRepeatAttribute : Attribute
    {
        private int _interval;

        /// <summary>
        /// Interval in seconds
        /// </summary>
        public int IntervalInSeconds
        {
            get { return this._interval; }
            set { this._interval = value; }
        }
    }
}
