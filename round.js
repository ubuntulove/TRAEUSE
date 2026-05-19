export class WeatherSystem {
    constructor() {
        this.currentWeather = null;
        this.weatherTypes = new Map();
        this.transitions = new Map();
        this.intensity = 1.0;
        this.particles = [];
        this.maxParticles = 10000;
        this.fogSettings = {
            enabled: false,
            start: 0,
            end: 1000,
            color: { r: 192, g: 192, b: 192 },
            density: 0.5
        };
        this.skySettings = {
            skybox: 'default',
            ambientLight: { r: 1, g: 1, b: 1 },
            directionalLight: { r: 1, g: 1, b: 1 },
            sunPosition: { x: 0, y: 0, z: 0 },
            sunAngle: 0
        };
        this.timeSettings = {
            enabled: false,
            timeOfDay: 12,
            timeScale: 0
        };
        this.registerWeatherTypes();
    }

    registerWeatherTypes() {
        this.weatherTypes.set('clear', {
            name: 'Clear',
            particleType: null,
            fog: false,
            wind: 0,
            visibility: 100
        });

        this.weatherTypes.set('rain', {
            name: 'Rain',
            particleType: 'rain',
            fog: true,
            wind: 5,
            visibility: 80
        });

        this.weatherTypes.set('heavy_rain', {
            name: 'Heavy Rain',
            particleType: 'rain_heavy',
            fog: true,
            wind: 15,
            visibility: 50
        });

        this.weatherTypes.set('snow', {
            name: 'Snow',
            particleType: 'snow',
            fog: true,
            wind: 2,
            visibility: 60
        });

        this.weatherTypes.set('fog', {
            name: 'Fog',
            particleType: null,
            fog: true,
            wind: 0,
            visibility: 30
        });

        this.weatherTypes.set('dust', {
            name: 'Dust',
            particleType: 'dust',
            fog: true,
            wind: 10,
            visibility: 70
        });
    }

    setWeather(type, duration = 0) {
        const weather = this.weatherTypes.get(type);
        if (!weather) return false;

        this.currentWeather = {
            type,
            weather,
            startTime: Date.now(),
            duration,
            intensity: 1.0
        };

        if (weather.fog) {
            this.fogSettings.enabled = true;
        } else {
            this.fogSettings.enabled = false;
        }

        return true;
    }

    setIntensity(intensity) {
        this.intensity = Math.max(0, Math.min(1, intensity));
        if (this.currentWeather) {
            this.currentWeather.intensity = intensity;
        }
    }

    update(deltaTime) {
        if (!this.currentWeather) return;

        for (let i = this.particles.length - 1; i >= 0; i--) {
            this.updateParticle(this.particles[i], deltaTime);

            if (this.particles[i].lifetime <= 0) {
                this.particles.splice(i, 1);
            }
        }

        if (this.particles.length < this.maxParticles && this.currentWeather.weather.particleType) {
            this.spawnParticle();
        }
    }

    spawnParticle() {
        const particle = {
            type: this.currentWeather.weather.particleType,
            position: {
                x: Math.random() * 2000 - 1000,
                y: Math.random() * 2000 - 1000,
                z: 500 + Math.random() * 200
            },
            velocity: {
                x: (Math.random() - 0.5) * 2,
                y: -10 - Math.random() * 10,
                z: -1 - Math.random() * 2
            },
            lifetime: 5 + Math.random() * 5,
            size: 1 + Math.random() * 2,
            opacity: 0.5 + Math.random() * 0.5
        };

        const weather = this.currentWeather.weather;
        if (weather.wind > 0) {
            particle.velocity.x += weather.wind;
        }

        this.particles.push(particle);
    }

    updateParticle(particle, deltaTime) {
        particle.position.x += particle.velocity.x * deltaTime * 10;
        particle.position.y += particle.velocity.y * deltaTime * 10;
        particle.position.z += particle.velocity.z * deltaTime * 10;
        particle.lifetime -= deltaTime;
        particle.opacity = Math.max(0, particle.opacity - deltaTime * 0.1);
    }

    setFog(start, end, color, density = 0.5) {
        this.fogSettings.enabled = true;
        this.fogSettings.start = start;
        this.fogSettings.end = end;
        this.fogSettings.color = color;
        this.fogSettings.density = density;
    }

    disableFog() {
        this.fogSettings.enabled = false;
    }

    setSkybox(name) {
        this.skySettings.skybox = name;
    }

    setTimeOfDay(hour) {
        this.timeSettings.enabled = true;
        this.timeSettings.timeOfDay = hour;

        const t = hour / 24;
        this.skySettings.ambientLight = {
            r: 0.3 + Math.sin(t * Math.PI * 2) * 0.2,
            g: 0.3 + Math.sin(t * Math.PI * 2) * 0.2,
            b: 0.4 + Math.sin(t * Math.PI * 2) * 0.2
        };

        const angle = t * Math.PI * 2 - Math.PI / 2;
        this.skySettings.sunPosition = {
            x: Math.cos(angle) * 1000,
            y: Math.sin(angle) * 1000,
            z: Math.sin(angle) * 500
        };
        this.skySettings.sunAngle = angle;
    }

    getWeatherInfo() {
        return {
            current: this.currentWeather?.type || 'none',
            intensity: this.intensity,
            fog: this.fogSettings,
            sky: this.skySettings,
            time: this.timeSettings,
            particleCount: this.particles.length
        };
    }

    serialize() {
        return this.getWeatherInfo();
    }
}

