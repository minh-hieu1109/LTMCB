# chat/middleware_stack.py
from channels.auth import AuthMiddlewareStack
from .middleware import JWTAuthMiddleware

def JWTAuthMiddlewareStack(inner):
    return JWTAuthMiddleware(AuthMiddlewareStack(inner))
