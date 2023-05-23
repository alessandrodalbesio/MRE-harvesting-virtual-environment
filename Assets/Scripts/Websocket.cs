using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketReceiver : MonoBehaviour {
    private const int BufferSize = 1024;
    private const string WebSocketUrl = "ws://localhost:9500";

    private string[] REFRESH_WEBSOCKET_MESSAGES = new string[]{"new-model", "update-model", "delete-model", "new-texture", "delete-texture", "texture-set-default"};
    private string SET_ACTIVE_MODEL_MESSAGE = "set-active-model";
    private string UNSET_ACTIVE_MODEL_MESSAGE = "unset-active-model";

    private async Task ReceiveWebSocketMessages() {
        using (ClientWebSocket webSocket = new ClientWebSocket()) {
            try {
                await webSocket.ConnectAsync(new Uri(WebSocketUrl), CancellationToken.None);
                Debug.Log("WebSocket connection established.");

                while (webSocket.State == WebSocketState.Open) {
                    byte[] buffer = new byte[BufferSize];
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text) {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        if (Array.Exists(REFRESH_WEBSOCKET_MESSAGES, element => element == result.MessageType.ToString())) {
                            this.GetComponent<DataSynchronizer>().refreshDatabase();
                        } else if (message == SET_ACTIVE_MODEL_MESSAGE) {
                            this.GetComponent<DataSynchronizer>().setActiveModelFromServerResponse(message);
                        } else if (message == UNSET_ACTIVE_MODEL_MESSAGE) {
                            this.GetComponent<DataSynchronizer>().unsetActiveModelFromServer();
                        }
                    }
                }
            }
            catch (Exception ex) {
                Debug.LogError("WebSocket error: " + ex.Message);
            }
            finally {
                if (webSocket.State == WebSocketState.Open)
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "WebSocket connection closed", CancellationToken.None);
            }
        }
    }

    private async void Start() {
        await ReceiveWebSocketMessages();
    }
}