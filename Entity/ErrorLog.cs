namespace LoggingSample.Entity {
	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;

	/// <summary>
	/// FRAGILE: Until errors go to the ELK stack ...
	/// </summary>
	public class ErrorLog : IEntity {
		public string UserMessage { get; set; }
		// nvarchar(MAX)
		public string ExceptionDetails { get; set; }
		[StringLength(10)]
		public string HttpMethod { get; set; }
		[StringLength(2000)]
		public string Url { get; set; }
		[StringLength(2000)]
		public string ReferrerUrl { get; set; }
		[StringLength(512)]
		public string UserAgent { get; set; }
		[StringLength(128)]
		public string ClientAddress { get; set; }
		// nvarchar(MAX)
		public string Headers { get; set; }
		// nvarchar(MAX)
		public string Body { get; set; }

		public int? UserId { get; set; }
		[ForeignKey("UserId")]
		public User User { get; set; }

	}
}
