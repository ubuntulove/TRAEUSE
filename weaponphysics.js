import { GameState } from './config.js';

export const WeaponType = {
    PISTOL: 'pistol',
    RIFLE: 'rifle',
    SUBMACHINE_GUN: 'smg',
    SHOTGUN: 'shotgun',
    SNIPER_RIFLE: 'sniper',
    MACHINE_GUN: 'machinegun',
    KNIFE: 'knife',
    GRENADE: 'grenade',
    C4: 'c4'
};

export const FireMode = {
    SINGLE: 'single',
    AUTOMATIC: 'automatic',
    BURST: 'burst'
};

export const WeaponFireMode = {
    [WeaponType.PISTOL]: [FireMode.SINGLE],
    [WeaponType.RIFLE]: [FireMode.SINGLE, FireMode.AUTOMATIC],
    [WeaponType.SUBMACHINE_GUN]: [FireMode.SINGLE, FireMode.AUTOMATIC],
    [WeaponType.SHOTGUN]: [FireMode.SINGLE],
    [WeaponType.SNIPER_RIFLE]: [FireMode.SINGLE],
    [WeaponType.MACHINE_GUN]: [FireMode.AUTOMATIC],
    [WeaponType.KNIFE]: [FireMode.SINGLE],
    [WeaponType.GRENADE]: [FireMode.SINGLE],
    [WeaponType.C4]: [FireMode.SINGLE]
};

