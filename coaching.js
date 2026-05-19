export class AICoachSystem {
    constructor() {
        this.coaches = new Map();
        this.playerProfiles = new Map();
        this.matchAnalyses = new Map();
        this.tips = new Map();
        this.recommendations = new Map();
        this.trainings = new Map();
        this.sessions = new Map();
        this.analytics = new CoachAnalytics();
    }

    initialize() {
        this.loadDefaultCoaches();
        this.initializeTipDatabase();
    }

    loadDefaultCoaches() {
        const coaches = [
            {
                id: 'coach_analyst',
                name: 'Analyst',
                description: 'Focuses on tactical analysis and strategic advice',
                specialties: ['tactics', 'strategy', 'positioning'],
                style: 'analytical',
                avatar: 'analyst_avatar'
            },
            {
                id: 'coach_trainer',
                name: 'Trainer',
                description: 'Helps improve aim and mechanical skills',
                specialties: ['aim', 'mechanics', 'reflexes'],
                style: 'motivational',
                avatar: 'trainer_avatar'
            },
            {
                id: 'coach_mentor',
                name: 'Mentor',
                description: 'Provides general guidance and support',
                specialties: ['mindset', 'communication', 'teamwork'],
                style: 'supportive',
                avatar: 'mentor_avatar'
            }
        ];

        for (const coach of coaches) {
            this.coaches.set(coach.id, new Coach(coach));
        }
    }

    initializeTipDatabase() {
        const tips = [
            {
                id: 'tip_crosshair_placement',
                category: 'aim',
                title: 'Crosshair Placement',
                content: 'Keep your crosshair at head height when moving. Pre-aim common angles before peeking.',
                priority: 'high',
                difficulty: 'beginner',
                tags: ['aim', 'positioning', 'technique']
            },
            {
                id: 'tip_movement',
                category: 'movement',
                title: 'Counter-Strafing',
                content: 'Release movement keys before shooting to achieve accurate shots. Practice counter-strafing in aim training maps.',
                priority: 'high',
                difficulty: 'intermediate',
                tags: ['movement', 'shooting', 'technique']
            },
            {
                id: 'tip_economy',
                category: 'game_sense',
                title: 'Economy Management',
                content: 'Track enemy money and plan your buy round accordingly. Eco rounds can set up force buys for maximum impact.',
                priority: 'high',
                difficulty: 'intermediate',
                tags: ['economy', 'game_sense', 'strategy']
            },
            {
                id: 'tip_utility',
                category: 'utility',
                title: 'Utility Usage',
                content: 'Throw smokes to block sightlines, not just for aesthetics. Flash before entering angles.',
                priority: 'medium',
                difficulty: 'intermediate',
                tags: ['utility', 'tactics', 'positioning']
            },
            {
                id: 'tip_positioning',
                category: 'positioning',
                title: 'Off-angle Positions',
                content: 'Take unexpected angles to catch enemies off guard. Don\'t always play the same spots.',
                priority: 'medium',
                difficulty: 'advanced',
                tags: ['positioning', 'tactics', 'surprise']
            },
            {
                id: 'tip_communication',
                category: 'teamwork',
                title: 'Callouts',
                content: 'Give precise callouts: distance, position, number of enemies. "Two mid going B short" is better than "they\'re moving".',
                priority: 'high',
                difficulty: 'beginner',
                tags: ['communication', 'teamwork', 'information']
            },
            {
                id: 'tip_aim_training',
                category: 'aim',
                title: 'Aim Routine',
                content: 'Practice 15-30 minutes daily on aim training maps. Focus on tracking and flicking separately.',
                priority: 'high',
                difficulty: 'beginner',
                tags: ['aim', 'practice', 'mechanics']
            },
            {
                id: 'tip_peeking',
                category: 'movement',
                title: 'Wide Peeking',
                content: 'Use wide peeks for trades and narrow peeks for taking duels. Match your peek style to the situation.',
                priority: 'medium',
                difficulty: 'intermediate',
                tags: ['movement', 'peeking', 'technique']
            },
            {
                id: 'tip_recoil',
                category: 'weapons',
                title: 'Recoil Control',
                content: 'Learn spray patterns for your primary weapons. Practice spray transfers between targets.',
                priority: 'high',
                difficulty: 'intermediate',
                tags: ['weapons', 'recoil', 'technique']
            },
            {
                id: 'tip_timing',
                category: 'game_sense',
                title: 'Round Timing',
                content: 'Track the clock. Attack sites before defenses can fully set up. Be aware of retake timings.',
                priority: 'medium',
                difficulty: 'advanced',
                tags: ['game_sense', 'timing', 'tactics']
            }
        ];

        for (const tip of tips) {
            this.tips.set(tip.id, tip);
        }
    }

    createPlayerProfile(playerId, data = {}) {
        const profile = {
            playerId,
            skillLevel: data.skillLevel || 1,
            rank: data.rank || 'silver',
            playtime: data.playtime || 0,
            strengths: data.strengths || [],
            weaknesses: data.weaknesses || [],
            preferredRoles: data.preferredRoles || [],
            preferredWeapons: data.preferredWeapons || [],
            goals: data.goals || [],
            metrics: {
                aim: 0,
                gameSense: 0,
                teamwork: 0,
                utility: 0,
                mechanics: 0,
                clutch: 0,
                communication: 0
            },
            history: [],
            recommendations: [],
            createdAt: Date.now(),
            lastUpdated: Date.now()
        };

        this.playerProfiles.set(playerId, profile);
        return profile;
    }

    analyzePlayer(playerId, matchData) {
        const profile = this.getOrCreateProfile(playerId);

        const analysis = {
            id: 'analysis_' + Date.now(),
            playerId,
            matchId: matchData.id,
            timestamp: Date.now(),
            statistics: this.calculateStatistics(matchData),
            strengths: this.identifyStrengths(matchData),
            weaknesses: this.identifyWeaknesses(matchData),
            recommendations: [],
            grade: this.calculateGrade(matchData)
        };

        analysis.recommendations = this.generateRecommendations(analysis);

        profile.history.push(analysis);
        profile.lastUpdated = Date.now();

        this.updateMetrics(profile, analysis);

        this.matchAnalyses.set(analysis.id, analysis);

        return analysis;
    }

    calculateStatistics(matchData) {
        const stats = {
            kills: matchData.kills || 0,
            deaths: matchData.deaths || 0,
            assists: matchData.assists || 0,
            damage: matchData.damage || 0,
            headshots: matchData.headshots || 0,
            mvps: matchData.mvps || 0,
            score: matchData.score || 0,
            kd: matchData.deaths > 0 ? (matchData.kills / matchData.deaths).toFixed(2) : matchData.kills,
            hsRate: matchData.kills > 0 ? ((matchData.headshots / matchData.kills) * 100).toFixed(1) : 0,
            adr: matchData.rounds > 0 ? (matchData.damage / matchData.rounds).toFixed(1) : 0,
            utilityDamage: matchData.utilityDamage || 0,
            flashAssists: matchData.flashAssists || 0,
            plants: matchData.plants || 0,
            defusals: matchData.defusals || 0,
            clutchAttempts: matchData.clutchAttempts || 0,
            clutchWins: matchData.clutchWins || 0,
            entryKills: matchData.entryKills || 0,
            tradeKills: matchData.tradeKills || 0,
            openingDuels: matchData.openingDuels || 0,
            openingWinRate: matchData.openingDuels > 0 ? ((matchData.entryKills / matchData.openingDuels) * 100).toFixed(1) : 0
        };

        return stats;
    }

    identifyStrengths(matchData) {
        const strengths = [];
        const stats = this.calculateStatistics(matchData);

        if (parseFloat(stats.kd) > 1.5) {
            strengths.push({
                category: 'combat',
                description: 'High K/D ratio',
                value: stats.kd
            });
        }

        if (parseFloat(stats.hsRate) > 40) {
            strengths.push({
                category: 'aim',
                description: 'Excellent headshot percentage',
                value: stats.hsRate + '%'
            });
        }

        if (parseFloat(stats.adr) > 90) {
            strengths.push({
                category: 'damage',
                description: 'High average damage per round',
                value: stats.adr
            });
        }

        if (stats.clutchWins > 0) {
            strengths.push({
                category: 'clutch',
                description: 'Clutch play ability',
                value: stats.clutchWins + ' clutches won'
            });
        }

        if (parseFloat(stats.openingWinRate) > 60) {
            strengths.push({
                category: 'aggression',
                description: 'Strong entry fragging',
                value: stats.openingWinRate + '% opening win rate'
            });
        }

        return strengths;
    }

    identifyWeaknesses(matchData) {
        const weaknesses = [];
        const stats = this.calculateStatistics(matchData);

        if (parseFloat(stats.kd) < 0.8) {
            weaknesses.push({
                category: 'combat',
                description: 'K/D ratio needs improvement',
                value: stats.kd,
                priority: 'high'
            });
        }

        if (parseFloat(stats.hsRate) < 20) {
            weaknesses.push({
                category: 'aim',
                description: 'Focus on aim training',
                value: stats.hsRate + '% HS rate',
                priority: 'medium'
            });
        }

        if (stats.flashAssists < 1) {
            weaknesses.push({
                category: 'utility',
                description: 'Use more utility to support team',
                value: stats.flashAssists + ' flash assists',
                priority: 'medium'
            });
        }

        if (stats.clutchAttempts > 0 && stats.clutchWins === 0) {
            weaknesses.push({
                category: 'clutch',
                description: 'Work on clutch situations',
                value: stats.clutchAttempts + ' clutch attempts',
                priority: 'high'
            });
        }

        return weaknesses;
    }

    generateRecommendations(analysis) {
        const recommendations = [];

        for (const weakness of analysis.weaknesses) {
            const relevantTips = this.findRelevantTips(weakness.category);
            recommendations.push({
                weakness: weakness.description,
                priority: weakness.priority,
                tips: relevantTips.slice(0, 2),
                practice: this.generatePracticePlan(weakness)
            });
        }

        recommendations.sort((a, b) => {
            const priorityOrder = { high: 0, medium: 1, low: 2 };
            return priorityOrder[a.priority] - priorityOrder[b.priority];
        });

        return recommendations;
    }

    findRelevantTips(category) {
        const relevantTips = [];

        for (const [id, tip] of this.tips) {
            if (tip.category === category || tip.tags.includes(category)) {
                relevantTips.push(tip);
            }
        }

        return relevantTips;
    }

    generatePracticePlan(weakness) {
        const plans = {
            combat: {
                duration: '30 minutes',
                activities: [
                    'Aim training map: 15 minutes',
                    'Deathmatch: 15 minutes',
                    'Focus on crosshair placement'
                ],
                resource: 'aim_training_map'
            },
            aim: {
                duration: '45 minutes',
                activities: [
                    'Warmup routine: 10 minutes',
                    'Aim botz map: 15 minutes',
                    'Flick training: 15 minutes',
                    'Review replays: 5 minutes'
                ],
                resource: 'aim_training_map'
            },
            utility: {
                duration: '20 minutes',
                activities: [
                    'Learn smoke lineups',
                    'Practice flash throws',
                    'Study molotov spreads'
                ],
                resource: 'utility_training_map'
            },
            clutch: {
                duration: '15 minutes',
                activities: [
                    'Watch clutch POVs',
                    'Practice 1vX situations',
                    'Learn common clutch spots'
                ],
                resource: 'clutch_tutorial'
            },
            default: {
                duration: '30 minutes',
                activities: [
                    'Practice relevant skills',
                    'Watch tutorial videos',
                    'Review personal replays'
                ],
                resource: 'general_training'
            }
        };

        return plans[weakness.category] || plans.default;
    }

    calculateGrade(matchData) {
        const stats = this.calculateStatistics(matchData);
        let grade = 'C';
        let score = 0;

        score += Math.min(30, parseFloat(stats.kd) * 15);
        score += Math.min(25, parseFloat(stats.hsRate) * 0.5);
        score += Math.min(20, parseFloat(stats.adr) * 0.2);
        score += Math.min(15, matchData.mvps * 5);
        score += Math.min(10, matchData.assists * 2);

        if (score >= 90) grade = 'S+';
        else if (score >= 80) grade = 'S';
        else if (score >= 70) grade = 'A';
        else if (score >= 60) grade = 'B';
        else if (score >= 50) grade = 'C';
        else grade = 'D';

        return {
            letter: grade,
            score: Math.round(score),
            breakdown: {
                kd: Math.min(30, parseFloat(stats.kd) * 15),
                hsRate: Math.min(25, parseFloat(stats.hsRate) * 0.5),
                adr: Math.min(20, parseFloat(stats.adr) * 0.2),
                mvps: Math.min(15, matchData.mvps * 5),
                assists: Math.min(10, matchData.assists * 2)
            }
        };
    }

    updateMetrics(profile, analysis) {
        const weight = 0.3;
        const current = profile.metrics;
        const newStats = analysis.statistics;

        current.aim = (current.aim * (1 - weight) + parseFloat(newStats.hsRate) * weight);
        current.mechanics = (current.mechanics * (1 - weight) + parseFloat(newStats.kd) * 20 * weight);
        current.gameSense = (current.gameSense * (1 - weight) + parseFloat(newStats.adr) * 0.3 * weight);

        if (analysis.statistics.clutchWins > 0) {
            current.clutch = (current.clutch * (1 - weight) + (analysis.statistics.clutchWins / analysis.statistics.clutchAttempts) * 100 * weight);
        }

        profile.metrics = current;
    }

    getOrCreateProfile(playerId) {
        if (!this.playerProfiles.has(playerId)) {
            this.createPlayerProfile(playerId);
        }
        return this.playerProfiles.get(playerId);
    }

    getCoach(coachId) {
        return this.coaches.get(coachId);
    }

    getCoachingSession(playerId, coachId) {
        const sessionId = `${playerId}_${coachId}`;
        return this.sessions.get(sessionId);
    }

    startCoachingSession(playerId, coachId) {
        const coach = this.coaches.get(coachId);
        if (!coach) return null;

        const session = {
            id: 'session_' + Date.now(),
            playerId,
            coachId,
            coachName: coach.name,
            status: 'active',
            startTime: Date.now(),
            tips: [],
            questions: [],
            feedback: [],
            milestones: [],
            duration: 0,
            progress: 0
        };

        const sessionId = `${playerId}_${coachId}`;
        this.sessions.set(sessionId, session);

        return session;
    }

    endCoachingSession(playerId, coachId) {
        const sessionId = `${playerId}_${coachId}`;
        const session = this.sessions.get(sessionId);

        if (session) {
            session.status = 'completed';
            session.endTime = Date.now();
            session.duration = session.endTime - session.startTime;
        }

        return session;
    }

    addTipToSession(sessionId, tipId) {
        const session = Array.from(this.sessions.values()).find(s => s.id === sessionId);
        if (!session) return false;

        const tip = this.tips.get(tipId);
        if (!tip) return false;

        session.tips.push({
            ...tip,
            addedAt: Date.now()
        });

        session.progress = Math.min(100, (session.tips.length / 5) * 100);

        return true;
    }

    getTipOfTheDay(playerId) {
        const profile = this.getOrCreateProfile(playerId);
        const skillLevel = profile.skillLevel;

        const relevantTips = Array.from(this.tips.values())
            .filter(tip => {
                const difficultyMap = { beginner: 1, intermediate: 2, advanced: 3 };
                return difficultyMap[tip.difficulty] <= skillLevel;
            });

        if (relevantTips.length === 0) {
            return Array.from(this.tips.values())[0];
        }

        const dayOfYear = Math.floor((Date.now() - new Date(new Date().getFullYear(), 0, 0)) / (1000 * 60 * 60 * 24));
        const index = dayOfYear % relevantTips.length;

        return relevantTips[index];
    }

    getPersonalizedTips(playerId, limit = 5) {
        const profile = this.getOrCreateProfile(playerId);
        const tips = [];

        for (const weakness of profile.weaknesses) {
            const relevantTips = this.findRelevantTips(weakness.category);
            tips.push(...relevantTips.map(tip => ({
                ...tip,
                relevance: 'weakness',
                weakness: weakness.description
            })));
        }

        for (const strength of profile.strengths) {
            const relevantTips = this.findRelevantTips(strength.category);
            tips.push(...relevantTips.map(tip => ({
                ...tip,
                relevance: 'strengthen'
            })));
        }

        const uniqueTips = [];
        const seen = new Set();
        for (const tip of tips) {
            if (!seen.has(tip.id)) {
                seen.add(tip.id);
                uniqueTips.push(tip);
            }
        }

        return uniqueTips.slice(0, limit);
    }

    createTraining(playerId, trainingData) {
        const training = {
            id: 'training_' + Date.now(),
            playerId,
            name: trainingData.name,
            type: trainingData.type,
            exercises: trainingData.exercises || [],
            duration: trainingData.duration || 30 * 60 * 1000,
            difficulty: trainingData.difficulty || 'intermediate',
            status: 'scheduled',
            schedule: trainingData.schedule,
            progress: 0,
            completedExercises: [],
            results: [],
            createdAt: Date.now()
        };

        this.trainings.set(training.id, training);
        return training;
    }

    completeExercise(trainingId, exerciseId, result) {
        const training = this.trainings.get(trainingId);
        if (!training) return false;

        training.completedExercises.push({
            exerciseId,
            completedAt: Date.now(),
            result
        });

        training.progress = (training.completedExercises.length / training.exercises.length) * 100;

        if (training.progress >= 100) {
            training.status = 'completed';
        }

        return true;
    }

    getPlayerProgress(playerId) {
        const profile = this.getOrCreateProfile(playerId);
        const recentAnalyses = profile.history.slice(-10);
        const sessions = Array.from(this.sessions.values())
            .filter(s => s.playerId === playerId);

        const progress = {
            playerId,
            metrics: profile.metrics,
            recentGrade: recentAnalyses.length > 0 ? recentAnalyses[recentAnalyses.length - 1].grade : null,
            improvement: this.calculateImprovement(recentAnalyses),
            totalSessions: sessions.length,
            totalTrainingTime: sessions.reduce((sum, s) => sum + (s.duration || 0), 0),
            tipsCompleted: sessions.reduce((sum, s) => sum + s.tips.length, 0)
        };

        return progress;
    }

    calculateImprovement(analyses) {
        if (analyses.length < 2) {
            return { improved: false, message: 'Not enough data to calculate improvement' };
        }

        const oldest = analyses[0];
        const newest = analyses[analyses.length - 1];

        const oldGrade = oldest.grade.score;
        const newGrade = newest.grade.score;

        const improvement = newGrade - oldGrade;

        return {
            improved: improvement > 5,
            change: improvement,
            message: improvement > 5 ? 'Great progress!' : improvement > 0 ? 'Showing improvement' : 'Keep practicing'
        };
    }

    generateWeeklyPlan(playerId) {
        const profile = this.getOrCreateProfile(playerId);
        const tips = this.getPersonalizedTips(playerId, 3);

        const plan = {
            monday: {
                focus: 'Aim Training',
                duration: '45 minutes',
                exercises: [
                    'Warmup routine: 10 min',
                    'Aim botz: 15 min',
                    'Flick training: 15 min',
                    'Deathmatch: 15 min'
                ],
                tips: tips.filter(t => t.category === 'aim')
            },
            tuesday: {
                focus: 'Utility Usage',
                duration: '30 minutes',
                exercises: [
                    'Learn smoke lineups: 10 min',
                    'Flash throw practice: 10 min',
                    'Utility in casual: 10 min'
                ],
                tips: tips.filter(t => t.category === 'utility')
            },
            wednesday: {
                focus: 'Game Sense',
                duration: '30 minutes',
                exercises: [
                    'Watch pro VODs: 15 min',
                    'Review personal replays: 10 min',
                    'Study common strategies: 5 min'
                ],
                tips: tips.filter(t => t.category === 'game_sense')
            },
            thursday: {
                focus: 'Practice Match',
                duration: '60 minutes',
                exercises: [
                    'Competitive match',
                    'Focus on applying learned tips',
                    'Review performance after'
                ],
                tips: []
            },
            friday: {
                focus: 'Movement & Positioning',
                duration: '30 minutes',
                exercises: [
                    'Movement maps: 15 min',
                    'Positioning practice: 15 min'
                ],
                tips: tips.filter(t => t.category === 'positioning')
            },
            saturday: {
                focus: 'Full Practice Session',
                duration: '90 minutes',
                exercises: [
                    'Aim training: 20 min',
                    'Utility practice: 20 min',
                    'Competitive match: 45 min',
                    'Review: 5 min'
                ],
                tips: []
            },
            sunday: {
                focus: 'Rest & Review',
                duration: '30 minutes',
                exercises: [
                    'Watch educational content',
                    'Plan for next week',
                    'Mental rest'
                ],
                tips: []
            }
        };

        return plan;
    }

    getMatchAnalysis(analysisId) {
        return this.matchAnalyses.get(analysisId);
    }

    getTraining(trainingId) {
        return this.trainings.get(trainingId);
    }

    serialize() {
        return {
            coaches: Array.from(this.coaches.entries()),
            playerProfiles: Array.from(this.playerProfiles.entries()),
            matchAnalyses: Array.from(this.matchAnalyses.entries()),
            tips: Array.from(this.tips.entries()),
            trainings: Array.from(this.trainings.entries()),
            sessions: Array.from(this.sessions.entries())
        };
    }
}

