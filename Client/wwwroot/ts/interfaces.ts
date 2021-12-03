

export interface LocationStyle {
	location: XYPoint,
	venue: number;
	label: string,
	count: number,
	height: number,
	name: string,
	styles: any[]
}

export interface XYPoint {
	x: number,
	y: number
}

export interface BrewerResult {
	id: number,
	name: string,
	type: string,
	url: string,
	location: XYPoint,
	beersBrewed: any,
	uniformData: {}
	angleData?: Array<number>
}

declare global {
	interface Window {
		interop: any;
	}
}
