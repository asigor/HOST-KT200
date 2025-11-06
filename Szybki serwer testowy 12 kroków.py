from http.server import BaseHTTPRequestHandler, HTTPServer
from urllib.parse import urlparse, parse_qs

class H(BaseHTTPRequestHandler):
    def do_GET(self):
        p = urlparse(self.path)
        qs = parse_qs(p.query)
        if p.path.endswith('/weblm/activation.php'):
            self.send_response(200); self.end_headers()
            self.wfile.write(b"OK\n"); self.wfile.write(b"A"*344)  # blob 344
        elif p.path.endswith('/handshake.php'):
            self.send_response(200); self.end_headers(); self.wfile.write(b"OK")
        elif p.path.endswith('/init2024.php'):
            self.send_response(200); self.end_headers()
            self.wfile.write(b"rCIQmkUf-79006b747d6b9022-37860030563130504428||44")
        elif p.path.endswith('/chatgpt.php'):
            self.send_response(200); self.end_headers()
            self.wfile.write(b"fdea43ddb9bc9d879d2a49ad9dc79bce1f1dc6b4")
        elif p.path.endswith('/comm2024.php'):
            self.send_response(200); self.end_headers()
            self.wfile.write(b"A"*344)
        else:
            self.send_response(404); self.end_headers()
    def log_message(self, *a): pass

HTTPServer(('127.0.0.1', 8080), H)()
