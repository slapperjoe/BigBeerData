using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BigBeerData.Shared;
using Microsoft.JSInterop;

namespace Client.Services
{
    public class BrowserService
    {

        public DataState state { get; set; }

        private readonly IJSRuntime _js;

        public BrowserService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<BrowserDimension> GetDimensions()
        {
            return await _js.InvokeAsync<BrowserDimension>("interop.getDimensions");
        }

        public async Task<BrowserDimension> GetRenderArea()
        {
            return await _js.InvokeAsync<BrowserDimension>("interop.getRenderArea");
        }

        public async Task<bool> ConsoleLog(string logContent)
        {
            return await _js.InvokeAsync<bool>("interop.consoleLog", new[] { logContent });
        }

        public async Task<bool> InitDeckGL(double longitude, double latitude, int zoom)
        {
            await _js.InvokeAsync<bool>("interop.InitDeckGL", longitude, latitude, zoom);
            return true;
        }

        public async Task<bool> FlyTo(double longitude, double latitude, int zoom)
        {
            await _js.InvokeAsync<bool>("interop.FlyTo", longitude, latitude, zoom);
            return true;
        }

        public async Task<bool> AddColumnChartPoint(double zoom)
        {
            await _js.InvokeAsync<bool>("interop.AddColumnChartPoint", zoom);
            return true;
        }

        public async Task<bool> SetMapState(LocationStyle[] styles,
          DotNetObjectReference<Pages.Index> dotNetRef)
        {
            await _js.InvokeVoidAsync("interop.hookDotNet", dotNetRef);
            await _js.InvokeAsync<DataState>("interop.SetMapState", styles);
            return true;
        }

        public async Task<bool> UpdateStyles(LocationStyle[] styles)
        {
            await _js.InvokeAsync<dynamic>("interop.SetMapState", styles);
            return true;
        }

    }

    public class BrowserDimension
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
