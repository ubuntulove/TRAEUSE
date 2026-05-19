export class DemoAnalyzer {
    constructor() {
        this.demos = new Map();
        this.analyses = new Map();
        this.bookmarks = new Map();
        this.shares = new Map();
        this.annotations = new Map();
        this.highlights = new Map();
    }

    loadDemo(demoData) {
        const demo = new Demo(demoData);
        this.demos.set(demo.id, demo);
        return demo;
    }

    getDemo(demoId) {
        return this.demos.get(demoId);
    }

    analyze(demoId, options = {}) {
        const demo = this.demos.get(demoId);
        if (!demo) return null;

        const analysis = {
            id: 'analysis_' + Date.now(),
            demoId,
            timestamp: Date.now(),
            duration: demo.duration,
            map: demo.map,
            gameMode: demo.gameMode,
            players: [],
            kills: [],
            rounds: [],
            economy: [],
            highlights: [],
            statistics: {},
            annotations: [],
            bookmarks: [],
            shareData: {
                views: 0,
                downloads: 0,
                shares: 0
            },
            reportGenerated: Date.now()
        };

        analysis.statistics = this.generateStatistics(demo, analysis);
        analysis.kills = this.analyzeKills(demo, analysis);
        analysis.rounds = this.analyzeRounds(demo, analysis);
        analysis.economy = this.analyzeEconomy(demo, analysis);
        analysis.highlights = this.detectHighlights(demo, analysis);

        this.analyses.set(analysis.id, analysis);
        demo.analysis = analysis;

        return analysis;
    }

    generateStatistics(demo, analysis) {
        const stats = {
            totalKills: 0,
            totalDeaths: 0,
            totalAssists: 0,
            totalRounds: 0,
            team1Score: 0,
            team2Score: 0,
            totalDamage: 0,
            headshotPercentage: 0,
            pistolRoundWins: { team1: 0, team2: 0 },
            clutchWins: { team1: 0, team2: 0 },
            entryKills: { team1: 0, team2: 0 },
            tradeKills: { team1: 0, team2: 0 },
            firstBloods: { team1: 0, team2: 0 }
        };

        for (const round of demo.rounds) {
            stats.totalRounds++;
            
            if (round.winner === 'team1') {
                if (round.endCondition === 'bomb_defused' || round.endCondition === 'bomb_exploded') {
                    stats.team1Score++;
                } else {
                    stats.team1Score++;
                }
            } else {
                stats.team2Score++;
            }

            if (round.isPistolRound) {
                if (round.winner === 'team1') stats.pistolRoundWins.team1++;
                else stats.pistolRoundWins.team2++;
            }

            stats.totalKills += round.kills.length;
            stats.totalAssists += round.assists.length;
        }

        for (const kill of demo.kills) {
            if (kill.headshot) {
                stats.headshotPercentage++;
            }
        }

        if (stats.totalKills > 0) {
            stats.headshotPercentage = (stats.headshotPercentage / stats.totalKills) * 100;
        }

        return stats;
    }

    analyzeKills(demo, analysis) {
        const kills = [];
        
        for (const kill of demo.kills) {
            const killData = {
                tick: kill.tick,
                time: kill.time,
                round: kill.round,
                killer: kill.killer,
                victim: kill.victim,
                weapon: kill.weapon,
                headshot: kill.headshot,
                throughWall: kill.throughWall,
                distance: kill.distance,
                damage: kill.damage,
                position: kill.position,
                velocity: kill.velocity,
                blindDuration: kill.blindDuration,
                crouching: kill.crouching,
                jumping: kill.jumping,
                type: this.categorizeKill(kill)
            };

            kills.push(killData);
        }

        return kills;
    }

    categorizeKill(kill) {
        if (kill.distance > 1000) return 'long_range';
        if (kill.throughWall) return 'wallbang';
        if (kill.headshot) return 'headshot';
        if (kill.jumping) return 'air';
        if (kill.crouching) return 'crouch';
        return 'standard';
    }

    analyzeRounds(demo, analysis) {
        const rounds = [];

        for (const round of demo.rounds) {
            const roundData = {
                number: round.number,
                startTick: round.startTick,
                endTick: round.endTick,
                duration: round.duration,
                winner: round.winner,
                endCondition: round.endCondition,
                team1Score: round.team1Score,
                team2Score: round.team2Score,
                isPistolRound: round.isPistolRound,
                isForceBuy: round.isForceBuy,
                isEcoRound: round.isEcoRound,
                kills: round.kills.length,
                damage: round.damage,
                economy: round.economy,
                utilityUsed: round.utilityUsed,
                abilitiesUsed: round.abilitiesUsed
            };

            rounds.push(roundData);
        }

        return rounds;
    }

    analyzeEconomy(demo, analysis) {
        const economy = [];

        for (const round of demo.rounds) {
            const economyData = {
                round: round.number,
                team1: {
                    money: round.economy.team1.money,
                    equipment: round.economy.team1.equipment,
                    total: round.economy.team1.total
                },
                team2: {
                    money: round.economy.team2.money,
                    equipment: round.economy.team2.equipment,
                    total: round.economy.team2.total
                }
            };

            economy.push(economyData);
        }

        return economy;
    }

    detectHighlights(demo, analysis) {
        const highlights = [];

        for (const kill of demo.kills) {
            if (kill.headshot && kill.distance > 500) {
                highlights.push({
                    type: 'highlight',
                    subType: 'highlight_shot',
                    tick: kill.tick,
                    time: kill.time,
                    description: `Highlight Shot: ${kill.killer} got a ${Math.round(kill.distance)}m headshot`,
                    player: kill.killer,
                    weapon: kill.weapon,
                    score: 90
                });
            }

            if (kill.throughWall) {
                highlights.push({
                    type: 'highlight',
                    subType: 'wallbang',
                    tick: kill.tick,
                    time: kill.time,
                    description: `Wallbang: ${kill.killer} killed ${kill.victim} through wall`,
                    player: kill.killer,
                    score: 80
                });
            }

            if (kill.noScope) {
                highlights.push({
                    type: 'highlight',
                    subType: 'no_scope',
                    tick: kill.tick,
                    time: kill.time,
                    description: `No Scope: ${kill.killer}`,
                    player: kill.killer,
                    score: 85
                });
            }
        }

        highlights.sort((a, b) => b.score - a.score);

        return highlights;
    }

    addBookmark(analysisId, bookmarkData) {
        const analysis = this.analyses.get(analysisId);
        if (!analysis) return null;

        const bookmark = {
            id: 'bookmark_' + Date.now(),
            analysisId,
            tick: bookmarkData.tick,
            time: bookmarkData.time,
            round: bookmarkData.round,
            description: bookmarkData.description,
            category: bookmarkData.category || 'general',
            tags: bookmarkData.tags || [],
            createdAt: Date.now()
        };

        if (!analysis.bookmarks) {
            analysis.bookmarks = [];
        }

        analysis.bookmarks.push(bookmark);
        return bookmark;
    }

    addAnnotation(analysisId, annotationData) {
        const analysis = this.analyses.get(analysisId);
        if (!analysis) return null;

        const annotation = {
            id: 'annotation_' + Date.now(),
            analysisId,
            tick: annotationData.tick,
            time: annotationData.time,
            round: annotationData.round,
            text: annotationData.text,
            player: annotationData.player,
            type: annotationData.type || 'note',
            color: annotationData.color || '#ffffff',
            createdAt: Date.now()
        };

        if (!analysis.annotations) {
            analysis.annotations = [];
        }

        analysis.annotations.push(annotation);
        return annotation;
    }

    exportAnalysis(analysisId, format = 'json') {
        const analysis = this.analyses.get(analysisId);
        if (!analysis) return null;

        switch (format) {
            case 'json':
                return JSON.stringify(analysis, null, 2);
            case 'csv':
                return this.exportToCSV(analysis);
            case 'pdf':
                return this.exportToPDF(analysis);
            default:
                return analysis;
        }
    }

    exportToCSV(analysis) {
        let csv = '';

        csv += 'Round Analysis\n';
        csv += 'Round,StartTick,EndTick,Duration,Winner,EndCondition,Team1Score,Team2Score\n';
        for (const round of analysis.rounds) {
            csv += `${round.number},${round.startTick},${round.endTick},${round.duration},${round.winner},${round.endCondition},${round.team1Score},${round.team2Score}\n`;
        }

        csv += '\nKill Analysis\n';
        csv += 'Tick,Time,Round,Killer,Victim,Weapon,Headshot,Distance\n';
        for (const kill of analysis.kills) {
            csv += `${kill.tick},${kill.time},${kill.round},${kill.killer},${kill.victim},${kill.weapon},${kill.headshot},${kill.distance}\n`;
        }

        return csv;
    }

    exportToPDF(analysis) {
        return {
            title: 'Match Analysis Report',
            generated: new Date().toISOString(),
            demoId: analysis.demoId,
            duration: analysis.duration,
            map: analysis.map,
            statistics: analysis.statistics,
            highlights: analysis.highlights.slice(0, 10)
        };
    }

    shareAnalysis(analysisId, shareData) {
        const analysis = this.analyses.get(analysisId);
        if (!analysis) return null;

        const share = {
            id: 'share_' + Date.now(),
            analysisId,
            ...shareData,
            createdAt: Date.now(),
            views: 0,
            downloads: 0
        };

        this.shares.set(share.id, share);

        if (!analysis.shareData) {
            analysis.shareData = { views: 0, downloads: 0, shares: 0 };
        }
        analysis.shareData.shares++;

        return share;
    }

    getSharedAnalysis(shareId) {
        return this.shares.get(shareId);
    }
}

