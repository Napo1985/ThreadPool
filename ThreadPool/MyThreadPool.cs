using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ThreadPool
{
	class MyThreadPool : IDisposable
	{

		#region ctor
		public MyThreadPool(int numOfThreads)
		{
		
			for (var i = 0; i < numOfThreads; i++)
			{
				var worker = new Thread(this.WaitForJob) { Name = string.Concat("Worker ", i) };
				worker.Start();
				m_workers.AddLast(worker);
			}
		}
		#endregion

		#region members
		private LinkedList<Thread> m_workers = new LinkedList<Thread>(); // queue of worker threads ready to process actions
		private bool m_blockAdd; // set to true when disposing queue but there are still tasks pending
		private bool m_disposed; // set to true when disposing queue and no more tasks are pending
		BlockingCollection<Action> m_tasks = new BlockingCollection<Action>(); // actions to be processed by worker threads

		#endregion

		#region functions
		private void WaitForJob() 
		{
			Action task = null;
			while (true) // loop until threadpool is disposed
			{
				if (m_disposed)
				{
					return;
				}

				task = m_tasks.Take();
				task?.Invoke();
				task = null;			
			}		
		}

		public void Dispose()
		{			
			var waitForThreads = false;
			lock (m_tasks)
			{
				if (!m_disposed)
				{
					GC.SuppressFinalize(this);

					m_blockAdd = true; // wait for all tasks to finish processing while not allowing any more new tasks
					while (m_tasks.Count > 0)
					{
						//Monitor.Wait(m_tasks);
					}

					m_disposed = true; 
					Monitor.PulseAll(m_tasks); // wake all workers (none of them will be active at this point; disposed flag will cause then to finish so that we can join them)
					waitForThreads = true;
				}
			}
			if (waitForThreads)
			{
				foreach (var worker in m_workers)
				{
					Console.WriteLine($"{worker.Name} wait to join ");
					worker.Join();
				}
			}
			m_tasks.Dispose();
		}

		public void QueueTask(Action task)
		{
			lock (m_tasks)
			{
				if (m_blockAdd)
				{
					throw new InvalidOperationException("This Pool instance is in the process of being disposed, can't add anymore");
				}
				if (m_disposed)
				{
					throw new ObjectDisposedException("This Pool instance has already been disposed");
				}
				//m_tasks.AddLast(task);
				m_tasks.Add(task);
				Monitor.PulseAll(m_tasks); // pulse because tasks count changed
			}
		}
		#endregion
	}
}