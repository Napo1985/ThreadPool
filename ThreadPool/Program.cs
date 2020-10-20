using System;
using System.Threading;

namespace ThreadPool
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var pool = new MyThreadPool(5))
			{
				var random = new Random();
				Action<int> randomizer = (index =>
				{
					int time = random.Next(20, 400);
					Console.WriteLine("{0}: Working on index {1} for {2}", Thread.CurrentThread.Name, index, time);
					Thread.Sleep(time);
					Console.WriteLine("{0}: Ending {1}", Thread.CurrentThread.Name, index);
				});

				for (var i = 0; i < 50; ++i)
				{
					var i1 = i;
					pool.QueueTask(() => randomizer(i1));
				}
			}

		}
	}
}
