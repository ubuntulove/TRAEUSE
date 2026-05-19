import { WEAPON_CONFIG, WEAPON_TYPE, DAMAGE_TYPE, GAME_CONFIG } from './config.js';

export class Weapon {
    constructor(weaponId, weaponData) {
        this.id = weaponId;
        this.name = weaponData.name || 'Unknown Weapon';
        this.type = weaponData.type || WEAPON_TYPE.UNKNOWN;
        this.category = weaponData.category || 'other';
        
        this.damage = weaponData.damage || 20;
        this.headshotMultiplier = weaponData.headshotMultiplier || 4.0;
        this.armorPenetration = weaponData.armorPenetration || 1.0;
        
        this.magazineSize = weaponData.magazineSize || 30;
        this.reserveAmmo = weaponData.reserveAmmo || 90;
        this.ammoType = weaponData.ammoType || '556mm';
        
        this.fireRate = weaponData.fireRate || 600;
        this.fireMode = weaponData.fireMode || 'automatic';
        this.burstRate = weaponData.burstRate || 3;
        
        this.recoil = {
            xMin: weaponData.recoil?.xMin || -1,
            xMax: weaponData.recoil?.xMax || 1,
            yMin: weaponData.recoil?.yMin || 0.5,
            yMax: weaponData.recoil?.yMax || 2.0,
            magnitude: weaponData.recoil?.magnitude || 1.0,
            pattern: weaponData.recoil?.pattern || []
        };
        
        this.spread = {
            standing: weaponData.spread?.standing || 0.1,
            moving: weaponData.spread?.moving || 0.2,
            jumping: weaponData.spread?.jumping || 0.4,
            crouching: weaponData.spread?.crouching || 0.05,
            scoped: weaponData.spread?.scoped || 0.02
        };
        
        this.reloadTime = weaponData.reloadTime || 2.0;
        this.drawTime = weaponData.drawTime || 0.5;
        this.equipTime = weaponData.equipTime || 0.5;
        
        this.moveSpeedMultiplier = weaponData.moveSpeedMultiplier || 1.0;
        this.inaccuracyMove = weaponData.inaccuracyMove || 0.0;
        this.inaccuracyStand = weaponData.inaccuracyStand || 0.0;
        
        this.price = weaponData.price || 0;
        this.kitRequired = weaponData.kitRequired || false;
        this.teamRestricted = weaponData.teamRestricted || null;
        
        this.sounds = {
            shoot: weaponData.sounds?.shoot || 'default_shot',
            reload: weaponData.sounds?.reload || 'default_reload',
            draw: weaponData.sounds?.draw || 'default_draw',
            empty: weaponData.sounds?.empty || 'default_empty'
        };
        
        this.model = {
            view: weaponData.model?.view || 'default_view',
            world: weaponData.model?.world || 'default_world',
            dropped: weaponData.model?.dropped || 'default_dropped'
        };
        
        this.muzzleVelocity = weaponData.muzzleVelocity || 800;
        this.projectileSpeed = weaponData.projectileSpeed || 800;
        this.gravity = weaponData.gravity || 0;
        
        this.range = weaponData.range || 4096;
        this.rangeModifier = weaponData.rangeModifier || 0.98;
        
        this.zoomLevels = weaponData.zoomLevels || [1.0];
        this.zoomFOV = weaponData.zoomFOV || [90];
        
        this.tracerFrequency = weaponData.tracerFrequency || 1;
        this.tracerStyle = weaponData.tracerStyle || 'default';
        
        this.killAward = weaponData.killAward || 300;
        this.headshotAward = weaponData.headshotAward || 150;
        
        this.state = {
            currentAmmo: this.magazineSize,
            reserveAmmo: this.reserveAmmo,
            lastShotTime: 0,
            burstCount: 0,
            isReloading: false,
            reloadStartTime: 0,
            isEquipped: false,
            isHolstered: true
        };
        
        this.statistics = {
            shotsFired: 0,
            shotsHit: 0,
            kills: 0,
            headshots: 0,
            damageDealt: 0,
            accuracy: 0
        };
    }
    
    canFire() {
        if (this.state.isReloading) return false;
        if (this.state.currentAmmo <= 0) return false;
        
        const currentTime = Date.now();
        const fireInterval = 60000 / this.fireRate;
        
        if (currentTime - this.state.lastShotTime < fireInterval) return false;
        
        return true;
    }
    
    fire() {
        if (!this.canFire()) return null;
        
        this.state.currentAmmo--;
        this.state.lastShotTime = Date.now();
        this.statistics.shotsFired++;
        
        const recoilX = this.recoil.xMin + Math.random() * (this.recoil.xMax - this.recoil.xMin);
        const recoilY = this.recoil.yMin + Math.random() * (this.recoil.yMax - this.recoil.yMin);
        
        const spread = this.calculateSpread();
        
        return {
            position: null,
            direction: null,
            recoil: { x: recoilX, y: recoilY },
            spread: spread,
            damage: this.damage,
            headshotMultiplier: this.headshotMultiplier,
            armorPenetration: this.armorPenetration,
            time: this.state.lastShotTime
        };
    }
    
    calculateSpread() {
        let baseSpread = this.spread.standing;
        
        baseSpread += this.spread.moving * 0.5;
        baseSpread += this.spread.jumping * 0.3;
        baseSpread *= this.spread.crouching;
        
        return baseSpread;
    }
    
    reload() {
        if (this.state.isReloading) return false;
        if (this.state.currentAmmo === this.magazineSize) return false;
        if (this.state.reserveAmmo <= 0) return false;
        
        this.state.isReloading = true;
        this.state.reloadStartTime = Date.now();
        
        return true;
    }
    
    completeReload() {
        const bulletsNeeded = this.magazineSize - this.state.currentAmmo;
        const bulletsToLoad = Math.min(bulletsNeeded, this.state.reserveAmmo);
        
        this.state.currentAmmo += bulletsToLoad;
        this.state.reserveAmmo -= bulletsToLoad;
        this.state.isReloading = false;
    }
    
    cancelReload() {
        this.state.isReloading = false;
    }
    
    equip() {
        this.state.isEquipped = true;
        this.state.isHolstered = false;
    }
    