export const WeaponData = {
    glock: {
        name: 'Glock-18',
        type: WeaponType.PISTOL,
        price: 200,
        damage: 30,
        armorPenetration: 0.75,
        fireRate: 400,
        recoilPattern: 'glock',
        spread: { standing: 1.5, crouching: 1.0, moving: 3.0 },
        recoilForce: 2.5,
        accuracyDecay: 0.15,
        recoveryRate: 0.3,
        recoveryTime: 0.15,
        magSize: 17,
        reserveAmmo: 160,
        reloadTime: 2.2,
        deployTime: 0.5
    },
    p250: {
        name: 'P250',
        type: WeaponType.PISTOL,
        price: 300,
        damage: 35,
        armorPenetration: 0.77,
        fireRate: 333,
        recoilPattern: 'p250',
        spread: { standing: 1.3, crouching: 0.9, moving: 2.8 },
        recoilForce: 3.0,
        accuracyDecay: 0.12,
        recoveryRate: 0.35,
        recoveryTime: 0.12,
        magSize: 12,
        reserveAmmo: 120,
        reloadTime: 2.0,
        deployTime: 0.5
    },
    deagle: {
        name: 'Desert Eagle',
        type: WeaponType.PISTOL,
        price: 700,
        damage: 63,
        armorPenetration: 0.85,
        fireRate: 300,
        recoilPattern: 'deagle',
        spread: { standing: 2.5, crouching: 1.8, moving: 5.0 },
        recoilForce: 5.5,
        accuracyDecay: 0.2,
        recoveryRate: 0.4,
        recoveryTime: 0.25,
        magSize: 7,
        reserveAmmo: 35,
        reloadTime: 2.3,
        deployTime: 0.5
    },
    ak47: {
        name: 'AK-47',
        type: WeaponType.RIFLE,
        price: 2700,
        damage: 36,
        armorPenetration: 0.75,
        fireRate: 600,
        recoilPattern: 'ak47',
        spread: { standing: 2.0, crouching: 1.5, moving: 4.5 },
        recoilForce: 4.5,
        accuracyDecay: 0.18,
        recoveryRate: 0.4,
        recoveryTime: 0.15,
        magSize: 30,
        reserveAmmo: 90,
        reloadTime: 2.5,
        deployTime: 0.65
    },
    m4a4: {
        name: 'M4A4',
        type: WeaponType.RIFLE,
        price: 3100,
        damage: 33,
        armorPenetration: 0.70,
        fireRate: 600,
        recoilPattern: 'm4a4',
        spread: { standing: 1.8, crouching: 1.3, moving: 4.0 },
        recoilForce: 3.8,
        accuracyDecay: 0.15,
        recoveryRate: 0.35,
        recoveryTime: 0.14,
        magSize: 25,
        reserveAmmo: 75,
        reloadTime: 2.8,
        deployTime: 0.65
    },
    m4a1s: {
        name: 'M4A1-S',
        type: WeaponType.RIFLE,
        price: 2900,
        damage: 33,
        armorPenetration: 0.70,
        fireRate: 600,
        recoilPattern: 'm4a1s',
        spread: { standing: 1.5, crouching: 1.1, moving: 3.5 },
        recoilForce: 3.2,
        accuracyDecay: 0.12,
        recoveryRate: 0.32,
        recoveryTime: 0.12,
        magSize: 20,
        reserveAmmo: 75,
        reloadTime: 2.5,
        deployTime: 0.6
    },
    awp: {
        name: 'AWP',
        type: WeaponType.SNIPER_RIFLE,
        price: 4750,
        damage: 115,
        armorPenetration: 0.95,
        fireRate: 55,
        recoilPattern: 'awp',
        spread: { standing: 0.5, crouching: 0.3, moving: 1.5 },
        recoilForce: 8.0,
        accuracyDecay: 0.05,
        recoveryRate: 0.6,
        recoveryTime: 0.4,
        magSize: 10,
        reserveAmmo: 30,
        reloadTime: 3.7,
        deployTime: 0.9,
        scope: true
    },
    aug: {
        name: 'AUG',
        type: WeaponType.RIFLE,
        price: 3300,
        damage: 32,
        armorPenetration: 0.70,
        fireRate: 600,
        recoilPattern: 'aug',
        spread: { standing: 1.7, crouching: 1.2, moving: 3.8 },
        recoilForce: 3.5,
        accuracyDecay: 0.14,
        recoveryRate: 0.35,
        recoveryTime: 0.14,
        magSize: 30,
        reserveAmmo: 90,
        reloadTime: 2.8,
        deployTime: 0.65,
        scope: true
    },
    sg553: {
        name: 'SG 553',
        type: WeaponType.RIFLE,
        price: 3000,
        damage: 34,
        armorPenetration: 0.72,
        fireRate: 600,
        recoilPattern: 'sg553',
        spread: { standing: 1.8, crouching: 1.3, moving: 4.0 },
        recoilForce: 3.8,
        accuracyDecay: 0.15,
        recoveryRate: 0.35,
        recoveryTime: 0.14,
        magSize: 30,
        reserveAmmo: 90,
        reloadTime: 2.7,
        deployTime: 0.65,
        scope: true
    },
    galilar: {
        name: 'Galil AR',
        type: WeaponType.RIFLE,
        price: 2000,
        damage: 30,
        armorPenetration: 0.68,
        fireRate: 600,
        recoilPattern: 'galilar',
        spread: { standing: 2.2, crouching: 1.6, moving: 4.8 },
        recoilForce: 4.0,
        accuracyDecay: 0.16,
        recoveryRate: 0.38,
        recoveryTime: 0.15,
        magSize: 35,
        reserveAmmo: 105,
        reloadTime: 2.4,
        deployTime: 0.6
    },
    famas: {
        name: 'FAMAS',
        type: WeaponType.RIFLE,
        price: 2250,
        damage: 30,
        armorPenetration: 0.68,
        fireRate: 650,
        recoilPattern: 'famas',
        spread: { standing: 1.6, crouching: 1.1, moving: 3.5 },
        recoilForce: 3.2,
        accuracyDecay: 0.12,
        recoveryRate: 0.30,
        recoveryTime: 0.11,
        magSize: 25,
        reserveAmmo: 75,
        reloadTime: 2.5,
        deployTime: 0.6,
        burst: true
    },
    ssg08: {
        name: 'SSG 08',
        type: WeaponType.SNIPER_RIFLE,
        price: 2700,
        damage: 74,
        armorPenetration: 0.85,
        fireRate: 90,
        recoilPattern: 'ssg08',
        spread: { standing: 0.6, crouching: 0.4, moving: 1.8 },
        recoilForce: 5.5,
        accuracyDecay: 0.08,
        recoveryRate: 0.5,
        recoveryTime: 0.3,
        magSize: 10,
        reserveAmmo: 30,
        reloadTime: 3.0,
        deployTime: 0.75
    },
    scar20: {
        name: 'SCAR-20',
        type: WeaponType.SNIPER_RIFLE,
        price: 5000,
        damage: 70,
        armorPenetration: 0.82,
        fireRate: 250,
        recoilPattern: 'scar20',
        spread: { standing: 0.7, crouching: 0.5, moving: 2.0 },
        recoilForce: 4.0,
        accuracyDecay: 0.1,
        recoveryRate: 0.45,
        recoveryTime: 0.2,
        magSize: 20,
        reserveAmmo: 60,
        reloadTime: 2.7,
        deployTime: 0.7,
        automatic: true
    },
    g3sg1: {
        name: 'G3SG1',
        type: WeaponType.SNIPER_RIFLE,
        price: 5000,
        damage: 75,
        armorPenetration: 0.83,
        fireRate: 240,
        recoilPattern: 'g3sg1',
        spread: { standing: 0.8, crouching: 0.6, moving: 2.2 },
        recoilForce: 4.2,
        accuracyDecay: 0.12,
        recoveryRate: 0.48,
        recoveryTime: 0.22,
        magSize: 20,
        reserveAmmo: 60,
        reloadTime: 2.8,
        deployTime: 0.75
    },
    mp9: {
        name: 'MP9',
        type: WeaponType.SUBMACHINE_GUN,
        price: 1250,
        damage: 26,
        armorPenetration: 0.60,
        fireRate: 750,
        recoilPattern: 'mp9',
        spread: { standing: 2.0, crouching: 1.5, moving: 4.2 },
        recoilForce: 2.5,
        accuracyDecay: 0.18,
        recoveryRate: 0.32,
        recoveryTime: 0.11,
        magSize: 30,
        reserveAmmo: 120,
        reloadTime: 1.9,
        deployTime: 0.5
    },
    mac10: {
        name: 'MAC-10',
        type: WeaponType.SUBMACHINE_GUN,
        price: 1050,
        damage: 27,
        armorPenetration: 0.58,
        fireRate: 800,
        recoilPattern: 'mac10',
        spread: { standing: 2.5, crouching: 1.8, moving: 5.0 },
        recoilForce: 2.2,
        accuracyDecay: 0.22,
        recoveryRate: 0.28,
        recoveryTime: 0.1,
        magSize: 30,
        reserveAmmo: 120,
        reloadTime: 1.8,
        deployTime: 0.45
    },
    ump45: {
        name: 'UMP-45',
        type: WeaponType.SUBMACHINE_GUN,
        price: 1200,
        damage: 35,
        armorPenetration: 0.65,
        fireRate: 500,
        recoilPattern: 'ump45',
        spread: { standing: 1.5, crouching: 1.0, moving: 3.2 },
        recoilForce: 3.0,
        accuracyDecay: 0.14,
        recoveryRate: 0.35,
        recoveryTime: 0.13,
        magSize: 25,
        reserveAmmo: 100,
        reloadTime: 2.3,
        deployTime: 0.55
    },
    mp7: {
        name: 'MP7',
        type: WeaponType.SUBMACHINE_GUN,
        price: 1500,
        damage: 29,
        armorPenetration: 0.62,
        fireRate: 700,
        recoilPattern: 'mp7',
        spread: { standing: 1.6, crouching: 1.1, moving: 3.5 },
        recoilForce: 2.8,
        accuracyDecay: 0.15,
        recoveryRate: 0.33,
        recoveryTime: 0.12,
        magSize: 30,
        reserveAmmo: 120,
        reloadTime: 2.1,
        deployTime: 0.5
    },
    mp5sd: {
        name: 'MP5-SD',
        type: WeaponType.SUBMACHINE_GUN,
        price: 1700,
        damage: 27,
        armorPenetration: 0.58,
        fireRate: 600,
        recoilPattern: 'mp5sd',
        spread: { standing: 1.3, crouching: 0.9, moving: 2.8 },
        recoilForce: 2.5,
        accuracyDecay: 0.12,
        recoveryRate: 0.30,
        recoveryTime: 0.1,
        magSize: 30,
        reserveAmmo: 120,
        reloadTime: 2.0,
        deployTime: 0.5,
        silenced: true
    },
    bizon: {
        name: 'PP-Bizon',
        type: WeaponType.SUBMACHINE_GUN,
        price: 1400,
        damage: 27,
        armorPenetration: 0.58,
        fireRate: 700,
        recoilPattern: 'bizon',
        spread: { standing: 1.8, crouching: 1.3, moving: 3.8 },
        recoilForce: 2.4,
        accuracyDecay: 0.16,
        recoveryRate: 0.32,
        recoveryTime: 0.11,
        magSize: 64,
        reserveAmmo: 120,
        reloadTime: 2.2,
        deployTime: 0.5
    },
    nova: {
        name: 'Nova',
        type: WeaponType.SHOTGUN,
        price: 1200,
        damage: 26,
        armorPenetration: 0.45,
        fireRate: 80,
        recoilPattern: 'nova',
        spread: { standing: 12.0, crouching: 8.0, moving: 18.0 },
        recoilForce: 6.0,
        accuracyDecay: 0.3,
        recoveryRate: 0.5,
        recoveryTime: 0.5,
        magSize: 8,
        reserveAmmo: 32,
        reloadTime: 2.8,
        deployTime: 0.6,
        pellets: 8
    },
    mag7: {
        name: 'MAG-7',
        type: WeaponType.SHOTGUN,
        price: 1300,
        damage: 28,
        armorPenetration: 0.48,
        fireRate: 70,
        recoilPattern: 'mag7',
        spread: { standing: 10.0, crouching: 6.5, moving: 15.0 },
        recoilForce: 5.5,
        accuracyDecay: 0.25,
        recoveryRate: 0.45,
        recoveryTime: 0.4,
        magSize: 5,
        reserveAmmo: 28,
        reloadTime: 2.5,
        deployTime: 0.55,
        pellets: 8
    },
    xm1014: {
        name: 'XM1014',
        type: WeaponType.SHOTGUN,
        price: 2000,
        damage: 22,
        armorPenetration: 0.40,
        fireRate: 200,
        recoilPattern: 'xm1014',
        spread: { standing: 14.0, crouching: 9.0, moving: 20.0 },
        recoilForce: 5.0,
        accuracyDecay: 0.35,
        recoveryRate: 0.55,
        recoveryTime: 0.55,
        magSize: 4,
        reserveAmmo: 32,
        reloadTime: 3.0,
        deployTime: 0.65,
        automatic: true,
        pellets: 6
    },
    sawedoff: {
        name: 'Sawed-Off',
        type: WeaponType.SHOTGUN,
        price: 1100,
        damage: 30,
        armorPenetration: 0.42,
        fireRate: 60,
        recoilPattern: 'sawedoff',
        spread: { standing: 16.0, crouching: 10.0, moving: 22.0 },
        recoilForce: 7.0,
        accuracyDecay: 0.3,
        recoveryRate: 0.55,
        recoveryTime: 0.5,
        magSize: 7,
        reserveAmmo: 28,
        reloadTime: 2.2,
        deployTime: 0.5,
        pellets: 8
    },
    negev: {
        name: 'Negev',
        type: WeaponType.MACHINE_GUN,
        price: 5700,
        damage: 35,
        armorPenetration: 0.73,
        fireRate: 800,
        recoilPattern: 'negev',
        spread: { standing: 2.5, crouching: 1.8, moving: 5.5 },
        recoilForce: 4.0,
        accuracyDecay: 0.25,
        recoveryRate: 0.45,
        recoveryTime: 0.18,
        magSize: 150,
        reserveAmmo: 150,
        reloadTime: 5.5,
        deployTime: 0.85
    },
    m249: {
        name: 'M249',
        type: WeaponType.MACHINE_GUN,
        price: 5200,
        damage: 32,
        armorPenetration: 0.70,
        fireRate: 750,
        recoilPattern: 'm249',
        spread: { standing: 2.2, crouching: 1.6, moving: 5.0 },
        recoilForce: 4.2,
        accuracyDecay: 0.2,
        recoveryRate: 0.42,
        recoveryTime: 0.17,
        magSize: 100,
        reserveAmmo: 100,
        reloadTime: 5.5,
        deployTime: 0.8
    },
    knife: {
        name: 'Knife',
        type: WeaponType.KNIFE,
        price: 0,
        damage: 40,
        armorPenetration: 1.0,
        fireRate: 500,
        recoilPattern: 'knife',
        spread: { standing: 0, crouching: 0, moving: 0 },
        recoilForce: 0,
        magSize: 0,
        reserveAmmo: 0,
        reloadTime: 0,
        deployTime: 0.4
    }
};

