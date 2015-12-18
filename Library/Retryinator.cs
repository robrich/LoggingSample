namespace LoggingSample.Library {
	using System;

	public interface IRetryinator {
		T Retry<T>(Func<T> Func, Func<Exception, bool> ExceptionHandler, Action LapCleanup = null);
	}

	public class Retryinator : IRetryinator {

		public T Retry<T>(Func<T> Func, Func<Exception, bool> ExceptionHandler, Action LapCleanup = null) {

			int retryCount = 3;
			T results = default(T);

			while (true) {
				try {
					results = Func();

					break; // Completed successfully
				} catch (Exception ex) {
					if (ExceptionHandler(ex) && retryCount > 1) {
						retryCount--;
					} else {
						throw;
					}
				}

				if (LapCleanup != null) {
					LapCleanup();
				}
			}

			return results;
		}

	}
}
