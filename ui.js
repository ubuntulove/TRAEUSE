import { UI_CONFIG, GAME_CONFIG, TEAM } from './config.js';

export class UIManager {
    constructor() {
        this.panels = new Map();
        this.widgets = new Map();
        this.overlays = new Map();
        
        this.activePanel = null;
        this.previousPanel = null;
        
        this.cursorPosition = { x: 0, y: 0 };
        this.cursorVisible = true;
        
        this.screens = {
            mainMenu: null,
            inGame: null,
            pause: null,
            scoreboard: null,
            buyMenu: null,
            inventory: null,
            settings: null,
            console: null
        };
        
        this.hudElements = [];
        
        this.transitions = new Map();
        this.animations = new Map();
        
        this.zIndex = 0;
        
        this.inputEnabled = true;
        this.renderEnabled = true;
        
        this.colors = {
            primary: '#ffffff',
            secondary: '#cccccc',
            accent: '#ff6600',
            danger: '#ff0000',
            success: '#00ff00',
            warning: '#ffff00',
            background: 'rgba(0, 0, 0, 0.8)',
            text: '#ffffff',
            disabled: '#666666'
        };
        
        this.fonts = {
            primary: 'Arial',
            secondary: 'Helvetica',
            mono: 'Courier New',
            pixel: 'Press Start 2P'
        };
        
        this.initialize();
    }
    
    initialize() {
        this.createMainMenu();
        this.createHUD();
        this.createBuyMenu();
        this.createScoreboard();
    }
    
    createMainMenu() {
        const menu = new UIPanel('main-menu', {
            width: '100%',
            height: '100%',
            background: 'linear-gradient(135deg, #1a1a2e 0%, #16213e 100%)',
            align: 'center',
            valign: 'middle',
            zIndex: 1000
        });
        
        const title = new UIText('title', {
            text: GAME_CONFIG.TITLE,
            fontSize: 48,
            fontFamily: this.fonts.primary,
            color: this.colors.accent,
            align: 'center',
            valign: 'middle',
            margin: '0 0 50px 0'
        });
        
        const playButton = new UIButton('play-button', {
            text: 'PLAY',
            width: 300,
            height: 60,
            fontSize: 24,
            background: this.colors.accent,
            hoverBackground: '#ff8533',
            textColor: '#ffffff',
            borderRadius: 5,
            margin: '10px'
        });
        
        playButton.onClick = () => {
            this.hidePanel('main-menu');
            this.startGame();
        };
        
        const optionsButton = new UIButton('options-button', {
            text: 'OPTIONS',
            width: 300,
            height: 60,
            fontSize: 24,
            background: this.colors.secondary,
            hoverBackground: '#dddddd',
            textColor: '#333333',
            borderRadius: 5,
            margin: '10px'
        });
        
        optionsButton.onClick = () => {
            this.showPanel('settings');
        };
        
        const quitButton = new UIButton('quit-button', {
            text: 'QUIT',
            width: 300,
            height: 60,
            fontSize: 24,
            background: this.colors.danger,
            hoverBackground: '#ff3333',
            textColor: '#ffffff',
            borderRadius: 5,
            margin: '10px'
        });
        
        quitButton.onClick = () => {
            this.quitGame();
        };
        
        menu.addChild(title);
        menu.addChild(playButton);
        menu.addChild(optionsButton);
        menu.addChild(quitButton);
        
        this.addPanel(menu);
        this.screens.mainMenu = menu;
    }
    
    createHUD() {
        const hud = new UIPanel('hud', {
            width: '100%',
            height: '100%',
            pointerEvents: 'none',
            zIndex: 100
        });
        
        const healthBar = new UIHealthBar('health-bar', {
            x: 20,
            y: 20,
            width: 250,
            height: 30,
            value: 100,
            maxValue: 100,
            color: '#00ff00',
            background: 'rgba(0, 0, 0, 0.5)',
            showText: true,
            textColor: '#ffffff'
        });
        
        const armorBar = new UIArmorBar('armor-bar', {
            x: 20,
            y: 55,
            width: 250,
            height: 20,
            value: 0,
            maxValue: 100,
            color: '#0066ff',
            background: 'rgba(0, 0, 0, 0.5)',
            showText: false
        });
        
        const ammoCounter = new UIAmmoCounter('ammo-counter', {
            x: 20,
            y: 85,
            width: 200,
            height: 50,
            fontSize: 32,
            fontFamily: this.fonts.mono,
            color: '#ffffff'
        });
        
        const weaponIcon = new UIImage('weapon-icon', {
            x: 20,
            y: 145,
            width: 60,
            height: 30,
            src: '',
            opacity: 1
        });
        
        const grenadeSlots = new UIGrenadeSlots('grenade-slots', {
            x: 90,
            y: 150,
            width: 100,
            height: 25,
            slots: 4
        });
        
        const bombIndicator = new UIBombIndicator('bomb-indicator', {
            x: 20,
            y: 180,
            width: 200,
            height: 25,
            visible: false
        });
        
        const defuseIndicator = new UIDefuseIndicator('defuse-indicator', {
            x: '50%',
            y: '50%',
            width: 200,
            height: 100,
            visible: false
        });
        
        const roundTimer = new UIRoundTimer('round-timer', {
            x: '50%',
            y: 20,
            width: 150,
            height: 50,
            fontSize: 36,
            fontFamily: this.fonts.mono,
            color: '#ffffff',
            align: 'center',
            valign: 'middle'
        });
        
        const scoreBoard = new UIScoreBoard('score-board', {
            x: '50%',
            y: 80,
            width: 200,
            height: 50,
            fontSize: 28,
            fontFamily: this.fonts.mono,
            color: '#ffffff',
            team1Score: 0,
            team2Score: 0
        });
        
        const radar = new UIRadar('radar', {
            x: 20,
            y: '50%',
            width: 200,
            height: 200,
            zoom: 1.0,
            showNames: true
        });
        
        const crosshair = new UICrosshair('crosshair', {
            x: '50%',
            y: '50%',
            width: 30,
            height: 30,
            style: 'cross',
            color: '#ffffff'
        });
        
        const damageIndicator = new UIDamageIndicator('damage-indicator', {
            x: '50%',
            y: '50%',
            width: 100,
            height: 100,
            visible: false
        });
        
        const killFeed = new UIKillFeed('kill-feed', {
            x: '90%',
            y: '20%',
            width: 300,
            height: 200,
            maxEntries: 5
        });
        
        const moneyDisplay = new UIMoneyDisplay('money-display', {
            x: 20,
            y: 220,
            width: 200,
            height: 30,
            fontSize: 20,
            fontFamily: this.fonts.mono,
            color: '#00ff00'
        });
        
        const compass = new UICompass('compass', {
            x: '50%',
            y: 150,
            width: 400,
            height: 40,
            fontSize: 16,
            color: '#ffffff'
        });
        
        const minimap = new UIMinimap('minimap', {
            x: '50%',
            y: '50%',
            width: 300,
            height: 300,
            visible: false
        });
        
        const buyZoneIndicator = new UIBuyZoneIndicator('buy-zone', {
            x: '50%',
            y: '50%',
            width: 200,
            height: 50,
            visible: false
        });
        
        const spectatingInfo = new UISpectatingInfo('spectating', {
            x: '50%',
            y: '90%',
            width: 300,
            height: 30,
            visible: false
        });
        
        hud.addChild(healthBar);
        hud.addChild(armorBar);
        hud.addChild(ammoCounter);
        hud.addChild(weaponIcon);
        hud.addChild(grenadeSlots);
        hud.addChild(bombIndicator);
        hud.addChild(defuseIndicator);
        hud.addChild(roundTimer);
        hud.addChild(scoreBoard);
        hud.addChild(radar);
        hud.addChild(crosshair);
        hud.addChild(damageIndicator);
        hud.addChild(killFeed);
        hud.addChild(moneyDisplay);
        hud.addChild(compass);
        hud.addChild(minimap);
        hud.addChild(buyZoneIndicator);
        hud.addChild(spectatingInfo);
        
        this.addPanel(hud);
        this.screens.inGame = hud;
        
        this.hudElements = [
            healthBar, armorBar, ammoCounter, weaponIcon, grenadeSlots,
            bombIndicator, defuseIndicator, roundTimer, scoreBoard, radar,
            crosshair, damageIndicator, killFeed, moneyDisplay, compass,
            minimap, buyZoneIndicator, spectatingInfo
        ];
    }
    
