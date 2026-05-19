export class PlayerStats {
    constructor() {
        this.kills = 0;
        this.deaths = 0;
        this.assists = 0;
        this.headshots = 0;
        this.shotsFired = 0;
        this.shotsHit = 0;
        this.damageDealt = 0;
        this.moneyEarned = 0;
        this.moneySpent = 0;
        this.roundsPlayed = 0;
        this.roundsWon = 0;
        this.bombsPlanted = 0;
        this.bombsDefused = 0;
        this.hostagesRescued = 0;
        this.flashbangsThrown = 0;
        this.smokeGrenadesThrown = 0;
        this.heGrenadesThrown = 0;
        this.molotovsThrown = 0;
        this.decoysThrown = 0;
        this.killsThroughSmoke = 0;
        this.noScopeKills = 0;
        this.noScopeHeadshots = 0;
        this.whenFlashed = 0;
        this.whenBlind = 0;
        this.deathsWhileFlashed = 0;
        this.firstKill = 0;
        this.firstDeath = 0;
        this.tradeKill = 0;
        this.tradeDeath = 0;
        this.clutch = 0;
        this.clutchWon = 0;
        this.entries = 0;
        this.entriesWon = 0;
        this.suicides = 0;
        this.teamKills = 0;
        this.killStreak = 0;
        this.deathStreak = 0;
        this.bestKillStreak = 0;
        this.bestDeathStreak = 0;
        this.totalDamage = 0;
        this.totalDamageTaken = 0;
        this.dominations = 0;
        this.dominated = 0;
        this.revenges = 0;
        this.matchHistory = [];
        this.killHistory = [];
        this.maxKillHistory = 100;
    }

    addKill(kill) {
        this.kills++;
        this.killHistory.unshift({
            ...kill,
            timestamp: Date.now()
        });

        if (this.killHistory.length > this.maxKillHistory) {
            this.killHistory.pop();
        }
    }

    addDeath(death) {
        this.deaths++;
    }

    addAssist() {
        this.assists++;
    }

    addHeadshot() {
        this.headshots++;
    }

    addShot(fired, hit) {
        this.shotsFired += fired;
        this.shotsHit += hit;
    }

    addDamage(amount) {
        this.damageDealt += amount;
        this.totalDamage += amount;
    }

    addMoney(amount) {
        this.moneyEarned += amount;
    }

    spendMoney(amount) {
        this.moneySpent += amount;
    }

    addRound(won) {
        this.roundsPlayed++;
        if (won) {
            this.roundsWon++;
        }
    }

    addBombPlanted() {
        this.bombsPlanted++;
    }

    addBombDefused() {
        this.bombsDefused++;
    }

    addHostageRescued() {
        this.hostagesRescued++;
    }

    addGrenadeThrown(type) {
        switch (type) {
            case 'flashbang':
                this.flashbangsThrown++;
                break;
            case 'smoke':
                this.smokeGrenadesThrown++;
                break;
            case 'he':
                this.heGrenadesThrown++;
                break;
            case 'molotov':
            case 'incendiary':
                this.molotovsThrown++;
                break;
            case 'decoy':
                this.decoysThrown++;
                break;
        }
    }

    addKillThroughSmoke() {
        this.killsThroughSmoke++;
    }

    addNoScopeKill() {
        this.noScopeKills++;
    }

    addNoScopeHeadshot() {
        this.noScopeHeadshots++;
    }

    addFlashedEnemy() {
        this.whenFlashed++;
    }

    addBlindEnemy() {
        this.whenBlind++;
    }

    addDeathWhileFlashed() {
        this.deathsWhileFlashed++;
    }

    addFirstKill() {
        this.firstKill++;
    }

    addFirstDeath() {
        this.firstDeath++;
    }

    addTradeKill() {
        this.tradeKill++;
    }

    addTradeDeath() {
        this.tradeDeath++;
    }

    addClutch() {
        this.clutch++;
    }

    addClutchWon() {
        this.clutchWon++;
    }

    addEntry() {
        this.entries++;
    }

    addEntryWon() {
        this.entriesWon++;
    }

    addSuicide() {
        this.suicides++;
    }

    addTeamKill() {
        this.teamKills++;
    }

    updateKillStreak() {
        this.killStreak++;
        if (this.killStreak > this.bestKillStreak) {
            this.bestKillStreak = this.killStreak;
        }
    }

    updateDeathStreak() {
        this.deathStreak++;
        if (this.deathStreak > this.bestDeathStreak) {
            this.bestDeathStreak = this.deathStreak;
        }
    }

    resetStreaks() {
        this.killStreak = 0;
        this.deathStreak = 0;
    }

    reset() {
        this.kills = 0;
        this.deaths = 0;
        this.assists = 0;
        this.headshots = 0;
        this.shotsFired = 0;
        this.shotsHit = 0;
        this.damageDealt = 0;
        this.moneyEarned = 0;
        this.moneySpent = 0;
        this.roundsPlayed = 0;
        this.roundsWon = 0;
        this.bombsPlanted = 0;
        this.bombsDefused = 0;
        this.hostagesRescued = 0;
        this.flashbangsThrown = 0;
        this.smokeGrenadesThrown = 0;
        this.heGrenadesThrown = 0;
        this.molotovsThrown = 0;
        this.decoysThrown = 0;
        this.killsThroughSmoke = 0;
        this.noScopeKills = 0;
        this.noScopeHeadshots = 0;
        this.whenFlashed = 0;
        this.whenBlind = 0;
        this.deathsWhileFlashed = 0;
        this.firstKill = 0;
        this.firstDeath = 0;
        this.tradeKill = 0;
        this.tradeDeath = 0;
        this.clutch = 0;
        this.clutchWon = 0;
        this.entries = 0;
        this.entriesWon = 0;
        this.suicides = 0;
        this.teamKills = 0;
        this.killStreak = 0;
        this.deathStreak = 0;
        this.bestKillStreak = 0;
        this.bestDeathStreak = 0;
        this.totalDamage = 0;
        this.totalDamageTaken = 0;
        this.dominations = 0;
        this.dominated = 0;
        this.revenges = 0;
        this.matchHistory = [];
        this.killHistory = [];
    }

    getKDRatio() {
        return this.deaths > 0 ? this.kills / this.deaths : this.kills;
    }

    getHSRatio() {
        return this.kills > 0 ? this.headshots / this.kills : 0;
    }

    getAccuracy() {
        return this.shotsFired > 0 ? this.shotsHit / this.shotsFired : 0;
    }

    getADR() {
        return this.roundsPlayed > 0 ? this.totalDamage / this.roundsPlayed : 0;
    }

    getWinRate() {
        return this.roundsPlayed > 0 ? this.roundsWon / this.roundsPlayed : 0;
    }

    getKDDiff() {
        return this.kills - this.deaths;
    }

    getEntryRate() {
        return this.entries > 0 ? this.entriesWon / this.entries : 0;
    }

    getClutchRate() {
        return this.clutch > 0 ? this.clutchWon / this.clutch : 0;
    }

    calculateHLTVRating() {
        const killsPerRound = this.roundsPlayed > 0 ? this.kills / this.roundsPlayed : 0;
        const deathsPerRound = this.roundsPlayed > 0 ? this.deaths / this.roundsPlayed : 0;
        const adr = this.getADR();
        const hsRate = this.getHSRatio();
        const kd = this.getKDRatio();

        const rating = 0.0076 * killsPerRound +
                        0.0360 * adr / 100 +
                        0.3591 * hsRate +
                        0.0006 * kd +
                        0.0003 * (this.firstKill / this.roundsPlayed) -
                        0.0009 * deathsPerRound;

        return Math.max(0.5, Math.min(1.5, rating));
    }

    getRankedStats() {
        return {
            kills: this.kills,
            deaths: this.deaths,
            assists: this.assists,
            headshots: this.headshots,
            kd: this.getKDRatio().toFixed(2),
            adr: Math.round(this.getADR()),
            hsPercent: (this.getHSRatio() * 100).toFixed(1),
            winRate: (this.getWinRate() * 100).toFixed(1),
            clutchRate: this.clutch > 0 ? (this.clutchWon / this.clutch * 100).toFixed(1) : '0.0',
            rating: this.calculateHLTVRating().toFixed(2)
        };
    }

    serialize() {
        return {
            kills: this.kills,
            deaths: this.deaths,
            assists: this.assists,
            headshots: this.headshots,
            shotsFired: this.shotsFired,
            shotsHit: this.shotsHit,
            damageDealt: this.damageDealt,
            moneyEarned: this.moneyEarned,
            moneySpent: this.moneySpent,
            roundsPlayed: this.roundsPlayed,
            roundsWon: this.roundsWon,
            bombsPlanted: this.bombsPlanted,
            bombsDefused: this.bombsDefused,
            hostagesRescued: this.hostagesRescued,
            grenades: {
                flashbang: this.flashbangsThrown,
                smoke: this.smokeGrenadesThrown,
                he: this.heGrenadesThrown,
                molotov: this.molotovsThrown,
                decoy: this.decoysThrown
            },
            specialKills: {
                throughSmoke: this.killsThroughSmoke,
                noScope: this.noScopeKills,
                noScopeHeadshot: this.noScopeHeadshots
            },
            flashStats: {
                enemiesFlashed: this.whenFlashed,
                enemiesBlinded: this.whenBlind,
                deathsWhileFlashed: this.deathsWhileFlashed
            },
            roundStats: {
                firstKill: this.firstKill,
                firstDeath: this.firstDeath,
                tradeKill: this.tradeKill,
                tradeDeath: this.tradeDeath,
                clutch: this.clutch,
                clutchWon: this.clutchWon,
                entries: this.entries,
                entriesWon: this.entriesWon
            },
            misc: {
                suicides: this.suicides,
                teamKills: this.teamKills,
                bestKillStreak: this.bestKillStreak,
                bestDeathStreak: this.bestDeathStreak
            }
        };
    }
}

