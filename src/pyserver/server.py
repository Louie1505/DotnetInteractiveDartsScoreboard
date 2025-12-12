import asyncio
from websockets.asyncio.server import serve
from websockets.exceptions import ConnectionClosed
from get_scores import GetScores
from video_processing import VideoProcessing


class DartMonitorServer:
    def launch(self):
        async def handler(websocket):
            self.clients.add(websocket)
            try:
                await websocket.wait_closed()
            finally:
                self.clients.remove(websocket)

        async def send(websocket, message):
            try:
                await websocket.send(message)
            except ConnectionClosed:
                pass

        async def broadcast(message):
            for websocket in self.clients:
                asyncio.create_task(self.send(websocket, message))
        async def broadcast_messages():
            while True:
                await asyncio.sleep(1)
                message = "is a message"
                await broadcast(message)

        async def main():
            async with serve(handler, "localhost", 8765):
                self.video_processing = VideoProcessing()
                self.clients = set()
                self.video_processing.start()
        asyncio.run(main())

if __name__ == '__main__':
    DartMonitorServer().launch()
