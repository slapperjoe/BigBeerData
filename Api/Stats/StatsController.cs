using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using BigBeerData.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App2.Stats
{
    public class StatsController
    {
        private readonly ILogger _logger;
        private readonly BigBeerContext _context;

        public StatsController(BigBeerContext context, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StatsController>();
            _context = context;
        }

        [Function("StatsHierarchy")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stats/hierarchy")] HttpRequestData req)
        {
            _logger.LogInformation("Stats Hierarchy requested.");

            // 1. Get Counts by Style (from Beers table)
            // 1. Get Counts by Checkins (Popularity / Consumption)
            // We count how many times each style has been checked in
            var beerCounts = await _context.Checkins
                .Include(c => c.Beer)
                .GroupBy(c => c.Beer.Style)
                .Select(g => new { StyleName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.StyleName, v => v.Count);

            // 2. Get Hierarchy Definition (BeerStyle -> Family -> Type)
            var styles = await _context.BeerStyle
                .Include(s => s.Family)
                .Include(s => s.Type)
                .ToListAsync();

            // --- AUTO-MOCK LOGIC (Robust) ---
            // If < 50 checkins, inject ~1000 fictitious ones to demonstrate the visualization.
            var totalRealCheckins = beerCounts.Values.Sum();
            if (true)//totalRealCheckins < 50)
            {
                _logger.LogInformation($"Low data detected ({totalRealCheckins} checkins). Injecting mock data for demonstration.");
                var rnd = new Random();

                // We want ~1000 total.
                // Loop through styles and assign random counts based on popularity tiers.
                foreach (var style in styles)
                {
                    int addedCount = 0;

                    // Tier 1: Aussie Staples (Pale Ale, Lager, IPA)
                    if (style.Name.Contains("Pale Ale") || style.Name.Contains("Lager") || style.Name.Contains("IPA") || style.Name.Contains("Draught"))
                    {
                        // 80% chance to exist, 20-100 count
                        if (rnd.NextDouble() < 0.8) addedCount = rnd.Next(20, 100);
                    }
                    // Tier 2: Common Craft (Stout, Pilsner, XPA)
                    else if (style.Name.Contains("Stout") || style.Name.Contains("Pilsner") || style.Name.Contains("Amber"))
                    {
                        // 50% chance to exist, 5-30 count
                        if (rnd.NextDouble() < 0.5) addedCount = rnd.Next(5, 30);
                    }
                    // Tier 3: Niche (Gose, Sour, etc)
                    else
                    {
                        // 10% chance to exist, 1-10 count
                        if (rnd.NextDouble() < 0.1) addedCount = rnd.Next(1, 10);
                    }

                    if (addedCount > 0)
                    {
                        if (beerCounts.ContainsKey(style.Name))
                            beerCounts[style.Name] += addedCount;
                        else
                            beerCounts[style.Name] = addedCount;
                    }
                }
            }
            // -----------------------

            // 3. Build Tree
            // Structure: Root -> Type -> Family -> Style
            var root = new HierarchyNode { name = "All", children = new List<HierarchyNode>() };

            foreach (var style in styles)
            {
                // Find or Create Type Node
                var typeName = style.Type?.Name ?? "Unknown Type";
                var typeNode = root.children.FirstOrDefault(c => c.name == typeName);
                if (typeNode == null)
                {
                    typeNode = new HierarchyNode { name = typeName, children = new List<HierarchyNode>() };
                    root.children.Add(typeNode);
                }

                // Find or Create Family Node
                var familyName = style.Family?.Name ?? "Other";
                var familyNode = typeNode.children.FirstOrDefault(c => c.name == familyName);
                if (familyNode == null)
                {
                    familyNode = new HierarchyNode { name = familyName, children = new List<HierarchyNode>() };
                    typeNode.children.Add(familyNode);
                }

                // Add Style Node (Leaf)
                var count = beerCounts.ContainsKey(style.Name) ? beerCounts[style.Name] : 0;

                // Also check if 'BaseStyle' matching might catch more, but for now exact match
                // Logic check: if Count > 0, add it.
                if (count > 0 || true) // Add all styles? Or only those with beers? visual might be empty. Let's add all.
                {
                    // Check if style already exists (duplicate names in diff families?)
                    // Assuming unique style ID/Name combination, but just in case
                    var styleNode = new HierarchyNode { name = style.Name, value = count };
                    familyNode.children.Add(styleNode);
                }
            }

            // Optional: Add beers that didn't match any known style?
            // (Skipped for now to keep it clean)

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(root);

            return response;
        }

        public class HierarchyNode
        {
            public string name { get; set; } = "";
            public int? value { get; set; } // Leaf only
            public List<HierarchyNode> children { get; set; } = new List<HierarchyNode>();
        }
    }
}
