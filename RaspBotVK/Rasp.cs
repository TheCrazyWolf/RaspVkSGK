using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspBotVK
{
    [Serializable]
    internal class Rasp
    {
        //Типа инкеремент
        public int Id { get; set; }

        //Тип расписания для студента или группы
        public string Type { get; set; }

        //Значение
        public string Value { get; set; }

        //Ид группы вк
        public string ValueIdGroupVk { get; set; }

        public string Result;


    }
}