export class DamageSystem {
    constructor() {
        this.damageTypes = this.initializeDamageTypes();
        this.hitgroups = [
            'generic', 'head', 'chest', 'stomach', 'leftArm', 'rightArm',
            'leftLeg', 'rightLeg', 'neck', 'gear', 'shield'
        ];
        this.damageMultipliers = this.initializeMultipliers();
        this.armorProtection = 0.8;
        this.headshotMultiplier = 4.0;
        this.legDamageMultiplier = 0.75;
    }

    initializeDamageTypes() {
        return {
            bullet: { name: 'Bullet', penetration: 1.0 },
            knife: { name: 'Knife', penetration: 1.0 },
            grenade: { name: 'Grenade', penetration: 1.0 },
            explosive: { name: 'Explosive', penetration: 1.0 },
            fire: { name: 'Fire', penetration: 0 },
            fall: { name: 'Fall', penetration: 0 },
            melee: { name: 'Melee', penetration: 1.0 },
            flashbang: { name: 'Flashbang', penetration: 0 },
            smoke: { name: 'Smoke', penetration: 0 },
            environment: { name: 'Environment', penetration: 0 },
            collision: { name: 'Collision', penetration: 0 }
        };
    }

    initializeMultipliers() {
        const multipliers = {};
        for (const hitgroup of this.hitgroups) {
            multipliers[hitgroup] = 1.0;
        }
        multipliers.head = this.headshotMultiplier;
        multipliers.chest = 1.0;
        multipliers.stomach = 1.25;
        multipliers.leftArm = 1.0;
        multipliers.rightArm = 1.0;
        multipliers.leftLeg = this.legDamageMultiplier;
        multipliers.rightLeg = this.legDamageMultiplier;
        multipliers.neck = 1.5;
        multipliers.gear = 0.5;
        return multipliers;
    }

    calculateDamage(baseDamage, hitgroup, armor, distance = 0) {
        let damage = baseDamage;
        const multiplier = this.damageMultipliers[hitgroup] || 1.0;
        damage *= multiplier;

        if (distance > 500) {
            const falloff = Math.max(0.5, 1 - (distance - 500) / 3500);
            damage *= falloff;
        }

        if (armor > 0 && hitgroup !== 'head') {
            const armorDamage = damage * (1 - this.armorProtection);
            damage = armorDamage;
        }

        return Math.round(damage);
    }

    applyDamage(target, damage, hitgroup, attacker, weapon) {
        const result = {
            damage,
            hitgroup,
            armorDamage: 0,
            healthDamage: 0,
            killed: false,
            armorPenetrated: false
        };

        if (target.armor > 0 && hitgroup !== 'head') {
            const armorDamage = damage * (1 - this.armorProtection);
            target.armor = Math.max(0, target.armor - armorDamage);
            result.armorDamage = armorDamage;
        }

        result.healthDamage = damage;
        target.health = Math.max(0, target.health - damage);

        if (target.health <= 0) {
            result.killed = true;
        }

        return result;
    }

    getDamageType(type) {
        return this.damageTypes[type] || this.damageTypes.bullet;
    }