class Coach {
    constructor(data) {
        this.id = data.id;
        this.name = data.name;
        this.description = data.description;
        this.specialties = data.specialties;
        this.style = data.style;
        this.avatar = data.avatar;
        this.personality = this.generatePersonality();
    }

    generatePersonality() {
        const personalities = {
            analytical: {
                communicationStyle: 'detailed',
                feedbackStyle: 'data-driven',
                encouragement: 'moderate'
            },
            motivational: {
                communicationStyle: 'energetic',
                feedbackStyle: 'positive',
                encouragement: 'high'
            },
            supportive: {
                communicationStyle: 'patient',
                feedbackStyle: 'constructive',
                encouragement: 'high'
            }
        };

        return personalities[this.style] || personalities.supportive;
    }

    provideFeedback(context) {
        return {
            message: this.generateFeedbackMessage(context),
            suggestions: this.generateSuggestions(context)
        };
    }

    generateFeedbackMessage(context) {
        const templates = {
            good: [
                'Excellent work on {aspect}!',
                'Your {aspect} has improved significantly.',
                'Great job with {aspect}!'
            ],
            improvement: [
                'Focus on improving your {aspect}.',
                'Consider working on {aspect} more.',
                'Your {aspect} could use some attention.'
            ]
        };

        const aspects = Object.keys(context);
        const aspect = aspects[Math.floor(Math.random() * aspects.length)];
        const value = context[aspect];

        const templateSet = value > 0 ? templates.good : templates.improvement;
        const template = templateSet[Math.floor(Math.random() * templateSet.length)];

        return template.replace('{aspect}', aspect);
    }

