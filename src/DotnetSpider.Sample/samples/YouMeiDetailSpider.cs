using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Downloader;
using DotnetSpider.Selector;
using System.Linq;

namespace DotnetSpider.Sample.samples
{
	public class YouMeiDetailSpider : DataParser
	{
		public YouMeiDetailSpider()
		{
			//只处理详细页数据其他页数据交给其他类型处理器 http://www.umei.cc/p/gaoqing/cn/188495.htm
			Required = DataParserHelper.CheckIfRequiredByRegex("^((https|http)?:\\/\\/)www.umei.cc\\/p\\/gaoqing\\/cn\\/\\d{3,7}.htm$", "^((https|http)?:\\/\\/)www.umei.cc\\/p\\/gaoqing\\/cn\\/\\d{3,7}_\\d{1,3}.htm$");
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
					var requests = new List<Request>();
					for (int i = 2; i <= pages; i++)
					{
						var request = new Request() { OwnerId = context.Response.Request.OwnerId, Url = context.Response.Request.Url.Replace(".htm", $"_{i}.htm") };
						request.AddProperty("tag", context.Selectable.XPath(".//title").GetValue());
						requests.Add(request);
					}
					context.AddExtraRequests(requests.ToArray()); ;
				}
			}
			var imgNodes = context.Selectable.XPath(".//div[@id='ArticleId0']//p//a//img").Nodes();
			foreach (var nodes in imgNodes)
			{
				var url = nodes.XPath("@src").GetValue();
				var newNode = (nodes as Selectable).Elements.FirstOrDefault();
				var alt = new Selectable(newNode.OuterHtml.Replace("alt=\"\"", ""));
				var name = alt.Regex("alt=\"[\u4e00-\u9fa5]+\"").GetValue().Replace("alt=\"", "").Replace("\"", "");
				var request = new Request() { Url = url, OwnerId = context.Response.Request.OwnerId };
				request.AddProperty("tag", context.Selectable.XPath(".//div[@class='position gray']//div[1]//a[2]").GetValue());
				request.AddProperty("referer", context.Response.Request.GetProperty("referer") ?? url);
				request.AddProperty("subject", name);
				ImageDownloader.GetInstance().AddRequest(request);
			}
			return Task.FromResult(DataFlowResult.Success);
		}
	}
}