    holster() {
        this.state.isEquipped = false;
        this.state.isHolstered = true;
    }
    
    calculateDamage(distance, isHeadshot, targetArmor) {
        let damage = this.damage;
        
        const distanceFactor = Math.pow(this.rangeModifier, distance / 500);
        damage *= distanceFactor;
        
        if (isHeadshot) {
            damage *= this.headshotMultiplier;
        }
        
        if (targetArmor > 0) {
            const armorReduction = targetArmor * 0.01 * this.armorPenetration;
            damage *= (1 - armorReduction);
        }
        
        return Math.max(1, Math.floor(damage));
    }
    
    getFireInterval() {
        return 60000 / this.fireRate;
    }
    
    getReloadProgress() {
        if (!this.state.isReloading) return 1.0;
        
        const elapsed = Date.now() - this.state.reloadStartTime;
        return Math.min(elapsed / (this.reloadTime * 1000), 1.0);
    }
    
    getState() {
        return {
            currentAmmo: this.state.currentAmmo,
            reserveAmmo: this.state.reserveAmmo,
            isReloading: this.state.isReloading,
            reloadProgress: this.getReloadProgress(),
            isEquipped: this.state.isEquipped
        };
    }
    
    getStatistics() {
        if (this.statistics.shotsFired > 0) {
            this.statistics.accuracy = (this.statistics.shotsHit / this.statistics.shotsFired * 100).toFixed(1);
        }
        
        return { ...this.statistics };
    }
    
    resetStatistics() {
        this.statistics = {
            shotsFired: 0,
            shotsHit: 0,
            kills: 0,
            headshots: 0,
            damageDealt: 0,
            accuracy: 0
        };
    }
    
    reset() {
        this.state.currentAmmo = this.magazineSize;
        this.state.reserveAmmo = this.reserveAmmo;
        this.state.lastShotTime = 0;
        this.state.burstCount = 0;
        this.state.isReloading = false;
        this.state.reloadStartTime = 0;
    }
}

export class WeaponFactory {
    static createWeapon(weaponId) {
        const weaponData = this.getWeaponData(weaponId);
        if (!weaponData) return null;
        
        return new Weapon(weaponId, weaponData);
    }
    