export class RecoilPattern {
    constructor(name) {
        this.name = name;
        this.points = this.generatePattern(name);
    }

    generatePattern(name) {
        const patterns = {
            ak47: [
                { x: 0, y: 0 },
                { x: 1.5, y: -2 },
                { x: -0.5, y: -4 },
                { x: 2.5, y: -6 },
                { x: -1.5, y: -8 },
                { x: 0, y: -10 },
                { x: 1, y: -12 },
                { x: -0.5, y: -14 },
                { x: 2, y: -16 },
                { x: -1, y: -18 },
                { x: 0.5, y: -20 },
                { x: 1, y: -22 },
                { x: -0.5, y: -24 },
                { x: 0, y: -26 }
            ],
            m4a4: [
                { x: 0, y: 0 },
                { x: 1, y: -1.5 },
                { x: -0.5, y: -3 },
                { x: 0.5, y: -4.5 },
                { x: -1, y: -6 },
                { x: 0, y: -7.5 },
                { x: 1, y: -9 },
                { x: -0.5, y: -10.5 },
                { x: 0.5, y: -12 },
                { x: -1, y: -13.5 },
                { x: 0, y: -15 }
            ],
            m4a1s: [
                { x: 0, y: 0 },
                { x: 0.8, y: -1.2 },
                { x: -0.3, y: -2.4 },
                { x: 0.4, y: -3.6 },
                { x: -0.6, y: -4.8 },
                { x: 0, y: -6 },
                { x: 0.6, y: -7.2 },
                { x: -0.4, y: -8.4 },
                { x: 0.2, y: -9.6 },
                { x: -0.2, y: -10.8 }
            ],
            awp: [
                { x: 0, y: 0 },
                { x: 1.5, y: -2 },
                { x: -1, y: -4 },
                { x: 1, y: -6 },
                { x: -0.5, y: -8 },
                { x: 0.5, y: -10 }
            ],
            deagle: [
                { x: 0, y: 0 },
                { x: 3, y: -2 },
                { x: -2, y: -5 },
                { x: 4, y: -8 },
                { x: -3, y: -12 }
            ],
            glock: [
                { x: 0, y: 0 },
                { x: 1, y: -1 },
                { x: -0.5, y: -2 },
                { x: 0.5, y: -3 },
                { x: -1, y: -4 },
                { x: 0.5, y: -5 },
                { x: -0.5, y: -6 }
            ],
            p250: [
                { x: 0, y: 0 },
                { x: 1.2, y: -1.5 },
                { x: -0.8, y: -3 },
                { x: 1, y: -4.5 },
                { x: -0.5, y: -6 }
            ],
            default: [
                { x: 0, y: 0 },
                { x: 0, y: -1 },
                { x: 0, y: -2 },
                { x: 0, y: -3 },
                { x: 0, y: -4 },
                { x: 0, y: -5 }
            ]
        };

        return patterns[name] || patterns.default;
    }

