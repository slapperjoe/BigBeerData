import type { Color } from "@deck.gl/core/typed";
import { ColumnLayer } from "@deck.gl/layers/typed";

export interface ColourEntry {
	name: string;
	colour: number[];
}

export interface StyleMapEntry {
	location: { x: number; y: number };
	styles: Array<{ height: number; count: number; name: string }>;
	venue: unknown;
	name: string;
}

export interface ColumnDatum {
	centroid: [number, number, number];
	value: number;
	name: string;
	colour?: number[];
	venue: unknown;
	venuename: string;
}

export function buildColumnData(map: StyleMapEntry[], colourMap: ColourEntry[], scale: number): ColumnDatum[] {
	const columnData = map.map((entry) => {
		return entry.styles.map((style) => {
			return {
				centroid: [entry.location.x, entry.location.y, style.height * scale] as [number, number, number],
				value: style.count,
				name: style.name,
				colour: colourMap.find((c) => c.name === style.name)?.colour,
				venue: entry.venue,
				venuename: entry.name,
			};
		});
	});

	return columnData.flat();
}

export function createColumnLayer(data: ColumnDatum[], scale: number, onClick: (point: ColumnDatum) => void): ColumnLayer<ColumnDatum> {
	return new ColumnLayer<ColumnDatum>({
		id: "column-layer",
		data,
		diskResolution: 48,
		radius: 50,
		extruded: true,
		autoHighlight: true,
		pickable: true,
		elevationScale: scale,
		getPosition: (d) => d.centroid,
		getFillColor: (d): Color => (d.colour ? (d.colour as Color) : ([128, 128, 128, 192] as Color)),
		getLineColor: [0, 0, 0],
		getElevation: (d) => d.value,
		onClick: (info) => {
			if (info.object) {
				onClick(info.object as ColumnDatum);
			}
		},
	});
}
