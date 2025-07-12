from django.contrib import admin
from .models import *
# Register your models here.
admin.site.register(Friendship)
admin.site.register(FriendRequest)
admin.site.register(GroupMessage)
admin.site.register(ChatGroup)
admin.site.register(Match)
admin.site.register(Player)
admin.site.register(MatchHistory)