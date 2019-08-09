using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Downloader;

namespace DotnetSpider.Sample.samples
{
	public class YouMeiSpider : DataParser
	{
		public YouMeiSpider()
		{
			//只处理第父页数据其他页数据交给其他类型处理器
			Required = DataParserHelper.CheckIfRequiredByRegex("^((https|http)?:\\/\\/)www.umei.cc\\/p\\/gaoqing\\/cn\\/[0-50].htm$");
			//Follow = XpathFollow(".");
		}

		protected override Task<DataFlowResult> Parse(DataFlowContext context)
		{
			Dictionary<string, string> tags = new Dictionary<string, string>();
			var tagNodes = context.Selectable.XPath(".//div[@class='TypeList']//ul//li").Nodes();
			foreach (var node in tagNodes)
			{
				var url = node.XPath(".//a[@class='TypeBigPics']/@href").GetValue();
				var name = node.XPath(".//div[@class='ListTit']").GetValue();
				tags.Add(url, name);
				Console.WriteLine("url:" + url + " - name:" + name);
			}

			var requests = new List<Request>();
			foreach (var tag in tags)
			{
				var request = new Request
				{
					Url = tag.Key,
					OwnerId = context.Response.Request.OwnerId
				};
				request.AddProperty("tag", tag.Value);
				request.AddProperty("referer", context.Response.Request.GetProperty("referer") ?? tag.Key);
				request.AddProperty("subject", context.Selectable.XPath(".//title").GetValue());
				requests.Add(request);
				//ImageDownloader.GetInstance().AddRequest(request);
			}
			context.AddExtraRequests(requests.ToArray());
			return Task.FromResult(DataFlowResult.Success);
		}
	}
}