    createBuyMenu() {
        const buyMenu = new UIPanel('buy-menu', {
            width: '80%',
            height: '80%',
            background: 'rgba(0, 0, 0, 0.9)',
            align: 'center',
            valign: 'middle',
            zIndex: 500,
            visible: false
        });
        
        const categories = ['assault', 'smg', 'sniper', 'shotgun', 'pistol', 'equipment', 'other'];
        const categoryTabs = new UITabGroup('category-tabs', {
            x: 0,
            y: 0,
            width: '100%',
            height: 50,
            tabs: categories
        });
        
        const itemGrid = new UIItemGrid('item-grid', {
            x: 0,
            y: 50,
            width: '100%',
            height: 'calc(100% - 100px)',
            columns: 5,
            rows: 4
        });
        
        const totalPrice = new UIMoneyDisplay('total-price', {
            x: 0,
            y: 'calc(100% - 50px)',
            width: '50%',
            height: 50,
            fontSize: 24,
            color: '#00ff00'
        });
        
        const buyButton = new UIButton('buy-button', {
            x: '75%',
            y: 'calc(100% - 50px)',
            width: '25%',
            height: 50,
            text: 'BUY',
            fontSize: 24,
            background: '#00ff00',
            enabled: false
        });
        
        buyMenu.addChild(categoryTabs);
        buyMenu.addChild(itemGrid);
        buyMenu.addChild(totalPrice);
        buyMenu.addChild(buyButton);
        
        this.addPanel(buyMenu);
        this.screens.buyMenu = buyMenu;
    }
    
    createScoreboard() {
        const scoreboard = new UIPanel('scoreboard', {
            width: '80%',
            height: '80%',
            background: 'rgba(0, 0, 0, 0.9)',
            align: 'center',
            valign: 'middle',
            zIndex: 600,
            visible: false
        });
        
        const title = new UIText('scoreboard-title', {
            text: 'SCOREBOARD',
            fontSize: 32,
            color: '#ffffff',
            align: 'center',
            margin: '0 0 20px 0'
        });
        
        const team1Panel = new UIPanel('team1-panel', {
            width: '45%',
            height: '70%',
            background: 'rgba(0, 100, 255, 0.3)',
            border: '2px solid #0066ff'
        });
        
        const team2Panel = new UIPanel('team2-panel', {
            x: '55%',
            width: '45%',
            height: '70%',
            background: 'rgba(255, 100, 0, 0.3)',
            border: '2px solid #ff6600'
        });
        
        const matchInfo = new UIText('match-info', {
            text: 'Map: de_dust2 | Time: 1:45',
            fontSize: 16,
            color: '#cccccc',
            align: 'center'
        });
        
        scoreboard.addChild(title);
        scoreboard.addChild(team1Panel);
        scoreboard.addChild(team2Panel);
        scoreboard.addChild(matchInfo);
        
        this.addPanel(scoreboard);
        this.screens.scoreboard = scoreboard;
    }
    
    addPanel(panel) {
        this.panels.set(panel.id, panel);
    }
    
    removePanel(panelId) {
        this.panels.delete(panelId);
    }
    
    getPanel(panelId) {
        return this.panels.get(panelId);
    }
    
    showPanel(panelId) {
        const panel = this.panels.get(panelId);
        if (panel) {
            this.previousPanel = this.activePanel;
            this.activePanel = panel;
            panel.visible = true;
            panel.show();
        }
    }
    
    hidePanel(panelId) {
        const panel = this.panels.get(panelId);
        if (panel) {
            panel.visible = false;
            panel.hide();
            
            if (this.activePanel === panel) {
                this.activePanel = this.previousPanel;
                this.previousPanel = null;
            }
        }
    }
    
    togglePanel(panelId) {
        const panel = this.panels.get(panelId);
        if (panel) {
            if (panel.visible) {
                this.hidePanel(panelId);
            } else {
                this.showPanel(panelId);
            }
        }
    }
    
