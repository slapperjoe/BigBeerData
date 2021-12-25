import * as mapboxgl from "mapbox-gl"
import { TextLayer, ColumnLayer, FlyToInterpolator, ArcLayer, SimpleMeshLayer, Deck } from "deck.gl"
//import { CylinderGeometry } from "luma.gl"
import { Texture2D, CylinderGeometry } from "@luma.gl/core"

window.interop = {
	dotNet: null,
	state: {
		_revision: 1,
		map: [],
		_previousState: null,
		colourMap: [],
		layers: [],
		brewerMap: [],
		mapLoaded: true,
		deck: null,
		currentZoom: 0,
		//layerSet: null,
		longitude: 0,
		latitude: 0,
		label: ""
	},
	setState: (stateObj) => {
		delete window.interop.state._previousState;
		window.interop.state._previousState = Object.assign({}, window.interop.state);
		window.interop.state = Object.assign(Object.assign(Object.assign({}, window.interop.state), stateObj), { _revision: (window.interop.state._revision + 1) });
		//return this.state;
		return window.interop.state;
		//fire change notifier
	},
	getDimensions: () => {
		return {
			width: window.innerWidth,
			height: window.innerHeight
		};
	}
};

window.interop.getRenderArea = () => {
	var renderArea = document.getElementById('renderArea');
	if (renderArea) {
		return {
			width: renderArea.clientWidth,
			height: renderArea.clientHeight
		};
	}
};
window.interop.consoleLog = (textString) => {
	window.console.log(textString);
	return true;
};
window.interop.hookDotNet = (dotNetObj) => {
	window.interop.dotNet = dotNetObj;
};

function throwOnGLError(err, funcName, args) {
	//@ts-ignore
	throw WebGLDebugUtils.glEnumToString(err) + " was caused by call to: " + funcName;
};

function logGLCall(functionName, args) {
	console.log("gl." + functionName + "(" +
		//@ts-ignore
		WebGLDebugUtils.glFunctionArgsToString(functionName, args) + ")");
}

const ColourValues = [
	/*[255, 0, 0], */[0, 255, 0], [0, 0, 255], [255, 255, 0], [255, 0, 255], [0, 255, 255], /*[0, 0, 0],*/
	[192, 64, 0], [64, 192, 0], [64, 192, 0], [192, 64, 0], [64, 0, 192], [192, 0, 64], [192, 192, , 64], [64, 192, 192], [192.64, 192], [64, 192, 64], [64, 192, 192], [192, 192, 64],
	[128, 0, 0], [0, 128, 0], [0, 0, 128], [128, 128, 0], [128, 0, 128], [0, 128, 128], [128, 128, 128],
	[192, 0, 0], [0, 192, 0], [0, 0, 192], [192, 192, 0], [192, 0, 192], [0, 192, 192], [192, 192, 192],
	[64, 0, 0], [0, 64, 0], [0, 0, 64], [64, 64, 0], [64, 0, 64], [0, 64, 64], [64, 64, 64],
	[32, 0, 0], [0, 32, 0], [0, 0, 32], [32, 32, 0], [32, 0, 32], [0, 32, 32], [32, 32, 32],
	[96, 0, 0], [0, 96, 0], [0, 0, 96], [96, 96, 0], [96, 0, 96], [0, 96, 96], [96, 96, 96],
	[160, 0, 0], [0, 160, 0], [0, 0, 160], [160, 160, 0], [160, 0, 160], [0, 160, 160], [160, 160, 160],
	[224, 0, 0], [0, 224, 0], [0, 0, 224], [224, 224, 0], [224, 0, 224], [0, 224, 224], [224, 224, 224]
];

window.interop.FlyTo = (longitude, latitude, zoom) => {
	window.interop.deck.setProps({
		initialViewState: {
			longitude: longitude,
			latitude: latitude,
			zoom: (zoom > 9 ? zoom : 15),
			bearing: 0,
			pitch: 45,
			transitionInterpolator: new FlyToInterpolator({ speed: 1.5 }),
			transitionDuration: 'auto'
		}
	})
	return true;
}

window.interop.SetMapState = (...styles) => {
	//@ts-ignore
	var jim = window.interop.setState({ label: "Bpb", map: styles, layers: [] });
	debugger;
	console.log(jim);
	return jim;
}

