// Aspire injects service URLs in the format: services__<servicename>__http__0
const apiUrl = process.env.services__myapi__http__0 || 
               process.env.API_URL || 
               'https://localhost:7156';  // Backend uses HTTPS

console.log('\n========================================');
console.log('🔧 PROXY CONFIGURATION LOADED');
console.log('========================================');
console.log('API_URL:', process.env.API_URL);
console.log('services__myapi__http__0:', process.env.services__myapi__http__0);
console.log('✅ Using target:', apiUrl);
console.log('========================================\n');

export default {
  '/api': {
    target: apiUrl,
    secure: false,
    changeOrigin: true,
    logLevel: 'debug',
    onProxyReq: function(proxyReq, req, res) {
      console.log(`🔄 Proxying: ${req.method} ${req.url} -> ${apiUrl}${req.url}`);
    },
    onError: function(err, req, res) {
      console.error(`❌ Proxy error: ${err.message}`);
    }
  }
};