export class MatchPlayer extends PlayerStats {
    constructor(playerId, team) {
        super();
        this.playerId = playerId;
        this.team = team;
        this.name = '';
        this.steamId = '';
        this.isAlive = true;
        this.health = 100;
        this.armor = 0;
        this.hasHelmet = false;
        this.hasDefuseKit = false;
        this.hasBomb = false;
        this.position = { x: 0, y: 0, z: 0 };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.viewAngle = { x: 0, y: 0 };
        this.isScoped = false;
        this.isInBuyZone = false;
        this.isDucking = false;
        this.isWalking = false;
        this.isReloading = false;
        this.isFlashed = false;
        this.flashDuration = 0;
        this.currentWeapon = null;
        this.weapons = [];
        this.grenades = {
            flashbang: 0,
            smoke: 0,
            he: 0,
            molotov: 0,
            decoy: 0
        };
        this.money = 800;
        this.accountValue = 0;
        this.lastSeenPosition = null;
        this.lastSeenTime = 0;
        this.isSpotted = false;
        this.spottedBy = new Set();
        this.observerTargets = [];
        this.isBot = false;
        this.isConnected = true;
        this.isAFK = false;
        this.ping = 0;
        this.score = 0;
    }

    spawn(position) {
        this.isAlive = true;
        this.health = 100;
        this.armor = 0;
        this.hasHelmet = false;
        this.hasDefuseKit = false;
        this.position = { ...position };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.isScoped = false;
        this.isDucking = false;
        this.isWalking = false;
        this.isReloading = false;
        this.isFlashed = false;
    }

