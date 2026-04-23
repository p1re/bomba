const WebSocket = require('ws');

const ws = new WebSocket('ws://localhost:8080');

ws.on('open', function open() {
    console.log('[CLIENT] Connected to server');
    
    // Simulate a player joining
    const joinMsg = JSON.stringify({
        type: 'player_join',
        payload: {
            name: 'TestPlayer',
            position: { x: 5, y: -2 }
        }
    });
    ws.send(joinMsg);

    // Simulate placing a bomb after 2 seconds
    setTimeout(() => {
        const bombMsg = JSON.stringify({
            type: 'bomb_placed',
            payload: {
                position: { x: 5, y: -2 }
            }
        });
        console.log('[CLIENT] Sending bomb_placed');
        ws.send(bombMsg);
    }, 2000);
});

ws.on('message', function message(data) {
    const msg = JSON.parse(data);
    console.log('[CLIENT] Received from server:', msg.type, msg.payload);
});

ws.on('close', function close() {
    console.log('[CLIENT] Disconnected');
});
