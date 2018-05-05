(function() {
	'dont use strict';
	var $asm = {};
	global.tab = global.tab || {};
	ss.initAssembly($asm, 'LayoutMetrics');
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Bootstrap.NullMetricsContext
	var $tab_$NullMetricsContext = function() {
		$tab_MetricsContext.call(this, -1, 0, '', {});
		this.open = false;
		this.start = 0;
		this.end = 0;
	};
	$tab_$NullMetricsContext.__typeName = 'tab.$NullMetricsContext';
	$tab_$NullMetricsContext.get_$instance = function NullMetricsContext$get_Instance() {
		if (ss.isNullOrUndefined($tab_$NullMetricsContext.$instance)) {
			$tab_$NullMetricsContext.$instance = new $tab_$NullMetricsContext();
		}
		return $tab_$NullMetricsContext.$instance;
	};
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.BaseLogAppender
	var $tab_BaseLogAppender = function() {
		this.$filters = null;
		this.$filters = [];
	};
	$tab_BaseLogAppender.__typeName = 'tab.BaseLogAppender';
	global.tab.BaseLogAppender = $tab_BaseLogAppender;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.BrowserSupport
	var $tab_BrowserSupport = function() {
	};
	$tab_BrowserSupport.__typeName = 'tab.BrowserSupport';
	$tab_BrowserSupport.get_getComputedStyle = function BrowserSupport$get_GetComputedStyle() {
		return 'getComputedStyle' in window;
	};
	$tab_BrowserSupport.get_addEventListener = function BrowserSupport$get_AddEventListener() {
		return 'addEventListener' in document;
	};
	$tab_BrowserSupport.get_selectStart = function BrowserSupport$get_SelectStart() {
		return $tab_BrowserSupport.$selectStart;
	};
	$tab_BrowserSupport.get_touch = function BrowserSupport$get_Touch() {
		return $tab_BrowserSupport.$touch;
	};
	$tab_BrowserSupport.get_fontLoaderApi = function BrowserSupport$get_FontLoaderApi() {
		return $tab_BrowserSupport.$fonts;
	};
	$tab_BrowserSupport.get_dataUri = function BrowserSupport$get_DataUri() {
		return $tab_BrowserSupport.$dataUri;
	};
	$tab_BrowserSupport.get_postMessage = function BrowserSupport$get_PostMessage() {
		return $tab_BrowserSupport.$postMessage;
	};
	$tab_BrowserSupport.get_historyApi = function BrowserSupport$get_HistoryApi() {
		return $tab_BrowserSupport.$historyApi;
	};
	$tab_BrowserSupport.get_consoleLogFormating = function BrowserSupport$get_ConsoleLogFormating() {
		return $tab_BrowserSupport.$consoleLogFormatting;
	};
	$tab_BrowserSupport.get_isMobile = function BrowserSupport$get_IsMobile() {
		return $tab_BrowserSupport.$isAndroid || $tab_BrowserSupport.$isIos;
	};
	$tab_BrowserSupport.get_isIos = function BrowserSupport$get_IsIos() {
		return $tab_BrowserSupport.$isIos;
	};
	$tab_BrowserSupport.get_isAndroid = function BrowserSupport$get_IsAndroid() {
		return $tab_BrowserSupport.$isAndroid;
	};
	$tab_BrowserSupport.get_isChrome = function BrowserSupport$get_IsChrome() {
		return $tab_BrowserSupport.$isChrome;
	};
	$tab_BrowserSupport.get_isMac = function BrowserSupport$get_IsMac() {
		return $tab_BrowserSupport.$isMac;
	};
	$tab_BrowserSupport.get_isIE = function BrowserSupport$get_IsIE() {
		return $tab_BrowserSupport.$isIE;
	};
	$tab_BrowserSupport.get_isFF = function BrowserSupport$get_IsFF() {
		return $tab_BrowserSupport.$isFF;
	};
	$tab_BrowserSupport.get_isOpera = function BrowserSupport$get_IsOpera() {
		return $tab_BrowserSupport.$isOpera;
	};
	$tab_BrowserSupport.get_isSafari = function BrowserSupport$get_IsSafari() {
		return $tab_BrowserSupport.$isSafari;
	};
	$tab_BrowserSupport.get_isWindows = function BrowserSupport$get_IsWindows() {
		return $tab_BrowserSupport.$isWindows;
	};
	$tab_BrowserSupport.get_browserVersion = function BrowserSupport$get_BrowserVersion() {
		return $tab_BrowserSupport.$internetExplorerVersion;
	};
	$tab_BrowserSupport.get_safariVersion = function BrowserSupport$get_SafariVersion() {
		return $tab_BrowserSupport.$safariVersion;
	};
	$tab_BrowserSupport.get_iosVersion = function BrowserSupport$get_IosVersion() {
		return $tab_BrowserSupport.$iosVersion;
	};
	$tab_BrowserSupport.get_raisesEventOnImageReassignment = function BrowserSupport$get_RaisesEventOnImageReassignment() {
		return !$tab_BrowserSupport.$isSafari;
	};
	$tab_BrowserSupport.get_imageLoadIsSynchronous = function BrowserSupport$get_ImageLoadIsSynchronous() {
		return $tab_BrowserSupport.$isIE;
	};
	$tab_BrowserSupport.get_useAlternateHitStrategy = function BrowserSupport$get_UseAlternateHitStrategy() {
		return $tab_BrowserSupport.$shouldUseAlternateHitStrategy;
	};
	$tab_BrowserSupport.get_cssTransform = function BrowserSupport$get_CssTransform() {
		return ss.isValue($tab_BrowserSupport.$cssTransformName);
	};
	$tab_BrowserSupport.get_cssTransformName = function BrowserSupport$get_CssTransformName() {
		return $tab_BrowserSupport.$cssTransformName;
	};
	$tab_BrowserSupport.get_cssTransitionName = function BrowserSupport$get_CssTransitionName() {
		return $tab_BrowserSupport.$cssTransitionName;
	};
	$tab_BrowserSupport.get_cssTranslate2D = function BrowserSupport$get_CssTranslate2D() {
		return $tab_BrowserSupport.$cssTranslate2d;
	};
	$tab_BrowserSupport.get_cssTranslate3D = function BrowserSupport$get_CssTranslate3D() {
		return $tab_BrowserSupport.$cssTranslate3d;
	};
	$tab_BrowserSupport.get_backingStoragePixelRatio = function BrowserSupport$get_BackingStoragePixelRatio() {
		return $tab_BrowserSupport.$backingStoragePixelRatio;
	};
	$tab_BrowserSupport.get_devicePixelRatio = function BrowserSupport$get_DevicePixelRatio() {
		return $tab_BrowserSupport.$devicePixelRatio;
	};
	$tab_BrowserSupport.get_canvasLinePattern = function BrowserSupport$get_CanvasLinePattern() {
		return $tab_BrowserSupport.$canvasLinePattern;
	};
	$tab_BrowserSupport.get_dateInput = function BrowserSupport$get_DateInput() {
		return $tab_BrowserSupport.$dateInput;
	};
	$tab_BrowserSupport.get_dateTimeInput = function BrowserSupport$get_DateTimeInput() {
		return $tab_BrowserSupport.$dateTimeInput;
	};
	$tab_BrowserSupport.get_dateTimeLocalInput = function BrowserSupport$get_DateTimeLocalInput() {
		return $tab_BrowserSupport.$dateTimeLocalInput;
	};
	$tab_BrowserSupport.get_timeInput = function BrowserSupport$get_TimeInput() {
		return $tab_BrowserSupport.$timeInput;
	};
	$tab_BrowserSupport.get_setSelectionRange = function BrowserSupport$get_SetSelectionRange() {
		return $tab_BrowserSupport.$setSelectionRange;
	};
	$tab_BrowserSupport.get_mouseWheelEvent = function BrowserSupport$get_MouseWheelEvent() {
		var mouseWheelEvent;
		if ('onwheel' in window.document.documentElement) {
			mouseWheelEvent = 'wheel';
		}
		else if ('onmousewheel' in window.document.documentElement) {
			mouseWheelEvent = 'mousewheel';
		}
		else {
			mouseWheelEvent = 'MozMousePixelScroll';
		}
		return mouseWheelEvent;
	};
	$tab_BrowserSupport.get_mouseCapture = function BrowserSupport$get_MouseCapture() {
		return 'releaseCapture' in document;
	};
	$tab_BrowserSupport.get_orientationChange = function BrowserSupport$get_OrientationChange() {
		return 'onorientationchange' in window;
	};
	$tab_BrowserSupport.get_isGeolocationSupported = function BrowserSupport$get_IsGeolocationSupported() {
		return ss.isValue(window.navigator.geolocation);
	};
	$tab_BrowserSupport.detectBrowserSupport = function BrowserSupport$DetectBrowserSupport() {
		var body = document.body;
		var div = document.createElement('div');
		body.appendChild(div);
		$tab_BrowserSupport.$selectStart = 'onselectstart' in div;
		body.removeChild(div).style.display = 'none';
		$tab_BrowserSupport.$postMessage = 'postMessage' in window;
		$tab_BrowserSupport.$historyApi = typeof(window.history['pushState']) === 'function' && typeof(window.history['replaceState']) === 'function';
		$tab_BrowserSupport.$detectDataUriSupport();
		$tab_BrowserSupport.$detectConsoleLogFormatting();
		$tab_BrowserSupport.$detectBrowser();
		$tab_BrowserSupport.$detectTransitionSupport();
		$tab_BrowserSupport.$detectTransformSupport();
		$tab_BrowserSupport.$detectDocumentElementFromPoint();
		$tab_BrowserSupport.$detectDevicePixelRatio();
		$tab_BrowserSupport.$detectBackingStoragePixelRatio();
		$tab_BrowserSupport.$detectDateInputSupport();
		$tab_BrowserSupport.$detectCanvasLinePattern();
		$tab_BrowserSupport.$detectSetSelectionRangeSupport();
	};
	$tab_BrowserSupport.getOrigin = function BrowserSupport$GetOrigin(location) {
		var origin = location.origin;
		if (ss.isNullOrUndefined(origin)) {
			origin = location.protocol + '//' + location.host;
		}
		return origin;
	};
	$tab_BrowserSupport.doPostMessageWithContext = function BrowserSupport$DoPostMessageWithContext(message) {
		var success = $tab_Utility.doPostMessageWithContext(message);
		if (!success) {
		}
	};
	$tab_BrowserSupport.$detectDataUriSupport = function BrowserSupport$DetectDataUriSupport() {
		var imgObj = $('<img />');
		var img = imgObj[0];
		imgObj.on('load error', function() {
			$tab_BrowserSupport.$dataUri = img.width === 1 && img.height === 1;
		});
		img.src = 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==';
	};
	$tab_BrowserSupport.$detectDocumentElementFromPoint = function BrowserSupport$DetectDocumentElementFromPoint() {
		var body = window.document.body;
		if ($tab_BrowserSupport.$isWebKit && $tab_BrowserSupport.get_isMobile()) {
			var target = $('<div></div>');
			target.css(ss.mkdict(['position', 'absolute', 'top', '300px', 'left', '300px', 'width', '25px', 'height', '25px', 'z-index', '10000']));
			var elem = target.get(0);
			try {
				body.appendChild(elem);
				$tab_BrowserSupport.$shouldUseAlternateHitStrategy = !ss.referenceEquals(document.elementFromPoint(310, 310), elem);
			}
			catch ($t1) {
			}
			finally {
				target.remove();
			}
		}
	};
	$tab_BrowserSupport.$detectConsoleLogFormatting = function BrowserSupport$DetectConsoleLogFormatting() {
		try {
			if (ss.isValue(window.console && ss.isValue(window.console.log))) {
				$tab_BrowserSupport.$consoleLogFormatting = window.navigator.userAgent.indexOf('iPad') < 0;
			}
			else {
				$tab_BrowserSupport.$consoleLogFormatting = false;
			}
		}
		catch ($t1) {
			$tab_BrowserSupport.$consoleLogFormatting = false;
		}
	};
	$tab_BrowserSupport.$detectBrowser = function BrowserSupport$DetectBrowser() {
		var ua = $tab_BrowserSupport.$getUserAgent();
		$tab_BrowserSupport.$isKhtml = ua.indexOf('Konqueror') >= 0;
		$tab_BrowserSupport.$isWebKit = ua.indexOf('WebKit') >= 0;
		$tab_BrowserSupport.$isChrome = ua.indexOf('Chrome') >= 0;
		$tab_BrowserSupport.$isSafari = ua.indexOf('Safari') >= 0 && !$tab_BrowserSupport.$isChrome;
		$tab_BrowserSupport.$isOpera = ua.indexOf('Opera') >= 0;
		if ($tab_BrowserSupport.$isSafari) {
			var versionMatches = ua.match(new RegExp('\\bVersion\\/(\\d+\\.\\d+)'));
			if (ss.isValue(versionMatches)) {
				$tab_BrowserSupport.$safariVersion = parseFloat(versionMatches[1]);
			}
		}
		$tab_BrowserSupport.$internetExplorerVersion = 0;
		$tab_BrowserSupport.$isIE = false;
		var oldIEVersions = ua.match(new RegExp('\\bMSIE (\\d+\\.\\d+)'));
		if (ss.isValue(oldIEVersions)) {
			$tab_BrowserSupport.$isIE = true;
			$tab_BrowserSupport.$internetExplorerVersion = parseFloat(oldIEVersions[1]);
		}
		if (!$tab_BrowserSupport.$isIE && !$tab_BrowserSupport.$isOpera && (ua.indexOf('Trident') >= 0 || ua.indexOf('Edge/') >= 0)) {
			var tridentIEVersions = ua.match(new RegExp('\\brv:(\\d+\\.\\d+)'));
			var edgeIEVersions = ua.match(new RegExp('Edge/(\\d+\\.\\d+)'));
			if (ss.isValue(tridentIEVersions)) {
				$tab_BrowserSupport.$isIE = true;
				$tab_BrowserSupport.$internetExplorerVersion = parseFloat(tridentIEVersions[1]);
			}
			else if (ss.isValue(edgeIEVersions)) {
				$tab_BrowserSupport.$isIE = true;
				$tab_BrowserSupport.$isChrome = false;
				$tab_BrowserSupport.$isSafari = false;
				$tab_BrowserSupport.$internetExplorerVersion = parseFloat(edgeIEVersions[1]);
			}
		}
		$tab_BrowserSupport.$isMozilla = !$tab_BrowserSupport.$isKhtml && !$tab_BrowserSupport.$isWebKit && !$tab_BrowserSupport.$isIE && ua.indexOf('Gecko') >= 0;
		$tab_BrowserSupport.$isFF = $tab_BrowserSupport.$isMozilla || ua.indexOf('Firefox') >= 0 || ua.indexOf('Minefield') >= 0;
		var commandRegex = new RegExp('iPhone|iPod|iPad');
		$tab_BrowserSupport.$isIos = commandRegex.test(ua);
		if ($tab_BrowserSupport.$isIos) {
			var iosVersions = ua.match(new RegExp('\\bOS ([\\d+_?]+) like Mac OS X'));
			if (ss.isValue(iosVersions)) {
				$tab_BrowserSupport.$iosVersion = parseFloat(ss.replaceAllString(iosVersions[1].replace('_', '.'), '_', ''));
			}
		}
		$tab_BrowserSupport.$isAndroid = ua.indexOf('Android') >= 0 && !$tab_BrowserSupport.$isIE;
		$tab_BrowserSupport.$isMac = ua.indexOf('Mac') >= 0;
		$tab_BrowserSupport.$isWindows = ua.indexOf('Windows') >= 0;
	};
	$tab_BrowserSupport.$getUserAgent = function BrowserSupport$GetUserAgent() {
		return window.navigator.userAgent;
	};
	$tab_BrowserSupport.$trySettingCssProperty = function BrowserSupport$TrySettingCssProperty(styleProp, cssProp, val) {
		var e = document.createElement('div');
		try {
			document.body.insertBefore(e, null);
			if (!(styleProp in e.style)) {
				return false;
			}
			e.style[styleProp] = val;
			var s = $tab_DomUtil.getComputedStyle(e);
			var computedVal = s[cssProp];
			return !$tab_MiscUtil.isNullOrEmpty$1(computedVal) && computedVal !== 'none';
		}
		finally {
			document.body.removeChild(e).style.display = 'none';
		}
	};
	$tab_BrowserSupport.$detectTransitionSupport = function BrowserSupport$DetectTransitionSupport() {
		var transitions = ss.mkdict(['transition', 'transition', 'webkitTransition', '-webkit-transition', 'msTransition', '-ms-transition', 'mozTransition', '-moz-transition', 'oTransition', '-o-transition']);
		var $t1 = new ss.ObjectEnumerator(transitions);
		try {
			while ($t1.moveNext()) {
				var t = $t1.current();
				var $t2 = document.body.style;
				if (!(t.key in $t2)) {
					continue;
				}
				$tab_BrowserSupport.$cssTransitionName = t.value;
				break;
			}
		}
		finally {
			$t1.dispose();
		}
	};
	$tab_BrowserSupport.$detectTransformSupport = function BrowserSupport$DetectTransformSupport() {
		var transforms = ss.mkdict(['transform', 'transform', 'webkitTransform', '-webkit-transform', 'msTransform', '-ms-transform', 'mozTransform', '-moz-transform', 'oTransform', '-o-transform']);
		var $t1 = new ss.ObjectEnumerator(transforms);
		try {
			while ($t1.moveNext()) {
				var t = $t1.current();
				var $t2 = document.body.style;
				if (!(t.key in $t2)) {
					continue;
				}
				$tab_BrowserSupport.$cssTransformName = t.value;
				$tab_BrowserSupport.$cssTranslate2d = $tab_BrowserSupport.$trySettingCssProperty(t.key, t.value, 'translate(1px,1px)');
				$tab_BrowserSupport.$cssTranslate3d = $tab_BrowserSupport.$trySettingCssProperty(t.key, t.value, 'translate3d(1px,1px,1px)');
				break;
			}
		}
		finally {
			$t1.dispose();
		}
	};
	$tab_BrowserSupport.$detectDevicePixelRatio = function BrowserSupport$DetectDevicePixelRatio() {
		$tab_BrowserSupport.$devicePixelRatio = ss.coalesce(window.self['devicePixelRatio'], 1);
	};
	$tab_BrowserSupport.$detectBackingStoragePixelRatio = function BrowserSupport$DetectBackingStoragePixelRatio() {
		var canvas = document.createElement('canvas');
		if (ss.isNullOrUndefined(canvas)) {
			$tab_BrowserSupport.$backingStoragePixelRatio = 1;
			return;
		}
		var context = null;
		if (typeof(ss.getInstanceType(canvas)['getContext']) === 'function') {
			context = canvas.getContext('2d');
		}
		if (ss.isNullOrUndefined(context)) {
			$tab_BrowserSupport.$backingStoragePixelRatio = 1;
			return;
		}
		var ctx = context;
		$tab_BrowserSupport.$backingStoragePixelRatio = ctx.webkitBackingStorePixelRatio || ctx.mozBackingStorePixelRatio || ctx.msBackingStorePixelRatio || ctx.oBackingStorePixelRatio || 1;
	};
	$tab_BrowserSupport.$detectCanvasLinePattern = function BrowserSupport$DetectCanvasLinePattern() {
		var canvas = document.createElement('canvas');
		if (ss.isNullOrUndefined(canvas)) {
			return;
		}
		var context = null;
		if (typeof(canvas['getContext']) === 'function') {
			context = canvas.getContext('2d');
		}
		if (ss.isNullOrUndefined(context)) {
			return;
		}
		$tab_BrowserSupport.$canvasLinePattern = typeof(context['setLineDash']) === 'function' || 'mozDash' in context || 'webkitLineDash' in context;
	};
	$tab_BrowserSupport.$detectSetSelectionRangeSupport = function BrowserSupport$DetectSetSelectionRangeSupport() {
		var inputObject = $('<input>');
		$tab_BrowserSupport.$setSelectionRange = typeof(inputObject.get(0)['setSelectionRange']) === 'function';
	};
	$tab_BrowserSupport.$detectDateInputSupport = function BrowserSupport$DetectDateInputSupport() {
		$tab_BrowserSupport.$dateInput = $tab_BrowserSupport.$detectCustomInputSupport('date');
		$tab_BrowserSupport.$dateTimeInput = $tab_BrowserSupport.$detectCustomInputSupport('datetime');
		$tab_BrowserSupport.$dateTimeLocalInput = $tab_BrowserSupport.$detectCustomInputSupport('datetime-local');
		$tab_BrowserSupport.$timeInput = $tab_BrowserSupport.$detectCustomInputSupport('time');
	};
	$tab_BrowserSupport.$detectCustomInputSupport = function BrowserSupport$DetectCustomInputSupport(inputType) {
		var inputObject = $("<input type='" + inputType + "'>").css({ position: 'absolute', visibility: 'hidden' }).appendTo($(document.body));
		var input = inputObject.get(0);
		var reportedInputType = input.getAttribute('type');
		var InvalidDataString = '@inva/1d:)';
		input.value = InvalidDataString;
		var supportsInput = ss.referenceEquals(reportedInputType, inputType) && !ss.referenceEquals(input.value, InvalidDataString);
		inputObject.remove();
		return supportsInput;
	};
	global.tab.BrowserSupport = $tab_BrowserSupport;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.ConsoleLogAppender
	var $tab_ConsoleLogAppender = function() {
		this.$levelMethods = null;
		$tab_BaseLogAppender.call(this);
	};
	$tab_ConsoleLogAppender.__typeName = 'tab.ConsoleLogAppender';
	$tab_ConsoleLogAppender.enableLogging = function ConsoleLogAppender$EnableLogging(filter) {
		if (ss.isNullOrUndefined($tab_ConsoleLogAppender.$globalAppender)) {
			$tab_ConsoleLogAppender.$globalAppender = new $tab_ConsoleLogAppender();
			$tab_Logger.addAppender($tab_ConsoleLogAppender.$globalAppender);
		}
		$tab_ConsoleLogAppender.$globalAppender.addFilter(filter || function() {
			return true;
		});
	};
	$tab_ConsoleLogAppender.disableLogging = function ConsoleLogAppender$DisableLogging() {
		if (ss.isNullOrUndefined($tab_ConsoleLogAppender.$globalAppender)) {
			return;
		}
		$tab_Logger.removeAppender($tab_ConsoleLogAppender.$globalAppender);
		$tab_ConsoleLogAppender.$globalAppender = null;
	};
	global.tab.ConsoleLogAppender = $tab_ConsoleLogAppender;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.DomUtil
	var $tab_DomUtil = function() {
	};
	$tab_DomUtil.__typeName = 'tab.DomUtil';
	$tab_DomUtil.get_$log = function DomUtil$get_Log() {
		return $tab_Logger.lazyGetLogger($tab_DomUtil);
	};
	$tab_DomUtil.get_documentBody = function DomUtil$get_DocumentBody() {
		return document.body;
	};
	$tab_DomUtil.getComputedStyle = function DomUtil$GetComputedStyle(e) {
		if ($tab_BrowserSupport.get_getComputedStyle()) {
			var s = window.getComputedStyle(e);
			if (ss.isValue(s)) {
				return s;
			}
		}
		return e.style;
	};
	$tab_DomUtil.getComputedZIndex = function DomUtil$GetComputedZIndex(child) {
		$tab_Param.verifyValue(child, 'child');
		var iter = $(child);
		var lastPositioned = iter;
		var html = document.documentElement;
		var body = document.body;
		while (iter.length !== 0 && !ss.referenceEquals(iter[0], body) && !ss.referenceEquals(iter[0], html)) {
			var pos = iter.css('position');
			if (pos === 'absolute' || pos === 'fixed') {
				lastPositioned = iter;
			}
			iter = iter.offsetParent();
		}
		return $tab_DomUtil.$parseZIndexProperty(lastPositioned);
	};
	$tab_DomUtil.resize = function DomUtil$Resize(e, rect) {
		if (typeof(e['resize']) === 'function') {
			e.resize(rect);
		}
		else {
			$tab_DomUtil.setMarginBox(e.domNode || e, rect);
		}
	};
	$tab_DomUtil.getContentBox = function DomUtil$GetContentBox(e) {
		var obj = $(e);
		return { l: parseInt(obj.css('padding-left'), 10) || 0, t: parseInt(obj.css('padding-top'), 10) || 0, w: obj.width(), h: obj.height() };
	};
	$tab_DomUtil.setContentBox = function DomUtil$SetContentBox(e, r) {
		$(e).width(r.w).height(r.h);
	};
	$tab_DomUtil.setMarginBox = function DomUtil$SetMarginBox(e, r) {
		$tab_DomUtil.setMarginBoxJQ($(e), r);
	};
	$tab_DomUtil.setMarginBoxJQ = function DomUtil$SetMarginBoxJQ(o, r) {
		$tab_DomUtil.$setMarginSizeJQ(o, $tab_RecordCast.rectAsSize(r));
		if (!isNaN(r.t)) {
			o.css('top', r.t + 'px');
		}
		if (!isNaN(r.l)) {
			o.css('left', r.l + 'px');
		}
	};
	$tab_DomUtil.setAbsolutePositionBox = function DomUtil$SetAbsolutePositionBox(o, r) {
		o.css({ left: (r.l || 0) + 'px', top: (r.t || 0) + 'px', width: (r.w || 0) + 'px', height: (r.h || 0) + 'px' });
	};
	$tab_DomUtil.getMarginBox = function DomUtil$GetMarginBox(e) {
		return $tab_DomUtil.getMarginBoxJQ($(e));
	};
	$tab_DomUtil.getMarginBoxJQ = function DomUtil$GetMarginBoxJQ(o) {
		var p = o.position();
		return { l: $tab_DoubleUtil.roundToInt(p.left), t: $tab_DoubleUtil.roundToInt(p.top), w: o.outerWidth(true), h: o.outerHeight(true) };
	};
	$tab_DomUtil.getRectXY = function DomUtil$GetRectXY(o) {
		var x = $tab_DoubleUtil.roundToInt($tab_DomUtil.getPageOffset(o).left);
		var y = $tab_DoubleUtil.roundToInt($tab_DomUtil.getPageOffset(o).top);
		var w = o.outerWidth(true);
		var h = o.outerHeight(true);
		return { x: x, y: y, w: w, h: h };
	};
	$tab_DomUtil.isAncestorOf = function DomUtil$IsAncestorOf(ancestor, child) {
		if (ss.isNullOrUndefined(ancestor) || ss.isNullOrUndefined(child)) {
			return false;
		}
		return $(child).parents().index(ancestor) >= 0;
	};
	$tab_DomUtil.isEqualOrAncestorOf = function DomUtil$IsEqualOrAncestorOf(ancestor, child) {
		if (ss.isNullOrUndefined(ancestor) || ss.isNullOrUndefined(child)) {
			return false;
		}
		return ss.referenceEquals(ancestor, child) || $tab_DomUtil.isAncestorOf(ancestor, child);
	};
	$tab_DomUtil.setElementPosition = function DomUtil$SetElementPosition(e, pageX, pageY, duration, useTransform) {
		if ((!ss.isValue(useTransform) || ss.unbox(useTransform)) && $tab_BrowserSupport.get_cssTransform()) {
			var styling = ss.mkdict(['top', '0px', 'left', '0px']);
			if ($tab_BrowserSupport.get_cssTranslate3D()) {
				var transformVal = (new ss.StringBuilder('translate3d(')).append(pageX).append('px,').append(pageY).append('px,').append('0px)').toString();
				styling[$tab_BrowserSupport.get_cssTransformName()] = transformVal;
				if (ss.isValue(duration)) {
					styling[$tab_BrowserSupport.get_cssTransitionName() + '-duration'] = duration;
				}
				e.css(styling);
				return;
			}
			if ($tab_BrowserSupport.get_cssTranslate2D()) {
				var transformVal1 = (new ss.StringBuilder('translate(')).append(pageX).append('px,').append(pageY).append('px)').toString();
				styling[$tab_BrowserSupport.get_cssTransformName()] = transformVal1;
				if (ss.isValue(duration)) {
					styling[$tab_BrowserSupport.get_cssTransitionName() + '-duration'] = duration;
				}
				e.css(styling);
				return;
			}
		}
		var css = ss.mkdict(['position', 'absolute', 'top', pageY + 'px', 'left', pageX + 'px']);
		if ($tab_BrowserSupport.get_cssTransform()) {
			css[$tab_BrowserSupport.get_cssTransformName()] = '';
		}
		e.css(css);
	};
	$tab_DomUtil.getElementPosition = function DomUtil$GetElementPosition(e) {
		return $tab_PointUtil.fromPosition(e.offset());
	};
	$tab_DomUtil.getElementClientPosition = function DomUtil$GetElementClientPosition(e) {
		var p = $tab_DomUtil.getElementPosition(e);
		p.x -= $(document.documentElement).scrollLeft();
		p.y -= $(document.documentElement).scrollTop();
		return p;
	};
	$tab_DomUtil.getTransformOffset = function DomUtil$GetTransformOffset(element) {
		if (ss.isNullOrUndefined(element)) {
			return { left: 0, top: 0 };
		}
		var fullTransform = element.css('transform');
		if (ss.isNullOrEmptyString(fullTransform)) {
			return { left: 0, top: 0 };
		}
		var transform = fullTransform.split('(');
		var index = $tab_DomUtil.$translationFuncIndexer[transform[0]];
		if (ss.isNullOrUndefined(index)) {
			return { left: 0, top: 0 };
		}
		var vals = transform[1].split(',');
		return { left: ss.coalesce($tab_DoubleUtil.parseDouble(vals[ss.unbox(index)]), 0), top: ss.coalesce($tab_DoubleUtil.parseDouble(vals[ss.unbox(index) + 1]), 0) };
	};
	$tab_DomUtil.getTransformScale = function DomUtil$GetTransformScale(element) {
		if (ss.isNullOrUndefined(element)) {
			return 1;
		}
		var fullTransform = element.css('transform');
		if (ss.isNullOrEmptyString(fullTransform)) {
			return 1;
		}
		var transform = fullTransform.split('(');
		if (transform[0] === 'scale' || transform[0] === 'matrix' || transform[0] === 'matrix3d') {
			return ss.coalesce($tab_DoubleUtil.parseDouble(transform[1]), 1);
		}
		else {
			return 1;
		}
	};
	$tab_DomUtil.getPageOffset = function DomUtil$GetPageOffset(e) {
		var r = e[0].getBoundingClientRect();
		if ($tab_BrowserSupport.get_isChrome() && tsConfig.is_mobile && window.innerWidth / window.outerWidth <= 0.9) {
			return { left: r.left, top: r.top };
		}
		else {
			return { left: r.left + window.pageXOffset, top: r.top + window.pageYOffset };
		}
	};
	$tab_DomUtil.roomAroundPosition = function DomUtil$RoomAroundPosition(p) {
		var roomAbove = p.top - window.pageYOffset;
		var roomBelow = window.pageYOffset + window.innerHeight - p.top;
		var roomLeft = p.left - window.pageXOffset;
		var roomRight = window.pageXOffset + window.innerWidth - p.left;
		return { roomAbove: roomAbove, roomBelow: roomBelow, roomLeft: roomLeft, roomRight: roomRight };
	};
	$tab_DomUtil.getElementRelativePosition = function DomUtil$GetElementRelativePosition(e, p) {
		if (ss.isNullOrUndefined(p)) {
			p = e.parent();
		}
		var ep = e.offset();
		var pp = p.offset();
		return { x: $tab_DoubleUtil.roundToInt(ep.left) - $tab_DoubleUtil.roundToInt(pp.left), y: $tab_DoubleUtil.roundToInt(ep.top) - $tab_DoubleUtil.roundToInt(pp.top) };
	};
	$tab_DomUtil.parseWidthFromStyle = function DomUtil$ParseWidthFromStyle(style) {
		if (ss.isValue(style) && !$tab_MiscUtil.isNullOrEmpty$1(style.width)) {
			return parseInt(style.width);
		}
		return Number.NaN;
	};
	$tab_DomUtil.parseHeightFromStyle = function DomUtil$ParseHeightFromStyle(style) {
		if (ss.isValue(style) && !$tab_MiscUtil.isNullOrEmpty$1(style.height)) {
			return parseInt(style.height);
		}
		return Number.NaN;
	};
	$tab_DomUtil.createNamespacedEventName = function DomUtil$CreateNamespacedEventName(eventName, eventNamespace) {
		if (ss.isValue(eventNamespace)) {
			return eventName + eventNamespace;
		}
		return eventName.toString();
	};
	$tab_DomUtil.stopPropagationOfInputEvents = function DomUtil$StopPropagationOfInputEvents(o, eventNamespace) {
		var stopPropagation = function(e) {
			e.stopPropagation();
		};
		$tab_DomUtil.handleInputEvents(o, eventNamespace, stopPropagation);
	};
	$tab_DomUtil.handleInputEvents = function DomUtil$HandleInputEvents(o, eventNamespace, handler) {
		o.on($tab_DomUtil.createNamespacedEventName('touchstart', eventNamespace), handler).on($tab_DomUtil.createNamespacedEventName('touchcancel', eventNamespace), handler).on($tab_DomUtil.createNamespacedEventName('touchend', eventNamespace), handler).on($tab_DomUtil.createNamespacedEventName('touchmove', eventNamespace), handler).on($tab_DomUtil.createNamespacedEventName('click', eventNamespace), handler).on($tab_DomUtil.createNamespacedEventName('mousedown', eventNamespace), handler).on($tab_DomUtil.createNamespacedEventName('mousemove', eventNamespace), handler).on($tab_DomUtil.createNamespacedEventName('mouseup', eventNamespace), handler);
	};
	$tab_DomUtil.isFocusableTextElement = function DomUtil$IsFocusableTextElement(domElement) {
		if (ss.isValue(domElement) && ss.isValue(domElement.tagName)) {
			var targetTagName = domElement.tagName.toLowerCase();
			if (targetTagName === 'textarea' || targetTagName === 'input' || targetTagName === 'select') {
				return true;
			}
		}
		return false;
	};
	$tab_DomUtil.isCheckboxElement = function DomUtil$IsCheckboxElement(domElement) {
		if (ss.isValue(domElement) && ss.isValue(domElement.tagName)) {
			var targetTagName = domElement.tagName.toLowerCase();
			var typeAttributeValue = $(domElement).attr('type');
			if (targetTagName === 'input' && typeAttributeValue === 'checkbox') {
				return true;
			}
		}
		return false;
	};
	$tab_DomUtil.handleTouchEvents = function DomUtil$HandleTouchEvents(domElement) {
		if ($tab_DomUtil.isCheckboxElement(domElement)) {
			return false;
		}
		if ($tab_DomUtil.isFocusableTextElement(domElement)) {
			return false;
		}
		return true;
	};
	$tab_DomUtil.setCapture = function DomUtil$SetCapture(e, retargetToElement) {
		if (!$tab_BrowserSupport.get_mouseCapture()) {
			return;
		}
		e.setCapture(retargetToElement);
	};
	$tab_DomUtil.releaseCapture = function DomUtil$ReleaseCapture() {
		if (!$tab_BrowserSupport.get_mouseCapture()) {
			return;
		}
		document.releaseCapture();
	};
	$tab_DomUtil.blur = function DomUtil$Blur() {
		var activeElem = document.activeElement;
		if (ss.isValue(activeElem) && !ss.referenceEquals(activeElem, $tab_DomUtil.get_documentBody())) {
			activeElem.blur();
		}
	};
	$tab_DomUtil.$convertCssToInt = function DomUtil$ConvertCssToInt(o, css, defaultValue) {
		var x = parseInt(o.css(css), 10);
		return (isNaN(x) ? defaultValue : x);
	};
	$tab_DomUtil.$setOuterWidth = function DomUtil$SetOuterWidth(o, outerWidth) {
		var marginLeft = $tab_DomUtil.$convertCssToInt(o, 'margin-left', 0);
		var borderLeft = $tab_DomUtil.$convertCssToInt(o, 'border-left-width', 0);
		var paddingLeft = $tab_DomUtil.$convertCssToInt(o, 'padding-left', 0);
		var paddingRight = $tab_DomUtil.$convertCssToInt(o, 'padding-right', 0);
		var borderRight = $tab_DomUtil.$convertCssToInt(o, 'border-right-width', 0);
		var marginRight = $tab_DomUtil.$convertCssToInt(o, 'margin-right', 0);
		var newVal = Math.max(outerWidth - marginLeft - borderLeft - paddingLeft - paddingRight - borderRight - marginRight, 0);
		o.width(newVal);
	};
	$tab_DomUtil.$setOuterHeight = function DomUtil$SetOuterHeight(o, outerHeight) {
		var marginTop = $tab_DomUtil.$convertCssToInt(o, 'margin-top', 0);
		var borderTop = $tab_DomUtil.$convertCssToInt(o, 'border-top-width', 0);
		var paddingTop = $tab_DomUtil.$convertCssToInt(o, 'padding-top', 0);
		var paddingBottom = $tab_DomUtil.$convertCssToInt(o, 'padding-bottom', 0);
		var borderBottom = $tab_DomUtil.$convertCssToInt(o, 'border-bottom-width', 0);
		var marginBottom = $tab_DomUtil.$convertCssToInt(o, 'margin-bottom', 0);
		var newVal = Math.max(outerHeight - marginTop - borderTop - paddingTop - paddingBottom - borderBottom - marginBottom, 0);
		o.height(newVal);
	};
	$tab_DomUtil.$setMarginSizeJQ = function DomUtil$SetMarginSizeJQ(o, s) {
		if (s.w >= 0) {
			$tab_DomUtil.$setOuterWidth(o, s.w);
		}
		if (s.h >= 0) {
			$tab_DomUtil.$setOuterHeight(o, s.h);
		}
	};
	$tab_DomUtil.$parseZIndexProperty = function DomUtil$ParseZIndexProperty(o) {
		$tab_Param.verifyValue(o, 'o');
		var zindexProperty = o.css('z-index');
		if (_.isNumber(zindexProperty)) {
			return zindexProperty;
		}
		if (_.isString(zindexProperty)) {
			if (!ss.isNullOrEmptyString(zindexProperty) && zindexProperty !== 'auto' && zindexProperty !== 'inherits') {
				return parseInt(zindexProperty, 10);
			}
		}
		return 0;
	};
	$tab_DomUtil.makeHtmlSafeId = function DomUtil$MakeHtmlSafeId(value) {
		return ss.replaceAllString(encodeURIComponent(value), '.', 'dot');
	};
	$tab_DomUtil.setSelectionRangeOnInput = function DomUtil$SetSelectionRangeOnInput(inputElement, selectionStart, selectionEnd) {
		if ($tab_BrowserSupport.get_setSelectionRange()) {
			try {
				inputElement.setSelectionRange(selectionStart, selectionEnd);
			}
			catch ($t1) {
			}
		}
	};
	$tab_DomUtil.selectAllInputText = function DomUtil$SelectAllInputText(inputElement) {
		try {
			if ($tab_BrowserSupport.get_setSelectionRange()) {
				inputElement.get(0).setSelectionRange(0, inputElement.val().length);
			}
			else {
				inputElement.select();
			}
		}
		catch ($t1) {
		}
	};
	$tab_DomUtil.setNativeTooltip = function DomUtil$SetNativeTooltip(obj, tooltipText) {
		var empty = ss.isNullOrEmptyString(tooltipText);
		if (empty) {
			obj.removeAttr('title');
		}
		else {
			obj.attr('title', tooltipText);
		}
		if (tsConfig.is_mobile) {
			obj.children('.tab-mobileTooltip').remove();
			if (!empty) {
				var tooltipDiv = $("<div class='tab-mobileTooltip'/>").text(tooltipText);
				obj.append(tooltipDiv);
			}
		}
	};
	global.tab.DomUtil = $tab_DomUtil;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.DoubleUtil
	var $tab_DoubleUtil = function() {
	};
	$tab_DoubleUtil.__typeName = 'tab.DoubleUtil';
	$tab_DoubleUtil.get_$log = function DoubleUtil$get_Log() {
		return $tab_Logger.lazyGetLogger($tab_DoubleUtil);
	};
	$tab_DoubleUtil.isApproximatelyEqual = function DoubleUtil$IsApproximatelyEqual(d1, d2) {
		if (Math.abs(d1 - d2) < $tab_DoubleUtil.epsilon) {
			return true;
		}
		if (d1 === 0) {
			return false;
		}
		var intermediate = d2 / d1;
		if ($tab_DoubleUtil.$lowerBound <= intermediate && intermediate <= $tab_DoubleUtil.$upperBound) {
			return true;
		}
		return false;
	};
	$tab_DoubleUtil.isApproximatelyZero = function DoubleUtil$IsApproximatelyZero(d) {
		return $tab_DoubleUtil.isApproximatelyEqual(0, d);
	};
	$tab_DoubleUtil.isLessThanAndNotApproximatelyEqual = function DoubleUtil$IsLessThanAndNotApproximatelyEqual(d1, d2) {
		return d1 < d2 && !$tab_DoubleUtil.isApproximatelyEqual(d1, d2);
	};
	$tab_DoubleUtil.isLessThanOrApproximatelyEqual = function DoubleUtil$IsLessThanOrApproximatelyEqual(d1, d2) {
		return d1 < d2 || $tab_DoubleUtil.isApproximatelyEqual(d1, d2);
	};
	$tab_DoubleUtil.isGreaterThanAndNotApproximatelyEqual = function DoubleUtil$IsGreaterThanAndNotApproximatelyEqual(d1, d2) {
		return d1 > d2 && !$tab_DoubleUtil.isApproximatelyEqual(d1, d2);
	};
	$tab_DoubleUtil.isGreaterThanOrApproximatelyEqual = function DoubleUtil$IsGreaterThanOrApproximatelyEqual(d1, d2) {
		return d1 > d2 || $tab_DoubleUtil.isApproximatelyEqual(d1, d2);
	};
	$tab_DoubleUtil.sigFigs = function DoubleUtil$SigFigs(n, numSigFigs) {
		if (n === 0 || numSigFigs === 0) {
			return n;
		}
		var mult = Math.pow(10, numSigFigs - Math.floor(Math.log(Math.abs(n)) / Math.LN10) - 1);
		return ss.round(n * mult) / mult;
	};
	$tab_DoubleUtil.roundToInt = function DoubleUtil$RoundToInt(value) {
		return ss.round(value);
	};
	$tab_DoubleUtil.parseDouble = function DoubleUtil$ParseDouble(s) {
		var val = parseFloat(s);
		return (isFinite(val) ? val : null);
	};
	global.tab.DoubleUtil = $tab_DoubleUtil;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Bootstrap.EmbedMode
	var $tab_EmbedMode = function() {
	};
	$tab_EmbedMode.__typeName = 'tab.EmbedMode';
	global.tab.EmbedMode = $tab_EmbedMode;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.ErrorTrace
	var $tab_ErrorTrace = function() {
	};
	$tab_ErrorTrace.__typeName = 'tab.ErrorTrace';
	$tab_ErrorTrace.install = function ErrorTrace$Install() {
		var enabled = tsConfig.clientErrorReportingLevel;
		if (!ss.isNullOrEmptyString(enabled)) {
			if (enabled === 'debug') {
				$tab_ErrorTrace.$getStack = true;
			}
		}
		$tab_ErrorTrace.$extendToAsynchronousCallback('setTimeout');
		$tab_ErrorTrace.$extendToAsynchronousCallback('setInterval');
		$tab_ErrorTrace.installGlobalHandler();
	};
	$tab_ErrorTrace.wrap = function ErrorTrace$Wrap(func) {
		return function() {
			try {
				return func.apply(this, [Array.prototype.slice.call(arguments)]);
			}
			catch ($t1) {
				var e = ss.Exception.wrap($t1);
				$tab_ErrorTrace.report(e, false);
				throw $t1;
			}
		};
	};
	$tab_ErrorTrace.$extendToAsynchronousCallback = function ErrorTrace$ExtendToAsynchronousCallback(functionName) {
		var originalFunction = window[functionName];
		var callback = function() {
			var args = ss.arrayClone(Array.prototype.slice.call(arguments));
			var originalCallback = args[0];
			if (ss.referenceEquals(ss.getInstanceType(originalCallback), Function)) {
				args[0] = $tab_ErrorTrace.wrap(originalCallback);
			}
			if ('apply' in originalFunction) {
				return originalFunction.apply(this, args);
			}
			else {
				return originalFunction(args[0], args[1]);
			}
		};
		window[functionName] = callback;
	};
	$tab_ErrorTrace.windowOnError = function ErrorTrace$WindowOnError(message, url, lineNo, column, error) {
		var stack;
		if (ss.isValue($tab_ErrorTrace.$lastExceptionStack)) {
			$tab_ErrorTrace.$augmentStackTraceWithInitialElement($tab_ErrorTrace.$lastExceptionStack, url, lineNo, message);
			stack = $tab_ErrorTrace.$lastExceptionStack;
			$tab_ErrorTrace.$lastExceptionStack = null;
			$tab_ErrorTrace.$lastException = null;
		}
		else {
			var location = new $tab_StackLocation(url, lineNo);
			location.functionName = $tab_ErrorTrace.$guessFunctionName(location);
			location.context = $tab_ErrorTrace.$gatherContext(location);
			stack = new $tab_StackTrace('onError', message);
			stack.name = 'window.onError';
			stack.locations = [location];
		}
		$tab_ErrorTrace.$queuedTraces.push(stack);
		if (ss.isValue($tab_ErrorTrace.$oldOnErrorHandler)) {
			$tab_ErrorTrace.$oldOnErrorHandler.apply(this, Array.prototype.slice.call(arguments));
		}
		return false;
	};
	$tab_ErrorTrace.$augmentStackTraceWithInitialElement = function ErrorTrace$AugmentStackTraceWithInitialElement(stack, url, lineNo, message) {
		var initial = new $tab_StackLocation(url, lineNo);
		if (ss.isValue(initial.url) && ss.isValue(initial.lineNo)) {
			stack.isIncomplete = false;
			if (ss.isNullOrUndefined(initial.functionName)) {
				initial.functionName = $tab_ErrorTrace.$guessFunctionName(initial);
			}
			if (ss.isNullOrUndefined(initial.context)) {
				initial.context = $tab_ErrorTrace.$gatherContext(initial);
			}
			var reference = message.match(new RegExp(" '([^']+)' "));
			if (ss.isValue(reference) && reference.length > 1) {
				initial.columnNo = $tab_ErrorTrace.$findSourceInLine(reference[1], initial);
			}
			if (ss.isValue(stack.locations) && stack.locations.length > 0) {
				var top = stack.locations[0];
				if (ss.referenceEquals(top.url, initial.url)) {
					if (top.lineNo === initial.lineNo) {
						return false;
					}
					else if (ss.isNullOrUndefined(top.lineNo) && ss.referenceEquals(top.functionName, initial.functionName)) {
						top.lineNo = initial.lineNo;
						top.context = initial.context;
						return false;
					}
				}
			}
			stack.locations.unshift(initial);
			stack.isPartial = true;
			return true;
		}
		else {
			stack.isIncomplete = true;
		}
		return false;
	};
	$tab_ErrorTrace.$loadSource = function ErrorTrace$LoadSource(url) {
		if (!$tab_ErrorTrace.$remoteFetching) {
			return '';
		}
		try {
			var srcRequest = new XMLHttpRequest();
			srcRequest.open('GET', url, false);
			srcRequest.send('');
			return srcRequest.responseText;
		}
		catch ($t1) {
			return '';
		}
	};
	$tab_ErrorTrace.$getSource = function ErrorTrace$GetSource(url) {
		if (ss.isNullOrUndefined(url)) {
			return [];
		}
		if (!ss.keyExists($tab_ErrorTrace.$sourceCache, url)) {
			var source = '';
			if (url.indexOf(document.domain) > -1) {
				source = $tab_ErrorTrace.$loadSource(url);
			}
			$tab_ErrorTrace.$sourceCache[url] = (ss.isNullOrEmptyString(source) ? [] : source.split('\n'));
		}
		return $tab_ErrorTrace.$sourceCache[url];
	};
	$tab_ErrorTrace.$findSourceInLine = function ErrorTrace$FindSourceInLine(fragment, location) {
		var source = $tab_ErrorTrace.$getSource(location.url);
		var re = new RegExp('\\b' + $tab_ErrorTrace.$escapeRegexp(fragment) + '\\b');
		if (ss.isValue(source) && source.length > location.lineNo) {
			var matches = re.exec(source[location.lineNo]);
			if (ss.isValue(matches)) {
				return matches.index;
			}
		}
		return -1;
	};
	$tab_ErrorTrace.$guessFunctionName = function ErrorTrace$GuessFunctionName(location) {
		var functionArgNames = new RegExp('function ([^(]*)\\(([^)]*)\\)');
		var guessFunction = new RegExp('[\'"]?([0-9A-Za-z$_]+)[\'"]?\\s*[:=]\\s*(function|eval|new Function)');
		var line = '';
		var maxLines = 10;
		var source = $tab_ErrorTrace.$getSource(location.url);
		if (source.length === 0) {
			return $tab_ErrorTrace.$unknownFunctionName;
		}
		for (var i = 0; i < maxLines; i++) {
			line = source[location.lineNo - 1] + line;
			if (!ss.isNullOrEmptyString(line)) {
				var matches = guessFunction.exec(line);
				if (ss.isValue(matches) && matches.length > 0) {
					return matches[1];
				}
				matches = functionArgNames.exec(line);
				if (ss.isValue(matches) && matches.length > 0) {
					return matches[1];
				}
			}
		}
		return $tab_ErrorTrace.$unknownFunctionName;
	};
	$tab_ErrorTrace.$gatherContext = function ErrorTrace$GatherContext(location) {
		var source = $tab_ErrorTrace.$getSource(location.url);
		if (ss.isNullOrUndefined(source) || source.length === 0) {
			return null;
		}
		var context = [];
		var linesBefore = Math.floor($tab_ErrorTrace.$linesOfContext / 2);
		var linesAfter = linesBefore + $tab_ErrorTrace.$linesOfContext % 2;
		var start = Math.max(0, location.lineNo - linesBefore - 1);
		var end = Math.min(source.length, location.lineNo + linesAfter - 1);
		location.lineNo -= 1;
		for (var i = start; i < end; i++) {
			if (!ss.isNullOrEmptyString(source[i])) {
				context.push(source[i]);
			}
		}
		return context;
	};
	$tab_ErrorTrace.$escapeRegexp = function ErrorTrace$EscapeRegexp(input) {
		return input.replace(new RegExp('[\\-\\[\\]{}()*+?.,\\\\\\^$|#]', 'g'), '\\\\$&');
	};
	$tab_ErrorTrace.$escapeCodeAsRegexpForMatchingInsideHTML = function ErrorTrace$EscapeCodeAsRegexpForMatchingInsideHTML(body) {
		return ss.replaceAllString(ss.replaceAllString(ss.replaceAllString(ss.replaceAllString($tab_ErrorTrace.$escapeRegexp(body), '<', '(?:<|&lt;)'), '>', '(?:>|&gt;)'), '&', '(?:&|&amp;)'), '"', '(?:"|&quot;)').replace(new RegExp('\\\\s+', 'g'), '\\\\s+');
	};
	$tab_ErrorTrace.$findSourceInUrls = function ErrorTrace$FindSourceInUrls(re, urls) {
		for (var $t1 = 0; $t1 < urls.length; $t1++) {
			var url = urls[$t1];
			var source = $tab_ErrorTrace.$getSource(url);
			if (ss.isValue(source) && source.length > 0) {
				for (var lineNo = 0; lineNo < source.length; lineNo++) {
					var matches = re.exec(source[lineNo]);
					if (ss.isValue(matches) && matches.length > 0) {
						var location = new $tab_StackLocation(url, lineNo);
						location.columnNo = matches.index;
						return location;
					}
				}
			}
		}
		return null;
	};
	$tab_ErrorTrace.$getStackTraceFor = function ErrorTrace$GetStackTraceFor(e) {
		var defaultTrace = new $tab_StackTrace('stack', e.get_message());
		defaultTrace.name = e.name;
		if ($tab_ErrorTrace.$getStack) {
			var stackTraceComputers = [];
			stackTraceComputers.push($tab_ErrorTrace.$computeStackTraceFromStackTraceProp);
			stackTraceComputers.push($tab_ErrorTrace.computeStackTraceByWalkingCallerChain);
			for (var $t1 = 0; $t1 < stackTraceComputers.length; $t1++) {
				var stackTraceComputer = stackTraceComputers[$t1];
				var stack = null;
				try {
					stack = stackTraceComputer(e);
				}
				catch ($t2) {
					var inner = ss.Exception.wrap($t2);
					if ($tab_ErrorTrace.$shouldReThrow) {
						throw inner;
					}
				}
				if (ss.isValue(stack)) {
					return stack;
				}
			}
		}
		else {
			return defaultTrace;
		}
		defaultTrace.traceMode = 'failed';
		return defaultTrace;
	};
	$tab_ErrorTrace.computeStackTraceByWalkingCallerChain = function ErrorTrace$ComputeStackTraceByWalkingCallerChain(e) {
		var err = e._error;
		var functionName = new RegExp('function\\s+([_$a-zA-Z -￿][_$a-zA-Z0-9 -￿]*)?\\s*\\(', 'i');
		var locations = [];
		var funcs = {};
		var recursion = false;
		var curr = null;
		for (curr = $tab_ErrorTrace.computeStackTraceByWalkingCallerChain.caller; ss.isValue(curr) && !recursion; curr = curr.caller) {
			if (ss.referenceEquals(curr, $tab_ErrorTrace)) {
				continue;
			}
			var functionText = curr.toString();
			var item = new $tab_StackLocation(null, 0);
			if (ss.isValue(curr.name)) {
				item.functionName = curr.name;
			}
			else {
				var parts = functionName.exec(functionText);
				if (ss.isValue(parts) && parts.length > 1) {
					item.functionName = parts[1];
				}
			}
			var source = $tab_ErrorTrace.$findSourceByFunctionBody(curr);
			if (ss.isValue(source)) {
				item.url = source.url;
				item.lineNo = source.lineNo;
				if (ss.referenceEquals(item.functionName, $tab_ErrorTrace.$unknownFunctionName)) {
					item.functionName = $tab_ErrorTrace.$guessFunctionName(item);
				}
				var reference = (new RegExp(" '([^']+)' ")).exec(e.get_message() || e['description']);
				if (ss.isValue(reference) && reference.length > 1) {
					item.columnNo = $tab_ErrorTrace.$findSourceInLine(reference[1], source);
				}
			}
			if (ss.keyExists(funcs, functionText)) {
				recursion = true;
			}
			else {
				funcs[functionText] = true;
			}
			locations.push(item);
		}
		var result = new $tab_StackTrace('callers', e.get_message());
		result.name = err['name'];
		result.locations = locations;
		$tab_ErrorTrace.$augmentStackTraceWithInitialElement(result, err['sourceURL'] || err['fileName'], err['line'] || err['lineNumber'], e.get_message() || err['description']);
		return result;
	};
	$tab_ErrorTrace.$findSourceByFunctionBody = function ErrorTrace$FindSourceByFunctionBody(func) {
		var urls = [window.location.href];
		var scripts = document.getElementsByTagName('script');
		var code = func.toString();
		var codeMatcher = new RegExp('');
		var matcher;
		for (var i = 0; i < scripts.length; i++) {
			var script = scripts[i];
			if (script.hasAttribute('src') && ss.isValue(script.getAttribute('src'))) {
				urls.push(script.getAttribute('src'));
			}
		}
		var parts = codeMatcher.exec(code);
		if (ss.isValue(parts) && parts.length > 0) {
			matcher = new RegExp($tab_ErrorTrace.$escapeRegexp(code).replace(new RegExp('\\s+', 'g'), '\\\\s+'));
		}
		else {
			var name = ((parts.length > 1) ? ('\\\\s+' + parts[1]) : '');
			var args = parts[2].split(',').join('\\\\s*,\\\\s*');
			var body = $tab_ErrorTrace.$escapeRegexp(parts[3]).replace(new RegExp(';$'), ';?');
			matcher = new RegExp('function' + name + '\\\\s*\\\\(\\\\s*' + args + '\\\\s*\\\\)\\\\s*{\\\\s*' + body + '\\\\s*}');
		}
		var result = $tab_ErrorTrace.$findSourceInUrls(matcher, urls);
		if (ss.isValue(result)) {
			return result;
		}
		return null;
	};
	$tab_ErrorTrace.$computeStackTraceFromStackTraceProp = function ErrorTrace$ComputeStackTraceFromStackTraceProp(e) {
		var err = e._error;
		if (ss.isNullOrUndefined(err) || ss.isNullOrUndefined(err.stack)) {
			return null;
		}
		var chromeMatcher = new RegExp('^\\s*at (?:((?:\\[object object\\])?\\S+(?: \\[as \\S+\\])?) )?\\(?((?:file|http|https):.*?):(\\d+)(?::(\\d+))?\\)?\\s*$', 'i');
		var geckoMatcher = new RegExp('^\\s*(\\S*)(?:\\((.*?)\\))?@((?:file|http|https).*?):(\\d+)(?::(\\d+))?\\s*$', 'i');
		var matcher = ($tab_BrowserSupport.get_isFF() ? geckoMatcher : chromeMatcher);
		var lines = err['stack'].split('\n');
		var locations = [];
		var reference = (new RegExp('^(.*) is undefined')).exec(e.get_message());
		for (var $t1 = 0; $t1 < lines.length; $t1++) {
			var line = lines[$t1];
			var parts = matcher.exec(line);
			if (ss.isValue(parts) && parts.length >= 5) {
				var functionName = parts[1];
				var url = parts[2];
				var lineNumStr = parts[3];
				var colNumStr = parts[4];
				var element = new $tab_StackLocation(url, parseInt(lineNumStr));
				if (ss.isValue(functionName)) {
					element.functionName = functionName;
				}
				if (ss.isValue(colNumStr)) {
					element.columnNo = parseInt(colNumStr);
				}
				if (ss.isValue(element.lineNo)) {
					if (ss.isNullOrUndefined(element.functionName)) {
						element.functionName = $tab_ErrorTrace.$guessFunctionName(element);
					}
					element.context = $tab_ErrorTrace.$gatherContext(element);
				}
				locations.push(element);
			}
		}
		if (locations.length > 0 && ss.isValue(locations[0].lineNo) && ss.isNullOrUndefined(locations[0].columnNo) && ss.isValue(reference) && reference.length > 1) {
			locations[0].columnNo = $tab_ErrorTrace.$findSourceInLine(reference[1], locations[0]);
		}
		if (locations.length === 0) {
			return null;
		}
		var stack = new $tab_StackTrace('stack', e.get_message());
		stack.name = e.name;
		stack.locations = locations;
		return stack;
	};
	$tab_ErrorTrace.hasTraces = function ErrorTrace$HasTraces() {
		return $tab_ErrorTrace.$queuedTraces.length > 0;
	};
	$tab_ErrorTrace.dequeueTraces = function ErrorTrace$DequeueTraces() {
		var traces = $tab_ErrorTrace.$queuedTraces;
		$tab_ErrorTrace.$queuedTraces = [];
		return traces;
	};
	$tab_ErrorTrace.installGlobalHandler = function ErrorTrace$InstallGlobalHandler() {
		if ($tab_ErrorTrace.$onErrorHandlerInstalled || !$tab_ErrorTrace.$collectWindowErrors) {
			return;
		}
		$tab_ErrorTrace.$oldOnErrorHandler = window.onerror;
		window.onerror = $tab_ErrorTrace.windowOnError;
		$tab_ErrorTrace.$onErrorHandlerInstalled = true;
	};
	$tab_ErrorTrace.report = function ErrorTrace$Report(e, rethrow) {
		if (ss.isNullOrUndefined(rethrow)) {
			rethrow = true;
		}
		if (ss.isValue($tab_ErrorTrace.$lastExceptionStack)) {
			if (ss.referenceEquals($tab_ErrorTrace.$lastException, e)) {
				return;
			}
			else {
				var s = $tab_ErrorTrace.$lastExceptionStack;
				$tab_ErrorTrace.$lastExceptionStack = null;
				$tab_ErrorTrace.$lastException = null;
				$tab_ErrorTrace.$queuedTraces.push(s);
			}
		}
		var stack = $tab_ErrorTrace.$getStackTraceFor(e);
		$tab_ErrorTrace.$lastExceptionStack = stack;
		$tab_ErrorTrace.$lastException = e;
		window.setTimeout(function() {
			if (ss.referenceEquals($tab_ErrorTrace.$lastException, e)) {
				$tab_ErrorTrace.$lastExceptionStack = null;
				$tab_ErrorTrace.$lastException = null;
				$tab_ErrorTrace.$queuedTraces.push(stack);
			}
		}, (stack.isIncomplete ? 2000 : 0));
		if (rethrow) {
			throw e;
		}
	};
	global.tab.ErrorTrace = $tab_ErrorTrace;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.IBrowserViewport
	var $tab_IBrowserViewport = function() {
	};
	$tab_IBrowserViewport.__typeName = 'tab.IBrowserViewport';
	global.tab.IBrowserViewport = $tab_IBrowserViewport;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.ILogAppender
	var $tab_ILogAppender = function() {
	};
	$tab_ILogAppender.__typeName = 'tab.ILogAppender';
	global.tab.ILogAppender = $tab_ILogAppender;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Bootstrap.LayoutMetrics
	var $tab_LayoutMetrics = function(scrollbarSize) {
		this.$cloneDefaultDb();
		if (ss.isNullOrUndefined(scrollbarSize)) {
			var size = $tab_LayoutMetrics.getScrollbarSize();
			scrollbarSize = { w: size, h: size };
		}
		this['scrollbar'] = { w: scrollbarSize.w, h: scrollbarSize.h };
	};
	$tab_LayoutMetrics.__typeName = 'tab.LayoutMetrics';
	$tab_LayoutMetrics.clone = function LayoutMetrics$Clone(other) {
		var obj = new $tab_LayoutMetrics(null);
		var $t1 = new ss.ObjectEnumerator($tab_LayoutMetrics.$defaultDb);
		try {
			while ($t1.moveNext()) {
				var entry = $t1.current();
				var v = other[entry.key];
				if (ss.isValue(v)) {
					obj[entry.key] = v;
				}
			}
		}
		finally {
			$t1.dispose();
		}
		return obj;
	};
	$tab_LayoutMetrics.getScrollbarSize = function LayoutMetrics$GetScrollbarSize() {
		if (tsConfig.is_mobile) {
			return 0;
		}
		var outer = document.createElement('div');
		var style = outer.style;
		style.width = '100px';
		style.height = '100px';
		style.overflow = 'scroll';
		style.position = 'absolute';
		style.top = '0px';
		style.filter = 'alpha(opacity=0)';
		style.opacity = '0';
		style.left = '0px';
		var inner = document.createElement('div');
		inner.style.width = '400px';
		inner.style.height = '400px';
		outer.appendChild(inner);
		document.body.appendChild(outer);
		var width = outer.offsetWidth - outer.clientWidth;
		document.body.removeChild(outer);
		outer.removeChild(inner);
		outer = inner = null;
		width = ((width > 0) ? width : 9);
		return width;
	};
	global.tab.LayoutMetrics = $tab_LayoutMetrics;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.Log
	var $tab_Log = function() {
	};
	$tab_Log.__typeName = 'tab.Log';
	$tab_Log.get = function Log$Get(o) {
		return $tab_Logger.lazyGetLogger(ss.getInstanceType(o));
	};
	global.tab.Log = $tab_Log;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.Logger
	var $tab_Logger = function(name) {
		this.$name = null;
		this.$name = name;
	};
	$tab_Logger.__typeName = 'tab.Logger';
	$tab_Logger.get_globalLog = function Logger$get_GlobalLog() {
		return $tab_Logger.global;
	};
	$tab_Logger.get_$appenders = function Logger$get_Appenders() {
		return $tab_MiscUtil.lazyInitStaticField($tab_Logger, 'appenders', function() {
			return [];
		});
	};
	$tab_Logger.get_$filters = function Logger$get_Filters() {
		return $tab_MiscUtil.lazyInitStaticField($tab_Logger, 'filters', function() {
			return [];
		});
	};
	$tab_Logger.get_$nullLog = function Logger$get_NullLog() {
		return $tab_MiscUtil.lazyInitStaticField($tab_Logger, 'nullLog', function() {
			return new $tab_Logger('');
		});
	};
	$tab_Logger.clearFilters = function Logger$ClearFilters() {
		var $t1 = $tab_Logger.get_$appenders();
		for (var $t2 = 0; $t2 < $t1.length; $t2++) {
			var logAppender = $t1[$t2];
			logAppender.clearFilters();
		}
		ss.clear($tab_Logger.get_$filters());
	};
	$tab_Logger.filterByLogger = function Logger$FilterByLogger(validLogger, minLogLevel) {
		minLogLevel = minLogLevel || 0;
		$tab_Logger.$addFilter(function(l, ll) {
			return ss.referenceEquals(l, validLogger) && ll >= minLogLevel;
		});
	};
	$tab_Logger.filterByType = function Logger$FilterByType(t, minLogLevel) {
		minLogLevel = minLogLevel || 0;
		$tab_Logger.$addFilter(function(l, ll) {
			return ll >= minLogLevel && ss.referenceEquals(l.get_name(), ss.getTypeName(t));
		});
	};
	$tab_Logger.filterByName = function Logger$FilterByName(namePattern, minLogLevel) {
		minLogLevel = minLogLevel || 0;
		var regex = new RegExp(namePattern, 'i');
		$tab_Logger.$addFilter(function(l, ll) {
			return ll >= minLogLevel && ss.isValue(l.get_name().match(regex));
		});
	};
	$tab_Logger.clearAppenders = function Logger$ClearAppenders() {
		ss.clear($tab_Logger.get_$appenders());
	};
	$tab_Logger.addAppender = function Logger$AddAppender(appender) {
		var $t1 = $tab_Logger.get_$filters();
		for (var $t2 = 0; $t2 < $t1.length; $t2++) {
			var filter = $t1[$t2];
			appender.addFilter(filter);
		}
		$tab_Logger.get_$appenders().push(appender);
	};
	$tab_Logger.removeAppender = function Logger$RemoveAppender(appender) {
		ss.remove($tab_Logger.get_$appenders(), appender);
	};
	$tab_Logger.lazyGetLogger = function Logger$LazyGetLogger(t) {
		return $tab_MiscUtil.lazyInitStaticField(t, '_logger', function() {
			return $tab_Logger.getLogger(t);
		});
	};
	$tab_Logger.getLogger = function Logger$GetLogger(t, ll) {
		var l = $tab_Logger.getLoggerWithName(ss.getTypeName(t));
		if (ss.isValue(ll)) {
		}
		return l;
	};
	$tab_Logger.getLoggerWithName = function Logger$GetLoggerWithName(name) {
		return $tab_Logger.get_$nullLog();
	};
	$tab_Logger.$setupUrlFilters = function Logger$SetupUrlFilters() {
		var queryParams = $tab_MiscUtil.getUriQueryParameters(window.self.location.search);
		if (!ss.keyExists(queryParams, $tab_Logger.$logQueryParam)) {
			return;
		}
		var logParams = queryParams[$tab_Logger.$logQueryParam];
		if (logParams.length === 0) {
		}
		for (var $t1 = 0; $t1 < logParams.length; $t1++) {
			var logParam = logParams[$t1];
			var logVals = logParam.split(String.fromCharCode(58));
			var level = 1;
			if (logVals.length > 0 && ss.isValue(logVals[1])) {
				var key = logVals[1].toLowerCase();
				var index = ss.indexOf($tab_Logger.loggerLevelNames, key);
				if (index >= 0) {
					level = index;
				}
			}
		}
	};
	$tab_Logger.$addFilter = function Logger$AddFilter(filterFunc) {
		$tab_Logger.get_$filters().push(filterFunc);
		var $t1 = $tab_Logger.get_$appenders();
		for (var $t2 = 0; $t2 < $t1.length; $t2++) {
			var logAppender = $t1[$t2];
			logAppender.addFilter(filterFunc);
		}
	};
	global.tab.Logger = $tab_Logger;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.LoggerLevel
	var $tab_LoggerLevel = function() {
	};
	$tab_LoggerLevel.__typeName = 'tab.LoggerLevel';
	global.tab.LoggerLevel = $tab_LoggerLevel;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Bootstrap.Metric
	var $tab_Metric = function() {
	};
	$tab_Metric.__typeName = 'tab.Metric';
	global.tab.Metric = $tab_Metric;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Bootstrap.MetricsContext
	var $tab_MetricsContext = function(contextID, suite, desc, props) {
		this.id = 0;
		this.metricSuite = 0;
		this.description = null;
		this.$propBag = null;
		this.start = 0;
		this.end = 0;
		this.open = false;
		this.id = contextID;
		this.metricSuite = suite;
		this.description = desc;
		this.start = $tab_MetricsController.getTiming();
		this.open = true;
		this.$propBag = props;
	};
	$tab_MetricsContext.__typeName = 'tab.MetricsContext';
	global.tab.MetricsContext = $tab_MetricsContext;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Bootstrap.MetricsController
	var $tab_MetricsController = function() {
		this.$nextContextID = 0;
		this.$contextStack = [];
		this.$eventBuffer = [];
		this.$eventLogger = null;
		this.$sessionId = '';
		this.$workbookName = '';
		this.$sheetName = '';
		this.$metricSessionId = '';
		this.$metricsFilter = 0;
		if (ss.isValue(tsConfig.metricsFilter) && tsConfig.metricsFilter !== '') {
			var filter = 0;
			var filters = tsConfig.metricsFilter.split(String.fromCharCode(124));
			for (var $t1 = 0; $t1 < filters.length; $t1++) {
				var suite = filters[$t1];
				var trimmedSuite = ((typeof(suite['trim']) === 'function') ? suite.trim() : suite);
				trimmedSuite = trimmedSuite.toLowerCase();
				if (ss.keyExists($tab_MetricsController.$suiteNameLookup, trimmedSuite)) {
					filter |= $tab_MetricsController.$suiteNameLookup[trimmedSuite];
				}
			}
			this.$metricsFilter = filter;
		}
	};
	$tab_MetricsController.__typeName = 'tab.MetricsController';
	$tab_MetricsController.get_$instance = function MetricsController$get_Instance() {
		if (ss.isNullOrUndefined($tab_MetricsController.$instance)) {
			$tab_MetricsController.$instance = new $tab_MetricsController();
		}
		return $tab_MetricsController.$instance;
	};
	$tab_MetricsController.createContext = function MetricsController$CreateContext(description, suite, props) {
		if (ss.isNullOrUndefined(suite)) {
			suite = 32;
		}
		var filteredMetric = suite === 0 || (suite & $tab_MetricsController.get_$instance().$metricsFilter) !== suite;
		if (ss.isNullOrUndefined(props)) {
			props = {};
		}
		var newContext;
		if (filteredMetric) {
			newContext = $tab_$NullMetricsContext.get_$instance();
		}
		else {
			newContext = new $tab_MetricsContext($tab_MetricsController.get_$instance().$getNextContextID(), suite, description, props);
			$tab_MetricsController.get_$instance().$contextStack.push(newContext);
		}
		return newContext;
	};
	$tab_MetricsController.logEvent = function MetricsController$LogEvent(evt) {
		if (evt.metricSuite === 0 || (evt.metricSuite & $tab_MetricsController.get_$instance().$metricsFilter) !== evt.metricSuite) {
			return;
		}
		if (ss.isValue($tab_MetricsController.get_$instance().$eventLogger)) {
			evt.parameters['sid'] = $tab_MetricsController.get_$instance().$metricSessionId;
			$tab_MetricsController.get_$instance().$eventLogger(evt);
		}
		else {
			$tab_MetricsController.get_$instance().$eventBuffer.push(evt);
		}
	};
	$tab_MetricsController.setEventLogger = function MetricsController$SetEventLogger(logger) {
		$tab_MetricsController.get_$instance().$eventLogger = logger;
		if (ss.isValue(logger) && $tab_MetricsController.$instance.$eventBuffer.length > 0) {
			for (var $t1 = 0; $t1 < $tab_MetricsController.$instance.$eventBuffer.length; $t1++) {
				var bufferedEvt = $tab_MetricsController.$instance.$eventBuffer[$t1];
				bufferedEvt.parameters['sid'] = $tab_MetricsController.$instance.$metricSessionId;
				$tab_MetricsController.$instance.$eventLogger(bufferedEvt);
			}
			$tab_MetricsController.$instance.$eventBuffer = [];
		}
	};
	$tab_MetricsController.initSessionInfo = function MetricsController$InitSessionInfo() {
		var localInstance = $tab_MetricsController.get_$instance();
		var currentSheet = (ss.isNullOrEmptyString(tsConfig.current_sheet_name) ? tsConfig.sheetId : tsConfig.current_sheet_name);
		if (ss.referenceEquals(localInstance.$sessionId, tsConfig.sessionid) && ss.referenceEquals(localInstance.$workbookName, tsConfig.workbookName) && ss.referenceEquals(localInstance.$sheetName, currentSheet)) {
			return;
		}
		localInstance.$sessionId = tsConfig.sessionid;
		localInstance.$workbookName = tsConfig.workbookName;
		localInstance.$sheetName = currentSheet;
		var now = new Date();
		localInstance.$metricSessionId = now.getTime().toString(36);
		localInstance.$metricSessionId = localInstance.$metricSessionId.substr(localInstance.$metricSessionId.length - 6);
		if (localInstance.$sessionId.length >= 5) {
			localInstance.$metricSessionId = localInstance.$metricSessionId + localInstance.$sessionId.substr(1, 1);
			localInstance.$metricSessionId = localInstance.$metricSessionId + localInstance.$sessionId.substr(4, 1);
		}
		localInstance.$logSessionInfo();
	};
	$tab_MetricsController.closeContext = function MetricsController$CloseContext(context) {
		var id = context.id;
		var pos = -1;
		for (var i = $tab_MetricsController.get_$instance().$contextStack.length - 1; i >= 0; i--) {
			if ($tab_MetricsController.$instance.$contextStack[i].id === id) {
				pos = i;
				break;
			}
		}
		if (pos !== -1) {
			var cnt = $tab_MetricsController.$instance.$contextStack.length - pos;
			for (var i1 = 0; i1 < cnt; i1++) {
				$tab_MetricsController.$instance.$contextStack.pop();
			}
		}
		$tab_MetricsController.$instance.$logContextEnd(context);
	};
	$tab_MetricsController.$buildMetricsEventCommonParameters = function MetricsController$BuildMetricsEventCommonParameters(context) {
		var parameters = {};
		parameters['id'] = context.id;
		parameters['d'] = context.description;
		if (ss.getKeyCount(context.get_properties()) > 0) {
			parameters['p'] = context.get_properties();
		}
		return parameters;
	};
	global.tab.MetricsController = $tab_MetricsController;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Bootstrap.MetricsEvent
	var $tab_MetricsEvent = function(evtType, suite, eventParams) {
		this.eventType = null;
		this.metricSuite = 0;
		this.parameters = null;
		this.eventType = evtType;
		this.metricSuite = suite;
		this.parameters = eventParams;
	};
	$tab_MetricsEvent.__typeName = 'tab.MetricsEvent';
	global.tab.MetricsEvent = $tab_MetricsEvent;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.MetricsLogger
	var $tab_MetricsLogger = function() {
		this.$eventBuffer = null;
		this.$logger = null;
		this.$beaconImages = null;
		this.$beaconCleanupTimerId = null;
		this.$bufferProcessTimerId = null;
		this.$eventBuffer = [];
		this.$beaconImages = [];
		this.$bufferProcessTimerId = null;
		this.$beaconCleanupTimerId = null;
	};
	$tab_MetricsLogger.__typeName = 'tab.MetricsLogger';
	$tab_MetricsLogger.get_instance = function MetricsLogger$get_Instance() {
		if (!ss.isValue($tab_MetricsLogger.$instance)) {
			$tab_MetricsLogger.$instance = new $tab_MetricsLogger();
		}
		return $tab_MetricsLogger.$instance;
	};
	$tab_MetricsLogger.$formatEvent = function MetricsLogger$FormatEvent(evt, verbose) {
		var delimiter = (verbose ? ', ' : ',');
		var strBuilder = new ss.StringBuilder();
		strBuilder.append((verbose ? $tab_MetricsLogger.$debugEventNames[evt.eventType] : evt.eventType.toString()));
		var count = ss.getKeyCount(evt.parameters);
		if (count > 0) {
			strBuilder.append('=');
			strBuilder.append('{');
			var i = 0;
			var propSeparator = (verbose ? ': ' : ':');
			var $t1 = ss.getEnumerator(Object.keys(evt.parameters));
			try {
				while ($t1.moveNext()) {
					var key = $t1.current();
					if (key === 'id' && evt.eventType !== 'init') {
						continue;
					}
					if (i++ > 0) {
						strBuilder.append(delimiter);
					}
					strBuilder.append((verbose ? $tab_MetricsLogger.$debugParamNames[key] : key.toString()));
					strBuilder.append(propSeparator);
					var val = evt.parameters[key];
					$tab_MetricsLogger.$formatValue(strBuilder, val, verbose);
				}
			}
			finally {
				$t1.dispose();
			}
			strBuilder.append('}');
		}
		return strBuilder.toString();
	};
	$tab_MetricsLogger.$formatDictionaryValues = function MetricsLogger$FormatDictionaryValues(strBuilder, dict, verbose) {
		var delimiter = (verbose ? ', ' : ',');
		var propSeparator = (verbose ? ': ' : ':');
		var propCount = ss.getKeyCount(dict);
		var j = 0;
		var $t1 = ss.getEnumerator(Object.keys(dict));
		try {
			while ($t1.moveNext()) {
				var propertyName = $t1.current();
				if (dict.hasOwnProperty(propertyName)) {
					var propertyVal = dict[propertyName];
					strBuilder.append(propertyName);
					strBuilder.append(propSeparator);
					$tab_MetricsLogger.$formatValue(strBuilder, propertyVal, verbose);
					if (++j < propCount) {
						strBuilder.append(delimiter);
					}
				}
			}
		}
		finally {
			$t1.dispose();
		}
	};
	$tab_MetricsLogger.$formatValue = function MetricsLogger$FormatValue(strBuilder, value, verbose) {
		var type = typeof(value);
		if (type === 'number' && Math.floor(value) !== value) {
			strBuilder.append(value.toFixed(1));
		}
		else if (type === 'string') {
			if (verbose) {
				strBuilder.append('"');
				strBuilder.append(value);
				strBuilder.append('"');
			}
			else {
				strBuilder.append(encodeURIComponent(value));
			}
		}
		else if ($.isArray(value)) {
			strBuilder.append('[');
			strBuilder.append(value);
			strBuilder.append(']');
		}
		else if (type === 'object') {
			strBuilder.append('{');
			var dict = value;
			$tab_MetricsLogger.$formatDictionaryValues(strBuilder, dict, verbose);
			strBuilder.append('}');
		}
		else {
			strBuilder.append(value);
		}
	};
	global.tab.MetricsLogger = $tab_MetricsLogger;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Bootstrap.MetricsSuites
	var $tab_MetricsSuites = function() {
	};
	$tab_MetricsSuites.__typeName = 'tab.MetricsSuites';
	global.tab.MetricsSuites = $tab_MetricsSuites;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.MiscUtil
	var $tab_MiscUtil = function() {
	};
	$tab_MiscUtil.__typeName = 'tab.MiscUtil';
	$tab_MiscUtil.get_pathName = function MiscUtil$get_PathName() {
		var window = $tab_Utility.get_locationWindow();
		return $tab_WindowHelper.getLocation(window).pathname;
	};
	$tab_MiscUtil.get_urlPathnameParts = function MiscUtil$get_UrlPathnameParts() {
		var pathname = $tab_MiscUtil.get_pathName();
		var siteRoot = tsConfig.site_root;
		var index = pathname.indexOf(siteRoot, 0);
		var actualPath = pathname.substr(index + siteRoot.length);
		var pathnameParts = actualPath.split('/');
		var pathnameProps = {};
		pathnameProps[2] = pathnameParts[2];
		pathnameProps[3] = pathnameParts[3];
		pathnameProps[4] = pathnameParts[4];
		return pathnameProps;
	};
	$tab_MiscUtil.lazyInitStaticField = function MiscUtil$LazyInitStaticField(t, fieldName, initializer) {
		var value = t[fieldName];
		if (ss.isNullOrUndefined(value)) {
			value = initializer();
			t[fieldName] = value;
		}
		return value;
	};
	$tab_MiscUtil.percentEncode = function MiscUtil$PercentEncode(valueToEncode, unreservedChars) {
		valueToEncode = encodeURIComponent(valueToEncode);
		if (ss.isNullOrUndefined(unreservedChars)) {
			return valueToEncode;
		}
		var sb = new ss.StringBuilder();
		var i = 0;
		while (i < valueToEncode.length) {
			var s = valueToEncode.substr(i, 1);
			if (s === '%') {
				sb.append(valueToEncode.substr(i, 3));
				i += 2;
			}
			else if (!ss.keyExists(unreservedChars, s)) {
				sb.append('%').append(s.charCodeAt(0).toString(16).toUpperCase());
			}
			else {
				sb.append(s);
			}
			i++;
		}
		return sb.toString();
	};
	$tab_MiscUtil.encodeForWG = function MiscUtil$EncodeForWG(valueToEncode) {
		var usernameValidChars = {};
		var addCodes = function(from, to) {
			for (var i = from; i <= to; i++) {
				var s = String.fromCharCode(i);
				usernameValidChars[s] = s;
			}
		};
		addCodes(97, 122);
		addCodes(65, 90);
		addCodes(48, 57);
		addCodes(95, 95);
		addCodes(45, 45);
		valueToEncode = $tab_MiscUtil.percentEncode(valueToEncode, usernameValidChars);
		valueToEncode = $tab_MiscUtil.percentEncode(valueToEncode, null);
		return valueToEncode;
	};
	$tab_MiscUtil.isNullOrUndefined = function MiscUtil$IsNullOrUndefined(args) {
		if (ss.isNullOrUndefined(args)) {
			return true;
		}
		for (var i = 0; i < args.length; i++) {
			if (ss.isNullOrUndefined(args[i])) {
				return true;
			}
		}
		return false;
	};
	$tab_MiscUtil.isNullOrEmpty = function MiscUtil$IsNullOrEmpty(args) {
		if (ss.isNullOrUndefined(args)) {
			return true;
		}
		var dict = args;
		if (ss.isValue(dict['length']) && dict['length'] === 0) {
			return true;
		}
		return false;
	};
	$tab_MiscUtil.isNullOrEmpty$1 = function MiscUtil$IsNullOrEmpty(s) {
		return ss.isNullOrEmptyString(s);
	};
	$tab_MiscUtil.isValidIndex = function MiscUtil$IsValidIndex(index, arr) {
		return index >= 0 && index < arr.length;
	};
	$tab_MiscUtil.toBoolean = function MiscUtil$ToBoolean(value, defaultIfMissing) {
		var positiveRegex = new RegExp('^(yes|y|true|t|1)$', 'i');
		if ($tab_MiscUtil.isNullOrEmpty$1(value)) {
			return defaultIfMissing;
		}
		var match = value.match(positiveRegex);
		return !$tab_MiscUtil.isNullOrEmpty(match);
	};
	$tab_MiscUtil.getUriQueryParameters = function MiscUtil$GetUriQueryParameters(uri) {
		var parameters = {};
		if (ss.isNullOrUndefined(uri)) {
			return parameters;
		}
		var indexOfQuery = uri.indexOf('?');
		if (indexOfQuery < 0) {
			return parameters;
		}
		var query = uri.substr(indexOfQuery + 1);
		var indexOfHash = query.indexOf('#');
		if (indexOfHash >= 0) {
			query = query.substr(0, indexOfHash);
		}
		if (ss.isNullOrEmptyString(query)) {
			return parameters;
		}
		var paramPairs = query.split('&');
		for (var $t1 = 0; $t1 < paramPairs.length; $t1++) {
			var pair = paramPairs[$t1];
			var keyValue = pair.split('=');
			var key = decodeURIComponent(keyValue[0]);
			var values;
			if (ss.keyExists(parameters, key)) {
				values = parameters[key];
			}
			else {
				values = [];
				parameters[key] = values;
			}
			if (keyValue.length > 1) {
				values.push(decodeURIComponent(keyValue[1]));
			}
		}
		return parameters;
	};
	$tab_MiscUtil.replaceUriQueryParameters = function MiscUtil$ReplaceUriQueryParameters(uri, parameters) {
		if (ss.getKeyCount(parameters) === 0) {
			return uri;
		}
		var newQueryString = new ss.StringBuilder();
		var first = true;
		var appendSeparator = function() {
			newQueryString.append((first ? '?' : '&'));
			first = false;
		};
		var $t1 = _.keys(parameters);
		for (var $t2 = 0; $t2 < $t1.length; $t2++) {
			var key = $t1[$t2];
			var vals = parameters[key];
			var keyEncoded = encodeURIComponent(key);
			if (ss.isNullOrUndefined(vals) || vals.length === 0) {
				appendSeparator();
				newQueryString.append(keyEncoded);
			}
			else {
				for (var $t3 = 0; $t3 < vals.length; $t3++) {
					var value = vals[$t3];
					appendSeparator();
					newQueryString.append(keyEncoded).append('=').append(encodeURIComponent(value));
				}
			}
		}
		var hash = '';
		var baseUri = '';
		if (uri.length > 0) {
			var indexOfQuery = uri.indexOf('?');
			var indexOfHash = uri.indexOf('#');
			var indexOfEnd = Math.min(((indexOfQuery < 0) ? uri.length : indexOfQuery), ((indexOfHash < 0) ? uri.length : indexOfHash));
			baseUri = uri.substr(0, indexOfEnd);
			hash = ((indexOfHash < 0) ? '' : uri.substr(indexOfHash));
		}
		return baseUri + newQueryString + hash;
	};
	$tab_MiscUtil.sanatizeBoolean = function MiscUtil$SanatizeBoolean(value) {
		if (ss.isNullOrUndefined(value)) {
			return value;
		}
		return value.toString().toLowerCase() === 'true';
	};
	$tab_MiscUtil.dispose$1 = function MiscUtil$Dispose(d) {
		if (ss.isValue(d)) {
			d.dispose();
		}
		return null;
	};
	$tab_MiscUtil.dispose = function MiscUtil$Dispose(d) {
		if (ss.isValue(d)) {
			for (var $t1 = 0; $t1 < d.length; $t1++) {
				var v = d[$t1];
				v.dispose();
			}
			ss.clear(d);
		}
		return null;
	};
	$tab_MiscUtil.clearTimeout = function MiscUtil$ClearTimeout(handle) {
		if (ss.isValue(handle)) {
			window.clearTimeout(ss.unbox(handle));
		}
		return null;
	};
	$tab_MiscUtil.cloneObject = function MiscUtil$CloneObject(src) {
		var objStr = JSON.stringify(src);
		return JSON.parse(objStr);
	};
	global.tab.MiscUtil = $tab_MiscUtil;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.NavigationMetricsCollector
	var $tab_NavigationMetricsCollector = function() {
	};
	$tab_NavigationMetricsCollector.__typeName = 'tab.NavigationMetricsCollector';
	$tab_NavigationMetricsCollector.collectMetrics = function NavigationMetricsCollector$CollectMetrics() {
		if (typeof(window) !== 'undefined' && typeof(window.performance) !== 'undefined' && typeof(window.performance.timing) !== 'undefined') {
			$tab_NavigationMetricsCollector.$navMetrics = window.performance.timing;
			if ('navigationStart' in $tab_NavigationMetricsCollector.$navMetrics) {
				var start = $tab_NavigationMetricsCollector.$navMetrics[$tab_NavigationMetricsCollector.$navigationMetricsOrder[0]];
				var metricArray = [];
				for (var $t1 = 0; $t1 < $tab_NavigationMetricsCollector.$navigationMetricsOrder.length; $t1++) {
					var name = $tab_NavigationMetricsCollector.$navigationMetricsOrder[$t1];
					var metric = $tab_NavigationMetricsCollector.$navMetrics[name];
					metric = ((metric === 0) ? 0 : (metric - start));
					metricArray.push(metric);
				}
				var parameters = {};
				parameters['v'] = metricArray;
				var evt = new $tab_MetricsEvent('nav', 1, parameters);
				$tab_MetricsController.logEvent(evt);
			}
		}
	};
	global.tab.NavigationMetricsCollector = $tab_NavigationMetricsCollector;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.Param
	var $tab_Param = function() {
	};
	$tab_Param.__typeName = 'tab.Param';
	$tab_Param.createArgumentNullOrUndefinedException = function Param$CreateArgumentNullOrUndefinedException(paramName) {
		var ex = new ss.Exception(paramName + ' is null or undefined.');
		ex.paramName = paramName;
		return ex;
	};
	$tab_Param.verifyString = function Param$VerifyString(param, paramName) {
		$tab_Param.verifyValue(param, paramName);
		if (param.trim().length === 0) {
			var ex = new ss.Exception(paramName + ' contains only white space');
			ex.paramName = paramName;
			$tab_Param.$showParameterAlert(ex);
			throw ex;
		}
	};
	$tab_Param.verifyValue = function Param$VerifyValue(param, paramName) {
		if (ss.isNullOrUndefined(param)) {
			var ex = $tab_Param.createArgumentNullOrUndefinedException(paramName);
			$tab_Param.$showParameterAlert(ex);
			throw ex;
		}
	};
	$tab_Param.$showParameterAlert = function Param$ShowParameterAlert(ex) {
		if ($tab_Param.suppressAlerts) {
			return;
		}
		try {
			throw ex;
		}
		catch ($t1) {
			var exceptionWithStack = ss.Exception.wrap($t1);
			window.alert($tab_Param.$formatExceptionMessage(exceptionWithStack));
		}
	};
	$tab_Param.$formatExceptionMessage = function Param$FormatExceptionMessage(ex) {
		var message;
		if (ss.isValue(ex.stack)) {
			message = ex.stack;
		}
		else {
			message = ex.get_message();
		}
		return message;
	};
	global.tab.Param = $tab_Param;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.PathnameKey
	var $tab_PathnameKey = function() {
	};
	$tab_PathnameKey.__typeName = 'tab.PathnameKey';
	global.tab.PathnameKey = $tab_PathnameKey;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.PointFUtil
	var $tab_PointFUtil = function() {
	};
	$tab_PointFUtil.__typeName = 'tab.PointFUtil';
	$tab_PointFUtil.subtract = function PointFUtil$Subtract(first, second) {
		return { x: first.x - second.x, y: first.y - second.y };
	};
	$tab_PointFUtil.timesScalar = function PointFUtil$TimesScalar(p, scalar) {
		return { x: p.x * scalar, y: p.y * scalar };
	};
	$tab_PointFUtil.round = function PointFUtil$Round(p) {
		return { x: Math.round(p.x), y: Math.round(p.y) };
	};
	global.tab.PointFUtil = $tab_PointFUtil;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.PointUtil
	var $tab_PointUtil = function() {
	};
	$tab_PointUtil.__typeName = 'tab.PointUtil';
	$tab_PointUtil.fromPresModel = function PointUtil$FromPresModel(pointPM) {
		if (ss.isNullOrUndefined(pointPM)) {
			return null;
		}
		return { x: pointPM.x, y: pointPM.y };
	};
	$tab_PointUtil.toPresModel = function PointUtil$ToPresModel(pt) {
		if (ss.isNullOrUndefined(pt)) {
			return null;
		}
		var pointPM = {};
		pointPM.x = pt.x;
		pointPM.y = pt.y;
		return pointPM;
	};
	$tab_PointUtil.fromPosition = function PointUtil$FromPosition(position) {
		return { x: $tab_DoubleUtil.roundToInt(position.left), y: $tab_DoubleUtil.roundToInt(position.top) };
	};
	$tab_PointUtil.add = function PointUtil$Add(first, second) {
		if (ss.isNullOrUndefined(first) || ss.isNullOrUndefined(second)) {
			return first;
		}
		return { x: first.x + second.x, y: first.y + second.y };
	};
	$tab_PointUtil.subtract = function PointUtil$Subtract(first, second) {
		return { x: first.x - second.x, y: first.y - second.y };
	};
	$tab_PointUtil.distance = function PointUtil$Distance(first, second) {
		$tab_Param.verifyValue(first, 'first');
		$tab_Param.verifyValue(second, 'second');
		var diffX = first.x - second.x;
		var diffY = first.y - second.y;
		return Math.sqrt(diffX * diffX + diffY * diffY);
	};
	$tab_PointUtil.isWithinDistance = function PointUtil$IsWithinDistance(first, second, distance) {
		var diffX = first.x - second.x;
		var diffY = first.y - second.y;
		return diffX * diffX + diffY * diffY <= distance * distance;
	};
	$tab_PointUtil.equals = function PointUtil$Equals(p, p2) {
		return ss.isValue(p) && ss.isValue(p2) && p2.x === p.x && p2.y === p.y;
	};
	global.tab.PointUtil = $tab_PointUtil;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.RecordCast
	var $tab_RecordCast = function() {
	};
	$tab_RecordCast.__typeName = 'tab.RecordCast';
	$tab_RecordCast.rectAsSize = function RecordCast$RectAsSize(r) {
		return r;
	};
	$tab_RecordCast.regionRectAsSize = function RecordCast$RegionRectAsSize(r) {
		return r;
	};
	$tab_RecordCast.rectPresModelAsRectXY = function RecordCast$RectPresModelAsRectXY(rpm) {
		if (ss.isNullOrUndefined(rpm)) {
			return null;
		}
		return { x: rpm.x, y: rpm.y, w: rpm.w, h: rpm.h };
	};
	$tab_RecordCast.doubleRectPresModelAsDoubleRectXY = function RecordCast$DoubleRectPresModelAsDoubleRectXY(rpm) {
		if (ss.isNullOrUndefined(rpm)) {
			return null;
		}
		return { x: rpm.doubleLeft, y: rpm.doubleTop, w: rpm.width, h: rpm.height };
	};
	$tab_RecordCast.sizeAsSizePresModel = function RecordCast$SizeAsSizePresModel(sz) {
		if (ss.isNullOrUndefined(sz)) {
			return null;
		}
		var spm = {};
		spm.w = sz.w;
		spm.h = sz.h;
		return spm;
	};
	global.tab.RecordCast = $tab_RecordCast;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.ScriptEx
	var $tab_ScriptEx = function() {
	};
	$tab_ScriptEx.__typeName = 'tab.ScriptEx';
	global.tab.ScriptEx = $tab_ScriptEx;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.StackLocation
	var $tab_StackLocation = function(url, lineNo) {
		this.url = null;
		this.lineNo = 0;
		this.columnNo = 0;
		this.functionName = $tab_ErrorTrace.$unknownFunctionName;
		this.context = null;
		this.url = url;
		this.lineNo = lineNo;
	};
	$tab_StackLocation.__typeName = 'tab.StackLocation';
	global.tab.StackLocation = $tab_StackLocation;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.StackTrace
	var $tab_StackTrace = function(traceMode, message) {
		this.userAgent = window.navigator.userAgent;
		this.traceMode = 'onError';
		this.message = null;
		this.url = null;
		this.locations = null;
		this.isIncomplete = false;
		this.isPartial = false;
		this.name = null;
		this.traceMode = traceMode;
		this.message = message;
		this.url = document.URL;
		this.locations = [];
	};
	$tab_StackTrace.__typeName = 'tab.StackTrace';
	global.tab.StackTrace = $tab_StackTrace;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.StackTraceAppender
	var $tab_StackTraceAppender = function() {
		$tab_BaseLogAppender.call(this);
	};
	$tab_StackTraceAppender.__typeName = 'tab.StackTraceAppender';
	$tab_StackTraceAppender.enableLogging = function StackTraceAppender$EnableLogging(filter) {
		if (ss.isNullOrUndefined($tab_StackTraceAppender.$globalAppender)) {
			$tab_StackTraceAppender.$globalAppender = new $tab_StackTraceAppender();
			$tab_Logger.addAppender($tab_StackTraceAppender.$globalAppender);
		}
		$tab_StackTraceAppender.$globalAppender.addFilter(filter || function() {
			return true;
		});
	};
	$tab_StackTraceAppender.disableLogging = function StackTraceAppender$DisableLogging() {
		if (ss.isValue($tab_StackTraceAppender.$globalAppender)) {
			$tab_Logger.removeAppender($tab_StackTraceAppender.$globalAppender);
			$tab_StackTraceAppender.$globalAppender = null;
		}
	};
	global.tab.StackTraceAppender = $tab_StackTraceAppender;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Bootstrap.UIMetricType
	var $tab_UIMetricType = function() {
	};
	$tab_UIMetricType.__typeName = 'tab.UIMetricType';
	global.tab.UIMetricType = $tab_UIMetricType;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Bootstrap.Utility
	var $tab_Utility = function() {
	};
	$tab_Utility.__typeName = 'tab.Utility';
	$tab_Utility.get_$needsSafari7HackFix = function Utility$get_NeedsSafari7HackFix() {
		if (!tsConfig.is_mobile_device || tsConfig.embedded) {
			return false;
		}
		var isAndroid = window.navigator.userAgent.indexOf('Android') !== -1;
		if (isAndroid) {
			return false;
		}
		var isSafari7 = window.navigator.userAgent.indexOf('Safari') !== -1 && window.navigator.userAgent.indexOf('OS 7') !== -1;
		return isSafari7;
	};
	$tab_Utility.get_$inLandscapeMode = function Utility$get_InLandscapeMode() {
		try {
			var win = $tab_Utility.$getTopmostWindow();
			var orientation = win.orientation;
			return ss.isValue(orientation) && (orientation === 90 || orientation === -90);
		}
		catch ($t1) {
		}
		return false;
	};
	$tab_Utility.get_urlLocationSearchParams = function Utility$get_UrlLocationSearchParams() {
		return $tab_Utility.$parseQueryParamString($tab_Utility.get_urlLocationSearch().substring(1));
	};
	$tab_Utility.get_urlLocationHashData = function Utility$get_UrlLocationHashData() {
		var urlHashData = {};
		var fragmentId = $tab_Utility.get_urlLocationHash();
		if (fragmentId.length < 2) {
			return {};
		}
		fragmentId = fragmentId.substr(1);
		var pairs = fragmentId.split('&');
		for (var $t1 = 0; $t1 < pairs.length; $t1++) {
			var pair = pairs[$t1];
			var keyVal = pair.split('=');
			if (keyVal.length === 1) {
				urlHashData[$tab_Utility.CLIENTNO] = keyVal[0];
			}
			else if (keyVal.length === 2) {
				var key = decodeURIComponent(keyVal[0]);
				var value = decodeURIComponent(keyVal[1]);
				urlHashData[key] = value;
			}
		}
		return urlHashData;
	};
	$tab_Utility.set_urlLocationHashData = function Utility$set_UrlLocationHashData(value) {
		var newFragmentId = new ss.StringBuilder();
		var first = true;
		var appendSeparator = function() {
			newFragmentId.append((first ? '#' : '&'));
			first = false;
		};
		var $t1 = new ss.ObjectEnumerator(value);
		try {
			while ($t1.moveNext()) {
				var pairs = $t1.current();
				var keyEncoded = encodeURIComponent(pairs.key);
				appendSeparator();
				if (ss.referenceEquals(keyEncoded, $tab_Utility.CLIENTNO)) {
					newFragmentId.append(pairs.value);
				}
				else if (ss.isNullOrUndefined(pairs.value)) {
					newFragmentId.append(keyEncoded);
				}
				else {
					newFragmentId.append(keyEncoded).append('=').append(encodeURIComponent(pairs.value));
				}
			}
		}
		finally {
			$t1.dispose();
		}
		if (ss.isValue(newFragmentId)) {
			var window = $tab_Utility.get_locationWindow();
			if ($tab_Utility.historyApiSupported()) {
				$tab_Utility.replaceState(window, null, null, newFragmentId.toString());
			}
			else {
				window.location.hash = newFragmentId.toString();
			}
		}
	};
	$tab_Utility.get_urlLocationHash = function Utility$get_UrlLocationHash() {
		var window = $tab_Utility.get_locationWindow();
		return window.location.hash;
	};
	$tab_Utility.get_urlLocationSearch = function Utility$get_UrlLocationSearch() {
		var window = $tab_Utility.get_locationWindow();
		return window.location.search;
	};
	$tab_Utility.get_embedMode = function Utility$get_EmbedMode() {
		return $tab_Utility.$embedModeVar;
	};
	$tab_Utility.get_locationWindow = function Utility$get_LocationWindow() {
		return (($tab_Utility.get_embedMode() === 'embeddedInWg') ? window.parent : window.self);
	};
	$tab_Utility.$parseQueryParamString = function Utility$ParseQueryParamString(urlStr) {
		var urlData = {};
		var pairs = urlStr.split('&');
		for (var $t1 = 0; $t1 < pairs.length; $t1++) {
			var pair = pairs[$t1];
			var keyVal = pair.split('=');
			if (keyVal.length === 2) {
				var key = decodeURIComponent(keyVal[0]);
				var value = decodeURIComponent(keyVal[1]);
				urlData[key] = value;
			}
		}
		return urlData;
	};
	$tab_Utility.xhrPostJsonChunked = function Utility$XhrPostJsonChunked(uri, param, firstChunkCallback, secondaryChunkCallback, errBack, asynchronous) {
		var xhr = $tab_Utility.$createXhr();
		xhr.open('POST', uri, asynchronous);
		xhr.setRequestHeader('Accept', 'text/javascript');
		xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
		if (!ss.isNullOrUndefined(tsConfig.sheetId)) {
			xhr.setRequestHeader('X-Tsi-Active-Tab', tsConfig.sheetId);
		}
		var invokeError = $tab_Utility.$getInvokeErrorDelegate(xhr, errBack);
		var byteOffset = 0;
		var consumeJSONChunks = function() {
			var buffer = '';
			try {
				buffer = xhr.responseText;
			}
			catch ($t1) {
			}
			var bufferLength = buffer.length;
			while (byteOffset < bufferLength) {
				var newData = buffer.substr(byteOffset);
				var regex = new RegExp('^(\\d+);');
				var match = newData.match(regex);
				if (!ss.isValue(match)) {
					return;
				}
				var chunkStart = match[0].length;
				var chunkLength = parseInt(match[1]);
				if (chunkStart + chunkLength > newData.length) {
					return;
				}
				newData = newData.substr(chunkStart, chunkLength);
				var json = null;
				try {
					var contextStr = 'Parse ' + ((byteOffset === 0) ? 'Primary' : 'Secondary') + ' JSON';
					{
						var mc = $tab_MetricsController.createContext(contextStr, 32, null);
						try {
							json = $tab_Utility.$parseJson(newData);
						}
						finally {
							if (ss.isValue(mc)) {
								mc.dispose();
							}
						}
					}
				}
				catch ($t2) {
					invokeError(new ss.Exception('Invalid JSON'));
				}
				if (byteOffset === 0) {
					firstChunkCallback(json);
				}
				else {
					secondaryChunkCallback(json);
				}
				byteOffset += chunkStart + chunkLength;
			}
		};
		var intervalID = -1;
		var isReceiving = false;
		var cannotTouchXhrWhileDownloading = window.navigator.userAgent.indexOf('MSIE') >= 0 && parseFloat(window.navigator.appVersion.split('MSIE ')[1]) < 10;
		xhr.onreadystatechange = function() {
			try {
				if (!cannotTouchXhrWhileDownloading && xhr.readyState === 3 && xhr.status === 200 && !isReceiving) {
					consumeJSONChunks();
					if (intervalID === -1) {
						intervalID = window.setInterval(consumeJSONChunks, 10);
					}
					isReceiving = true;
					return;
				}
				if (xhr.readyState !== 4) {
					return;
				}
				if (intervalID !== -1) {
					window.clearInterval(intervalID);
					intervalID = -1;
				}
				if ($tab_Utility.$isSuccessStatus(xhr)) {
					consumeJSONChunks();
				}
				else {
					invokeError(new ss.Exception('Unable to load ' + uri + '; status: ' + xhr.status));
				}
			}
			catch ($t3) {
				var ex = ss.Exception.wrap($t3);
				if (typeof(ss.getType('ss')) === 'undefined') {
					xhr.abort();
				}
				else {
					throw ex;
				}
			}
		};
		try {
			xhr.send(param);
		}
		catch ($t4) {
			var e = ss.Exception.wrap($t4);
			invokeError(e);
		}
	};
	$tab_Utility.xhrGetXmlSynchronous = function Utility$XhrGetXmlSynchronous(uri, errBack) {
		var xhr = $tab_Utility.$createXhr();
		xhr.open('GET', uri, false);
		xhr.setRequestHeader('Accept', 'text/xml');
		try {
			xhr.send();
		}
		catch ($t1) {
			var e = ss.Exception.wrap($t1);
			$tab_Utility.$invokeErrorDelegate(xhr, errBack, e);
			return null;
		}
		return xhr.responseText;
	};
	$tab_Utility.xhrPostJson = function Utility$XhrPostJson(uri, param, callback, errBack, asynchronous) {
		var xhr = $tab_Utility.$createXhr();
		xhr.open('POST', uri, asynchronous);
		xhr.setRequestHeader('Accept', 'text/javascript');
		xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
		if (ss.isValue(tsConfig.sheetId)) {
			xhr.setRequestHeader('X-Tsi-Active-Tab', tsConfig.sheetId);
		}
		var invokeError = $tab_Utility.$getInvokeErrorDelegate(xhr, errBack);
		xhr.onreadystatechange = function() {
			if (xhr.readyState !== 4) {
				return;
			}
			if ($tab_Utility.$isSuccessStatus(xhr)) {
				try {
					var json = $tab_Utility.$parseJson(xhr.responseText);
					callback(json);
				}
				catch ($t1) {
					var x = ss.Exception.wrap($t1);
					invokeError(x);
				}
			}
			else {
				invokeError(new ss.Exception('Unable to load ' + uri + '; status: ' + xhr.status));
			}
		};
		try {
			xhr.send(param);
		}
		catch ($t2) {
			var e = ss.Exception.wrap($t2);
			invokeError(e);
		}
	};
	$tab_Utility.applySafari7CSSHackFix = function Utility$ApplySafari7CSSHackFix() {
		if ($tab_Utility.get_$needsSafari7HackFix()) {
			if ($tab_Utility.get_$inLandscapeMode()) {
				window.document.body.style.height = window.outerHeight - $tab_Utility.$safari7ClientHeightErrorPixels + 'px';
			}
			else {
				window.document.body.style.height = '';
			}
		}
	};
	$tab_Utility.attachOneTimeMessageHandler = function Utility$AttachOneTimeMessageHandler(eventHandler) {
		var messageListener = null;
		messageListener = function(ev) {
			var e = ev;
			if (eventHandler(e)) {
				if (ss.isValue(window.self.removeEventListener)) {
					window.removeEventListener('message', messageListener, false);
				}
				else {
					window.self.detachEvent('onmessage', messageListener);
				}
			}
		};
		if (ss.isValue(window.self.addEventListener)) {
			window.addEventListener('message', messageListener, false);
		}
		else {
			window.self.attachEvent('onmessage', messageListener);
		}
	};
	$tab_Utility.doPostMessageWithContext = function Utility$DoPostMessageWithContext(message) {
		var success = false;
		if (tsConfig.loadOrderID >= 0) {
			message += ',' + tsConfig.loadOrderID;
		}
		if (!ss.isNullOrEmptyString(tsConfig.apiID)) {
			if (tsConfig.loadOrderID < 0) {
				message += ',' + tsConfig.loadOrderID;
			}
			message += ',' + tsConfig.apiID;
		}
		success = $tab_Utility.doPostMessage(message);
		return success;
	};
	$tab_Utility.doPostMessage = function Utility$DoPostMessage(message) {
		var success = false;
		if ('postMessage' in window) {
			try {
				window.parent.postMessage(message, '*');
				success = true;
			}
			catch ($t1) {
			}
		}
		return success;
	};
	$tab_Utility.$calculateEmbedMode = function Utility$CalculateEmbedMode() {
		var parentIsSelf = false;
		try {
			parentIsSelf = ss.referenceEquals(window.self, window.parent);
		}
		catch ($t1) {
		}
		if (parentIsSelf) {
			return 'notEmbedded';
		}
		if (tsConfig.single_frame) {
			return 'embeddedNotInWg';
		}
		return 'embeddedInWg';
	};
	$tab_Utility.$trim = function Utility$Trim(text) {
		if (ss.isNullOrUndefined(text)) {
			return '';
		}
		var nativeTrimFunction = String.prototype['trim'];
		if (ss.isValue(nativeTrimFunction)) {
			var result = nativeTrimFunction.call(text);
			return result;
		}
		else {
			return text.replace($tab_Utility.$regexTrimLeft, '').replace($tab_Utility.$regexTrimRight, '');
		}
	};
	$tab_Utility.$parseJson = function Utility$ParseJson(data) {
		if (ss.isNullOrUndefined(data) || typeof(data) !== 'string') {
			return null;
		}
		data = $tab_Utility.$trim(data);
		var result = ((window.JSON && window.JSON.parse) ? window.JSON.parse(data) : (new Function('return ' + data))());
		return result;
	};
	$tab_Utility.$createXhr = function Utility$CreateXhr() {
		try {
			return new XMLHttpRequest();
		}
		catch ($t1) {
		}
		try {
			return new ActiveXObject('Microsoft.XMLHTTP');
		}
		catch ($t2) {
		}
		throw new ss.Exception('XMLHttpRequest not supported');
	};
	$tab_Utility.$getViewport = function Utility$GetViewport() {
		var docElem = window.document.documentElement;
		return { w: docElem.clientWidth, h: docElem.clientHeight };
	};
	$tab_Utility.$getTopmostWindow = function Utility$GetTopmostWindow() {
		var win = window.self;
		while (ss.isValue(win.parent) && !ss.referenceEquals(win.parent, win)) {
			win = win.parent;
		}
		return win;
	};
	$tab_Utility.$getNonEmbeddedMobileViewport = function Utility$GetNonEmbeddedMobileViewport() {
		var temp, chromeSpace;
		var w = window.document.documentElement.clientWidth;
		var h = window.document.documentElement.clientHeight;
		var isAndroid = window.navigator.userAgent.indexOf('Android') !== -1;
		if (isAndroid) {
			if (w === window.screen.height) {
				chromeSpace = window.screen.width - h;
				temp = w - chromeSpace;
				w = h + chromeSpace;
				h = temp;
			}
		}
		else if ($tab_Utility.get_$inLandscapeMode()) {
			if (window.innerHeight < h && $tab_Utility.get_$needsSafari7HackFix()) {
				h -= $tab_Utility.$safari7ClientHeightErrorPixels;
			}
			if (w === window.screen.width) {
				chromeSpace = window.screen.height - h;
				temp = w - chromeSpace;
				w = h + chromeSpace;
				h = temp;
			}
		}
		else if (w === window.screen.height) {
			chromeSpace = window.screen.width - h;
			temp = w - chromeSpace;
			w = h + chromeSpace;
			h = temp;
		}
		return { w: w, h: h };
	};
	$tab_Utility.$isCanvasSupported = function Utility$IsCanvasSupported() {
		var canvas = document.createElement('canvas');
		if (ss.isNullOrUndefined(canvas) || ss.isNullOrUndefined(canvas['getContext'])) {
			return false;
		}
		var context = canvas.getContext('2d');
		return typeof(context['fillText']) === 'function' && ss.isValue(context['measureText']('foo'));
	};
	$tab_Utility.get_hashClientNumber = function Utility$get_HashClientNumber() {
		var info = $tab_Utility.get_urlLocationHashData();
		return ((ss.isValue(info) && ss.isValue(info[$tab_Utility.CLIENTNO])) ? info[$tab_Utility.CLIENTNO] : '');
	};
	$tab_Utility.addToUrlHash = function Utility$AddToUrlHash(key, value) {
		var urlHash = $tab_Utility.get_urlLocationHashData();
		urlHash[key] = value;
		$tab_Utility.set_urlLocationHashData(urlHash);
	};
	$tab_Utility.historyApiSupported = function Utility$HistoryApiSupported() {
		return typeof(window.history['pushState']) === 'function' && typeof(window.history['replaceState']) === 'function';
	};
	$tab_Utility.replaceState = function Utility$ReplaceState(window, state, title, url) {
		try {
			window.history.replaceState(state, title, url);
		}
		catch ($t1) {
		}
	};
	$tab_Utility.getValueFromUrlHash = function Utility$GetValueFromUrlHash(key) {
		var urlHash = $tab_Utility.get_urlLocationHashData();
		return (ss.keyExists(urlHash, key) ? urlHash[key] : '');
	};
	$tab_Utility.removeEntryFromUrlHash = function Utility$RemoveEntryFromUrlHash(key) {
		var fragInfo = $tab_Utility.get_urlLocationHashData();
		delete fragInfo[key];
		$tab_Utility.set_urlLocationHashData(fragInfo);
	};
	$tab_Utility.$getDevicePixelRatio = function Utility$GetDevicePixelRatio() {
		var devicePixelRatio = 1;
		if (ss.isValue(tsConfig.highDpi) && tsConfig.highDpi) {
			if (ss.isValue(tsConfig.pixelRatio)) {
				devicePixelRatio = tsConfig.pixelRatio;
			}
			else {
				devicePixelRatio = ss.coalesce(window.self['devicePixelRatio'], 1);
			}
		}
		return devicePixelRatio;
	};
	$tab_Utility.$isSuccessStatus = function Utility$IsSuccessStatus(xhr) {
		var status = (ss.isValue(xhr.status) ? xhr.status : 0);
		if (status >= 200 && status < 300 || status === 304 || status === 1223 || 0 === status && (window.location.protocol === 'file:' || window.location.protocol === 'chrome:')) {
			return true;
		}
		return false;
	};
	$tab_Utility.$invokeErrorDelegate = function Utility$InvokeErrorDelegate(xhr, errBack, e) {
		if (ss.staticEquals(errBack, null)) {
			return;
		}
		var invokeError = $tab_Utility.$getInvokeErrorDelegate(xhr, errBack);
		invokeError(e);
	};
	$tab_Utility.$getInvokeErrorDelegate = function Utility$GetInvokeErrorDelegate(xhr, errBack) {
		return function(err) {
			err.status = xhr.status;
			err.responseText = xhr.responseText;
			errBack(err);
		};
	};
	global.tab.Utility = $tab_Utility;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.Vql.Core.WindowAppender
	var $tab_WindowAppender = function() {
		this.$logDiv = null;
		$tab_BaseLogAppender.call(this);
	};
	$tab_WindowAppender.__typeName = 'tab.WindowAppender';
	$tab_WindowAppender.enableLogging = function WindowAppender$EnableLogging(filter) {
		if (ss.isNullOrUndefined($tab_WindowAppender.$globalAppender)) {
			$tab_WindowAppender.$globalAppender = new $tab_WindowAppender();
			$tab_Logger.addAppender($tab_WindowAppender.$globalAppender);
		}
		$tab_WindowAppender.$globalAppender.addFilter(filter || function() {
			return true;
		});
	};
	$tab_WindowAppender.disableLogging = function WindowAppender$DisableLogging() {
		if (ss.isNullOrUndefined($tab_WindowAppender.$globalAppender)) {
			return;
		}
		$tab_Logger.removeAppender($tab_WindowAppender.$globalAppender);
		$tab_WindowAppender.$globalAppender = null;
	};
	global.tab.WindowAppender = $tab_WindowAppender;
	////////////////////////////////////////////////////////////////////////////////
	// Tableau.JavaScript.CoreSlim.WindowHelper
	var $tab_WindowHelper = function(window) {
		this.$window = null;
		this.$window = window;
	};
	$tab_WindowHelper.__typeName = 'tab.WindowHelper';
	$tab_WindowHelper.get_windowSelf = function WindowHelper$get_WindowSelf() {
		return window.self;
	};
	$tab_WindowHelper.get_selection = function WindowHelper$get_Selection() {
		if (typeof(window['getSelection']) === 'function') {
			return window.getSelection();
		}
		else if (typeof(document['getSelection']) === 'function') {
			return document.getSelection();
		}
		return null;
	};
	$tab_WindowHelper.close = function WindowHelper$Close(window) {
		window.close();
	};
	$tab_WindowHelper.getOpener = function WindowHelper$GetOpener(window) {
		return window.opener;
	};
	$tab_WindowHelper.getLocation = function WindowHelper$GetLocation(window) {
		return window.location;
	};
	$tab_WindowHelper.getPathAndSearch = function WindowHelper$GetPathAndSearch(window) {
		return window.location.pathname + window.location.search;
	};
	$tab_WindowHelper.setLocationHref = function WindowHelper$SetLocationHref(window, href) {
		window.location.href = href;
	};
	$tab_WindowHelper.locationReplace = function WindowHelper$LocationReplace(window, url) {
		window.location.replace(url);
	};
	$tab_WindowHelper.open = function WindowHelper$Open(href, target, options) {
		return window.open(href, target, options);
	};
	$tab_WindowHelper.reload = function WindowHelper$Reload(w, forceGet) {
		w.location.reload(forceGet);
	};
	$tab_WindowHelper.requestAnimationFrame = function WindowHelper$RequestAnimationFrame(action) {
		return $tab_WindowHelper.$requestAnimationFrameFunc(action);
	};
	$tab_WindowHelper.cancelAnimationFrame = function WindowHelper$CancelAnimationFrame(animationId) {
		if (ss.isValue(animationId)) {
			$tab_WindowHelper.$cancelAnimationFrameFunc(animationId);
		}
	};
	$tab_WindowHelper.setTimeout = function WindowHelper$SetTimeout(callback, milliseconds) {
		window.setTimeout(callback, milliseconds);
	};
	$tab_WindowHelper.addListener = function WindowHelper$AddListener(windowParam, eventName, messageListener) {
		if ('addEventListener' in windowParam) {
			windowParam.addEventListener(eventName, messageListener, false);
		}
		else {
			windowParam.attachEvent('on' + eventName, messageListener);
		}
	};
	$tab_WindowHelper.removeListener = function WindowHelper$RemoveListener(window, eventName, messageListener) {
		if ('removeEventListener' in window) {
			window.removeEventListener(eventName, messageListener, false);
		}
		else {
			window.detachEvent('on' + eventName, messageListener);
		}
	};
	$tab_WindowHelper.$setDefaultRequestAnimationFrameImpl = function WindowHelper$SetDefaultRequestAnimationFrameImpl() {
		var lastTime = 0;
		$tab_WindowHelper.$requestAnimationFrameFunc = function(callback) {
			var curTime = (new Date()).getTime();
			var timeToCall = Math.max(0, 16 - (curTime - lastTime));
			lastTime = curTime + timeToCall;
			var id = window.setTimeout(callback, timeToCall);
			return id;
		};
	};
	$tab_WindowHelper.clearSelection = function WindowHelper$ClearSelection() {
		var selection = $tab_WindowHelper.get_selection();
		if (ss.isValue(selection)) {
			if (typeof(selection['removeAllRanges']) === 'function') {
				selection.removeAllRanges();
			}
			else if (typeof(selection['empty']) === 'function') {
				selection['empty']();
			}
		}
	};
	global.tab.WindowHelper = $tab_WindowHelper;
	ss.initClass($tab_MetricsContext, $asm, {
		get_endTime: function MetricsContext$get_EndTime() {
			return this.end;
		},
		get_properties: function MetricsContext$get_Properties() {
			return this.$propBag;
		},
		dispose: function MetricsContext$Dispose() {
			this.close();
		},
		close: function MetricsContext$Close() {
			if (this.open) {
				this.end = $tab_MetricsController.getTiming();
				$tab_MetricsController.closeContext(this);
				this.open = false;
			}
		},
		elapsedMS: function MetricsContext$ElapsedMS() {
			if (!this.open) {
				return this.end - this.start;
			}
			return $tab_MetricsController.getTiming() - this.start;
		}
	}, null, [ss.IDisposable]);
	ss.initClass($tab_$NullMetricsContext, $asm, {
		close: function NullMetricsContext$Close() {
		}
	}, $tab_MetricsContext, [ss.IDisposable]);
	ss.initInterface($tab_ILogAppender, $asm, { addFilter: null, removeFilter: null, clearFilters: null, log: null });
	ss.initClass($tab_BaseLogAppender, $asm, {
		clearFilters: function BaseLogAppender$ClearFilters() {
			ss.clear(this.$filters);
		},
		addFilter: function BaseLogAppender$AddFilter(f) {
			this.$filters.push(f);
		},
		removeFilter: function BaseLogAppender$RemoveFilter(f) {
			ss.remove(this.$filters, f);
		},
		log: function BaseLogAppender$Log(source, level, message, args) {
		},
		logInternal: null,
		formatMessage: function BaseLogAppender$FormatMessage(message, args) {
			if (ss.isNullOrUndefined(args) || args.length === 0) {
				return message;
			}
			var sb = new ss.StringBuilder();
			var argNum = 0;
			var prevPercent = false;
			for (var i = 0; i < message.length; i++) {
				var currChar = message.charCodeAt(i);
				if (currChar === 37) {
					if (prevPercent) {
						sb.append('%');
						prevPercent = false;
					}
					else {
						prevPercent = true;
					}
				}
				else {
					if (prevPercent) {
						switch (currChar) {
							case 98:
							case 115:
							case 100:
							case 110:
							case 111: {
								sb.append(((args.length > argNum) ? args[argNum] : ''));
								argNum++;
								break;
							}
						}
					}
					else {
						sb.appendChar(currChar);
					}
					prevPercent = false;
				}
			}
			return sb.toString();
		}
	}, null, [$tab_ILogAppender]);
	ss.initClass($tab_BrowserSupport, $asm, {});
	ss.initClass($tab_ConsoleLogAppender, $asm, {
		logInternal: function ConsoleLogAppender$LogInternal(source, level, message, args) {
			if (typeof(window.console) !== 'object') {
				return;
			}
			message = source.get_name() + ': ' + message;
			var consoleArgs = [];
			if ($tab_BrowserSupport.get_consoleLogFormating()) {
				var $t1 = consoleArgs.concat(message);
				consoleArgs = $t1.concat.apply($t1, args);
			}
			else {
				consoleArgs = consoleArgs.concat(this.formatMessage(message, args));
			}
			try {
				Function.prototype.apply.call(this.$getConsoleMethod(level), window.console, consoleArgs);
			}
			catch ($t2) {
			}
		},
		$getConsoleMethod: function ConsoleLogAppender$GetConsoleMethod(level) {
			var console = window.self['console'];
			if (ss.isNullOrUndefined(this.$levelMethods)) {
				this.$levelMethods = {};
				this.$levelMethods[(1).toString()] = console.log;
				this.$levelMethods[(4).toString()] = console.error;
				this.$levelMethods[(2).toString()] = console.info;
				this.$levelMethods[(3).toString()] = console.warn;
			}
			var $t1 = this.$levelMethods[level.toString()];
			if (ss.isNullOrUndefined($t1)) {
				$t1 = console.log;
			}
			return $t1;
		}
	}, $tab_BaseLogAppender, [$tab_ILogAppender]);
	ss.initClass($tab_DomUtil, $asm, {});
	ss.initClass($tab_DoubleUtil, $asm, {});
	ss.initEnum($tab_EmbedMode, $asm, { notEmbedded: 'notEmbedded', embeddedInWg: 'embeddedInWg', embeddedNotInWg: 'embeddedNotInWg' }, true);
	ss.initClass($tab_ErrorTrace, $asm, {});
	ss.initInterface($tab_IBrowserViewport, $asm, { get_dimensions: null, translatePositionToViewport: null, getVisibleRoom: null, getDocumentViewport: null });
	ss.initClass($tab_LayoutMetrics, $asm, {
		toJson: function LayoutMetrics$ToJson() {
			var sb = new ss.StringBuilder();
			sb.append('{\n');
			var length = 0;
			var $t1 = new ss.ObjectEnumerator($tab_LayoutMetrics.$defaultDb);
			try {
				while ($t1.moveNext()) {
					var entry = $t1.current();
					length++;
				}
			}
			finally {
				$t1.dispose();
			}
			var i = 0;
			var $t2 = new ss.ObjectEnumerator($tab_LayoutMetrics.$defaultDb);
			try {
				while ($t2.moveNext()) {
					var entry1 = $t2.current();
					if (ss.isValue(this[entry1.key])) {
						var m = this[entry1.key];
						sb.append('\t"' + entry1.key + '": {\n');
						sb.append('\t\t"w": ' + m.w + ',\n');
						sb.append('\t\t"h": ' + m.h + '\n');
						sb.append('\t}');
						if (i < length - 1) {
							sb.append(',');
						}
						sb.append('\n');
					}
					++i;
				}
			}
			finally {
				$t2.dispose();
			}
			sb.append('}');
			return sb.toString();
		},
		$cloneDefaultDb: function LayoutMetrics$CloneDefaultDb() {
			var $t1 = new ss.ObjectEnumerator($tab_LayoutMetrics.$defaultDb);
			try {
				while ($t1.moveNext()) {
					var entry = $t1.current();
					this[entry.key] = entry.value;
				}
			}
			finally {
				$t1.dispose();
			}
		}
	});
	ss.initClass($tab_Log, $asm, {});
	ss.initClass($tab_Logger, $asm, {
		get_name: function Logger$get_Name() {
			return this.$name;
		},
		debug: function Logger$Debug(message, args) {
		},
		info: function Logger$Info(message, args) {
		},
		warn: function Logger$Warn(message, args) {
		},
		error: function Logger$Error(message, args) {
		},
		log: function Logger$Log(level, message, args) {
		},
		$logInternal: function Logger$LogInternal(level, message, args) {
			try {
				var $t1 = $tab_Logger.get_$appenders();
				for (var $t2 = 0; $t2 < $t1.length; $t2++) {
					var logAppender = $t1[$t2];
					logAppender.log(this, level, message, args);
				}
			}
			catch ($t3) {
			}
		}
	});
	ss.initEnum($tab_LoggerLevel, $asm, { all: 0, debug: 1, info: 2, warn: 3, error: 4, off: 5 });
	ss.initClass($tab_Metric, $asm, {}, Object);
	ss.initClass($tab_MetricsController, $asm, {
		$getNextContextID: function MetricsController$GetNextContextID() {
			var id = this.$nextContextID;
			++this.$nextContextID;
			return id;
		},
		$logSessionInfo: function MetricsController$LogSessionInfo() {
			var parameters = {};
			parameters['id'] = this.$sessionId;
			parameters['sid'] = this.$metricSessionId;
			parameters['wb'] = this.$workbookName;
			parameters['s'] = this.$sheetName;
			parameters['m'] = tsConfig.is_mobile;
			$tab_MetricsController.logEvent(new $tab_MetricsEvent('init', 2, parameters));
		},
		$logContextEnd: function MetricsController$LogContextEnd(context) {
			var parameters = $tab_MetricsController.$buildMetricsEventCommonParameters(context);
			parameters['t'] = context.get_endTime();
			parameters['e'] = context.elapsedMS();
			$tab_MetricsController.logEvent(new $tab_MetricsEvent('wp', context.metricSuite, parameters));
		}
	});
	ss.initClass($tab_MetricsEvent, $asm, {});
	ss.initClass($tab_MetricsLogger, $asm, {
		logEvent: function MetricsLogger$LogEvent(evt) {
			if (this.$eventBuffer.length >= $tab_MetricsLogger.$maxEventBufferSize) {
				this.$eventBuffer.shift();
			}
			this.$eventBuffer.push(evt);
			this.$startProcessingTimer();
		},
		attach: function MetricsLogger$Attach() {
			$tab_MetricsController.setEventLogger(ss.mkdel(this, this.logEvent));
		},
		$startProcessingTimer: function MetricsLogger$StartProcessingTimer(delay) {
			if (ss.isValue(this.$bufferProcessTimerId)) {
				return;
			}
			delay = ss.coalesce(delay, $tab_MetricsLogger.$defaultProcessingDelay);
			this.$bufferProcessTimerId = window.setTimeout(ss.mkdel(this, this.$processBufferedEvents), delay);
		},
		$processBufferedEvents: function MetricsLogger$ProcessBufferedEvents() {
			this.$bufferProcessTimerId = null;
			var metricsToProcess;
			if (this.$eventBuffer.length > $tab_MetricsLogger.$maxEventsToProcess) {
				metricsToProcess = this.$eventBuffer.slice(0, $tab_MetricsLogger.$maxEventsToProcess);
				this.$eventBuffer = this.$eventBuffer.slice($tab_MetricsLogger.$maxEventsToProcess);
				this.$startProcessingTimer($tab_MetricsLogger.$overflowProcessingDelay);
			}
			else {
				metricsToProcess = this.$eventBuffer;
				this.$eventBuffer = [];
			}
			this.$outputEventsToConsole(metricsToProcess);
			if (tsConfig.metricsReportingEnabled) {
				try {
					this.$outputEventsToServer(metricsToProcess);
				}
				catch ($t1) {
				}
			}
		},
		$outputEventsToConsole: function MetricsLogger$OutputEventsToConsole(evts) {
			this.$logger = this.$logger || $tab_Logger.lazyGetLogger($tab_MetricsLogger);
			for (var $t1 = 0; $t1 < evts.length; $t1++) {
				var evt = evts[$t1];
			}
		},
		$outputEventsToServer: function MetricsLogger$OutputEventsToServer(evts) {
			var MaxPayloadLength = 1500;
			var numEvents = evts.length;
			var payload = '';
			if (numEvents === 0) {
				return;
			}
			for (var i = 0; i < numEvents; i++) {
				var evt = evts[i];
				if (evt.eventType === 'wps') {
					continue;
				}
				var formattedEvent = $tab_MetricsLogger.$formatEvent(evt, false);
				if (payload.length > 0 && payload.length + formattedEvent.length > MaxPayloadLength) {
					this.$sendBeacon(tsConfig.metricsServerHostname, payload);
					payload = '';
				}
				else if (payload.length > 0) {
					payload += '&';
				}
				payload += formattedEvent;
			}
			if (payload.length > 0) {
				this.$sendBeacon(tsConfig.metricsServerHostname, payload);
			}
			if (ss.isNullOrUndefined(this.$beaconCleanupTimerId)) {
				this.$beaconCleanupTimerId = window.setTimeout(ss.mkdel(this, this.$cleanupBeaconImages), $tab_MetricsLogger.$beaconCleanupDelay);
			}
		},
		$sendBeacon: function MetricsLogger$SendBeacon(hostname, payload) {
			var Version = 1;
			if ($tab_MiscUtil.isNullOrEmpty$1(hostname) || $tab_MiscUtil.isNullOrEmpty$1(payload)) {
				return;
			}
			var beaconImg = document.createElement('img');
			var versionStr = 'v=' + Version.toString();
			var beaconStr = hostname;
			beaconStr += '?' + versionStr + '&' + payload;
			beaconImg.src = beaconStr;
			this.$beaconImages.push(beaconImg);
			if (this.$beaconImages.length > $tab_MetricsLogger.$maxBeaconElementArraySize) {
				this.$beaconImages.shift();
			}
		},
		$cleanupBeaconImages: function MetricsLogger$CleanupBeaconImages() {
			try {
				this.$beaconCleanupTimerId = null;
				var index = 0;
				while (index < this.$beaconImages.length) {
					if (this.$beaconImages[index].complete) {
						this.$beaconImages.splice(index, 1);
					}
					else {
						index++;
					}
				}
				if (this.$beaconImages.length > 0) {
					this.$beaconCleanupTimerId = window.setTimeout(ss.mkdel(this, this.$cleanupBeaconImages), $tab_MetricsLogger.$beaconCleanupDelay);
				}
			}
			catch ($t1) {
			}
		}
	});
	ss.initEnum($tab_MetricsSuites, $asm, { none: 0, navigation: 1, bootstrap: 2, rendering: 4, commands: 8, hitTest: 16, debug: 32, toolbar: 64, fonts: 128, min: 3, core: 15, all: 255 });
	ss.initClass($tab_MiscUtil, $asm, {});
	ss.initClass($tab_NavigationMetricsCollector, $asm, {});
	ss.initClass($tab_Param, $asm, {});
	ss.initEnum($tab_PathnameKey, $asm, { workbookName: 2, sheetId: 3, authoringSheet: 4 });
	ss.initClass($tab_PointFUtil, $asm, {});
	ss.initClass($tab_PointUtil, $asm, {});
	ss.initClass($tab_RecordCast, $asm, {});
	ss.initClass($tab_ScriptEx, $asm, {});
	ss.initClass($tab_StackLocation, $asm, {});
	ss.initClass($tab_StackTrace, $asm, {});
	ss.initClass($tab_StackTraceAppender, $asm, {
		logInternal: function StackTraceAppender$LogInternal(source, level, message, args) {
			message = this.formatMessage(ss.replaceAllString(message, '\\n', '<br />'), args);
			if (level > 2) {
				try {
					throw new ss.Exception('Logged(' + $tab_Logger.loggerLevelNames[level] + ', from ' + source.get_name() + '): ' + message);
				}
				catch ($t1) {
					var e = ss.Exception.wrap($t1);
					$tab_ErrorTrace.report(e, false);
				}
			}
		}
	}, $tab_BaseLogAppender, [$tab_ILogAppender]);
	ss.initEnum($tab_UIMetricType, $asm, { scrollbar: 'scrollbar', qfixed: 'qfixed', qslider: 'qslider', qreadout: 'qreadout', cfixed: 'cfixed', citem: 'citem', hfixed: 'hfixed', hitem: 'hitem', cmslider: 'cmslider', cmdropdown: 'cmdropdown', cmpattern: 'cmpattern', rdate: 'rdate', rdatep: 'rdatep', capply: 'capply', cmtypeinsearch: 'cmtypeinsearch', ccustomitem: 'ccustomitem' }, true);
	ss.initClass($tab_Utility, $asm, {});
	ss.initClass($tab_WindowAppender, $asm, {
		logInternal: function WindowAppender$LogInternal(source, level, message, args) {
			if (ss.isNullOrUndefined(this.$logDiv)) {
				this.$buildLogDiv();
			}
			message = this.formatMessage(ss.replaceAllString(message, '\\n', '<br />'), args);
			this.$logDiv.html(message);
		},
		$buildLogDiv: function WindowAppender$BuildLogDiv() {
			this.$logDiv = $("<div class='log-window-appender'>Debug mode ON</div>");
			this.$logDiv.css(ss.mkdict(['position', 'absolute', 'bottom', '0px', 'right', '0px', 'backgroundColor', 'white', 'opacity', '.8', 'border', '1px solid black', 'minWidth', '5px', 'minHeight', '5px', 'z-index', '100']));
			$('body').append(this.$logDiv);
		}
	}, $tab_BaseLogAppender, [$tab_ILogAppender]);
	ss.initClass($tab_WindowHelper, $asm, {
		get_pageXOffset: function WindowHelper$get_PageXOffset() {
			return $tab_WindowHelper.$pageXOffsetFunc(this.$window);
		},
		get_pageYOffset: function WindowHelper$get_PageYOffset() {
			return $tab_WindowHelper.$pageYOffsetFunc(this.$window);
		},
		get_clientWidth: function WindowHelper$get_ClientWidth() {
			return $tab_WindowHelper.$clientWidthFunc(this.$window);
		},
		get_clientHeight: function WindowHelper$get_ClientHeight() {
			return $tab_WindowHelper.$clientHeightFunc(this.$window);
		},
		get_innerWidth: function WindowHelper$get_InnerWidth() {
			return $tab_WindowHelper.$innerWidthFunc(this.$window);
		},
		get_outerWidth: function WindowHelper$get_OuterWidth() {
			return $tab_WindowHelper.$outerWidthFunc(this.$window);
		},
		get_innerHeight: function WindowHelper$get_InnerHeight() {
			return $tab_WindowHelper.$innerHeightFunc(this.$window);
		},
		get_outerHeight: function WindowHelper$get_OuterHeight() {
			return $tab_WindowHelper.$outerHeightFunc(this.$window);
		},
		get_screenLeft: function WindowHelper$get_ScreenLeft() {
			return $tab_WindowHelper.$screenLeftFunc(this.$window);
		},
		get_screenTop: function WindowHelper$get_ScreenTop() {
			return $tab_WindowHelper.$screenTopFunc(this.$window);
		}
	});
	ss.setMetadata($tab_MetricsSuites, { enumFlags: true });
	(function() {
		$tab_$NullMetricsContext.$instance = null;
	})();
	(function() {
		$tab_MetricsController.getTiming = null;
		$tab_MetricsController.$epoch = 0;
		$tab_MetricsController.$instance = null;
		$tab_MetricsController.$suiteNameLookup = null;
		if (ss.isValue(window) && ss.isValue(window.self.performance) && ss.isValue(window.self.performance['now'])) {
			if (ss.isValue(window.self.performance.timing)) {
				$tab_MetricsController.$epoch = window.self.performance.timing.responseStart - window.self.performance.timing.navigationStart;
			}
			else {
				$tab_MetricsController.$epoch = 0;
			}
			$tab_MetricsController.getTiming = function() {
				return window.self.performance.now() - $tab_MetricsController.$epoch;
			};
		}
		else {
			$tab_MetricsController.$epoch = (new Date()).getTime();
			$tab_MetricsController.getTiming = function() {
				return (new Date()).getTime() - $tab_MetricsController.$epoch;
			};
		}
		$tab_MetricsController.$suiteNameLookup = {};
		$tab_MetricsController.$suiteNameLookup['none'] = 0;
		$tab_MetricsController.$suiteNameLookup['navigation'] = 1;
		$tab_MetricsController.$suiteNameLookup['bootstrap'] = 2;
		$tab_MetricsController.$suiteNameLookup['rendering'] = 4;
		$tab_MetricsController.$suiteNameLookup['commands'] = 8;
		$tab_MetricsController.$suiteNameLookup['toolbar'] = 64;
		$tab_MetricsController.$suiteNameLookup['hittest'] = 16;
		$tab_MetricsController.$suiteNameLookup['debug'] = 32;
		$tab_MetricsController.$suiteNameLookup['fonts'] = 128;
		$tab_MetricsController.$suiteNameLookup['min'] = 3;
		$tab_MetricsController.$suiteNameLookup['core'] = 15;
		$tab_MetricsController.$suiteNameLookup['all'] = 255;
	})();
	(function() {
		$tab_Utility.$safari7ClientHeightErrorPixels = 20;
		$tab_Utility.$regexNotwhite = new RegExp('\\s');
		$tab_Utility.$regexTrimLeft = new RegExp('^\\s+');
		$tab_Utility.$regexTrimRight = new RegExp('\\s+$');
		$tab_Utility.$embedModeVar = null;
		$tab_Utility.CLIENTNO = 'cn';
		if ($tab_Utility.$regexNotwhite.test('\\xA0')) {
			$tab_Utility.$regexTrimLeft = new RegExp('^[\\s\\xA0]+');
			$tab_Utility.$regexTrimRight = new RegExp('[\\s\\xA0]+$');
		}
		$tab_Utility.$embedModeVar = $tab_Utility.$calculateEmbedMode();
	})();
	(function() {
		$tab_WindowHelper.$innerWidthFunc = null;
		$tab_WindowHelper.$innerHeightFunc = null;
		$tab_WindowHelper.$clientWidthFunc = null;
		$tab_WindowHelper.$clientHeightFunc = null;
		$tab_WindowHelper.$pageXOffsetFunc = null;
		$tab_WindowHelper.$pageYOffsetFunc = null;
		$tab_WindowHelper.$screenLeftFunc = null;
		$tab_WindowHelper.$screenTopFunc = null;
		$tab_WindowHelper.$outerWidthFunc = null;
		$tab_WindowHelper.$outerHeightFunc = null;
		$tab_WindowHelper.$requestAnimationFrameFunc = null;
		$tab_WindowHelper.$cancelAnimationFrameFunc = null;
		if ('innerWidth' in window) {
			$tab_WindowHelper.$innerWidthFunc = function(w) {
				return w.innerWidth;
			};
		}
		else {
			$tab_WindowHelper.$innerWidthFunc = function(w1) {
				return w1.document.documentElement.offsetWidth;
			};
		}
		if ('outerWidth' in window) {
			$tab_WindowHelper.$outerWidthFunc = function(w2) {
				return w2.outerWidth;
			};
		}
		else {
			$tab_WindowHelper.$outerWidthFunc = $tab_WindowHelper.$innerWidthFunc;
		}
		if ('innerHeight' in window) {
			$tab_WindowHelper.$innerHeightFunc = function(w3) {
				return w3.innerHeight;
			};
		}
		else {
			$tab_WindowHelper.$innerHeightFunc = function(w4) {
				return w4.document.documentElement.offsetHeight;
			};
		}
		if ('outerHeight' in window) {
			$tab_WindowHelper.$outerHeightFunc = function(w5) {
				return w5.outerHeight;
			};
		}
		else {
			$tab_WindowHelper.$outerHeightFunc = $tab_WindowHelper.$innerHeightFunc;
		}
		if ('clientWidth' in window) {
			$tab_WindowHelper.$clientWidthFunc = function(w6) {
				return w6['clientWidth'];
			};
		}
		else {
			$tab_WindowHelper.$clientWidthFunc = function(w7) {
				return w7.document.documentElement.clientWidth;
			};
		}
		if ('clientHeight' in window) {
			$tab_WindowHelper.$clientHeightFunc = function(w8) {
				return w8['clientHeight'];
			};
		}
		else {
			$tab_WindowHelper.$clientHeightFunc = function(w9) {
				return w9.document.documentElement.clientHeight;
			};
		}
		if (ss.isValue(window.self.pageXOffset)) {
			$tab_WindowHelper.$pageXOffsetFunc = function(w10) {
				return w10.pageXOffset;
			};
		}
		else {
			$tab_WindowHelper.$pageXOffsetFunc = function(w11) {
				return w11.document.documentElement.scrollLeft;
			};
		}
		if (ss.isValue(window.self.pageYOffset)) {
			$tab_WindowHelper.$pageYOffsetFunc = function(w12) {
				return w12.pageYOffset;
			};
		}
		else {
			$tab_WindowHelper.$pageYOffsetFunc = function(w13) {
				return w13.document.documentElement.scrollTop;
			};
		}
		if ('screenLeft' in window) {
			$tab_WindowHelper.$screenLeftFunc = function(w14) {
				return w14.screenLeft;
			};
		}
		else {
			$tab_WindowHelper.$screenLeftFunc = function(w15) {
				return w15.screenX;
			};
		}
		if ('screenTop' in window) {
			$tab_WindowHelper.$screenTopFunc = function(w16) {
				return w16.screenTop;
			};
		}
		else {
			$tab_WindowHelper.$screenTopFunc = function(w17) {
				return w17.screenY;
			};
		}
		{
			var DefaultRequestName = 'requestAnimationFrame';
			var DefaultCancelName = 'cancelAnimationFrame';
			var vendors = ['ms', 'moz', 'webkit', 'o'];
			var requestFuncName = null;
			var cancelFuncName = null;
			if (DefaultRequestName in window) {
				requestFuncName = DefaultRequestName;
			}
			if (DefaultCancelName in window) {
				cancelFuncName = DefaultCancelName;
			}
			for (var ii = 0; ii < vendors.length && (ss.isNullOrUndefined(requestFuncName) || ss.isNullOrUndefined(cancelFuncName)); ++ii) {
				var vendor = vendors[ii];
				var funcName = vendor + 'RequestAnimationFrame';
				if (ss.isNullOrUndefined(requestFuncName) && funcName in window) {
					requestFuncName = funcName;
				}
				if (ss.isNullOrUndefined(cancelFuncName)) {
					funcName = vendor + 'CancelAnimationFrame';
					if (funcName in window) {
						cancelFuncName = funcName;
					}
					funcName = vendor + 'CancelRequestAnimationFrame';
					if (funcName in window) {
						cancelFuncName = funcName;
					}
				}
			}
			if (ss.isValue(requestFuncName)) {
				$tab_WindowHelper.$requestAnimationFrameFunc = function(callback) {
					return window[requestFuncName](callback);
				};
			}
			else {
				$tab_WindowHelper.$setDefaultRequestAnimationFrameImpl();
			}
			if (ss.isValue(cancelFuncName)) {
				$tab_WindowHelper.$cancelAnimationFrameFunc = function(animationId) {
					window[cancelFuncName](animationId);
				};
			}
			else {
				$tab_WindowHelper.$cancelAnimationFrameFunc = function(id) {
					window.clearTimeout(id);
				};
			}
		}
	})();
	(function() {
		$tab_Logger.global = $tab_Logger.getLoggerWithName('global');
		$tab_Logger.loggerLevelNames = [];
		$tab_Logger.$logQueryParam = ':log';
		$tab_Logger.loggerLevelNames[0] = 'all';
		$tab_Logger.loggerLevelNames[1] = 'debug';
		$tab_Logger.loggerLevelNames[2] = 'info';
		$tab_Logger.loggerLevelNames[3] = 'warn';
		$tab_Logger.loggerLevelNames[4] = 'error';
		$tab_Logger.loggerLevelNames[5] = 'off';
	})();
	(function() {
		$tab_Param.suppressAlerts = false;
	})();
	(function() {
		$tab_DoubleUtil.epsilon = Math.pow(2, -52);
		$tab_DoubleUtil.$onePlusEpsilon = 1 + $tab_DoubleUtil.epsilon;
		$tab_DoubleUtil.$upperBound = $tab_DoubleUtil.$onePlusEpsilon;
		$tab_DoubleUtil.$lowerBound = 1 / $tab_DoubleUtil.$onePlusEpsilon;
	})();
	(function() {
		$tab_BrowserSupport.$selectStart = false;
		$tab_BrowserSupport.$touch = 'ontouchend' in document;
		$tab_BrowserSupport.$fonts = 'fonts' in document;
		$tab_BrowserSupport.$dataUri = false;
		$tab_BrowserSupport.$postMessage = false;
		$tab_BrowserSupport.$historyApi = false;
		$tab_BrowserSupport.$consoleLogFormatting = false;
		$tab_BrowserSupport.$cssTransformName = null;
		$tab_BrowserSupport.$cssTransitionName = null;
		$tab_BrowserSupport.$cssTranslate2d = false;
		$tab_BrowserSupport.$cssTranslate3d = false;
		$tab_BrowserSupport.$shouldUseAlternateHitStrategy = false;
		$tab_BrowserSupport.$canvasLinePattern = false;
		$tab_BrowserSupport.$isSafari = false;
		$tab_BrowserSupport.$isChrome = false;
		$tab_BrowserSupport.$isIE = false;
		$tab_BrowserSupport.$internetExplorerVersion = 0;
		$tab_BrowserSupport.$safariVersion = 0;
		$tab_BrowserSupport.$iosVersion = 0;
		$tab_BrowserSupport.$isFF = false;
		$tab_BrowserSupport.$isOpera = false;
		$tab_BrowserSupport.$isKhtml = false;
		$tab_BrowserSupport.$isWebKit = false;
		$tab_BrowserSupport.$isMozilla = false;
		$tab_BrowserSupport.$isIos = false;
		$tab_BrowserSupport.$isAndroid = false;
		$tab_BrowserSupport.$isMac = false;
		$tab_BrowserSupport.$isWindows = false;
		$tab_BrowserSupport.$devicePixelRatio = 1;
		$tab_BrowserSupport.$backingStoragePixelRatio = 1;
		$tab_BrowserSupport.$dateInput = false;
		$tab_BrowserSupport.$dateTimeInput = false;
		$tab_BrowserSupport.$dateTimeLocalInput = false;
		$tab_BrowserSupport.$timeInput = false;
		$tab_BrowserSupport.$setSelectionRange = false;
		$tab_BrowserSupport.$detectBrowser();
		$($tab_BrowserSupport.detectBrowserSupport);
	})();
	(function() {
		$tab_DomUtil.$translationFuncIndexer = ss.mkdict(['matrix', 4, 'matrix3d', 12, 'translate', 0, 'translate3d', 0, 'translateX', 0, 'translateY', -1]);
	})();
	(function() {
		$tab_ConsoleLogAppender.$globalAppender = null;
	})();
	(function() {
		$tab_ErrorTrace.$unknownFunctionName = '?';
		$tab_ErrorTrace.$shouldReThrow = false;
		$tab_ErrorTrace.$remoteFetching = true;
		$tab_ErrorTrace.$collectWindowErrors = true;
		$tab_ErrorTrace.$linesOfContext = 3;
		$tab_ErrorTrace.$getStack = false;
		$tab_ErrorTrace.$lastExceptionStack = null;
		$tab_ErrorTrace.$lastException = null;
		$tab_ErrorTrace.$sourceCache = {};
		$tab_ErrorTrace.$queuedTraces = [];
		$tab_ErrorTrace.$onErrorHandlerInstalled = false;
		$tab_ErrorTrace.$oldOnErrorHandler = null;
	})();
	(function() {
		$tab_LayoutMetrics.$defaultDb = null;
		$tab_LayoutMetrics.$metricToClientMetric = {};
		$tab_LayoutMetrics.$defaultDb = {};
		$tab_LayoutMetrics.$defaultDb['scrollbar'] = { w: 17, h: 17 };
		$tab_LayoutMetrics.$defaultDb['qfixed'] = { w: 0, h: 5 };
		$tab_LayoutMetrics.$defaultDb['qslider'] = { w: 0, h: 18 };
		$tab_LayoutMetrics.$defaultDb['qreadout'] = { w: 0, h: 20 };
		$tab_LayoutMetrics.$defaultDb['cfixed'] = { w: 0, h: 6 };
		$tab_LayoutMetrics.$defaultDb['citem'] = { w: 0, h: 18 };
		$tab_LayoutMetrics.$defaultDb['ccustomitem'] = { w: 0, h: 9 };
		$tab_LayoutMetrics.$defaultDb['hfixed'] = { w: 0, h: 21 };
		$tab_LayoutMetrics.$defaultDb['hitem'] = { w: 0, h: 18 };
		$tab_LayoutMetrics.$defaultDb['cmslider'] = { w: 0, h: 49 };
		$tab_LayoutMetrics.$defaultDb['cmdropdown'] = { w: 0, h: 29 };
		$tab_LayoutMetrics.$defaultDb['cmpattern'] = { w: 0, h: 29 };
		$tab_LayoutMetrics.$defaultDb['capply'] = { w: 0, h: 21 };
		$tab_LayoutMetrics.$defaultDb['cmtypeinsearch'] = { w: 0, h: 22 };
		$tab_LayoutMetrics.$defaultDb['rdate'] = { w: 0, h: 28 };
		if (tsConfig.is_mobile) {
			$tab_LayoutMetrics.$defaultDb['scrollbar'] = { w: 0, h: 0 };
		}
		$tab_LayoutMetrics.$metricToClientMetric['scrollbar'] = 'scrollbar-metric';
		$tab_LayoutMetrics.$metricToClientMetric['qfixed'] = 'q-filter-fixed-metric';
		$tab_LayoutMetrics.$metricToClientMetric['qslider'] = 'q-filter-slider-metric';
		$tab_LayoutMetrics.$metricToClientMetric['qreadout'] = 'q-filter-readout-metric';
		$tab_LayoutMetrics.$metricToClientMetric['cfixed'] = 'c-filter-fixed-metric';
		$tab_LayoutMetrics.$metricToClientMetric['citem'] = 'c-filter-item-metric';
		$tab_LayoutMetrics.$metricToClientMetric['capply'] = 'c-filter-apply-metric';
		$tab_LayoutMetrics.$metricToClientMetric['hfixed'] = 'h-filter-fixed-metric';
		$tab_LayoutMetrics.$metricToClientMetric['hitem'] = 'h-filter-item-metric';
		$tab_LayoutMetrics.$metricToClientMetric['cmslider'] = 'cm-slider-filter-metric';
		$tab_LayoutMetrics.$metricToClientMetric['cmdropdown'] = 'cm-dropdown-filter-metric';
		$tab_LayoutMetrics.$metricToClientMetric['cmpattern'] = 'cm-pattern-filter-metric';
		$tab_LayoutMetrics.$metricToClientMetric['rdate'] = 'r-date-filter-metric';
		$tab_LayoutMetrics.$metricToClientMetric['rdatep'] = 'r-date-p-filter-metric';
		$tab_LayoutMetrics.$metricToClientMetric['cmtypeinsearch'] = 'cm-type-in-search-metric';
		$tab_LayoutMetrics.$metricToClientMetric['ccustomitem'] = 'c-filter-custom-item-metric';
	})();
	(function() {
		$tab_MetricsLogger.$maxBeaconElementArraySize = 100;
		$tab_MetricsLogger.$maxEventBufferSize = 400;
		$tab_MetricsLogger.$maxEventsToProcess = 20;
		$tab_MetricsLogger.$defaultProcessingDelay = 250;
		$tab_MetricsLogger.$overflowProcessingDelay = 5;
		$tab_MetricsLogger.$beaconCleanupDelay = 250;
		$tab_MetricsLogger.$debugParamNames = null;
		$tab_MetricsLogger.$debugEventNames = null;
		$tab_MetricsLogger.$instance = null;
		$tab_MetricsLogger.$debugParamNames = {};
		$tab_MetricsLogger.$debugParamNames['d'] = 'DESC';
		$tab_MetricsLogger.$debugParamNames['t'] = 'TIME';
		$tab_MetricsLogger.$debugParamNames['id'] = 'ID';
		$tab_MetricsLogger.$debugParamNames['sid'] = 'SESSION_ID';
		$tab_MetricsLogger.$debugParamNames['e'] = 'ELAPSED';
		$tab_MetricsLogger.$debugParamNames['v'] = 'VALS';
		$tab_MetricsLogger.$debugParamNames['wb'] = 'WORKBOOK';
		$tab_MetricsLogger.$debugParamNames['s'] = 'SHEET_NAME';
		$tab_MetricsLogger.$debugParamNames['p'] = 'PROPS';
		$tab_MetricsLogger.$debugParamNames['m'] = 'MOBILE';
		$tab_MetricsLogger.$debugEventNames = {};
		$tab_MetricsLogger.$debugEventNames['nav'] = 'Navigation';
		$tab_MetricsLogger.$debugEventNames['wps'] = 'ProfileStart';
		$tab_MetricsLogger.$debugEventNames['wp'] = 'ProfileEnd';
		$tab_MetricsLogger.$debugEventNames['gen'] = 'Generic';
		$tab_MetricsLogger.$debugEventNames['init'] = 'SessionInit';
	})();
	(function() {
		$tab_NavigationMetricsCollector.$navigationMetricsOrder = ['navigationStart', 'unloadEventStart', 'unloadEventEnd', 'redirectStart', 'redirectEnd', 'fetchStart', 'domainLookupStart', 'domainLookupEnd', 'connectStart', 'connectEnd', 'secureConnectionStart', 'requestStart', 'responseStart', 'responseEnd', 'domLoading', 'domInteractive', 'domContentLoadedEventStart', 'domContentLoadedEventEnd', 'domComplete', 'loadEventStart', 'loadEventEnd'];
		$tab_NavigationMetricsCollector.$navMetrics = null;
		var w = window;
		var loadHandler = function(ev) {
			_.defer($tab_NavigationMetricsCollector.collectMetrics);
		};
		if (ss.isValue(w.addEventListener)) {
			window.addEventListener('load', loadHandler, false);
		}
		else if (ss.isValue(w.attachEvent)) {
			w.attachEvent('load', loadHandler);
		}
	})();
	(function() {
		$tab_StackTraceAppender.$globalAppender = null;
	})();
	(function() {
		$tab_WindowAppender.$globalAppender = null;
	})();
})();
