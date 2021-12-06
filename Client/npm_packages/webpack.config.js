const path = require("path");

module.exports = {
	module: {
		rules: [
			{
				test: '/\.glsl$/',
				use: ['raw-loader'],
			},
			{
				test: /\.js$/,
				enforce: 'pre',
				use: ['source-map-loader'],
			},
			{
				test: /\.(js|jsx)$/,
				exclude: /node_modules/,
				use: {
					loader: 'babel-loader?cacheDirectory=true',
				}
			}
		]
	},
	entry: {
		'index': './src/index.js',
		'deckgl': './src/deckgl.mjs',
	},
	output: {
		path: path.resolve(__dirname, '../wwwroot/js'),
		filename: "[name].bundle.js",
		library: "[name]"
	},
	devtool: 'source-map'
};