using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace advanced_parallel_asyn_await
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"Creator: Felipe Bossolani - fbossolani[at]gmail.com");
            Console.WriteLine(@"Examples based on: http://returnsmart.blogspot.com/2015/07/mcsd-programming-in-c-part-2-70-483.html");
            Console.WriteLine("Choose a Method: ");
            Console.WriteLine("01- Regular For versus Parallel.For/ForEach");
            Console.WriteLine("02- Async/Await 01");
            Console.WriteLine("03- Async/Await 02");
            Console.WriteLine("04- PLINQ AsParallel");
            Console.WriteLine("05- PLINQ AsParallel / AsSequential");
            Console.WriteLine("06- PLINQ ForAll");
            Console.WriteLine("07- BlockingCollection");
            
            int option = 0;
            int.TryParse(Console.ReadLine(), out option);

            switch (option)
            {
                case 1:
                    {
                        RegularForVsParallelFor();
                        break;
                    }
                case 2:
                    {
                        AsyncAwait01();
                        break;
                    }
                case 3:
                    {
                        AsyncAwait02();
                        break;
                    }
                case 4:
                    {
                        PLinqAsParallel();
                        break;
                    }
                case 5:
                    {
                        PLinqAsParallelAsSequential();
                        break;
                    }
                case 6:
                    {
                        PLinqForAll();
                        break;
                    }
                case 7:
                    {
                        BlockingCollectionEx();
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Invalid option...");
                        break;
                    }
            }
        }

        private static void BlockingCollectionEx()
        {
            Console.WriteLine("Press any key (n times) to test and press <enter> to exit...");
            BlockingCollection<string> col = new BlockingCollection<string>();
            Task read = Task.Run(() =>
            {
                while (true)
                {
                    Console.WriteLine(col.Take());
                }
            });

            Task write = Task.Run(() =>
            {
                while (true)
                {
                    string s = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(s)) break;
                    col.Add(s);
                }
            });

            write.Wait();
        }

        private static async void AsyncAwait02()
        {
            HttpClient httpClient = new HttpClient();
            string content = await httpClient
                .GetStringAsync("http://www.microsoft.com")
                .ConfigureAwait(false);

            using (FileStream sourceStream = new FileStream("temp.html", FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                byte[] encodedText = Encoding.Unicode.GetBytes(content);
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length).ConfigureAwait(false);
            }
        }

        private static void AsyncAwait01()
        {
            string result = DownloadContent().Result;
            Console.WriteLine(result);
        }

        private static async Task<String> DownloadContent()
        {
            using(HttpClient client = new HttpClient())
            {
                string result = await client.GetStringAsync("http://www.microsoft.com");
                return result;
            }
        }

        private static void PLinqAsParallelAsSequential()
        {
            var numbers = Enumerable.Range(0, 100);

            var parallelResult = numbers.AsParallel().AsOrdered()
                    .Where(i => i % 2 == 0) //pairs numbers
                    .AsSequential();

            foreach(int i in parallelResult.Take(5))
            {
                Console.WriteLine(i);
            }
        }

        private static void PLinqForAll()
        {
            Console.WriteLine("If you don'y mind get not ordered you can use ForAll()");

            var numbers = Enumerable.Range(0, 100);
            var parallelResult = numbers.AsParallel()
                .Where(i => i % 2 == 0); //pairs numbers

            parallelResult.ForAll(i => Console.WriteLine(i));
        }


        private static void PLinqAsParallel()
        {
            var numbers = Enumerable.Range(0, 100);
            var parallelResult = numbers.AsParallel()
                .Where(i => i % 2 == 0) //pairs numbers
                .ToArray();

            Console.WriteLine("Numbers from 0 to 100 will not appear ordered");

            foreach(int i in parallelResult)
            {
                Console.WriteLine(i);
            }
        }

        private static void RegularForVsParallelFor()
        {
            Console.WriteLine("starting: {0}", DateTime.Now.TimeOfDay);
            for(int i = 0; i < 10; i ++)
            {
                Thread.Sleep(1000);
            };
            Console.WriteLine("After regular loop: {0}", DateTime.Now.TimeOfDay);
            Parallel.For(0, 10, i =>
            {
                 Thread.Sleep(1000);
            });
            Console.WriteLine("After Parallel.For: {0}", DateTime.Now.TimeOfDay);

            var numbers = Enumerable.Range(0, 10);
            Parallel.ForEach(numbers, i =>
            {
                Thread.Sleep(1000);
            });
            Console.WriteLine("After Parallel.ForEach: {0}", DateTime.Now.TimeOfDay);

            ParallelLoopResult result = Parallel.
                For(0, 1000, (int i, ParallelLoopState loopState) =>
                {
                    if (i == 500)
                    {
                        Console.WriteLine(" Breaking loop");
                        loopState.Break();
                    }
                    return;
                });
            Console.WriteLine("After LoopState.Break: {0}", DateTime.Now.TimeOfDay);
        }

    }
}