export class Demo {
    constructor(data) {
        this.id = data.id || 'demo_' + Date.now();
        this.name = data.name;
        this.filename = data.filename;
        this.matchId = data.matchId;
        this.map = data.map;
        this.gameMode = data.gameMode;
        this.tickRate = data.tickRate || 64;
        this.startTick = data.startTick || 0;
        this.endTick = data.endTick || 0;
        this.duration = data.duration || 0;
        this.fileSize = data.fileSize || 0;
        this.md5 = data.md5;
        this.uploadedBy = data.uploadedBy;
        this.uploadedAt = Date.now();
        this.processed = false;
        this.analysis = null;
        this.players = new Map();
        this.teams = {
            team1: [],
            team2: []
        };
        this.kills = [];
        this.rounds = [];
        this.events = [];
        this.settings = {
            playbackSpeed: 1.0,
            showPlayers: true,
            showProjectiles: true,
            showGrenades: true,
            showDamage: true,
            showFootsteps: false,
            showLines: true
        };
    }

    addPlayer(playerData) {
        const player = {
            id: playerData.id,
            name: playerData.name,
            steamId: playerData.steamId,
            team: playerData.team,
            kills: 0,
            deaths: 0,
            assists: 0,
            damage: 0,
            score: 0,
            headshots: 0,
            stats: {}
        };

        this.players.set(player.id, player);

        if (playerData.team === 'team1') {
            this.teams.team1.push(player.id);
        } else {
            this.teams.team2.push(player.id);
        }

        return player;
    }

