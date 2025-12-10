import * as mapboxgl from "mapbox-gl"
import { TextLayer } from "@deck.gl/layers/typed"
import { FlyToInterpolator, Deck } from '@deck.gl/core/typed';
import * as oneChart from "./oneChart";

window.interop = {
	dotNet: null,
	deck: null,
	mapDiv: null,
	state: {
		_revision: 1,
		map: [],
		_previousState: null,
		colourMap: [],
		//layers: [],
		brewerMap: [],
		mapLoaded: true,
		currentZoom: 0,
		//layerSet: null,
		longitude: 0,
		latitude: 0,
		label: "",
		viewingVenue: true,
		uniformData: {
			colourMap: []
		},
		selectedVenue: null,
	},
	setState: (stateObj) => {
		delete globalThis.interop.state._previousState;
		globalThis.interop.state._previousState = { ...globalThis.interop.state };
		globalThis.interop.state = { ...globalThis.interop.state, ...stateObj, _revision: (globalThis.interop.state._revision + 1) };
		if (globalThis.interop.dotNet) {
			//globalThis.interop.dotNet.invokeMethodAsync('SetState', globalThis.interop.state);
		}
		return globalThis.interop.state;
	},
	getDimensions: () => {
		return {
			width: window.innerWidth,
			height: window.innerHeight
		};
	},
	getRenderArea: () => {
		const renderArea = document.getElementById('renderArea');
		if (renderArea) {
			return {
				width: renderArea.clientWidth,
				height: renderArea.clientHeight
			};
		}
	},
	consoleLog: (textString) => {
		globalThis.console.log(textString);
		return true;
	},
	hookDotNet: (dotNetObj) => {
		globalThis.interop.dotNet = dotNetObj;
	},
	FlyTo: (longitude, latitude, zoom) => {
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
	},
	SetMapState: (...styles) => {
		const locations = styles[0] as any[];
		console.log(`[BBD] SetMapState called with ${locations?.length || 0} locations`);
		//@ts-ignore
		window.interop.setState({ map: styles, layers: [] });
	},
	InitDeckGL: (longitude, latitude, zoom) => {

		const INITIAL_VIEW_STATE = {
			latitude: latitude,
			longitude: longitude,
			zoom: zoom,
			bearing: 0,
			pitch: 80
		};
		console.log(`[BBD] InitDeckGL: long=${longitude}, lat=${latitude}, zoom=${zoom}`);

		(mapboxgl as any).accessToken = 'pk.eyJ1IjoibWFyaWMxIiwiYSI6Ii0xdWs1TlUifQ.U56tiQG_kj88zNf_1PxHQw';// process.env.MapboxAccessToken; // eslint-disable-line

		const map = new mapboxgl.Map({
			container: 'map',
			style: 'mapbox://styles/maric1/ckclqelzf0fo71ipirav7fckc',
			interactive: true,
			center: [INITIAL_VIEW_STATE.longitude, INITIAL_VIEW_STATE.latitude],
			zoom: INITIAL_VIEW_STATE.zoom,
			bearing: INITIAL_VIEW_STATE.bearing,
			pitch: INITIAL_VIEW_STATE.pitch
		});


		map.addControl(new mapboxgl.FullscreenControl());

		const deck = new Deck({
			canvas: 'deck-canvas',
			width: '100%',
			height: '100%',
			initialViewState: INITIAL_VIEW_STATE,
			controller: true,
			onWebGLInitialized: (gl) => {
				// @ts-ignore
				//gl = WebGLDebugUtils.makeDebugContext(gl, throwOnGLError, logGLCall);
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
					let newLayers = [];
					if (window.interop.deck.props.layers.length > 2) {
						newLayers = [...window.interop.deck.props.layers]
						newLayers[1] = generateNewTextLayer(viewState.zoom, window.interop.state.map, 'text-layer', 64);
						newLayers[window.interop.deck.props.layers.length - 1] = generateNewTextLayer(viewState.zoom, window.interop.state.brewerMap, 'pie-text-layer', 128);
						window.interop.setState({
							viewingVenue: true
						})
					}
					else {
						newLayers = [window.interop.deck.props.layers[0], generateNewTextLayer(viewState.zoom, window.interop.state.map, 'text-layer', 64)],
							window.interop.setState({
								viewingVenue: true
							})
					}
					window.interop.deck.setProps({
						layers: newLayers
					});
				}
			},
			layers: [],
			//log: {
			//    level: 1
			//},
			getTooltip: (hv) => {
				if (hv.object) {
					switch (hv.layer.id) {
						case ("column-layer"):
							return `${hv.object.venuename}\r\n${hv.object.name} - ${hv.object.value}`;
						case ("arc-layer"):
							return hv.object.name;
						default:
							if (hv.layer.id.indexOf('piechart-layer') == 0) {
								return {
									html:
										`<div><span>Beers:</span><table>${hv.object.beersBrewed.map(bb => createLabelRow(bb)).join('')}</table></div>`,
									style: {
										fontSize: '0.8em'
									}
								}
							}
							break;
					}
				}
			}
		});

		const colourMap = ((window.interop.state.map).map(a => a.styles.map(b => b.name))).flat()
			.filter((value, index, self) => self.indexOf(value) === index).map((a, b) => { return { name: a, colour: ColourValues[b] } });

		window.interop.setState({ colourMap: colourMap, currentZoom: zoom, mapLoaded: true });
		window.interop.deck = deck;
		window.interop.mapDiv = map;
		console.log("[BBD] InitDeckGL: Deck and Map initialized");
		return true;
	},
	AddColumnChartPoint: (zoom) => {
		if (!globalThis.interop.state.mapLoaded) {
			return false;
		}

		const scale = 20;
		console.log(`[BBD] AddColumnChartPoint: Building column data for ${globalThis.interop.state.map.length} locations`);
		const columnData = oneChart.buildColumnData(globalThis.interop.state.map, globalThis.interop.state.colourMap, scale);
		console.log(`[BBD] AddColumnChartPoint: Generated ${columnData.length} columns`);
		const columnLayer = oneChart.createColumnLayer(columnData, scale, (selected) => {
			if (!globalThis.interop.dotNet) {
				return;
			}
			globalThis.interop.dotNet.invokeMethodAsync('GetBrewersByVenue', selected.venue, selected.name)
				.then((result) => {
					globalThis.interop.setState({
						brewerMap: result,
						selectedVenue: selected
					});

					const arcLayer = oneChart.createArcLayer(selected, result, {
						currentZoom: globalThis.interop.state.currentZoom,
						viewingVenue: globalThis.interop.state.viewingVenue,
						flyTo: globalThis.interop.FlyTo,
						toggleViewing: (next) => globalThis.interop.setState({ viewingVenue: next })
					});

					const pieChartLayers = oneChart.createPieChartLayers(result, globalThis.deckGLContext as WebGLRenderingContext, oneChart.ColourValues);
					const pieLabelLayer = generateNewTextLayer(zoom, globalThis.interop.state.brewerMap, 'pie-text-layer', 128);
					const layers = [globalThis.interop.deck.props.layers[0], globalThis.interop.deck.props.layers[1],
						arcLayer, ...pieChartLayers, pieLabelLayer];
					globalThis.interop.setState({ viewingVenue: false });
					globalThis.interop.deck.setProps({
						layers: layers
					});

				});
		});

		globalThis.interop.deck.setProps({
			layers: [columnLayer, generateNewTextLayer(zoom, globalThis.interop.state.map, 'text-layer', 64)]
		});
		return true;
	},
	RefreshImage: async (imageElementId, url) => {
		debugger;
		const image: HTMLElement = document.getElementById(imageElementId);
		image.onload = () => {
			URL.revokeObjectURL(url);
		}
		(<HTMLImageElement>image).src = url;
	},
	ShowLoadBox: (element) => {
		element.style.display = "block"
	},
	HideLoadBox: (element) => {
		element.style.display = "none"
	}
};

