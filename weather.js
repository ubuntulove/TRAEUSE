export class WeatherSystem {
    constructor(config = {}) {
        this.id = config.id || WeatherSystem.nextId++;
        this.name = config.name || 'Weather System';
        this.currentWeather = null;
        this.weatherPresets = new Map();
        this.transitionDuration = config.transitionDuration || 5000;
        this.transitionProgress = 1;
        this.isTransitioning = false;
        this.transitionStart = null;
        this.startWeather = null;
        this.targetWeather = null;
        this.globalEnabled = config.enabled !== undefined ? config.enabled : true;
        this.timeOfDay = config.timeOfDay || 'day';
        this.season = config.season || 'summer';
        this.temperature = config.temperature || 20;
        this.windDirection = config.windDirection || 0;
        this.windSpeed = config.windSpeed || 0;
        this.precipitation = config.precipitation || 0;
        this.fogDensity = config.fogDensity || 0;
        this.cloudCoverage = config.cloudCoverage || 0;
        this.visibility = config.visibility || 10000;
        this.effects = [];
        this.particles = [];
        this.sounds = new Map();
        this.ambientSounds = [];
        this.onWeatherChange = null;
        this.onTransitionStart = null;
        this.onTransitionEnd = null;
        this.initializePresets();
    }

    static get nextId() {
        return WeatherSystem._nextId++;
    }

    initializePresets() {
        this.addPreset(new WeatherPreset({
            id: 'clear',
            name: 'Clear Sky',
            description: 'Clear weather with no precipitation',
            cloudCoverage: 0,
            fogDensity: 0,
            precipitation: 0,
            windSpeed: 5,
            visibility: 10000,
            ambientSounds: ['birds_chirping', 'wind_light'],
            particleEffects: []
        }));

        this.addPreset(new WeatherPreset({
            id: 'cloudy',
            name: 'Cloudy',
            description: 'Overcast sky with clouds',
            cloudCoverage: 0.7,
            fogDensity: 0.1,
            precipitation: 0,
            windSpeed: 10,
            visibility: 8000,
            ambientSounds: ['wind_medium', 'clouds_rolling'],
            particleEffects: []
        }));

        this.addPreset(new WeatherPreset({
            id: 'rain_light',
            name: 'Light Rain',
            description: 'Light rainfall',
            cloudCoverage: 0.8,
            fogDensity: 0.2,
            precipitation: 0.3,
            windSpeed: 15,
            visibility: 5000,
            ambientSounds: ['rain_light', 'wind_rain'],
            particleEffects: ['rain_light']
        }));

        this.addPreset(new WeatherPreset({
            id: 'rain_heavy',
            name: 'Heavy Rain',
            description: 'Heavy rainfall with reduced visibility',
            cloudCoverage: 1,
            fogDensity: 0.5,
            precipitation: 1,
            windSpeed: 25,
            visibility: 2000,
            ambientSounds: ['rain_heavy', 'thunder_distant'],
            particleEffects: ['rain_heavy', 'puddles']
        }));

        this.addPreset(new WeatherPreset({
            id: 'thunderstorm',
            name: 'Thunderstorm',
            description: 'Thunderstorm with lightning',
            cloudCoverage: 1,
            fogDensity: 0.6,
            precipitation: 1,
            windSpeed: 30,
            visibility: 1500,
            ambientSounds: ['thunder_close', 'rain_heavy', 'wind_storm'],
            particleEffects: ['rain_heavy', 'lightning_flashes', 'puddles']
        }));

        this.addPreset(new WeatherPreset({
            id: 'snow_light',
            name: 'Light Snow',
            description: 'Light snowfall',
            cloudCoverage: 0.9,
            fogDensity: 0.3,
            precipitation: 0.4,
            windSpeed: 10,
            visibility: 3000,
            temperature: -5,
            ambientSounds: ['snow_light', 'wind_cold'],
            particleEffects: ['snow_light']
        }));

        this.addPreset(new WeatherPreset({
            id: 'snow_heavy',
            name: 'Heavy Snow',
            description: 'Heavy snowfall with reduced visibility',
            cloudCoverage: 1,
            fogDensity: 0.7,
            precipitation: 1,
            windSpeed: 20,
            visibility: 500,
            temperature: -15,
            ambientSounds: ['snow_heavy', 'wind_cold'],
            particleEffects: ['snow_heavy', 'snow_ground']
        }));

        this.addPreset(new WeatherPreset({
            id: 'fog',
            name: 'Foggy',
            description: 'Dense fog',
            cloudCoverage: 0.5,
            fogDensity: 0.9,
            precipitation: 0,
            windSpeed: 3,
            visibility: 500,
            ambientSounds: ['fog_ambient', 'wind_light'],
            particleEffects: ['fog_layer']
        }));

        this.addPreset(new WeatherPreset({
            id: 'sandstorm',
            name: 'Sandstorm',
            description: 'Sandstorm with high winds',
            cloudCoverage: 0.6,
            fogDensity: 0.8,
            precipitation: 0.5,
            windSpeed: 50,
            visibility: 200,
            temperature: 35,
            ambientSounds: ['sand_wind', 'sand_impact'],
            particleEffects: ['sand_particles', 'dust_devils']
        }));
    }

    addPreset(preset) {
        this.weatherPresets.set(preset.id, preset);
    }

    getPreset(presetId) {
        return this.weatherPresets.get(presetId);
    }

    setWeather(presetId, config = {}) {
        const preset = this.weatherPresets.get(presetId);
        if (!preset) {
            console.warn(`Weather preset ${presetId} not found`);
            return false;
        }

        if (this.transitionDuration > 0 && this.currentWeather) {
            this.startTransition(preset, config);
        } else {
            this.applyWeather(preset, config);
        }

        return true;
    }

    applyWeather(preset, config = {}) {
        this.currentWeather = preset.clone();
        this.transitionProgress = 1;
        this.isTransitioning = false;

        this.cloudCoverage = config.cloudCoverage ?? preset.cloudCoverage;
        this.fogDensity = config.fogDensity ?? preset.fogDensity;
        this.precipitation = config.precipitation ?? preset.precipitation;
        this.windSpeed = config.windSpeed ?? preset.windSpeed;
        this.windDirection = config.windDirection ?? this.windDirection;
        this.visibility = config.visibility ?? preset.visibility;
        this.temperature = config.temperature ?? preset.temperature;

        this.updateEffects();
        this.updateAmbientSounds();

        if (this.onWeatherChange) {
            this.onWeatherChange(this.currentWeather);
        }
    }

    startTransition(preset, config = {}) {
        this.startWeather = this.currentWeather ? this.currentWeather.clone() : null;
        this.targetWeather = preset.clone();
        this.isTransitioning = true;
        this.transitionStart = Date.now();
        this.transitionProgress = 0;

        this.targetWeatherConfig = config;

        if (this.onTransitionStart) {
            this.onTransitionStart(this.startWeather, this.targetWeather);
        }
    }

    update(dt) {
        if (!this.globalEnabled) return;

        if (this.isTransitioning) {
            const elapsed = Date.now() - this.transitionStart;
            this.transitionProgress = Math.min(1, elapsed / this.transitionDuration);

            if (this.transitionProgress >= 1) {
                this.completeTransition();
            } else {
                this.updateTransition();
            }
        }

        this.updateParticles(dt);
        this.updateEffects();
    }

    updateTransition() {
        const t = this.easeInOutCubic(this.transitionProgress);

        if (this.startWeather) {
            this.cloudCoverage = this.lerp(this.startWeather.cloudCoverage, this.targetWeather.cloudCoverage, t);
            this.fogDensity = this.lerp(this.startWeather.fogDensity, this.targetWeather.fogDensity, t);
            this.precipitation = this.lerp(this.startWeather.precipitation, this.targetWeather.precipitation, t);
            this.windSpeed = this.lerp(this.startWeather.windSpeed, this.targetWeather.windSpeed, t);
            this.visibility = this.lerp(this.startWeather.visibility, this.targetWeather.visibility, t);
            this.temperature = this.lerp(this.startWeather.temperature, this.targetWeather.temperature, t);
        } else {
            this.cloudCoverage = this.targetWeather.cloudCoverage * t;
            this.fogDensity = this.targetWeather.fogDensity * t;
            this.precipitation = this.targetWeather.precipitation * t;
            this.windSpeed = this.targetWeather.windSpeed * t;
            this.visibility = this.targetWeather.visibility * t;
            this.temperature = this.targetWeather.temperature * t;
        }
    }

    completeTransition() {
        this.applyWeather(this.targetWeather, this.targetWeatherConfig);
        this.isTransitioning = false;

        if (this.onTransitionEnd) {
            this.onTransitionEnd(this.currentWeather);
        }
    }

    updateEffects() {
        this.effects = [];

        if (this.precipitation > 0) {
            this.effects.push({
                type: 'precipitation',
                intensity: this.precipitation,
                particlesPerSecond: Math.floor(1000 * this.precipitation)
            });
        }

        if (this.fogDensity > 0) {
            this.effects.push({
                type: 'fog',
                density: this.fogDensity,
                near: 10 * (1 - this.fogDensity),
                far: this.visibility
            });
        }

        if (this.cloudCoverage > 0) {
            this.effects.push({
                type: 'clouds',
                coverage: this.cloudCoverage,
                color: this.getCloudColor()
            });
        }

        if (this.windSpeed > 20) {
            this.effects.push({
                type: 'wind',
                speed: this.windSpeed,
                direction: this.windDirection,
                intensity: Math.min(1, (this.windSpeed - 20) / 30)
            });
        }
    }

    updateParticles(dt) {
        for (const particle of this.particles) {
            particle.position.add(particle.velocity.clone().multiply(dt));
            particle.life -= dt;
            particle.velocity.add(particle.acceleration.clone().multiply(dt));

            const windForce = new Vector3(
                Math.cos(this.windDirection) * this.windSpeed * 0.1,
                particle.type === 'rain' || particle.type === 'snow' ? -particle.fallSpeed : 0,
                Math.sin(this.windDirection) * this.windSpeed * 0.1
            );
            particle.velocity.add(windForce.multiply(dt));
        }

        this.particles = this.particles.filter(p => p.life > 0);

        if (this.precipitation > 0 && this.particles.length < 1000 * this.precipitation) {
            this.spawnPrecipitationParticle();
        }
    }

    spawnPrecipitationParticle() {
        const isSnow = this.temperature < 2;
        const particle = {
            type: isSnow ? 'snow' : 'rain',
            position: new Vector3(
                Math.random() * 100 - 50,
                20 + Math.random() * 10,
                Math.random() * 100 - 50
            ),
            velocity: new Vector3(),
            acceleration: new Vector3(),
            fallSpeed: isSnow ? 2 + Math.random() * 2 : 20 + Math.random() * 10,
            size: isSnow ? 0.1 + Math.random() * 0.1 : 0.02 + Math.random() * 0.02,
            life: 3 + Math.random() * 2,
            rotation: Math.random() * Math.PI * 2
        };

        this.particles.push(particle);
    }

    updateAmbientSounds() {
        if (!this.currentWeather) return;

        for (const soundId of this.ambientSounds) {
            if (!this.sounds.has(soundId)) {
                this.sounds.set(soundId, { volume: 0, soundId });
            }
        }

        for (const [soundId, sound] of this.sounds) {
            if (this.currentWeather.ambientSounds.includes(soundId)) {
                sound.volume = Math.min(1, sound.volume + 0.1);
            } else {
                sound.volume = Math.max(0, sound.volume - 0.1);
            }
        }
    }

    getCloudColor() {
        const time = this.timeOfDay;
        if (time === 'night') {
            return new Color(0.05, 0.05, 0.1);
        } else if (time === 'sunset' || time === 'sunrise') {
            return new Color(0.8, 0.4, 0.2);
        } else {
            return new Color(0.8, 0.8, 0.85);
        }
    }

    setTimeOfDay(time) {
        this.timeOfDay = time;
        this.updateEffects();
    }

    setSeason(season) {
        this.season = season;
        this.updateEffects();
    }

    setWind(direction, speed) {
        this.windDirection = direction;
        this.windSpeed = speed;
    }

    lerp(a, b, t) {
        return a + (b - a) * t;
    }

    easeInOutCubic(t) {
        return t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;
    }

    getCurrentConditions() {
        return {
            weather: this.currentWeather ? this.currentWeather.name : 'None',
            temperature: this.temperature,
            windSpeed: this.windSpeed,
            windDirection: this.windDirection,
            cloudCoverage: this.cloudCoverage,
            fogDensity: this.fogDensity,
            precipitation: this.precipitation,
            visibility: this.visibility,
            timeOfDay: this.timeOfDay,
            season: this.season,
            isTransitioning: this.isTransitioning,
            transitionProgress: this.transitionProgress,
            effects: this.effects,
            particleCount: this.particles.length
        };
    }

    saveState() {
        return {
            currentWeatherId: this.currentWeather?.id,
            timeOfDay: this.timeOfDay,
            season: this.season,
            temperature: this.temperature,
            windDirection: this.windDirection,
            windSpeed: this.windSpeed
        };
    }

    loadState(state) {
        if (state.currentWeatherId) {
            this.setWeather(state.currentWeatherId);
        }
        this.timeOfDay = state.timeOfDay;
        this.season = state.season;
        this.temperature = state.temperature;
        this.windDirection = state.windDirection;
        this.windSpeed = state.windSpeed;
    }
}