    update(deltaTime) {
        for (const panel of this.panels.values()) {
            if (panel.visible) {
                panel.update(deltaTime);
            }
        }
        
        this.updateAnimations(deltaTime);
    }
    
    render(ctx) {
        if (!this.renderEnabled) return;
        
        const sortedPanels = Array.from(this.panels.values())
            .filter(p => p.visible)
            .sort((a, b) => (a.zIndex || 0) - (b.zIndex || 0));
        
        for (const panel of sortedPanels) {
            panel.render(ctx);
        }
        
        if (this.cursorVisible) {
            this.renderCursor(ctx);
        }
    }
    
    renderCursor(ctx) {
        ctx.save();
        ctx.strokeStyle = '#ffffff';
        ctx.lineWidth = 2;
        
        ctx.beginPath();
        ctx.moveTo(this.cursorPosition.x - 10, this.cursorPosition.y);
        ctx.lineTo(this.cursorPosition.x + 10, this.cursorPosition.y);
        ctx.moveTo(this.cursorPosition.x, this.cursorPosition.y - 10);
        ctx.lineTo(this.cursorPosition.x, this.cursorPosition.y + 10);
        ctx.stroke();
        
        ctx.restore();
    }
    
    handleMouseMove(x, y) {
        this.cursorPosition.x = x;
        this.cursorPosition.y = y;
        
        for (const panel of this.panels.values()) {
            if (panel.visible && panel.containsPoint(x, y)) {
                panel.handleMouseMove(x, y);
            }
        }
    }
    
    handleMouseClick(x, y, button) {
        for (const panel of this.panels.values()) {
            if (panel.visible && panel.containsPoint(x, y)) {
                panel.handleMouseClick(x, y, button);
            }
        }
    }
    
    handleKeyDown(key) {
        if (this.activePanel) {
            this.activePanel.handleKeyDown(key);
        }
    }
    
    updateAnimations(deltaTime) {
        for (const [id, animation] of this.animations) {
            animation.progress += deltaTime / animation.duration;
            
            if (animation.progress >= 1) {
                this.animations.delete(id);
                if (animation.onComplete) {
                    animation.onComplete();
                }
            } else {
                this.applyAnimation(id, animation);
            }
        }
    }
    
    animate(elementId, property, from, to, duration, easing = 'linear', onComplete = null) {
        this.animations.set(elementId + '_' + property, {
            elementId,
            property,
            from,
            to,
            duration,
            easing,
            progress: 0,
            onComplete
        });
    }
    
    applyAnimation(id, animation) {
        const element = this.widgets.get(animation.elementId);
        if (!element) return;
        
        const easedProgress = this.applyEasing(animation.progress, animation.easing);
        const value = this.lerp(animation.from, animation.to, easedProgress);
        
        element.setProperty(animation.property, value);
    }
    
    applyEasing(t, easing) {
        switch (easing) {
            case 'linear': return t;
            case 'easeIn': return t * t;
            case 'easeOut': return t * (2 - t);
            case 'easeInOut': return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
            case 'easeInQuad': return t * t;
            case 'easeOutQuad': return t * (2 - t);
            case 'easeInOutQuad': return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
            case 'easeInCubic': return t * t * t;
            case 'easeOutCubic': return (--t) * t * t + 1;
            case 'easeInOutCubic': return t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
            case 'bounce': return this.bounceEase(t);
            case 'elastic': return this.elasticEase(t);
            default: return t;
        }
    }
    
    bounceEase(t) {
        if (t < 1 / 2.75) {
            return 7.5625 * t * t;
        } else if (t < 2 / 2.75) {
            t -= 1.5 / 2.75;
            return 7.5625 * t * t + 0.75;
        } else if (t < 2.5 / 2.75) {
            t -= 2.25 / 2.75;
            return 7.5625 * t * t + 0.9375;
        } else {
            t -= 2.625 / 2.75;
            return 7.5625 * t * t + 0.984375;
        }
    }
    
    elasticEase(t) {
        if (t === 0 || t === 1) return t;
        return Math.pow(2, -10 * t) * Math.sin((t - 0.1) * 5 * Math.PI) + 1;
    }
    
    lerp(a, b, t) {
        return a + (b - a) * t;
    }
    
    startGame() {
        this.hidePanel('main-menu');
        
        for (const element of this.hudElements) {
            element.show();
        }
    }
    
    endGame() {
        for (const element of this.hudElements) {
            element.hide();
        }
        
        this.showPanel('main-menu');
    }
    
    quitGame() {
        console.log('Quit game');
    }
    
    showBuyMenu() {
        this.showPanel('buy-menu');
    }
    
    hideBuyMenu() {
        this.hidePanel('buy-menu');
    }
    
    showScoreboard() {
        this.showPanel('scoreboard');
    }
    
    hideScoreboard() {
        this.hidePanel('scoreboard');
    }
    
    toggleScoreboard() {
        this.togglePanel('scoreboard');
    }
}

export class UIPanel {
    constructor(id, options = {}) {
        this.id = id;
        this.x = options.x || 0;
        this.y = options.y || 0;
        this.width = options.width || 100;
        this.height = options.height || 100;
        this.background = options.background || 'transparent';
        this.border = options.border || 'none';
        this.borderRadius = options.borderRadius || 0;
        this.padding = options.padding || 0;
        this.margin = options.margin || 0;
        this.align = options.align || 'left';
        this.valign = options.valign || 'top';
        this.visible = options.visible !== undefined ? options.visible : true;
        this.zIndex = options.zIndex || 0;
        this.pointerEvents = options.pointerEvents !== undefined ? options.pointerEvents : true;
        
        this.children = [];
        this.parent = null;
        
        this.hovered = false;
        this.focused = false;
        
        this.animations = new Map();
    }
    
    addChild(child) {
        child.parent = this;
        this.children.push(child);
    }
    
    removeChild(child) {
        const index = this.children.indexOf(child);
        if (index > -1) {
            this.children.splice(index, 1);
            child.parent = null;
        }
    }
    
    containsPoint(x, y) {
        if (!this.pointerEvents || !this.visible) return false;
        
        const bounds = this.getBounds();
        return x >= bounds.x && x <= bounds.x + bounds.width &&
               y >= bounds.y && y <= bounds.y + bounds.height;
    }
    