    static getWeaponData(weaponId) {
        const weapons = {
            'weapon_knife': {
                name: 'Knife',
                type: WEAPON_TYPE.KNIFE,
                category: 'melee',
                damage: 40,
                headshotMultiplier: 1.0,
                price: 0,
                fireMode: 'melee'
            },
            
            'weapon_glock': {
                name: 'Glock-18',
                type: WEAPON_TYPE.PISTOL,
                category: 'pistol',
                damage: 28,
                headshotMultiplier: 4.0,
                armorPenetration: 0.75,
                magazineSize: 20,
                reserveAmmo: 120,
                fireRate: 400,
                fireMode: 'automatic',
                recoil: {
                    xMin: -3.0,
                    xMax: 3.0,
                    yMin: 0.5,
                    yMax: 2.0,
                    magnitude: 1.0
                },
                spread: {
                    standing: 0.02,
                    moving: 0.04,
                    jumping: 0.12
                },
                reloadTime: 2.2,
                price: 200,
                killAward: 300
            },
            
            'weapon_usp': {
                name: 'USP-S',
                type: WEAPON_TYPE.PISTOL,
                category: 'pistol',
                damage: 35,
                headshotMultiplier: 4.0,
                armorPenetration: 0.8,
                magazineSize: 12,
                reserveAmmo: 120,
                fireRate: 350,
                fireMode: 'automatic',
                recoil: {
                    xMin: -2.5,
                    xMax: 2.5,
                    yMin: 0.4,
                    yMax: 1.8,
                    magnitude: 0.9
                },
                spread: {
                    standing: 0.018,
                    moving: 0.035,
                    jumping: 0.11
                },
                reloadTime: 2.3,
                price: 200,
                killAward: 300
            },
            
            'weapon_p250': {
                name: 'P250',
                type: WEAPON_TYPE.PISTOL,
                category: 'pistol',
                damage: 35,
                headshotMultiplier: 4.0,
                armorPenetration: 0.8,
                magazineSize: 13,
                reserveAmmo: 130,
                fireRate: 400,
                fireMode: 'semi-auto',
                recoil: {
                    xMin: -2.0,
                    xMax: 2.0,
                    yMin: 0.5,
                    yMax: 1.5,
                    magnitude: 0.8
                },
                spread: {
                    standing: 0.025,
                    moving: 0.045,
                    jumping: 0.13
                },
                reloadTime: 2.1,
                price: 300,
                killAward: 300
            },
            
            'weapon_deagle': {
                name: 'Desert Eagle',
                type: WEAPON_TYPE.PISTOL,
                category: 'pistol',
                damage: 63,
                headshotMultiplier: 4.0,
                armorPenetration: 0.95,
                magazineSize: 7,
                reserveAmmo: 70,
                fireRate: 300,
                fireMode: 'semi-auto',
                recoil: {
                    xMin: -4.0,
                    xMax: 4.0,
                    yMin: 1.5,
                    yMax: 4.0,
                    magnitude: 1.5
                },
                spread: {
                    standing: 0.03,
                    moving: 0.06,
                    jumping: 0.18
                },
                reloadTime: 2.2,
                price: 700,
                killAward: 300
            },
            
            'weapon_ak47': {
                name: 'AK-47',
                type: WEAPON_TYPE.RIFLE,
                category: 'assault',
                damage: 36,
                headshotMultiplier: 4.0,
                armorPenetration: 0.9,
                magazineSize: 30,
                reserveAmmo: 90,
                fireRate: 600,
                fireMode: 'automatic',
                recoil: {
                    xMin: -2.5,
                    xMax: 2.5,
                    yMin: 0.8,
                    yMax: 2.0,
                    magnitude: 1.2
                },
                spread: {
                    standing: 0.035,
                    moving: 0.07,
                    jumping: 0.2,
                    crouching: 0.025
                },
                reloadTime: 2.4,
                price: 2700,
                killAward: 300,
                teamRestricted: TEAM.TERRORIST
            },
            
            'weapon_m4a4': {
                name: 'M4A4',
                type: WEAPON_TYPE.RIFLE,
                category: 'assault',
                damage: 33,
                headshotMultiplier: 4.0,
                armorPenetration: 0.9,
                magazineSize: 30,
                reserveAmmo: 90,
                fireRate: 600,
                fireMode: 'automatic',
                recoil: {
                    xMin: -2.0,
                    xMax: 2.0,
                    yMin: 0.6,
                    yMax: 1.5,
                    magnitude: 1.0
                },
                spread: {
                    standing: 0.03,
                    moving: 0.06,
                    jumping: 0.18,
                    crouching: 0.02
                },
                reloadTime: 2.3,
                price: 3100,
                killAward: 300,
                teamRestricted: TEAM.COUNTER_TERRORIST
            },
            
            'weapon_m4a1': {
                name: 'M4A1-S',
                type: WEAPON_TYPE.RIFLE,
                category: 'assault',
                damage: 33,
                headshotMultiplier: 4.0,
                armorPenetration: 0.9,
                magazineSize: 25,
                reserveAmmo: 75,
                fireRate: 600,
                fireMode: 'automatic',
                recoil: {
                    xMin: -1.8,
                    xMax: 1.8,
                    yMin: 0.5,
                    yMax: 1.3,
                    magnitude: 0.9
                },
                spread: {
                    standing: 0.028,
                    moving: 0.055,
                    jumping: 0.17,
                    crouching: 0.018
                },
                reloadTime: 2.2,
                price: 2900,
                killAward: 300,
                teamRestricted: TEAM.COUNTER_TERRORIST
            },
            
            'weapon_awp': {
                name: 'AWP',
                type: WEAPON_TYPE.SNIPER,
                category: 'sniper',
                damage: 115,
                headshotMultiplier: 1.0,
                armorPenetration: 0.99,
                magazineSize: 10,
                reserveAmmo: 50,
                fireRate: 60,
                fireMode: 'bolt-action',
                zoomLevels: [1.0, 4.0],
                zoomFOV: [90, 25],
                recoil: {
                    xMin: -5.0,
                    xMax: 5.0,
                    yMin: 2.0,
                    yMax: 5.0,
                    magnitude: 2.5
                },
                spread: {
                    standing: 0.01,
                    moving: 0.15,
                    jumping: 0.5,
                    scoped: 0.005
                },
                reloadTime: 3.7,
                price: 4750,
                killAward: 300
            },
            
            'weapon_sg553': {
                name: 'SG 553',
                type: WEAPON_TYPE.RIFLE,
                category: 'assault',
                damage: 38,
                headshotMultiplier: 4.0,
                armorPenetration: 0.9,
                magazineSize: 30,
                reserveAmmo: 90,
                fireRate: 600,
                fireMode: 'automatic',
                recoil: {
                    xMin: -2.5,
                    xMax: 2.5,
                    yMin: 0.7,
                    yMax: 1.8,
                    magnitude: 1.15
                },
                spread: {
                    standing: 0.035,
                    moving: 0.07,
                    jumping: 0.2
                },
                reloadTime: 2.5,
                price: 3000,
                killAward: 300,
                teamRestricted: TEAM.TERRORIST
            },
            
            'weapon_aug': {
                name: 'AUG',
                type: WEAPON_TYPE.RIFLE,
                category: 'assault',
                damage: 35,
                headshotMultiplier: 4.0,
                armorPenetration: 0.9,
                magazineSize: 30,
                reserveAmmo: 90,
                fireRate: 600,
                fireMode: 'automatic',
                zoomLevels: [1.0, 1.5],
                zoomFOV: [90, 70],
                recoil: {
                    xMin: -2.0,
                    xMax: 2.0,
                    yMin: 0.6,
                    yMax: 1.5,
                    magnitude: 1.0
                },
                spread: {
                    standing: 0.03,
                    moving: 0.06,
                    jumping: 0.18
                },
                reloadTime: 2.4,
                price: 3300,
                killAward: 300,
                teamRestricted: TEAM.COUNTER_TERRORIST
            },
            
            'weapon_famas': {
                name: 'FAMAS',
                type: WEAPON_TYPE.RIFLE,
                category: 'assault',
                damage: 30,
                headshotMultiplier: 4.0,
                armorPenetration: 0.85,
                magazineSize: 25,
                reserveAmmo: 75,
                fireRate: 650,
                fireMode: 'burst',
                burstRate: 3,
                recoil: {
                    xMin: -1.5,
                    xMax: 1.5,
                    yMin: 0.4,
                    yMax: 1.0,
                    magnitude: 0.8
                },
                spread: {
                    standing: 0.028,
                    moving: 0.055,
                    jumping: 0.17
                },
                reloadTime: 2.3,
                price: 2050,
                killAward: 300,
                teamRestricted: TEAM.COUNTER_TERRORIST
            },
            
            'weapon_galilar': {
                name: 'Galil AR',
                type: WEAPON_TYPE.RIFLE,
                category: 'assault',
                damage: 30,
                headshotMultiplier: 4.0,
                armorPenetration: 0.85,
                magazineSize: 35,
                reserveAmmo: 105,
                fireRate: 600,
                fireMode: 'automatic',
                recoil: {
                    xMin: -2.5,
                    xMax: 2.5,
                    yMin: 0.8,
                    yMax: 2.0,
                    magnitude: 1.1
                },
                spread: {
                    standing: 0.038,
                    moving: 0.075,
                    jumping: 0.22
                },
                reloadTime: 2.4,
                price: 2000,
                killAward: 300,
                teamRestricted: TEAM.TERRORIST
            },
            
            'weapon_ssg08': {
                name: 'SSG 08',
                type: WEAPON_TYPE.SNIPER,
                category: 'sniper',
                damage: 50,
                headshotMultiplier: 4.0,
                armorPenetration: 0.95,
                magazineSize: 10,
                reserveAmmo: 50,
                fireRate: 90,
                fireMode: 'bolt-action',
                zoomLevels: [1.0, 2.0],
                zoomFOV: [90, 50],
                recoil: {
                    xMin: -3.0,
                    xMax: 3.0,
                    yMin: 1.0,
                    yMax: 2.5,
                    magnitude: 1.2
                },
                spread: {
                    standing: 0.015,
                    moving: 0.1,
                    jumping: 0.35,
                    scoped: 0.008
                },
                reloadTime: 2.8,
                price: 1700,
                killAward: 300
            },
            
            'weapon_awp': {
                name: 'AWP',
                type: WEAPON_TYPE.SNIPER,
                category: 'sniper',
                damage: 115,
                headshotMultiplier: 1.0,
                armorPenetration: 0.99,
                magazineSize: 10,
                reserveAmmo: 50,
                fireRate: 60,
                fireMode: 'bolt-action',
                zoomLevels: [1.0, 4.0],
                zoomFOV: [90, 25],
                recoil: {
                    xMin: -5.0,
                    xMax: 5.0,
                    yMin: 2.0,
                    yMax: 5.0,
                    magnitude: 2.5
                },
                spread: {
                    standing: 0.01,
                    moving: 0.15,
                    jumping: 0.5,
                    scoped: 0.005
                },
                reloadTime: 3.7,
                price: 4750,
                killAward: 300
            },
            
            'weapon_scar20': {
                name: 'SCAR-20',
                type: WEAPON_TYPE.SNIPER,
                category: 'sniper',
                damage: 65,
                headshotMultiplier: 4.0,
                armorPenetration: 0.98,
                magazineSize: 20,
                reserveAmmo: 60,
                fireRate: 270,
                fireMode: 'automatic',
                zoomLevels: [1.0, 2.0],
                zoomFOV: [90, 50],
                recoil: {
                    xMin: -2.5,
                    xMax: 2.5,
                    yMin: 0.8,
                    yMax: 2.0,
                    magnitude: 1.3
                },
                spread: {
                    standing: 0.02,
                    moving: 0.12,
                    jumping: 0.4,
                    scoped: 0.01
                },
                reloadTime: 2.9,
                price: 5000,
                killAward: 300,
                teamRestricted: TEAM.TERRORIST
            },
            
            'weapon_g3sg1': {
                name: 'G3SG1',
                type: WEAPON_TYPE.SNIPER,
                category: 'sniper',
                damage: 65,
                headshotMultiplier: 4.0,
                armorPenetration: 0.98,
                magazineSize: 20,
                reserveAmmo: 60,
                fireRate: 270,
                fireMode: 'automatic',
                zoomLevels: [1.0, 2.0],
                zoomFOV: [90, 50],
                recoil: {
                    xMin: -2.5,
                    xMax: 2.5,
                    yMin: 0.8,
                    yMax: 2.0,
                    magnitude: 1.3
                },
                spread: {
                    standing: 0.02,
                    moving: 0.12,
                    jumping: 0.4,
                    scoped: 0.01
                },
                reloadTime: 2.9,
                price: 5000,
                killAward: 300,
                teamRestricted: TEAM.COUNTER_TERRORIST
            },
            
            'weapon_m249': {
                name: 'M249',
                type: WEAPON_TYPE.HEAVY,
                category: 'heavy',
                damage: 32,
                headshotMultiplier: 4.0,
                armorPenetration: 0.85,
                magazineSize: 100,
                reserveAmmo: 200,
                fireRate: 750,
                fireMode: 'automatic',
                recoil: {
                    xMin: -3.0,
                    xMax: 3.0,
                    yMin: 1.0,
                    yMax: 2.5,
                    magnitude: 1.4
                },
                spread: {
                    standing: 0.045,
                    moving: 0.09,
                    jumping: 0.25
                },
                reloadTime: 4.7,
                price: 5200,
                killAward: 300
            },
            
            'weapon_negev': {
                name: 'Negev',
                type: WEAPON_TYPE.HEAVY,
                category: 'heavy',
                damage: 35,
                headshotMultiplier: 4.0,
                armorPenetration: 0.85,
                magazineSize: 150,
                reserveAmmo: 300,
                fireRate: 800,
                fireMode: 'automatic',
                recoil: {
                    xMin: -3.5,
                    xMax: 3.5,
                    yMin: 1.2,
                    yMax: 3.0,
                    magnitude: 1.5
                },
                spread: {
                    standing: 0.05,
                    moving: 0.1,
                    jumping: 0.28
                },
                reloadTime: 5.0,
                price: 5700,
                killAward: 300
            },
            
            'weapon_mp9': {
                name: 'MP9',
                type: WEAPON_TYPE.SMG,
                category: 'smg',
                damage: 26,
                headshotMultiplier: 4.0,
                armorPenetration: 0.7,
                magazineSize: 30,
                reserveAmmo: 120,
                fireRate: 750,
                fireMode: 'automatic',
                recoil: {
                    xMin: -1.5,
                    xMax: 1.5,
                    yMin: 0.4,
                    yMax: 1.0,
                    magnitude: 0.7
                },
                spread: {
                    standing: 0.035,
                    moving: 0.07,
                    jumping: 0.2
                },
                reloadTime: 1.8,
                price: 1250,
                killAward: 300,
                teamRestricted: TEAM.COUNTER_TERRORIST
            },
            
            'weapon_mac10': {
                name: 'MAC-10',
                type: WEAPON_TYPE.SMG,
                category: 'smg',
                damage: 27,
                headshotMultiplier: 4.0,
                armorPenetration: 0.7,
                magazineSize: 30,
                reserveAmmo: 120,
                fireRate: 750,
                fireMode: 'automatic',
                recoil: {
                    xMin: -1.5,
                    xMax: 1.5,
                    yMin: 0.4,
                    yMax: 1.0,
                    magnitude: 0.7
                },
                spread: {
                    standing: 0.038,
                    moving: 0.075,
                    jumping: 0.22
                },
                reloadTime: 1.7,
                price: 1050,
                killAward: 300,
                teamRestricted: TEAM.TERRORIST
            },
            
            'weapon_ump45': {
                name: 'UMP-45',
                type: WEAPON_TYPE.SMG,
                category: 'smg',
                damage: 30,
                headshotMultiplier: 4.0,
                armorPenetration: 0.75,
                magazineSize: 25,
                reserveAmmo: 100,
                fireRate: 500,
                fireMode: 'automatic',
                recoil: {
                    xMin: -2.0,
                    xMax: 2.0,
                    yMin: 0.5,
                    yMax: 1.3,
                    magnitude: 0.85
                },
                spread: {
                    standing: 0.032,
                    moving: 0.065,
                    jumping: 0.19
                },
                reloadTime: 2.0,
                price: 1200,
                killAward: 300
            },
            
            'weapon_bizon': {
                name: 'PP-Bizon',
                type: WEAPON_TYPE.SMG,
                category: 'smg',
                damage: 27,
                headshotMultiplier: 4.0,
                armorPenetration: 0.7,
                magazineSize: 64,
                reserveAmmo: 120,
                fireRate: 700,
                fireMode: 'automatic',
                recoil: {
                    xMin: -1.5,
                    xMax: 1.5,
                    yMin: 0.4,
                    yMax: 1.0,
                    magnitude: 0.7
                },
                spread: {
                    standing: 0.038,
                    moving: 0.075,
                    jumping: 0.22
                },
                reloadTime: 2.1,
                price: 1400,
                killAward: 300
            },
            
            'weapon_mp7': {
                name: 'MP7',
                type: WEAPON_TYPE.SMG,
                category: 'smg',
                damage: 28,
                headshotMultiplier: 4.0,
                armorPenetration: 0.75,
                magazineSize: 30,
                reserveAmmo: 120,
                fireRate: 700,
                fireMode: 'automatic',
                recoil: {
                    xMin: -1.5,
                    xMax: 1.5,
                    yMin: 0.4,
                    yMax: 1.0,
                    magnitude: 0.7
                },
                spread: {
                    standing: 0.035,
                    moving: 0.07,
                    jumping: 0.2
                },
                reloadTime: 1.9,
                price: 1500,
                killAward: 300
            },
            
            'weapon_mp5sd': {
                name: 'MP5-SD',
                type: WEAPON_TYPE.SMG,
                category: 'smg',
                damage: 27,
                headshotMultiplier: 4.0,
                armorPenetration: 0.75,
                magazineSize: 30,
                reserveAmmo: 120,
                fireRate: 600,
                fireMode: 'automatic',
                recoil: {
                    xMin: -1.5,
                    xMax: 1.5,
                    yMin: 0.4,
                    yMax: 1.0,
                    magnitude: 0.65
                },
                spread: {
                    standing: 0.03,
                    moving: 0.06,
                    jumping: 0.18
                },
                reloadTime: 1.9,
                price: 1700,
                killAward: 300,
                teamRestricted: TEAM.COUNTER_TERRORIST
            },
            
            'weapon_nova': {
                name: 'Nova',
                type: WEAPON_TYPE.SHOTGUN,
                category: 'shotgun',
                damage: 26,
                headshotMultiplier: 4.0,
                armorPenetration: 0.6,
                magazineSize: 8,
                reserveAmmo: 32,
                fireRate: 80,
                fireMode: 'pump',
                pelletCount: 8,
                recoil: {
                    xMin: -4.0,
                    xMax: 4.0,
                    yMin: 2.0,
                    yMax: 5.0,
                    magnitude: 2.0
                },
                spread: {
                    standing: 0.15,
                    moving: 0.25,
                    jumping: 0.5
                },
                reloadTime: 2.5,
                price: 1050,
                killAward: 300
            },
            
            'weapon_xm1014': {
                name: 'XM1014',
                type: WEAPON_TYPE.SHOTGUN,
                category: 'shotgun',
                damage: 24,
                headshotMultiplier: 4.0,
                armorPenetration: 0.6,
                magazineSize: 4,
                reserveAmmo: 24,
                fireRate: 200,
                fireMode: 'automatic',
                pelletCount: 6,
                recoil: {
                    xMin: -3.0,
                    xMax: 3.0,
                    yMin: 1.5,
                    yMax: 4.0,
                    magnitude: 1.8
                },
                spread: {
                    standing: 0.12,
                    moving: 0.22,
                    jumping: 0.45
                },
                reloadTime: 3.0,
                price: 1700,
                killAward: 300
            },
            
            'weapon_mag7': {
                name: 'MAG-7',
                type: WEAPON_TYPE.SHOTGUN,
                category: 'shotgun',
                damage: 32,
                headshotMultiplier: 4.0,
                armorPenetration: 0.6,
                magazineSize: 5,
                reserveAmmo: 28,
                fireRate: 70,
                fireMode: 'pump',
                pelletCount: 8,
                recoil: {
                    xMin: -4.0,
                    xMax: 4.0,
                    yMin: 2.5,
                    yMax: 5.5,
                    magnitude: 2.2
                },
                spread: {
                    standing: 0.15,
                    moving: 0.25,
                    jumping: 0.5
                },
                reloadTime: 2.4,
                price: 1300,
                killAward: 300,
                teamRestricted: TEAM.COUNTER_TERRORIST
            },
            
            'weapon_sawedoff': {
                name: 'Sawed-Off',
                type: WEAPON_TYPE.SHOTGUN,
                category: 'shotgun',
                damage: 36,
                headshotMultiplier: 4.0,
                armorPenetration: 0.6,
                magazineSize: 2,
                reserveAmmo: 20,
                fireRate: 60,
                fireMode: 'pump',
                pelletCount: 8,
                recoil: {
                    xMin: -5.0,
                    xMax: 5.0,
                    yMin: 3.0,
                    yMax: 6.0,
                    magnitude: 2.5
                },
                spread: {
                    standing: 0.2,
                    moving: 0.35,
                    jumping: 0.6
                },
                reloadTime: 2.2,
                price: 1100,
                killAward: 300,
                teamRestricted: TEAM.TERRORIST
            },
            
            'weapon_tec9': {
                name: 'Tec-9',
                type: WEAPON_TYPE.PISTOL,
                category: 'pistol',
                damage: 32,
                headshotMultiplier: 4.0,
                armorPenetration: 0.8,
                magazineSize: 18,
                reserveAmmo: 120,
                fireRate: 500,
                fireMode: 'automatic',
                recoil: {
                    xMin: -2.5,
                    xMax: 2.5,
                    yMin: 0.6,
                    yMax: 1.5,
                    magnitude: 0.9
                },
                spread: {
                    standing: 0.035,
                    moving: 0.07,
                    jumping: 0.2
                },
                reloadTime: 2.0,
                price: 500,
                killAward: 300,
                teamRestricted: TEAM.TERRORIST
            },
            
            'weapon_cz75a': {
                name: 'CZ75-Auto',
                type: WEAPON_TYPE.PISTOL,
                category: 'pistol',
                damage: 31,
                headshotMultiplier: 4.0,
                armorPenetration: 0.8,
                magazineSize: 12,
                reserveAmmo: 108,
                fireRate: 450,
                fireMode: 'automatic',
                recoil: {
                    xMin: -2.0,
                    xMax: 2.0,
                    yMin: 0.5,
                    yMax: 1.3,
                    magnitude: 0.85
                },
                spread: {
                    standing: 0.03,
                    moving: 0.06,
                    jumping: 0.18
                },
                reloadTime: 2.1,
                price: 500,
                killAward: 300
            },
            
            'weapon_r8': {
                name: 'R8 Revolver',
                type: WEAPON_TYPE.PISTOL,
                category: 'pistol',
                damage: 68,
                headshotMultiplier: 4.0,
                armorPenetration: 0.95,
                magazineSize: 8,
                reserveAmmo: 80,
                fireRate: 60,
                fireMode: 'double-action',
                recoil: {
                    xMin: -4.0,
                    xMax: 4.0,
                    yMin: 1.5,
                    yMax: 4.0,
                    magnitude: 1.8
                },
                spread: {
                    standing: 0.025,
                    moving: 0.08,
                    jumping: 0.25
                },
                reloadTime: 2.2,
                price: 600,
                killAward: 300
            },
            
            'weapon_hegrenade': {
                name: 'HE Grenade',
                type: WEAPON_TYPE.GRENADE,
                category: 'grenade',
                damage: 98,
                radius: 350,
                price: 300,
                fireMode: 'throwable'
            },
            
            'weapon_flashbang': {
                name: 'Flashbang',
                type: WEAPON_TYPE.GRENADE,
                category: 'grenade',
                flashDuration: 1500,
                flashIntensity: 1.0,
                price: 200,
                fireMode: 'throwable'
            },
            
            'weapon_smoke': {
                name: 'Smoke Grenade',
                type: WEAPON_TYPE.GRENADE,
                category: 'grenade',
                smokeDuration: 17000,
                smokeRadius: 250,
                price: 200,
                fireMode: 'throwable'
            },
            
            'weapon_molotov': {
                name: 'Molotov',
                type: WEAPON_TYPE.GRENADE,
                category: 'grenade',
                damage: 60,
                fireDuration: 7000,
                fireRadius: 200,
                price: 400,
                fireMode: 'throwable',
                teamRestricted: TEAM.TERRORIST
            },
            
            'weapon_incendiary': {
                name: 'Incendiary Grenade',
                type: WEAPON_TYPE.GRENADE,
                category: 'grenade',
                damage: 60,
                fireDuration: 7000,
                fireRadius: 200,
                price: 400,
                fireMode: 'throwable',
                teamRestricted: TEAM.COUNTER_TERRORIST
            },
            
            'weapon_decoy': {
                name: 'Decoy Grenade',
                type: WEAPON_TYPE.GRENADE,
                category: 'grenade',
                duration: 5000,
                price: 50,
                fireMode: 'throwable'
            },
            
            'weapon_zeus_x27': {
                name: 'Zeus x27',
                type: WEAPON_TYPE.OTHER,
                category: 'equipment',
                damage: 500,
                range: 150,
                price: 200,
                killAward: 150
            },
            
            'weapon_kevlar': {
                name: 'Kevlar Vest',
                type: WEAPON_TYPE.ARMOR,
                category: 'armor',
                armorValue: 100,
                price: 650
            },
            
            'weapon_helmet': {
                name: 'Helmet',
                type: WEAPON_TYPE.ARMOR,
                category: 'armor',
                armorValue: 100,
                helmet: true,
                price: 350
            },
            
            'weapon_defusekit': {
                name: 'Defuse Kit',
                type: WEAPON_TYPE.OTHER,
                category: 'equipment',
                price: 400,
                kitRequired: true
            },
            
            'weapon_c4': {
                name: 'C4 Explosive',
                type: WEAPON_TYPE.EQUIPMENT,
                category: 'bomb',
                damage: 98,
                radius: 500,
                plantTime: 3.0,
                defuseTimeRegular: 5.0,
                defuseTimeKit: 2.5,
                timer: 40,
                price: 0,
                teamRestricted: TEAM.TERRORIST
            }
        };
        
        return weapons[weaponId] || null;
    }
    
