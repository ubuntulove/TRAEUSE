import { GameState } from './config.js';

export const HUDComponentType = {
    HEALTH_ARMOR: 'health_armor',
    WEAPON_PANEL: 'weapon_panel',
    RADAR: 'radar',
    MINIMAP: 'minimap',
    KILL_FEED: 'kill_feed',
    SCOREBOARD: 'scoreboard',
    CHAT_BOX: 'chat_box',
    BUY_MENU: 'buy_menu',
    ROUND_TIMER: 'round_timer',
    BOMB_TIMER: 'bomb_timer',
    CROSSHAIR: 'crosshair',
    DAMAGE_INDICATOR: 'damage_indicator',
    SPECTATOR_INFO: 'spectator_info',
    TEAM_INFO: 'team_info',
    INVENTORY_PANEL: 'inventory_panel',
    OBJECTIVE_PANEL: 'objective_panel',
    COMPASS: 'compass',
    AMMO_COUNTER: 'ammo_counter',
    MONEY_DISPLAY: 'money_display',
    UTILITY_COUNTER: 'utility_counter'
};

export const HUDVisibility = {
    VISIBLE: 'visible',
    HIDDEN: 'hidden',
    FADING: 'fading',
    TRANSITIONING: 'transitioning'
};

export const HUDPosition = {
    TOP_LEFT: 'top_left',
    TOP_CENTER: 'top_center',
    TOP_RIGHT: 'top_right',
    BOTTOM_LEFT: 'bottom_left',
    BOTTOM_CENTER: 'bottom_center',
    BOTTOM_RIGHT: 'bottom_right',
    CENTER: 'center',
    TOP: 'top',
    BOTTOM: 'bottom'
};

export const HUDScaling = {
    TINY: 0.5,
    SMALL: 0.75,
    NORMAL: 1.0,
    LARGE: 1.25,
    HUGE: 1.5
};

export class HUDStyle {
    constructor(theme = 'default') {
        this.theme = theme;
        this.colors = {
            primary: '#4A90D9',
            secondary: '#2C3E50',
            accent: '#E74C3C',
            background: 'rgba(0, 0, 0, 0.7)',
            text: '#FFFFFF',
            success: '#2ECC71',
            warning: '#F39C12',
            error: '#E74C3C',
            health: '#2ECC71',
            armor: '#3498DB',
            money: '#F1C40F',
            enemy: '#E74C3C',
            friendly: '#2ECC71',
            neutral: '#95A5A6'
        };
        this.fonts = {
            primary: 'Counter-Strike, Arial, sans-serif',
            secondary: 'Roboto, Arial, sans-serif',
            monospace: 'Consolas, monospace'
        };
        this.borderRadius = 4;
        this.padding = 8;
        this.margin = 4;
        this.opacity = 1.0;
        this.scaling = HUDScaling.NORMAL;
        this.shadow = true;
        this.animationEnabled = true;
        this.animationDuration = 200;
        this.transition = 'ease-out';
    }

    applyColorScheme(scheme) {
        const schemes = {
            dark: {
                primary: '#4A90D9',
                secondary: '#1a1a2e',
                background: 'rgba(20, 20, 30, 0.85)',
                text: '#FFFFFF'
            },
            light: {
                primary: '#3498DB',
                secondary: '#ECF0F1',
                background: 'rgba(255, 255, 255, 0.9)',
                text: '#2C3E50'
            },
            neon: {
                primary: '#00FF00',
                secondary: '#FF00FF',
                background: 'rgba(0, 0, 0, 0.5)',
                text: '#00FF00'
            },
            competitive: {
                primary: '#FF6B6B',
                secondary: '#4ECDC4',
                background: 'rgba(0, 0, 0, 0.8)',
                text: '#FFFFFF'
            }
        };
        this.colors = { ...this.colors, ...schemes[scheme] };
    }

    setScaling(scale) {
        this.scaling = scale;
    }

    enableAnimations(enabled = true) {
        this.animationEnabled = enabled;
    }

    export() {
        return {
            theme: this.theme,
            colors: { ...this.colors },
            fonts: { ...this.fonts },
            borderRadius: this.borderRadius,
            padding: this.padding,
            margin: this.margin,
            opacity: this.opacity,
            scaling: this.scaling,
            shadow: this.shadow,
            animationEnabled: this.animationEnabled,
            animationDuration: this.animationDuration,
            transition: this.transition
        };
    }

    import(data) {
        Object.assign(this, data);
    }
}

export class HUDPosition2D {
    constructor(x = 0, y = 0, anchor = 'topleft') {
        this.x = x;
        this.y = y;
        this.anchor = anchor;
        this.offsetX = 0;
        this.offsetY = 0;
        this.rotation = 0;
        this.scaleX = 1;
        this.scaleY = 1;
    }

    setPosition(x, y) {
        this.x = x;
        this.y = y;
    }

    setOffset(x, y) {
        this.offsetX = x;
        this.offsetY = y;
    }

    setAnchor(anchor) {
        this.anchor = anchor;
    }

    setRotation(degrees) {
        this.rotation = degrees;
    }

    setScale(scaleX, scaleY = scaleX) {
        this.scaleX = scaleX;
        this.scaleY = scaleY;
    }

