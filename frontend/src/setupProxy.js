// Proxy middleware temporarily disabled - using package.json proxy instead
// const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function(app) {
  console.log('🔧 Proxy middleware disabled - using package.json proxy');
  
  // app.use(
  //   '/api',
  //   createProxyMiddleware({
  //     target: 'http://localhost:5000',
  //     changeOrigin: true,
  //     secure: false,
  //     logLevel: 'debug',
  //     onProxyReq: (proxyReq, req, res) => {
  //       console.log(`🔄 Proxy: ${req.method} ${req.url} -> http://localhost:5000${req.url}`);
  //     },
  //     onProxyRes: (proxyRes, req, res) => {
  //       console.log(`✅ Proxy Response: ${proxyRes.statusCode} for ${req.url}`);
  //     },
  //     onError: (err, req, res) => {
  //       console.error(`❌ Proxy Error for ${req.url}:`, err.message);
  //     }
  //   })
  // );
};

