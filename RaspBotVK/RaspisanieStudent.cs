using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspBotVK
{
    public class RaspisanieStudent
    {
        public string date { get; set; }
        public List<LessonsStudent> lessons { get; set; }
    }

    public class LessonsStudent
    {
        public string title { get; set; }
        public string num { get; set; }
        public string teachername { get; set; }
        public object nameGroup { get; set; }
        public string cab { get; set; }
        public string resource { get; set; }
    }
}