    getBounds() {
        let x = this.x;
        let y = this.y;
        let width = this.parseSize(this.width, this.parent ? this.parent.width : window.innerWidth);
        let height = this.parseSize(this.height, this.parent ? this.parent.height : window.innerHeight);
        
        if (typeof this.x === 'string' && this.x.includes('%')) {
            x = (parseFloat(this.x) / 100) * (this.parent ? this.parent.width : window.innerWidth);
        }
        
        if (typeof this.y === 'string' && this.y.includes('%')) {
            y = (parseFloat(this.y) / 100) * (this.parent ? this.parent.height : window.innerHeight);
        }
        
        return { x, y, width, height };
    }
    
    parseSize(value, parentSize) {
        if (typeof value === 'number') return value;
        if (typeof value === 'string') {
            if (value.endsWith('px')) {
                return parseFloat(value);
            } else if (value.endsWith('%')) {
                return (parseFloat(value) / 100) * parentSize;
            } else if (value.startsWith('calc(')) {
                return this.evaluateCalc(value);
            }
        }
        return 0;
    }
    
    evaluateCalc(calc) {
        return 100;
    }
    
    update(deltaTime) {
        for (const child of this.children) {
            if (child.update) {
                child.update(deltaTime);
            }
        }
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        if (this.borderRadius > 0) {
            this.roundRect(ctx, bounds.x, bounds.y, bounds.width, bounds.height, this.borderRadius);
            ctx.clip();
        }
        
        if (this.background !== 'transparent') {
            ctx.fillStyle = this.background;
            ctx.fillRect(bounds.x, bounds.y, bounds.width, bounds.height);
        }
        
        if (this.border !== 'none') {
            ctx.strokeStyle = this.border;
            ctx.lineWidth = 2;
            ctx.strokeRect(bounds.x, bounds.y, bounds.width, bounds.height);
        }
        
        ctx.restore();
        
        for (const child of this.children) {
            if (child.render) {
                ctx.save();
                child.render(ctx);
                ctx.restore();
            }
        }
    }
    
    roundRect(ctx, x, y, width, height, radius) {
        ctx.beginPath();
        ctx.moveTo(x + radius, y);
        ctx.lineTo(x + width - radius, y);
        ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
        ctx.lineTo(x + width, y + height - radius);
        ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
        ctx.lineTo(x + radius, y + height);
        ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
        ctx.lineTo(x, y + radius);
        ctx.quadraticCurveTo(x, y, x + radius, y);
        ctx.closePath();
    }
    
    handleMouseMove(x, y) {
        for (const child of this.children) {
            if (child.handleMouseMove) {
                child.handleMouseMove(x, y);
            }
        }
    }
    
    handleMouseClick(x, y, button) {
        for (const child of this.children) {
            if (child.handleMouseClick) {
                child.handleMouseClick(x, y, button);
            }
        }
    }
    
    handleKeyDown(key) {
        for (const child of this.children) {
            if (child.handleKeyDown) {
                child.handleKeyDown(key);
            }
        }
    }
    
    show() {
        this.visible = true;
    }
    
    hide() {
        this.visible = false;
    }
    
    setProperty(property, value) {
        if (this.hasOwnProperty(property)) {
            this[property] = value;
        }
    }
}

export class UIButton extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.text = options.text || 'Button';
        this.fontSize = options.fontSize || 16;
        this.fontFamily = options.fontFamily || 'Arial';
        this.textColor = options.textColor || '#ffffff';
        this.hoverTextColor = options.hoverTextColor || this.textColor;
        this.background = options.background || '#666666';
        this.hoverBackground = options.hoverBackground || this.background;
        this.pressedBackground = options.pressedBackground || this.hoverBackground;
        this.enabled = options.enabled !== undefined ? options.enabled : true;
        this.borderRadius = options.borderRadius || 5;
        
        this.onClick = null;
        this.onHover = null;
        this.onLeave = null;
        
        this.pressed = false;
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        let bgColor = this.background;
        let txtColor = this.textColor;
        
        if (!this.enabled) {
            bgColor = '#333333';
            txtColor = '#666666';
        } else if (this.pressed) {
            bgColor = this.pressedBackground;
        } else if (this.hovered) {
            bgColor = this.hoverBackground;
            txtColor = this.hoverTextColor;
        }
        
        ctx.fillStyle = bgColor;
        this.roundRect(ctx, bounds.x, bounds.y, bounds.width, bounds.height, this.borderRadius);
        ctx.fill();
        
        if (this.border !== 'none') {
            ctx.strokeStyle = this.border;
            ctx.lineWidth = 2;
            ctx.stroke();
        }
        
        ctx.fillStyle = txtColor;
        ctx.font = `${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(
            this.text,
            bounds.x + bounds.width / 2,
            bounds.y + bounds.height / 2
        );
        
        ctx.restore();
    }
    
    handleMouseMove(x, y) {
        const bounds = this.getBounds();
        const wasHovered = this.hovered;
        this.hovered = this.containsPoint(x, y);
        
        if (this.hovered && !wasHovered && this.onHover) {
            this.onHover();
        } else if (!this.hovered && wasHovered && this.onLeave) {
            this.onLeave();
        }
    }
    
    handleMouseClick(x, y, button) {
        if (!this.enabled) return;
        
        if (this.containsPoint(x, y)) {
            this.pressed = true;
            
            setTimeout(() => {
                this.pressed = false;
            }, 100);
            
            if (this.onClick) {
                this.onClick();
            }
        }
    }
    
    roundRect(ctx, x, y, width, height, radius) {
        ctx.beginPath();
        ctx.moveTo(x + radius, y);
        ctx.lineTo(x + width - radius, y);
        ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
        ctx.lineTo(x + width, y + height - radius);
        ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
        ctx.lineTo(x + radius, y + height);
        ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
        ctx.lineTo(x, y + radius);
        ctx.quadraticCurveTo(x, y, x + radius, y);
        ctx.closePath();
    }
}

export class UIText extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.text = options.text || '';
        this.fontSize = options.fontSize || 16;
        this.fontFamily = options.fontFamily || 'Arial';
        this.color = options.color || '#ffffff';
        this.align = options.align || 'left';
        this.valign = options.valign || 'top';
        this.textShadow = options.textShadow || null;
        this.textOutline = options.textOutline || null;
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.fillStyle = this.color;
        ctx.font = `${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = this.align;
        ctx.textBaseline = this.valign === 'middle' ? 'middle' : this.valign;
        
        let x = bounds.x;
        let y = bounds.y;
        
        if (this.align === 'center') {
            x = bounds.x + bounds.width / 2;
        } else if (this.align === 'right') {
            x = bounds.x + bounds.width;
        }
        
        if (this.valign === 'middle') {
            y = bounds.y + bounds.height / 2;
        } else if (this.valign === 'bottom') {
            y = bounds.y + bounds.height;
        }
        
        if (this.textShadow) {
            ctx.shadowColor = this.textShadow.color || 'rgba(0, 0, 0, 0.5)';
            ctx.shadowBlur = this.textShadow.blur || 4;
            ctx.shadowOffsetX = this.textShadow.offsetX || 2;
            ctx.shadowOffsetY = this.textShadow.offsetY || 2;
        }
        
        if (this.textOutline) {
            ctx.strokeStyle = this.textOutline.color || '#000000';
            ctx.lineWidth = this.textOutline.width || 1;
            ctx.strokeText(this.text, x, y);
        }
        
        ctx.fillText(this.text, x, y);
        
        ctx.restore();
    }
}

