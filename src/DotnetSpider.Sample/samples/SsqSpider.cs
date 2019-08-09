using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Parser.Attribute;
using DotnetSpider.DataFlow.Parser.Formatter;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.DataFlow.Storage.Model;
using DotnetSpider.DataFlow.Storage.MySql;
using DotnetSpider.Downloader;
using DotnetSpider.EventBus;
using DotnetSpider.Scheduler;
using DotnetSpider.Selector;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class SsqSpider : Spider
	{
		public SsqSpider(SpiderParameters parameters) : base(parameters)
		{
		}
		protected override void Initialize()
		{
			NewGuidId();
			Scheduler = new QueueDistinctBfsScheduler();
			Speed = 1;
			Depth = 3;
			AddDataFlow(new DataParser<CnblogsEntry>()).AddDataFlow(new MySqlEntityStorage(
				StorageType.InsertAndUpdate,
				"Database='mysql';Data Source=192.168.11.128;password=Yang123456.;User ID=root;Port=3306;"));
			AddRequests(
				new Request("http://datachart.500.com/ssq/", new Dictionary<string, string> { { "彩票", "双色球" } })
				);
		}

		protected override async Task OnExiting()
		{
		}

		[Schema("lotteryticket", "lottery_ticket_dlt")]
		[EntitySelector(Expression = ".//div[@class='warp']//table//tbody[@id='tdata']//tr[count(td)>1]", Type = SelectorType.XPath)]
		//[FollowSelector(XPaths = new[] {"/div[@class='pager']"})]
		public class CnblogsEntry : EntityBase<CnblogsEntry>
		{
			protected override void Configure()
			{
				HasIndex(x => new { x.ID });
			}

			//public string ID { get; set; } = Guid.NewGuid().ToString("N");

			[StringLength(100)]
			[ValueSelector(Expression = "td[@align='center']")]
			public string ID { get; set; }

			[StringLength(200)]
			[ValueSelector(Expression = "彩票", Type = SelectorType.Enviroment)]
			public string Title { get; set; }

			[ValueSelector(Expression = "td[@class='chartBall01'][1]")]
			public string RedOne { get; set; }

			[ValueSelector(Expression = "td[@class='chartBall01'][2]")]
			public string RedTwo { get; set; }

			[ValueSelector(Expression = "td[@class='chartBall01'][3]")]
			public string RedThree { get; set; }

			[ValueSelector(Expression = "td[@class='chartBall01'][4]")]
			public string RedFour { get; set; }

			[ValueSelector(Expression = "td[@class='chartBall01'][5]")]
			public string RedFive { get; set; }

			[ValueSelector(Expression = "td[@class='chartBall01'][6]")]
			public string RedSix { get; set; }

			[ValueSelector(Expression = "td[@class='chartBall02']")]
			public string Blue { get; set; }

			[ValueSelector(Expression = "DATETIME", Type = SelectorType.Enviroment)]
			public DateTime CreationTime { get; set; }
		}
	}
}