window.interop.InitDeckGL = (longitude, latitude, zoom) => {

	const INITIAL_VIEW_STATE = {
		latitude: latitude,
		longitude: longitude,
		zoom: zoom,
		bearing: 0,
		pitch: 45
	};

	mapboxgl.accessToken = 'pk.eyJ1IjoibWFyaWMxIiwiYSI6Ii0xdWs1TlUifQ.U56tiQG_kj88zNf_1PxHQw';// process.env.MapboxAccessToken; // eslint-disable-line

	const map = new mapboxgl.Map({
		container: 'map',
		style: 'mapbox://styles/maric1/ckclqelzf0fo71ipirav7fckc',
		interactive: false,
		center: [INITIAL_VIEW_STATE.longitude, INITIAL_VIEW_STATE.latitude],
		zoom: INITIAL_VIEW_STATE.zoom,
		bearing: INITIAL_VIEW_STATE.bearing,
		pitch: INITIAL_VIEW_STATE.pitch
	});

	const deck = new Deck({
		canvas: 'deck-canvas',
		width: '100%',
		height: '100%',
		initialViewState: INITIAL_VIEW_STATE,
		controller: true,
		onWebGLInitialized: (gl) => {
			gl = WebGLDebugUtils.makeDebugContext(gl, throwOnGLError, logGLCall);
			window.deckGLContext = gl;
		},
		onViewStateChange: ({ viewState, interactionState, oldViewState }) => {
			//@ts-ignore
			map.jumpTo({
				center: [viewState.longitude, viewState.latitude],
				zoom: viewState.zoom,
				bearing: viewState.bearing,
				pitch: viewState.pitch
			});
			if (interactionState.isZooming) {
				if (window.interop.state.layers.length >= 2) {
					let newLayers = [...window.interop.state.layers]
					newLayers[1] = generateNewTextLayer(viewState.zoom);
					window.interop.setState({
						layers: newLayers
					})
				}
				else {
					window.interop.setState({ layers: [window.interop.state.layers[0], generateNewTextLayer(viewState.zoom)] })
				}
				window.interop.deck.setProps({
					layers: window.interop.state.layers
				});
			}
		},
		layers: [],
		log: {
			level: 1
		},
		getTooltip: (hv) => {
			if (hv.object) {
				switch (hv.layer.id) {
					case ("column-layer"):
						return `${hv.object.venuename}\r\n${hv.object.name} - ${hv.object.value}`;					
				}
				if (hv.layer.id.indexOf('piechart-layer') == 0) {
					console.log(hv);
					return "bob";
                }
			}
		}
	});

	const colourMap = ((window.interop.state.map).map(a => a.styles.map(b => b.name))).flat()
		.filter((value, index, self) => self.indexOf(value) === index).map((a, b) => { return { name: a, colour: ColourValues[b] } });

	window.interop.setState({ colourMap: colourMap });

	window.interop.currentZoom = zoom;
	window.interop.mapLoaded = true;
	window.interop.map = map;
	window.interop.deck = deck;
	return true;
}

