using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Xml.Serialization;

namespace RaspBotVK
{
    internal class Program
    {

        static Settings prop = new Settings();
 
        static void Main(string[] args)
        {
            Write("БОТЯРА ДЛЯ ОТПРАВКИ РАСПИСАНИЯ В ВК ИЗ СГК");
            
            LoadSettings();


            //Пускаю выполнятся отдельно от нас
            Task.Run(() => Thead());

            while (true)
            {
                Command(Console.ReadLine());
            }
        }

        /// <summary>
        /// Мировые запросы и ответы
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static string Response(string url)
        {
            using (var wb = new WebClient())
            {
                wb.Headers.Set("Accept", "application/json");
                return wb.DownloadString(url);
            }

        }
        static void RaspStudent(int id)
        {
            try
            {
                var item = prop.groups.FirstOrDefault(x => x.Id == id);

                Write($"Task # {item.Id}. Вытягиваем расписание из АСУ СГК");

                string response_nextday = Response(prop.GetStringApiAsuForGroup(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), item.Value));

                if (item.Result != response_nextday)
                {
                   // string response_today = Response(prop.GetStringApiAsuForGroup(DateTime.Now.ToString("yyyy-MM-dd"), item.Value));

                    Write($"Task # {item.Id}. Парсинг данных");
                    var parse_nextday = JsonSerializer.Deserialize<RaspisanieStudent>(response_nextday);
                    //var parse_today = JsonSerializer.Deserialize<RaspisanieStudent>(response_today);

                    string text = "";

                    // text= $"\n{parse_today.date}";
                    //foreach (var itemToday in parse_today.lessons)
                    //{
                    //    text += $"\n[{itemToday.num}] {itemToday.title} ({itemToday.teachername}) {itemToday.cab}";
                    //}

                    // text += $"\n\n{parse_nextday.date}";

                    foreach (var itemNext in parse_nextday.lessons)
                    {
                        text += $"\n[{itemNext.num}] {itemNext.title} ({itemNext.teachername}) {itemNext.cab}";
                    }

                    Write($"Task # {item.Id}. Отправка расписание в ВК в ЛС - ({item.ValueIdGroupVk}). Тип: {item.Type}.");
                    var vk_response = Response(prop.GetStringApiVk(item.ValueIdGroupVk, text));
                    item.Result = response_nextday;
                }

                Write($"Task # {item.Id}. Ничего не изменилось");
            }
            catch (Exception ex)
            {
                Write(ex.ToString());
            }
        }
        static void RaspTeacher(int id)
        {
            try
            {
                var item = prop.groups.FirstOrDefault(x => x.Id == id);

                Write($"Task # {item.Id}. Вытягиваем расписание из АСУ СГК");

                string response_nextday = Response(prop.GetStringApiAsuForTeacher(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), item.Value));

                if (item.Result != response_nextday)
                {
                    string response_today = Response(prop.GetStringApiAsuForTeacher(DateTime.Now.ToString("yyyy-MM-dd"), item.Value));

                    var parse_nextday = JsonSerializer.Deserialize<RaspisanieTeacher>(response_nextday);
                    var parse_today = JsonSerializer.Deserialize<RaspisanieTeacher>(response_today);

                    string text = "";

                    // text = $"\n{parse_today.date}";

                    //foreach (var itemToday in parse_today.lessons)
                    //{
                    //    text += $"\n[{itemToday.num}] [{itemToday.nameGroup}] {itemToday.title} {itemToday.cab}";
                    //}

                    text += $"\n\n{parse_nextday.date}";

                    foreach (var itemNext in parse_nextday.lessons)
                    {
                        text += $"\n[{itemNext.num}] [{itemNext.nameGroup}] {itemNext.title} {itemNext.cab}";
                    }

                    Write($"Task # {item.Id}. Отправка расписание в ВК в ЛС - ({item.ValueIdGroupVk}). Тип: {item.Type}.");

                    var vk_response = Response(prop.GetStringApiVk(item.ValueIdGroupVk, text));
                    item.Result = response_nextday;
                }

                Write($"Task # {item.Id}. Ничего не изменилось");
            }
            catch (Exception ex)
            {
                Write(ex.ToString());
            }
        }
        static void Thead()
        {
            while (true)
            {
                Thread.Sleep(prop.Timer);


                foreach (var item in prop.groups)
                {
                    switch (item.Type)
                    {
                        case "S":
                            Write($"Task # {item.Id}. Вызов задачи");
                            RaspStudent(item.Id);
                            break;

                        case "T":
                            Write($"Task # {item.Id}. Вызов задачи");
                            RaspTeacher(item.Id);
                            break;

                        default:
                            break;
                    }

                    //Задержка между тасками
                    Thread.Sleep(2000);
                }

            }
        }



        static void Write(string msg)
        {
            Console.Write($"[{DateTime.Now}] {msg}\n");
        }
        static void LoadSettings()
        {
            switch (File.Exists("props.json"))
            {
                case true:
                    try
                    {
                        string json = File.ReadAllText("props.json");
                        prop = JsonSerializer.Deserialize<Settings>(json);
                        Write("Настройки успешно загружены");
                    }
                    catch (Exception ex)
                    {
                        Write($"ПРОИЗОШЛА ОШИБКА ПРИ ЗАГРУЗКИ НАСТРОЕК \n {ex.Message.ToString()}");
                    }
                    break;

                case false:
                    NewSettings();
                    SaveSettings();
                    break;
            }

        }
        static void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize<Settings>(prop);

                File.WriteAllText("props.json", json);
            }
            catch (Exception ex)
            {
                Write($"ПРОИЗОШЛА ОШИБКА ПРИ СОХРАНЕНИИ НАСТРОЕК \n {ex.Message.ToString()}");
            }
        }


        //ВЫПОЛНЕНИЕ КОМАНД ИЗ МЕНЮ
        static void Command(string cmd)
        {
            switch (cmd)
            {
                case "help":
                    Write("Какие команды здесь есть - !");
                    Write("help - помощь");
                    Write("add - добавление задач");
                    Write("del - удаление задач");
                    Write("show teachers - получить список преподавателей");
                    Write("show groups - получить список групп");
                    Write("settings show - показ настроек программы");
                    Write("settings set token - задать новый токен от ВК");
                    Write("settings set time - задать задержку между запросами");
                    break;
                case "add":
                    AddNewValue();
                    break;
                case "del":
                    DelValueTask();
                    break;
                case "show teachers":
                    ShowTeacher();
                    break;
                case "show groups":
                    ShowGroups();
                    break;
                case "settings show":
                    Write($"Установлены следующие настройки:");
                    Write($"Токен ВК - {prop.Token}");
                    Write($"Ожидание (мс) - {prop.Timer}");
                    break;
                case "settings set token":
                    ChangeToken();
                    break;
                case "settings set time":
                    ChangeTimer();
                    break;

                case "clear":
                case "cls":
                    Console.Clear();
                    Write("Консоль очищена");
                    break;
                default:
                    Write($"Неизвестная команда");
                    break;
            }
        }
        static void ShowGroups()
        {
            Write("Выполняется запрос ожидайте..");

            List<GroupsArray> groups = new List<GroupsArray>();
            try
            {
                var json = Response("https://mfc.samgk.ru/api/groups");
                groups = JsonSerializer.Deserialize<List<GroupsArray>>(json);

            }
            catch (Exception ex)
            {
                Write($"Ошибка при запросе \n{ex.Message.ToString()}");
            }

            Write("СПИСОК ГРУПП: ");
            Write("# | НОМЕР ГРУППЫ ");

            foreach (var item in groups)
            {
                Write($"{item.id} | {item.name}");
            }


        }
        static void ShowTeacher()
        {
            Write("Выполняется запрос ожидайте..");

            List<Teachers> teachers = new List<Teachers>();
            try
            {
                var json = Response("https://asu.samgk.ru/api/teachers");
                teachers = JsonSerializer.Deserialize<List<Teachers>>(json);

            }
            catch (Exception ex)
            {
                Write($"Ошибка при запросе \n{ex.Message.ToString()}");
            }

            Write("СПИСОК ПРЕПОДАВАТЕЛЕЙ: ");
            Write("# | ФИО ");

            foreach (var item in teachers)
            {
                Write($"{item.id} | {item.name}");
            }


        }
        static void ChangeTimer()
        {
            Write("Укажите новый таймер");
            try
            {
                prop.Timer = int.Parse(Console.ReadLine());
                Write("Новая задержка задана");
            }
            catch (Exception ex)
            {
                Write("Ошибка при изменении");
            }

            SaveSettings();
        }
        static void ChangeToken()
        {
            Write("Укажите новый токен");
            prop.Token = Console.ReadLine();
            Write("Новый токен задан");

            SaveSettings(); 
        }
        static void AddNewValue()
        {
            Write("РЕЖИМ ДОБАВЛЕНИЯ НОВЫХ БЕСЕД - СООБЩЕНИЙ В БОТА");
            Write("ВВЕДИТЕ # БЕСЕДЫ ВК");
            string value_vk = Console.ReadLine();

            Write("ВВЕДИТЕ РАСПИСАНИЕ ПО ГРУППЕ (БУКВА - S) ИЛИ ПРЕПОДАВАТЕЛЯ (ГРУППА T)");
            string value_type = Console.ReadLine();

            Write("ВВЕДИТЕ # ГРУППЫ/ПРЕПОДАВАТЕЛЯ ИЗ АСУ СГК");
            string value_id = Console.ReadLine();

            try
            {
                prop.NewGroup(value_type, value_id, value_vk);
                Write("Добавлено!");
            }
            catch (Exception ex)
            {
                Write($"Ошибка при добавлении \n {ex.Message}");
            }

            SaveSettings();
        }
        static void DelValueTask()
        {
            Write("РЕЖИМ УДАЛЕНИЕ ЗАДАЧ");
            Write("# ЗАДАЧИ | ТИП | VALUE | VALUE PEER ID VK ");

            foreach (var item in prop.groups)
            {
                Write($"{item.Id} | {item.Type} | {item.Value} | {item.ValueIdGroupVk}");
            }

            Write("Какой # задачи удалить?");

            int id;
            try
            {
                id = int.Parse(Console.ReadLine());
            }
            catch (Exception ex)
            {
                Write($"Ошибка ввода ID\n {ex.Message.ToString()}");
                return;
            }

            try
            {
                prop.DelTask(id);
                Write($"Задача {id} удалена!");
                SaveSettings();

            }
            catch (Exception ex)
            {
                Write($"Ошибка при удалении ID\n {ex.Message.ToString()}");
            }

        }
        static void NewSettings()
        {
            Write("Введите пожалуйста токен от группы с правами - отправлять сообщения!");
            prop.Token = Console.ReadLine();
            Write("Введите пожалуйста количество милисекунд для проверки API расписания (В цифрах)");
            prop.Timer = int.Parse(Console.ReadLine());
            prop.isFirstStart = false;
        }

    }
}
