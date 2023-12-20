
declare global {
   interface Window {
      interop: {
         dotNet: any;
         deck: any;
         mapDiv: any;
         state: StateObject;
         setState: (a: {}) => void;
         getDimensions: () => {};
         getRenderArea: () => {};
         consoleLog: (a: string) => boolean;
         hookDotNet: (a: any) => void;
         FlyTo: (longitude, latitude, zoom) => void;
         SetMapState: (styles: {}) => void;
         InitDeckGL: (longitude, latitude, zoom) => boolean;
         AddColumnChartPoint: (a: number) => void;
         RefreshImage: (imageElementId, url) => Promise<void>;
         ShowLoadBox: (element) => void;
         HideLoadBox: (element) => void;
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