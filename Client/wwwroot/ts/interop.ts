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

	setState(stateObj: object) {
		delete this.state._previousState;
		this.state._previousState = { ...this.state };
		this.state = { ...this.state, ...stateObj, _revision: (this.state._revision + 1) };
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
			}
		}
	},

	consoleLog(textString: string): boolean {
		window.console.log(textString);
		return true;
	},

	hookDotNet(dotNetObj: any) {
		interop.dotNet = dotNetObj;
	},

	InitDeckGL(longitude: number, latitude: number, zoom: number) {
		//@ts-ignore
		return initDeckGL(longitude, latitude, zoom);
	},

	FlyTo(longitude: number, latitude: number, zoom: number) {
		//@ts-ignore
		return flyTo(longitude, latitude, zoom);
	},

	AddColumnChartPoint(zoom: number) {
		//@ts-ignore
		return addColumnChartPoint(zoom);
	},

	SetMapState(...styles: any[]) {
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
}

window.interop = interop;