import os
import re
from rest_framework import generics
from django.contrib.auth.models import User
from .serializers import PlayerRegisterSerializer
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework.permissions import IsAuthenticated, AllowAny
from django.db.models import Q
from .models import Player, Friendship, FriendRequest
from .serializers import *
from django.utils.crypto import get_random_string
from django.core.cache import cache
from django.core.mail import send_mail
from django.http import HttpResponse
from rest_framework import serializers

# TEST GỬI EMAIL
def send_test_email(request):
    try:
        send_mail(
            subject='Test Email from Django',
            message='This is a test email.',
            from_email=os.getenv('DEFAULT_FROM_EMAIL', 'anhquan02114869@gmail.com'),
            recipient_list=['anhquan02114869@gmail.com'],
            fail_silently=False,
        )
        print("[DEBUG] Test email sent successfully")
        return HttpResponse("Email đã được gửi thành công!")
    except Exception as e:
        print(f"[ERROR] Gửi email thất bại: {e}")
        return HttpResponse(f"Không thể gửi email: {e}")

# ĐĂNG KÝ NGƯỜI DÙNG + GỬI MÃ XÁC THỰC
class CreatePlayerUserView(generics.CreateAPIView):
    queryset = User.objects.all()
    serializer_class = PlayerRegisterSerializer
    permission_classes = [AllowAny]

    def perform_create(self, serializer):
        email = serializer.validated_data.get("email")
        username = serializer.validated_data.get("username")
        print(f"[DEBUG] Received data: username={username}, email={email}")
        
        # Kiểm tra định dạng email
        if not re.match(r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$', email):
            raise serializers.ValidationError("Email không hợp lệ.")
        
        if User.objects.filter(email=email).exists():
            raise serializers.ValidationError("Email đã được sử dụng.")
        if User.objects.filter(username=username).exists():
            raise serializers.ValidationError("Username đã được sử dụng.")
        
        # Tạo user
        user = serializer.save(is_active=False)
        verification_code = get_random_string(length=6, allowed_chars='0123456789')
        cache.set(f"email_verification_{user.username}", verification_code, timeout=600)
        
        try:
            send_mail(
                subject='Xác thực tài khoản Unity Game',
                message=f'Chào bạn,\n\nMã xác thực tài khoản Unity Game của bạn là: {verification_code}\n\nVui lòng nhập mã này trong ứng dụng để kích hoạt tài khoản.\n\nTrân trọng,\nUnity Game Team',
                from_email=os.getenv('DEFAULT_FROM_EMAIL', 'anhquan02114869@gmail.com'),
                recipient_list=[email],
                fail_silently=False,
            )
            print(f"[DEBUG] Sent code {verification_code} to {email}")
        except Exception as e:
            user.delete()
            print(f"[ERROR] Gửi email thất bại: {e}")
            raise serializers.ValidationError(f"Không thể gửi mã xác thực: {e}")

    def create(self, request, *args, **kwargs):
        print(f"[DEBUG] Request data: {request.data}")
        serializer = self.get_serializer(data=request.data)
        serializer.is_valid(raise_exception=True)
        self.perform_create(serializer)
        username = serializer.validated_data['username']
        email = serializer.validated_data['email']
        return Response({
            "message": "Tài khoản đã được tạo. Vui lòng kiểm tra email để xác thực.",
            "username": username,
            "email": email
        }, status=201)

# XÁC THỰC EMAIL TỪ NGƯỜI DÙNG
class VerifyEmailView(APIView):
    permission_classes = [AllowAny]

    def get(self, request):
        username = request.GET.get("username")
        token = request.GET.get("token")

        if not username or not token:
            return Response({"error": "Thiếu username hoặc token"}, status=400)

        verification_code = cache.get(f"email_verification_{username}")
        if verification_code != token:
            return Response({"error": "Mã xác thực không đúng hoặc đã hết hạn"}, status=400)

        try:
            user = User.objects.get(username=username)
            user.is_active = True
            user.save()
            cache.delete(f"email_verification_{username}")
            return Response({"message": "Xác thực email thành công!"})
        except User.DoesNotExist:
            return Response({"error": "Không tìm thấy người dùng"}, status=404)

# backend/api/views.py
from rest_framework_simplejwt.views import TokenObtainPairView
from rest_framework import serializers

class CustomTokenObtainPairView(TokenObtainPairView):
    def post(self, request, *args, **kwargs):
        user = User.objects.filter(username=request.data.get('username')).first()
        if user and not user.is_active:
            raise serializers.ValidationError("Tài khoản chưa được xác thực qua email.")
        return super().post(request, *args, **kwargs)

# Các view khác
class SearchFriendView(APIView):
    permission_classes = [IsAuthenticated]
    def get(self, request):
        query = request.GET.get("q", "")
        try:
            me = Player.objects.get(user=request.user)
        except Player.DoesNotExist:
            return Response({"error": "Player not found"}, status=404)

        friendships = Friendship.objects.filter(Q(from_player=me) | Q(to_player=me))
        friend_ids = set()
        for f in friendships:
            friend_ids.add(f.from_player.id)
            friend_ids.add(f.to_player.id)

        players = Player.objects.filter(nickname__icontains=query)\
                                .exclude(id__in=friend_ids)\
                                .exclude(id=me.id)

        serializer = PlayerSearchSerializer(players, many=True)
        return Response(serializer.data)

# Các view tiếp theo giữ nguyên như cũ

class SearchPlayerView(APIView):
    permission_classes = [AllowAny]

    def get(self, request):
        query = request.GET.get("q", "")
        players = Player.objects.filter(nickname__icontains=query)
        if not players:
            return Response({"error": "No players found"}, status=404)
        serializer = PlayerSearchSerializer(players, many=True)
        return Response(serializer.data)
    
class AddFriendView(APIView):
    permission_classes = [IsAuthenticated]

    def post(self, request):
        serializer = AddFriendSerializer(data=request.data)
        if serializer.is_valid():
            player_id = serializer.validated_data['player_id']
            me = Player.objects.get(user=request.user)
            if me.id == player_id:
                return Response({"error": "You cannot friend yourself"}, status=400)
            try:
                friend = Player.objects.get(id=player_id)
            except Player.DoesNotExist:
                return Response({"error": "Player not found"}, status=404)
            if FriendRequest.objects.filter(from_player=me, to_player=friend).exists():
                return Response({"error": "Friend request already sent"}, status=400)
            if Friendship.objects.filter(from_player=me, to_player=friend).exists() or \
               Friendship.objects.filter(from_player=friend, to_player=me).exists():
                return Response({"error": "You are already friends"}, status=400)
            FriendRequest.objects.create(from_player=me, to_player=friend)
            return Response({"message": "Friend request sent"})
        
        return Response(serializer.errors, status=400)
    
class RemoveFriendView(APIView):
    permission_classes = [IsAuthenticated]

    def post(self, request):
        serializer = AddFriendSerializer(data=request.data)
        if serializer.is_valid():
            player_id = serializer.validated_data['player_id']
            me = Player.objects.get(user=request.user)

            try:
                friend = Player.objects.get(id=player_id)
            except Player.DoesNotExist:
                return Response({"error": "Player not found"})

            friendship = Friendship.objects.filter(from_player=me, to_player=friend).first() or \
                         Friendship.objects.filter(from_player=friend, to_player=me).first()

            if not friendship:
                return Response({"error": "You are not friends"})

            friendship.delete()
            return Response({"message": f"You are no longer friends with {friend.nickname}"})
        
        return Response(serializer.errors)

class RespondFriendRequestView(APIView):
    permission_classes = [IsAuthenticated]

    def post(self, request):
        serializer = RespondFriendRequestSerializer(data=request.data)
        if serializer.is_valid():
            request_id = serializer.validated_data['request_id']
            action = serializer.validated_data['action']
            me = Player.objects.get(user=request.user)

            try:
                friend_request = FriendRequest.objects.get(id=request_id, to_player=me)
            except FriendRequest.DoesNotExist:
                return Response({"error": "Friend request not found"}, status=404)

            if friend_request.accepted is not None:
                return Response({"error": "Friend request already responded"}, status=400)

            if action == "accept":
                friend_request.accepted = True
                friend_request.save()
                Friendship.objects.create(from_player=friend_request.from_player, to_player=friend_request.to_player)
                return Response({"message": "Friend request accepted"})
            else:
                friend_request.accepted = False
                friend_request.save()
                return Response({"message": "Friend request rejected"})

        return Response(serializer.errors, status=400)
    
class GetFriendRequestsView(APIView):
    permission_classes = [IsAuthenticated]

    def get(self, request):
        # Lấy người chơi hiện tại
        me = Player.objects.get(user=request.user)
        
        # Lấy tất cả yêu cầu kết bạn của người chơi mà chưa được chấp nhận hoặc từ chối
        friend_requests = FriendRequest.objects.filter(to_player=me, accepted__isnull=True)
        
        # Serialize dữ liệu yêu cầu kết bạn
        serializer = FriendRequestSerializer(friend_requests, many=True)
        
        return Response(serializer.data)
    
class GetFriendRequestCount(APIView):
    permission_classes = [IsAuthenticated]

    def get(self, request):
        me = Player.objects.get(user=request.user)
        friend_requests_count = FriendRequest.objects.filter(to_player=me, accepted__isnull=True).count()
        return Response({'request_count': friend_requests_count})