    getPoint(index) {
        return this.points[index % this.points.length];
    }

    getControlPoint(count) {
        const points = [];
        for (let i = 0; i < count; i++) {
            points.push(this.getPoint(i));
        }
        return points;
    }
}

export class WeaponPhysics {
    constructor(weaponId) {
        this.weaponId = weaponId;
        this.data = WeaponData[weaponId] || WeaponData.knife;
        this.recoilPattern = new RecoilPattern(this.data.recoilPattern || 'default');
        this.currentRecoil = { x: 0, y: 0 };
        this.spread = 0;
        this.accuracy = 1.0;
        this.lastFireTime = 0;
        this.lastRecoveryTime = 0;
        this.isRecovering = false;
        this.burstCount = 0;
        this.recoilIndex = 0;
    }

    fire(fireMode, deltaTime) {
        const now = Date.now();
        const timeSinceLastFire = now - this.lastFireTime;
        const fireInterval = 60000 / this.data.fireRate;

        if (timeSinceLastFire < fireInterval) {
            return null;
        }

        if (fireMode === FireMode.BURST) {
            this.burstCount++;
            if (this.burstCount >= 3) {
                this.burstCount = 0;
            }
        }

        this.recoilIndex++;
        const recoilPoint = this.recoilPattern.getPoint(this.recoilIndex);
        this.currentRecoil.x += recoilPoint.x * this.data.recoilForce * 0.1;
        this.currentRecoil.y += recoilPoint.y * this.data.recoilForce * 0.1;
        this.currentRecoil.y = Math.min(this.currentRecoil.y, -30);
        this.currentRecoil.x = Math.max(-15, Math.min(15, this.currentRecoil.x));

        const spreadMultiplier = 1 + Math.abs(this.currentRecoil.y) * 0.02;
        this.spread = this.calculateSpread() * spreadMultiplier;
        this.accuracy = Math.max(0, 1 - this.spread / 10);

        this.lastFireTime = now;
        this.lastRecoveryTime = now;
        this.isRecovering = false;

        return {
            recoil: { ...this.currentRecoil },
            spread: this.spread,
            accuracy: this.accuracy
        };
    }

