/*global $:false */
(function () {
	'use strict';

	if (window.self !== window.top) {
		window.top.location.href = window.self.location.href;
	}

	$.log.url = '/error/log';
	// Hook $.ajax's error details
	$.logAjax({
		filter: function (e) {
			var resp;
			var ex;
			var parser;
			var url;
			if (e && e.log) {
				resp = e.log.responseText;
				ex = e.log.ex;
				if (ex && ex.success === false && ex.statusCode) {
					if (ex.url) {
						// server told us to go to that url instead
						window.location.href = ex.url;
						return false; // No need to log it or tell the user
					}
					if (ex.message) {
						e.log.message = ex.message + ', ' + e.log.message;
					}
					if (ex.errorId) {
						// don't tell server, it already knows
						e.settings.system = false;
						e.log.message += ', reference problem ticket id <a href="/Error/Error/' + encodeURIComponent(ex.errorId) + '">' + ex.errorId + '</a>';
					}
				}
				// FRAGILE: string matching portions of the login box and hard-coding the login url
				if (resp && typeof resp === 'string' && resp.indexOf('<html') > -1 && resp.indexOf('/Account/Login') > -1) {
					// They're unauthenticated getting to an authenticated page
					parser = $('<a />').get(0);
					parser.href = window.location.href;
					url = parser.pathname + parser.search + parser.hash;
					window.location.href = '/Account/Login?ReturnUrl=' + encodeURI(url);
					return false; // No need to log it or tell the user
				}
				if (e.log.httpStatus === 404) {
					// They're unauthenticated getting to an authenticated page
					parser = $('<a />').get(0);
					parser.href = window.location.href;
					url = parser.pathname + parser.search + parser.hash;
					window.location.href = '/Account/Login?ReturnUrl=' + encodeURI(url);
					return false; // No need to log it or tell the user
				}
			}
		}
	});
	// Hook browser's error details
	$.logBrowser();

	// Trim leading/trailing white space off each value prior to validation
	$.each($.validator.methods, function (key, value) {
		$.validator.methods[key] = function () {
			if (arguments.length > 0) {
				arguments[0] = $.trim(arguments[0]);
			}
			return value.apply(this, arguments);
		};
	});

}());