window.addEventListener('resize', function () {
	if (window.interop.dotNet) {
		window.interop.dotNet.invokeMethodAsync('GetMainArea', true);
	}
});

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
	[192, 64, 0], [64, 192, 0], [64, 192, 0], [192, 64, 0], [64, 0, 192], [192, 0, 64], [192, 192, 64],
	[64, 192, 192], [192.64, 192], [64, 192, 64], [64, 192, 192], [192, 192, 64],
	[128, 0, 0], [0, 128, 0], [0, 0, 128], [128, 128, 0], [128, 0, 128], [0, 128, 128], [128, 128, 128],
	[192, 0, 0], [0, 192, 0], [0, 0, 192], [192, 192, 0], [192, 0, 192], [0, 192, 192], [192, 192, 192],
	[64, 0, 0], [0, 64, 0], [0, 0, 64], [64, 64, 0], [64, 0, 64], [0, 64, 64], [64, 64, 64],
	[32, 0, 0], [0, 32, 0], [0, 0, 32], [32, 32, 0], [32, 0, 32], [0, 32, 32], [32, 32, 32],
	[96, 0, 0], [0, 96, 0], [0, 0, 96], [96, 96, 0], [96, 0, 96], [0, 96, 96], [96, 96, 96],
	[160, 0, 0], [0, 160, 0], [0, 0, 160], [160, 160, 0], [160, 0, 160], [0, 160, 160], [160, 160, 160],
	[224, 0, 0], [0, 224, 0], [0, 0, 224], [224, 224, 0], [224, 0, 224], [0, 224, 224], [224, 224, 224]
];


function generateNewTextLayer(zoom, dataSet, layerName, offset) {

	var locationData = dataSet.map((a) => {
		return { centroid: [a.location.x, a.location.y], name: a.name }
	});

	let fontSize = (zoom * 3) - 10;

	if (zoom >= 14) {
		fontSize = 32;
	}
	//console.log(fontSize);
	return new TextLayer({
		id: layerName + '-' + zoom,
		data: locationData,
		pickable: false,
		billboard: false,
		getPosition: d => d.centroid,
		getText: d => d.name,
		getSize: fontSize,
		getAngle: 0,
		getTextAnchor: 'middle',
		getAlignmentBaseline: 'center',
		fontFamily: "Helvetica Neue",
		getPixelOffset: (a, b) => {
			var pixelVal = pixelValue(a.centroid[1], offset, zoom);
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

function createLabelRow(beerBrewed) {
	const colSet = `rgb(${beerBrewed.color[0]},${beerBrewed.color[1]},${beerBrewed.color[2]})`
	return `<tr className='beer-row'>
    <td>
      <div className="mud-elevation-0 d-flex justify-center align-center" style="height: 24px; width: 24px;">
        <svg className="mud-icon-root mud-svg-icon mud-light-text mud-icon-size-medium" focusable="false" viewBox="0 0 24 24" aria-hidden="true">
          <path d="M0 0h24v24H0z" fill="none"></path>
          <path d="M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2z" fill="${colSet}"></path>
        </svg>
      </div>
    </td>
    <td>${beerBrewed.name}</td>
    <td>${beerBrewed.count}</td>
  </tr>`
}