export class UISlider extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.min = options.min || 0;
        this.max = options.max || 100;
        this.value = options.value || 50;
        this.step = options.step || 1;
        this.horizontal = options.horizontal !== undefined ? options.horizontal : true;
        this.trackColor = options.trackColor || '#666666';
        this.fillColor = options.fillColor || '#00ff00';
        this.thumbColor = options.thumbColor || '#ffffff';
        this.thumbSize = options.thumbSize || 20;
        
        this.dragging = false;
        this.onChange = null;
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.fillStyle = this.trackColor;
        ctx.fillRect(bounds.x, bounds.y + bounds.height / 2 - 3, bounds.width, 6);
        
        const percentage = (this.value - this.min) / (this.max - this.min);
        
        if (this.horizontal) {
            const fillWidth = bounds.width * percentage;
            ctx.fillStyle = this.fillColor;
            ctx.fillRect(bounds.x, bounds.y + bounds.height / 2 - 3, fillWidth, 6);
            
            const thumbX = bounds.x + fillWidth;
            ctx.fillStyle = this.thumbColor;
            ctx.beginPath();
            ctx.arc(thumbX, bounds.y + bounds.height / 2, this.thumbSize / 2, 0, Math.PI * 2);
            ctx.fill();
        } else {
            const fillHeight = bounds.height * percentage;
            ctx.fillStyle = this.fillColor;
            ctx.fillRect(bounds.x + bounds.width / 2 - 3, bounds.y + bounds.height - fillHeight, 6, fillHeight);
            
            const thumbY = bounds.y + bounds.height - fillHeight;
            ctx.fillStyle = this.thumbColor;
            ctx.beginPath();
            ctx.arc(bounds.x + bounds.width / 2, thumbY, this.thumbSize / 2, 0, Math.PI * 2);
            ctx.fill();
        }
        
        ctx.restore();
    }
    
    handleMouseMove(x, y) {
        if (!this.dragging) return;
        
        const bounds = this.getBounds();
        
        if (this.horizontal) {
            const percentage = Math.max(0, Math.min(1, (x - bounds.x) / bounds.width));
            this.value = this.min + (this.max - this.min) * percentage;
            this.value = Math.round(this.value / this.step) * this.step;
            
            if (this.onChange) {
                this.onChange(this.value);
            }
        }
    }
    
    handleMouseClick(x, y, button) {
        if (button !== 0) return;
        
        const bounds = this.getBounds();
        
        if (this.horizontal) {
            const percentage = Math.max(0, Math.min(1, (x - bounds.x) / bounds.width));
            this.value = this.min + (this.max - this.min) * percentage;
            this.value = Math.round(this.value / this.step) * this.step;
            
            this.dragging = true;
            
            if (this.onChange) {
                this.onChange(this.value);
            }
        }
    }
}

export class UIProgressBar extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.value = options.value || 0;
        this.maxValue = options.maxValue || 100;
        this.color = options.color || '#00ff00';
        this.background = options.background || '#333333';
        this.showText = options.showText || false;
        this.textColor = options.textColor || '#ffffff';
        this.fontSize = options.fontSize || 16;
        this.fontFamily = options.fontFamily || 'Arial';
        this.animated = options.animated || false;
        this.animationSpeed = options.animationSpeed || 0.1;
        this.displayValue = this.value;
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        if (this.animated) {
            this.displayValue += (this.value - this.displayValue) * this.animationSpeed;
        } else {
            this.displayValue = this.value;
        }
        
        ctx.save();
        
        ctx.fillStyle = this.background;
        ctx.fillRect(bounds.x, bounds.y, bounds.width, bounds.height);
        
        const percentage = this.displayValue / this.maxValue;
        ctx.fillStyle = this.color;
        ctx.fillRect(bounds.x, bounds.y, bounds.width * percentage, bounds.height);
        
        if (this.showText) {
            ctx.fillStyle = this.textColor;
            ctx.font = `${this.fontSize}px ${this.fontFamily}`;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText(
                `${Math.round(this.displayValue)} / ${this.maxValue}`,
                bounds.x + bounds.width / 2,
                bounds.y + bounds.height / 2
            );
        }
        
        ctx.restore();
    }
}

