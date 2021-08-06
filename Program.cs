using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace VkOnlineStatusChecker
{
    class Program
    {
        public static bool tgUser = false;
        public static string botToken= File.ReadAllText(Directory.GetCurrentDirectory() + "//tgbottoken.txt");
        public static long chatID;
        public static TelegramBotClient tgBot = new TelegramBotClient(botToken);
        static void Main(string[] args)
        {
            string currentDirrectory = Directory.GetCurrentDirectory();
            botToken = File.ReadAllText(currentDirrectory + "//tgbottoken.txt");

            bool useTgBot = true;
            if (botToken == "0")
            {
                useTgBot = false;
            }
            if (useTgBot == true)
            {
                tgBot.StartReceiving();
                tgBot.OnMessage += OnMessangeHandler;
                Console.WriteLine("send any message to the bot and press any key");
                Console.ReadKey();
            }
            string [] urls = File.ReadAllLines(currentDirrectory+"//users.txt");
            foreach (string url in urls)
            {
                File.WriteAllText(currentDirrectory + "//Logs//" + url.Replace("https://vk.com/", "")+".txt", url.Replace("https://vk.com/", "")+" activity log");
            }
            while (true)
            {
                Checker(urls);
            }
        }

        private static void OnMessangeHandler(object sender, MessageEventArgs e)
        {
            var msg = e.Message;
            chatID = msg.Chat.Id;
            tgUser = true;
        }

        private static void Checker(string [] urls)
        {
            foreach(string url in urls)
            {
                string result = Parsing(url);
                Console.WriteLine(url + " status " + result);
                Logger(url, result);
            }
        }
        private static async void Logger(string url, string result)
        {
            string currentDirrectory = Directory.GetCurrentDirectory();
            string logsfile = currentDirrectory + "//Logs//" + url.Replace("https://vk.com/", "") + ".txt";
            string[] logs = File.ReadAllLines(logsfile);
            if (logs[logs.Length-1]!=result)
            {
                if (tgUser == true)
                {
                    await tgBot.SendTextMessageAsync(chatID, url + " status " + result);
                }
                File.AppendAllText(logsfile, Environment.NewLine+Convert.ToString(DateTime.Now)+ Environment.NewLine);
                File.AppendAllText(logsfile, result);
            }
        }
        private static string Parsing(string url)
        {
            string answer;
            var request = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                answer = reader.ReadToEnd();
            }
            if(answer.Contains("Online"))
            {
                answer = "online";
            }
            else if (answer.Contains("Страница доступна только авторизованным пользователям."))
            {
                answer = "hidden";
            }
            else
            {
                answer = "offline";
            }
            return answer;
        }
    }
}