window.interop.AddColumnChartPoint = (zoom) => {
	if (window.interop.mapLoaded) {

		const scale = 20;

		var mapVals = window.interop.state.map;
		var columnData = mapVals.map((a) => {
			return a.styles.map(b => {
				return {
					centroid: [a.location.x, a.location.y, b.height * scale],
					value: b.count,
					name: b.name,
					colour: window.interop.state.colourMap.find(c => c.name === b.name)?.colour,
					venue: a.venue,
					venuename: a.name
				}
			})
		});
		var flatData = columnData.flat();

		window.interop.setState({
			layers: [new ColumnLayer({
				id: 'column-layer',
				data: flatData,
				diskResolution: 48,
				radius: 50,
				extruded: true,
				autoHighlight: true,
				pickable: true,
				elevationScale: scale,
				getPosition: d => d.centroid,
				getFillColor: d => [...(d.colour ? d.colour : [128, 128, 128]), 192],
				getLineColor: [0, 0, 0],
				getElevation: d => d.value,
				onClick: (a) => {
					window.interop.dotNet.invokeMethodAsync('GetBrewersByVenue', a.object.venue, a.object.name)
						.then((result) => {
							window.interop.setState({
								brewerMap: result.map(a => a.id)
							})
								
							const arcLayer = new ArcLayer({
								id: 'arc-layer',
								data: result,
								pickable: false,
								getWidth: 4,
								getSourcePosition: [a.object.centroid[0], a.object.centroid[1]],
								getTargetPosition: d => [d.location.x, d.location.y],
								getSourceColor: a.object.colour,//d => [255, 214, 0],
								getTargetColor: a.object.colour,
							});

							const pieChartLayers = result.map(item => new SimpleMeshLayer({
								id: 'piechart-layer-' + item.name,
								data: [item],
								texture: new Promise((resolve, reject) => {
									let dataArray = [];
									item.beersBrewed.forEach((bb, i) => {
										for (var j = 0; j < bb.count; j++) {
											dataArray.push(ColourValues[i])
										}
									});
									const texture = new Texture2D(window.deckGLContext, {
										width: item.beersBrewed.flatMap(a => a.count).reduce((a, b) => a + b, 0),
										height: 1,
										format: window.deckGLContext.RGB,
										data: new Uint8Array(dataArray.flat()),
										parameters: {
											[window.deckGLContext.TEXTURE_MAG_FILTER]: window.deckGLContext.NEAREST,
											[window.deckGLContext.TEXTURE_MIN_FILTER]: window.deckGLContext.NEAREST
										},
										pixelStore: {
											[window.deckGLContext.UNPACK_FLIP_Y_WEBGL]: true
										},
										mipmaps: true
									});
									resolve(texture);
								}),
								
								mesh: new CylinderGeometry({ radius: 5, height: 1, topCap: true, nradial: 48, bottomCap: false }),
								sizeScale: 16,
								getPosition: d => [d.location.x, d.location.y],
								getColor: d => [255, 214, 0],
								getOrientation: d => [0, 0, 270]
							}))

							const pieLabelLayer = new TextLayer({
								id: 'pie-text-layer' + zoom,
								data: result,
								//pickable: true,
								//billboard: true,
								getPosition: d => [d.location.x, d.location.y],
								getText: d => d.name,
								getSize: 24,
								getAngle: 0,
								getTextAnchor: 'middle',
								getAlignmentBaseline: 'center',
								fontFamily: "Montserrat",
								getPixelOffset: (a, b) => {
									var pixelVal = pixelValue(a.location.y, 64, zoom);
									return [0, pixelVal];
								}
							});
							window.interop.setState({
								layers: [window.interop.state.layers[0], window.interop.state.layers[1], arcLayer, pieChartLayers, pieLabelLayer].flat()
							})
							window.interop.deck.setProps({
								layers: window.interop.state.layers
							});
							//window.interop.deck.update();

						});
				}
				//onHover: ({x, y, object}) => setTooltip(x, y, object ? `${object.name}\n${object.address}` : null)
			}), generateNewTextLayer(zoom)]
		});

		window.interop.deck.setProps({
			layers: window.interop.state.layers
		});
		return true;
	}
	return false;
}


function generateNewTextLayer(zoom) {

	var mapVals = window.interop.state.map;
	var locationData = mapVals.map((a) => {
		return { centroid: [a.location.x, a.location.y], name: a.name }
	});

	let fontSize = (zoom * 3) - 10;
	
	if (zoom >= 14) {
		fontSize = 32;
	}
	console.log(fontSize);
	return new TextLayer({
		id: 'text-layer' + zoom,
		data: locationData,
		pickable: true,
		billboard: true,
		getPosition: d => d.centroid,
		getText: d => d.name,
		getSize: fontSize,
		getAngle: 0,
		getTextAnchor: 'middle',
		getAlignmentBaseline: 'center',
		fontFamily: "Montserrat",
		getPixelOffset: (a, b) => {
			var pixelVal = pixelValue(a.centroid[1], 64, zoom);
			//console.log(`Zoom is ${zoom}  -  offset is ${pixelVal}`);
			return [0, pixelVal];
		}
	});
}

function pixelValue(latitude, meters, zoomLevel) {
	const mapPixels = meters / (78271.484 / 2 ** zoomLevel) / Math.cos((latitude * Math.PI) / 180);
	const screenPixel = mapPixels * Math.floor(window.devicePixelRatio);
	return screenPixel;
}

