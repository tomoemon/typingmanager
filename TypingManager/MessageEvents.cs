// Stephen Toub

using System;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace TypingManager
{
	public class MessageReceivedEventArgs : EventArgs
	{
		private readonly Message _message;
		public MessageReceivedEventArgs(Message message) { _message = message; }
		public Message Message { get { return _message; } }
	}

	public static class MessageEvents
	{
		private static object _lock = new object();
		private static MessageWindow _window;
		private static IntPtr _windowHandle;
        private static string _windowTitle = "MessageEventsWindow";
		private static SynchronizationContext _context;

        public static string WindowTitle
        {
            get { return _windowTitle; }
            set { _windowTitle = value; }
        }

		public static void WatchMessage(int message, Action<MessageReceivedEventArgs> action)
		{
			EnsureInitialized();
			_window.RegisterEventForMessage(message, action);
		}

		public static IntPtr WindowHandle
		{
			get
			{
				EnsureInitialized();
				return _windowHandle;
			}
		}

		private static void EnsureInitialized()
		{
			lock (_lock)
			{
				if (_window == null)
				{
					_context = AsyncOperationManager.SynchronizationContext;
					using (ManualResetEvent mre = new ManualResetEvent(false))
					{
						Thread t = new Thread((ThreadStart)delegate
						{
							_window = new MessageWindow();
                            _window.Text = WindowTitle;
							_windowHandle = _window.Handle;
							mre.Set();
							Application.Run();
						});
						t.Name = "MessageEvents message loop";
						t.IsBackground = true;
						t.Start();

						mre.WaitOne();
					}
				}
			}
		}

		private class MessageWindow : Form
		{
			private ReaderWriterLock _lock = new ReaderWriterLock();
			private Dictionary<int, Action<MessageReceivedEventArgs>> _messageSet = new Dictionary<int, Action<MessageReceivedEventArgs>>();

			public void RegisterEventForMessage(int messageID, Action<MessageReceivedEventArgs> action)
			{
				_lock.AcquireWriterLock(Timeout.Infinite);
				_messageSet[messageID] = action;
				_lock.ReleaseWriterLock();
			}

			protected override void WndProc(ref Message m)
			{
				_lock.AcquireReaderLock(Timeout.Infinite);
                if (_messageSet.ContainsKey(m.Msg))
                {
                    var handler = _messageSet[m.Msg];
					MessageEvents._context.Post(state =>
					{
                        handler(new MessageReceivedEventArgs((Message)state));
					}, m);
				}
                _lock.ReleaseReaderLock();
                base.WndProc(ref m);
			}
		}
	}
}