    static getAllWeaponIds() {
        return [
            'weapon_knife',
            'weapon_glock',
            'weapon_usp',
            'weapon_p250',
            'weapon_deagle',
            'weapon_ak47',
            'weapon_m4a4',
            'weapon_m4a1',
            'weapon_awp',
            'weapon_sg553',
            'weapon_aug',
            'weapon_famas',
            'weapon_galilar',
            'weapon_ssg08',
            'weapon_scar20',
            'weapon_g3sg1',
            'weapon_m249',
            'weapon_negev',
            'weapon_mp9',
            'weapon_mac10',
            'weapon_ump45',
            'weapon_bizon',
            'weapon_mp7',
            'weapon_mp5sd',
            'weapon_nova',
            'weapon_xm1014',
            'weapon_mag7',
            'weapon_sawedoff',
            'weapon_tec9',
            'weapon_cz75a',
            'weapon_r8',
            'weapon_hegrenade',
            'weapon_flashbang',
            'weapon_smoke',
            'weapon_molotov',
            'weapon_incendiary',
            'weapon_decoy',
            'weapon_zeus_x27',
            'weapon_kevlar',
            'weapon_helmet',
            'weapon_defusekit',
            'weapon_c4'
        ];
    }
    
    static getWeaponsByCategory(category) {
        const weapons = this.getAllWeaponIds();
        return weapons.filter(id => {
            const data = this.getWeaponData(id);
            return data && data.category === category;
        });
    }
    