WeatherSystem._nextId = 0;

export class WeatherPreset {
    constructor(config = {}) {
        this.id = config.id || '';
        this.name = config.name || 'Weather';
        this.description = config.description || '';
        this.cloudCoverage = config.cloudCoverage || 0;
        this.fogDensity = config.fogDensity || 0;
        this.precipitation = config.precipitation || 0;
        this.windSpeed = config.windSpeed || 0;
        this.visibility = config.visibility || 10000;
        this.temperature = config.temperature || 20;
        this.ambientSounds = config.ambientSounds || [];
        this.particleEffects = config.particleEffects || [];
        this.skyColor = config.skyColor || new Color(0.5, 0.7, 1);
        this.groundColor = config.groundColor || new Color(0.2, 0.3, 0.1);
        this.fogColor = config.fogColor || new Color(0.8, 0.8, 0.9);
        this.lightingIntensity = config.lightingIntensity || 1;
        this.particleDensity = config.particleDensity || 100;
    }

    clone() {
        return new WeatherPreset({
            id: this.id,
            name: this.name,
            description: this.description,
            cloudCoverage: this.cloudCoverage,
            fogDensity: this.fogDensity,
            precipitation: this.precipitation,
            windSpeed: this.windSpeed,
            visibility: this.visibility,
            temperature: this.temperature,
            ambientSounds: [...this.ambientSounds],
            particleEffects: [...this.particleEffects],
            skyColor: this.skyColor.clone(),
            groundColor: this.groundColor.clone(),
            fogColor: this.fogColor.clone(),
            lightingIntensity: this.lightingIntensity,
            particleDensity: this.particleDensity
        });
    }
}

