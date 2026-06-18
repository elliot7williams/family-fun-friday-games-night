<!DOCTYPE html>
<html>
	<head>
		<title>Google Maps Output</title>
		<meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">
		<meta charset="utf-8">
		<style>
			html, body, #mapContainer {
				margin: 0;
				padding: 0;
				height: 100%;
			}
			#mapContainer {
				width: 100%; 
				text-align: center;
			}
			a {
				color: #06C; 
				text-decoration: none;
			}
			a:visited {
				color: #06C;
			}
		</style>
		<script type="text/javascript" src="https://maps.googleapis.com/maps/api/js?v=3&sensor=false"></script>
		<script type="text/javascript">
		//<![CDATA[

		// send a cross domain iframe mesage to the parent to resize the frame to this height
		var resizeMessage = function(event) {
			var imageHeight = document.getElementById('mapContainer').clientHeight;

			window.parent.postMessage(
				imageHeight,
				'*'
			);  
		};

		function gLoad() {
			var touchSupported = (('ontouchstart' in window) || window.DocumentTouch && document instanceof DocumentTouch),
				forceTouch = false;

			var myLatLng = new google.maps.LatLng(44.1695748, -77.3727328);
			var mapOptions = {
				scrollwheel: false,
				center: myLatLng,
				zoom: 15,
				panControl: false, zoomControl: true, zoomControlOptions: { style: google.maps.ZoomControlStyle.SMALL },
				overviewMapControl: false,
				scaleControl: false,
				mapTypeControl: false,
				streetViewControl: false,
				mapTypeId: google.maps.MapTypeId.ROADMAP
			};

			if (touchSupported || forceTouch) {
				mapOptions.draggable = false;
				mapOptions.panControl = false;
				mapOptions.scrollwheel = false;
				mapOptions.zoomControlOptions = { style: google.maps.ZoomControlStyle.SMALL };
				mapOptions.zoomControl = true;
			}

			var map = new google.maps.Map(document.getElementById("map"), mapOptions);

			var point = new google.maps.Marker({position: myLatLng, map: map});
		}

		//]]>
		</script>

	</head>
	<body onload="gLoad();" style="margin: 0; padding: 0;">

		<div id="mapContainer">
			<div id="map" style="height: 150px; max-width: 100%; "> </div>
		</div>

	</body>
</html>
