import base64, hashlib

def b64_sha1(s: str) -> str:
    return base64.b64encode(hashlib.sha1(s.encode('utf-8')).digest()).decode('ascii')

# przykład:
# print(b64_sha1(f"{code}|{hwid_hex}|{ts}"))
