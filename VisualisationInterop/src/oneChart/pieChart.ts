import { SimpleMeshLayer } from "@deck.gl/mesh-layers";
import { CylinderGeometry } from "@luma.gl/engine";
import { Texture } from "@luma.gl/core";
import { WebGLDevice } from "@luma.gl/webgl";

/**
 * Aggregated counts per brewer for pie construction.
 */
export interface BrewerBrewed {
	name: string;
	count: number;
	color?: number[];
	beers?: string[];
}

/**
 * Brewer map payload used to render pies at locations.
 */
export interface BrewerMapItem {
	location: { x: number; y: number };
	name: string;
	beersBrewed: BrewerBrewed[];
}

/**
 * Create SimpleMeshLayer pies with baked textures derived from beer counts.
 */
export function createPieChartLayers(brewerMap: BrewerMapItem[], glContext: WebGLRenderingContext, colourValues: number[][]) {
	return brewerMap.map((item) => new SimpleMeshLayer({
		id: `piechart-layer-${item.name}`,
		data: [item],
		texture: (() => {
			const dataArray: number[][] = [];
			item.beersBrewed.forEach((bb, i) => {
				for (let j = 0; j < bb.count; j++) {
					bb.color = colourValues[i];
					dataArray.push(colourValues[i]);
				}
			});
			const width = item.beersBrewed.flatMap((a) => a.count).reduce((a, b) => a + b, 0);
			// @ts-ignore
			const device = new WebGLDevice({ gl: glContext });
			return device.createTexture({
				width,
				height: 1,
				format: 'rgb8unorm' as any, // glContext.RGB usually maps to this or similar, explicit format is safer in v9
				data: new Uint8Array(dataArray.flat()),
				sampler: {
					minFilter: 'nearest',
					magFilter: 'nearest',
				},
			});
		})(),
		onClick: (pI) => {
			/*
			const image = (pI.layer?.props as any)?.["image"];
			if (pI.coordinate && image) {
				const pixelColor = readPixelsToArray(image, {
					sourceX: pI.coordinate[0],
					sourceY: pI.coordinate[1],
					sourceWidth: 1,
					sourceHeight: 1,
				});
				console.log("Color at picked pixel:", pixelColor);
			}
			*/
		},
		autoHighlight: true,
		pickable: true,
		// @ts-ignore
		mesh: new CylinderGeometry({ radius: 5, height: 1, topCap: true, nradial: 48, bottomCap: false }),
		sizeScale: 16,
		_useMeshColors: true,
		getPosition: (d: BrewerMapItem) => [d.location.x, d.location.y],
		getColor: (_d: BrewerMapItem) => [255, 214, 0],
		getOrientation: (_d: BrewerMapItem) => [0, 0, 270],
	}));
}
