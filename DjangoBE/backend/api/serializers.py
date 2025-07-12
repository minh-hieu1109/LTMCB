from django.contrib.auth.models import User
from rest_framework import serializers
from .models import *

class PlayerRegisterSerializer(serializers.ModelSerializer):
    first_name = serializers.CharField(write_only=True)
    last_name = serializers.CharField(write_only=True)
    email = serializers.EmailField(write_only=True)
    password = serializers.CharField(write_only=True)

    class Meta:
        model = User
        fields = ['id', 'username', 'password', 'first_name', 'last_name', 'email']
        # Không cần extra_kwargs vì các field đã được khai báo write_only bên trên

    # Kiểm tra tính hợp lệ của email
    def validate_email(self, value):
        if User.objects.filter(email=value).exists():
            raise serializers.ValidationError("Email đã tồn tại.")
        return value

    # Kiểm tra tính hợp lệ của username
    def validate_username(self, value):
        if User.objects.filter(username=value).exists():
            raise serializers.ValidationError("Username đã tồn tại.")
        return value

    # Tạo mới User và Player tương ứng
    def create(self, validated_data):
        # Dùng create_user để đảm bảo mật khẩu được mã hóa
        user = User.objects.create_user(**validated_data)
        # Tạo Player tương ứng với nickname = username
        Player.objects.create(user=user, nickname=user.username)
        return user

class PlayerSearchSerializer(serializers.ModelSerializer):
    class Meta:
        model = Player
        fields = ['id', 'nickname', 'score', 'coin', 'is_online']

class AddFriendSerializer(serializers.Serializer):
    player_id = serializers.IntegerField()

class RespondFriendRequestSerializer(serializers.Serializer):
    request_id = serializers.IntegerField()
    action = serializers.ChoiceField(choices=['accept', 'reject'])

class FriendRequestSerializer(serializers.ModelSerializer):
    class Meta:
        model = FriendRequest
        fields = ['id', 'from_player', 'to_player', 'created_at']

class FriendRequestCountSerializer(serializers.Serializer):
    request_count = serializers.IntegerField()

class MatchSerializer(serializers.ModelSerializer):
    class Meta:
        model = Match
        fields = '__all__'