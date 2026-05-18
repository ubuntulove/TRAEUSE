
## 1. Architecture Design
This is a simple frontend-only game using HTML5 Canvas for rendering and vanilla JavaScript for game logic.

```mermaid
graph TB
    subgraph Frontend
        A[HTML5 Canvas]
        B[Game Engine]
        C[Input Handler]
        D[UI Components]
    end
    A &lt;-- Rendering --&gt; B
    C -- Input Events --&gt; B
    B -- Update UI --&gt; D
```

## 2. Technology Description
- **Frontend**: Vanilla JavaScript + HTML5 Canvas + CSS3
- **Build Tool**: None - simple static files
- **No Backend or Database Needed**

## 3. File Structure
```
/workspace
├── index.html          # Main HTML file
├── style.css           # Stylesheet
└── game.js             # Game logic
```

## 4. Core Game Logic
### 4.1 Mecha Class
```javascript
class Mecha {
    constructor(x, y, color, controls) {
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
        this.controls = controls; // { left, right, jump, attack, defend }
        this.facingRight = true;
    }
}
```

### 4.2 Game States
- `PLAYING`: Active battle
- `GAME_OVER`: Battle ended, show winner

### 4.3 Input Controls
| Action | Player 1 | Player 2 |
|--------|----------|----------|
| Move Left | A | Left Arrow |
| Move Right | D | Right Arrow |
| Jump | W | Up Arrow |
| Attack | Space | Enter |
| Defend | S | Down Arrow |
