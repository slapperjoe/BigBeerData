using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBeerData.Shared
{
    public class DataState
    {
        public int _revision { get; set; }
        public LocationStyle[] map { get; set; }
        public DataState _previousState { get; set; }
        //object[] colourMap;
        //object[] layers;
        //object[] brewerMap;
        ////UniformData uniformData;
        public bool mapLoaded { get; set; }
        //object? deck;
        public double currentZoom { get; set; }
        //layerSet: null,
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string label { get; set; }
    }

    public class UniformData
    {

        List<object> outSliceLength;
        List<object> outSliceData;
        List<object> colourMap;

    }
}

