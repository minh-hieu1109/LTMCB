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
        extra_kwargs = {'password': {'write_only': True}}

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

    def create(self, validated_data):
        user = User.objects.create_user(
            username=validated_data["username"],
            password=validated_data["password"],
            first_name=validated_data["first_name"],
            last_name=validated_data["last_name"],
            email=validated_data["email"]
        )

        Player.objects.create(
            user=user,
            nickname=user.username
        )

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