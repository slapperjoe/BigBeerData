"use strict";
var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
var __spreadArray = (this && this.__spreadArray) || function (to, from, pack) {
    if (pack || arguments.length === 2) for (var i = 0, l = from.length, ar; i < l; i++) {
        if (ar || !(i in from)) {
            if (!ar) ar = Array.prototype.slice.call(from, 0, i);
            ar[i] = from[i];
        }
    }
    return to.concat(ar || Array.prototype.slice.call(from));
};
exports.__esModule = true;
var mapboxgl = require("mapbox-gl");
var deck_gl_1 = require("deck.gl");
var core_1 = require("@luma.gl/core");
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
        selectedVenue: null
    },
    setState: function (stateObj) {
        delete window.interop.state._previousState;
        window.interop.state._previousState = __assign({}, window.interop.state);
        window.interop.state = Object.assign(Object.assign(Object.assign({}, window.interop.state), stateObj), { _revision: (window.interop.state._revision + 1) });
        //return this.state;		
        window.interop.dotNet.invokeMethodAsync('SetState', window.interop.state);
        return window.interop.state;
        //fire change notifier
    },
    getDimensions: function () {
        return {
            width: window.innerWidth,
            height: window.innerHeight
        };
    },
    getRenderArea: function () {
        var renderArea = document.getElementById('renderArea');
        if (renderArea) {
            return {
                width: renderArea.clientWidth,
                height: renderArea.clientHeight
            };
        }
    },
    consoleLog: function (textString) {
        window.console.log(textString);
        return true;
    },
    hookDotNet: function (dotNetObj) {
        window.interop.dotNet = dotNetObj;
    },
    FlyTo: function (longitude, latitude, zoom) {
        window.interop.deck.setProps({
            initialViewState: {
                longitude: longitude,
                latitude: latitude,
                zoom: (zoom > 9 ? zoom : 15),
                bearing: 0,
                pitch: 45,
                transitionInterpolator: new deck_gl_1.FlyToInterpolator({ speed: 1.5 }),
                transitionDuration: 'auto'
            }
        });
        return true;
    },
    SetMapState: function () {
        var styles = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            styles[_i] = arguments[_i];
        }
        //@ts-ignore
        window.interop.setState({ map: styles, layers: [] });
    },
    InitDeckGL: function (longitude, latitude, zoom) {
        var INITIAL_VIEW_STATE = {
            latitude: latitude,
            longitude: longitude,
            zoom: zoom,
            bearing: 0,
            pitch: 80
        };
        mapboxgl.accessToken = 'pk.eyJ1IjoibWFyaWMxIiwiYSI6Ii0xdWs1TlUifQ.U56tiQG_kj88zNf_1PxHQw'; // process.env.MapboxAccessToken; // eslint-disable-line
        var map = new mapboxgl.Map({
            container: 'map',
            style: 'mapbox://styles/maric1/ckclqelzf0fo71ipirav7fckc',
            interactive: true,
            center: [INITIAL_VIEW_STATE.longitude, INITIAL_VIEW_STATE.latitude],
            zoom: INITIAL_VIEW_STATE.zoom,
            bearing: INITIAL_VIEW_STATE.bearing,
            pitch: INITIAL_VIEW_STATE.pitch
        });
        var layerList = document.getElementById('menu');
        var inputs = layerList.getElementsByTagName('input');
        // @ts-ignore
        for (var _i = 0, inputs_1 = inputs; _i < inputs_1.length; _i++) {
            var input = inputs_1[_i];
            input.onclick = function (layer) {
                // @ts-ignore
                var layerId = layer.target.id;
                map.setStyle('mapbox://styles/mapbox/' + layerId);
            };
        }
        map.on('load', function () {
            map.addSource('mapbox-dem', {
                'type': 'raster-dem',
                'url': 'mapbox://mapbox.mapbox-terrain-dem-v1',
                'tileSize': 512,
                'maxzoom': 25
            });
            // add the DEM source as a terrain layer with exaggerated height
            map.setTerrain({ 'source': 'mapbox-dem', 'exaggeration': 1.5 });
            // add a sky layer that will show when the map is highly pitched
            map.addLayer({
                'id': 'sky',
                'type': 'sky',
                'paint': {
                    'sky-type': 'atmosphere',
                    'sky-atmosphere-sun': [0.0, 0.0],
                    'sky-atmosphere-sun-intensity': 15
                }
            });
        });
        map.addControl(new mapboxgl.FullscreenControl());
        var deck = new deck_gl_1.Deck({
            canvas: 'deck-canvas',
            width: '100%',
            height: '100%',
            initialViewState: INITIAL_VIEW_STATE,
            controller: true,
            onWebGLInitialized: function (gl) {
                // @ts-ignore
                gl = WebGLDebugUtils.makeDebugContext(gl, throwOnGLError, logGLCall);
                window.deckGLContext = gl;
            },
            onViewStateChange: function (_a) {
                var viewState = _a.viewState, interactionState = _a.interactionState, oldViewState = _a.oldViewState;
                //@ts-ignore
                map.jumpTo({
                    center: [viewState.longitude, viewState.latitude],
                    zoom: viewState.zoom,
                    bearing: viewState.bearing,
                    pitch: viewState.pitch
                });
                //if (interactionState.isZooming) {
                //	let newLayers = [];
                //	if (window.interop.deck.props.layers.length > 2) {
                //		newLayers = [...window.interop.deck.props.layers]
                //		newLayers[1] = generateNewTextLayer(viewState.zoom, window.interop.state.map, 'text-layer', 64);
                //		newLayers[window.interop.deck.props.layers.length - 1] = generateNewTextLayer(viewState.zoom, window.interop.state.brewerMap, 'pie-text-layer', 128);
                //		window.interop.setState({
                //			viewingVenue: true
                //		})
                //	}
                //	else {
                //		newLayers = [window.interop.deck.props.layers[0], generateNewTextLayer(viewState.zoom, window.interop.state.map, 'text-layer', 64)],
                //		window.interop.setState({
                //			viewingVenue: true
                //		})
                //	}
                //	window.interop.deck.setProps({
                //		layers: newLayers
                //	});
                //}
            },
            layers: [],
            log: {
                level: 1
            },
            getTooltip: function (hv) {
                if (hv.object) {
                    switch (hv.layer.id) {
                        case ("column-layer"):
                            return "".concat(hv.object.venuename, "\r\n").concat(hv.object.name, " - ").concat(hv.object.value);
                        case ("arc-layer"):
                            return hv.object.name;
                        default:
                            if (hv.layer.id.indexOf('piechart-layer') == 0) {
                                return {
                                    html: "Beers:<table>".concat(hv.object.beersBrewed.map(function (bb) { return createLabelRow(bb); }).join(''), "</table>"),
                                    style: {
                                        fontSize: '0.8em'
                                    }
                                };
                            }
                            break;
                    }
                }
            }
        });
        var colourMap = ((window.interop.state.map).map(function (a) { return a.styles.map(function (b) { return b.name; }); })).flat()
            .filter(function (value, index, self) { return self.indexOf(value) === index; }).map(function (a, b) { return { name: a, colour: ColourValues[b] }; });
        window.interop.setState({ colourMap: colourMap, currentZoom: zoom, mapLoaded: true });
        window.interop.deck = deck;
        window.interop.mapDiv = map;
        return true;
    },
    AddColumnChartPoint: function (zoom) {
        if (window.interop.state.mapLoaded) {
            var scale_1 = 20;
            debugger;
            var mapVals = window.interop.state.map;
            var columnData = mapVals.map(function (a) {
                return a.styles.map(function (b) {
                    var _a;
                    return {
                        centroid: [a.location.x, a.location.y, b.height * scale_1],
                        value: b.count,
                        name: b.name,
                        colour: (_a = window.interop.state.colourMap.find(function (c) { return c.name === b.name; })) === null || _a === void 0 ? void 0 : _a.colour,
                        venue: a.venue,
                        venuename: a.name
                    };
                });
            });
            var flatData = columnData.flat();
            window.interop.deck.setProps({
                layers: [new deck_gl_1.ColumnLayer({
                        id: 'column-layer',
                        data: flatData,
                        diskResolution: 48,
                        radius: 50,
                        extruded: true,
                        autoHighlight: true,
                        pickable: true,
                        elevationScale: scale_1,
                        getPosition: function (d) { return d.centroid; },
                        getFillColor: function (d) { return __spreadArray(__spreadArray([], (d.colour ? d.colour : [128, 128, 128]), true), [192], false); },
                        getLineColor: [0, 0, 0],
                        getElevation: function (d) { return d.value; },
                        onClick: function (a) {
                            window.interop.dotNet.invokeMethodAsync('GetBrewersByVenue', a.object.venue, a.object.name)
                                .then(function (result) {
                                window.interop.setState({
                                    brewerMap: result,
                                    selectedVenue: a.object
                                });
                                var arcLayer = new deck_gl_1.ArcLayer({
                                    id: 'arc-layer',
                                    data: result,
                                    pickable: true,
                                    getWidth: 4,
                                    getSourcePosition: [a.object.centroid[0], a.object.centroid[1]],
                                    getTargetPosition: function (d) { return [d.location.x, d.location.y]; },
                                    getSourceColor: a.object.colour,
                                    getTargetColor: a.object.colour,
                                    onClick: function (a) {
                                        var locs = [a.object.location.x, a.object.location.y];
                                        if (window.interop.state.viewingVenue) {
                                            locs = a.layer.props.getSourcePosition;
                                        }
                                        window.interop.FlyTo(locs[0], locs[1], window.interop.state.currentZoom);
                                        window.interop.setState({ viewingVenue: !window.interop.state.viewingVenue });
                                    }
                                });
                                var pieChartLayers = result.map(function (item) { return new deck_gl_1.SimpleMeshLayer({
                                    id: 'piechart-layer-' + item.name,
                                    data: [item],
                                    texture: new Promise(function (resolve, reject) {
                                        var _a, _b;
                                        var dataArray = [];
                                        item.beersBrewed.forEach(function (bb, i) {
                                            for (var j = 0; j < bb.count; j++) {
                                                bb.color = ColourValues[i];
                                                dataArray.push(ColourValues[i]);
                                            }
                                        });
                                        var texture = new core_1.Texture2D(window.deckGLContext, {
                                            width: item.beersBrewed.flatMap(function (a) { return a.count; }).reduce(function (a, b) { return a + b; }, 0),
                                            height: 1,
                                            format: window.deckGLContext.RGB,
                                            data: new Uint8Array(dataArray.flat()),
                                            parameters: (_a = {},
                                                _a[window.deckGLContext.TEXTURE_MAG_FILTER] = window.deckGLContext.NEAREST,
                                                _a[window.deckGLContext.TEXTURE_MIN_FILTER] = window.deckGLContext.NEAREST,
                                                _a),
                                            pixelStore: (_b = {},
                                                _b[window.deckGLContext.UNPACK_FLIP_Y_WEBGL] = true,
                                                _b),
                                            mipmaps: true
                                        });
                                        resolve(texture);
                                    }),
                                    autoHighlight: true,
                                    pickable: true,
                                    mesh: new core_1.CylinderGeometry({ radius: 5, height: 1, topCap: true, nradial: 48, bottomCap: false }),
                                    sizeScale: 16,
                                    getPosition: function (d) { return [d.location.x, d.location.y]; },
                                    getColor: function (d) { return [255, 214, 0]; },
                                    getOrientation: function (d) { return [0, 0, 270]; }
                                }); });
                                var pieLabelLayer = generateNewTextLayer(zoom, window.interop.state.brewerMap, 'pie-text-layer', 128);
                                var layers = [window.interop.deck.props.layers[0], window.interop.deck.props.layers[1], arcLayer, pieChartLayers, pieLabelLayer].flat();
                                window.interop.setState({
                                    viewingVenue: false
                                });
                                window.interop.deck.setProps({
                                    layers: layers
                                });
                            });
                        }
                        //onHover: ({x, y, object}) => setTooltip(x, y, object ? `${object.name}\n${object.address}` : null)
                    }), generateNewTextLayer(zoom, window.interop.state.map, 'text-layer', 64)]
            });
            return true;
        }
        return false;
    },
    RefreshImage: async (imageElementId, imageStream) => {
        debugger;
        const arrayBuffer = await imageStream.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        const image = document.getElementById(imageElementId);
        image.onload = () => {
            URL.revokeObjectURL(url);
        }
        image.src = url;
    }
};
function throwOnGLError(err, funcName, args) {
    //@ts-ignore
    throw WebGLDebugUtils.glEnumToString(err) + " was caused by call to: " + funcName;
}
;
function logGLCall(functionName, args) {
    console.log("gl." + functionName + "(" +
        //@ts-ignore
        WebGLDebugUtils.glFunctionArgsToString(functionName, args) + ")");
}
var ColourValues = [
    /*[255, 0, 0], */ [0, 255, 0], [0, 0, 255], [255, 255, 0], [255, 0, 255], [0, 255, 255],
    [192, 64, 0], [64, 192, 0], [64, 192, 0], [192, 64, 0], [64, 0, 192], [192, 0, 64], [192, 192, , 64], [64, 192, 192], [192.64, 192], [64, 192, 64], [64, 192, 192], [192, 192, 64],
    [128, 0, 0], [0, 128, 0], [0, 0, 128], [128, 128, 0], [128, 0, 128], [0, 128, 128], [128, 128, 128],
    [192, 0, 0], [0, 192, 0], [0, 0, 192], [192, 192, 0], [192, 0, 192], [0, 192, 192], [192, 192, 192],
    [64, 0, 0], [0, 64, 0], [0, 0, 64], [64, 64, 0], [64, 0, 64], [0, 64, 64], [64, 64, 64],
    [32, 0, 0], [0, 32, 0], [0, 0, 32], [32, 32, 0], [32, 0, 32], [0, 32, 32], [32, 32, 32],
    [96, 0, 0], [0, 96, 0], [0, 0, 96], [96, 96, 0], [96, 0, 96], [0, 96, 96], [96, 96, 96],
    [160, 0, 0], [0, 160, 0], [0, 0, 160], [160, 160, 0], [160, 0, 160], [0, 160, 160], [160, 160, 160],
    [224, 0, 0], [0, 224, 0], [0, 0, 224], [224, 224, 0], [224, 0, 224], [0, 224, 224], [224, 224, 224]
];
function generateNewTextLayer(zoom, dataSet, layerName, offset) {
    var locationData = dataSet.map(function (a) {
        return { centroid: [a.location.x, a.location.y], name: a.name };
    });
    var fontSize = (zoom * 3) - 10;
    if (zoom >= 14) {
        fontSize = 32;
    }
    //console.log(fontSize);
    return new deck_gl_1.TextLayer({
        id: layerName + '-' + zoom,
        data: locationData,
        pickable: false,
        billboard: false,
        getPosition: function (d) { return d.centroid; },
        getText: function (d) { return d.name; },
        getSize: fontSize,
        getAngle: 0,
        getTextAnchor: 'middle',
        getAlignmentBaseline: 'center',
        fontFamily: "Helvetica Neue",
        getPixelOffset: function (a, b) {
            var pixelVal = pixelValue(a.centroid[1], offset, zoom);
            //console.log(`Zoom is ${zoom}  -  offset is ${pixelVal}`);
            return [0, pixelVal];
        }
    });
}
function pixelValue(latitude, meters, zoomLevel) {
    var mapPixels = meters / (78271.484 / Math.pow(2, zoomLevel)) / Math.cos((latitude * Math.PI) / 180);
    var screenPixel = mapPixels * Math.floor(window.devicePixelRatio);
    return screenPixel;
}
function createLabelRow(bb) {
    return "<tr><td><span style=\"font-size: 150%; width: 30px; color: rgba(".concat(bb.color[0], ",").concat(bb.color[1], ",").concat(bb.color[2], ", 1);\">&#8226;</span></td><td>").concat(bb.name, "</td><td>").concat(bb.count, "</td></tr>");
}