export class UIImage extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.src = options.src || '';
        this.image = null;
        this.loaded = false;
        this.opacity = options.opacity || 1;
        this.preserveAspectRatio = options.preserveAspectRatio !== undefined ? options.preserveAspectRatio : true;
        
        if (this.src) {
            this.loadImage(this.src);
        }
    }
    
    loadImage(src) {
        this.image = new Image();
        this.image.onload = () => {
            this.loaded = true;
        };
        this.image.src = src;
    }
    
    render(ctx) {
        if (!this.visible || !this.loaded) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        ctx.globalAlpha = this.opacity;
        
        if (this.preserveAspectRatio) {
            const scale = Math.min(bounds.width / this.image.width, bounds.height / this.image.height);
            const width = this.image.width * scale;
            const height = this.image.height * scale;
            const x = bounds.x + (bounds.width - width) / 2;
            const y = bounds.y + (bounds.height - height) / 2;
            ctx.drawImage(this.image, x, y, width, height);
        } else {
            ctx.drawImage(this.image, bounds.x, bounds.y, bounds.width, bounds.height);
        }
        
        ctx.restore();
    }
}

export class UIGrid extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.columns = options.columns || 4;
        this.rows = options.rows || 4;
        this.cellWidth = options.cellWidth || 50;
        this.cellHeight = options.cellHeight || 50;
        this.cellSpacing = options.cellSpacing || 5;
        this.gridColor = options.gridColor || '#444444';
        this.showGrid = options.showGrid !== undefined ? options.showGrid : true;
        
        this.items = [];
        this.selectedItem = null;
        this.onItemSelect = null;
    }
    
    addItem(item, column, row) {
        this.items.push({
            item,
            column,
            row
        });
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        if (this.showGrid) {
            ctx.strokeStyle = this.gridColor;
            ctx.lineWidth = 1;
            
            for (let col = 0; col <= this.columns; col++) {
                const x = bounds.x + col * this.cellWidth;
                ctx.beginPath();
                ctx.moveTo(x, bounds.y);
                ctx.lineTo(x, bounds.y + this.rows * this.cellHeight);
                ctx.stroke();
            }
            
            for (let row = 0; row <= this.rows; row++) {
                const y = bounds.y + row * this.cellHeight;
                ctx.beginPath();
                ctx.moveTo(bounds.x, y);
                ctx.lineTo(bounds.x + this.columns * this.cellWidth, y);
                ctx.stroke();
            }
        }
        
        for (const { item, column, row } of this.items) {
            this.renderItem(ctx, item, bounds.x + column * this.cellWidth, bounds.y + row * this.cellHeight);
        }
        
        ctx.restore();
    }
    
    renderItem(ctx, item, x, y) {
        if (item.render) {
            item.render(ctx, x, y, this.cellWidth, this.cellHeight);
        }
    }
    
    handleMouseClick(x, y, button) {
        const bounds = this.getBounds();
        
        const col = Math.floor((x - bounds.x) / this.cellWidth);
        const row = Math.floor((y - bounds.y) / this.cellHeight);
        
        const clickedItem = this.items.find(i => i.column === col && i.row === row);
        
        if (clickedItem) {
            this.selectedItem = clickedItem.item;
            
            if (this.onItemSelect) {
                this.onItemSelect(clickedItem.item);
            }
        }
    }
}

export class UIHealthBar extends UIProgressBar {
    constructor(id, options = {}) {
        super(id, options);
        
        this.healthColorLow = '#ff0000';
        this.healthColorMid = '#ffff00';
        this.healthColorHigh = '#00ff00';
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const percentage = this.value / this.maxValue;
        
        if (percentage < 0.25) {
            this.color = this.healthColorLow;
        } else if (percentage < 0.5) {
            this.color = this.healthColorMid;
        } else {
            this.color = this.healthColorHigh;
        }
        
        super.render(ctx);
    }
}

export class UIArmorBar extends UIProgressBar {
    constructor(id, options = {}) {
        super(id, options);
        this.color = options.color || '#0066ff';
    }
}

export class UIAmmoCounter extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.currentAmmo = 0;
        this.reserveAmmo = 0;
        this.maxAmmo = 30;
        this.fontSize = options.fontSize || 32;
        this.fontFamily = options.fontFamily || 'Arial';
        this.color = options.color || '#ffffff';
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.fillStyle = this.color;
        ctx.font = `${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'left';
        ctx.textBaseline = 'middle';
        
        const text = `${this.currentAmmo} / ${this.reserveAmmo}`;
        ctx.fillText(text, bounds.x, bounds.y + bounds.height / 2);
        
        if (this.currentAmmo === 0) {
            ctx.fillStyle = '#ff0000';
            ctx.fillText('RELOAD', bounds.x + 150, bounds.y + bounds.height / 2);
        }
        
        ctx.restore();
    }
}

export class UIRoundTimer extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.time = 0;
        this.maxTime = GAME_CONFIG.ROUND_TIME;
        this.format = options.format || 'mm:ss';
        this.color = options.color || '#ffffff';
        this.fontSize = options.fontSize || 36;
        this.fontFamily = options.fontFamily || 'Arial';
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        const minutes = Math.floor(this.time / 60);
        const seconds = Math.floor(this.time % 60);
        const text = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        
        ctx.fillStyle = this.color;
        ctx.font = `bold ${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(text, bounds.x + bounds.width / 2, bounds.y + bounds.height / 2);
        
        ctx.restore();
    }
}