    serialize() {
        return {
            damageTypes: this.damageTypes,
            hitgroups: this.hitgroups,
            multipliers: this.damageMultipliers,
            armorProtection: this.armorProtection,
            headshotMultiplier: this.headshotMultiplier
        };
    }
}

export class SpawnSystem {
    constructor() {
        this.spawnPoints = new Map();
        this.spawnZones = new Map();
        this.respawnPoints = new Map();
        this.safeZones = new Map();
        this.randomSpawn = true;
        this.spawnProtection = true;
        this.spawnProtectionDuration = 3000;
        this.minSpawnDistance = 200;
        this.maxSpawnAttempts = 10;
    }

    addSpawnPoint(team, type, position, rotation = 0) {
        const key = `${team}_${type}`;
        if (!this.spawnPoints.has(key)) {
            this.spawnPoints.set(key, []);
        }

        const spawn = {
            position: { ...position },
            rotation,
            type,
            team,
            isOccupied: false,
            lastUsed: 0
        };

        this.spawnPoints.get(key).push(spawn);
        return spawn;
    }

    getSpawnPoint(team, type = 'default') {
        const key = `${team}_${type}`;
        const points = this.spawnPoints.get(key);

        if (!points || points.length === 0) {
            return null;
        }

        if (this.randomSpawn) {
            const available = points.filter(p => !p.isOccupied);
            if (available.length > 0) {
                const spawn = available[Math.floor(Math.random() * available.length)];
                spawn.isOccupied = true;
                spawn.lastUsed = Date.now();
                return spawn;
            }
        }

        const spawn = points.find(p => !p.isOccupied);
        if (spawn) {
            spawn.isOccupied = true;
            spawn.lastUsed = Date.now();
        }

        return spawn || points[0];
    }

    releaseSpawnPoint(team, type, position) {
        const key = `${team}_${type}`;
        const points = this.spawnPoints.get(key);

        if (!points) return;

        for (const spawn of points) {
            const dx = spawn.position.x - position.x;
            const dy = spawn.position.y - position.y;
            const dz = spawn.position.z - position.z;
            const dist = Math.sqrt(dx * dx + dy * dy + dz * dz);

            if (dist < 50) {
                spawn.isOccupied = false;
            }
        }
    }

    releaseTeamSpawns(team) {
        const keys = Array.from(this.spawnPoints.keys()).filter(k => k.startsWith(team));
        for (const key of keys) {
            const points = this.spawnPoints.get(key);
            if (points) {
                for (const spawn of points) {
                    spawn.isOccupied = false;
                }
            }
        }
    }

    addSpawnZone(id, bounds, team, type = 'default') {
        const zone = {
            id,
            bounds,
            team,
            type,
            capacity: bounds.width * bounds.height / 10000,
            currentCount: 0
        };
        this.spawnZones.set(id, zone);
        return zone;
    }

    getSpawnZone(id) {
        return this.spawnZones.get(id);
    }

    addRespawnPoint(team, position, priority = 0) {
        const key = `respawn_${team}`;
        if (!this.respawnPoints.has(key)) {
            this.respawnPoints.set(key, []);
        }

        const point = {
            position: { ...position },
            priority,
            lastUsed: 0,
            cooldown: 5000
        };

        this.respawnPoints.get(key).push(point);
        return point;
    }

    getRespawnPoint(team, avoidPositions = []) {
        const key = `respawn_${team}`;
        const points = this.respawnPoints.get(key);

        if (!points || points.length === 0) {
            return this.getSpawnPoint(team);
        }

        const now = Date.now();
        const available = points.filter(p =>
            now - p.lastUsed > p.cooldown &&
            !this.isPositionNearAny(avoidPositions, p.position, this.minSpawnDistance)
        );

        if (available.length === 0) {
            return points.reduce((best, p) =>
                now - p.lastUsed > now - best.lastUsed ? p : best
            , points[0]);
        }

        available.sort((a, b) => b.priority - a.priority);
        const point = available[0];
        point.lastUsed = now;
        return point;
    }

    isPositionNearAny(positions, target, distance) {
        for (const pos of positions) {
            const dx = pos.x - target.x;
            const dy = pos.y - target.y;
            const dz = pos.z - target.z;
            if (Math.sqrt(dx * dx + dy * dy + dz * dz) < distance) {
                return true;
            }
        }
        return false;
    }

