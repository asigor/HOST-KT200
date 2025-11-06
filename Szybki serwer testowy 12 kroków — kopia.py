from http.server import BaseHTTPRequestHandler, HTTPServer
from urllib.parse import urlparse, parse_qs
from handlers import activation, handshake, init2024, chatgpt, comm2024

class KTHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        p = urlparse(self.path)
        q = parse_qs(p.query)
        route = p.path.lower()

        if route.endswith('/weblm/activation.php'):
            response = activation.handle(q)
        elif route.endswith('/handshake.php'):
            response = handshake.handle(q)
        elif route.endswith('/init2024.php'):
            response = init2024.handle(q)
        elif route.endswith('/chatgpt.php'):
            response = chatgpt.handle(q)
        elif route.endswith('/comm2024.php'):
            response = comm2024.handle(q)
        else:
            response = "ERR\nUnknown endpoint"

        self.send_response(200)
        self.send_header("Content-Type", "text/plain")
        self.end_headers()
        if isinstance(response, str):
            self.wfile.write(response.encode())
        else:
            self.wfile.write(response)

    def log_message(self, *a): 
        pass

if __name__ == "__main__":
    HTTPServer(("127.0.0.1", 8080), KTHandler).serve_forever()
