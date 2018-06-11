var LibraryWebSockets = {
$webSocketInstances: [],

SocketCreate: function(url)
{
	var str = Pointer_stringify(url);
	var socket = {
		socket: new WebSocket(str),
		buffer: new Uint8Array(0),
		error: null,
		messages: []
	}

	socket.socket.binaryType = 'arraybuffer';

	socket.socket.onmessage = function (e) {
		// NOTE
		// 文字列のみ必要なので以下はコメントアウト
		//if (e.data instanceof ArrayBuffer) {
		//	var array = new Uint8Array(e.data);
		//	socket.messages.push(array);
		//}
		//else if (e.data instanceof Blob)
		//{
		//	var reader = new FileReader();
		//	reader.addEventListener("loadend", function() {
		//		var array = new Uint8Array(reader.result);
		//		socket.messages.push(array);
		//	});
		//	reader.readAsArrayBuffer(e.data);
		//}
		if (typeof e.data == 'string')
		{
			// NOTE
			// Recv() 時に最後の文字が切れるので延長。
			// おそらく Recv() でバッファに詰め込む際
			// '\0' 分の長さが足りないので切れていると思われる。
			var s = new String(e.data) + '\0';
			socket.messages.push(s);
		}
	};

	socket.socket.onclose = function (e) {
		if (e.code != 1000)
		{
			if (e.reason != null && e.reason.length > 0)
				socket.error = e.reason;
			else
			{
				switch (e.code)
				{
					case 1001: 
						socket.error = "Endpoint going away.";
						break;
					case 1002: 
						socket.error = "Protocol error.";
						break;
					case 1003: 
						socket.error = "Unsupported message.";
						break;
					case 1005: 
						socket.error = "No status.";
						break;
					case 1006: 
						socket.error = "Abnormal disconnection.";
						break;
					case 1009: 
						socket.error = "Data frame too large.";
						break;
					default:
						socket.error = "Error "+e.code;
				}
			}
		}
	}
	var instance = webSocketInstances.push(socket) - 1;
	return instance;
},

SocketState: function (socketInstance)
{
	var socket = webSocketInstances[socketInstance];
	return socket.socket.readyState;
},

SocketError: function (socketInstance, ptr, bufsize)
{
 	var socket = webSocketInstances[socketInstance];
 	if (socket.error == null)
 		return 0;
    var str = socket.error.slice(0, Math.max(0, bufsize - 1));
    stringToUTF8(str, ptr, bufsize);
	return 1;
},

SocketSend: function (socketInstance, ptr, length)
{
	var socket = webSocketInstances[socketInstance];
	socket.socket.send (HEAPU8.buffer.slice(ptr, ptr+length));
},

SocketRecvLength: function(socketInstance)
{
	var socket = webSocketInstances[socketInstance];
	if (socket.messages.length == 0)
		return 0;
	return socket.messages[0].length;
},

SocketRecv: function (socketInstance, ptr, length)
{
	var socket = webSocketInstances[socketInstance];
	if (socket.messages.length == 0)
		return 0;
	if (socket.messages[0].length > length)
		return 0;
	stringToUTF8(socket.messages[0], ptr, length);
	socket.messages = socket.messages.slice(1);
},

SocketClose: function (socketInstance)
{
	var socket = webSocketInstances[socketInstance];
	socket.socket.close();
}
};

autoAddDeps(LibraryWebSockets, '$webSocketInstances');
mergeInto(LibraryManager.library, LibraryWebSockets);
