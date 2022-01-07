const path = require('path');

module.exports = {
	entry: ['./src/index.tsx','../wwwroot/css/app.less'],
	devtool: 'source-map',
	mode: 'production',
	module: {
		rules: [
			{
				test: /\.tsx?$/,
				use: 'ts-loader',
				exclude: /node_modules/,
			},
			{
				test: /\.less$/i,
				use: [
					// compiles Less to CSS
					"style-loader",
					{
            loader: "css-loader",
            options: {
              sourceMap: true,
            },
          },
          {
            loader: "less-loader",
            options: {
              sourceMap: true,
            },
          },
				],
			},
		],
	},
	resolve: {
		extensions: ['.tsx', '.ts', '.js'],
	},
	output: {
		filename: 'index.bundle.js',
		path: path.resolve(__dirname, '../wwwroot/js'),
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