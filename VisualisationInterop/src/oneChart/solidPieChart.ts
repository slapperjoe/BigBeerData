
import { SolidPolygonLayer } from "@deck.gl/layers";
import { BrewerMapItem } from "./pieChart"; // Re-use interface

export function createSolidPieChartLayers(brewerMap: BrewerMapItem[], colourValues: number[][]) {
    const layerData: any[] = [];

    brewerMap.forEach(item => {
        const totalCount = item.beersBrewed.reduce((acc, b) => acc + b.count, 0);
        let currentAngle = 0;

        item.beersBrewed.forEach((brew, index) => {
            const sweepAngle = (brew.count / totalCount) * 360;
            if (sweepAngle > 0) {
                layerData.push({
                    polygon: generateCircleSector(item.location.x, item.location.y, 100, currentAngle, currentAngle + sweepAngle),
                    name: brew.name, // Not used directly by layer but good for picking
                    count: brew.count, // For tooltip
                    beers: brew.beers, // For tooltip details
                    color: colourValues[index],
                    height: 25, // Constant height as requested (halved)
                    brewerName: item.name
                });
                currentAngle += sweepAngle;
            }
        });
    });

    return new SolidPolygonLayer({
        id: 'solid-pie-layer',
        data: layerData,
        pickable: true,
        extruded: true,
        wireframe: false,
        getPolygon: d => d.polygon,
        getFillColor: d => [d.color[0], d.color[1], d.color[2], 255],
        getElevation: d => d.height, // Use constant height
        autoHighlight: true,
        highlightColor: [255, 255, 255, 128],
        onClick: (info) => {
            if (!info.object) return;
            // Generate HTML for the popup
            const d = info.object;
            const beerItems = d.beers && d.beers.length > 0
                ? `<ul style="margin: 5px 0 0 15px; padding: 0;">${d.beers.map(b => `<li>${b}</li>`).join('')}</ul>`
                : '';

            const html = `
                <div>
                    <div style="font-weight: bold; margin-bottom: 4px;">Brewer: ${d.brewerName}</div>
                    <div style="font-weight: bold; margin-bottom: 4px;">Style: ${d.name} (${d.count})</div>
                    ${beerItems}
                </div>
            `;

            // Call interop to show popup
            window.interop.ShowPopup([info.coordinate[0], info.coordinate[1]], html);
            return true;
        }
        //material: true // Use default lighting
    });
}

function generateCircleSector(centerX: number, centerY: number, radiusMeters: number, startAngle: number, endAngle: number, segments: number = 20) {
    const positions: number[][] = [];

    const startRad = (startAngle * Math.PI) / 180;
    const endRad = (endAngle * Math.PI) / 180;

    positions.push([centerX, centerY]); // Center point for pie slice

    // Approximate meters to degrees conversion
    // 1 degree lat ~= 111,320 meters
    // 1 degree lng ~= 111,320 * cos(lat) meters
    const lat = centerY;
    const degreesPerMeterLat = 1 / 111320;
    const degreesPerMeterLng = 1 / (111320 * Math.cos(lat * Math.PI / 180));

    for (let i = 0; i <= segments; i++) {
        const angle = startRad + (i / segments) * (endRad - startRad);

        // Calculate offset in meters (using standard math: 0 is East/Right, increasing CCW)
        const dxMeters = Math.cos(angle) * radiusMeters;
        const dyMeters = Math.sin(angle) * radiusMeters;

        // Convert offset to degrees
        const dxDegrees = dxMeters * degreesPerMeterLng;
        const dyDegrees = dyMeters * degreesPerMeterLat;

        positions.push([
            centerX + dxDegrees,
            centerY + dyDegrees
        ]);
    }

    return positions;
}
