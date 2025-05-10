from rest_framework import generics
from django.contrib.auth.models import User
from .serializers import PlayerRegisterSerializer
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework.permissions import IsAuthenticated,AllowAny
from django.db.models import Q
from .models import Player, Friendship
from .serializers import *

class CreatePlayerUserView(generics.CreateAPIView):
    queryset = User.objects.all()
    serializer_class = PlayerRegisterSerializer
    permission_classes = [AllowAny]

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