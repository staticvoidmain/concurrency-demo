using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace demo.part_3
{
	public class Engine
	{
		private static readonly HashSet<int> visited = new HashSet<int>();
		private static readonly ConcurrentQueue<string> urls = new ConcurrentQueue<string>();
		private static readonly ConcurrentQueue<WebResponse> responses = new ConcurrentQueue<WebResponse>();
		private static readonly ConcurrentQueue<CancellationTokenSource> tokenSources = new ConcurrentQueue<CancellationTokenSource>();

		private static readonly int maxDegreeOfParallelism = Environment.ProcessorCount;

		// singleton, there can be only one!
		private static readonly Engine engine = new Engine();

		public static Engine Instance
		{
			get
			{
				return engine;
			}
		}

		private int _threads = 1;
		public int Threads
		{
			get { return _threads; }
			set
			{
				if (_threads != value)
				{
					UpdateThreads(_threads, value);
					_threads = value;
				}
			}
		}

		private void UpdateThreads(int previous, int value)
		{
			int diff = previous - value;

			if (diff < 0) { AddThreads(-diff); }
			else { RemoveThreads(diff); }
		}

		private void RemoveThreads(int diff)
		{
			while (diff > 0) 
			{
				CancellationTokenSource token;

				if (tokenSources.TryDequeue(out token))
				{
					token.Cancel();
					diff--;
				}
			}
		}

		private void AddThreads(int p)
		{
			for (int i = 0; i < p; i++)
			{
				var source = new CancellationTokenSource();
				var token = source.Token;

				tokenSources.Enqueue(source);

				Task.Factory.StartNew(() => 
				{
					while (true)
					{
						if (token.IsCancellationRequested)
						{
							token.ThrowIfCancellationRequested();
						}

						Process();
					}
				}, token);
			}
		}


		public bool UseAsync { get; set; }

		public void Start()
		{
			AddThreads(this.Threads);
		}

		private void Process()
		{
			string uri;

			if (urls.TryDequeue(out uri))
			{
				WebRequest request = WebRequest.Create(uri);

				if (UseAsync)
				{
					request
						.GetResponseAsync()
						.ContinueWith(r =>
						{
							ProcessResponse(r.Result);
						});
				}
				else
				{
					ProcessResponse(request.GetResponse());
				}
			}
			else
			{
				// yield
				Thread.Yield();
			}
		}

		private void ProcessResponse(WebResponse webResponse)
		{
			const int blockSize = 512;
			var stream = webResponse.GetResponseStream();

			using (StreamReader reader = new StreamReader(stream))
			{
				int index = 0;
				int bytesRead = 0;
				char[] buffer = new char[blockSize];
				ICollection<char> partialLink = new Collection<char>();

				do
				{
					bytesRead = reader.ReadBlock(buffer, index, blockSize);

					ICollection<string> links = ProcessBuffer(buffer, ref partialLink);

					if (links.Count > 0)
					{
						foreach (string link in links)
						{
							if (visited.Add(link.GetHashCode()))
							{
								urls.Enqueue(link);
							}
						}
					}

					index += bytesRead;
				} while (bytesRead > 0);
			}
		}

		private static readonly char[] token = "<a href=\"".ToCharArray();

		private ICollection<string> ProcessBuffer(char[] buffer, ref ICollection<char> partialLink)
		{
			const char closingToken = '"';
			int tokenPosition = 0;

			ICollection<string> links = new Collection<string>();

			for (int i = 0; i < buffer.Length; i++)
			{
				Char current = buffer[i];

				if (partialLink.Count > 0)
				{
					if (current == closingToken)
					{
						char[] temp = new char[partialLink.Count];
						partialLink.CopyTo(temp, 0);

						links.Add(new string(temp));
						partialLink.Clear();
					}
					else
					{
						partialLink.Add(current);
						tokenPosition = 0;
					}
				}
				else
				{
					if (current != token[tokenPosition])
					{
						tokenPosition = 0;
					} 
					else if (tokenPosition == token.Length)
					{
						partialLink.Add(current);
						tokenPosition = 0;
					}
				}
			}

			return links;
		}

		public void Stop() 
		{
			RemoveThreads(this.Threads);
		}
	}
}
