import { PolygonLayer, PathLayer } from '@deck.gl/layers';
import * as d3 from 'd3';

export const createSunburstLayer = (data: any, centers: { x: number, y: number }[], radius: number, onClick: (info: any) => void) => {

    // 1. Process Hierarchy
    const root = d3.hierarchy(data)
        .sum(d => d.value)
        .sort((a, b) => b.value - a.value);

    // 2. Compute Partition Layout
    // Radius passed in (degrees)
    const partition = d3.partition()
        .size([2 * Math.PI, radius * radius]);
    // .padding(0.005); // POTENTIAL BUG: Padding might be consuming entire space if scale is erratic

    partition(root);

    // 4. Pre-calculate Colors Hierarchically
    // User requested "lots more colours" and "greater contrast".
    // We will use a Rainbow Spectrum mapped by global index/family to maximize difference.

    if (root.children) {
        const totalFamilies = root.children.length;

        root.children.forEach((familyNode, index) => {
            // Rainbow mapping: 0 to 1
            // Use d3.interpolateRainbow or similar cyclic scale
            const t = index / totalFamilies;
            const familyColor = d3.interpolateSinebow(t); // Sinebow provides better perceptual contrast than Rainbow
            (familyNode as any).color = familyColor;

            if (familyNode.children) {
                const typeCount = familyNode.children.length;
                familyNode.children.forEach((typeNode, typeIndex) => {
                    // Shift the color drastically for children
                    // We can step "forward" in the spectrum by a fraction
                    const c = d3.hsl(familyColor);

                    if (typeCount > 1) {
                        // Distribute children across a hue range centered on parent, 
                        // OR just continue the spectrum if we want chaos.
                        // Let's spread +/- 45 degrees
                        const spread = 90;
                        const shift = ((typeIndex / (typeCount - 1 || 1)) * spread) - (spread / 2);
                        c.h += shift;

                        // Alternate Lightness/Saturation extremely for neighbors
                        if (typeIndex % 2 === 0) {
                            c.l = 0.4; // Dark
                            c.s = 1.0; // Saturate
                        } else {
                            c.l = 0.7; // Light
                            c.s = 0.6; // Desaturate
                        }
                    }
                    (typeNode as any).color = c.hex();
                    const typeHex = c.hex();

                    if (typeNode.children) {
                        const styleCount = typeNode.children.length;
                        typeNode.children.forEach((styleNode, styleIndex) => {
                            const sc = d3.hsl(typeHex);
                            if (styleCount > 1) {
                                // Micro-variations for leaves
                                const leafShift = ((styleIndex / (styleCount - 1 || 1)) * 30) - 15;
                                sc.h += leafShift;
                                sc.l += (styleIndex % 2 === 0 ? 0.15 : -0.15); // Contrast stripes
                            }
                            (styleNode as any).color = sc.hex();
                        });
                    }
                });
            }
        });
    }

    // 3. Flatten for Deck.GL
    // We need to generate nodes for EVERY center.
    const baseNodes = root.descendants().filter(d => d.depth > 0);

    // Duplicate nodes for each center
    const allNodes: any[] = [];

    centers.forEach(center => {
        baseNodes.forEach(node => {
            // Clone the node context to specific location
            allNodes.push({
                ...node, // Copies x0, x1, y0, y1, etc.
                center: [center.x, center.y] // Attach specific center
            });
        });
    });

    // console.log(`[Sunburst] DIAGNOSTIC: ...`); 

    // Layer 1: The Extruded Body (Filled, No Stroke)
    const bodyLayer = new PolygonLayer({
        id: 'sunburst-layer-body',
        data: allNodes,
        stroked: false,
        filled: true,
        wireframe: false,
        getPolygon: (d: any) => {
            return getArcPolygon(d.x0, d.x1, Math.sqrt(d.y0), Math.sqrt(d.y1), 20, d.center) as any;
        },
        getFillColor: (d: any) => {
            if (d.color) {
                const rgb = hexToRgb(d.color);
                return [rgb[0], rgb[1], rgb[2], 230];
            }
            return [128, 128, 128, 230];
        },
        getElevation: (d: any) => 50, // Constant Height as requested
        extruded: true,
        pickable: true,
        onClick: (info) => { if (info.object) onClick(info.object); },
        autoHighlight: true,
        highlightColor: [255, 255, 255, 100]
    });

    // Layer 2: The Outline (PathLayer for explicit 3D wireframes)
    const outlineLayer = new PathLayer({
        id: 'sunburst-layer-outline',
        data: allNodes,
        widthUnits: 'pixels',
        widthMinPixels: 1, // Thinner (1px) for finest wireframe
        getPath: (d: any) => {
            const z = 50; // Constant Height
            const poly = getArcPolygon(d.x0, d.x1, Math.sqrt(d.y0), Math.sqrt(d.y1), 20, d.center);
            poly.push(poly[0]); // Close loop
            return poly.map(p => [p[0], p[1], z]) as any;
        },
        getColor: (d: any) => {
            if (d.color) {
                // "Stronger" color: Darker version of fill to act as a solid border
                const c = d3.color(d.color);
                if (c) {
                    const rgb = c.rgb(); // Explicit conversion
                    return [rgb.r * 0.6, rgb.g * 0.6, rgb.b * 0.6]; // 40% darker
                }
            }
            return [255, 255, 255];
        },
        pickable: false,
        rounded: true, // Round joints to prevent "white bits"
        billboard: true
    });

    return [bodyLayer, outlineLayer];
};

// Helper: Hex to RGB
function hexToRgb(hex: string): [number, number, number] {
    const c = d3.color(hex);
    if (c) {
        const rgb = c.rgb();
        return [rgb.r, rgb.g, rgb.b];
    }
    return [200, 200, 200];
}

// Helper: Invert RGB
function invertRgb(hex: string): [number, number, number] {
    const c = d3.color(hex);
    if (c) {
        const rgb = c.rgb();
        return [255 - rgb.r, 255 - rgb.g, 255 - rgb.b];
    }
    return [0, 0, 0];
}

// Helper: Generate Arc Polygon
function getArcPolygon(startAngle: number, endAngle: number, innerRadius: number, outerRadius: number, segments: number, center: [number, number]): number[][] {
    const polygon = [];
    const [cx, cy] = center;

    // Outer Arc
    for (let i = 0; i <= segments; i++) {
        const angle = startAngle + (endAngle - startAngle) * (i / segments);
        // Mapbox/DeckGL coordinate system: check rotation.
        // Usually standard Math.cos/sin works.
        polygon.push([
            cx + Math.cos(angle) * outerRadius,
            cy + Math.sin(angle) * outerRadius
        ]);
    }

    // Inner Arc (reverse)
    for (let i = segments; i >= 0; i--) {
        const angle = startAngle + (endAngle - startAngle) * (i / segments);
        polygon.push([
            cx + Math.cos(angle) * innerRadius,
            cy + Math.sin(angle) * innerRadius
        ]);
    }

    // Close loop (SolidPolygonLayer handles it, but good to be explicit)
    // polygon.push(polygon[0]);
    return polygon;
}