    getScreenPosition(screenWidth, screenHeight, elementWidth, elementHeight) {
        let posX = this.x;
        let posY = this.y;

        switch (this.anchor) {
            case 'top_left':
                posX = this.x + this.offsetX;
                posY = this.y + this.offsetY;
                break;
            case 'top_right':
                posX = screenWidth - this.x - elementWidth + this.offsetX;
                posY = this.y + this.offsetY;
                break;
            case 'bottom_left':
                posX = this.x + this.offsetX;
                posY = screenHeight - this.y - elementHeight + this.offsetY;
                break;
            case 'bottom_right':
                posX = screenWidth - this.x - elementWidth + this.offsetX;
                posY = screenHeight - this.y - elementHeight + this.offsetY;
                break;
            case 'center':
                posX = (screenWidth - elementWidth) / 2 + this.offsetX;
                posY = (screenHeight - elementHeight) / 2 + this.offsetY;
                break;
            case 'top_center':
                posX = (screenWidth - elementWidth) / 2 + this.offsetX;
                posY = this.y + this.offsetY;
                break;
            case 'bottom_center':
                posX = (screenWidth - elementWidth) / 2 + this.offsetX;
                posY = screenHeight - this.y - elementHeight + this.offsetY;
                break;
        }

        return { x: posX, y: posY, rotation: this.rotation };
    }

    export() {
        return {
            x: this.x,
            y: this.y,
            anchor: this.anchor,
            offset_x: this.offsetX,
            offset_y: this.offsetY,
            rotation: this.rotation,
            scale_x: this.scaleX,
            scale_y: this.scaleY
        };
    }

    static import(data) {
        const pos = new HUDPosition2D(data.x, data.y, data.anchor);
        pos.offsetX = data.offset_x || 0;
        pos.offsetY = data.offset_y || 0;
        pos.rotation = data.rotation || 0;
        pos.scaleX = data.scale_x || 1;
        pos.scaleY = data.scale_y || 1;
        return pos;
    }
}

export class HUDComponent {
    constructor(type, options = {}) {
        this.id = `hud_${type}_${Date.now()}`;
        this.type = type;
        this.position = new HUDPosition2D(
            options.x || 0,
            options.y || 0,
            options.anchor || 'top_left'
        );
        this.size = {
            width: options.width || 200,
            height: options.height || 100
        };
        this.visibility = HUDVisibility.VISIBLE;
        this.enabled = true;
        this.zIndex = options.zIndex || 1;
        this.style = new HUDStyle();
        this.data = {};
        this.children = [];
        this.parent = null;
        this.animation = {
            fadeIn: { enabled: true, duration: 300 },
            fadeOut: { enabled: true, duration: 200 },
            pulse: { enabled: false, duration: 1000, scale: 1.1 },
            shake: { enabled: false, duration: 500, intensity: 5 }
        };
        this.events = {
            onClick: null,
            onHover: null,
            onShow: null,
            onHide: null,
            onUpdate: null
        };
        this.metadata = {};
    }

    setPosition(x, y) {
        this.position.setPosition(x, y);
    }

    setAnchor(anchor) {
        this.position.setAnchor(anchor);
    }

    setSize(width, height) {
        this.size.width = width;
        this.size.height = height;
    }

    setVisibility(visibility) {
        const oldVisibility = this.visibility;
        this.visibility = visibility;

        if (visibility === HUDVisibility.HIDDEN && oldVisibility !== HUDVisibility.HIDDEN) {
            this.triggerEvent('onHide');
        } else if (visibility === HUDVisibility.VISIBLE && oldVisibility === HUDVisibility.HIDDEN) {
            this.triggerEvent('onShow');
        }
    }

    show() {
        this.setVisibility(HUDVisibility.VISIBLE);
    }

    hide() {
        this.setVisibility(HUDVisibility.HIDDEN);
    }

    toggle() {
        if (this.visibility === HUDVisibility.VISIBLE) {
            this.hide();
        } else {
            this.show();
        }
    }

    enable() {
        this.enabled = true;
    }

    disable() {
        this.enabled = false;
    }

    setZIndex(zIndex) {
        this.zIndex = zIndex;
    }

    setStyle(style) {
        this.style = style;
    }

    setData(data) {
        this.data = { ...this.data, ...data };
        this.triggerEvent('onUpdate', this.data);
    }

    addChild(component) {
        component.parent = this;
        this.children.push(component);
    }

    removeChild(componentId) {
        const index = this.children.findIndex(c => c.id === componentId);
        if (index > -1) {
            this.children[index].parent = null;
            this.children.splice(index, 1);
        }
    }

    triggerEvent(eventName, data = null) {
        if (this.events[eventName]) {
            this.events[eventName](data);
        }
    }

    on(eventName, callback) {
        if (this.events.hasOwnProperty(eventName)) {
            this.events[eventName] = callback;
        }
    }

    update(deltaTime) {
        for (const child of this.children) {
            child.update(deltaTime);
        }
    }

    render(context) {
        if (!this.enabled || this.visibility === HUDVisibility.HIDDEN) {
            return;
        }

        context.save();

        const screenPos = this.position.getScreenPosition(
            context.canvas.width,
            context.canvas.height,
            this.size.width * this.style.scaling,
            this.size.height * this.style.scaling
        );

        context.translate(screenPos.x, screenPos.y);
        context.rotate((screenPos.rotation * Math.PI) / 180);
        context.scale(this.position.scaleX, this.position.scaleY);

        this.draw(context);

        for (const child of this.children) {
            child.render(context);
        }

        context.restore();
    }