    takeDamage(amount, hitgroup) {
        let actualDamage = amount;

        if (hitgroup === 'head' && this.hasHelmet) {
            actualDamage *= 0.75;
        }

        if (this.armor > 0) {
            const armorDamage = actualDamage * 0.8;
            this.armor = Math.max(0, this.armor - armorDamage);
        }

        this.health = Math.max(0, this.health - actualDamage);

        if (this.health <= 0) {
            this.die();
        }

        return actualDamage;
    }

    heal(amount) {
        this.health = Math.min(100, this.health + amount);
    }

    giveArmor(amount) {
        this.armor = Math.min(100, this.armor + amount);
    }

    giveHelmet() {
        this.hasHelmet = true;
    }

    giveDefuseKit() {
        this.hasDefuseKit = true;
    }

    giveBomb() {
        this.hasBomb = true;
    }

    takeBomb() {
        this.hasBomb = false;
    }

    giveMoney(amount) {
        this.money = Math.min(16000, this.money + amount);
    }

    spendMoney(amount) {
        if (this.money >= amount) {
            this.money -= amount;
            return true;
        }
        return false;
    }

    giveWeapon(weapon) {
        this.weapons.push(weapon);
        if (weapon.category === 'primary' || weapon.category === 'secondary') {
            this.currentWeapon = weapon;
        }
    }

    dropWeapon(weaponId) {
        const index = this.weapons.findIndex(w => w.id === weaponId);
        if (index !== -1) {
            const weapon = this.weapons[index];
            this.weapons.splice(index, 1);
            if (this.currentWeapon === weapon) {
                this.currentWeapon = this.weapons[0] || null;
            }
            return weapon;
        }
        return null;
    }