    static getWeaponsByType(type) {
        const weapons = this.getAllWeaponIds();
        return weapons.filter(id => {
            const data = this.getWeaponData(id);
            return data && data.type === type;
        });
    }
    
    static getBuyableWeapons() {
        return this.getAllWeaponIds().filter(id => {
            const data = this.getWeaponData(id);
            return data && data.price > 0 && data.category !== 'armor' && data.category !== 'equipment';
        });
    }
}

export class WeaponManager {
    constructor() {
        this.weapons = new Map();
        this.equippedWeapon = null;
        this.weaponSlots = new Array(5).fill(null);
    }
    
    addWeapon(weapon) {
        this.weapons.set(weapon.id, weapon);
    }
    
    removeWeapon(weaponId) {
        this.weapons.delete(weaponId);
    }
    
    getWeapon(weaponId) {
        return this.weapons.get(weaponId);
    }
    
    getEquippedWeapon() {
        return this.equippedWeapon;
    }
    
    equipWeapon(weaponId) {
        const weapon = this.weapons.get(weaponId);
        if (!weapon) return false;
        
        if (this.equippedWeapon) {
            this.equippedWeapon.holster();
        }
        
        weapon.equip();
        this.equippedWeapon = weapon;
        
        return true;
    }
    
    unequipWeapon() {
        if (!this.equippedWeapon) return false;
        
        this.equippedWeapon.holster();
        this.equippedWeapon = null;
        
        return true;
    }
    
