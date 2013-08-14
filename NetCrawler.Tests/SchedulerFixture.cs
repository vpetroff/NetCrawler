using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NetCrawler.Tests
{
	[TestFixture]
	public class SchedulerFixture
	{
		[Test]
		public void Should_get_work_from_queue()
		{
			var startingUrls = new[] {"a", "b", "c"};
			var loaded = 0;

			var jobQueue = new JobQueue(1)
				{
					QueueLoaderAction = queue =>
						{
//							if (loaded > 10)
//								return;

							foreach (var url in startingUrls)
							{
								queue.Enqueue(url);
							}

							loaded++;
						}, 
					WorkerAction = s =>
						{
							Thread.Sleep(1000);
							Console.WriteLine(s);
						}
				};

			var cancellataionSource = new CancellationTokenSource();
			Task.Factory.StartNew(() =>
			{
				Console.ReadKey();
				cancellataionSource.Cancel();
			});

			var t = jobQueue.Start(cancellataionSource.Token);
			t.Wait();
		}
	}

	public class Scheduler
	{
		private readonly IJobQueue jobQueue;

		public Scheduler(IJobQueue jobQueue)
		{
			this.jobQueue = jobQueue;

		}

		public Task<bool> Schedule(IEnumerable<string> urls)
		{
			return jobQueue.Start();
		}
	}

	public interface IJobQueue
	{
		Task<bool> Start();
		Task<bool> Start(CancellationToken token);

		Action<ConcurrentQueue<string>> QueueLoaderAction { get; set; }
		Action<string> WorkerAction { get; set; }
	}

	public class JobQueue : IJobQueue
	{
		private CancellationTokenSource cancellationSource;

		private readonly ConcurrentQueue<string> persistentQueue = new ConcurrentQueue<string>();

		private readonly Worker[] workers;

		private readonly ManualResetEventSlim waitingForWork = new ManualResetEventSlim(false);
		private readonly Thread queueLoaderThread;
		private readonly TaskCompletionSource<bool> taskCompletionSource;

		public JobQueue(int workersCount)
		{
			taskCompletionSource = new TaskCompletionSource<bool>();
			taskCompletionSource.Task.ContinueWith(t =>
			{
				if (cancellationSource != null)
					cancellationSource.Cancel();
				return false;
			}, TaskContinuationOptions.OnlyOnCanceled);

			queueLoaderThread = new Thread(QueueLoader) { IsBackground = true };
			queueLoaderThread.Start();

			workers = Enumerable.Range(0, workersCount).Select(x =>
				{
					var thread = new Thread(Worker) {IsBackground = true};
					thread.Start();

					return new Worker(thread);
				}).ToArray();
		}

		private void QueueLoader()
		{
			while (true)
			{
				if (cancellationSource != null && cancellationSource.IsCancellationRequested)
					return;

				if (waitingForWork.Wait(10))
				{
					var handler = QueueLoaderAction;
					if (handler != null)
						handler.Invoke(persistentQueue);

					if (persistentQueue.IsEmpty)
						taskCompletionSource.TrySetResult(true);

					waitingForWork.Reset();
				}
			}
		}

		public Action<ConcurrentQueue<string>>  QueueLoaderAction { get; set; }
		public Action<string>  WorkerAction { get; set; }

		private void Worker()
		{
			while (true)
			{
				if (cancellationSource != null && cancellationSource.IsCancellationRequested)
					return;

				string url;

				if (persistentQueue.TryDequeue(out url))
				{
					var handler = WorkerAction;
					if (handler != null)
						handler.Invoke(url);
				}
				else
				{
					waitingForWork.Set();
				}
			}
		}

		public Task<bool> Start()
		{
			cancellationSource = new CancellationTokenSource();
			return Start(cancellationSource.Token);
		}

		public Task<bool> Start(CancellationToken token)
		{
			Thread.Sleep(100);
			return taskCompletionSource.Task;
		}
	}

	internal class Worker
	{
		public Worker(Thread thread)
		{
			Thread = thread;
		}

		public Thread Thread { get; private set; }
	}
}