    calculateSpread(movementState = 'standing') {
        let baseSpread = this.data.spread.standing;

        switch (movementState) {
            case 'crouching':
                baseSpread = this.data.spread.crouching;
                break;
            case 'moving':
                baseSpread = this.data.spread.moving;
                break;
        }

        const recoilPenalty = Math.abs(this.currentRecoil.y) * 0.15;
        const spreadPenalty = Math.abs(this.currentRecoil.x) * 0.1;

        return baseSpread + recoilPenalty + spreadPenalty;
    }

    update(deltaTime) {
        const now = Date.now();
        const timeSinceLastFire = now - this.lastRecoveryTime;
        const recoveryDelay = this.data.recoveryTime * 1000;

        if (timeSinceLastFire > recoveryDelay && !this.isRecovering) {
            this.isRecovering = true;
        }

        if (this.isRecovering && (this.currentRecoil.x !== 0 || this.currentRecoil.y !== 0)) {
            const recoverySpeed = this.data.recoveryRate * (deltaTime / 16);
            this.currentRecoil.x *= (1 - recoverySpeed);
            this.currentRecoil.y *= (1 - recoverySpeed);

            if (Math.abs(this.currentRecoil.x) < 0.1) this.currentRecoil.x = 0;
            if (Math.abs(this.currentRecoil.y) < 0.1) {
                this.currentRecoil.y = 0;
                this.recoilIndex = 0;
                this.isRecovering = false;
            }
        }

        this.spread = this.calculateSpread();
        this.accuracy = Math.max(0, 1 - this.spread / 10);
    }

