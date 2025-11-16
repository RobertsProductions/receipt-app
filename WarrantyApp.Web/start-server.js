#!/usr/bin/env node

// Start Angular dev server with the PORT from environment variable
// Falls back to 4200 if PORT is not set

const { spawn } = require('child_process');

const port = process.env.PORT || '4200';

console.log(`Starting Angular dev server on port ${port}`);

const ngServe = spawn('ng', ['serve', '--proxy-config', 'proxy.conf.mjs', '--port', port], {
  stdio: 'inherit',
  shell: true
});

ngServe.on('error', (error) => {
  console.error(`Error starting Angular dev server: ${error.message}`);
  process.exit(1);
});

ngServe.on('close', (code) => {
  process.exit(code);
});
