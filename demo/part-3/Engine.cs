using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace demo.part_3
{
	public class Engine
	{
		// singleton, there can be only one!
		private static readonly Engine engine = new Engine();

		private static readonly char[] tag = "<a ".ToCharArray();
		private static readonly char[] attr = "href=\"".ToCharArray();

		private static readonly int maxDegreeOfParallelism = Environment.ProcessorCount;
		private static readonly ConcurrentQueue<WebResponse> responses = new ConcurrentQueue<WebResponse>();
		private static readonly ConcurrentQueue<CancellationTokenSource> tokenSources = new ConcurrentQueue<CancellationTokenSource>();
		private static readonly ConcurrentQueue<string> urls = new ConcurrentQueue<string>();
		private static readonly ConcurrentDictionary<string, int> words = new ConcurrentDictionary<string, int>();
		private static readonly HashSet<int> visited = new HashSet<int>();
		private static readonly ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();

		public event EventHandler PageIndexed;

		private string _seed;
		private int _threads = 1;

		public static Engine Instance
		{
			get
			{
				return engine;
			}
		}

		public string Seed
		{
			get { return this._seed; }
			set
			{
				this._seed = value;
			}
		}

		public bool Started { get; set; }

		public int Threads
		{
			get { return _threads; }
			set
			{
				if (_threads != value)
				{
					if (Started)
					{
						UpdateThreads(this._threads, value);
					}

					this._threads = value;
				}
			}
		}

		public bool UseAsync { get; set; }

		public void Start()
		{
			urls.Enqueue(this.Seed);
			AddThreads(this.Threads);
			Started = true;
		}

		public void Stop()
		{
			RemoveThreads(this.Threads);		
		}

		private void AddThreads(int p)
		{
			for (int i = 0; i < p; i++)
			{
				var source = new CancellationTokenSource();
				var token = source.Token;

				tokenSources.Enqueue(source);

				tasks.Add(Task.Factory.StartNew(() =>
				{
					int stalls = 0;

					while (true)
					{
						token.ThrowIfCancellationRequested();

						if (!Process())
						{
							stalls++;
							Thread.Yield();
						}
					}
				}, TaskCreationOptions.LongRunning));
			}
		}

		private bool Process()
		{
			string uri;
			bool complete = false;

			if (urls.TryDequeue(out uri))
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
				request.MaximumAutomaticRedirections = 2;
				request.Pipelined = true;
				request.AutomaticDecompression = DecompressionMethods.GZip;
				request.Timeout = 1500;

				try
				{
					if (UseAsync)
					{
						request
							.GetResponseAsync()
							.ContinueWith(r =>
							{
								ProcessResponse((HttpWebResponse)r.Result);
							});
					}
					else
					{
						ProcessResponse((HttpWebResponse)request.GetResponse());
					}

					complete = true;
				}
				catch (WebException) { }
			}

			return complete;
		}

		#region Premature Optimizations

		//private enum ParserStates
		//{
		//	Seeking,
		//	InsideOpeningTag,
		//	InsideClosingTag,
		//	InsideAnchor,
		//	Href,
		//	Content
		//}

		//// forget save this for someday when I give a crap.
		//private ICollection<string> ProcessBuffer(char[] buffer, ref ParserStates state, ref ICollection<char> partialLink)
		//{
		//	ICollection<string> links = new Collection<string>();
		//	int len = buffer.Length;

		//	for (int i = 0; i < len; i++)
		//	{
		//		Char current = buffer[i];

		//		if (current == '>')
		//		{
		//			state = state == ParserStates.InsideOpeningTag
		//				? ParserStates.Content
		//				: ParserStates.Seeking;
		//		}
		//		else if (current == '<')
		//		{
		//			state = state == ParserStates.Content
		//				? ParserStates.InsideClosingTag
		//				: ParserStates.InsideOpeningTag;
		//		}
		//		else
		//		{
		//			if (state == ParserStates.InsideOpeningTag)
		//			{
		//				if (current == 'a')
		//				{
		//					state = ParserStates.InsideAnchor;
		//				}
		//				else
		//				{
		//					// move next until content
		//					for (; i < len; i++)
		//					{
		//						if (buffer[i] == '>')
		//							break;
		//					}

		//					state = ParserStates.Content;
		//				}
		//			}
		//			else if (state == ParserStates.InsideAnchor)
		//			{
		//				int tokenPosition = 0;

		//				for (; i < len; i++)
		//				{
		//					current = buffer[i];

		//					if (attr[tokenPosition] == current)
		//					{
		//						tokenPosition++;
		//						if (tokenPosition == attr.Length)
		//						{
		//							state = ParserStates.Href;
		//							break;
		//						}
		//					}
		//					else
		//					{
		//						// maybe we didn't find one? malformed, who knows.
		//						if (current == '>')
		//							state = ParserStates.Content;
		//					}
		//				}
		//			}
		//			else if (state == ParserStates.Href)
		//			{
		//				for (; i < len; i++)
		//				{
		//					current = buffer[i];

		//					if (current == '"')
		//					{
		//						char[] temp = new char[partialLink.Count];
		//						partialLink.CopyTo(temp, 0);

		//						links.Add(new string(temp));
		//						partialLink.Clear();

		//						for (; i < len; i++)
		//						{
		//							// move next until be a closing tag
		//							current = buffer[i];

		//							if (current == '>')
		//							{
		//								state = ParserStates.Content;
		//								break;
		//							}
		//						}
		//					}
		//					else
		//					{
		//						partialLink.Add(current);
		//					}
		//				}
		//			}
		//			else if (state == ParserStates.Content)
		//			{
		//				for (; i < len; i++)
		//				{
		//					// feck
		//				}
		//			}
		//		}
		//	}

		//	return links;
		//}

		//private void ProcessResponse(HttpWebResponse webResponse)
		//{
		//	const int blockSize = 1024;

		//	if (webResponse.StatusCode == HttpStatusCode.OK)
		//	{
		//		Stream stream = webResponse.GetResponseStream();

		//		using (StreamReader reader = new StreamReader(stream))
		//		{
		//			int index = 0;
		//			int bytesRead = 0;
		//			char[] buffer = new char[blockSize];
		//			ICollection<char> partialLink = new Collection<char>();
		//			ParserStates state = ParserStates.Seeking;

		//			try
		//			{
		//				do
		//				{
		//					bytesRead = reader.ReadBlock(buffer, 0, blockSize);

		//					ICollection<string> links = ProcessBuffer(buffer, ref state, ref partialLink);

		//					if (links.Count > 0)
		//					{
		//						foreach (string link in links)
		//						{
		//							if (visited.Add(link.GetHashCode()))
		//							{
		//								urls.Enqueue(link);
		//							}
		//						}
		//					}

		//					index += bytesRead;
		//				} while (bytesRead > 0);
		//			}
		//			catch (Exception ex)
		//			{
		//				Trace.Write(ex);
		//			}
		//		}
		//	}
		//}

		#endregion

		private void ProcessResponse(HttpWebResponse webResponse)
		{
			if (webResponse.StatusCode == HttpStatusCode.OK 
				&& webResponse.ContentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
			{
				Stream stream = webResponse.GetResponseStream();

				using (StreamReader reader = new StreamReader(stream))
				{
					string result = reader.ReadToEnd();

					foreach (string word in GetWords(result))
					{
						words.AddOrUpdate(word, 1, UpdateWordCount);
					}

					foreach (string link in GetLinks(result))
					{
						if (IsStaticContent(link))
							continue;

						Uri uri = new Uri(
							link.StartsWith("//") ? string.Concat(webResponse.ResponseUri.Scheme, ":", link) : link,
							UriKind.RelativeOrAbsolute);

						if (uri.IsAbsoluteUri == false)
						{
							Uri host = new Uri(
								string.Concat(webResponse.ResponseUri.Scheme, "://", webResponse.ResponseUri.Host, "/"));

							uri = new Uri(host, uri);
						}

						if (uri.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase))
						{
							if (visited.Add(uri.AbsoluteUri.GetHashCode()))
							{
								urls.Enqueue(uri.AbsoluteUri);
							}
						}
					}

					OnPageIndexed();
				}
			}
		}

		private bool IsStaticContent(string link)
		{
			return link.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
				|| link.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
				|| link.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
				|| link.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
				|| link.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase);
		}
		
		private void OnPageIndexed()
		{
			var local = PageIndexed;

			if (local != null) 
			{
				local(this, null);
			}
		}

		private int UpdateWordCount(string word, int count)
		{
			return count + 1;
		}

		private readonly Regex wordRegex = new Regex(@"\w\w+", RegexOptions.Compiled);

		private ICollection<string> GetWords(string result)
		{
			ICollection<string> words = new Collection<string>();

			foreach (Match m in wordRegex.Matches(result))
			{
				words.Add(m.Groups[0].Value);
			}

			return words;
		}

		private readonly Regex hrefRegex = new Regex("href=\"([^\"]*)\"", RegexOptions.Compiled);

		private ICollection<string> GetLinks(string result)
		{
			ICollection<string> links = new Collection<string>();

			foreach (Match m in hrefRegex.Matches(result))
			{
				links.Add(m.Groups[1].Value);
			}

			return links;
		}

		private void RemoveThreads(int diff)
		{
			if (tokenSources.Count == 0)
				return;

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

		private void UpdateThreads(int previous, int value)
		{
			int diff = previous - value;

			if (diff < 0) { AddThreads(-diff); }
			else { RemoveThreads(diff); }
		}
	}
}