    fire() {
        if (!this.equippedWeapon) return null;
        return this.equippedWeapon.fire();
    }
    
    reload() {
        if (!this.equippedWeapon) return false;
        return this.equippedWeapon.reload();
    }
    
    addToSlot(slot, weaponId) {
        if (slot < 0 || slot >= this.weaponSlots.length) return false;
        
        const weapon = WeaponFactory.createWeapon(weaponId);
        if (!weapon) return false;
        
        this.weaponSlots[slot] = weapon;
        this.weapons.set(weaponId, weapon);
        
        return true;
    }
    
    getFromSlot(slot) {
        if (slot < 0 || slot >= this.weaponSlots.length) return null;
        return this.weaponSlots[slot];
    }
    
    cycleWeapons() {
        const currentIndex = this.weaponSlots.indexOf(this.equippedWeapon);
        let nextIndex = (currentIndex + 1) % this.weaponSlots.length;
        
        while (nextIndex !== currentIndex) {
            if (this.weaponSlots[nextIndex]) {
                this.equipWeapon(this.weaponSlots[nextIndex].id);
                return true;
            }
            nextIndex = (nextIndex + 1) % this.weaponSlots.length;
        }
        
        return false;
    }
    
    getAllWeapons() {
        return Array.from(this.weapons.values());
    }
    
