
//import { LayerExtensions } from 'LayerExtensions.js'

//import { SimpleMeshLayer, CubeGeometry, CylinderGeometry, Deck, FlyToInterpolator, ScatterplotLayer, BitmapLayer, ArcLayer, ColumnLayer, TextLayer, mapboxgl }
//	from "../../npm_packages/src/index"


import { default as mapboxgl } from 'mapbox-gl/dist/mapbox-gl';

import { LocationStyle, BrewerResult } from "./interfaces";

import * as dgl from "deck.gl";
import * as luma from "luma.gl";




export module deckgl {
	function throwOnGLError(err, funcName, args) {
		//@ts-ignore
		throw WebGLDebugUtils.glEnumToString(err) + " was caused by call to: " + funcName;
	};

	function logGLCall(functionName, args) {
		console.log("gl." + functionName + "(" +
			//@ts-ignore
			WebGLDebugUtils.glFunctionArgsToString(functionName, args) + ")");
	}

	const ColourValues: number[][] = [
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

	export function flyTo(longitude: number, latitude: number, zoom: number) {
		window.interop.deck.setProps({
			initialViewState: {
				longitude: longitude,
				latitude: latitude,
				zoom: (zoom > 9 ? zoom : 15),
				bearing: 0,
				pitch: 45,
				transitionInterpolator: new dgl.FlyToInterpolator({ speed: 1.5 }),
				transitionDuration: 'auto'
			}
		})
		return true;
	}

	export function initDeckGL(longitude: number, latitude: number, zoom: number) {

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

		const deck = new dgl.Deck({
			canvas: 'deck-canvas',
			width: '100%',
			height: '100%',
			initialViewState: INITIAL_VIEW_STATE,
			controller: true,
			onWebGLInitialized: (gl) => {
				// @ts-ignore
				gl = WebGLDebugUtils.makeDebugContext(gl, throwOnGLError, logGLCall);
				// @ts-ignore
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
					if (window.interop.state.layers.length == 4) {
						window.interop.setState({
							layers: [window.interop.state.layers[0], generateNewTextLayer(viewState.zoom),
							window.interop.state.layers[2], window.interop.state.layers[3]]
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
						case ("scatterplot-layer"):
							return `${hv.object.name} \r\n ${hv.object.url}`;
					}
				}
			}
		});

		const colourMap = (<any>(<LocationStyle[]>window.interop.state.map).map(a => a.styles.map(b => b.name))).flat()
			.filter((value, index, self) => self.indexOf(value) === index).map((a, b) => { return { name: a, colour: ColourValues[b] } });

		window.interop.setState({ colourMap: colourMap });

		window.interop.currentZoom = zoom;
		window.interop.mapLoaded = true;
		window.interop.map = map;
		window.interop.deck = deck;
		return true;
	}

	function generateNewTextLayer(zoom: number) {

		var mapVals: LocationStyle[] = window.interop.state.map;
		var locationData = mapVals.map((a) => {
			return { centroid: [a.location.x, a.location.y], name: a.name }
		});

		if (zoom >= 14) {
			return new dgl.TextLayer({
				id: 'text-layer' + zoom,
				data: locationData,
				pickable: true,
				billboard: true,
				getPosition: d => d.centroid,
				getText: d => d.name,
				getSize: 32,
				fontFamily: "Montserrat",
				getAngle: 0,
				getTextAnchor: 'middle',
				getAlignmentBaseline: 'center',
				getPixelOffset: (a, b) => {
					var pixelVal = pixelValue(a.centroid[1], 64, zoom);
					console.log(`Zoom is ${zoom}  -  offset is ${pixelVal}`);
					return [0, pixelVal];
				}
			})
		} else {
			return new dgl.TextLayer({});
		};
	}

	function pixelValue(latitude: number, meters: number, zoomLevel: number) {
		const mapPixels = meters / (78271.484 / 2 ** zoomLevel) / Math.cos((latitude * Math.PI) / 180);
		const screenPixel = mapPixels * Math.floor(window.devicePixelRatio);
		return screenPixel;
	}

	export function addColumnChartPoint(zoom: number) {
		if (window.interop.mapLoaded) {

			const scale = 20;

			var mapVals: LocationStyle[] = window.interop.state.map;
			var columnData: any = mapVals.map((a) => {
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
				layers: [new dgl.ColumnLayer({
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
							.then((result: BrewerResult[]) => {
								window.interop.setState({
									brewerMap: result.map(a => a.id)
								})
								let outSliceDataLength = [];
								let outSliceData = [];
								result.forEach((d: BrewerResult) => {
									let counts: Array<number> = d.beersBrewed.map(a => a.count)
									let total = 0;
									let rolling = 0;
									let last = 0;
									counts.forEach(n => { total += n; });
									//@ts-ignore
									const sliceData = counts.map((num, ind, arr) => {
										last = rolling;
										rolling += num
										if (ind == 0) {
											return [0, num / total * 360]
										}
										return [last / total * 360, rolling / total * 360]
									})
										//@ts-ignore
										.flat();
									d.angleData = sliceData;
									outSliceDataLength.push(sliceData.length);
									outSliceData.push(sliceData);
								});

								//@ts-ignore
								//const brewerLayer = new LayerExtensions.PieScatterplotLayer({
								//	id: 'piescatterplot-layer',
								//	data: result,
								//	pickable: true,
								//	opacity: 0.8,
								//	stroked: true,
								//	filled: true,
								//	radiusScale: 4,
								//	radiusMinPixels: 1,
								//	radiusMaxPixels: 100,
								//	lineWidthMinPixels: 1,

								//	getPosition: d => [d.location.x, d.location.y],
								//	getRadius: d => 48,
								//	getFillColor: d => [255, 140, 0],
								//	getLineColor: d => [0, 0, 0],
								//	getBrewerIndex: d => {
								//		return window.interop.state.brewerMap.indexOf(d.id);
								//	},
								//	getStartIndex: //d => d.beersBrewed.map(a => a.count)
								//		function (d: BrewerResult) {
								//			var index = window.interop.state.brewerMap.indexOf(d.id);
								//			let startIndex = 0;
								//			for (var i = 0; i < index; i++) {
								//				startIndex += outSliceDataLength[i]
								//			}
								//			return startIndex;
								//		},
								//	getEndIndex: //d => d.beersBrewed.map(a => a.count)
								//		function (d: BrewerResult) {
								//			var index = window.interop.state.brewerMap.indexOf(d.id);
								//			let endIndex = 0;
								//			for (var i = 0; i <= index; i++) {
								//				endIndex += outSliceDataLength[i]
								//			}
								//			return endIndex - 1;
								//		},
								//	//getAngleData:
								//	//	(d: BrewerResult) => [0, 36, 36, 288, 288, 324, 324, 360],//.angleData,
								//	getAngleNumber: (d: BrewerResult) => d.angleData.length,//4,//d.angleData.length,

								//	outSliceLength: outSliceDataLength, //[8]
								//	//@ts-ignore
								//	outSliceData: outSliceData.flat(), //[0, 36, 36, 288, 288, 324, 324, 360],
								//	breweryCount: outSliceDataLength.length, //1
								//	//@ts-ignore
								//	totalLength: outSliceData.flat().length, //8
								//});

								//@ts-ignore
								window.interop.setState({
									uniformData: {
										//@ts-ignore
										//colourMap: ColourValues.map( a => [a[0]/255, a[1]/255, a[2]/255]).flat()
										colourMap: [
											1.0, 0.0, 0.0,
											0.0, 1.0, 0.0,
											0.0, 0.0, 1.0,
											1.0, 0.5, 0.0,
											0.0, 1.0, 0.0,
											0.0, 0.0, 1.0,
											1.0, 0.0, 0.0,
											0.0, 1.0, 0.0,
											0.0, 0.0, 1.0,
											1.0, 0.0, 0.0,
											0.0, 1.0, 0.0,
											0.0, 0.0, 1.0
										]
									}
								});
								const arcLayer = new dgl.ArcLayer({
									id: 'arc-layer',
									data: result,
									pickable: false,
									getWidth: 4,
									getSourcePosition: [a.object.centroid[0], a.object.centroid[1]],
									getTargetPosition: d => [d.location.x, d.location.y],
									getSourceColor: a.object.colour,//d => [255, 214, 0],
									getTargetColor: a.object.colour,
								});

								const pieChartLayer = new dgl.SimpleMeshLayer({
									id: 'piechart-layer',
									data: result,
									texture: '/img/texture.png',
									mesh: new luma.CylinderGeometry({ radius: 5, height: 1, topCap: true, nradial: 48, bottomCap: false }),
									sizeScale: 4,
									getPosition: d => [d.location.x, d.location.y],
									getColor: d => [255, 214, 0],
									getOrientation: d => [0, 0, 270]
								})
								window.interop.setState({
									layers: [window.interop.state.layers[0], window.interop.state.layers[1], arcLayer, pieChartLayer]
								})
								window.interop.deck.setProps({
									layers: window.interop.state.layers
								});
								//window.interop.deck.update();

							});
					}
					//onHover: ({ x, y, object }) => setTooltip(x, y, object ? `${object.name}\n${object.address}` : null)
				}), generateNewTextLayer(zoom)]
			});

			window.interop.deck.setProps({
				layers: window.interop.state.layers
			});
			return true;
		}
		return false;
	}
} 