    giveGrenade(type, count = 1) {
        if (this.grenades[type] !== undefined) {
            this.grenades[type] += count;
        }
    }

    useGrenade(type) {
        if (this.grenades[type] > 0) {
            this.grenades[type]--;
            return true;
        }
        return false;
    }

    flash(duration) {
        this.isFlashed = true;
        this.flashDuration = duration;
    }

    update(deltaTime) {
        if (this.isFlashed) {
            this.flashDuration -= deltaTime;
            if (this.flashDuration <= 0) {
                this.isFlashed = false;
                this.flashDuration = 0;
            }
        }
    }

    die() {
        this.isAlive = false;
        this.health = 0;
        this.lastSeenPosition = { ...this.position };
        this.lastSeenTime = Date.now();
    }

    observe(target) {
        if (!this.observerTargets.includes(target)) {
            this.observerTargets.push(target);
        }
    }

    stopObserving(target) {
        const index = this.observerTargets.indexOf(target);
        if (index !== -1) {
            this.observerTargets.splice(index, 1);
        }
    }

    getEyePosition() {
        const height = this.isDucking ? 46 : 72;
        return {
            x: this.position.x,
            y: this.position.y,
            z: this.position.z + height
        };
    }

    lookAt(position) {
        const dx = position.x - this.position.x;
        const dy = position.y - this.position.y;
        const dz = position.z - this.getEyePosition().z;

        this.viewAngle.y = Math.atan2(dy, dx);
        this.viewAngle.x = Math.atan2(-dz, Math.sqrt(dx * dx + dy * dy));
    }

    move(direction, speed) {
        const rad = this.viewAngle.y;
        const sin = Math.sin(rad);
        const cos = Math.cos(rad);

        this.velocity.x = cos * direction.x - sin * direction.y;
        this.velocity.y = sin * direction.x + cos * direction.y;

        this.position.x += this.velocity.x * speed;
        this.position.y += this.velocity.y * speed;
    }

    jump() {
        this.velocity.z = 250;
    }

    crouch() {
        this.isDucking = true;
    }

    stand() {
        this.isDucking = false;
    }

    scope() {
        this.isScoped = true;
    }

    unscoped() {
        this.isScoped = false;
    }

    reload() {
        if (this.currentWeapon && !this.isReloading) {
            this.isReloading = true;
            return this.currentWeapon.reloadTime;
        }
        return 0;
    }

    finishReload() {
        this.isReloading = false;
    }

    getWeaponByType(type) {
        return this.weapons.find(w => w.type === type);
    }

    hasWeapon(weaponId) {
        return this.weapons.some(w => w.id === weaponId);
    }

    getTotalEquipmentValue() {
        let value = 0;

        for (const weapon of this.weapons) {
            value += weapon.price || 0;
        }

        value += this.grenades.flashbang * 200;
        value += this.grenades.smoke * 300;
        value += this.grenades.he * 300;
        value += this.grenades.molotov * 400;
        value += this.grenades.decoy * 50;

        if (this.hasHelmet) value += 350;
        if (this.armor > 0) value += this.armor > 50 ? 650 : 400;
        if (this.hasDefuseKit) value += 400;

        return value;
    }

    serialize() {
        return {
            playerId: this.playerId,
            team: this.team,
            name: this.name,
            isAlive: this.isAlive,
            health: this.health,
            armor: this.armor,
            hasHelmet: this.hasHelmet,
            hasDefuseKit: this.hasDefuseKit,
            hasBomb: this.hasBomb,
            position: this.position,
            isFlashed: this.isFlashed,
            money: this.money,
            accountValue: this.getTotalEquipmentValue(),
            stats: {
                kills: this.kills,
                deaths: this.deaths,
                assists: this.assists
            }
        };
    }
}