    reset() {
        this.currentRecoil = { x: 0, y: 0 };
        this.spread = this.data.spread.standing;
        this.accuracy = 1.0;
        this.recoilIndex = 0;
        this.isRecovering = false;
        this.burstCount = 0;
    }

    getAimPenalty() {
        return {
            recoilPenalty: Math.sqrt(this.currentRecoil.x ** 2 + this.currentRecoil.y ** 2) * 0.5,
            spreadPenalty: this.spread * 2,
            totalPenalty: (Math.sqrt(this.currentRecoil.x ** 2 + this.currentRecoil.y ** 2) * 0.5) + (this.spread * 2)
        };
    }

    export() {
        return {
            weapon_id: this.weaponId,
            current_recoil: { ...this.currentRecoil },
            spread: this.spread,
            accuracy: this.accuracy,
            recoil_index: this.recoilIndex,
            is_recovering: this.isRecovering
        };
    }
}

export class BulletPhysics {
    constructor(weaponData, startPosition, direction, velocity) {
        this.weaponData = weaponData;
        this.startPosition = { ...startPosition };
        this.position = { ...startPosition };
        this.direction = { ...direction };
        this.velocity = { ...velocity };
        this.speed = Math.sqrt(velocity.x ** 2 + velocity.y ** 2 + velocity.z ** 2);
        this.lifetime = 0;
        this.maxLifetime = 2000;
        this.gravity = 800;
        this.drag = 0.99;
        this.hasHit = false;
        this.hitPosition = null;
        this.hitNormal = null;
        this.hitEntity = null;
        this.damageFalloff = [];
        this.travelDistance = 0;
        this.pellets = weaponData.pellets || 1;
    }

