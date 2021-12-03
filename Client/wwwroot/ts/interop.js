var interop = {
    dotNet: null,
    state: {
        _revision: 1,
        map: [],
        _previousState: null,
        colourMap: [],
        layers: [],
        brewerMap: [],
        uniformData: {
            outSliceLength: [],
            outSliceData: [],
            colourMap: []
        }
    },
    setState(stateObj) {
        delete this.state._previousState;
        this.state._previousState = Object.assign({}, this.state);
        this.state = Object.assign(Object.assign(Object.assign({}, this.state), stateObj), { _revision: (this.state._revision + 1) });
        //return this.state;
        return true;
        //fire change notifier
    },
    getDimensions() {
        return {
            width: window.innerWidth,
            height: window.innerHeight
        };
    },
    getRenderArea() {
        var renderArea = document.getElementById('renderArea');
        if (renderArea) {
            return {
                width: renderArea.clientWidth,
                height: renderArea.clientHeight
            };
        }
    },
    consoleLog(textString) {
        window.console.log(textString);
        return true;
    },
    hookDotNet(dotNetObj) {
        interop.dotNet = dotNetObj;
    },
    InitDeckGL(longitude, latitude, zoom) {
        //@ts-ignore
        return initDeckGL(longitude, latitude, zoom);
    },
    FlyTo(longitude, latitude, zoom) {
        //@ts-ignore
        return flyTo(longitude, latitude, zoom);
    },
    AddColumnChartPoint(zoom) {
        //@ts-ignore
        return addColumnChartPoint(zoom);
    },
    SetMapState(...styles) {
        //@ts-ignore
        this.setState({ map: styles, layers: [] });
        return true;
    },
    mapLoaded: true,
    map: null,
    deck: null,
    currentZoom: 0,
    //layerSet: null,
    longitude: 0,
    latitude: 0,
    label: ""
};
window.interop = interop;
//# sourceMappingURL=interop.js.map