export class Match {
    constructor(config = {}) {
        this.id = Match.nextId++;
        this.config = config;
        this.state = 'created';
        this.gameMode = config.gameMode || 'competitive';
        this.map = config.map || 'de_dust2';
        this.tickRate = config.tickRate || 128;
        this.maxRounds = config.maxRounds || 30;
        this.roundTime = config.roundTime || 115;
        this.freezeTime = config.freezeTime || 15;
        this.buyTime = config.buyTime || 20;
        this.players = new Map();
        this.spectators = [];
        this.currentRound = 0;
        this.roundPhase = 'freezetime';
        this.roundTimer = this.freezeTime;
        this.bombTimer = 40;
        this.scores = { terrorist: 0, counterTerrorist: 0 };
        this.teamAlive = { terrorist: 5, counterTerrorist: 5 };
        this.bombPlanted = false;
        this.bombSite = null;
        this.bombPlanter = null;
        this.hostages = [];
        this.startTime = 0;
        this.endTime = 0;
        this.duration = 0;
        this.events = [];
        this.frames = [];
        this.winner = null;
        this.stats = {
            totalKills: 0,
            totalDamage: 0,
            grenadeUsage: 0,
            clutchSituations: 0
        };
    }

    initialize() {
        this.state = 'initializing';
        this.createDefaultTeams();
        this.initializeHostages();
        this.startTime = Date.now();
        this.state = 'running';
    }

    createDefaultTeams() {
        for (let i = 0; i < 5; i++) {
            const ctPlayer = new MatchPlayer(`ct_${i}`, 'counterTerrorist');
            ctPlayer.name = `CT Player ${i + 1}`;
            this.players.set(ctPlayer.playerId, ctPlayer);
        }

        for (let i = 0; i < 5; i++) {
            const tPlayer = new MatchPlayer(`t_${i}`, 'terrorist');
            tPlayer.name = `T Player ${i + 1}`;
            this.players.set(tPlayer.playerId, tPlayer);
        }
    }

    initializeHostages() {
        if (this.gameMode === 'hostage') {
            for (let i = 0; i < 4; i++) {
                this.hostages.push({
                    id: i,
                    position: { x: 0, y: 0, z: 0 },
                    state: 'idle',
                    health: 100,
                    beingCarried: false,
                    carrier: null,
                    rescued: false,
                    rescueZone: null
                });
            }
        }
    }

    addPlayer(playerId, team) {
        if (this.players.has(playerId)) return false;

        const player = new MatchPlayer(playerId, team);
        this.players.set(playerId, player);
        return player;
    }

    removePlayer(playerId) {
        return this.players.delete(playerId);
    }

    getPlayer(playerId) {
        return this.players.get(playerId);
    }

    getTeamPlayers(team) {
        return Array.from(this.players.values()).filter(p => p.team === team);
    }

    getAlivePlayers(team) {
        return this.getTeamPlayers(team).filter(p => p.isAlive);
    }

    getPlayerCount(team = null) {
        if (team) {
            return this.getTeamPlayers(team).length;
        }
        return this.players.size;
    }

    startRound() {
        this.currentRound++;
        this.roundPhase = 'freezetime';
        this.roundTimer = this.freezeTime;
        this.bombPlanted = false;
        this.bombSite = null;
        this.bombPlanter = null;

        for (const player of this.players.values()) {
            player.spawn(this.getSpawnPosition(player.team));
        }
    }

    endFreezeTime() {
        this.roundPhase = 'active';
        this.roundTimer = this.roundTime;
    }

    update(deltaTime) {
        if (this.state !== 'running') return;

        if (this.roundPhase === 'freezetime') {
            this.roundTimer -= deltaTime;
            if (this.roundTimer <= 0) {
                this.endFreezeTime();
            }
        } else if (this.roundPhase === 'active') {
            this.roundTimer -= deltaTime;

            if (this.bombPlanted) {
                this.bombTimer -= deltaTime;
            }

            if (this.roundTimer <= 0 || this.checkRoundEnd()) {
                this.endRound(this.determineRoundWinner());
            }
        }

        for (const player of this.players.values()) {
            player.update(deltaTime);
        }
    }

    checkRoundEnd() {
        const ctAlive = this.getAlivePlayers('counterTerrorist').length;
        const tAlive = this.getAlivePlayers('terrorist').length;

        if (ctAlive === 0 && tAlive === 0) return true;
        if (ctAlive === 0) return true;
        if (tAlive === 0) return true;
        if (this.bombPlanted && ctAlive === 0) return true;

        return false;
    }

    determineRoundWinner() {
        if (this.bombPlanted && this.bombTimer <= 0) {
            return 'terrorist';
        }

        const ctAlive = this.getAlivePlayers('counterTerrorist').length;
        const tAlive = this.getAlivePlayers('terrorist').length;

        if (ctAlive === 0 && tAlive === 0) return null;
        if (ctAlive === 0) return 'terrorist';
        if (tAlive === 0) return 'counterTerrorist';

        if (!this.bombPlanted && this.roundTimer <= 0) {
            return 'counterTerrorist';
        }

        return null;
    }

