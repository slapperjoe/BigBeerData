const path = require('path');

module.exports = {
    entry: './src/index.ts',
    devtool: 'inline-source-map',
    mode: 'development',
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },
        ],
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    output: {
        filename: 'index.bundle.js',
        path: path.resolve(__dirname, '../wwwroot/js'),
        scriptType: 'text/javascript'
    },
};

////const path = require("path");

////module.exports = {
////	module: {
////		rules: [
////			{
////				test: '/\.glsl$/',
////				use: ['raw-loader'],
////			},
////			{
////				test: /\.js$/,
////				enforce: 'pre',
////				use: ['source-map-loader'],
////			},
////			{
////				test: /\.(js|jsx)$/,
////				exclude: /node_modules/,
////				use: {
////					loader: 'babel-loader?cacheDirectory=true',
////				}
////			}
////		]
////	},
////	entry: {
////		'index': './src/index.js',
////		'deckgl': './src/deckgl.mjs',
////	},
////	output: {
////		path: path.resolve(__dirname, '../wwwroot/js'),
////		filename: "[name].bundle.js",
////		library: "[name]"
////	},
////	devtool: 'source-map'
////};