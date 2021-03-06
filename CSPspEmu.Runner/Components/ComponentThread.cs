﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CSharpUtils.Threading;
using CSPspEmu.Core;
using CSharpUtils;

namespace CSPspEmu.Runner.Components
{
	abstract public class ComponentThread : PspEmulatorComponent, IRunnableComponent
	{
		protected AutoResetEvent RunningUpdatedEvent = new AutoResetEvent(false);
		public bool Running = true;

		protected Thread ComponentThreadThread;
		protected AutoResetEvent StopCompleteEvent = new AutoResetEvent(false);
		protected AutoResetEvent PauseEvent = new AutoResetEvent(false);
		protected AutoResetEvent ResumeEvent = new AutoResetEvent(false);

		readonly public TaskQueue ThreadTaskQueue = new TaskQueue();
		abstract protected String ThreadName { get; }

		public void StartSynchronized()
		{
			Console.WriteLine("Component {0} StartSynchronized!", this);
			var StartStartTime = DateTime.UtcNow;
			{
				ComponentThreadThread = new Thread(() =>
				{
					ComponentThreadThread.Name = this.ThreadName;
					Thread.CurrentThread.CurrentCulture = new CultureInfo(PspConfig.CultureName);
					try
					{
						Main();
					}
					finally
					{
						Running = false;
						RunningUpdatedEvent.Set();
						StopCompleteEvent.Set();
						Console.WriteLine("Component {0} Stopped!", this);
					}
				});
				ComponentThreadThread.Start();
				ThreadTaskQueue.EnqueueAndWaitCompleted(() =>
				{
				});
			}
			var StartEndTime = DateTime.UtcNow;
			Console.Write("Component {0} Started! StartedTime(", this);
			var ElapsedTime = StartEndTime - StartStartTime;
			ConsoleUtils.SaveRestoreConsoleState(() =>
			{
				Console.ForegroundColor = (ElapsedTime.TotalSeconds > 0.1) ? ConsoleColor.Yellow : ConsoleColor.Green;
				Console.Write("{0}", ElapsedTime.TotalSeconds);
			});
			Console.WriteLine(")");
		}

		public void StopSynchronized()
		{
			Console.Write("Component {0} StopSynchronized...", this);
			var StopStartTime = DateTime.UtcNow;
			{
				if (Running)
				{
					StopCompleteEvent.Reset();
					{
						Running = false;
						RunningUpdatedEvent.Set();
					}
					if (!StopCompleteEvent.WaitOne(1000))
					{
						Console.Error.WriteLine("Error stopping {0}", this);
						ComponentThreadThread.Abort();
					}
				}
			}
			var StopEndTime = DateTime.UtcNow;
			Console.WriteLine("Stopped! {0}", StopEndTime - StopStartTime);
		}

		public void PauseSynchronized()
		{
			Console.WriteLine("Component {0} PauseSynchronized!", this);

			ThreadTaskQueue.EnqueueAndWaitStarted(() =>
			{
				while (!PauseEvent.WaitOne(TimeSpan.FromMilliseconds(10)))
				{
					if (!Running) break;
				}
			});
		}

		public void ResumeSynchronized()
		{
			Console.WriteLine("Component {0} ResumeSynchronized!", this);

			PauseEvent.Set();
		}

		abstract protected void Main();
	}
}