    endRound(winner) {
        if (winner) {
            this.scores[winner]++;
        }

        for (const player of this.players.values()) {
            player.addRound(player.team === winner);
        }

        if (this.checkMatchEnd()) {
            this.endMatch(winner);
        } else {
            this.startRound();
        }
    }

    checkMatchEnd() {
        const scoreDiff = Math.abs(this.scores.terrorist - this.scores.counterTerrorist);
        const maxScore = Math.max(this.scores.terrorist, this.scores.counterTerrorist);

        if (this.currentRound >= this.maxRounds && scoreDiff > 0) {
            return true;
        }

        if (maxScore >= this.maxRounds / 2 + 1) {
            return true;
        }

        return false;
    }

    endMatch(winner) {
        this.state = 'ended';
        this.endTime = Date.now();
        this.duration = this.endTime - this.startTime;
        this.winner = winner;
        this.recordMatch();
    }

    recordMatch() {
        for (const player of this.players.values()) {
            const stats = player.serialize();
            player.matchHistory.push({
                matchId: this.id,
                date: this.startTime,
                map: this.map,
                score: { ...this.scores },
                won: player.team === this.winner,
                stats
            });
        }
    }

    plantBomb(playerId, site) {
        const player = this.getPlayer(playerId);
        if (!player || !player.hasBomb) return false;
        if (this.roundPhase !== 'active') return false;

        this.bombPlanted = true;
        this.bombSite = site;
        this.bombPlanter = playerId;
        this.bombTimer = 40;
        player.hasBomb = false;
        player.addBombPlanted();

        return true;
    }

    defuseBomb(playerId, hasKit) {
        if (!this.bombPlanted) return false;
        const player = this.getPlayer(playerId);
        if (!player) return false;

        const timeRequired = hasKit ? 5 : 10;
        const timeRemaining = this.bombTimer;

        return {
            canComplete: timeRemaining > timeRequired,
            timeRequired,
            timeRemaining
        };
    }

    completeDefuse(playerId) {
        if (!this.bombPlanted) return false;

        const player = this.getPlayer(playerId);
        if (player) {
            player.addBombDefused();
        }

        this.endRound('counterTerrorist');
        return true;
    }

    getSpawnPosition(team) {
        if (team === 'counterTerrorist') {
            return { x: 2500, y: -2500, z: 0 };
        } else {
            return { x: -2500, y: 2500, z: 0 };
        }
    }

    serialize() {
        return {
            id: this.id,
            state: this.state,
            gameMode: this.gameMode,
            map: this.map,
            currentRound: this.currentRound,
            roundPhase: this.roundPhase,
            roundTimer: this.roundTimer,
            scores: this.scores,
            teamAlive: {
                terrorist: this.getAlivePlayers('terrorist').length,
                counterTerrorist: this.getAlivePlayers('counterTerrorist').length
            },
            bombPlanted: this.bombPlanted,
            bombSite: this.bombSite,
            bombTimer: this.bombTimer,
            duration: this.duration,
            winner: this.winner
        };
    }
}
Match.nextId = 0;

export class MatchManager {
    constructor() {
        this.matches = new Map();
        this.activeMatch = null;
        this.matchHistory = [];
        this.maxHistorySize = 100;
    }

    createMatch(config = {}) {
        const match = new Match(config);
        this.matches.set(match.id, match);
        this.activeMatch = match;
        return match;
    }

    getMatch(matchId) {
        return this.matches.get(matchId);
    }

    getActiveMatch() {
        return this.activeMatch;
    }

    endMatch(matchId) {
        const match = this.matches.get(matchId);
        if (match) {
            match.endMatch(null);

            this.matchHistory.push(match);
            if (this.matchHistory.length > this.maxHistorySize) {
                this.matchHistory.shift();
            }

            if (this.activeMatch === match) {
                this.activeMatch = null;
            }

            return match;
        }
        return null;
    }

    getMatchHistory(playerId = null) {
        if (playerId) {
            return this.matchHistory.filter(match =>
                match.players.has(playerId)
            );
        }
        return this.matchHistory;
    }

    update(deltaTime) {
        if (this.activeMatch) {
            this.activeMatch.update(deltaTime);
        }
    }

    serialize() {
        return {
            activeMatches: this.matches.size,
            matchHistory: this.matchHistory.length
        };
    }
}

export default MatchManager;