    draw(context) {
    }

    export() {
        return {
            id: this.id,
            type: this.type,
            position: this.position.export(),
            size: { ...this.size },
            visibility: this.visibility,
            enabled: this.enabled,
            z_index: this.zIndex,
            style: this.style.export(),
            data: { ...this.data },
            children: this.children.map(c => c.export()),
            animation: { ...this.animation },
            metadata: { ...this.metadata }
        };
    }

    static import(data) {
        const component = new HUDComponent(data.type, {
            x: data.position.x,
            y: data.position.y,
            anchor: data.position.anchor,
            width: data.size.width,
            height: data.size.height,
            zIndex: data.z_index
        });
        component.visibility = data.visibility;
        component.enabled = data.enabled;
        component.data = data.data || {};
        component.animation = data.animation || component.animation;
        component.metadata = data.metadata || {};
        if (data.style) {
            component.style.import(data.style);
        }
        return component;
    }
}

export class HealthArmorComponent extends HUDComponent {
    constructor(options = {}) {
        super(HUDComponentType.HEALTH_ARMOR, {
            x: 20,
            y: 20,
            anchor: 'bottom_left',
            width: 250,
            height: 80,
            ...options
        });
        this.health = 100;
        this.maxHealth = 100;
        this.armor = 0;
        this.maxArmor = 100;
        this.helmet = false;
        this.ducking = false;
        this.moving = false;
    }

    setHealth(health, maxHealth = 100) {
        this.health = Math.max(0, Math.min(maxHealth, health));
        this.maxHealth = maxHealth;
    }

    setArmor(armor, maxArmor = 100, helmet = false) {
        this.armor = Math.max(0, Math.min(maxArmor, armor));
        this.maxArmor = maxArmor;
        this.helmet = helmet;
    }

    setMovementState(ducking, moving) {
        this.ducking = ducking;
        this.moving = moving;
    }

    draw(context) {
        const width = this.size.width * this.style.scaling;
        const height = this.size.height * this.style.scaling;
        const padding = this.style.padding;

        context.fillStyle = this.style.colors.background;
        context.beginPath();
        context.roundRect(0, 0, width, height, this.style.borderRadius);
        context.fill();

        const healthBarY = 20;
        const healthBarHeight = 20;
        const healthBarWidth = width - padding * 2;

        const healthPercentage = this.health / this.maxHealth;
        const healthWidth = healthBarWidth * healthPercentage;

        context.fillStyle = this.style.colors.secondary;
        context.beginPath();
        context.roundRect(padding, healthBarY, healthBarWidth, healthBarHeight, 4);
        context.fill();

        let healthColor = this.style.colors.health;
        if (this.health <= 25) {
            healthColor = this.style.colors.error;
        } else if (this.health <= 50) {
            healthColor = this.style.colors.warning;
        }

        context.fillStyle = healthColor;
        context.beginPath();
        context.roundRect(padding, healthBarY, healthWidth, healthBarHeight, 4);
        context.fill();

        context.fillStyle = this.style.colors.text;
        context.font = `bold ${14 * this.style.scaling}px ${this.style.fonts.primary}`;
        context.textAlign = 'left';
        context.fillText(`${this.health}`, padding + 8, healthBarY + healthBarHeight - 4);

        if (this.armor > 0) {
            const armorBarY = healthBarY + healthBarHeight + 8;
            const armorPercentage = this.armor / this.maxArmor;
            const armorWidth = healthBarWidth * armorPercentage;

            context.fillStyle = this.style.colors.secondary;
            context.beginPath();
            context.roundRect(padding, armorBarY, healthBarWidth, 14, 4);
            context.fill();

            context.fillStyle = this.helmet ? this.style.colors.primary : this.style.colors.armor;
            context.beginPath();
            context.roundRect(padding, armorBarY, armorWidth, 14, 4);
            context.fill();

            context.fillStyle = this.style.colors.text;
            context.font = `${11 * this.style.scaling}px ${this.style.fonts.primary}`;
            context.textAlign = 'left';
            context.fillText(`${this.armor}`, padding + 8, armorBarY + 11);
        }
    }
}

export class WeaponPanelComponent extends HUDComponent {
    constructor(options = {}) {
        super(HUDComponentType.WEAPON_PANEL, {
            x: 20,
            y: 20,
            anchor: 'bottom_left',
            width: 300,
            height: 60,
            ...options
        });
        this.weapons = [];
        this.currentWeaponIndex = 0;
        this.primaryWeapon = null;
        this.secondaryWeapon = null;
        this.knife = null;
        this.grenades = [];
        this.c4 = false;
    }

    setWeapons(weapons) {
        this.weapons = weapons;
        this.primaryWeapon = weapons.find(w => w.slot === 'primary');
        this.secondaryWeapon = weapons.find(w => w.slot === 'secondary');
        this.knife = weapons.find(w => w.slot === 'knife');
        this.grenades = weapons.filter(w => w.slot === 'grenade');
        this.c4 = weapons.some(w => w.name === 'C4');
    }

    setCurrentWeapon(index) {
        this.currentWeaponIndex = index;
    }

