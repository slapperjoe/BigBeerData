import type { Color } from "@deck.gl/core";
import { ColumnLayer } from "@deck.gl/layers";

/**
 * Shape of a colour lookup entry that aligns beer style names to RGBA arrays.
 */
export interface ColourEntry {
	name: string;
	colour: number[];
}

/**
 * Minimal map entry representing a venue location and its style metrics.
 */
export interface StyleMapEntry {
	location: { x: number; y: number };
	styles: Array<{ height: number; count: number; name: string }>;
	venue: unknown;
	name: string;
}

/**
 * Flattened point used by Deck.GL ColumnLayer.
 */
export interface ColumnDatum {
	centroid: [number, number, number];
	value: number;
	name: string;
	colour?: number[];
	venue: unknown;
	venuename: string;
	totalHeight?: number; // Added to lift arc start point
}

/**
 * Expand location/style data into per-column datapoints for Deck.GL.
 */
export function buildColumnData(map: StyleMapEntry[], colourMap: ColourEntry[], scale: number): ColumnDatum[] {
	const columnData = map.map((entry) => {
		// Calculate total height of the stack for this venue
		const totalStackHeight = entry.styles.reduce((max, style) => Math.max(max, style.height + style.count), 0);

		return entry.styles.map((style) => {
			const matchedColor = colourMap.find((c) => c.name === style.name)?.colour;
			if (!matchedColor) console.warn(`[BBD] No color found for style: '${style.name}'`);

			return {
				centroid: [entry.location.x, entry.location.y, style.height * scale] as [number, number, number],
				value: style.count,
				name: style.name,
				colour: matchedColor,
				venue: entry.venue,
				venuename: entry.name,
				totalHeight: totalStackHeight * scale // Scale the total height
			};
		});
	});

	return columnData.flat();
}

/**
 * Create a ColumnLayer with click handling delegated to the caller.
 */
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
