import * as mapboxgl from "mapbox-gl"
import { TextLayer } from "@deck.gl/layers"
import { FlyToInterpolator, Deck } from '@deck.gl/core';
import * as oneChart from "./oneChart";

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

// Inject CSS for Dark Mode Popup, Padding, Z-Index, and Font
const style = document.createElement('style');
style.innerHTML = `
  .mapboxgl-popup-content {
    background: #16181be0 !important;
    color: #c8c8c8ff !important; /* High contrast text */
    border: 1px solid #444;
    padding: 15px 25px 10px 15px !important; /* Right padding for close button */
    z-index: 10000 !important;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif !important;
  }
  .mapboxgl-popup-tip {
    border-top-color: #000 !important;
    border-bottom-color: #000 !important;
  }
  .mapboxgl-popup-close-button {
    color: #fff !important; /* White close button */
    font-size: 16px;
    padding: 5px;
  }
  .mapboxgl-popup-close-button:hover {
    background-color: #333 !important;
  }
  .mapboxgl-popup {
    z-index: 10000 !important;
  }
  
  /* Loading Overlay */
  .bbd-loading-overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(22, 24, 27, 0.8); /* Semi-transparent dark bg */
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 20000; /* Above everything */
    pointer-events: all;
  }
  
  .bbd-loading-spinner {
    width: 100px;
    height: 100px;
    animation: bbd-spin 1.2s linear infinite;
  }
  
  @keyframes bbd-spin {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
  }
`;
document.head.appendChild(style);