    getTotalStatistics() {
        const stats = {
            shotsFired: 0,
            shotsHit: 0,
            kills: 0,
            headshots: 0,
            damageDealt: 0
        };
        
        for (const weapon of this.weapons.values()) {
            const weaponStats = weapon.getStatistics();
            stats.shotsFired += weaponStats.shotsFired;
            stats.shotsHit += weaponStats.shotsHit;
            stats.kills += weaponStats.kills;
            stats.headshots += weaponStats.headshots;
            stats.damageDealt += weaponStats.damageDealt;
        }
        
        return stats;
    }
    
    reset() {
        for (const weapon of this.weapons.values()) {
            weapon.reset();
        }
    }
    
    clear() {
        this.weapons.clear();
        this.equippedWeapon = null;
        this.weaponSlots = new Array(5).fill(null);
    }
}

export class RecoilPattern {
    constructor() {
        this.patterns = new Map();
        this.initializePatterns();
    }
    
    initializePatterns() {
        this.patterns.set('ak47', [
            { x: 0, y: 0 },
            { x: -2.5, y: 4.0 },
            { x: 3.5, y: 6.5 },
            { x: -1.0, y: 8.5 },
            { x: 2.0, y: 10.0 },
            { x: -2.5, y: 11.0 },
            { x: 1.5, y: 12.5 },
            { x: -1.5, y: 13.0 },
            { x: 1.0, y: 14.0 },
            { x: -1.0, y: 14.5 }
        ]);
        
        this.patterns.set('m4a4', [
            { x: 0, y: 0 },
            { x: 1.5, y: 3.0 },
            { x: -1.5, y: 5.5 },
            { x: 1.0, y: 7.5 },
            { x: -0.5, y: 9.0 },
            { x: 0.5, y: 10.5 },
            { x: -0.5, y: 11.5 },
            { x: 0.5, y: 12.5 },
            { x: -0.5, y: 13.5 }
        ]);
        
        this.patterns.set('m4a1', [
            { x: 0, y: 0 },
            { x: 1.0, y: 2.5 },
            { x: -1.0, y: 4.5 },
            { x: 0.5, y: 6.0 },
            { x: -0.5, y: 7.5 },
            { x: 0.5, y: 8.5 },
            { x: -0.5, y: 9.5 }
        ]);
        
        this.patterns.set('awp', [
            { x: 0, y: 0 },
            { x: -3.0, y: 5.0 },
            { x: 3.0, y: 9.0 },
            { x: -2.0, y: 12.0 },
            { x: 2.0, y: 14.0 }
        ]);
        
        this.patterns.set('glock', [
            { x: 0, y: 0 },
            { x: 1.5, y: 2.5 },
            { x: -1.5, y: 4.5 },
            { x: 1.5, y: 6.0 },
            { x: -1.5, y: 7.5 },
            { x: 1.0, y: 8.5 },
            { x: -1.0, y: 9.5 }
        ]);
        
        this.patterns.set('deagle', [
            { x: 0, y: 0 },
            { x: -2.0, y: 4.0 },
            { x: 2.5, y: 7.5 },
            { x: -1.5, y: 10.0 }
        ]);
    }
    
    getPattern(weaponId) {
        const baseWeaponId = weaponId.replace('weapon_', '');
        return this.patterns.get(baseWeaponId) || [];
    }
    
