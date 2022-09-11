using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspBotVK
{
    [Serializable]
    internal class Settings
    {
        public bool isFirstStart { get; set; }
        public string Token { get; set; }
        public int Timer { get; set; }

        public List<Rasp> groups { get; set; }

        public Settings()
        {
            groups = new List<Rasp>();
        }

        public void NewGroup(string type, string value, string valuevk)
        {
            Rasp rasp = new Rasp()
            {
                Id = groups.Count + 1,
                Type = type,
                Value = value,
                ValueIdGroupVk = valuevk
            };

            groups.Add(rasp);
        }

        public void DelTask(int id)
        {
            var temp = groups.FirstOrDefault(x => x.Id == id);
            groups.Remove(temp);
        }

        public string GetStringApiVk(string peerid, string text)
        {
            return $"https://api.vk.com/method/messages.send?&peer_id={peerid}&random_id=0&message={text}&access_token={Token}&v=5.131";
        }

        public string GetStringApiAsuForTeacher(string date, string id_teacher)
        {
            return $"https://asu.samgk.ru/api/schedule/teacher/{date}/{id_teacher}";
        }

        public string GetStringApiAsuForGroup(string date, string id_group)
        {
            return $"https://asu.samgk.ru/api/schedule/{id_group}/{date}/";
        }


    }
}
