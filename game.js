const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');
const gameOverDiv = document.getElementById('gameOver');
const winnerText = document.getElementById('winnerText');
const restartBtn = document.getElementById('restartBtn');

// Set canvas dimensions
canvas.width = 800;
canvas.height = 400;

// Game state
let gameState = 'PLAYING'; // PLAYING or GAME_OVER
let keys = {};

// Mecha class
class Mecha {
    constructor(x, y, color, controls, isPlayer2 = false) {
        this.x = x;
        this.y = y;
        this.width = 64;
        this.height = 96;
        this.color = color;
        this.hp = 100;
        this.maxHp = 100;
        this.velocityX = 0;
        this.velocityY = 0;
        this.speed = 5;
        this.jumpForce = 15;
        this.isJumping = false;
        this.isAttacking = false;
        this.isDefending = false;
        this.attackCooldown = 0;
        this.attackDuration = 0;
        this.controls = controls;
        this.facingRight = !isPlayer2;
        this.isPlayer2 = isPlayer2;
        this.groundY = canvas.height - 100 - this.height;
    }

    update(opponent) {
        // Handle input
        this.velocityX = 0;
        if (keys[this.controls.left]) {
            this.velocityX = -this.speed;
            this.facingRight = false;
        }
        if (keys[this.controls.right]) {
            this.velocityX = this.speed;
            this.facingRight = true;
        }
        if (keys[this.controls.jump] && !this.isJumping) {
            this.velocityY = -this.jumpForce;
            this.isJumping = true;
        }
        this.isDefending = keys[this.controls.defend];

        if (keys[this.controls.attack] && this.attackCooldown <= 0 && !this.isDefending) {
            this.isAttacking = true;
            this.attackDuration = 20;
            this.attackCooldown = 40;
        }

        // Update cooldowns and durations
        if (this.attackCooldown > 0) this.attackCooldown--;
        if (this.attackDuration > 0) {
            this.attackDuration--;
            if (this.attackDuration === 0) {
                this.isAttacking = false;
            }
        }

        // Apply gravity
        this.velocityY += 0.8;

        // Update position
        this.x += this.velocityX;
        this.y += this.velocityY;

        // Boundary checks
        if (this.x < 0) this.x = 0;
        if (this.x + this.width > canvas.width) this.x = canvas.width - this.width;

        // Ground collision
        if (this.y >= this.groundY) {
            this.y = this.groundY;
            this.velocityY = 0;
            this.isJumping = false;
        }

        // Check attack hit
        if (this.isAttacking && this.attackDuration === 15) {
            this.checkAttackHit(opponent);
        }
    }

    checkAttackHit(opponent) {
        const attackRange = 80;
        const attackX = this.facingRight ? this.x + this.width : this.x - attackRange;
        const attackWidth = attackRange;

        if (
            attackX < opponent.x + opponent.width &&
            attackX + attackWidth > opponent.x &&
            this.y < opponent.y + opponent.height &&
            this.y + this.height > opponent.y
        ) {
            if (!opponent.isDefending) {
                opponent.takeDamage(20);
            }
        }
    }

    takeDamage(damage) {
        this.hp -= damage;
        if (this.hp < 0) this.hp = 0;
    }

