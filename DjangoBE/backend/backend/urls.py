"""
URL configuration for backend project.

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/5.2/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  path('', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  path('', Home.as_view(), name='home')
Including another URLconf
    1. Import the include() function: from django.urls import include, path
    2. Add a URL to urlpatterns:  path('blog/', include('blog.urls'))
"""
from django.contrib import admin
from django.urls import path, include
from api.views import *
from rest_framework_simplejwt.views import TokenObtainPairView, TokenRefreshView
from django.conf import settings
from api.views import (
    CreatePlayerUserView, SearchFriendView, SearchPlayerView, AddFriendView,
    RemoveFriendView, RespondFriendRequestView, GetFriendRequestsView,
    GetFriendRequestCount, VerifyEmailView, send_test_email, CustomTokenObtainPairView
)

def send_verification_email(user_email, token):
    subject = 'Xác thực tài khoản'
    message = f'Nhấn vào đường link sau để xác thực tài khoản của bạn:\nhttp://127.0.0.1:8000/verify-email/?token={token}'
    from_email = settings.DEFAULT_FROM_EMAIL
    recipient_list = [user_email]

    send_mail(subject, message, from_email, recipient_list)  # Gửi email

urlpatterns = [
    path('admin/', admin.site.urls),
    path("player/register/", CreatePlayerUserView.as_view(), name="register"),
    path("token/", CustomTokenObtainPairView.as_view(), name="get_token"),  # Thay TokenObtainPairView
    path("token/refresh/", TokenRefreshView.as_view(), name="refresh"),
    path("api-auth/", include("rest_framework.urls")),
    path("search-friends/", SearchFriendView.as_view(), name="search-friends"),
    path('search-player/', SearchPlayerView.as_view(), name='search-player'),
    path('add-friend/', AddFriendView.as_view(), name='add-friend'),
    path('remove-friend/', RemoveFriendView.as_view(), name='remove-friend'),
    path('respond-friend-request/', RespondFriendRequestView.as_view(), name='respond-friend-request'),
    path('get-friend-requests/', GetFriendRequestsView.as_view(), name='get-friend-requests'),
    path('get-friend-request-count/', GetFriendRequestCount.as_view(), name='get_friend_request_count'),
    path('verify-email/', VerifyEmailView.as_view(), name='verify-email'),
    path('send_test_email/', send_test_email),
    
]