    applyRecoil(weaponId, shotIndex) {
        const pattern = this.getPattern(weaponId);
        if (pattern.length === 0) return { x: 0, y: 0 };
        
        const index = Math.min(shotIndex, pattern.length - 1);
        return pattern[index];
    }
}

export class WeaponSkin {
    constructor(skinId, weaponId, wear, pattern) {
        this.id = skinId;
        this.weaponId = weaponId;
        this.wear = wear;
        this.pattern = pattern;
        
        this.name = '';
        this.rarity = 'common';
        this.price = 0;
        
        this.float = 0;
        this.condition = 'Factory New';
    }
    
    setWear(wear) {
        this.wear = wear;
        this.float = wear;
        
        if (wear < 0.07) {
            this.condition = 'Factory New';
        } else if (wear < 0.15) {
            this.condition = 'Minimal Wear';
        } else if (wear < 0.38) {
            this.condition = 'Field-Tested';
        } else if (wear < 0.45) {
            this.condition = 'Well-Worn';
        } else {
            this.condition = 'Battle-Scarred';
        }
    }
}

export class WeaponSoundManager {
    constructor() {
        this.sounds = new Map();
        this.volume = 1.0;
        this.distanceModel = 'inverse';
        this.initializeSounds();
    }
    
    initializeSounds() {
        this.sounds.set('default_shot', {
            filename: 'shot_default.wav',
            volume: 1.0,
            pitch: [0.95, 1.05],
            delay: 0
        });
        
        this.sounds.set('default_reload', {
            filename: 'reload_default.wav',
            volume: 0.8,
            pitch: [0.98, 1.02],
            delay: 0
        });
        
        this.sounds.set('default_draw', {
            filename: 'draw_default.wav',
            volume: 0.7,
            pitch: [0.98, 1.02],
            delay: 0
        });
        
        this.sounds.set('default_empty', {
            filename: 'empty_default.wav',
            volume: 0.6,
            pitch: [1.0, 1.0],
            delay: 0
        });
    }
    
    playSound(soundId, position = null, listenerPosition = null) {
        const sound = this.sounds.get(soundId);
        if (!sound) return null;
        
        let finalVolume = sound.volume * this.volume;
        
        if (position && listenerPosition) {
            const distance = this.calculateDistance(position, listenerPosition);
            finalVolume *= this.calculateAttenuation(distance);
        }
        
        return {
            soundId: soundId,
            volume: finalVolume,
            pitch: sound.pitch[0] + Math.random() * (sound.pitch[1] - sound.pitch[0])
        };
    }
    
    calculateDistance(pos1, pos2) {
        const dx = pos1.x - pos2.x;
        const dy = pos1.y - pos2.y;
        const dz = pos1.z - pos2.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }
    
    calculateAttenuation(distance) {
        if (this.distanceModel === 'inverse') {
            return Math.max(0, 1 / (1 + distance * 0.01));
        } else if (this.distanceModel === 'linear') {
            return Math.max(0, 1 - distance / 1000);
        } else {
            return Math.max(0, 1 / (1 + distance * distance * 0.00001));
        }
    }
    
    setVolume(volume) {
        this.volume = Math.max(0, Math.min(1, volume));
    }
    
    addSound(soundId, soundData) {
        this.sounds.set(soundId, soundData);
    }
}

export class WeaponAnimationController {
    constructor() {
        this.animations = new Map();
        this.currentAnimation = null;
        this.animationTime = 0;
        this.blendTime = 0.2;
        this.initializeAnimations();
    }
    
    initializeAnimations() {
        this.animations.set('idle', {
            duration: 2.0,
            loop: true,
            events: []
        });
        
        this.animations.set('fire', {
            duration: 0.1,
            loop: false,
            events: [
                { time: 0, event: 'muzzle_flash' },
                { time: 0.05, event: 'recoil' }
            ]
        });
        
        this.animations.set('fire_ads', {
            duration: 0.08,
            loop: false,
            events: [
                { time: 0, event: 'muzzle_flash' },
                { time: 0.04, event: 'recoil' }
            ]
        });
        
        this.animations.set('reload', {
            duration: 2.0,
            loop: false,
            events: [
                { time: 0.5, event: 'magazine_out' },
                { time: 1.2, event: 'magazine_in' }
            ]
        });
        
        this.animations.set('draw', {
            duration: 0.5,
            loop: false,
            events: [
                { time: 0.3, event: 'weapon_ready' }
            ]
        });
        
        this.animations.set('holster', {
            duration: 0.5,
            loop: false,
            events: []
        });
        
        this.animations.set('empty', {
            duration: 0.3,
            loop: false,
            events: [
                { time: 0.1, event: 'empty_click' }
            ]
        });
    }
    
    playAnimation(animationName, blendTime = 0.2) {
        const animation = this.animations.get(animationName);
        if (!animation) return false;
        
        this.currentAnimation = animationName;
        this.animationTime = 0;
        this.blendTime = blendTime;
        
        return true;
    }
    
    update(deltaTime) {
        if (!this.currentAnimation) return;
        
        const animation = this.animations.get(this.currentAnimation);
        if (!animation) return;
        
        this.animationTime += deltaTime;
        
        if (this.animationTime >= animation.duration) {
            if (animation.loop) {
                this.animationTime = 0;
            } else {
                this.playAnimation('idle');
            }
        }
        
        const events = this.getEventsAtTime(this.animationTime);
        return events;
    }
    
    getEventsAtTime(time) {
        const animation = this.animations.get(this.currentAnimation);
        if (!animation) return [];
        
        return animation.events.filter(event => {
            return Math.abs(event.time - time) < 0.05;
        });
    }
    
    getAnimationProgress() {
        const animation = this.animations.get(this.currentAnimation);
        if (!animation) return 0;
        
        return this.animationTime / animation.duration;
    }
    
    getCurrentBoneTransforms() {
        const progress = this.getAnimationProgress();
        
        return {
            position: { x: 0, y: 0, z: 0 },
            rotation: { x: 0, y: 0, z: 0 },
            scale: { x: 1, y: 1, z: 1 },
            progress: progress
        };
    }
}
