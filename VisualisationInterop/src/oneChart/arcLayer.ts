import type { Color } from "@deck.gl/core/typed";
import { ArcLayer } from "@deck.gl/layers/typed";
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
		getSourcePosition: () => [selected.centroid[0], selected.centroid[1]],
		getTargetPosition: (d: any) => [d.location.x, d.location.y],
		getSourceColor: (): Color => (selected.colour as Color) ?? ([255, 214, 0] as Color),
		getTargetColor: (): Color => (selected.colour as Color) ?? ([255, 214, 0] as Color),
		onClick: (info) => {
			if (!info.object) return;
			let locs: [number, number] = [info.object.location.x, info.object.location.y];
			if (ctx.viewingVenue && info.layer) {
				const source = (info.layer.props as any)["getSourcePosition"] as [number, number];
				locs = [source[0], source[1]];
			}
			ctx.flyTo(locs[0], locs[1], ctx.currentZoom);
			ctx.toggleViewing(!ctx.viewingVenue);
		},
	});
}
