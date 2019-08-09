using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Downloader;

namespace DotnetSpider.Sample.samples
{
	public class YouMeiDetailSpider : DataParser
	{
		public YouMeiDetailSpider()
		{
			//只处理详细页数据其他页数据交给其他类型处理器 http://www.umei.cc/p/gaoqing/cn/188495.htm
			Required = DataParserHelper.CheckIfRequiredByRegex("^((https|http)?:\\/\\/)www.umei.cc\\/p\\/gaoqing\\/cn\\/\\d{3,7}.htm$$");
			//Follow = XpathFollow(".");
		}

		protected override Task<DataFlowResult> Parse(DataFlowContext context)
		{
			Dictionary<string, string> tags = new Dictionary<string, string>();
			var tagNodes = context.Selectable.Regex("Next(.+)").Nodes();
			foreach (var node in tagNodes)
			{
				//找到页面函数里面有总页数和当前页面ID
				var el = node.GetValue().Replace("Next(", "").Replace("\\", "").Replace("\"", "");
				var elArry = el.Split(',');
				int.TryParse(elArry[1], out int pages);
				if (pages > 1)
				{
					for (int i = 2; i < pages; i++)
					{
						context.AddExtraRequests(new Request() { OwnerId = context.Response.Request.OwnerId, Url = context.Response.Request.Url.Replace(".htm", $"{i}_.htm") }); ;
					}
				}
			}

			var imgNodes = context.Selectable.XPath(".//div[@id='ArticleId0']//p//a//img").Nodes();
			foreach (var nodes in imgNodes)
			{
				var url = nodes.XPath("@src").GetValue();
				var name = nodes.XPath("@alt").GetValue();
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