export class UIScoreBoard extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.team1Score = options.team1Score || 0;
        this.team2Score = options.team2Score || 0;
        this.team1Color = '#0066ff';
        this.team2Color = '#ff6600';
        this.fontSize = options.fontSize || 28;
        this.fontFamily = options.fontFamily || 'Arial';
        this.color = options.color || '#ffffff';
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.fillStyle = this.color;
        ctx.font = `bold ${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        
        const scoreText = `${this.team1Score} - ${this.team2Score}`;
        ctx.fillText(scoreText, bounds.x + bounds.width / 2, bounds.y + bounds.height / 2);
        
        ctx.restore();
    }
}

export class UIRadar extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.zoom = options.zoom || 1.0;
        this.showNames = options.showNames !== undefined ? options.showNames : true;
        this.range = 1000;
        this.backgroundColor = 'rgba(0, 50, 0, 0.8)';
        this.gridColor = 'rgba(0, 100, 0, 0.5)';
        this.playerColor = '#ffffff';
        this.enemyColor = '#ff0000';
        this.bombColor = '#ff6600';
        this.hostageColor = '#00ff00';
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        const centerX = bounds.x + bounds.width / 2;
        const centerY = bounds.y + bounds.height / 2;
        const radius = Math.min(bounds.width, bounds.height) / 2;
        
        ctx.save();
        
        ctx.fillStyle = this.backgroundColor;
        ctx.beginPath();
        ctx.arc(centerX, centerY, radius, 0, Math.PI * 2);
        ctx.fill();
        
        ctx.strokeStyle = this.gridColor;
        ctx.lineWidth = 1;
        
        ctx.beginPath();
        ctx.arc(centerX, centerY, radius, 0, Math.PI * 2);
        ctx.stroke();
        
        ctx.beginPath();
        ctx.moveTo(centerX - radius, centerY);
        ctx.lineTo(centerX + radius, centerY);
        ctx.moveTo(centerX, centerY - radius);
        ctx.lineTo(centerX, centerY + radius);
        ctx.stroke();
        
        ctx.restore();
    }
}

export class UICrosshair extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.style = options.style || 'cross';
        this.color = options.color || '#ffffff';
        this.size = options.size || 10;
        this.thickness = options.thickness || 2;
        this.dot = options.dot !== undefined ? options.dot : false;
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        const centerX = bounds.x + bounds.width / 2;
        const centerY = bounds.y + bounds.height / 2;
        
        ctx.save();
        
        ctx.strokeStyle = this.color;
        ctx.lineWidth = this.thickness;
        
        if (this.style === 'cross') {
            const gap = 4;
            
            ctx.beginPath();
            ctx.moveTo(centerX - this.size, centerY);
            ctx.lineTo(centerX - gap, centerY);
            ctx.moveTo(centerX + gap, centerY);
            ctx.lineTo(centerX + this.size, centerY);
            ctx.moveTo(centerX, centerY - this.size);
            ctx.lineTo(centerX, centerY - gap);
            ctx.moveTo(centerX, centerY + gap);
            ctx.lineTo(centerX, centerY + this.size);
            ctx.stroke();
        }
        
        if (this.dot) {
            ctx.fillStyle = this.color;
            ctx.beginPath();
            ctx.arc(centerX, centerY, 2, 0, Math.PI * 2);
            ctx.fill();
        }
        
        ctx.restore();
    }
}

export class UIDamageIndicator extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.direction = { x: 0, y: 0 };
        this.damage = 0;
        this.color = options.color || '#ff0000';
        this.visible = false;
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        const centerX = bounds.x + bounds.width / 2;
        const centerY = bounds.y + bounds.height / 2;
        
        ctx.save();
        
        ctx.strokeStyle = this.color;
        ctx.lineWidth = 3;
        ctx.globalAlpha = 0.8;
        
        ctx.translate(centerX, centerY);
        ctx.rotate(Math.atan2(this.direction.y, this.direction.x));
        ctx.translate(-centerX, -centerY);
        
        ctx.beginPath();
        ctx.moveTo(centerX + 30, centerY);
        ctx.lineTo(centerX + 50, centerY);
        ctx.moveTo(centerX + 40, centerY - 10);
        ctx.lineTo(centerX + 50, centerY);
        ctx.lineTo(centerX + 40, centerY + 10);
        ctx.stroke();
        
        ctx.restore();
    }
}

export class UIKillFeed extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.maxEntries = options.maxEntries || 5;
        this.entries = [];
        this.fadeTime = 5000;
        this.fontSize = 16;
        this.fontFamily = 'Arial';
        this.textColor = '#ffffff';
        this.killerColor = '#ffffff';
        this.victimColor = '#cccccc';
        this.headshotColor = '#ff6600';
    }
    
    addKill(killer, victim, weapon, headshot, time) {
        this.entries.unshift({
            killer,
            victim,
            weapon,
            headshot,
            time,
            alpha: 1
        });
        
        if (this.entries.length > this.maxEntries) {
            this.entries.pop();
        }
    }
    
    update(deltaTime) {
        const now = Date.now();
        
        for (let i = this.entries.length - 1; i >= 0; i--) {
            const entry = this.entries[i];
            const age = now - entry.time;
            
            if (age > this.fadeTime) {
                entry.alpha = 0;
                this.entries.splice(i, 1);
            } else if (age > this.fadeTime - 1000) {
                entry.alpha = 1 - (age - (this.fadeTime - 1000)) / 1000;
            }
        }
    }
    
    render(ctx) {
        if (!this.visible || this.entries.length === 0) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.font = `${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'right';
        ctx.textBaseline = 'top';
        
        let y = bounds.y;
        
        for (const entry of this.entries) {
            ctx.globalAlpha = entry.alpha;
            
            let text = `${entry.killer} [${entry.weapon}] ${entry.victim}`;
            if (entry.headshot) {
                text += ' HS';
                ctx.fillStyle = this.headshotColor;
            } else {
                ctx.fillStyle = this.textColor;
            }
            
            ctx.fillText(text, bounds.x + bounds.width, y);
            y += this.fontSize + 5;
        }
        
        ctx.restore();
    }
}

export class UIMoneyDisplay extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.money = 0;
        this.fontSize = options.fontSize || 20;
        this.fontFamily = options.fontFamily || 'Arial';
        this.color = options.color || '#00ff00';
        this.prefix = options.prefix || '$';
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.fillStyle = this.color;
        ctx.font = `${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'left';
        ctx.textBaseline = 'middle';
        
        const text = `${this.prefix}${this.money.toLocaleString()}`;
        ctx.fillText(text, bounds.x, bounds.y + bounds.height / 2);
        
        ctx.restore();
    }
}

export class UICompass extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.heading = 0;
        this.fontSize = options.fontSize || 16;
        this.fontFamily = options.fontFamily || 'Arial';
        this.color = options.color || '#ffffff';
        this.markers = ['N', 'NE', 'E', 'SE', 'S', 'SW', 'W', 'NW'];
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        const centerX = bounds.x + bounds.width / 2;
        const centerY = bounds.y + bounds.height / 2;
        
        ctx.save();
        
        ctx.fillStyle = this.color;
        ctx.font = `${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        
        const degrees = ['N', '0', 'E', '0', 'S', '0', 'W', '0'];
        
        for (let i = 0; i < degrees.length; i++) {
            const angle = (i * 45 - this.heading) * Math.PI / 180;
            const x = centerX + Math.sin(angle) * (bounds.width / 2 - 20);
            const y = centerY;
            
            ctx.globalAlpha = Math.abs(angle) < Math.PI / 4 ? 1 : 0.5;
            ctx.fillText(degrees[i], x, y);
        }
        
        ctx.globalAlpha = 1;
        ctx.fillStyle = '#ff0000';
        ctx.fillText('^', centerX, centerY - bounds.height / 2 + 5);
        
        ctx.restore();
    }
}