    draw(context) {
        const width = this.size.width * this.style.scaling;
        const height = this.size.height * this.style.scaling;

        let y = this.position.y;

        if (this.primaryWeapon) {
            this.drawWeaponSlot(context, this.primaryWeapon, 0, y, width, true);
            y += 45;
        }

        if (this.secondaryWeapon) {
            this.drawWeaponSlot(context, this.secondaryWeapon, 1, y, width, this.currentWeaponIndex === 1);
            y += 45;
        }

        if (this.knife) {
            this.drawWeaponSlot(context, this.knife, 2, y, width, this.currentWeaponIndex === 2);
            y += 45;
        }

        let x = 0;
        for (const grenade of this.grenades) {
            this.drawGrenadeSlot(context, grenade, x, y, 40);
            x += 45;
        }

        if (this.c4) {
            this.drawGrenadeSlot(context, { name: 'C4', icon: 'bomb' }, x, y, 40, true);
        }
    }

    drawWeaponSlot(context, weapon, slot, y, width, selected) {
        const height = 40;

        context.fillStyle = selected
            ? this.style.colors.primary + '80'
            : this.style.colors.background;
        context.beginPath();
        context.roundRect(0, y, width, height, 4);
        context.fill();

        if (selected) {
            context.strokeStyle = this.style.colors.primary;
            context.lineWidth = 2;
            context.stroke();
        }

        context.fillStyle = this.style.colors.text;
        context.font = `${12 * this.style.scaling}px ${this.style.fonts.primary}`;
        context.textAlign = 'left';
        context.fillText(`${slot + 1}  ${weapon.name}`, 10, y + height / 2 + 4);

        if (weapon.ammo !== undefined) {
            context.textAlign = 'right';
            context.fillText(`${weapon.ammo}/${weapon.reserve || 0}`, width - 10, y + height / 2 + 4);
        }
    }

    drawGrenadeSlot(context, grenade, x, y, size, isC4 = false) {
        context.fillStyle = isC4 ? this.style.colors.error : this.style.colors.background;
        context.beginPath();
        context.roundRect(x, y, size, size, 4);
        context.fill();

        context.fillStyle = this.style.colors.text;
        context.font = `${10 * this.style.scaling}px ${this.style.fonts.primary}`;
        context.textAlign = 'center';
        context.fillText(grenade.name.substring(0, 3), x + size / 2, y + size / 2 + 4);
    }
}

export class RadarComponent extends HUDComponent {
    constructor(options = {}) {
        super(HUDComponentType.RADAR, {
            x: 20,
            y: 120,
            anchor: 'bottom_left',
            width: 180,
            height: 180,
            ...options
        });
        this.centerX = 90;
        this.centerY = 90;
        this.radius = 80;
        this.zoom = 1.0;
        this.rotation = 0;
        this.showGrid = true;
        this.showIcons = true;
        this.showNames = false;
        this.playerPosition = { x: 0, y: 0, angle: 0 };
        this.teammates = [];
        this.enemies = [];
        this.bomb = null;
        this.hostages = [];
    }

    setPlayerPosition(x, y, angle) {
        this.playerPosition = { x, y, angle };
        this.rotation = angle;
    }

    setTeammates(teammates) {
        this.teammates = teammates;
    }

    setEnemies(enemies) {
        this.enemies = enemies;
    }

    setBomb(bomb) {
        this.bomb = bomb;
    }

    setZoom(zoom) {
        this.zoom = Math.max(0.5, Math.min(2.0, zoom));
    }

    worldToRadar(worldX, worldY) {
        const dx = (worldX - this.playerPosition.x) * this.zoom;
        const dy = (worldY - this.playerPosition.y) * this.zoom;

        const cos = Math.cos(-this.rotation * Math.PI / 180);
        const sin = Math.sin(-this.rotation * Math.PI / 180);

        const radarX = this.centerX + dx * cos - dy * sin;
        const radarY = this.centerY + dx * sin + dy * cos;

        return { x: radarX, y: radarY };
    }

    draw(context) {
        const size = this.size.width * this.style.scaling;

        context.fillStyle = this.style.colors.background + 'CC';
        context.beginPath();
        context.arc(size / 2, size / 2, size / 2, 0, Math.PI * 2);
        context.fill();

        context.strokeStyle = this.style.colors.primary + '40';
        context.lineWidth = 1;
        context.beginPath();
        context.arc(size / 2, size / 2, size / 2, 0, Math.PI * 2);
        context.stroke();

        if (this.showGrid) {
            context.strokeStyle = this.style.colors.secondary + '30';
            context.beginPath();
            context.moveTo(size / 2, 0);
            context.lineTo(size / 2, size);
            context.moveTo(0, size / 2);
            context.lineTo(size, size / 2);
            context.stroke();

            context.beginPath();
            context.arc(size / 2, size / 2, size / 4, 0, Math.PI * 2);
            context.stroke();
        }

        for (const teammate of this.teammates) {
            const pos = this.worldToRadar(teammate.x, teammate.y);
            this.drawDot(context, pos.x, pos.y, this.style.colors.friendly, 6);
        }

        for (const enemy of this.enemies) {
            const pos = this.worldToRadar(enemy.x, enemy.y);
            if (enemy.isSpotted) {
                this.drawDot(context, pos.x, pos.y, this.style.colors.enemy, 6);
            }
        }

        if (this.bomb) {
            const pos = this.worldToRadar(this.bomb.x, this.bomb.y);
            this.drawDot(context, pos.x, pos.y, this.style.colors.warning, 8);
        }

        const playerPos = this.worldToRadar(this.playerPosition.x, this.playerPosition.y);
        this.drawDot(context, playerPos.x, playerPos.y, this.style.colors.primary, 8);
    }