export class TimeOfDaySystem {
    constructor(config = {}) {
        this.cycleDuration = config.cycleDuration || 600000;
        this.currentTime = config.startTime || 0;
        this.isPaused = false;
        this.timeMultiplier = config.timeMultiplier || 1;
        this.sunrise = 6;
        this.sunset = 18;
        this.moonrise = 18;
        this.moonset = 6;
        this.lighting = this.calculateLighting();
        this.skyboxColors = this.calculateSkyboxColors();
        this.onTimeChange = null;
    }

    update(dt) {
        if (this.isPaused) return;

        this.currentTime += dt * this.timeMultiplier * 1000;

        if (this.currentTime >= 24) {
            this.currentTime -= 24;
        }

        this.lighting = this.calculateLighting();
        this.skyboxColors = this.calculateSkyboxColors();

        if (this.onTimeChange) {
            this.onTimeChange(this.getTimeData());
        }
    }

    calculateLighting() {
        const time = this.currentTime;

        let intensity = 0;
        let ambientIntensity = 0;

        if (time >= this.sunrise && time <= this.sunset) {
            const dayProgress = (time - this.sunrise) / (this.sunset - this.sunrise);
            intensity = Math.sin(dayProgress * Math.PI);
            ambientIntensity = 0.5 + 0.5 * intensity;
        } else {
            ambientIntensity = 0.1;
            intensity = 0;

            if (time >= this.sunset || time <= this.moonset) {
                const nightProgress = time >= this.sunset
                    ? (time - this.sunset) / (24 - this.sunset + this.moonset)
                    : (time + 24 - this.sunset) / (24 - this.sunset + this.moonset);
                ambientIntensity += 0.2 * Math.sin(nightProgress * Math.PI);
            }
        }

        return {
            sunIntensity: intensity,
            ambientIntensity: ambientIntensity,
            shadowIntensity: intensity * 0.5
        };
    }

