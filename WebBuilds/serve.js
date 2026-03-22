const http = require('http');
const fs = require('fs');
const path = require('path');

const MIME_TYPES = {
    '.html': 'text/html',
    '.js': 'application/javascript',
    '.wasm': 'application/wasm',
    '.data': 'application/octet-stream',
    '.json': 'application/json',
};

http.createServer((req, res) => {
    let filePath = '.' + req.url;
    if (filePath === './') filePath = './index.html';

    // Strip .gz to determine the real content type
    const isGzip = filePath.endsWith('.gz');
    const basePath = isGzip ? filePath.slice(0, -3) : filePath;
    const ext = path.extname(basePath);
    const contentType = MIME_TYPES[ext] || 'application/octet-stream';

    fs.readFile(filePath, (err, data) => {
        if (err) { res.writeHead(404); res.end(); return; }

        res.setHeader('Content-Type', contentType);
        if (isGzip) res.setHeader('Content-Encoding', 'gzip');

        res.writeHead(200);
        res.end(data);
    });
}).listen(8080, () => console.log('Serving on http://localhost:8080'));