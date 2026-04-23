const http = require('http');
const { WebSocketServer } = require('ws');
const { v4: uuidv4 } = require('uuid');

// In-memory state tracking
const gameState = {
    players: {}, // id -> { name, position, stats }
    bombs: []    // Array of { id, position, ownerId }
};

// 1. Create HTTP Server for UnityWebRequest
const server = http.createServer((req, res) => {
    // Enable CORS for Unity
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type');

    if (req.method === 'OPTIONS') {
        res.writeHead(204);
        res.end();
        return;
    }

    if (req.url === '/status' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        const status = {
            online: true,
            playerCount: Object.keys(gameState.players).length,
            bombCount: gameState.bombs.length,
            uptime: process.uptime()
        };
        res.end(JSON.stringify(status));
    } else {
        res.writeHead(404);
        res.end(JSON.stringify({ error: 'Not Found' }));
    }
});

// 2. Attach WebSocket Server to the same HTTP Server
const wss = new WebSocketServer({ server });

const clients = new Map(); // ws -> clientId

wss.on('connection', (ws) => {
    const clientId = uuidv4();
    clients.set(ws, clientId);
    console.log(`[SERVER] Client connected: ${clientId}`);

    // Send initial state to the newly connected client
    ws.send(JSON.stringify({
        type: 'initial_state',
        payload: gameState
    }));

    ws.on('message', (data) => {
        try {
            const message = JSON.parse(data);
            console.log(`[SERVER] Received from ${clientId}:`, message.type);

            handleMessage(ws, clientId, message);
        } catch (err) {
            console.error('[SERVER] Error parsing message:', err.message);
        }
    });

    ws.on('close', () => {
        console.log(`[SERVER] Client disconnected: ${clientId}`);
        clients.delete(ws);
        
        // Remove player from state if it was a player
        if (gameState.players[clientId]) {
            delete gameState.players[clientId];
            broadcast({
                type: 'player_left',
                payload: { id: clientId }
            }, ws);
        }
    });
});

function handleMessage(ws, clientId, message) {
    const { type, payload } = message;

    switch (type) {
        case 'player_join':
            gameState.players[clientId] = {
                id: clientId,
                name: payload.name || 'Anonymous',
                position: payload.position || { x: 0, y: 0 },
                stats: payload.stats || {}
            };
            broadcast(message, ws);
            break;

        case 'player_move':
            if (gameState.players[clientId]) {
                gameState.players[clientId].position = payload.position;
                broadcast(message, ws);
            }
            break;

        case 'bomb_placed':
            const bombId = uuidv4();
            const newBomb = {
                id: bombId,
                position: payload.position,
                ownerId: clientId
            };
            gameState.bombs.push(newBomb);
            broadcast({
                type: 'bomb_spawned',
                payload: newBomb
            }); // Broadcast to everyone including sender to confirm ID
            break;

        case 'bomb_exploded':
            gameState.bombs = gameState.bombs.filter(b => b.id !== payload.id);
            broadcast(message, ws);
            break;

        default:
            // Default behavior: broadcast any other message type
            broadcast(message, ws);
            break;
    }
}

function broadcast(message, senderWs = null) {
    const data = JSON.stringify(message);
    clients.forEach((id, clientWs) => {
        if (clientWs !== senderWs && clientWs.readyState === 1) { // 1 = OPEN
            clientWs.send(data);
        }
    });
}

// 3. Start the combined server
server.listen(8080, () => {
    console.log('[SERVER] Combined HTTP & WebSocket Server started on port 8080');
    console.log('[SERVER] Status endpoint available at http://localhost:8080/status');
});
