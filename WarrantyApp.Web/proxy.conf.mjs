// Aspire injects service URLs in the format: services__<servicename>__http__0
// Prefer HTTPS endpoint if available, fallback to HTTP, then hardcoded
const apiUrl = process.env.services__myapi__https__0 || 
               process.env.services__myapi__http__0 || 
               process.env.API_URL || 
               'https://localhost:7156';  // Backend uses HTTPS

console.log('\n========================================');
console.log('🔧 PROXY CONFIGURATION LOADED');
console.log('========================================');
console.log('API_URL:', process.env.API_URL);
console.log('services__myapi__http__0:', process.env.services__myapi__http__0);
console.log('services__myapi__https__0:', process.env.services__myapi__https__0);
console.log('✅ Using target:', apiUrl);
console.log('========================================\n');

export default {
  '/api': {
    target: apiUrl,
    secure: false,
    changeOrigin: true,
    logLevel: 'debug',
    // Prevent backend URL from being exposed to browser
    followRedirects: true,
    // Rewrite the Location header if backend sends redirects
    onProxyRes: function(proxyRes, req, res) {
      const location = proxyRes.headers['location'];
      if (location) {
        console.log('⚠️ Backend sent redirect Location header:', location);
        // Don't let the browser see backend URLs
        if (location.includes('localhost:7156')) {
          delete proxyRes.headers['location'];
          console.log('🛡️ Removed Location header to prevent direct backend access');
        }
      }
    },
    onProxyReq: function(proxyReq, req, res) {
      console.log(`🔄 Proxying: ${req.method} ${req.url} -> ${apiUrl}${req.url}`);
    },
    onError: function(err, req, res) {
      console.error(`❌ Proxy error: ${err.message}`);
    }
  }
};
