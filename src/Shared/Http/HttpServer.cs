namespace Shared.Http;

using Shared.Config;
using System.Net;

public abstract class HttpServer
{
	protected HttpRouter router;
	protected HttpListener server;

	public HttpServer()
	{
		router = new HttpRouter();

		Init();

		string host = Configuration.Get<string>("HOST", "http://127.0.0.1");
		string port = Configuration.Get<string>("PORT", "5000");
		string authority = $"{host}:{port}/";

		server = new HttpListener();
		server.Prefixes.Add(authority);

		Console.WriteLine("Server started at " + authority);
	}

	public abstract void Init();

	public async Task Start()
	{
		try
		{
			server.Start();
			Console.WriteLine($"HttpListener started. IsListening={server.IsListening}");
		}
		catch(Exception ex)
		{
			Console.WriteLine("Failed to start HttpListener: " + ex.ToString());
		
			throw;
		}

		while(server.IsListening)
		{
			try
			{
				HttpListenerContext ctx = await server.GetContextAsync();
				_ = router.HandleContextAsync(ctx);
			}
			catch(Exception ex)
			{
				Console.WriteLine("Error while accepting or handling context: " + ex.ToString());
			
				await Task.Delay(100);
			}
		}
	}

	public void Stop()
	{
		if(server.IsListening)
		{
			server.Stop();
			server.Close();
			Console.WriteLine("Server stopped.");
		}
	}
}