    drawDot(context, x, y, color, radius) {
        context.fillStyle = color;
        context.beginPath();
        context.arc(x, y, radius, 0, Math.PI * 2);
        context.fill();
    }
}

export class KillFeedComponent extends HUDComponent {
    constructor(options = {}) {
        super(HUDComponentType.KILL_FEED, {
            x: 20,
            y: 100,
            anchor: 'top_right',
            width: 300,
            height: 200,
            ...options
        });
        this.kills = [];
        this.maxKills = 5;
        this.fadeTime = 5000;
        this.showAssists = true;
    }

    addKill(killer, victim, weapon, headshot, assister = null) {
        const kill = {
            id: Date.now(),
            killer: killer,
            victim: victim,
            weapon: weapon,
            headshot: headshot,
            assister: assister,
            timestamp: Date.now()
        };

        this.kills.unshift(kill);

        if (this.kills.length > this.maxKills) {
            this.kills.pop();
        }
    }

    clear() {
        this.kills = [];
    }

    update(deltaTime) {
        const now = Date.now();
        this.kills = this.kills.filter(kill => {
            const age = now - kill.timestamp;
            return age < this.fadeTime;
        });
    }

    draw(context) {
        let y = 0;
        const lineHeight = 24 * this.style.scaling;

        for (const kill of this.kills) {
            const age = Date.now() - kill.timestamp;
            let alpha = 1.0;

            if (age > this.fadeTime - 1000) {
                alpha = 1.0 - (age - (this.fadeTime - 1000)) / 1000;
            }

            this.drawKillEntry(context, 0, y, kill, alpha);
            y += lineHeight;
        }
    }

    drawKillEntry(context, x, y, kill, alpha) {
        const width = this.size.width * this.style.scaling;

        context.globalAlpha = alpha;

        context.fillStyle = this.style.colors.background + 'AA';
        context.beginPath();
        context.roundRect(x, y, width, 22 * this.style.scaling, 3);
        context.fill();

        context.fillStyle = this.style.colors.friendly;
        context.font = `${11 * this.style.scaling}px ${this.style.fonts.primary}`;
        context.textAlign = 'left';
        context.fillText(kill.killer.name, x + 8, y + 15 * this.style.scaling);

        context.fillStyle = this.style.colors.text;
        const weaponText = kill.headshot ? `[HS] ${kill.weapon}` : kill.weapon;
        context.fillText(` ${weaponText} `, x + 100, y + 15 * this.style.scaling);

        context.fillStyle = this.style.colors.enemy;
        context.textAlign = 'right';
        context.fillText(kill.victim.name, x + width - 8, y + 15 * this.style.scaling);

        context.globalAlpha = 1.0;
    }
}

export class RoundTimerComponent extends HUDComponent {
    constructor(options = {}) {
        super(HUDComponentType.ROUND_TIMER, {
            x: 0,
            y: 10,
            anchor: 'top_center',
            width: 120,
            height: 40,
            ...options
        });
        this.timeRemaining = 115;
        this.maxTime = 115;
        this.isPaused = false;
        this.isOvertime = false;
        this.phase = 'live';
    }

    setTime(seconds) {
        this.timeRemaining = Math.max(0, seconds);
    }

    setPhase(phase) {
        this.phase = phase;
    }

    pause() {
        this.isPaused = true;
    }

    resume() {
        this.isPaused = false;
    }

    setOvertime(isOvertime) {
        this.isOvertime = isOvertime;
    }

    update(deltaTime) {
        if (!this.isPaused && this.phase === 'live') {
            this.timeRemaining = Math.max(0, this.timeRemaining - deltaTime / 1000);
        }
    }

    draw(context) {
        const width = this.size.width * this.style.scaling;
        const height = this.size.height * this.style.scaling;

        const minutes = Math.floor(this.timeRemaining / 60);
        const seconds = Math.floor(this.timeRemaining % 60);
        const timeString = `${minutes}:${seconds.toString().padStart(2, '0')}`;

        context.fillStyle = this.style.colors.background;
        context.beginPath();
        context.roundRect(0, 0, width, height, this.style.borderRadius);
        context.fill();

        let color = this.style.colors.text;
        if (this.timeRemaining <= 10) {
            color = this.style.colors.error;
        } else if (this.timeRemaining <= 30) {
            color = this.style.colors.warning;
        }

        if (this.isOvertime) {
            color = this.style.colors.warning;
        }

        context.fillStyle = color;
        context.font = `bold ${24 * this.style.scaling}px ${this.style.fonts.monospace}`;
        context.textAlign = 'center';
        context.textBaseline = 'middle';
        context.fillText(timeString, width / 2, height / 2);

        if (this.isOvertime) {
            context.fillStyle = this.style.colors.warning;
            context.font = `${10 * this.style.scaling}px ${this.style.fonts.primary}`;
            context.fillText('OT', width / 2, height - 8);
        }
    }
}

export class MoneyDisplayComponent extends HUDComponent {
    constructor(options = {}) {
        super(HUDComponentType.MONEY_DISPLAY, {
            x: 20,
            y: 20,
            anchor: 'bottom_right',
            width: 150,
            height: 35,
            ...options
        });
        this.money = 0;
        this.pendingMoney = 0;
        this.showChange = true;
    }