export class UIMinimap extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.mapData = null;
        this.zoom = options.zoom || 1.0;
        this.showPlayers = options.showPlayers !== undefined ? options.showPlayers : true;
        this.showObjectives = options.showObjectives !== undefined ? options.showObjectives : true;
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.fillStyle = '#1a1a1a';
        ctx.fillRect(bounds.x, bounds.y, bounds.width, bounds.height);
        
        ctx.restore();
    }
}

export class UIBombIndicator extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.bombSite = null;
        this.timer = 0;
        this.color = options.color || '#ff6600';
        this.fontSize = options.fontSize || 18;
        this.fontFamily = options.fontFamily || 'Arial';
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.fillStyle = this.color;
        ctx.font = `bold ${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        
        ctx.fillText('BOMB', bounds.x + bounds.width / 2, bounds.y + bounds.height / 2);
        
        if (this.timer > 0) {
            ctx.fillText(`${this.timer}s`, bounds.x + bounds.width / 2, bounds.y + bounds.height / 2 + this.fontSize);
        }
        
        ctx.restore();
    }
}

export class UIDefuseIndicator extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.progress = 0;
        this.hasKit = false;
        this.progressColor = '#00ff00';
        this.fontSize = options.fontSize || 24;
        this.fontFamily = options.fontFamily || 'Arial';
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        const centerX = bounds.x + bounds.width / 2;
        const centerY = bounds.y + bounds.height / 2;
        
        ctx.save();
        
        ctx.fillStyle = '#000000';
        ctx.globalAlpha = 0.7;
        ctx.fillRect(bounds.x, bounds.y, bounds.width, bounds.height);
        
        ctx.globalAlpha = 1;
        ctx.strokeStyle = this.progressColor;
        ctx.lineWidth = 5;
        ctx.beginPath();
        ctx.arc(centerX, centerY, 40, -Math.PI / 2, -Math.PI / 2 + Math.PI * 2 * this.progress);
        ctx.stroke();
        
        ctx.fillStyle = '#ffffff';
        ctx.font = `${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText('DEFUSING...', centerX, centerY);
        
        ctx.font = '16px Arial';
        ctx.fillText(this.hasKit ? 'KIT' : 'NO KIT', centerX, centerY + 30);
        
        ctx.restore();
    }
}

export class UIBuyZoneIndicator extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.visible = false;
        this.fontSize = options.fontSize || 24;
        this.fontFamily = options.fontFamily || 'Arial';
        this.color = options.color || '#00ff00';
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.fillStyle = this.color;
        ctx.font = `bold ${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.globalAlpha = 0.8;
        
        ctx.fillText('BUY ZONE', bounds.x + bounds.width / 2, bounds.y + bounds.height / 2);
        
        ctx.restore();
    }
}

export class UISpectatingInfo extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.target = null;
        this.targetHealth = 0;
        this.targetWeapon = '';
        this.fontSize = options.fontSize || 18;
        this.fontFamily = options.fontFamily || 'Arial';
        this.color = options.color || '#ffffff';
    }
    
    render(ctx) {
        if (!this.visible || !this.target) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.fillStyle = this.color;
        ctx.font = `${this.fontSize}px ${this.fontFamily}`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        
        ctx.fillText(`Spectating: ${this.target}`, bounds.x + bounds.width / 2, bounds.y + bounds.height / 2);
        
        ctx.restore();
    }
}

export class UIItemGrid extends UIGrid {
    constructor(id, options = {}) {
        super(id, options);
        
        this.columns = options.columns || 5;
        this.rows = options.rows || 4;
        this.onItemSelect = null;
    }
}

export class UIGrenadeSlots extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.slots = options.slots || 4;
        this.grenades = [];
        this.slotWidth = 25;
        this.slotHeight = 25;
        this.slotSpacing = 5;
        this.borderColor = '#666666';
        this.fillColor = '#ff6600';
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.strokeStyle = this.borderColor;
        ctx.lineWidth = 2;
        
        for (let i = 0; i < this.slots; i++) {
            const x = bounds.x + i * (this.slotWidth + this.slotSpacing);
            const y = bounds.y;
            
            ctx.strokeRect(x, y, this.slotWidth, this.slotHeight);
            
            if (i < this.grenades.length) {
                ctx.fillStyle = this.fillColor;
                ctx.fillRect(x + 2, y + 2, this.slotWidth - 4, this.slotHeight - 4);
            }
        }
        
        ctx.restore();
    }
}

export class UITabGroup extends UIPanel {
    constructor(id, options = {}) {
        super(id, options);
        
        this.tabs = options.tabs || [];
        this.selectedTab = 0;
        this.tabWidth = options.tabWidth || 100;
        this.tabHeight = options.tabHeight || 40;
        this.fontSize = options.fontSize || 16;
        this.fontFamily = options.fontFamily || 'Arial';
        this.activeColor = '#ffffff';
        this.inactiveColor = '#888888';
        this.background = '#333333';
        this.onTabChange = null;
    }
    
    render(ctx) {
        if (!this.visible) return;
        
        const bounds = this.getBounds();
        
        ctx.save();
        
        ctx.fillStyle = this.background;
        ctx.fillRect(bounds.x, bounds.y, bounds.width, bounds.height);
        
        for (let i = 0; i < this.tabs.length; i++) {
            const x = bounds.x + i * this.tabWidth;
            const y = bounds.y;
            
            ctx.fillStyle = i === this.selectedTab ? this.activeColor : this.inactiveColor;
            ctx.font = `${this.fontSize}px ${this.fontFamily}`;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            
            const tabName = this.tabs[i];
            ctx.fillText(tabName.toUpperCase(), x + this.tabWidth / 2, y + this.tabHeight / 2);
        }
        
        ctx.restore();
    }
}