    addSafeZone(id, bounds) {
        const zone = {
            id,
            bounds,
            active: true
        };
        this.safeZones.set(id, zone);
        return zone;
    }

    isPositionSafe(position) {
        for (const zone of this.safeZones.values()) {
            if (!zone.active) continue;

            if (position.x >= zone.bounds.minX && position.x <= zone.bounds.maxX &&
                position.y >= zone.bounds.minY && position.y <= zone.bounds.maxY &&
                position.z >= zone.bounds.minZ && position.z <= zone.bounds.maxZ) {
                return true;
            }
        }
        return false;
    }

    getSpawnInfo() {
        const info = {};
        for (const [key, points] of this.spawnPoints) {
            info[key] = {
                total: points.length,
                occupied: points.filter(p => p.isOccupied).length
            };
        }
        return info;
    }

    serialize() {
        return {
            spawnPoints: this.getSpawnInfo(),
            spawnZones: this.spawnZones.size,
            safeZones: this.safeZones.size,
            randomSpawn: this.randomSpawn,
            spawnProtection: this.spawnProtection
        };
    }
}

export class EconomySimulator {
    constructor() {
        this.teamEconomies = {
            terrorist: {
                money: 800,
                equipmentValue: 0,
                spentThisRound: 0,
                lossBonus: 0,
                consecutiveLosses: 0,
                totalSpent: 0
            },
            counterTerrorist: {
                money: 800,
                equipmentValue: 0,
                spentThisRound: 0,
                lossBonus: 0,
                consecutiveLosses: 0,
                totalSpent: 0
            }
        };

        this.weaponPrices = this.initializePrices();
        this.equipmentPrices = {
            kevlar: 650,
            helmet: 350,
            defusekit: 400,
            flashgrenade: 200,
            smokegrenade: 300,
            hegrenade: 300,
            molotov: 400,
            incgrenade: 400,
            decoy: 50
        };

        this.roundWinRewards = {
            bombDefused: 3500,
            bombExploded: 3250,
            hostageRescued: 3000,
            elimination: 1500,
            timeExpired: 2000
        };

        this.lossBonus = {
            0: 1400,
            1: 1900,
            2: 2400,
            3: 2900,
            4: 3400
        };

        this.spawnMoney = 800;
        this.pistolRoundBonus = 800;
        this.maxMoney = 16000;
        this.ecoRoundThreshold = 2000;
    }

    initializePrices() {
        return {
            glock: 200,
            usp_silencer: 200,
            p250: 300,
            deagle: 700,
            elite: 400,
            fiveseven: 500,
            tec9: 500,
            nova: 1200,
            xm1014: 1700,
            mag7: 1700,
            sawedoff: 1200,
            mp9: 1250,
            mac10: 1050,
            mp7: 1500,
            mp5sd: 1500,
            ump45: 1200,
            p90: 2350,
            ppbizon: 1400,
            galilar: 2000,
            famas: 2050,
            ak47: 2700,
            m4a4: 3100,
            m4a1_silencer: 3200,
            ssg08: 1700,
            sg556: 3000,
            aug: 3300,
            awp: 4750,
            g3sg1: 5000,
            scar20: 5000,
            ssg08: 1700,
            m249: 5200,
            negev: 1700,
            knife: 0
        };
    }

    resetTeam(team) {
        this.teamEconomies[team] = {
            money: this.spawnMoney,
            equipmentValue: 0,
            spentThisRound: 0,
            lossBonus: 0,
            consecutiveLosses: 0,
            totalSpent: 0
        };
    }

    addMoney(team, amount) {
        const economy = this.teamEconomies[team];
        economy.money = Math.min(this.maxMoney, economy.money + amount);
    }

    spendMoney(team, amount) {
        const economy = this.teamEconomies[team];
        if (economy.money >= amount) {
            economy.money -= amount;
            economy.spentThisRound += amount;
            economy.totalSpent += amount;
            return true;
        }
        return false;
    }

    canAfford(team, amount) {
        return this.teamEconomies[team].money >= amount;
    }

    processBuyPhase(team) {
        const economy = this.teamEconomies[team];
        economy.spentThisRound = 0;
        economy.equipmentValue = 0;
    }