    update(deltaTime) {
        if (this.hasHit) return;

        this.lifetime += deltaTime;

        if (this.lifetime > this.maxLifetime) {
            this.hasHit = true;
            return;
        }

        const timeStep = deltaTime / 1000;

        this.velocity.z -= this.gravity * timeStep;
        this.velocity.x *= this.drag;
        this.velocity.y *= this.drag;

        const movement = {
            x: this.velocity.x * timeStep,
            y: this.velocity.y * timeStep,
            z: this.velocity.z * timeStep
        };

        this.position.x += movement.x;
        this.position.y += movement.y;
        this.position.z += movement.z;

        const frameDistance = Math.sqrt(movement.x ** 2 + movement.y ** 2 + movement.z ** 2);
        this.travelDistance += frameDistance;
    }

    hit(position, normal, entity = null) {
        this.hasHit = true;
        this.hitPosition = { ...position };
        this.hitNormal = { ...normal };
        this.hitEntity = entity;
    }

    calculateDamage(distance, hitZone = 'body') {
        const baseDamage = this.weaponData.damage;
        const armorPen = this.weaponData.armorPenetration;
        const falloff = this.calculateDamageFalloff(distance);

        let damage = baseDamage * falloff * armorPen;

        switch (hitZone) {
            case 'head':
                damage *= 4.0;
                break;
            case 'chest':
                damage *= 1.0;
                break;
            case 'stomach':
                damage *= 1.25;
                break;
            case 'left_arm':
            case 'right_arm':
            case 'left_leg':
            case 'right_leg':
                damage *= 0.75;
                break;
        }

        return Math.round(damage);
    }

    calculateDamageFalloff(distance) {
        const baseRange = 500;
        const maxRange = 2000;

        if (distance <= baseRange) {
            return 1.0;
        }

        if (distance >= maxRange) {
            return 0.5;
        }

        const falloff = 1 - ((distance - baseRange) / (maxRange - baseRange)) * 0.5;
        return Math.max(0.5, falloff);
    }

    getTracerData() {
        return {
            start: this.startPosition,
            end: this.position,
            distance: this.travelDistance,
            hasHit: this.hasHit,
            hitPosition: this.hitPosition
        };
    }

    export() {
        return {
            weapon_data: this.weaponData,
            start_position: this.startPosition,
            position: this.position,
            velocity: this.velocity,
            lifetime: this.lifetime,
            has_hit: this.hasHit,
            hit_position: this.hitPosition,
            hit_normal: this.hitNormal,
            hit_entity: this.hitEntity,
            travel_distance: this.travelDistance,
            pellets: this.pellets
        };
    }
}

export class WeaponSystem {
    constructor() {
        this.weapons = new Map();
        this.currentWeapon = null;
        this.equippedWeapons = new Map();
        this.ammo = new Map();
        this.listeners = new Map();
        this.physics = new Map();
        this.bullets = [];
        this.activeBullets = new Map();
    }

    addWeapon(weaponId, weaponData) {
        this.weapons.set(weaponId, weaponData);
        this.physics.set(weaponId, new WeaponPhysics(weaponId));
        this.ammo.set(weaponId, {
            current: weaponData.magSize,
            reserve: weaponData.reserveAmmo,
            max: weaponData.reserveAmmo
        });
    }

    equipWeapon(weaponId, slot) {
        if (!this.weapons.has(weaponId)) {
            this.addWeapon(weaponId, WeaponData[weaponId]);
        }

        this.equippedWeapons.set(slot, weaponId);

        if (slot === 'primary' || slot === 'secondary' || slot === 'melee') {
            this.currentWeapon = weaponId;
        }

        this.emit('weaponEquipped', { weaponId, slot });
    }

    switchWeapon(slot) {
        const weaponId = this.equippedWeapons.get(slot);
        if (weaponId) {
            this.currentWeapon = weaponId;
            this.emit('weaponSwitched', { weaponId, slot });
            return weaponId;
        }
        return null;
    }