    calculateSkyboxColors() {
        const time = this.currentTime;
        let skyColor, horizonColor, groundColor;

        if (time >= this.sunrise && time <= this.sunset) {
            const dayProgress = (time - this.sunrise) / (this.sunset - this.sunrise);
            const sunPosition = Math.sin(dayProgress * Math.PI);

            skyColor = new Color(
                0.4 + 0.2 * sunPosition,
                0.5 + 0.3 * sunPosition,
                0.7 + 0.2 * sunPosition
            );

            horizonColor = new Color(
                0.8 + 0.1 * sunPosition,
                0.6 + 0.2 * sunPosition,
                0.4 + 0.3 * sunPosition
            );

            groundColor = new Color(0.2, 0.25, 0.15);
        } else {
            const nightProgress = time >= this.sunset
                ? (time - this.sunset) / (24 - this.sunset + this.moonset)
                : (time + 24 - this.sunset) / (24 - this.sunset + this.moonset);

            skyColor = new Color(
                0.05 + 0.03 * Math.sin(nightProgress * Math.PI * 2),
                0.05 + 0.03 * Math.sin(nightProgress * Math.PI * 2),
                0.1 + 0.05 * Math.sin(nightProgress * Math.PI * 2)
            );

            horizonColor = new Color(0.15, 0.12, 0.2);
            groundColor = new Color(0.05, 0.05, 0.08);
        }

        return { skyColor, horizonColor, groundColor };
    }

