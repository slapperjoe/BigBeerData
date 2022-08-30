import * as mapboxgl from "mapbox-gl"
import { TextLayer, ColumnLayer, FlyToInterpolator, ArcLayer, SimpleMeshLayer, Deck } from "deck.gl"
import { Texture2D, CylinderGeometry, readPixelsToArray } from "@luma.gl/core"

window.addEventListener('resize', function () {
  window.interop.dotNet.invokeMethodAsync('GetMainArea', true);
});

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
    delete window.interop.state._previousState;
    window.interop.state._previousState = { ...window.interop.state };
    window.interop.state = Object.assign(Object.assign(Object.assign({}, window.interop.state), stateObj), { _revision: (window.interop.state._revision + 1) });
    //return this.state;		
    window.interop.dotNet.invokeMethodAsync('SetState', window.interop.state)
    return window.interop.state;
    //fire change notifier
  },
  getDimensions: () => {
    return {
      width: window.innerWidth,
      height: window.innerHeight
    };
  },
  getRenderArea: () => {
    var renderArea = document.getElementById('renderArea');
    if (renderArea) {
      return {
        width: renderArea.clientWidth,
        height: renderArea.clientHeight
      };
    }
  },
  consoleLog: (textString) => {
    window.console.log(textString);
    return true;
  },
  hookDotNet: (dotNetObj) => {
    window.interop.dotNet = dotNetObj;
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

    mapboxgl.accessToken = 'pk.eyJ1IjoibWFyaWMxIiwiYSI6Ii0xdWs1TlUifQ.U56tiQG_kj88zNf_1PxHQw';// process.env.MapboxAccessToken; // eslint-disable-line

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
      log: {
        level: 1
      },
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
    return true;
  },
  AddColumnChartPoint: (zoom) => {
    if (window.interop.state.mapLoaded) {

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

      window.interop.deck.setProps({
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
                  brewerMap: result,
                  selectedVenue: a.object
                })

                const arcLayer = new ArcLayer({
                  id: 'arc-layer',
                  data: result,
                  pickable: true,
                  getWidth: 4,
                  getSourcePosition: [a.object.centroid[0], a.object.centroid[1]],
                  getTargetPosition: d => [d.location.x, d.location.y],
                  getSourceColor: a.object.colour,//d => [255, 214, 0],
                  getTargetColor: a.object.colour,
                  onClick: (a) => {
                    let locs = [a.object.location.x, a.object.location.y];
                    if (window.interop.state.viewingVenue) {
                      locs = a.layer.props.getSourcePosition;
                    }
                    window.interop.FlyTo(locs[0], locs[1], window.interop.state.currentZoom)
                    window.interop.setState({ viewingVenue: !window.interop.state.viewingVenue })
                  }
                });

                const pieChartLayers = result.map(item => new SimpleMeshLayer({
                  id: 'piechart-layer-' + item.name,
                  data: [item],
                  texture: new Promise((resolve, reject) => {
                    let dataArray = [];
                    item.beersBrewed.forEach((bb, i) => {
                      for (var j = 0; j < bb.count; j++) {
                        bb.color = ColourValues[i];
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
                  onClick: ({ bitmap, layer }) => {
                    if (bitmap) {
                      debugger;
                      const pixelColor = readPixelsToArray(layer.props.image, {
                        sourceX: bitmap.pixel[0],
                        sourceY: bitmap.pixel[1],
                        sourceWidth: 1,
                        sourceHeight: 1
                      })
                      console.log('Color at picked pixel:', pixelColor)
                    }
                  },
                  autoHighlight: true,
                  pickable: true,
                  mesh: new CylinderGeometry({ radius: 5, height: 1, topCap: true, nradial: 48, bottomCap: false }),
                  sizeScale: 16,
                  _useMeshColors: true,
                  getPosition: d => [d.location.x, d.location.y],
                  getColor: d => [255, 214, 0],
                  getOrientation: d => [0, 0, 270]
                }))

                const pieLabelLayer = generateNewTextLayer(zoom, window.interop.state.brewerMap, 'pie-text-layer', 128);
                const layers = [window.interop.deck.props.layers[0], window.interop.deck.props.layers[1], arcLayer, pieChartLayers, pieLabelLayer].flat();
                window.interop.setState({
                  viewingVenue: false
                })
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
  }

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

declare global {
  interface Window {
    interop: {
      dotNet: any;
      deck: any;
      mapDiv: any;
      state: StateObject;
      setState: (a: {}) => void;
      AddColumnChartPoint: (a: number) => void;
      InitDeckGL: (longitude, latitude, zoom) => boolean;
      FlyTo: (longitude, latitude, zoom) => void;
      SetMapState: (a: {}) => void;
      getDimensions: () => {};
      getRenderArea: () => {};
      hookDotNet: (a: any) => void;
      consoleLog: (a: string) => boolean;
    };
    deckGLContext: any;
  }
}

export interface StateObject {
  _revision: number;
  map: Array<any>;
  _previousState: StateObject;
  colourMap: Array<any>;
  //layers: Array<any>;
  brewerMap: Array<any>;
  mapLoaded: boolean;
  //deck: any;
  currentZoom: number;
  //layerSet: null,
  longitude: number;
  latitude: number;
  label: string;
  viewingVenue: boolean;
  uniformData: { colourMap: any };
  selectedVenue: any;
}
