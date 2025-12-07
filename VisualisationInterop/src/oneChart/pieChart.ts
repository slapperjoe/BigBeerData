import { SimpleMeshLayer } from "@deck.gl/mesh-layers/typed";
import { CylinderGeometry, Texture2D, readPixelsToArray } from "@luma.gl/core";

/**
 * Aggregated counts per brewer for pie construction.
 */
export interface BrewerBrewed {
	count: number;
	color?: number[];
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
		texture: new Promise((resolve) => {
			const dataArray: number[][] = [];
			item.beersBrewed.forEach((bb, i) => {
				for (let j = 0; j < bb.count; j++) {
					bb.color = colourValues[i];
					dataArray.push(colourValues[i]);
				}
			});
			const width = item.beersBrewed.flatMap((a) => a.count).reduce((a, b) => a + b, 0);
			const texture = new Texture2D(glContext, {
				width,
				height: 1,
				format: glContext.RGB,
				data: new Uint8Array(dataArray.flat()),
				parameters: {
					[glContext.TEXTURE_MAG_FILTER]: glContext.NEAREST,
					[glContext.TEXTURE_MIN_FILTER]: glContext.NEAREST,
				},
				pixelStore: {
					[glContext.UNPACK_FLIP_Y_WEBGL]: true,
				},
				mipmaps: true,
			});
			resolve(texture);
		}),
		onClick: (pI) => {
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
		},
		autoHighlight: true,
		pickable: true,
		mesh: new CylinderGeometry({ radius: 5, height: 1, topCap: true, nradial: 48, bottomCap: false }),
		sizeScale: 16,
		_useMeshColors: true,
		getPosition: (d: BrewerMapItem) => [d.location.x, d.location.y],
		getColor: (_d: BrewerMapItem) => [255, 214, 0],
		getOrientation: (_d: BrewerMapItem) => [0, 0, 270],
	}));
}