    draw() {
        ctx.save();

        // Draw mecha body (pixel art style)
        const pixelSize = 4;

        // Draw body
        ctx.fillStyle = this.color;
        // Torso
        this.drawPixelRect(this.x + 16, this.y + 24, 32, 40, pixelSize);
        // Head
        this.drawPixelRect(this.x + 20, this.y + 4, 24, 24, pixelSize);
        // Shoulders
        this.drawPixelRect(this.x + 4, this.y + 24, 12, 16, pixelSize);
        this.drawPixelRect(this.x + 48, this.y + 24, 12, 16, pixelSize);
        // Arms
        if (this.isAttacking) {
            if (this.facingRight) {
                this.drawPixelRect(this.x + 48, this.y + 32, 32, 12, pixelSize);
                // Attack effect
                ctx.fillStyle = '#ffffff';
                this.drawPixelRect(this.x + 76, this.y + 28, 16, 20, pixelSize);
            } else {
                this.drawPixelRect(this.x - 16, this.y + 32, 32, 12, pixelSize);
                // Attack effect
                ctx.fillStyle = '#ffffff';
                this.drawPixelRect(this.x - 28, this.y + 28, 16, 20, pixelSize);
            }
        } else if (this.isDefending) {
            // Defend pose - arms up
            this.drawPixelRect(this.x + 8, this.y + 12, 12, 24, pixelSize);
            this.drawPixelRect(this.x + 44, this.y + 12, 12, 24, pixelSize);
            // Shield effect
            ctx.fillStyle = 'rgba(255, 255, 255, 0.5)';
            ctx.beginPath();
            ctx.arc(this.x + this.width / 2, this.y + this.height / 2, 40, 0, Math.PI * 2);
            ctx.fill();
        } else {
            this.drawPixelRect(this.x + 8, this.y + 40, 12, 24, pixelSize);
            this.drawPixelRect(this.x + 44, this.y + 40, 12, 24, pixelSize);
        }
        // Legs
        this.drawPixelRect(this.x + 16, this.y + 64, 12, 32, pixelSize);
        this.drawPixelRect(this.x + 36, this.y + 64, 12, 32, pixelSize);

        // Draw HP bar
        const hpBarWidth = 64;
        const hpBarHeight = 8;
        const hpBarY = this.y - 20;
        const hpPercentage = this.hp / this.maxHp;

        // HP bar background
        ctx.fillStyle = '#333333';
        ctx.fillRect(this.x, hpBarY, hpBarWidth, hpBarHeight);

        // HP bar fill
        ctx.fillStyle = this.hp > 30 ? '#4ade80' : '#ef4444';
        ctx.fillRect(this.x, hpBarY, hpBarWidth * hpPercentage, hpBarHeight);

        // HP bar border
        ctx.strokeStyle = '#eeeeee';
        ctx.lineWidth = 2;
        ctx.strokeRect(this.x, hpBarY, hpBarWidth, hpBarHeight);

        ctx.restore();
    }

    drawPixelRect(x, y, width, height, pixelSize) {
        for (let px = 0; px < width; px += pixelSize) {
            for (let py = 0; py < height; py += pixelSize) {
                ctx.fillRect(x + px, y + py, pixelSize, pixelSize);
            }
        }
    }
}

// Initialize players
let player1, player2;

function initGame() {
    player1 = new Mecha(
        100,
        0,
        '#0f3460',
        { left: 'a', right: 'd', jump: 'w', attack: ' ', defend: 's' }
    );
    player2 = new Mecha(
        650,
        0,
        '#e94560',
        { left: 'ArrowLeft', right: 'ArrowRight', jump: 'ArrowUp', attack: 'Enter', defend: 'ArrowDown' },
        true
    );
    player1.y = player1.groundY;
    player2.y = player2.groundY;
    gameState = 'PLAYING';
    gameOverDiv.classList.add('hidden');
}

// Draw arena
function drawArena() {
    // Sky gradient
    const gradient = ctx.createLinearGradient(0, 0, 0, canvas.height - 100);
    gradient.addColorStop(0, '#0f0f23');
    gradient.addColorStop(1, '#1a1a2e');
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, canvas.width, canvas.height - 100);

    // Ground
    ctx.fillStyle = '#2d3436';
    ctx.fillRect(0, canvas.height - 100, canvas.width, 100);

    // Ground pixel details
    ctx.fillStyle = '#636e72';
    for (let i = 0; i < canvas.width; i += 16) {
        ctx.fillRect(i, canvas.height - 100, 8, 4);
    }
}

// Main game loop
function gameLoop() {
    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Draw arena
    drawArena();

    if (gameState === 'PLAYING') {
        // Update players
        player1.update(player2);
        player2.update(player1);

        // Check win condition
        if (player1.hp <= 0) {
            gameState = 'GAME_OVER';
            winnerText.textContent = 'Player 2 (Red) Wins!';
            winnerText.style.color = '#e94560';
            gameOverDiv.classList.remove('hidden');
        } else if (player2.hp <= 0) {
            gameState = 'GAME_OVER';
            winnerText.textContent = 'Player 1 (Blue) Wins!';
            winnerText.style.color = '#0f3460';
            gameOverDiv.classList.remove('hidden');
        }
    }

    // Draw players
    player1.draw();
    player2.draw();

    requestAnimationFrame(gameLoop);
}

// Event listeners
document.addEventListener('keydown', (e) => {
    keys[e.key] = true;
    // Prevent scrolling with space/arrow keys
    if ([' ', 'ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Enter'].includes(e.key)) {
        e.preventDefault();
    }
});

document.addEventListener('keyup', (e) => {
    keys[e.key] = false;
});

restartBtn.addEventListener('click', initGame);

// Initialize and start game
initGame();
gameLoop();
