# chat/middleware.py
from urllib.parse import parse_qs
from channels.middleware import BaseMiddleware
from django.contrib.auth.models import AnonymousUser
from django.contrib.auth import get_user_model
from channels.db import database_sync_to_async
from rest_framework_simplejwt.tokens import UntypedToken
from rest_framework_simplejwt.exceptions import InvalidToken, TokenError
from django.conf import settings
from jwt import decode as jwt_decode  # PyJWT

User = get_user_model()

@database_sync_to_async
def get_user(token):
    try:
        # Kiểm tra token có hợp lệ không
        UntypedToken(token)

        # Giải mã token để lấy user_id (payload)
        decoded_data = jwt_decode(token, settings.SECRET_KEY, algorithms=["HS256"])
        user_id = decoded_data.get("user_id")

        # Lấy user từ DB
        return User.objects.get(id=user_id)
    except Exception as e:
        print("❌ Lỗi xác thực JWT:", e)
        return AnonymousUser()
        
class JWTAuthMiddleware(BaseMiddleware):
    async def __call__(self, scope, receive, send):
        query_string = scope["query_string"].decode()
        token = parse_qs(query_string).get("token", [None])[0]

        if token:
            scope["user"] = await get_user(token)
        else:
            scope["user"] = AnonymousUser()

        return await super().__call__(scope, receive, send)