window.interop = {
	popup: null, // Track active popup
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
		defaultZoom: 16, // Default fallback
		//layerSet: null,
		longitude: 0,
		latitude: 0,
		label: "",
		viewingVenue: true,
		uniformData: {
			colourMap: []
		},
		selectedVenue: null,
		useSolidPies: true, // Default to true as per "real 3D segments" request
	},
	// ...
	InitDeckGL: (longitude, latitude, zoom) => {
		try {
			console.log("[BBD] InitDeckGL: Called");
			// Show loader immediately
			window.interop.ShowLoadBox(null);

			if (window.interop.deck) {
				console.log("[BBD] InitDeckGL: Deck already initialized, skipping");
				return true;
			}

			let lat = Number(latitude);
			let lon = Number(longitude);
			let z = Number(zoom);

			if (isNaN(lat)) { console.warn("[BBD] InitDeckGL: Invalid latitude, defaulting to 0"); lat = 0; }
			if (isNaN(lon)) { console.warn("[BBD] InitDeckGL: Invalid longitude, defaulting to 0"); lon = 0; }
			if (isNaN(z)) { console.warn("[BBD] InitDeckGL: Invalid zoom, defaulting to 1"); z = 1; }

			// Store default zoom
			window.interop.state.defaultZoom = z;
			window.interop.state.currentZoom = z;

			console.log(`[BBD] InitDeckGL: Initializing at ${lon}, ${lat}, ${z}`);

			const INITIAL_VIEW_STATE = {
				latitude: lat,
				longitude: lon,
				zoom: z,
				bearing: 0,
				pitch: 80
			};

			(mapboxgl as any).accessToken = 'pk.eyJ1IjoibWFyaWMxIiwiYSI6Ii0xdWs1TlUifQ.U56tiQG_kj88zNf_1PxHQw';

			const map = new mapboxgl.Map({
				container: 'map',
				style: 'mapbox://styles/maric1/ckclqelzf0fo71ipirav7fckc',
				center: [lon, lat],
				zoom: z,
				bearing: 0,
				pitch: 80,
				interactive: false
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
						//window.interop.deck.setProps({layers: newLayers });
					}
					window.interop.setState({
						currentZoom: viewState.zoom
					});
				},
				layers: [],
				//log: {
				//    level: 1
				//},
				getTooltip: (hv) => {
					if (hv.object) {
						// console.log(`[BBD] Tooltip: Layer=${hv.layer.id}`, hv.object);
						switch (hv.layer.id) {
							case ("column-layer"):
								return `${hv.object.venuename}\r\n${hv.object.name} - ${hv.object.value}`;
							case ("arc-layer"):
								return hv.object.name;
							case ("solid-pie-layer"):
								return `Style: ${hv.object.name} - ${hv.object.count}`;
							case ("sunburst-layer-body"): // Updated ID
								// object is the d3 node, so data is in object.data
								return `${hv.object.data.name}\r\nCount: ${hv.object.value}`;
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
						}
					}
				},
				onClick: (info) => {
					// Handle Sticky Tooltips via Popup
					if (info.object && info.layer.id === 'solid-pie-layer') {
						console.log("3D Pie Click", info);
						window.interop.ShowPopup(info.object.position, `Style: ${info.object.name} - ${info.object.count}`);
						return true;
					}
					// Sunburst Click
					if (info.object && info.layer.id === 'sunburst-layer') {
						console.log("Sunburst Click", info);
						// Optional: Center view or filter?
						return true;
					}

					if (info.object && info.layer.id.indexOf('piechart-layer') == 0) {
						console.log("Pie Click", info);
						clickPie(info.object);
						return true;
					}

					if (info.object && info.layer.id == 'arc-layer') {
						// console.log("Arc Click", info);
						// toggleNavigation();
						// Prevent default?
					}

					return false;
				}
			});

			// Add Click Handler for Arc Layer specifically if needed,
			// or rely on layer onClick. The deck instance handles it.

			window.interop.deck = deck;
			window.interop.mapDiv = map;

			globalThis.interop.dotNet.invokeMethodAsync('GetEstablishmentData')
				.then(data => {
					console.log("[BBD] GetEstablishmentData Init", data);

					// Transform data for column layer
					const scale = 20;

					// 1. Generate Colour Map (Fix: use modulo to prevent index out of bounds)
					// Handle cases where data is empty or styles missing structure
					if (!data) data = [];
					const styleBlock = data.map(a => a.styles || []).flat();
					const uniqueStyles = (styleBlock.map(b => b.name || "")).flat()
						.filter((value, index, self) => value && self.indexOf(value) === index);

					console.log(`[BBD] Found ${uniqueStyles.length} unique styles for coloring.`);

					const colourMap = uniqueStyles.map((styleName, index) => {
						return {
							name: styleName,
							colour: ColourValues[index % ColourValues.length]
						};
					});

					// Debug output for colors
					if (colourMap.length > 0) {
						console.log(`[BBD] Colour Map Sample: ${colourMap[0].name} = ${colourMap[0].colour}`);
					}

					window.interop.setState({ map: data, colourMap: colourMap, mapLoaded: true });

					// 2. Build Column Data
					const columnData = oneChart.buildColumnData(data, colourMap, scale);

					// 3. Define Interactions
					const onColumnClick = (selected) => {
						console.log("Column Click", selected);
						if (!globalThis.interop.dotNet) return;

						globalThis.interop.dotNet.invokeMethodAsync('GetBrewersByVenue', selected.venue, selected.name)
							.then((result) => {
								globalThis.interop.setState({
									brewerMap: result,
									selectedVenue: selected
								});

								const arcLayer = oneChart.createArcLayer(selected, result, {
									currentZoom: Number(globalThis.interop.state.currentZoom),
									viewingVenue: globalThis.interop.state.viewingVenue,
									flyTo: globalThis.interop.FlyTo,
									toggleViewing: (next) => globalThis.interop.setState({ viewingVenue: next })
								});

								// Toggle between implementations
								const pieChartLayers = globalThis.interop.state.useSolidPies
									? oneChart.createSolidPieChartLayers(result, ColourValues)
									: oneChart.createPieChartLayers(result, globalThis.deckGLContext as WebGLRenderingContext, ColourValues);

								const pieLabelLayer = generateNewTextLayer(z, globalThis.interop.state.brewerMap, 'pie-text-layer', 128);

								// Base layers
								const baseColumnLayer = oneChart.createColumnLayer(columnData, scale, onColumnClick);
								const baseTextLayer = generateNewTextLayer(z, data, 'text-layer', 64);

								const layers = [
									// oneChart.createSunburstLayer(...), // Maybe?
									baseColumnLayer,
									baseTextLayer,
									arcLayer,
									...[pieChartLayers].flat(),
									pieLabelLayer
								];

								globalThis.interop.setState({ viewingVenue: false });
								window.interop.deck.setProps({ layers: layers });
							});
					};

					const columnLayer = oneChart.createColumnLayer(columnData, scale, onColumnClick);
					const textLayer = generateNewTextLayer(z, data, 'text-layer', 64);

					// 4. Initial Render
					window.interop.deck.setProps({
						layers: [columnLayer, textLayer]
					});

					// Hide loader
					window.interop.HideLoadBox(null);
				});


			return true;
		} catch (e) {
			console.error("[BBD] CRITICAL ERROR in InitDeckGL:", e);
			throw e; // Rethrow to see if it propagates
		}
	},
	// ...
	ShowSunburst: () => {
		// Toggle        // Check if layer exists (Look for the BODY layer now)
		const currentLayers = window.interop.deck.props.layers || [];
		const existingLayer = currentLayers.find(l => l.id === 'sunburst-layer-body');

		const btn = document.querySelector('button[onclick*="window.interop.ShowSunburst()"]') as HTMLButtonElement;

		if (existingLayer) {
			console.log("[BBD] ShowSunburst: Toggling OFF");
			// To restore "normal" state, we should probably just trigger a state update or re-set the map layers.
			// Since we filtered them out completely, we need to get them back.
			// The easiest way is to re-trigger the main map render or restore from state.

			// Re-run the main render logic? 
			// Better: We just stored them in 'window.interop.deck.props.layers' before we nuked them? No, that's gone.
			// reliability: calling InitDeckGL again might be heavy.
			// Let's manually rebuild the standard layers from state.

			const data = window.interop.state.map || [];
			if (data.length > 0) {
				const z = window.interop.state.currentZoom;
				// Rebuild Standard Layers
				// 1. Colour Map
				const colourMap = window.interop.state.colourMap || [];
				// 2. Column Data
				const columnData = oneChart.buildColumnData(data, colourMap, 20);

				const onColumnClick = (selected) => { /* Re-bind if possible, or just rely on existing closure if we didn't lose it? Hard to re-bind full closure here. */ };
				// Actually this closure is huge. 

				// SIMPLER HACK: Just reload the page? No.
				// We should have hidden them with 'visible: false' instead of removing them.
				// But removing is cleaner for "Focus".

				// Let's try to restore from a cached "baseLayers" if we can? 
				// We didn't cache them.

				// Okay, let's use 'visible: false' in the ShowSunburst logic below instead of removing.
				// Then here we just toggle them back to 'visible: true'.
			}

			// RE-PLAN: Modify the ShowSunburst logic to use 'visible: false' and here to set 'visible: true'.
			// Checking next step.

			// For now, let's assume we will switch to 'visible: false' in the next step.
			// Restore logic:
			const current = window.interop.deck.props.layers || [];
			const restored = current.map(l => {
				if (l.id === 'sunburst-layer-body' || l.id === 'sunburst-layer-outline') return null;
				return l.clone({ visible: true, opacity: 1.0 });
			}).filter(l => l);

			window.interop.deck.setProps({ layers: restored });
			if (btn) btn.innerText = "🍺 Show Beer Wheel";
			return;
		}

		console.log("[BBD] ShowSunburst: Toggling ON");
		if (btn) btn.innerText = "🍺 Hide Beer Wheel";

		// 1. Get Hierarchy Data
		console.log("[BBD] ShowSunburst: Fetching hierarchy...");
		globalThis.interop.dotNet.invokeMethodAsync('GetStatsHierarchy')
			.then((data) => {
				// MOCK VALUES ONLY: User wants to keep the REAL hierarchy but mock the numbers.
				// CRITICAL FIX: Only assign values to LEAF nodes.
				// Do NOT sum them manually on parents, because d3.hierarchy().sum() does that automatically.
				// If we sum manually, D3 adds our sum + the children's sum, causing double counting.
				const inflateValues = (node: any) => {
					if (node.children && node.children.length > 0) {
						// Parent Node: reset value to 0 to avoid double-counting
						node.value = 0;
						node.children.forEach(child => inflateValues(child));
					} else {
						// Leaf Node: Assign random large value
						node.value = Math.floor(Math.random() * 400) + 100;
					}
				};

				if (data) {
					inflateValues(data);
					console.log("[BBD] ShowSunburst: Inflated Real Data Values", data);
				}

				// 2. Identify Target Location (Centroid of all venues)
				const currentMapData = window.interop.state.map || [];
				let targetCenter = { x: 133.7751, y: -25.2744 }; // Default AU

				// Determine target zoom (use default zoom passed from C# or current fallback)
				const targetZoom = window.interop.state.defaultZoom || 16;

				if (currentMapData.length > 0) {
					// Calculate Centroid
					const validLocations = currentMapData.filter(m => m.location && m.location.x !== 0);
					if (validLocations.length > 0) {
						const sumX = validLocations.reduce((acc, curr) => acc + curr.location.x, 0);
						const sumY = validLocations.reduce((acc, curr) => acc + curr.location.y, 0);
						targetCenter = {
							x: sumX / validLocations.length,
							y: sumY / validLocations.length
						};
						console.log(`[BBD] Sunburst Context: Centered on centroid of ${validLocations.length} venues: ${targetCenter.x}, ${targetCenter.y}`);
					} else {
						// Fallback to first if filtering fails
						targetCenter = { x: currentMapData[0].location.x, y: currentMapData[0].location.y };
						console.log(`[BBD] Sunburst Context: Centered on first venue (Fallback)`);
					}

					// Fly to target at default zoom
					if (window.interop.FlyTo) {
						window.interop.FlyTo(targetCenter.x, targetCenter.y, targetZoom);
					}
				}

				// 3. Calculate Dynamic Radius
				// Formula: BaseRadius (0.002 at Z16) * 2^(16 - TargetZoom)
				// This keeps it screen-relative size roughly constant.
				const baseRadius = 0.002;
				const baseZoom = 16;
				const radius = baseRadius * Math.pow(2, (baseZoom - targetZoom));
				console.log(`[BBD] Sunburst Radius calculated: ${radius} degrees for Zoom ${targetZoom}`);

				// 4. Create Layer (Returns array [body, outline])
				const sunburstLayers = oneChart.createSunburstLayer(data, [targetCenter], radius, (node) => {
					console.log("Sunburst node clicked:", node);
				});

				// 5. Append to existing layers
				const activeLayers = window.interop.deck.props.layers || [];
				// Hide all existing layers
				const hiddenBaseLayers = activeLayers.map(l => l.clone({ visible: false }));

				window.interop.deck.setProps({
					layers: [
						...hiddenBaseLayers,
						...[sunburstLayers].flat() // Flatten in case it returns array or single
					]
				});
			})
			.catch(err => {
				console.error("[BBD] ShowSunburst Error:", err);
				if (btn) btn.innerText = "🍺 The Beer Wheel (Error)";
			});
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
		let lat = Number(latitude);
		let lon = Number(longitude);
		let z = Number(zoom);

		if (isNaN(lat)) { console.warn("[BBD] FlyTo: Invalid latitude, ignoring"); return false; }
		if (isNaN(lon)) { console.warn("[BBD] FlyTo: Invalid longitude, ignoring"); return false; }
		if (isNaN(z)) { z = window.interop.state.currentZoom || 10; }

		window.interop.deck.setProps({
			initialViewState: {
				longitude: lon,
				latitude: lat,
				zoom: (z > 9 ? z : 15),
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
						currentZoom: Number(globalThis.interop.state.currentZoom),
						viewingVenue: globalThis.interop.state.viewingVenue,
						flyTo: globalThis.interop.FlyTo,
						toggleViewing: (next) => globalThis.interop.setState({ viewingVenue: next })
					});

					// Toggle between implementations
					const pieChartLayers = globalThis.interop.state.useSolidPies
						? oneChart.createSolidPieChartLayers(result, oneChart.ColourValues)
						: oneChart.createPieChartLayers(result, globalThis.deckGLContext as WebGLRenderingContext, oneChart.ColourValues);

					const pieLabelLayer = generateNewTextLayer(zoom, globalThis.interop.state.brewerMap, 'pie-text-layer', 128);
					const layers = [globalThis.interop.deck.props.layers[0], globalThis.interop.deck.props.layers[1],
						arcLayer, ...[pieChartLayers].flat(), pieLabelLayer];
					globalThis.interop.setState({ viewingVenue: false });
					globalThis.interop.deck.setProps({
						layers: layers
					});

				});
		});

		globalThis.interop.deck.setProps({
			layers: [columnLayer, generateNewTextLayer(zoom, globalThis.interop.state.map, 'text-layer', 64)]
		});

		// Hide loader when done
		window.interop.HideLoadBox(null);

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
		// If explicit element passed, use it (legacy/Razor support)
		if (element && element.style) {
			element.style.display = "block";
			return;
		}

		// Otherwise, look for or create our custom overlay
		let overlay = document.getElementById('bbd-loading-overlay');
		if (!overlay) {
			overlay = document.createElement('div');
			overlay.id = 'bbd-loading-overlay';
			overlay.className = 'bbd-loading-overlay';
			overlay.innerHTML = '<img src="/img/beerload.svg" class="bbd-loading-spinner" />';

			// Append to map container if available, otherwise body
			if (globalThis.interop.mapDiv) {
				globalThis.interop.mapDiv.appendChild(overlay);
			} else {
				document.body.appendChild(overlay);
			}
		}
		overlay.style.display = 'flex';
	},
	HideLoadBox: (element) => {
		if (element && element.style) {
			element.style.display = "none";
		}
		const overlay = document.getElementById('bbd-loading-overlay');
		if (overlay) {
			overlay.style.display = 'none';
		}
	},
	togglePieMode: () => {
		const nextMode = !globalThis.interop.state.useSolidPies;
		console.log(`[BBD] Toggling pie mode to ${nextMode ? 'SOLID' : 'TEXTURE'}`);
		globalThis.interop.setState({ useSolidPies: nextMode });
		// Trigger re-render of layers
		if (globalThis.interop.state.selectedVenue && globalThis.interop.state.brewerMap) {
			globalThis.interop.AddColumnChartPoint(globalThis.interop.state.currentZoom);
		}
	},
	ShowPopup: (coordinates, html) => {
		// Close existing popup if any
		if (globalThis.interop.popup) {
			globalThis.interop.popup.remove();
		}

		const popup = new mapboxgl.Popup({ closeOnClick: true, maxWidth: '300px' })
			.setLngLat(coordinates)
			.setHTML(html)
			.addTo(globalThis.interop.mapDiv as mapboxgl.Map);

		globalThis.interop.popup = popup;

		// Clear reference when closed
		popup.on('close', () => {
			globalThis.interop.popup = null;
		});
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

// Stub for missing function
function clickPie(d) {
	console.log("clickPie called", d);
}



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
