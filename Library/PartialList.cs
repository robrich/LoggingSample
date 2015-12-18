namespace LoggingSample.Library {
	using System.Collections.Generic;

	public class PartialList<T> : List<T> {

		public PartialList() {
		}

		public PartialList(IEnumerable<T> Items, int TotalCount) {
			this.AddRange(Items);
			this.TotalCount = TotalCount;
		}

		/// <summary>
		/// This is not the total items returned but rather the total matching rows,
		/// of which the returned items are the subset that fits on the page specified
		/// </summary>
		public int TotalCount { get; set; }

	}
}
