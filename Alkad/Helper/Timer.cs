using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GameWer.Helper
{
	public class Timer
	{
		private Action CurrentAction;

		private Action<Exception> OnException;

		private TimeSpan Time;

		private bool HasInterval = false;

		private bool HasStop = false;

		private Timer()
		{
		}

		private void Start()
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.WorkerThread));
		}

		internal void Stop()
		{
			this.HasStop = true;
		}

		internal static Timer Timeout(Action action, Action<Exception> exception, float timeout)
		{
			Timer timer = new Timer();
			timer.CurrentAction = action;
			timer.OnException = exception;
			timer.Time = TimeSpan.FromMilliseconds((double)(timeout * (float)int.Parse("1000")));
			timer.Start();
			return timer;
		}

		internal static Timer Interval(Action action, Action<Exception> exception, float timeout)
		{
			Timer timer = new Timer();
			timer.CurrentAction = action;
			timer.Time = TimeSpan.FromMilliseconds((double)(timeout * (float)int.Parse("1000")));
			timer.OnException = exception;
			timer.HasInterval = true;
			timer.Start();
			return timer;
		}

		[CompilerGenerated]
		private void WorkerThread(object obj)
		{
			while (!this.HasStop)
			{
				Thread.Sleep(this.Time);
				ApplicationManager.SetTaskInMainThread(new Action(this.DoWork));
				bool flag = !this.HasInterval;
				if (flag)
				{
					break;
				}
			}
		}

		[CompilerGenerated]
		private void DoWork()
		{
			try
			{
				Action currentAction = this.CurrentAction;
				if (currentAction != null)
				{
					currentAction();
				}
			}
			catch (Exception obj)
			{
				Action<Exception> onException = this.OnException;
				if (onException != null)
				{
					onException(obj);
				}
			}
		}
	}
}