    setMoney(amount) {
        const diff = amount - this.money;
        this.pendingMoney = diff;
        this.money = amount;
    }

    draw(context) {
        const width = this.size.width * this.style.scaling;
        const height = this.size.height * this.style.scaling;

        context.fillStyle = this.style.colors.background;
        context.beginPath();
        context.roundRect(0, 0, width, height, this.style.borderRadius);
        context.fill();

        context.fillStyle = this.style.colors.money;
        context.font = `bold ${16 * this.style.scaling}px ${this.style.fonts.primary}`;
        context.textAlign = 'center';
        context.textBaseline = 'middle';
        context.fillText(`$${this.money.toLocaleString()}`, width / 2, height / 2);

        if (this.showChange && this.pendingMoney !== 0) {
            const changeText = this.pendingMoney > 0
                ? `+$${this.pendingMoney}`
                : `-$${Math.abs(this.pendingMoney)}`;
            const changeColor = this.pendingMoney > 0
                ? this.style.colors.success
                : this.style.colors.error;

            context.fillStyle = changeColor;
            context.font = `${12 * this.style.scaling}px ${this.style.fonts.primary}`;
            context.fillText(changeText, width / 2, height + 12);
        }
    }
}

export class CrosshairComponent extends HUDComponent {
    constructor(options = {}) {
        super(HUDComponentType.CROSSHAIR, {
            x: 0,
            y: 0,
            anchor: 'center',
            width: 30,
            height: 30,
            ...options
        });
        this.style.type = 'default';
        this.style.color = '#FFFFFF';
        this.style.gap = 4;
        this.style.length = 6;
        this.style.thickness = 2;
        this.style.dots = false;
        this.style.opacity = 1.0;
        this.style.outline = false;
        this.style.red = false;
        this.spread = 0;
        this.inCrosshair = false;
    }

    setStyle(style) {
        this.style = { ...this.style, ...style };
    }

    setSpread(spread) {
        this.spread = spread;
    }

    setInCrosshair(inCrosshair) {
        this.inCrosshair = inCrosshair;
    }

    draw(context) {
        const centerX = this.size.width / 2;
        const centerY = this.size.height / 2;
        const gap = this.style.gap + this.spread * 0.5;
        const length = this.style.length;
        const thickness = this.style.thickness;

        context.globalAlpha = this.style.opacity;
        context.strokeStyle = this.style.color;
        context.fillStyle = this.style.color;
        context.lineWidth = thickness;

        if (this.style.outline) {
            context.strokeStyle = '#000000';
            context.lineWidth = thickness + 2;
        }

        context.beginPath();

        context.moveTo(centerX - gap - length, centerY);
        context.lineTo(centerX - gap, centerY);

        context.moveTo(centerX + gap, centerY);
        context.lineTo(centerX + gap + length, centerY);

        context.moveTo(centerX, centerY - gap - length);
        context.lineTo(centerX, centerY - gap);

        context.moveTo(centerX, centerY + gap);
        context.lineTo(centerX, centerY + gap + length);

        context.stroke();

        if (this.style.dots) {
            context.fillRect(centerX - thickness / 2, centerY - thickness / 2, thickness, thickness);
        }

        if (this.inCrosshair) {
            context.fillStyle = this.style.red ? '#FF0000' : this.style.color;
            context.globalAlpha = 0.5;
            context.beginPath();
            context.arc(centerX, centerY, 3, 0, Math.PI * 2);
            context.fill();
        }

        context.globalAlpha = 1.0;
    }
}

export class BuyMenuComponent extends HUDComponent {
    constructor(options = {}) {
        super(HUDComponentType.BUY_MENU, {
            x: 0,
            y: 0,
            anchor: 'center',
            width: 600,
            height: 500,
            ...options
        });
        this.isOpen = false;
        this.selectedCategory = 'rifles';
        this.selectedItem = null;
        this.items = [];
        this.maxAffordable = 0;
        this.currentMoney = 0;
        this.categories = {
            primaries: { name: 'Primaries', items: [] },
            pistols: { name: 'Pistols', items: [] },
            gear: { name: 'Gear', items: [] },
            utilities: { name: 'Utilities', items: [] }
        };
    }

    open() {
        this.isOpen = true;
        this.visibility = HUDVisibility.VISIBLE;
    }

    close() {
        this.isOpen = false;
        this.visibility = HUDVisibility.HIDDEN;
    }

    toggle() {
        if (this.isOpen) {
            this.close();
        } else {
            this.open();
        }
    }

    setMoney(money) {
        this.currentMoney = money;
        this.updateAffordability();
    }

    setItems(category, items) {
        if (this.categories[category]) {
            this.categories[category].items = items;
        }
    }

    selectCategory(category) {
        this.selectedCategory = category;
    }

    selectItem(item) {
        this.selectedItem = item;
    }

    updateAffordability() {
        this.maxAffordable = this.currentMoney;
        for (const category of Object.values(this.categories)) {
            for (const item of category.items) {
                item.affordable = item.price <= this.currentMoney;
            }
        }
    }

