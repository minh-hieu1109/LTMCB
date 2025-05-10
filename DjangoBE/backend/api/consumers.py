from channels.generic.websocket import WebsocketConsumer
from asgiref.sync import async_to_sync
from .models import *
import json

class ChatroomConsumer(WebsocketConsumer):
    def connect(self):
        # Nhận thông tin người dùng và tên phòng chat từ URL
        self.user = self.scope['user']
        self.chatroom_name = self.scope['url_route']['kwargs']['chatroom_name']
        try:
            self.chatroom = ChatGroup.objects.get(name=self.chatroom_name)
        except ChatGroup.DoesNotExist:
            self.chatroom = ChatGroup.objects.create(name=self.chatroom_name)

        # Tham gia vào nhóm chat của phòng chat
        async_to_sync(self.channel_layer.group_add)(self.chatroom_name, self.channel_name)

        # Chấp nhận kết nối WebSocket
        self.accept()

    def disconnect(self, close_code):
        # Rời nhóm chat khi kết nối bị ngắt
        async_to_sync(self.channel_layer.group_discard)(self.chatroom_name, self.channel_name)

    def receive(self, text_data):
        message = json.loads(text_data)

        content = message.get('body')
        player_id = message.get('player_id')
        group_name = message.get('group')

        if not content or not player_id or not group_name:
            self.send(text_data=json.dumps({'error': 'Missing content, player_id, or group'}))
            return

        player = Player.objects.get(id=player_id)
        group = ChatGroup.objects.get(name=group_name)

        # Tạo và lưu GroupMessage
        group_message = GroupMessage.objects.create(
            group=group,
            sender=player,
            content=content
        )

        # Gửi tin nhắn đến tất cả client trong nhóm chat
        async_to_sync(self.channel_layer.group_send)(
            self.chatroom_name,
            {
                'type': 'message_handler',
                'message_id': group_message.id,
            }
        )

    def message_handler(self, event):
        message_id = event['message_id']
        message = GroupMessage.objects.get(id=message_id)

        # Gửi lại tin nhắn cho tất cả các client
        self.send(text_data=json.dumps({
            'content': message.content,
            'sender': message.sender.nickname,
            'group': message.group.name,
            'created_at': message.created_at.isoformat(),
        }))