    getTimeData() {
        const hours = Math.floor(this.currentTime);
        const minutes = Math.floor((this.currentTime - hours) * 60);
        const timeString = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;

        let period = 'day';
        if (this.currentTime >= this.sunrise && this.currentTime <= this.sunset) {
            period = 'day';
        } else if (this.currentTime >= this.sunset && this.currentTime <= 21) {
            period = 'sunset';
        } else if (this.currentTime >= 21 || this.currentTime <= 5) {
            period = 'night';
        } else {
            period = 'sunrise';
        }

        return {
            time: this.currentTime,
            timeString,
            hours,
            minutes,
            period,
            lighting: this.lighting,
            skyboxColors: this.skyboxColors,
            sunAngle: this.getSunAngle(),
            moonAngle: this.getMoonAngle()
        };
    }

    getSunAngle() {
        if (this.currentTime < this.sunrise || this.currentTime > this.sunset) {
            return -90;
        }

        const progress = (this.currentTime - this.sunrise) / (this.sunset - this.sunrise);
        return 90 - progress * 180;
    }

    getMoonAngle() {
        if (this.currentTime >= this.moonrise || this.currentTime <= this.moonset) {
            if (this.currentTime >= this.moonrise) {
                const progress = (this.currentTime - this.moonrise) / (24 - this.moonrise + this.moonset);
                return 90 - progress * 180;
            } else {
                const progress = (this.currentTime + 24 - this.moonrise) / (24 - this.moonrise + this.moonset);
                return 90 - progress * 180;
            }
        }
        return -90;
    }

    setTime(time) {
        this.currentTime = time % 24;
        this.lighting = this.calculateLighting();
        this.skyboxColors = this.calculateSkyboxColors();
    }

    pause() {
        this.isPaused = true;
    }

    resume() {
        this.isPaused = false;
    }

    setTimeMultiplier(multiplier) {
        this.timeMultiplier = multiplier;
    }
}

export class SeasonSystem {
    constructor(config = {}) {
        this.currentSeason = config.season || 'summer';
        this.seasonDuration = config.seasonDuration || 90 * 24 * 60 * 60 * 1000;
        this.seasonStartTime = config.seasonStartTime || Date.now();
        this.seasons = this.initializeSeasons();
        this.updateCurrentSeason();
    }

