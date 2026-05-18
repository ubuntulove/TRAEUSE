// Game constants
const CANVAS_WIDTH = window.innerWidth;
const CANVAS_HEIGHT = window.innerHeight;
const PLAYER_SIZE = 30;
const BULLET_SIZE = 5;
const ENEMY_SIZE = 25;
const PLAYER_SPEED = 5;
const BULLET_SPEED = 10;
const ENEMY_SPEED = 2;
const MAX_AMMO = 30;
const MAX_RESERVE = 120;

// Game state
let player = {
    x: CANVAS_WIDTH / 2,
    y: CANVAS_HEIGHT / 2,
    health: 100,
    ammo: MAX_AMMO,
    reserve: MAX_RESERVE,
    angle: 0
};

let bullets = [];
let enemies = [];
let score = 0;
let keys = {};
let mouse = { x: CANVAS_WIDTH / 2, y: CANVAS_HEIGHT / 2 };

// Canvas setup
const canvas = document.getElementById('game-canvas');
const ctx = canvas.getContext('2d');
canvas.width = CANVAS_WIDTH;
canvas.height = CANVAS_HEIGHT;

// Event listeners
document.addEventListener('keydown', (e) => keys[e.key.toLowerCase()] = true);
document.addEventListener('keyup', (e) => keys[e.key.toLowerCase()] = false);
document.addEventListener('mousemove', (e) => {
    mouse.x = e.clientX;
    mouse.y = e.clientY;
});
document.addEventListener('click', shoot);
document.addEventListener('keydown', (e) => {
    if (e.key.toLowerCase() === 'r') reload();
});

// Spawn enemies periodically
setInterval(spawnEnemy, 2000);

// Game loop
function gameLoop() {
    update();
    render();
    requestAnimationFrame(gameLoop);
}

// Update game state
function update() {
    // Player movement
    if (keys['w'] || keys['arrowup']) player.y -= PLAYER_SPEED;
    if (keys['s'] || keys['arrowdown']) player.y += PLAYER_SPEED;
    if (keys['a'] || keys['arrowleft']) player.x -= PLAYER_SPEED;
    if (keys['d'] || keys['arrowright']) player.x += PLAYER_SPEED;

    // Keep player in bounds
    player.x = Math.max(PLAYER_SIZE / 2, Math.min(CANVAS_WIDTH - PLAYER_SIZE / 2, player.x));
    player.y = Math.max(PLAYER_SIZE / 2, Math.min(CANVAS_HEIGHT - PLAYER_SIZE / 2, player.y));

    // Update player angle
    player.angle = Math.atan2(mouse.y - player.y, mouse.x - player.x);

    // Update bullets
    bullets = bullets.filter(bullet => {
        bullet.x += Math.cos(bullet.angle) * BULLET_SPEED;
        bullet.y += Math.sin(bullet.angle) * BULLET_SPEED;
        return bullet.x > 0 && bullet.x < CANVAS_WIDTH && bullet.y > 0 && bullet.y < CANVAS_HEIGHT;
    });

    // Update enemies
    enemies.forEach(enemy => {
        const dx = player.x - enemy.x;
        const dy = player.y - enemy.y;
        const dist = Math.sqrt(dx * dx + dy * dy);
        enemy.x += (dx / dist) * ENEMY_SPEED;
        enemy.y += (dy / dist) * ENEMY_SPEED;

        // Check collision with player
        if (Math.sqrt((player.x - enemy.x) ** 2 + (player.y - enemy.y) ** 2) < (PLAYER_SIZE + ENEMY_SIZE) / 2) {
            player.health -= 10;
            updateUI();
            if (player.health <= 0) {
                alert('Game Over! Score: ' + score);
                resetGame();
            }
        }
    });

    // Check bullet-enemy collisions
    bullets.forEach((bullet, bIndex) => {
        enemies.forEach((enemy, eIndex) => {
            if (Math.sqrt((bullet.x - enemy.x) ** 2 + (bullet.y - enemy.y) ** 2) < (BULLET_SIZE + ENEMY_SIZE) / 2) {
                bullets.splice(bIndex, 1);
                enemies.splice(eIndex, 1);
                score += 10;
                updateUI();
            }
        });
    });
}

// Render game
function render() {
    // Clear canvas
    ctx.fillStyle = '#222';
    ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

    // Draw player
    ctx.save();
    ctx.translate(player.x, player.y);
    ctx.rotate(player.angle);
    ctx.fillStyle = '#00ff00';
    ctx.fillRect(-PLAYER_SIZE / 2, -PLAYER_SIZE / 2, PLAYER_SIZE, PLAYER_SIZE);
    // Gun
    ctx.fillStyle = '#666';
    ctx.fillRect(0, -3, PLAYER_SIZE / 2, 6);
    ctx.restore();

    // Draw bullets
    ctx.fillStyle = '#ffff00';
    bullets.forEach(bullet => {
        ctx.beginPath();
        ctx.arc(bullet.x, bullet.y, BULLET_SIZE, 0, Math.PI * 2);
        ctx.fill();
    });

    // Draw enemies
    ctx.fillStyle = '#ff0000';
    enemies.forEach(enemy => {
        ctx.fillRect(enemy.x - ENEMY_SIZE / 2, enemy.y - ENEMY_SIZE / 2, ENEMY_SIZE, ENEMY_SIZE);
    });
}

// Shoot function
function shoot() {
    if (player.ammo > 0) {
        bullets.push({
            x: player.x + Math.cos(player.angle) * PLAYER_SIZE / 2,
            y: player.y + Math.sin(player.angle) * PLAYER_SIZE / 2,
            angle: player.angle
        });
        player.ammo--;
        updateUI();
    }
}

// Reload function
function reload() {
    if (player.reserve > 0) {
        const needed = MAX_AMMO - player.ammo;
        const toReload = Math.min(needed, player.reserve);
        player.ammo += toReload;
        player.reserve -= toReload;
        updateUI();
    }
}

// Spawn enemy
function spawnEnemy() {
    const side = Math.floor(Math.random() * 4);
    let x, y;
    switch (side) {
        case 0: x = Math.random() * CANVAS_WIDTH; y = -ENEMY_SIZE; break;
        case 1: x = CANVAS_WIDTH + ENEMY_SIZE; y = Math.random() * CANVAS_HEIGHT; break;
        case 2: x = Math.random() * CANVAS_WIDTH; y = CANVAS_HEIGHT + ENEMY_SIZE; break;
        case 3: x = -ENEMY_SIZE; y = Math.random() * CANVAS_HEIGHT; break;
    }
    enemies.push({ x, y });
}

// Update UI
function updateUI() {
    document.getElementById('health').textContent = 'Health: ' + player.health;
    document.getElementById('ammo').textContent = 'Ammo: ' + player.ammo + '/' + player.reserve;
    document.getElementById('score').textContent = 'Score: ' + score;
}

// Reset game
function resetGame() {
    player = {
        x: CANVAS_WIDTH / 2,
        y: CANVAS_HEIGHT / 2,
        health: 100,
        ammo: MAX_AMMO,
        reserve: MAX_RESERVE,
        angle: 0
    };
    bullets = [];
    enemies = [];
    score = 0;
    updateUI();
}

// Start game
gameLoop();
