using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
			Required = DataParserHelper.CheckIfRequiredByRegex("^((https|http)?:\\/\\/)www.umei.cc\\/p\\/gaoqing\\/cn\\/\\d{1,2}.htm$");
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
			}
			var requests = new List<Request>();
			foreach (var tag in tags)
			{
				var request = new Request
				{
					Url = tag.Key,
					OwnerId = context.Response.Request.OwnerId
				};
				//Console.WriteLine(tag.Key);
				request.AddProperty("tag", tag.Value);
				request.AddProperty("referer", context.Response.Request.GetProperty("referer") ?? tag.Key);
				request.AddProperty("subject", context.Selectable.XPath(".//title").GetValue());
				requests.Add(request);
			}

			//如果当前为第一页找到最后一页并将2-最后一页加入到请求中
			var thisPage = context.Selectable.XPath(".//div[@class='NewPages']//ul//li[3]//a").GetValue();
			if (thisPage == "2")
			{
				tagNodes = context.Selectable.XPath(".//div[@class='NewPages']//ul//li[last()]//a").Nodes();
				foreach (var node in tagNodes)
				{
					var href = node.XPath("@href").Regex("\\d{1,3}.htm").GetValue().Replace(".htm", "");
					int.TryParse(href, out var totalPage);
					var reg = new Regex("\\d{1,3}.htm");
					Request[] reqArry = new Request[totalPage - 1];
					for (int i = 2; i <= totalPage; i++)
					{
						var url = reg.Replace(context.Response.Request.Url, $"{i}.htm");
						var request = new Request() { Url = url, OwnerId = context.Response.Request.OwnerId };
						reqArry[i - 2] = request;
					}
					context.AddExtraRequests(reqArry);
				}
			}
			context.AddExtraRequests(requests.ToArray());
			return Task.FromResult(DataFlowResult.Success);
		}
	}
}