    processRoundEnd(winner, reason) {
        const loser = winner === 'terrorist' ? 'counterTerrorist' : 'terrorist';
        const winnerEconomy = this.teamEconomies[winner];
        const loserEconomy = this.teamEconomies[loser];

        loserEconomy.consecutiveLosses++;
        const lossBonusAmount = this.getLossBonus(loserEconomy.consecutiveLosses);

        switch (reason) {
            case 'bomb_defused':
                this.addMoney('counterTerrorist', this.roundWinRewards.bombDefused);
                loserEconomy.lossBonus = lossBonusAmount;
                break;
            case 'bomb_exploded':
                this.addMoney('terrorist', this.roundWinRewards.bombExploded);
                loserEconomy.lossBonus = lossBonusAmount;
                break;
            case 'elimination':
                this.addMoney(winner, this.roundWinRewards.elimination);
                loserEconomy.lossBonus = lossBonusAmount;
                break;
            case 'time_expired':
                this.addMoney('counterTerrorist', this.roundWinRewards.timeExpired);
                loserEconomy.consecutiveLosses = 0;
                loserEconomy.lossBonus = 0;
                break;
            case 'hostage_rescued':
                this.addMoney('counterTerrorist', this.roundWinRewards.hostageRescued);
                loserEconomy.consecutiveLosses = 0;
                loserEconomy.lossBonus = 0;
                break;
        }

        if (winnerEconomy.consecutiveLosses > 0) {
            winnerEconomy.consecutiveLosses = 0;
        }

        if (reason !== 'time_expired' && reason !== 'hostage_rescued') {
            this.addMoney(loser, lossBonusAmount);
        }
    }

    getLossBonus(consecutiveLosses) {
        const index = Math.min(consecutiveLosses, 4);
        return this.lossBonus[index];
    }

    isEcoRound(team) {
        return this.teamEconomies[team].money < this.ecoRoundThreshold;
    }

    forceBuyRound(team) {
        const economy = this.teamEconomies[team];
        economy.money += economy.lossBonus;
        economy.lossBonus = 0;
        economy.consecutiveLosses = 0;
    }

    getTeamEconomy(team) {
        return { ...this.teamEconomies[team] };
    }

    getRecommendedBuy(team) {
        const economy = this.teamEconomies[team];
        const money = economy.money;

        if (money < 2000) {
            return { type: 'eco', budget: money };
        } else if (money < 4500) {
            return { type: 'force', budget: money };
        } else {
            return { type: 'full', budget: money };
        }
    }

    serialize() {
        return {
            teams: {
                terrorist: this.teamEconomies.terrorist,
                counterTerrorist: this.teamEconomies.counterTerrorist
            },
            weaponPrices: this.weaponPrices,
            equipmentPrices: this.equipmentPrices,
            roundRewards: this.roundWinRewards
        };
    }
}

export class RoundSystem {
    constructor() {
        this.currentRound = 0;
        this.maxRounds = 30;
        this.roundPhase = 'freezetime';
        this.roundPhaseTimer = 15;
        this.roundTimer = 115;
        this.maxOvertimeRounds = 6;
        this.isOvertime = false;
        this.overtimeCount = 0;
        this.scores = { terrorist: 0, counterTerrorist: 0 };
        this.teamAlive = { terrorist: 5, counterTerrorist: 5 };
        this.bombPlanted = false;
        this.bombSite = null;
        this.bombPlanter = null;
        this.bombTimer = 40;
        this.hostages = [];
        this.activeHostages = 0;
        this.rescuedHostages = 0;
        this.roundHistory = [];
        this.eventCallbacks = {
            roundStart: [],
            roundEnd: [],
            bombPlanted: [],
            bombDefused: [],
            bombExploded: []
        };
    }

    startRound() {
        this.currentRound++;
        this.roundPhase = 'freezetime';
        this.roundPhaseTimer = 15;
        this.bombPlanted = false;
        this.bombSite = null;
        this.bombPlanter = null;
        this.teamAlive = { terrorist: 5, counterTerrorist: 5 };

        this.emit('roundStart', {
            roundNumber: this.currentRound,
            overtime: this.isOvertime,
            overtimeCount: this.overtimeCount
        });
    }

    endFreezeTime() {
        this.roundPhase = 'active';
        this.roundPhaseTimer = this.roundTimer;
    }