    draw(context) {
        if (!this.isOpen) return;

        const width = this.size.width * this.style.scaling;
        const height = this.size.height * this.style.scaling;

        context.fillStyle = this.style.colors.background + 'EE';
        context.beginPath();
        context.roundRect(0, 0, width, height, 8);
        context.fill();

        let y = 10;
        for (const [key, category] of Object.entries(this.categories)) {
            const isSelected = key === this.selectedCategory;
            const buttonWidth = 140;
            const buttonHeight = 35;

            context.fillStyle = isSelected
                ? this.style.colors.primary
                : this.style.colors.secondary;
            context.beginPath();
            context.roundRect(10, y, buttonWidth, buttonHeight, 4);
            context.fill();

            context.fillStyle = this.style.colors.text;
            context.font = `${12 * this.style.scaling}px ${this.style.fonts.primary}`;
            context.textAlign = 'center';
            context.textBaseline = 'middle';
            context.fillText(category.name, 10 + buttonWidth / 2, y + buttonHeight / 2);

            y += buttonHeight + 10;
        }

        y = 20;
        const category = this.categories[this.selectedCategory];
        if (category) {
            for (const item of category.items) {
                const itemWidth = width - 180;
                const itemHeight = 40;

                context.fillStyle = item.affordable
                    ? this.style.colors.secondary + 'CC'
                    : this.style.colors.secondary + '44';
                context.beginPath();
                context.roundRect(170, y, itemWidth, itemHeight, 4);
                context.fill();

                context.fillStyle = item.affordable
                    ? this.style.colors.text
                    : this.style.colors.neutral;
                context.font = `${14 * this.style.scaling}px ${this.style.fonts.primary}`;
                context.textAlign = 'left';
                context.fillText(item.name, 180, y + itemHeight / 2 - 6);

                context.fillStyle = item.affordable
                    ? this.style.colors.money
                    : this.style.colors.neutral;
                context.textAlign = 'right';
                context.fillText(`$${item.price}`, 160 - 10, y + itemHeight / 2 - 6);

                if (item.ammo !== undefined) {
                    context.fillStyle = this.style.colors.text;
                    context.font = `${10 * this.style.scaling}px ${this.style.fonts.primary}`;
                    context.textAlign = 'left';
                    context.fillText(`Ammo: ${item.ammo}`, 180, y + itemHeight / 2 + 10);
                }

                y += itemHeight + 5;
            }
        }

        context.fillStyle = this.style.colors.money;
        context.font = `bold ${16 * this.style.scaling}px ${this.style.fonts.primary}`;
        context.textAlign = 'right';
        context.fillText(`Available: $${this.currentMoney}`, width - 20, height - 20);
    }
}

export class ScoreboardComponent extends HUDComponent {
    constructor(options = {}) {
        super(HUDComponentType.SCOREBOARD, {
            x: 0,
            y: 0,
            anchor: 'center',
            width: 700,
            height: 400,
            ...options
        });
        this.isVisible = false;
        this.teams = {
            terrorists: { name: 'Terrorists', players: [], score: 0 },
            counterTerrorists: { name: 'Counter-Terrorists', players: [], score: 0 }
        };
        this.matchInfo = {
            map: '',
            mode: '',
            duration: 0
        };
    }

    show() {
        this.isVisible = true;
        this.visibility = HUDVisibility.VISIBLE;
    }

    hide() {
        this.isVisible = false;
        this.visibility = HUDVisibility.HIDDEN;
    }

    toggle() {
        if (this.isVisible) {
            this.hide();
        } else {
            this.show();
        }
    }

    setTeams(terrorists, counterTerrorists) {
        this.teams.terrorists = terrorists;
        this.teams.counterTerrorists = counterTerrorists;
    }

    setMatchInfo(info) {
        this.matchInfo = { ...this.matchInfo, ...info };
    }

    draw(context) {
        if (!this.isVisible) return;

        const width = this.size.width * this.style.scaling;
        const height = this.size.height * this.style.scaling;

        context.fillStyle = this.style.colors.background + 'EE';
        context.beginPath();
        context.roundRect(0, 0, width, height, 8);
        context.fill();

        const headerHeight = 40;
        context.fillStyle = this.style.colors.secondary;
        context.fillRect(0, 0, width, headerHeight);

        context.fillStyle = this.style.colors.text;
        context.font = `bold ${14 * this.style.scaling}px ${this.style.fonts.primary}`;
        context.textAlign = 'center';
        context.fillText(`${this.matchInfo.map} - ${this.matchInfo.mode}`, width / 2, 25);

        const teamWidth = (width - 40) / 2;
        const teamY = headerHeight + 20;

        this.drawTeam(context, 10, teamY, teamWidth, this.teams.terrorists, this.style.colors.enemy);
        this.drawTeam(context, width / 2 + 10, teamY, teamWidth, this.teams.counterTerrorists, this.style.colors.friendly);

        const centerY = teamY + 20;
        context.fillStyle = this.style.colors.primary;
        context.font = `bold ${48 * this.style.scaling}px ${this.style.fonts.monospace}`;
        context.textAlign = 'center';
        context.fillText(`${this.teams.terrorists.score} - ${this.teams.counterTerrorists.score}`, width / 2, centerY + 20);
    }