    initializeSeasons() {
        return {
            spring: {
                name: 'Spring',
                startMonth: 3,
                endMonth: 5,
                temperature: { min: 10, max: 20 },
                foliage: true,
                flowerSeason: true,
                colors: {
                    grass: new Color(0.4, 0.7, 0.3),
                    trees: new Color(0.3, 0.6, 0.2),
                    flowers: [new Color(1, 0.8, 0.2), new Color(1, 0.4, 0.6), new Color(0.6, 0.4, 1)]
                }
            },
            summer: {
                name: 'Summer',
                startMonth: 6,
                endMonth: 8,
                temperature: { min: 20, max: 35 },
                foliage: true,
                flowerSeason: false,
                colors: {
                    grass: new Color(0.3, 0.6, 0.2),
                    trees: new Color(0.2, 0.5, 0.15),
                    flowers: [new Color(1, 0.2, 0.2)]
                }
            },
            autumn: {
                name: 'Autumn',
                startMonth: 9,
                endMonth: 11,
                temperature: { min: 5, max: 15 },
                foliage: true,
                flowerSeason: false,
                colors: {
                    grass: new Color(0.5, 0.4, 0.2),
                    trees: new Color(0.7, 0.4, 0.1),
                    flowers: [new Color(0.8, 0.3, 0.1)]
                }
            },
            winter: {
                name: 'Winter',
                startMonth: 12,
                endMonth: 2,
                temperature: { min: -10, max: 5 },
                foliage: false,
                flowerSeason: false,
                snowPossible: true,
                colors: {
                    grass: new Color(0.8, 0.85, 0.9),
                    trees: new Color(0.3, 0.3, 0.35),
                    snow: new Color(0.95, 0.98, 1)
                }
            }
        };
    }

    updateCurrentSeason() {
        const now = new Date();
        const month = now.getMonth() + 1;

        for (const [key, season] of Object.entries(this.seasons)) {
            if (season.startMonth <= season.endMonth) {
                if (month >= season.startMonth && month <= season.endMonth) {
                    this.currentSeason = key;
                    return;
                }
            } else {
                if (month >= season.startMonth || month <= season.endMonth) {
                    this.currentSeason = key;
                    return;
                }
            }
        }
    }

    getSeasonData() {
        return {
            season: this.currentSeason,
            ...this.seasons[this.currentSeason],
            temperature: this.getCurrentTemperature()
        };
    }

    getCurrentTemperature() {
        const season = this.seasons[this.currentSeason];
        const now = new Date();
        const dayOfYear = Math.floor((now - new Date(now.getFullYear(), 0, 0)) / (1000 * 60 * 60 * 24));
        const seasonalProgress = Math.sin((dayOfYear / 365) * Math.PI * 2 - Math.PI / 2) * 0.5 + 0.5;

        const temp = season.temperature.min + (season.temperature.max - season.temperature.min) * seasonalProgress;

        const timeOfDay = now.getHours() + now.getMinutes() / 60;
        const dailyVariation = Math.sin((timeOfDay - 6) / 12 * Math.PI) * 5;

        return temp + dailyVariation;
    }
}

export class EnvironmentalHazardSystem {
    constructor(config = {}) {
        this.hazards = new Map();
        this.activeHazards = [];
        this.damagePerSecond = config.damagePerSecond || 10;
        this.hazardTypes = this.initializeHazardTypes();
        this.onHazardEnter = null;
        this.onHazardExit = null;
        this.onHazardTick = null;
    }

    initializeHazardTypes() {
        return {
            fire: {
                name: 'Fire',
                damage: 20,
                tickRate: 0.5,
                effects: ['burning', 'screen_fire'],
                sound: 'fire_loop'
            },
            acid: {
                name: 'Acid',
                damage: 15,
                tickRate: 0.5,
                effects: ['corroding'],
                sound: 'acid_drip'
            },
            electricity: {
                name: 'Electricity',
                damage: 30,
                tickRate: 0.2,
                effects: ['shocked'],
                sound: 'electric_zap'
            },
            radiation: {
                name: 'Radiation',
                damage: 5,
                tickRate: 1,
                effects: ['irradiated'],
                sound: 'radiation_hum'
            },
            biological: {
                name: 'Biological',
                damage: 8,
                tickRate: 1,
                effects: ['poisoned', 'screen_green'],
                sound: 'bio_warning'
            },
            temperature_hot: {
                name: 'Extreme Heat',
                damage: 2,
                tickRate: 1,
                effects: ['heat_stroke'],
                sound: 'heat_wind'
            },
            temperature_cold: {
                name: 'Extreme Cold',
                damage: 2,
                tickRate: 1,
                effects: ['frozen', 'screen_blue'],
                sound: 'wind_cold'
            }
        };
    }