    update(deltaTime) {
        if (this.roundPhase === 'freezetime') {
            this.roundPhaseTimer -= deltaTime;
            if (this.roundPhaseTimer <= 0) {
                this.endFreezeTime();
            }
        } else if (this.roundPhase === 'active') {
            if (this.bombPlanted) {
                this.bombTimer -= deltaTime;
                if (this.bombTimer <= 0) {
                    this.explodeBomb();
                }
            } else {
                this.roundPhaseTimer -= deltaTime;
                if (this.roundPhaseTimer <= 0) {
                    this.endRoundByTime();
                }
            }
        }
    }

    plantBomb(playerId, site) {
        if (this.roundPhase !== 'active') return false;
        this.bombPlanted = true;
        this.bombSite = site;
        this.bombPlanter = playerId;
        this.bombTimer = 40;

        this.emit('bombPlanted', {
            playerId,
            site,
            time: this.bombTimer
        });

        return true;
    }

    defuseBomb(playerId, hasKit) {
        if (!this.bombPlanted) return false;
        const defuseTime = hasKit ? 5 : 10;
        return {
            success: true,
            defuseTime,
            timeRemaining: this.bombTimer
        };
    }

    completeDefuse(playerId, hasKit) {
        if (!this.bombPlanted) return false;

        this.emit('bombDefused', {
            playerId,
            site: this.bombSite,
            hadKit: hasKit
        });

        this.endRound('counterTerrorist', 'bomb_defused');
        return true;
    }

    explodeBomb() {
        this.emit('bombExploded', {
            site: this.bombSite,
            planter: this.bombPlanter
        });

        this.endRound('terrorist', 'bomb_exploded');
    }

    playerDied(playerId, team) {
        this.teamAlive[team]--;
        return this.checkRoundEnd();
    }

    checkRoundEnd() {
        if (this.teamAlive.terrorist === 0 && this.teamAlive.counterTerrorist === 0) {
            this.endRound(null, 'draw');
            return true;
        }

        if (this.teamAlive.terrorist === 0) {
            this.endRound('counterTerrorist', 'elimination');
            return true;
        }

        if (this.teamAlive.counterTerrorist === 0) {
            this.endRound('terrorist', 'elimination');
            return true;
        }

        return false;
    }

    endRoundByTime() {
        if (this.bombPlanted) {
            this.endRound('terrorist', 'bomb_exploded');
        } else {
            this.endRound('counterTerrorist', 'time_expired');
        }
    }

    endRound(winner, reason) {
        this.roundPhase = 'end';

        if (winner) {
            this.scores[winner]++;
        }

        const roundResult = {
            roundNumber: this.currentRound,
            winner,
            reason,
            scores: { ...this.scores },
            teamAlive: { ...this.teamAlive },
            overtime: this.isOvertime,
            overtimeCount: this.overtimeCount
        };

        this.roundHistory.push(roundResult);

        this.emit('roundEnd', roundResult);

        if (this.checkMatchEnd()) {
            return { matchEnd: true, winner: this.determineMatchWinner() };
        }

        if (this.checkOvertime()) {
            this.startOvertime();
        }

        return { matchEnd: false };
    }

    checkMatchEnd() {
        const scoreDiff = Math.abs(this.scores.terrorist - this.scores.counterTerrorist);
        const maxScore = Math.max(this.scores.terrorist, this.scores.counterTerrorist);

        if (this.isOvertime) {
            return this.roundHistory.slice(-this.maxOvertimeRounds).every(r =>
                r.winner !== null && this.roundHistory.slice(-this.maxOvertimeRounds).filter(
                    rr => rr.winner === 'terrorist'
                ).length > this.maxOvertimeRounds / 2
            );
        }

        if (this.currentRound >= this.maxRounds) {
            return scoreDiff > 0 || maxScore >= this.maxRounds / 2 + 1;
        }

        if (maxScore >= this.maxRounds / 2 + 1) {
            return true;
        }

        return false;
    }

    checkOvertime() {
        return this.currentRound >= this.maxRounds &&
               this.scores.terrorist === this.scores.counterTerrorist;
    }

    startOvertime() {
        this.isOvertime = true;
        this.overtimeCount++;
        this.scores = { terrorist: 0, counterTerrorist: 0 };
    }

    determineMatchWinner() {
        if (this.scores.terrorist > this.scores.counterTerrorist) {
            return 'terrorist';
        } else if (this.scores.counterTerrorist > this.scores.terrorist) {
            return 'counterTerrorist';
        }
        return null;
    }

