using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TextMe.Models
{
    public class EventModel
    {
        public int ID { get; set; }
        public string Action { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        private DateTime time;
        public DateTime Time
        {
            get
            {
                return time;
            }

            set
            {
                time = value.ToUniversalTime();
            }
        }
        public string Phone { get; set; }
    }
}