    addKill(killData) {
        const kill = {
            id: 'kill_' + Date.now(),
            tick: killData.tick,
            time: killData.time,
            round: killData.round,
            killer: killData.killer,
            victim: killData.victim,
            weapon: killData.weapon,
            headshot: killData.headshot || false,
            throughWall: killData.throughWall || false,
            distance: killData.distance || 0,
            damage: killData.damage || 100,
            position: killData.position,
            velocity: killData.velocity,
            blindDuration: killData.blindDuration || 0,
            crouching: killData.crouching || false,
            jumping: killData.jumping || false,
            noScope: killData.noScope || false
        };

        this.kills.push(kill);

        if (this.players.has(kill.killer)) {
            const killer = this.players.get(kill.killer);
            killer.kills++;
            killer.headshots += kill.headshot ? 1 : 0;
        }

        if (this.players.has(kill.victim)) {
            const victim = this.players.get(kill.victim);
            victim.deaths++;
        }

        return kill;
    }

    addRound(roundData) {
        const round = {
            id: 'round_' + Date.now(),
            number: roundData.number,
            startTick: roundData.startTick,
            endTick: roundData.endTick,
            duration: roundData.duration,
            winner: roundData.winner,
            endCondition: roundData.endCondition,
            team1Score: roundData.team1Score,
            team2Score: roundData.team2Score,
            isPistolRound: roundData.isPistolRound || false,
            isForceBuy: roundData.isForceBuy || false,
            isEcoRound: roundData.isEcoRound || false,
            kills: [],
            assists: [],
            damage: [],
            economy: {
                team1: roundData.economy?.team1 || { money: 0, equipment: 0 },
                team2: roundData.economy?.team2 || { money: 0, equipment: 0 }
            },
            utilityUsed: roundData.utilityUsed || [],
            abilitiesUsed: roundData.abilitiesUsed || []
        };

        this.rounds.push(round);
        return round;
    }

    getPlayerStats(playerId) {
        const player = this.players.get(playerId);
        if (!player) return null;

        return {
            ...player,
            kd: player.deaths > 0 ? (player.kills / player.deaths).toFixed(2) : player.kills.toFixed(2),
            hsRate: player.kills > 0 ? ((player.headshots / player.kills) * 100).toFixed(1) : '0.0',
            adr: 0,
            rating: this.calculatePlayerRating(playerId)
        };
    }

