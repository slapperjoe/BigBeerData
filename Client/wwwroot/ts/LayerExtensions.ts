
import { GL, Model, CylinderGeometry, ColumnLayer, ScatterplotLayer }
	from "../../npm_packages/src/index"


//import vs from 'raw-loader!./pie.vs.glsl';
//import ps from 'raw-loader!./pie.ps.glsl';

export module LayerExtensions {
	export class PieChartLayer extends ColumnLayer {
		declare props: any;
		declare context: any;
		declare setState: (a: any) => {};
		//declare _getModel: (a: any) => void;
		declare state: {
			model: any
		}

		constructor(...options) {
			super(...options);
		}


		initializeState() {
			const { gl } = this.context;
			this.setState({
				model: this._getModel(gl)
			});
			//super.initalizeState();
		}

		_getModel(gl) {
			return new Model(gl, Object.assign({}, super.getShaders(), {
				id: this.props.id,
				//geometry: new CylinderGeometry(),
				isInstanced: true,
			}));
		}


		static layerName = 'PieChartLayer';
	}


	export class PieScatterplotLayer extends ScatterplotLayer {

		declare props: any;
		declare setState: (a: any) => {};
		declare _getModel: (a: any) => void;
		declare state: {
			model: any
		}

		constructor(...options) {
			super(...options);
		}

		getShaders() {
			var orig = super.getShaders();
			//orig.vs = vs;
			//orig.fs = eval("`" + ps + "`");
			//orig.vs = vs.substr(vs.indexOf("#define"));
			//orig.fs = eval("`" + ps.indexOf("#define") + "`");;
			return orig;
		}

		initializeState() {
			super.initializeState();
			//@ts-ignore			
			this.getAttributeManager().addInstanced({
				instanceStartIndex: {
					//@ts-ignore
					size: 1,
					transition: true,
					normalized: true,
					type: GL.INT,
					accessor: 'getStartIndex',
					defaultValue: 0
				},
				instanceEndIndex: {
					//@ts-ignore
					size: 1,
					transition: true,
					normalized: true,
					type: GL.INT,
					accessor: 'getEndIndex',
					defaultValue: 0
				},
				instanceBrewerIndex: {
					size: 1,
					transition: true,
					normalized: true,
					type: GL.INT,
					accessor: 'getBrewerIndex',
					defaultValue: 0
				},
				//instanceAngleData: {
				//	size: 1,
				//	normalized: true,
				//	type: GL.FLOAT,
				//	accessor: 'getAngleData',
				//	defaultValue: [0, 90, 90, 180, 180, 270, 270, 360]
				//},
				instanceAngleNumber: {
					size: 1,
					accessor: 'getAngleNumber',
					transition: true,
					normalized: true,
					type: GL.FLOAT,
					defaultValue: 1
				},
			})
		};

		updateState({ props, oldProps, changeFlags }) {
			super.updateState({ props, oldProps, changeFlags });
			this.state.model.setUniforms({
				colourMap: window.interop.state.uniformData.colourMap,
				outSliceLength: props.outSliceLength,
				outSliceData: props.outSliceData,
				//angleNumber: 4//props.angleNumber()
			});
		}

		static defaultProps = {
			getStartIndex: { type: 'accessor', value: 0 },
			getEndIndex: { type: 'accessor', value: 0 },
			getBrewerIndex: { type: 'accessor', value: 0 },
			outSliceLength: { type: 'number', value: 3 },
			outSliceData: { type: 'array', value: [0, 0, 0, 0, 0, 0, 0, 0] },
			colourMap: {
				type: 'array', value: [
					1.0, 0.0, 0.0]//,
				//0.0, 1.0, 0.0,
				//0.0, 0.0, 1.0]
			},
			//angleNumber: {type: 'number', value: 4},
			//getAngleData: { type: 'accessor', value: [0, 90, 90, 180, 180, 270, 270, 360] },
			getAngleNumber: { type: 'accessor', value: 1 },
			...ScatterplotLayer.defaultProps
		}
		static layerName = 'PieScatterplotLayer';
	}
}