    generateSuggestions(context) {
        const suggestions = [];

        for (const [aspect, value] of Object.entries(context)) {
            if (value < 0) {
                suggestions.push({
                    aspect,
                    action: `Practice ${aspect}`,
                    resource: this.getResourceForAspect(aspect)
                });
            }
        }

        return suggestions;
    }

    getResourceForAspect(aspect) {
        const resources = {
            aim: 'aim_training_map',
            utility: 'utility_training_map',
            movement: 'movement_training_map',
            positioning: 'positioning_tutorial',
            game_sense: 'game_sense_guide'
        };

        return resources[aspect] || 'general_training';
    }
}

class CoachAnalytics {
    constructor() {
        this.coachingSessions = [];
        this.improvementData = new Map();
        this.popularTips = new Map();
    }

    recordSession(session) {
        this.coachingSessions.push(session);
    }

    recordTipView(tipId) {
        const count = this.popularTips.get(tipId) || 0;
        this.popularTips.set(tipId, count + 1);
    }

    getTopTips(limit = 10) {
        return Array.from(this.popularTips.entries())
            .sort((a, b) => b[1] - a[1])
            .slice(0, limit)
            .map(([tipId, views]) => ({ tipId, views }));
    }

    getImprovementTrend(playerId) {
        return this.improvementData.get(playerId) || [];
    }
}

export default {
    AICoachSystem,
    Coach,
    CoachAnalytics
};
