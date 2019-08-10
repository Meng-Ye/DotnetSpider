using System;
using System.Collections.Generic;
using DotnetSpider.Kafka;
using DotnetSpider.Sample.samples;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			ImageDownloader.GetInstance().Start();
//			var configure = new LoggerConfiguration()
//#if DEBUG
//				.MinimumLevel.Verbose()
//#else
//				.MinimumLevel.Information()
//#endif
//				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
//				.Enrich.FromLogContext()
//				.WriteTo.Console().WriteTo
//				.RollingFile("dotnet-spider.log");
//			Log.Logger = configure.CreateLogger();

			//Startup.Execute<SsqSpider>(args);
			var builder = new SpiderHostBuilder()
				.ConfigureLogging(x => x.AddSerilog())
				.ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureServices(services =>
				{
					services.AddKafkaEventBus();
					services.AddLocalEventBus();
					//services.AddLocalDownloadCenter();
					//services.AddDownloaderAgent(x =>
					//{
					//	x.UseFileLocker();
					//	x.UseDefaultAdslRedialer();
					//	x.UseDefaultInternetDetector();
					//});
					//services.AddStatisticsCenter(x => x.UseMemory());
				});
			var provider = builder.Build();
			var spider = provider.Create<Spider>();

			spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
			spider.Name = "优美图片采集"; // 设置任务名称
			spider.Speed = 2; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
			spider.Depth = 2; // 设置采集深度
			spider.AddDataFlow(new YouMeiSpider());
			spider.AddDataFlow(new YouMeiDetailSpider());
			//spider.AddDataFlow(new NvshensPageTagDataParser());
			//spider.AddDataFlow(new NvshensFirstPageDetailDataParser());
			//spider.AddDataFlow(new NvshensPageDetailDataParser());
			//spider.AddRequests("https://www.nvshens.com/gallery/"); // 设置起始链接
			spider.AddRequests("http://www.umei.cc/p/gaoqing/cn/1.htm"); // 设置起始链接
			spider.RunAsync(); // 启动

			// await DistributedSpider.Run();
			Console.Read();
		}
	}
}
