import type { Color } from "@deck.gl/core";
import { ArcLayer } from "@deck.gl/layers";
import type { ColumnDatum } from "./columnChart";

/**
 * Context needed to wire arc interactivity back to the host app.
 */
export interface ArcContext {
	currentZoom: number;
	viewingVenue: boolean;
	flyTo: (longitude: number, latitude: number, zoom: number) => void;
	toggleViewing: (next: boolean) => void;
}

/**
 * Build an ArcLayer that links a selected column to related brewer locations.
 */
export function createArcLayer(selected: ColumnDatum, data: any[], ctx: ArcContext): ArcLayer<any> {
	return new ArcLayer({
		id: "arc-layer",
		data,
		pickable: true,
		getWidth: 4,
		// Use totalHeight (index 2 + height) or default to centroid height if missing.
		getSourcePosition: () => [selected.centroid[0], selected.centroid[1], (selected.totalHeight || selected.centroid[2])],
		getTargetPosition: (d: any) => [d.location.x, d.location.y],
		getSourceColor: (): Color => (selected.colour as Color) ?? ([255, 214, 0] as Color),
		getTargetColor: (): Color => (selected.colour as Color) ?? ([255, 214, 0] as Color),
		onClick: (info) => {
			if (!info.object) return;
			// Toggle logic: If we are at the venue (viewingVenue=true), fly to the brewer (object.location).
			// If we are at the brewer (viewingVenue=false), fly back to the venue (selected.centroid).

			if (ctx.viewingVenue) {
				// Fly to Brewer
				ctx.flyTo(Number(info.object.location.x), Number(info.object.location.y), Number(ctx.currentZoom));
			} else {
				// Fly to Venue
				ctx.flyTo(Number(selected.centroid[0]), Number(selected.centroid[1]), Number(ctx.currentZoom));
			}
			ctx.toggleViewing(!ctx.viewingVenue);
		},
	});
}
