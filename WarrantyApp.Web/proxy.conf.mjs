export default function() {
  // Aspire injects service URLs in the format: services__<servicename>__http__0
  const apiUrl = process.env.API_URL || 
                 process.env.services__myapi__http__0 || 
                 'http://localhost:5000';
  
  console.log('Proxy configuration:');
  console.log('  API_URL:', process.env.API_URL);
  console.log('  services__myapi__http__0:', process.env.services__myapi__http__0);
  console.log('  Using target:', apiUrl);
  
  return {
    '/api': {
      target: apiUrl,
      secure: false,
      changeOrigin: true,
      logLevel: 'debug'
    }
  };
}
