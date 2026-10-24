import argparse
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path


class NoCacheHandler(SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header('Cache-Control', 'no-store, no-cache, must-revalidate, max-age=0')
        self.send_header('Pragma', 'no-cache')
        self.send_header('Expires', '0')
        super().end_headers()


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Serve local UML diagrams without caching.')
    parser.add_argument('html_dir', type=Path, help='Directory containing index.html')
    parser.add_argument('--port', type=int, default=8765)
    args = parser.parse_args()

    html_dir = args.html_dir.expanduser().resolve()
    if not (html_dir / 'index.html').is_file():
        parser.error(f'index.html not found in {html_dir}')

    handler = partial(NoCacheHandler, directory=str(html_dir))
    print(f'Serving {html_dir} at http://127.0.0.1:{args.port}/index.html', flush=True)
    ThreadingHTTPServer(('127.0.0.1', args.port), handler).serve_forever()
