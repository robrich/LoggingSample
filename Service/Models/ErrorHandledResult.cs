namespace LoggingSample.Service.Models {

	public class ErrorHandledResult {
		public bool Handled { get; set; }
		public string ViewName { get; set; }
		public int? ErrorId { get; set; }
	}
}