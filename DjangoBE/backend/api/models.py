from django.db import models

from django.db import models
from django.contrib.auth.models import User

class Player(models.Model):
    user = models.OneToOneField(User, on_delete=models.CASCADE)
    nickname = models.CharField(max_length=30, unique=True)
    score = models.IntegerField(default=0)
    coin = models.IntegerField(default=0)
    is_online = models.BooleanField(default=False)
    last_login = models.DateTimeField(auto_now=True)

    def __str__(self):
        return self.nickname

class Friendship(models.Model):
    from_player = models.ForeignKey(Player, related_name='from_player', on_delete=models.CASCADE)
    to_player = models.ForeignKey(Player, related_name='to_player', on_delete=models.CASCADE)
    created_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        unique_together = ('from_player', 'to_player')  # Đảm bảo mỗi cặp bạn bè chỉ xuất hiện một lần

    def __str__(self):
        return f"{self.from_player.nickname} is friends with {self.to_player.nickname}"

class GameStats(models.Model):
    player = models.OneToOneField(Player, on_delete=models.CASCADE)  # Liên kết với Player
    money_collected = models.IntegerField(default=0)  # Số tiền người chơi đã kiếm được trong game
    kills = models.IntegerField(default=0)  # Số lượng kills của người chơi
    deaths = models.IntegerField(default=0)  # Số lượng deaths của người chơi

    def __str__(self):
        return f"{self.player.nickname} - Kills: {self.kills}, Deaths: {self.deaths}"

    def get_kd_ratio(self):
        return self.kills / self.deaths if self.deaths > 0 else self.kills
    
class Match(models.Model):
    room_code = models.CharField(max_length=8, unique=True)  # Mã phòng game
    max_players = models.IntegerField()  # Số người chơi tối đa trong phòng
    is_active = models.BooleanField(default=True)  # Trạng thái phòng game (hoạt động hay không)
    current_players = models.ManyToManyField(Player)  # Liên kết với người chơi trong phòng
    created_at = models.DateTimeField(auto_now_add=True)  # Thời gian tạo phòng game

    def __str__(self):
        return f"Room {self.room_code} - {self.current_players.count()}/{self.max_players} players"

class MatchHistory(models.Model):
    player = models.ForeignKey(Player, on_delete=models.CASCADE)  # Liên kết với người chơi
    match = models.ForeignKey(Match, on_delete=models.CASCADE)  # Liên kết với trận đấu
    kills = models.IntegerField(default=0)  # Số kills của người chơi trong trận đấu
    deaths = models.IntegerField(default=0)  # Số deaths của người chơi trong trận đấu
    money_collected = models.IntegerField(default=0)  # Tiền kiếm được trong trận đấu
    match_date = models.DateTimeField(auto_now_add=True)  # Thời gian tham gia trận đấu

    def __str__(self):
        return f"{self.player.nickname} - {self.match.room_code} - Kills: {self.kills}, Deaths: {self.deaths}"

    def get_kd_ratio(self):
        # Tính tỷ lệ kills/deaths
        return self.kills / self.deaths if self.deaths > 0 else self.kills
    


class ChatGroup(models.Model):
    name = models.CharField(max_length=100, unique=True)
    members = models.ManyToManyField('Player', related_name='groups')
    created_at = models.DateTimeField(auto_now_add=True)

    def __str__(self):
        return self.name

class GroupMessage(models.Model):
    group = models.ForeignKey(ChatGroup, related_name='messages', on_delete=models.CASCADE)
    sender = models.ForeignKey(Player, related_name='group_messages', on_delete=models.CASCADE)
    content = models.TextField()
    created_at = models.DateTimeField(auto_now_add=True)

    def __str__(self):
        return f"[{self.group.name}] {self.sender.nickname}: {self.content[:20]}"

class FriendRequest(models.Model):
    from_player = models.ForeignKey(Player, related_name='sent_requests', on_delete=models.CASCADE)
    to_player = models.ForeignKey(Player, related_name='received_requests', on_delete=models.CASCADE)
    created_at = models.DateTimeField(auto_now_add=True)
    accepted = models.BooleanField(null=True)  # None = pending, True = accepted, False = rejected

    class Meta:
        unique_together = ('from_player', 'to_player')