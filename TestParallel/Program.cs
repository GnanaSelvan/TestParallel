using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TestParallel
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Hello World!");

            //PLINQ Operations

            //await AsOrdered();
            //await WithMergeOptions();
            //await WithDegreeOfParallelism();
            //await ParallelForMethod();
            //await ParallelForEachMethod();
            //await ForceParallelWithExecutionMode();
            //await WithCancellation();

            //await TryTask();

            #region Cancellation Token

            //var cts = new CancellationTokenSource();
            //cts.CancelAfter(3000); //Cancel it after 3 seconds
            //await WithCancellationThrowEx(cts.Token);
            //await WithCancellationReturnMsg(cts.Token);

            #endregion

            #region Cancel from Another Thread

             CancelfromAnotherThread();
            #endregion

            await Task.CompletedTask;

          

            Console.ReadLine();
        }

        static async Task AsOrdered()
        {

            #region Sequential

            var items = Enumerable.Range(1, 10);

            var result = from x in items
                         where x % 2 == 0 //check for even
                         select DoWork(x);

            foreach (var item in result)
            {
                Console.WriteLine(await item);
            }

            Console.WriteLine("Complete Sequential");
            Console.ReadLine();


            #endregion

            #region Parallel 

            var items1 = ParallelEnumerable.Range(1, 10);

            var result1 = from x1 in items1.AsParallel()
                          where x1 % 2 == 0 // check for even
                          select DoWork(x1);

            foreach (var item1 in result1)
            {
                Console.WriteLine(await item1);                
            }

            Console.WriteLine("Complete Parallel");
            Console.ReadLine();

            #endregion

            #region Parallel with Ordering

            var items2 = ParallelEnumerable.Range(1, 10);
            var result2 = from x2 in items2.AsOrdered()
                          where x2 % 2 == 0 // check for even
                          select DoWork(x2);

            foreach (var item2 in result2)
            {
                Console.WriteLine(await item2);

            }

            Console.WriteLine("Complete Parallel with Ordering");
            Console.ReadLine();
            #endregion 

        }


        static async Task WithMergeOptions()
        {

            //Defining the Query
            var items = ParallelEnumerable.Range(1, 10);

            var result = from x in items
                         select DoWork(x);




            #region AutoBuffered
            //Use a merge with output buffers of a size chosen by the system.
            //Results will accumulate into an output buffer before they are available to the consumer of the query.

            foreach (var item in result) // Default ParallelMergeOptions is AutoBuffered
            {
                Console.WriteLine(await item);
            }

            Console.WriteLine("AutoBuffered");
            Console.ReadLine();
            #endregion

            #region FullyBuffered
            //Use a merge with full output buffers.
            //The system will accumulate all of the results before making any of them available to the consumer of the query.

            foreach (var item in result.WithMergeOptions(ParallelMergeOptions.FullyBuffered))
            {
                Console.WriteLine(await item);
            }

            Console.WriteLine("FullyBuffered");
            Console.ReadLine();
            #endregion

            #region NotBuffered
            //Use a merge without output buffers.
            //As soon as result elements have been computed, make that element available to the consumer of the query.

            foreach (var item in result.WithMergeOptions(ParallelMergeOptions.NotBuffered))
            {
                Console.WriteLine(await item);
            }

            Console.WriteLine("NotBuffered");
            Console.ReadLine();
            #endregion
        }


        static async Task WithDegreeOfParallelism()
        {

            var items = ParallelEnumerable.Range(1, 100);

            #region DOP 1
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("Default DOP- Started");

            items.Average(e => DoWork2(e));

            sw.Stop();

            Console.WriteLine("Elapsed Milliseconds: " + sw.ElapsedMilliseconds);
            Console.WriteLine("Default DOP- Completed");
            //Console.ReadLine();
            #endregion

            #region DOP2
            sw.Reset();
            sw.Start();
            Console.WriteLine("DOP=2- Started");

            items.WithDegreeOfParallelism(2)
            .Average(e => DoWork2(e));

            sw.Stop();

            Console.WriteLine("Elapsed Milliseconds: " + sw.ElapsedMilliseconds);
            Console.WriteLine("DOP= 2- Completed");
            #endregion

            #region DOP4
            sw.Reset();
            sw.Start();
            Console.WriteLine("DOP=4- Started");


            items.WithDegreeOfParallelism(4)
            .Average(e => DoWork2(e));

            sw.Stop();

            Console.WriteLine("Elapsed Milliseconds: " + sw.ElapsedMilliseconds);
            Console.WriteLine("DOP= 4- Completed");
            #endregion

           

            Console.ReadLine();
        }

        static async Task ForceParallelWithExecutionMode()
        {
            var items = ParallelEnumerable.Range(1, 10_000);

            var sw = new Stopwatch();
            sw.Start();
            #region Default Execution Mode
            var result = from e in items.AsParallel().WithExecutionMode(ParallelExecutionMode.Default)
                         where e % 2 == 0 //check for even
                         select DoWork(e);

            foreach (var item in result)
            {
                Console.WriteLine(await item);
            }

            sw.Stop();
            Console.WriteLine("Completed ParallelExecutionMode.Default in : " + sw.ElapsedMilliseconds + " Milliseconds");
           
            Console.ReadLine();
            #endregion

            #region ForceParallelism

            sw.Reset();
            sw.Start();
            var result1 = from e in items.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                         where e % 2 == 0 //check for even
                         select DoWork(e);

            foreach (var item in result1)
            {
                Console.WriteLine(await item);
            }

            sw.Stop();
            Console.WriteLine("Completed ParallelExecutionMode.ForceParallelism in : " + sw.ElapsedMilliseconds + " Milliseconds");

            Console.ReadLine();
            #endregion

            #region None 

            sw.Reset();
            sw.Start();
            var result2 = from e in items.AsParallel()
                          where e % 2 == 0 //check for even
                          select DoWork(e);

            foreach (var item in result2)
            {
                Console.WriteLine(await item);
            }

            sw.Stop();
            Console.WriteLine("Completed None in : " + sw.ElapsedMilliseconds + " Milliseconds");

            Console.ReadLine();
            #endregion

            #region Non- Parallel

            sw.Reset();
            sw.Start();
            var result3 = from e in items
                          where e % 2 == 0 //check for even
                          select DoWork(e);

            foreach (var item in result3)
            {
                Console.WriteLine(await item);
            }

            sw.Stop();
            Console.WriteLine("Completed Non-Parallel in : " + sw.ElapsedMilliseconds + " Milliseconds");

            Console.ReadLine();
            #endregion




        }

        static async Task WithCancellationThrowEx(CancellationToken token)
        {

            Console.WriteLine("Method started");

            token.ThrowIfCancellationRequested();

            for (int i = 0; i < 100000; i++)
            {

                Console.WriteLine(i);
                await Task.Delay(0, token);
                //return i; 

            }

            Console.WriteLine("Finished processing before cancellation token");
        
        }

        static async Task WithCancellationReturnMsg(CancellationToken token)
        {

            
            while (true)
            {
                Console.WriteLine("Working...");
                
                await Task.Run(() =>
                {                   
                    Thread.Sleep(1500);
                });

                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Work Cancelled!");

                    return;
                }
            }

        }

        static  void CancelfromAnotherThread()
        {

            int[] nums = Enumerable.Range(0, 1_000_000).ToArray();

            CancellationTokenSource cts = new CancellationTokenSource();

            //Use paralleloptions to store values
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            Console.WriteLine("Press any key to start; Press c to cancel : Thread {0} ; total processor {1} ",Thread.CurrentThread.ManagedThreadId, po.MaxDegreeOfParallelism);
            Console.ReadKey();

            //Run a task on another thread; that will be cancelled from the first thread

            Task.Factory.StartNew(
                () => {
                    if (Console.ReadKey().KeyChar == 'c')
                        cts.Cancel();
                    Console.WriteLine("Press any key to exit");
                });

            try
            {
                Parallel.ForEach(nums, po, (nums) =>
                {
                    double d = Math.Sqrt(nums);
                    Console.WriteLine("{0} on {1}", d, Thread.CurrentThread.ManagedThreadId);
                    if (po.CancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Task cancelled from another thread");
                    }
                    //po.CancellationToken.ThrowIfCancellationRequested();

                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception" + e.Message );
            }
            finally {
                cts.Dispose();
            }

            Console.ReadKey();



        }

        static async Task TryTask()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(1));
            Task<int> task = Task.Run(() => slowFunc(1, 2, source.Token), source.Token);
            Console.WriteLine("Task Completed");
            Console.ReadLine();
            // (A canceled task will raise an exception when awaited).
            await task;
        }

        static int slowFunc(int a, int b, CancellationToken cancellationToken)
        {
            string someString = string.Empty;
            for (int i = 0; i < 200000; i++)
            {
                someString += "a";
                if (i % 1000 == 0)
                    cancellationToken.ThrowIfCancellationRequested();
            }

            return a + b;
        }

        #region ParallelFor  and Forach

        static async Task ParallelForMethod()
        {

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("Sequential iteration on index {0} running on thread {1}",i, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(200);            
            }

            Console.ReadLine();

            Parallel.For(0, 10, i =>
            {
                Console.WriteLine("Sequential iteration on index {0} running on thread {1}",i, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(200);
            });

            Console.ReadLine();
        }

        static async Task ParallelForEachMethod()
        {

            var items1 = ParallelEnumerable.Range(1, 10);

            var result1 = from x1 in items1
                          where x1 % 2 == 0 // check for even
                          select DoWork(x1);

            foreach (var item1 in result1)
            {
                Console.WriteLine("Sequential iteration on item '{0}' running on thread {1}", item1, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(200);
            }

         

            Console.ReadLine();

            Parallel.ForEach(result1,item1 =>
            {
                Console.WriteLine("Sequential iteration on item '{0}' running on thread {1}", item1, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(200);
            });

            Console.ReadLine();
        }

        #endregion


        #region Helper Functions

        static async Task<int> DoWork(int input)
        {
            Console.WriteLine("Waiting for 1 sec to do the work");
            await Task.Delay(1000);

            Console.WriteLine("Output Returned");
            return input * 2;
        
        }

        static int DoWork2(int input)
        {
            Thread.SpinWait(5_000_000);
            return input * 2;
        }

        #region Encryption Algorithm
        public static string EncryptRijndael(string text, string salt)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            var aesAlg = NewRijndaelManaged(salt);

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(text);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public static bool IsBase64String(string base64String)
        {
            base64String = base64String.Trim();
            return (base64String.Length % 4 == 0) &&
                   Regex.IsMatch(base64String, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }

        public static string DecryptRijndael(string cipherText, string salt)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");

            if (!IsBase64String(cipherText))
                throw new Exception("The cipherText input parameter is not base64 encoded");

            string text;

            var aesAlg = NewRijndaelManaged(salt);
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            var cipher = Convert.FromBase64String(cipherText);

            using (var msDecrypt = new MemoryStream(cipher))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        text = srDecrypt.ReadToEnd();
                    }
                }
            }
            return text;
        }

        private static RijndaelManaged NewRijndaelManaged(string salt)
        {
            if (salt == null) throw new ArgumentNullException("salt");
            var saltBytes = Encoding.ASCII.GetBytes(salt);
            var key = new Rfc2898DeriveBytes("tets", saltBytes);

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

            return aesAlg;
        }

        #endregion

        #endregion

        static void PLINQExceptions_1()
        {
            // Using the raw string array here. See PLINQ Data Sample.
            string[] customers = GetCustomersAsStrings().ToArray();

            // First, we must simulate some currupt input.
            customers[54] = "###";

            var parallelQuery = from cust in customers.AsParallel()
                                let fields = cust.Split(',')
                                where fields[3].StartsWith("C") //throw indexoutofrange
                                select new { city = fields[3], thread = Thread.CurrentThread.ManagedThreadId };
            try
            {
                // We use ForAll although it doesn't really improve performance
                // since all output is serialized through the Console.
                parallelQuery.ForAll(e => Console.WriteLine("City: {0}, Thread:{1}", e.city, e.thread));
            }

            // In this design, we stop query processing when the exception occurs.
            catch (AggregateException e)
            {
                foreach (var ex in e.InnerExceptions)
                {
                    Console.WriteLine(ex.Message);
                    if (ex is IndexOutOfRangeException)
                        Console.WriteLine("The data source is corrupt. Query stopped.");
                }
            }
        }



    }
}
