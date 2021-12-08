using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestAsyncAwait
{
    class Program
    {

        private const string URL = "https://docs.microsoft.com/en-us/dotnet/csharp/csharp";
       

        static void Main(string[] args)
        {
            CallSynchronousAlarm1();
            var someTask = CallAlarm2Async();
            CallAlarm3SynchronouslyAfterAwait();
            var someTask4 = CallAlarm4Async();
            //someTask.Wait(); //this is a blocking call, use it only on Main method
            //Console.WriteLine("All Task completed");
            Console.ReadLine();
        }
        public static void CallSynchronousAlarm1()
        {
            // You can do whatever work is needed here
            Console.WriteLine("1. Calling Alarm1 Synchronously");
        }

        static async Task CallAlarm2Async() //A Task return type will eventually yield a void
        {
            Console.WriteLine("2. AsyncAlarm2 task has started...");
            await GetStringAsync(); // we are awaiting the Async Method GetStringAsync
        }

        static async Task GetStringAsync()
        {
            using (var httpClient = new HttpClient())
            {
                Console.WriteLine("3. Awaiting the result of GetAlarm2Async ...");
                string result = await httpClient.GetStringAsync(URL); //execution pauses here while awaiting GetStringAsync to complete

                Task.Delay(3000).Wait();
                //From this line and below, the execution will resume once the above awaitable is done
               
                Console.WriteLine("3a. The awaited task has completed. Let's get the content length...");
                Console.WriteLine($"3b. The length of http Get for {URL}");
                Console.WriteLine($"3c. {result.Length} character");
            }
        }

        static void CallAlarm3SynchronouslyAfterAwait()
        {
            //This is the work we can do while waiting for the awaited Async Task to complete
            Console.WriteLine("4. Alarm3Called: While waiting for the GetAlarm2Async task to finish, we can call another alarm work like this..");
            for (var i = 0; i <= 5; i++)
            {
                for (var j = i; j <= 5; j++)
                {
                    Console.Write("*");
                }
                Console.WriteLine();
            }

        }

        static async Task CallAlarm4Async() //A Task return type will eventually yield a void
        {
            Console.WriteLine("5. AsyncAlarm4 task has started...");
            using (var httpClient = new HttpClient())
            {
                string result = await httpClient.GetStringAsync(URL); //execution pauses here while awaiting GetStringAsync to complete
            }
            Task.Delay(4000).Wait();
            Console.WriteLine("5a. AsyncAlarm4 completed");
            //await GetStringAsync(); // we are awaiting the Async Method GetStringAsync
        }
    }
}