    on(event, callback) {
        if (this.eventCallbacks[event]) {
            this.eventCallbacks[event].push(callback);
        }
    }

    emit(event, data) {
        if (this.eventCallbacks[event]) {
            for (const callback of this.eventCallbacks[event]) {
                callback(data);
            }
        }
    }

    getState() {
        return {
            currentRound: this.currentRound,
            maxRounds: this.maxRounds,
            roundPhase: this.roundPhase,
            roundPhaseTimer: this.roundPhaseTimer,
            scores: { ...this.scores },
            teamAlive: { ...this.teamAlive },
            bombPlanted: this.bombPlanted,
            bombSite: this.bombSite,
            bombTimer: this.bombTimer,
            isOvertime: this.isOvertime,
            overtimeCount: this.overtimeCount
        };
    }

    serialize() {
        return this.getState();
    }
}

export class Vector3 {
    constructor(x = 0, y = 0, z = 0) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    add(v) {
        return new Vector3(this.x + v.x, this.y + v.y, this.z + v.z);
    }

    subtract(v) {
        return new Vector3(this.x - v.x, this.y - v.y, this.z - v.z);
    }

    multiply(scalar) {
        return new Vector3(this.x * scalar, this.y * scalar, this.z * scalar);
    }

    divide(scalar) {
        return new Vector3(this.x / scalar, this.y / scalar, this.z / scalar);
    }

    length() {
        return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
    }

    normalize() {
        const len = this.length();
        if (len === 0) return new Vector3();
        return this.divide(len);
    }

    dot(v) {
        return this.x * v.x + this.y * v.y + this.z * v.z;
    }

    cross(v) {
        return new Vector3(
            this.y * v.z - this.z * v.y,
            this.z * v.x - this.x * v.z,
            this.x * v.y - this.y * v.x
        );
    }

    distance(v) {
        return this.subtract(v).length();
    }

    lerp(v, t) {
        return new Vector3(
            this.x + (v.x - this.x) * t,
            this.y + (v.y - this.y) * t,
            this.z + (v.z - this.z) * t
        );
    }

    clone() {
        return new Vector3(this.x, this.y, this.z);
    }
}

export class Quaternion {
    constructor(x = 0, y = 0, z = 0, w = 1) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    multiply(q) {
        return new Quaternion(
            this.w * q.x + this.x * q.w + this.y * q.z - this.z * q.y,
            this.w * q.y - this.x * q.z + this.y * q.w + this.z * q.x,
            this.w * q.z + this.x * q.y - this.y * q.x + this.z * q.w,
            this.w * q.w - this.x * q.x - this.y * q.y - this.z * q.z
        );
    }

    normalize() {
        const len = Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z + this.w * this.w);
        if (len === 0) return new Quaternion();
        return new Quaternion(this.x / len, this.y / len, this.z / len, this.w / len);
    }

    conjugate() {
        return new Quaternion(-this.x, -this.y, -this.z, this.w);
    }

    toEuler() {
        const sinr_cosp = 2 * (this.w * this.x + this.y * this.z);
        const cosr_cosp = 1 - 2 * (this.x * this.x + this.y * this.y);
        const roll = Math.atan2(sinr_cosp, cosr_cosp);

        const sinp = 2 * (this.w * this.y - this.z * this.x);
        const pitch = Math.abs(sinp) >= 1 ? Math.sign(sinp) * Math.PI / 2 : Math.asin(sinp);

        const siny_cosp = 2 * (this.w * this.z + this.x * this.y);
        const cosy_cosp = 1 - 2 * (this.y * this.y + this.z * this.z);
        const yaw = Math.atan2(siny_cosp, cosy_cosp);

        return { x: roll, y: pitch, z: yaw };
    }

    static fromEuler(x, y, z) {
        const cr = Math.cos(x / 2);
        const sr = Math.sin(x / 2);
        const cp = Math.cos(y / 2);
        const sp = Math.sin(y / 2);
        const cy = Math.cos(z / 2);
        const sy = Math.sin(z / 2);

        return new Quaternion(
            sr * cp * cy - cr * sp * sy,
            cr * sp * cy + sr * cp * sy,
            cr * cp * sy - sr * sp * cy,
            cr * cp * cy + sr * sp * sy
        );
    }

    clone() {
        return new Quaternion(this.x, this.y, this.z, this.w);
    }
}

export default RoundSystem;