    calculatePlayerRating(playerId) {
        const player = this.players.get(playerId);
        if (!player) return 0;

        const kdRatio = player.deaths > 0 ? player.kills / player.deaths : player.kills;
        const hsRatio = player.kills > 0 ? player.headshots / player.kills : 0;

        return (kdRatio * 0.6 + hsRatio * 2 * 0.4).toFixed(2);
    }

    getRound(roundNumber) {
        return this.rounds.find(r => r.number === roundNumber);
    }

    getKillsInRound(roundNumber) {
        return this.kills.filter(k => k.round === roundNumber);
    }

    getTicksInRange(startTick, endTick) {
        return {
            startTick,
            endTick,
            kills: this.kills.filter(k => k.tick >= startTick && k.tick <= endTick),
            events: this.events.filter(e => e.tick >= startTick && e.tick <= endTick)
        };
    }

    getHighlightReel(options = {}) {
        const highlights = [];

        for (const kill of this.kills) {
            if (kill.headshot && kill.distance > 300) {
                highlights.push({
                    type: 'ace_shot',
                    tick: kill.tick,
                    time: kill.time,
                    description: `Amazing Shot: ${kill.killer}`,
                    score: 90
                });
            }
        }

        highlights.sort((a, b) => b.score - a.score);

        return highlights.slice(0, options.limit || 10);
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            filename: this.filename,
            matchId: this.matchId,
            map: this.map,
            gameMode: this.gameMode,
            tickRate: this.tickRate,
            startTick: this.startTick,
            endTick: this.endTick,
            duration: this.duration,
            fileSize: this.fileSize,
            uploadedBy: this.uploadedBy,
            uploadedAt: this.uploadedAt,
            processed: this.processed,
            players: Array.from(this.players.entries()),
            teams: this.teams,
            kills: this.kills,
            rounds: this.rounds,
            settings: this.settings
        };
    }
}

export class DemoPlayer {
    constructor() {
        this.currentDemo = null;
        this.currentTick = 0;
        this.playbackSpeed = 1.0;
        this.isPlaying = false;
        this.isPaused = false;
        this.isLooping = false;
        this.volume = 1.0;
        this.muted = false;
    }

    loadDemo(demo) {
        this.currentDemo = demo;
        this.currentTick = demo.startTick;
        this.isPlaying = false;
        this.isPaused = false;
    }

    play() {
        if (!this.currentDemo) return;
        this.isPlaying = true;
        this.isPaused = false;
    }

    pause() {
        this.isPlaying = false;
        this.isPaused = true;
    }

    stop() {
        this.isPlaying = false;
        this.isPaused = false;
        this.currentTick = this.currentDemo.startTick;
    }

    seek(tick) {
        if (!this.currentDemo) return;
        this.currentTick = Math.max(this.currentDemo.startTick, Math.min(tick, this.currentDemo.endTick));
    }

    seekToTime(timeInSeconds) {
        if (!this.currentDemo) return;
        const tick = Math.floor(timeInSeconds * this.currentDemo.tickRate);
        this.seek(tick);
    }

    setPlaybackSpeed(speed) {
        this.playbackSpeed = Math.max(0.1, Math.min(4.0, speed));
    }

    getState() {
        return {
            isPlaying: this.isPlaying,
            isPaused: this.isPaused,
            currentTick: this.currentTick,
            currentTime: this.getCurrentTime(),
            totalTime: this.getTotalTime(),
            progress: this.getProgress(),
            playbackSpeed: this.playbackSpeed
        };
    }

    getCurrentTime() {
        return this.currentTick / this.currentDemo.tickRate;
    }

    getTotalTime() {
        return this.currentDemo.duration / this.currentDemo.tickRate;
    }

    getProgress() {
        return (this.currentTick / this.currentDemo.endTick) * 100;
    }

    tick(deltaTime) {
        if (!this.isPlaying || !this.currentDemo) return;

        const ticksToAdvance = Math.floor(deltaTime * this.currentDemo.tickRate * this.playbackSpeed);
        this.currentTick += ticksToAdvance;

        if (this.currentTick >= this.currentDemo.endTick) {
            if (this.isLooping) {
                this.currentTick = this.currentDemo.startTick;
            } else {
                this.stop();
            }
        }
    }
}

export default {
    DemoAnalyzer,
    Demo,
    DemoPlayer
};