    fire(fireMode, deltaTime) {
        if (!this.currentWeapon) return null;

        const ammo = this.ammo.get(this.currentWeapon);
        if (!ammo || ammo.current <= 0) {
            this.emit('outOfAmmo', { weaponId: this.currentWeapon });
            return null;
        }

        const physics = this.physics.get(this.currentWeapon);
        if (!physics) return null;

        const fireData = physics.fire(fireMode, deltaTime);
        if (!fireData) return null;

        ammo.current--;
        this.emit('weaponFired', {
            weaponId: this.currentWeapon,
            ammo: ammo.current,
            recoil: fireData.recoil,
            spread: fireData.spread,
            accuracy: fireData.accuracy
        });

        return fireData;
    }

    reload() {
        if (!this.currentWeapon) return false;

        const ammo = this.ammo.get(this.currentWeapon);
        const weaponData = this.weapons.get(this.currentWeapon);

        if (!ammo || !weaponData) return false;

        const needed = weaponData.magSize - ammo.current;
        const available = Math.min(needed, ammo.reserve);

        if (available <= 0) return false;

        ammo.current += available;
        ammo.reserve -= available;

        this.emit('weaponReloaded', {
            weaponId: this.currentWeapon,
            ammo: ammo.current,
            reserve: ammo.reserve
        });

        return true;
    }

    addAmmo(weaponId, amount) {
        const ammo = this.ammo.get(weaponId);
        if (ammo) {
            ammo.reserve = Math.min(ammo.reserve + amount, ammo.max);
            return true;
        }
        return false;
    }

    getAmmo(weaponId) {
        return this.ammo.get(weaponId);
    }

    getCurrentAmmo() {
        if (!this.currentWeapon) return null;
        return this.ammo.get(this.currentWeapon);
    }

    getWeaponData(weaponId) {
        return this.weapons.get(weaponId);
    }

    getPhysics(weaponId) {
        return this.physics.get(weaponId);
    }

    getCurrentPhysics() {
        if (!this.currentWeapon) return null;
        return this.physics.get(this.currentWeapon);
    }

    update(deltaTime) {
        for (const physics of this.physics.values()) {
            physics.update(deltaTime);
        }

        for (const bullet of this.bullets) {
            bullet.update(deltaTime);

            if (bullet.hasHit) {
                this.emit('bulletHit', bullet.getTracerData());
            }
        }

        this.bullets = this.bullets.filter(b => !b.hasHit && b.lifetime < b.maxLifetime);
    }

    createBullet(weaponId, startPosition, direction, velocity) {
        const weaponData = this.weapons.get(weaponId) || WeaponData[weaponId];
        const bullet = new BulletPhysics(weaponData, startPosition, direction, velocity);

        if (weaponData.pellets && weaponData.pellets > 1) {
            for (let i = 0; i < weaponData.pellets; i++) {
                const spreadDir = {
                    x: direction.x + (Math.random() - 0.5) * 0.1,
                    y: direction.y + (Math.random() - 0.5) * 0.1,
                    z: direction.z
                };
                const bullet = new BulletPhysics(weaponData, startPosition, spreadDir, velocity);
                this.bullets.push(bullet);
            }
        } else {
            this.bullets.push(bullet);
        }

        return bullet;
    }

    dropWeapon(weaponId) {
        const ammo = this.ammo.get(weaponId);
        const weaponData = this.weapons.get(weaponId);

        if (ammo && weaponData) {
            this.emit('weaponDropped', {
                weaponId,
                ammo: ammo.current,
                data: weaponData
            });

            this.ammo.delete(weaponId);
            this.weapons.delete(weaponId);
            this.physics.delete(weaponId);

            for (const [slot, id] of this.equippedWeapons) {
                if (id === weaponId) {
                    this.equippedWeapons.delete(slot);
                    break;
                }
            }

            if (this.currentWeapon === weaponId) {
                this.currentWeapon = null;
            }

            return true;
        }

        return false;
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
                console.error(`Error in weapon system listener: ${e}`);
            }
        }
    }

    export() {
        const weaponsData = {};
        for (const [id, data] of this.weapons) {
            weaponsData[id] = data;
        }

        return {
            current_weapon: this.currentWeapon,
            equipped_weapons: Object.fromEntries(this.equippedWeapons),
            ammo: Object.fromEntries(this.ammo),
            physics: Object.fromEntries(
                Array.from(this.physics.entries()).map(([k, v]) => [k, v.export()])
            ),
            bullets_count: this.bullets.length
        };
    }
}

export const Weapons = new WeaponSystem();
