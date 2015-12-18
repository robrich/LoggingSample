/*global window:true, jQuery:true, alert:false */
/*eslint no-shadow-restricted-names: 0*/
(function (window,document,$,undefined) {
	'use strict';
	// Set $.log.url before use

	var urlParser = document.createElement('a'); // https://gist.github.com/2428561
	var getLocalUrl = function (url) {
		var results = url;
		if (url) {
			/*eslint max-depth: [0,10]*/
			try {
				urlParser.href = url;
				if (urlParser.hostname) {
					// Only if a valid url
					if (urlParser.hostname === window.location.hostname) {
						// Only if url is in the same domain as the current page
						results = urlParser.pathname + urlParser.search + urlParser.hash;
						if (!results) {
							results = url;
						}
					}
				}
			} catch (err) {
				// FRAGILE: ASSUME: Failure to parse url isn't a deal breaker for logging
				results = url;
			}
		}
		return results;
	};

	// {text: 'show this', type: severity}
	$.logAlert = function (options) {
		// TODO: use a growl-like interface instead of just alerts
		alert(options.text);
	};

	// content can be object or string, if it has a message property, it'll display that and not the full object to the user
	// opts: {user: true, system: true, noticeType: 'error'}
	// user: tell the user, system: log to server, noticeType: severity passed to logAlert()
	var log = function (content, opts) {
		var mess = content; // message to system
		var usermess = null; // message to user
		var url = $.log.url;
		var data = null;
		var userComplete = false;
		var tellUser = null;

		var options = $.extend({
			user: true, // Tell the user
			system: true, // Tell the system
			noticeType: 'error' // Type type to pass to $.logAlert: notice, error, success
		}, opts || {});

		if (!url) {
			options.system = false; // Can't tell the server because no server url specified
		}

		if (!options.user && !options.system) {
			return; // We've successfully done nothing
		}

		if (!content) {
			return; // No need to tell everyone nothing
		}

		if (typeof content !== 'string') {
			mess = JSON.stringify(content);
			if (!mess) {
				return; // No need to tell everyone nothing
			}
		}

		if (options.user) {
			// Form the user message
			usermess = content; // default to JSON serialized content
			if (content && content.message) {
				usermess = content.message;
			}
			if(typeof usermess !== 'string') {
				usermess = JSON.stringify(usermess);
			}
			if (!usermess) {
				options.user = false; // Don't tell them nothing
			}
		}
		if (options.user) {
			// Tell the user
			if (options.system) {
				// Append to ajax result
				tellUser = function (errorId) {
					if (!userComplete) {
						userComplete = true;
						$.logAlert({ text: usermess, type: options.noticeType, errorId:errorId });
					}
				};
			} else {
				// Just tell the user now
				$.logAlert({ text: usermess, type: options.noticeType });
			}
		}

		if (options.system) {
			// Tell the server

			data = {
				message: mess,
				errorUrl: getLocalUrl(window.location.href),
				referrerUrl: null
			};
			if (document.referrer) {
				data.referrerUrl = document.referrer;
			}

			$.ajax(url, {
				type: 'POST',
				data: JSON.stringify(data),
				contentType: 'application/json; charset=utf-8',
				dataType: 'json',
				success: function (results) {
					if (tellUser) {
						if (results && results.mess) {
							usermess += ', ' + results.mess;
						}
						tellUser(results.errorId);
					}
				},
				error: function (/*xhr, status, error*/) {
					// Handle so we don't loop
					if (tellUser) {
						usermess += ', Error saving to log';
						tellUser();
					}
				}
			});
		}
	};

	var logBrowser = function (settings) {
		var origOnerror = window.onerror;
		var rootSettings;
		var filter;
		if (settings) {
			rootSettings = $.extend({}, settings);
		}
		if (rootSettings && rootSettings.filter && typeof rootSettings.filter === 'function') {
			filter = rootSettings.filter;
			delete rootSettings.filter;
		}
		window.onerror = function (message, url, lineNumber) {
			var proceed;
			var logContent;
			var localSettings;
			if (rootSettings) {
				localSettings = $.extend({}, rootSettings);
			}
			logContent = {
				message: message || 'window.onerror',
				url: getLocalUrl(url),
				lineNumber: lineNumber,
				source: 'window.onerror'
			};
			if (filter) {
				proceed = filter({args:arguments, log:logContent, settings:localSettings});
				// like jQuery return falsey to disable, return truthy or undefined to proceed
			}
			if (proceed === undefined || proceed) {
				log(logContent, localSettings);
			}
			if (origOnerror) {
				origOnerror.apply(window, arguments);
			}
		};
	};
	var logAjax = function (settings) {
		var rootSettings;
		var filter;
		if (settings) {
			rootSettings = $.extend({}, settings);
		}
		if (rootSettings && rootSettings.filter && typeof rootSettings.filter === 'function') {
			filter = rootSettings.filter;
			delete rootSettings.filter;
		}
		$(document).ajaxError(function (e, xhr, ajaxData/*, exception*/) {
			var proceed;
			var logContent;
			var localSettings;
			if (rootSettings) {
				localSettings = $.extend({}, rootSettings);
			}
			logContent = {
				message: 'A system error occurred in $.ajax()'
			};
			if (ajaxData && ajaxData.url) {
				if (ajaxData.url === $.log.url) {
					return; // Don't recurse
				}
				logContent.url = ajaxData.url;
			}
			if (xhr) {
				if (xhr.status === 0 || xhr.readyState === 0 || xhr.status === 12017 || xhr.status === 12029) {
					// Either server wasn't available or client killed it to navigate to a different page
					// 12017 is Windows error meaning closed connection
					// 12029 is Windows error meaning can't connect to server
					return; // FRAGILE: ASSUME: we don't need to log this
				}
				logContent.httpStatus = xhr.status;
				logContent.responseText = xhr.responseText;
				try {
					var contentType = xhr.getResponseHeader('content-type');
					if (contentType && contentType.indexOf('application/json') > -1) {
						logContent.ex = JSON.parse(xhr.responseText);
						if (logContent.ex && logContent.ex.Message) {
							logContent.xhrMessage = logContent.ex.Message;
						}
					}
				} catch (innerErr) {
					// FRAGILE: Swallow, avoid blowing up when trying to blow up
				}
			}
			if (filter) {
				proceed = filter({args:arguments, log:logContent, settings:localSettings});
				// like jQuery return falsey to disable, return truthy or undefined to proceed
			}
			if (proceed === undefined || proceed) {
				log(logContent, localSettings);
			}
		});
	};

	$.log = log;
	$.logBrowser = logBrowser;
	$.logAjax = logAjax;
}(window, window.document, jQuery));
