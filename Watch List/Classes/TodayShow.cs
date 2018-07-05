using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watch_List.Classes
{
    public class TodayShow
    {
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public TodayShow() { }
        public TodayShow(string name, DateTime time)
        {
            Name = name;
            Time = time;
        }
    }
}