    drawTeam(context, x, y, width, team, color) {
        const headerHeight = 30;
        const playerHeight = 25;

        context.fillStyle = color + '40';
        context.beginPath();
        context.roundRect(x, y, width, headerHeight + team.players.length * playerHeight + 10, 4);
        context.fill();

        context.fillStyle = color;
        context.font = `bold ${12 * this.style.scaling}px ${this.style.fonts.primary}`;
        context.textAlign = 'left';
        context.fillText(team.name, x + 10, y + 20);

        let playerY = y + headerHeight;
        for (const player of team.players) {
            context.fillStyle = this.style.colors.text;
            context.font = `${11 * this.style.scaling}px ${this.style.fonts.primary}`;
            context.textAlign = 'left';
            context.fillText(player.name, x + 10, playerY + 15);

            context.textAlign = 'right';
            context.fillText(`${player.kills}`, x + width - 100, playerY + 15);

            context.fillStyle = this.style.colors.neutral;
            context.fillText(`${player.deaths}`, x + width - 70, playerY + 15);
            context.fillText(`${player.assists}`, x + width - 40, playerY + 15);

            if (player.isMVPs) {
                context.fillStyle = this.style.colors.warning;
                context.fillText('MVP', x + width - 10, playerY + 15);
            }

            playerY += playerHeight;
        }
    }
}

export class HUDManager {
    static instance = null;

    constructor() {
        this.components = new Map();
        this.style = new HUDStyle();
        this.isEnabled = true;
        this.isPaused = false;
        this.listeners = new Map();
        this.updateOrder = [];
    }

    static getInstance() {
        if (!HUDManager.instance) {
            HUDManager.instance = new HUDManager();
        }
        return HUDManager.instance;
    }

    addComponent(component) {
        this.components.set(component.id, component);
        this.updateOrder.push(component.id);
        this.updateOrder.sort((a, b) => {
            return this.components.get(a).zIndex - this.components.get(b).zIndex;
        });
        this.emit('componentAdded', component);
    }

    removeComponent(componentId) {
        const removed = this.components.delete(componentId);
        if (removed) {
            this.updateOrder = this.updateOrder.filter(id => id !== componentId);
            this.emit('componentRemoved', componentId);
        }
        return removed;
    }

    getComponent(componentId) {
        return this.components.get(componentId);
    }

    getComponentByType(type) {
        return Array.from(this.components.values()).find(c => c.type === type);
    }

    getAllComponents() {
        return Array.from(this.components.values());
    }

    setEnabled(enabled) {
        this.isEnabled = enabled;
        this.emit('hudEnabledChanged', enabled);
    }

    setPaused(paused) {
        this.isPaused = paused;
    }

    setStyle(style) {
        this.style = style;
        for (const component of this.components.values()) {
            component.setStyle(style);
        }
        this.emit('styleChanged', style);
    }

    update(deltaTime) {
        if (!this.isEnabled || this.isPaused) return;

        for (const componentId of this.updateOrder) {
            const component = this.components.get(componentId);
            if (component) {
                component.update(deltaTime);
            }
        }
    }

    render(context) {
        if (!this.isEnabled) return;

        for (const componentId of this.updateOrder) {
            const component = this.components.get(componentId);
            if (component) {
                component.render(context);
            }
        }
    }

    createHealthArmorComponent(options = {}) {
        const component = new HealthArmorComponent(options);
        this.addComponent(component);
        return component;
    }

    createWeaponPanelComponent(options = {}) {
        const component = new WeaponPanelComponent(options);
        this.addComponent(component);
        return component;
    }

    createRadarComponent(options = {}) {
        const component = new RadarComponent(options);
        this.addComponent(component);
        return component;
    }

    createKillFeedComponent(options = {}) {
        const component = new KillFeedComponent(options);
        this.addComponent(component);
        return component;
    }

    createRoundTimerComponent(options = {}) {
        const component = new RoundTimerComponent(options);
        this.addComponent(component);
        return component;
    }

    createMoneyDisplayComponent(options = {}) {
        const component = new MoneyDisplayComponent(options);
        this.addComponent(component);
        return component;
    }

    createCrosshairComponent(options = {}) {
        const component = new CrosshairComponent(options);
        this.addComponent(component);
        return component;
    }

    createBuyMenuComponent(options = {}) {
        const component = new BuyMenuComponent(options);
        this.addComponent(component);
        return component;
    }

    createScoreboardComponent(options = {}) {
        const component = new ScoreboardComponent(options);
        this.addComponent(component);
        return component;
    }

    exportConfiguration() {
        return {
            style: this.style.export(),
            components: Array.from(this.components.values()).map(c => c.export()),
            isEnabled: this.isEnabled
        };
    }

    importConfiguration(config) {
        if (config.style) {
            this.style.import(config.style);
        }

        for (const componentData of config.components || []) {
            const component = HUDComponent.import(componentData);
            this.addComponent(component);
        }

        this.isEnabled = config.isEnabled !== false;
    }

    resetToDefault() {
        this.components.clear();
        this.updateOrder = [];
        this.style = new HUDStyle();
    }

    on(event, callback) {
        if (!this.listeners.has(event)) {
            this.listeners.set(event, []);
        }
        this.listeners.get(event).push(callback);
    }

    off(event, callback) {
        if (!this.listeners.has(event)) return;
        const callbacks = this.listeners.get(event);
        const index = callbacks.indexOf(callback);
        if (index > -1) {
            callbacks.splice(index, 1);
        }
    }

    emit(event, data) {
        if (!this.listeners.has(event)) return;
        for (const callback of this.listeners.get(event)) {
            try {
                callback(data);
            } catch (e) {
                console.error(`Error in HUD listener: ${e}`);
            }
        }
    }
}

export const HUD = HUDManager.getInstance();