    addHazard(config) {
        const hazard = {
            id: config.id || `hazard_${Date.now()}`,
            type: config.type,
            bounds: config.bounds,
            position: config.position || new Vector3(),
            radius: config.radius || 5,
            height: config.height || 10,
            damage: config.damage || this.hazardTypes[config.type]?.damage || 10,
            tickRate: config.tickRate || this.hazardTypes[config.type]?.tickRate || 1,
            effects: config.effects || this.hazardTypes[config.type]?.effects || [],
            duration: config.duration || null,
            startTime: Date.now(),
            isActive: true,
            owner: config.owner || null,
            metadata: config.metadata || {}
        };

        this.hazards.set(hazard.id, hazard);
        this.activeHazards.push(hazard);

        return hazard;
    }

    removeHazard(hazardId) {
        const hazard = this.hazards.get(hazardId);
        if (hazard) {
            const index = this.activeHazards.indexOf(hazard);
            if (index !== -1) {
                this.activeHazards.splice(index, 1);
            }
            this.hazards.delete(hazardId);
            return true;
        }
        return false;
    }

    getHazardsInArea(position, radius) {
        const inArea = [];

        for (const hazard of this.activeHazards) {
            if (this.isInHazardArea(position, hazard)) {
                inArea.push(hazard);
            }
        }

        return inArea;
    }

    isInHazardArea(position, hazard) {
        if (hazard.bounds) {
            return position.x >= hazard.bounds.min.x && position.x <= hazard.bounds.max.x &&
                   position.y >= hazard.bounds.min.y && position.y <= hazard.bounds.max.y &&
                   position.z >= hazard.bounds.min.z && position.z <= hazard.bounds.max.z;
        }

        if (hazard.radius) {
            const horizontalDist = Math.sqrt(
                Math.pow(position.x - hazard.position.x, 2) +
                Math.pow(position.z - hazard.position.z, 2)
            );
            const verticalDist = position.y - hazard.position.y;

            if (horizontalDist <= hazard.radius &&
                verticalDist >= 0 &&
                verticalDist <= hazard.height) {
                return true;
            }
        }

        return false;
    }

    update(playerPosition, entities) {
        for (const hazard of this.activeHazards) {
            if (!hazard.isActive) continue;

            if (hazard.duration && Date.now() - hazard.startTime > hazard.duration) {
                hazard.isActive = false;
                continue;
            }

            for (const entity of entities) {
                if (this.isInHazardArea(entity.position, hazard)) {
                    if (this.onHazardEnter) {
                        this.onHazardEnter(entity, hazard);
                    }

                    if (!entity.hazardTimers) {
                        entity.hazardTimers = new Map();
                    }

                    if (!entity.hazardTimers.has(hazard.id)) {
                        entity.hazardTimers.set(hazard.id, Date.now());
                    }

                    const timeInHazard = Date.now() - entity.hazardTimers.get(hazard.id);
                    const tickInterval = hazard.tickRate * 1000;

                    if (timeInHazard % tickInterval < 20) {
                        const damage = hazard.damage * hazard.tickRate;
                        entity.takeDamage(damage, hazard.type);

                        if (this.onHazardTick) {
                            this.onHazardTick(entity, hazard, damage);
                        }
                    }
                } else {
                    if (entity.hazardTimers?.has(hazard.id)) {
                        entity.hazardTimers.delete(hazard.id);

                        if (this.onHazardExit) {
                            this.onHazardExit(entity, hazard);
                        }
                    }
                }
            }
        }

        this.activeHazards = this.activeHazards.filter(h => h.isActive);
    }

    createFire(position, radius = 3, duration = 10000) {
        return this.addHazard({
            type: 'fire',
            position: position.clone(),
            radius,
            height: 3,
            duration
        });
    }

    createAcidPool(position, radius = 5, duration = 30000) {
        return this.addHazard({
            type: 'acid',
            position: position.clone(),
            radius,
            height: 1,
            duration
        });
    }

    createElectrifiedArea(position, radius = 4, duration = 5000) {
        return this.addHazard({
            type: 'electricity',
            position: position.clone(),
            radius,
            height: 5,
            duration
        });
    }

    createRadiationZone(position, radius = 10, duration = null) {
        return this.addHazard({
            type: 'radiation',
            position: position.clone(),
            radius,
            height: 20,
            duration
        });
    }
}
