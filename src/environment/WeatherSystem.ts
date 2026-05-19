export interface EnvironmentConfig {
  enableWeather: boolean;
  enableTimeOfDay: boolean;
  enableFog: boolean;
  enableSkybox: boolean;
  enableAmbientParticles: boolean;
  updateInterval: number;
  maxParticles: number;
}

export interface WeatherState {
  currentWeather: WeatherType;
  intensity: number;
  windDirection: Vec2;
  windSpeed: number;
  temperature: number;
  humidity: number;
  pressure: number;
  visibility: number;
  fogDensity: number;
}

export enum WeatherType {
  CLEAR = 'clear',
  CLOUDY = 'cloudy',
  OVERCAST = 'overcast',
  RAIN = 'rain',
  HEAVY_RAIN = 'heavy_rain',
  THUNDERSTORM = 'thunderstorm',
  SNOW = 'snow',
  HEAVY_SNOW = 'heavy_snow',
  SLEET = 'sleet',
  HAIL = 'hail',
  FOG = 'fog',
  DENSE_FOG = 'dense_fog',
  SANDSTORM = 'sandstorm',
  ASH = 'ash',
  SMOG = 'smog',
  BREEZE = 'breeze',
  WIND = 'wind',
  GALE = 'gale',
  TORNADO = 'tornado',
  HURRICANE = 'hurricane',
  HEAT_WAVE = 'heat_wave',
  COLD_SNAP = 'cold_snap',
  NIGHT_MIST = 'night_mist',
  MORNING_DEW = 'morning_dew',
}

export interface Vec2 {
  x: number;
  y: number;
}

export interface Vec3 {
  x: number;
  y: number;
  z: number;
}

export interface Color {
  r: number;
  g: number;
  b: number;
  a: number;
}

export interface TimeOfDay {
  hour: number;
  minute: number;
  second: number;
  sunAngle: number;
  moonAngle: number;
  ambientLight: Color;
  directionalLight: Color;
  fogColor: Color;
  skyColor: Color;
  horizonColor: Color;
  sunColor: Color;
  moonColor: Color;
  starIntensity: number;
  moonPhase: MoonPhase;
}

export enum MoonPhase {
  NEW_MOON = 0,
  WAXING_CRESCENT = 1,
  FIRST_QUARTER = 2,
  WAXING_GIBBOUS = 3,
  FULL_MOON = 4,
  WANING_GIBBOUS = 5,
  LAST_QUARTER = 6,
  WANING_CRESCENT = 7,
}

export interface Season {
  type: SeasonType;
  temperature: number;
  humidity: number;
  vegetation: number;
  dayLength: number;
  effects: string[];
}

export enum SeasonType {
  SPRING = 'spring',
  SUMMER = 'summer',
  AUTUMN = 'autumn',
  WINTER = 'winter',
}

export interface WeatherParticle {
  id: string;
  type: WeatherParticleType;
  x: number;
  y: number;
  z: number;
  vx: number;
  vy: number;
  vz: number;
  size: number;
  color: Color;
  opacity: number;
  rotation: number;
  rotationSpeed: number;
  lifetime: number;
  maxLifetime: number;
  mass: number;
  drag: number;
  active: boolean;
}

export enum WeatherParticleType {
  RAINDROP = 'raindrop',
  SNOWFLAKE = 'snowflake',
  SLEET_PELLET = 'sleet_pellet',
  HAILSTONE = 'hailstone',
  SAND_PARTICLE = 'sand_particle',
  ASH_PARTICLE = 'ash_particle',
  LEAF = 'leaf',
  DUST = 'dust',
  MIST = 'mist',
  SPARK = 'spark',
}

export interface Cloud {
  id: string;
  x: number;
  y: number;
  z: number;
  width: number;
  height: number;
  depth: number;
  opacity: number;
  color: Color;
  velocity: Vec2;
  type: CloudType;
  rotation: number;
}

export enum CloudType {
  CUMULUS = 'cumulus',
  STRATUS = 'stratus',
  CIRRUS = 'cirrus',
  NIMBUS = 'nimbus',
  CUMULONIMBUS = 'cumulonimbus',
}

export interface Lightning {
  id: string;
  startX: number;
  startY: number;
  startZ: number;
  endX: number;
  endY: number;
  endZ: number;
  segments: Vec3[];
  color: Color;
  intensity: number;
  duration: number;
  branches: LightningBranch[];
  timestamp: number;
}

export interface LightningBranch {
  startIndex: number;
  segments: Vec3[];
}

export interface ThunderSound {
  delay: number;
  volume: number;
  thunderType: ThunderType;
}

export enum ThunderType {
  DISTANT = 'distant',
  CLOSE = 'close',
  OVERHEAD = 'overhead',
}

export interface FogLayer {
  id: string;
  type: FogType;
  density: number;
  height: number;
  falloff: number;
  color: Color;
  maxDistance: number;
  noiseScale: number;
  noiseStrength: number;
}

export enum FogType {
  NONE = 'none',
  LINEAR = 'linear',
  EXPONENTIAL = 'exponential',
  EXPONENTIAL_SQUARED = 'exponential_squared',
  VOLUMETRIC = 'volumetric',
}

export interface Skybox {
  topColor: Color;
  bottomColor: Color;
  horizonColor: Color;
  sunPosition: Vec3;
  sunSize: number;
  sunColor: Color;
  sunIntensity: number;
  moonPosition: Vec3;
  moonSize: number;
  moonColor: Color;
  moonIntensity: number;
  stars: Star[];
  nebula: Nebula[];
  cloudLayers: CloudLayer[];
}

export interface Star {
  x: number;
  y: number;
  z: number;
  brightness: number;
  size: number;
  color: Color;
  twinkle: boolean;
  twinkleSpeed: number;
}

export interface Nebula {
  x: number;
  y: number;
  z: number;
  width: number;
  height: number;
  rotation: number;
  opacity: number;
  color: Color;
  type: NebulaType;
}

export enum NebulaType {
  TYPE_1 = 'type_1',
  TYPE_2 = 'type_2',
  TYPE_3 = 'type_3',
}

export interface CloudLayer {
  altitude: number;
  density: number;
  coverage: number;
  velocity: Vec2;
  turbulence: number;
  color: Color;
}

export interface WeatherEffect {
  type: WeatherType;
  particleTypes: WeatherParticleType[];
  particleCount: number;
  particleSpeed: number;
  particleSize: number;
  particleLifetime: number;
  windInfluence: number;
  temperatureModifier: number;
  visibilityModifier: number;
  audioAmbience: string;
  lightingModifier: number;
}

export interface EnvironmentalZone {
  id: string;
  name: string;
  x: number;
  y: number;
  width: number;
  height: number;
  weatherOverride: WeatherType | null;
  temperature: number;
  humidity: number;
  pollution: number;
  radiation: number;
  magneticField: number;
}

export interface PollutionSource {
  id: string;
  x: number;
  y: number;
  emissionRate: number;
  pollutionType: PollutionType;
  radius: number;
  intensity: number;
}

export enum PollutionType {
  SMOKE = 'smoke',
  CHEMICAL = 'chemical',
  RADIATION = 'radiation',
  THERMAL = 'thermal',
  LIGHT = 'light',
  NOISE = 'noise',
}

export interface WindZone {
  id: string;
  x: number;
  y: number;
  width: number;
  height: number;
  direction: Vec2;
  speed: number;
  turbulence: number;
  gustFrequency: number;
  gustStrength: number;
}

export interface AtmosphericLayer {
  name: string;
  altitude: number;
  thickness: number;
  density: number;
  temperature: number;
  pressure: number;
  composition: Record<string, number>;
}

export interface ClimateZone {
  type: ClimateType;
  temperature: number;
  precipitation: number;
  vegetation: number;
  description: string;
}

export enum ClimateType {
  TROPICAL = 'tropical',
  SUBTROPICAL = 'subtropical',
  TEMPERATE = 'temperate',
  CONTINENTAL = 'continental',
  POLAR = 'polar',
  ARID = 'arid',
  SEMI_ARID = 'semi_arid',
  MEDITERRANEAN = 'mediterranean',
}

export interface WeatherForecast {
  hour: number;
  weather: WeatherType;
  temperature: number;
  humidity: number;
  windSpeed: number;
  precipitation: number;
  confidence: number;
}

export class WeatherSystem {
  private config: EnvironmentConfig;
  private state: WeatherState;
  private timeOfDay: TimeOfDay;
  private season: Season;
  private particles: Map<string, WeatherParticle> = new Map();
  private clouds: Map<string, Cloud> = new Map();
  private fogLayers: Map<string, FogLayer> = new Map();
  private zones: Map<string, EnvironmentalZone> = new Map();
  private windZones: Map<string, WindZone> = new Map();
  private pollutionSources: Map<string, PollutionSource> = new Map();
  private lightning: Lightning | null = null;
  private lightningTimer: number = 0;
  private weatherEffects: Map<WeatherType, WeatherEffect>;
  private forecasts: WeatherForecast[] = [];
  private currentTime: number = 0;
  private dayLength: number = 1440;
  private particlePool: WeatherParticle[] = [];

  constructor(config: Partial<EnvironmentConfig> = {}) {
    this.config = {
      enableWeather: config.enableWeather ?? true,
      enableTimeOfDay: config.enableTimeOfDay ?? true,
      enableFog: config.enableFog ?? true,
      enableSkybox: config.enableSkybox ?? true,
      enableAmbientParticles: config.enableAmbientParticles ?? true,
      updateInterval: config.updateInterval || 16,
      maxParticles: config.maxParticles || 10000,
    };

    this.state = this.createInitialWeatherState();
    this.timeOfDay = this.createInitialTimeOfDay();
    this.season = this.createInitialSeason();
    this.weatherEffects = this.createWeatherEffects();
    this.initializeParticlePool();
  }

  private createInitialWeatherState(): WeatherState {
    return {
      currentWeather: WeatherType.CLEAR,
      intensity: 0,
      windDirection: { x: 1, y: 0 },
      windSpeed: 0,
      temperature: 20,
      humidity: 50,
      pressure: 1013.25,
      visibility: 10000,
      fogDensity: 0,
    };
  }

  private createInitialTimeOfDay(): TimeOfDay {
    return {
      hour: 12,
      minute: 0,
      second: 0,
      sunAngle: 0,
      moonAngle: 0,
      ambientLight: { r: 255, g: 255, b: 255, a: 255 },
      directionalLight: { r: 255, g: 255, b: 255, a: 255 },
      fogColor: { r: 200, g: 200, b: 200, a: 255 },
      skyColor: { r: 135, g: 206, b: 235, a: 255 },
      horizonColor: { r: 255, g: 200, b: 150, a: 255 },
      sunColor: { r: 255, g: 250, b: 200, a: 255 },
      moonColor: { r: 200, g: 220, b: 255, a: 255 },
      starIntensity: 0,
      moonPhase: MoonPhase.FULL_MOON,
    };
  }

  private createInitialSeason(): Season {
    return {
      type: SeasonType.SPRING,
      temperature: 15,
      humidity: 60,
      vegetation: 0.5,
      dayLength: 1.0,
      effects: ['blooming', 'mild_rain'],
    };
  }

  private createWeatherEffects(): Map<WeatherType, WeatherEffect> {
    const effects = new Map<WeatherType, WeatherEffect>();

    effects.set(WeatherType.CLEAR, {
      type: WeatherType.CLEAR,
      particleTypes: [],
      particleCount: 0,
      particleSpeed: 0,
      particleSize: 0,
      particleLifetime: 0,
      windInfluence: 0.1,
      temperatureModifier: 0,
      visibilityModifier: 1.0,
      audioAmbience: 'ambient_clear',
      lightingModifier: 1.0,
    });

    effects.set(WeatherType.RAIN, {
      type: WeatherType.RAIN,
      particleTypes: [WeatherParticleType.RAINDROP],
      particleCount: 500,
      particleSpeed: 15,
      particleSize: 0.2,
      particleLifetime: 2,
      windInfluence: 0.5,
      temperatureModifier: -5,
      visibilityModifier: 0.7,
      audioAmbience: 'ambient_rain',
      lightingModifier: 0.8,
    });

    effects.set(WeatherType.HEAVY_RAIN, {
      type: WeatherType.HEAVY_RAIN,
      particleTypes: [WeatherParticleType.RAINDROP],
      particleCount: 2000,
      particleSpeed: 20,
      particleSize: 0.3,
      particleLifetime: 1.5,
      windInfluence: 0.7,
      temperatureModifier: -10,
      visibilityModifier: 0.5,
      audioAmbience: 'ambient_heavy_rain',
      lightingModifier: 0.6,
    });

    effects.set(WeatherType.THUNDERSTORM, {
      type: WeatherType.THUNDERSTORM,
      particleTypes: [WeatherParticleType.RAINDROP, WeatherParticleType.SPARK],
      particleCount: 3000,
      particleSpeed: 25,
      particleSize: 0.3,
      particleLifetime: 1.5,
      windInfluence: 0.8,
      temperatureModifier: -8,
      visibilityModifier: 0.4,
      audioAmbience: 'ambient_thunderstorm',
      lightingModifier: 0.5,
    });

    effects.set(WeatherType.SNOW, {
      type: WeatherType.SNOW,
      particleTypes: [WeatherParticleType.SNOWFLAKE],
      particleCount: 800,
      particleSpeed: 2,
      particleSize: 0.5,
      particleLifetime: 5,
      windInfluence: 0.6,
      temperatureModifier: -15,
      visibilityModifier: 0.6,
      audioAmbience: 'ambient_snow',
      lightingModifier: 0.9,
    });

    effects.set(WeatherType.HEAVY_SNOW, {
      type: WeatherType.HEAVY_SNOW,
      particleTypes: [WeatherParticleType.SNOWFLAKE],
      particleCount: 3000,
      particleSpeed: 3,
      particleSize: 0.6,
      particleLifetime: 4,
      windInfluence: 0.8,
      temperatureModifier: -20,
      visibilityModifier: 0.3,
      audioAmbience: 'ambient_heavy_snow',
      lightingModifier: 0.85,
    });

    effects.set(WeatherType.FOG, {
      type: WeatherType.FOG,
      particleTypes: [WeatherParticleType.MIST],
      particleCount: 200,
      particleSpeed: 0.5,
      particleSize: 2,
      particleLifetime: 10,
      windInfluence: 0.2,
      temperatureModifier: 0,
      visibilityModifier: 0.2,
      audioAmbience: 'ambient_fog',
      lightingModifier: 0.7,
    });

    effects.set(WeatherType.SANDSTORM, {
      type: WeatherType.SANDSTORM,
      particleTypes: [WeatherParticleType.SAND_PARTICLE, WeatherParticleType.DUST],
      particleCount: 1500,
      particleSpeed: 12,
      particleSize: 0.4,
      particleLifetime: 3,
      windInfluence: 1.0,
      temperatureModifier: 10,
      visibilityModifier: 0.3,
      audioAmbience: 'ambient_sandstorm',
      lightingModifier: 0.6,
    });

    effects.set(WeatherType.SLEET, {
      type: WeatherType.SLEET,
      particleTypes: [WeatherParticleType.SLEET_PELLET, WeatherParticleType.RAINDROP],
      particleCount: 600,
      particleSpeed: 8,
      particleSize: 0.3,
      particleLifetime: 2,
      windInfluence: 0.6,
      temperatureModifier: -3,
      visibilityModifier: 0.5,
      audioAmbience: 'ambient_sleet',
      lightingModifier: 0.75,
    });

    effects.set(WeatherType.HAIL, {
      type: WeatherType.HAIL,
      particleTypes: [WeatherParticleType.HAILSTONE],
      particleCount: 400,
      particleSpeed: 10,
      particleSize: 0.5,
      particleLifetime: 3,
      windInfluence: 0.5,
      temperatureModifier: -5,
      visibilityModifier: 0.6,
      audioAmbience: 'ambient_hail',
      lightingModifier: 0.7,
    });

    return effects;
  }

  private initializeParticlePool(): void {
    for (let i = 0; i < this.config.maxParticles; i++) {
      this.particlePool.push(this.createEmptyParticle());
    }
  }

  private createEmptyParticle(): WeatherParticle {
    return {
      id: '',
      type: WeatherParticleType.RAINDROP,
      x: 0,
      y: 0,
      z: 0,
      vx: 0,
      vy: 0,
      vz: 0,
      size: 0,
      color: { r: 200, g: 200, b: 255, a: 200 },
      opacity: 1,
      rotation: 0,
      rotationSpeed: 0,
      lifetime: 0,
      maxLifetime: 0,
      mass: 1,
      drag: 0.1,
      active: false,
    };
  }

  public setWeather(weather: WeatherType, intensity: number = 1): void {
    this.state.currentWeather = weather;
    this.state.intensity = Math.max(0, Math.min(1, intensity));

    const effect = this.weatherEffects.get(weather);
    if (effect) {
      this.state.temperature += effect.temperatureModifier * this.state.intensity;
      this.state.visibility *= effect.visibilityModifier;
    }
  }

  public transitionWeather(from: WeatherType, to: WeatherType, duration: number): void {
    const startTime = this.currentTime;
    const startIntensity = this.state.intensity;
    const startWeather = from;

    const interpolate = () => {
      const elapsed = this.currentTime - startTime;
      const progress = Math.min(1, elapsed / duration);

      if (progress < 0.5) {
        this.setWeather(startWeather, startIntensity * (1 - progress * 2));
      } else {
        this.setWeather(to, (progress - 0.5) * 2);
      }

      if (progress < 1) {
        requestAnimationFrame(interpolate);
      }
    };

    interpolate();
  }

  public setWind(direction: Vec2, speed: number): void {
    const length = Math.sqrt(direction.x * direction.x + direction.y * direction.y);
    if (length > 0) {
      this.state.windDirection = { x: direction.x / length, y: direction.y / length };
    }
    this.state.windSpeed = Math.max(0, speed);
  }

  public setTimeOfDay(hour: number, minute: number = 0): void {
    this.timeOfDay.hour = hour;
    this.timeOfDay.minute = minute;
    this.updateTimeOfDay();
  }

  private updateTimeOfDay(): void {
    const totalMinutes = this.timeOfDay.hour * 60 + this.timeOfDay.minute;
    const sunProgress = totalMinutes / 1440;
    this.timeOfDay.sunAngle = sunProgress * 360 - 180;

    const moonProgress = (sunProgress + 0.5) % 1;
    this.timeOfDay.moonAngle = moonProgress * 360 - 180;

    const dayPhase = Math.sin((this.timeOfDay.sunAngle + 90) * Math.PI / 180);

    const dayColor = { r: 255, g: 255, b: 255, a: 255 };
    const nightColor = { r: 50, g: 50, b: 80, a: 255 };
    const sunsetColor = { r: 255, g: 150, b: 100, a: 255 };

    let dayNightFactor = Math.max(0, dayPhase);

    if (dayPhase < 0) {
      this.timeOfDay.starIntensity = Math.abs(dayPhase);
      this.timeOfDay.ambientLight = this.lerpColor(nightColor, { r: 100, g: 100, b: 120, a: 255 }, Math.abs(dayPhase));
    } else if (dayPhase > 0.8) {
      const sunsetFactor = (dayPhase - 0.8) / 0.2;
      this.timeOfDay.ambientLight = this.lerpColor(dayColor, sunsetColor, sunsetFactor);
      this.timeOfDay.horizonColor = sunsetColor;
    } else {
      this.timeOfDay.starIntensity = 0;
      this.timeOfDay.ambientLight = this.lerpColor(nightColor, dayColor, dayNightFactor);
    }

    const sunPosition = this.calculateCelestialPosition(this.timeOfDay.sunAngle, true);
    this.timeOfDay.sunPosition = sunPosition;

    const moonPosition = this.calculateCelestialPosition(this.timeOfDay.moonAngle, false);
    this.timeOfDay.moonPosition = moonPosition;
  }

  private calculateCelestialPosition(angle: number, isSun: boolean): Vec3 {
    const rad = angle * Math.PI / 180;
    const radius = 100;
    const height = Math.sin(rad) * radius;
    const horizontal = Math.cos(rad) * radius;

    return {
      x: horizontal,
      y: Math.abs(height),
      z: height,
    };
  }

  private lerpColor(c1: Color, c2: Color, t: number): Color {
    return {
      r: c1.r + (c2.r - c1.r) * t,
      g: c1.g + (c2.g - c1.g) * t,
      b: c1.b + (c2.b - c1.b) * t,
      a: c1.a + (c2.a - c1.a) * t,
    };
  }

  public setSeason(season: SeasonType): void {
    switch (season) {
      case SeasonType.SPRING:
        this.season = { type: season, temperature: 15, humidity: 60, vegetation: 0.6, dayLength: 1.0, effects: ['blooming', 'light_rain'] };
        break;
      case SeasonType.SUMMER:
        this.season = { type: season, temperature: 30, humidity: 40, vegetation: 1.0, dayLength: 1.2, effects: ['heat_wave', 'thunderstorm'] };
        break;
      case SeasonType.AUTUMN:
        this.season = { type: season, temperature: 12, humidity: 70, vegetation: 0.3, dayLength: 0.9, effects: ['falling_leaves', 'fog'] };
        break;
      case SeasonType.WINTER:
        this.season = { type: season, temperature: -5, humidity: 30, vegetation: 0.0, dayLength: 0.7, effects: ['snow', 'blizzard'] };
        break;
    }

    this.state.temperature = this.season.temperature;
  }

  public addEnvironmentalZone(zone: Omit<EnvironmentalZone, 'id'>): string {
    const id = `zone_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    this.zones.set(id, { ...zone, id });
    return id;
  }

  public removeEnvironmentalZone(zoneId: string): void {
    this.zones.delete(zoneId);
  }

  public addWindZone(zone: Omit<WindZone, 'id'>): string {
    const id = `wind_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    this.windZones.set(id, { ...zone, id });
    return id;
  }

  public removeWindZone(zoneId: string): void {
    this.windZones.delete(zoneId);
  }

  public addPollutionSource(source: Omit<PollutionSource, 'id'>): string {
    const id = `pollution_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    this.pollutionSources.set(id, { ...source, id });
    return id;
  }

  public removePollutionSource(sourceId: string): void {
    this.pollutionSources.delete(sourceId);
  }

  public addFogLayer(layer: Omit<FogLayer, 'id'>): string {
    const id = `fog_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    this.fogLayers.set(id, { ...layer, id });
    return id;
  }

  public removeFogLayer(layerId: string): void {
    this.fogLayers.delete(layerId);
  }

  public update(deltaTime: number): void {
    this.currentTime += deltaTime;

    if (this.config.enableTimeOfDay) {
      this.updateTimeOfDaySystem(deltaTime);
    }

    if (this.config.enableWeather) {
      this.updateWeatherSystem(deltaTime);
    }

    if (this.config.enableFog) {
      this.updateFogSystem(deltaTime);
    }

    this.updateClouds(deltaTime);
    this.updateLightning(deltaTime);
    this.updatePollution(deltaTime);
    this.updateForecasts();
  }

  private updateTimeOfDaySystem(_deltaTime: number): void {
    this.timeOfDay.second += 1;
    if (this.timeOfDay.second >= 60) {
      this.timeOfDay.second = 0;
      this.timeOfDay.minute += 1;
      if (this.timeOfDay.minute >= 60) {
        this.timeOfDay.minute = 0;
        this.timeOfDay.hour = (this.timeOfDay.hour + 1) % 24;
      }
    }

    this.updateTimeOfDay();
  }

  private updateWeatherSystem(deltaTime: number): void {
    const effect = this.weatherEffects.get(this.state.currentWeather);
    if (!effect) {
      return;
    }

    this.spawnWeatherParticles(effect, deltaTime);
    this.updateParticles(deltaTime);
    this.recycleParticles();

    this.state.humidity = Math.min(100, this.state.humidity + effect.temperatureModifier * 0.01);
    this.state.windSpeed += (Math.random() - 0.5) * 0.1;
    this.state.windSpeed = Math.max(0, Math.min(50, this.state.windSpeed));
  }

  private spawnWeatherParticles(effect: WeatherEffect, deltaTime: number): void {
    const spawnCount = Math.floor(effect.particleCount * this.state.intensity * deltaTime);

    for (let i = 0; i < spawnCount; i++) {
      const particle = this.getParticleFromPool();
      if (!particle) {
        break;
      }

      particle.x = Math.random() * 100 - 50;
      particle.y = 50;
      particle.z = Math.random() * 100 - 50;

      particle.vx = this.state.windDirection.x * effect.particleSpeed + (Math.random() - 0.5) * 2;
      particle.vy = -effect.particleSpeed;
      particle.vz = this.state.windDirection.y * effect.particleSpeed + (Math.random() - 0.5) * 2;

      particle.size = effect.particleSize * (0.5 + Math.random() * 0.5);
      particle.lifetime = 0;
      particle.maxLifetime = effect.particleLifetime;
      particle.active = true;
      particle.type = effect.particleTypes[Math.floor(Math.random() * effect.particleTypes.length)];

      switch (particle.type) {
        case WeatherParticleType.RAINDROP:
          particle.color = { r: 150, g: 180, b: 255, a: 180 };
          particle.drag = 0.05;
          break;
        case WeatherParticleType.SNOWFLAKE:
          particle.color = { r: 255, g: 255, b: 255, a: 220 };
          particle.vy = -effect.particleSpeed * 0.3;
          particle.vx += (Math.random() - 0.5) * 3;
          particle.vz += (Math.random() - 0.5) * 3;
          particle.drag = 0.2;
          particle.rotationSpeed = (Math.random() - 0.5) * 5;
          break;
        case WeatherParticleType.SAND_PARTICLE:
          particle.color = { r: 200, g: 180, b: 140, a: 200 };
          particle.size = effect.particleSize * 0.7;
          break;
        case WeatherParticleType.HAILSTONE:
          particle.color = { r: 220, g: 230, b: 255, a: 255 };
          particle.size = effect.particleSize * 1.5;
          particle.mass = 2;
          break;
        default:
          particle.color = { r: 200, g: 200, b: 200, a: 150 };
      }

      this.particles.set(particle.id, particle);
    }
  }

  private getParticleFromPool(): WeatherParticle | null {
    for (const particle of this.particlePool) {
      if (!particle.active) {
        particle.id = `particle_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
        return particle;
      }
    }
    return null;
  }

  private updateParticles(deltaTime: number): void {
    this.particles.forEach((particle, id) => {
      if (!particle.active) {
        return;
      }

      particle.lifetime += deltaTime;

      if (particle.lifetime >= particle.maxLifetime) {
        particle.active = false;
        return;
      }

      const gravity = particle.type === WeatherParticleType.RAINDROP ? -50 :
                      particle.type === WeatherParticleType.SNOWFLAKE ? -2 :
                      particle.type === WeatherParticleType.HAILSTONE ? -30 : -10;

      particle.vy += gravity * deltaTime;
      particle.vx += this.state.windDirection.x * this.state.windSpeed * 0.1 * deltaTime;
      particle.vz += this.state.windDirection.y * this.state.windSpeed * 0.1 * deltaTime;

      const speed = Math.sqrt(particle.vx * particle.vx + particle.vy * particle.vy + particle.vz * particle.vz);
      if (speed > 0) {
        const dragForce = particle.drag * speed * speed;
        particle.vx -= (particle.vx / speed) * dragForce * deltaTime;
        particle.vy -= (particle.vy / speed) * dragForce * deltaTime;
        particle.vz -= (particle.vz / speed) * dragForce * deltaTime;
      }

      particle.x += particle.vx * deltaTime;
      particle.y += particle.vy * deltaTime;
      particle.z += particle.vz * deltaTime;

      particle.rotation += particle.rotationSpeed * deltaTime;

      particle.opacity = 1 - (particle.lifetime / particle.maxLifetime);

      if (particle.y < -10) {
        particle.active = false;
      }
    });
  }

  private recycleParticles(): void {
    const toRemove: string[] = [];

    this.particles.forEach((particle, id) => {
      if (!particle.active) {
        toRemove.push(id);
      }
    });

    toRemove.forEach(id => {
      this.particles.delete(id);
    });
  }

  private updateFogSystem(_deltaTime: number): void {
    let totalFogDensity = 0;

    this.fogLayers.forEach(layer => {
      const effect = this.weatherEffects.get(this.state.currentWeather);
      if (effect) {
        layer.density = effect.visibilityModifier < 1 ? (1 - effect.visibilityModifier) * 0.5 : 0;
      }

      totalFogDensity += layer.density;
    });

    this.state.fogDensity = Math.min(1, totalFogDensity);
  }

  private updateClouds(deltaTime: number): void {
    this.clouds.forEach(cloud => {
      cloud.x += cloud.velocity.x * deltaTime;
      cloud.y += cloud.velocity.y * deltaTime;
      cloud.rotation += deltaTime * 0.01;
    });

    if (this.state.currentWeather === WeatherType.CLOUDY || this.state.currentWeather === WeatherType.OVERCAST) {
      if (Math.random() < 0.01) {
        this.spawnCloud();
      }
    }
  }

  private spawnCloud(): void {
    const cloudTypes = [CloudType.CUMULUS, CloudType.STRATUS, CloudType.CIRRUS];
    const type = cloudTypes[Math.floor(Math.random() * cloudTypes.length)];

    const cloud: Cloud = {
      id: `cloud_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      x: -100,
      y: 30 + Math.random() * 20,
      z: Math.random() * 100 - 50,
      width: 20 + Math.random() * 30,
      height: 10 + Math.random() * 10,
      depth: 20 + Math.random() * 20,
      opacity: 0.5 + Math.random() * 0.3,
      color: { r: 255, g: 255, b: 255, a: 255 },
      velocity: { x: 1 + Math.random(), y: 0 },
      type,
      rotation: 0,
    };

    this.clouds.set(cloud.id, cloud);
  }

  private updateLightning(deltaTime: number): void {
    if (this.state.currentWeather !== WeatherType.THUNDERSTORM) {
      this.lightning = null;
      return;
    }

    this.lightningTimer += deltaTime;

    if (this.lightningTimer > 5 + Math.random() * 15) {
      this.strikeLightning();
      this.lightningTimer = 0;
    }

    if (this.lightning) {
      this.lightning.duration -= deltaTime;
      if (this.lightning.duration <= 0) {
        this.lightning = null;
      }
    }
  }

  private strikeLightning(): void {
    const startX = Math.random() * 100 - 50;
    const startZ = Math.random() * 100 - 50;
    const endX = startX + (Math.random() - 0.5) * 20;
    const endZ = startZ + (Math.random() - 0.5) * 20;

    const segments: Vec3[] = [];
    let currentX = startX;
    let currentY = 50;
    let currentZ = startZ;

    while (currentY > 0) {
      segments.push({ x: currentX, y: currentY, z: currentZ });
      currentY -= 2 + Math.random() * 3;
      currentX += (Math.random() - 0.5) * 4;
      currentZ += (Math.random() - 0.5) * 4;
    }

    segments.push({ x: endX, y: 0, z: endZ });

    const branches: LightningBranch[] = [];
    if (Math.random() < 0.5) {
      const branchStart = Math.floor(segments.length * 0.3 + Math.random() * segments.length * 0.4);
      const branchSegments: Vec3[] = [segments[branchStart]];
      let bx = segments[branchStart].x;
      let by = segments[branchStart].y;
      let bz = segments[branchStart].z;

      for (let i = 0; i < 5; i++) {
        by -= 1 + Math.random() * 2;
        bx += (Math.random() - 0.5) * 3;
        bz += (Math.random() - 0.5) * 3;
        branchSegments.push({ x: bx, y: by, z: bz });
      }

      branches.push({ startIndex: branchStart, segments: branchSegments });
    }

    this.lightning = {
      id: `lightning_${Date.now()}`,
      startX,
      startY: 50,
      startZ,
      endX,
      endY: 0,
      endZ,
      segments,
      color: { r: 200, g: 200, b: 255, a: 255 },
      intensity: 1,
      duration: 0.2 + Math.random() * 0.3,
      branches,
      timestamp: Date.now(),
    };
  }

  private updatePollution(_deltaTime: number): void {
    let totalPollution = 0;

    this.pollutionSources.forEach(source => {
      const distance = Math.sqrt(source.x * source.x + source.y * source.y);
      if (distance < source.radius) {
        totalPollution += source.intensity * (1 - distance / source.radius);
      }
    });

    totalPollution /= Math.max(1, this.pollutionSources.size);

    this.state.visibility = Math.max(100, 10000 * (1 - totalPollution * 0.5));
  }

  private updateForecasts(): void {
    if (this.forecasts.length === 0 || this.forecasts[0].hour !== this.timeOfDay.hour) {
      this.generateForecasts();
    }
  }

  private generateForecasts(): void {
    this.forecasts = [];

    let currentWeather = this.state.currentWeather;
    let currentTemp = this.state.temperature;

    for (let i = 0; i < 24; i++) {
      const hour = (Math.floor(this.timeOfDay.hour) + i) % 24;

      if (Math.random() < 0.2) {
        const weatherOptions = [WeatherType.CLEAR, WeatherType.CLOUDY, WeatherType.RAIN, WeatherType.FOG];
        currentWeather = weatherOptions[Math.floor(Math.random() * weatherOptions.length)];
      }

      const tempChange = Math.sin((hour - 6) * Math.PI / 12) * 10;
      currentTemp = this.season.temperature + tempChange;

      this.forecasts.push({
        hour,
        weather: currentWeather,
        temperature: currentTemp,
        humidity: 40 + Math.random() * 40,
        windSpeed: 5 + Math.random() * 20,
        precipitation: currentWeather.includes('RAIN') ? 0.5 + Math.random() * 0.5 : 0,
        confidence: Math.max(0.3, 1 - i * 0.03),
      });
    }
  }

  public getWeatherState(): WeatherState {
    return { ...this.state };
  }

  public getTimeOfDay(): TimeOfDay {
    return { ...this.timeOfDay };
  }

  public getSeason(): Season {
    return { ...this.season };
  }

  public getParticles(): WeatherParticle[] {
    return Array.from(this.particles.values());
  }

  public getActiveParticles(): WeatherParticle[] {
    return Array.from(this.particles.values()).filter(p => p.active);
  }

  public getClouds(): Cloud[] {
    return Array.from(this.clouds.values());
  }

  public getFogLayers(): FogLayer[] {
    return Array.from(this.fogLayers.values());
  }

  public getEnvironmentalZones(): EnvironmentalZone[] {
    return Array.from(this.zones.values());
  }

  public getWindZones(): WindZone[] {
    return Array.from(this.windZones.values());
  }

  public getPollutionSources(): PollutionSource[] {
    return Array.from(this.pollutionSources.values());
  }

  public getLightning(): Lightning | null {
    return this.lightning;
  }

  public getForecast(hours: number = 24): WeatherForecast[] {
    return this.forecasts.slice(0, hours);
  }

  public getActiveWeatherEffect(): WeatherEffect | undefined {
    return this.weatherEffects.get(this.state.currentWeather);
  }

  public getSkybox(): Skybox {
    return {
      topColor: this.timeOfDay.skyColor,
      bottomColor: this.timeOfDay.horizonColor,
      horizonColor: this.timeOfDay.horizonColor,
      sunPosition: this.timeOfDay.sunPosition,
      sunSize: 5,
      sunColor: this.timeOfDay.sunColor,
      sunIntensity: Math.max(0, Math.sin((this.timeOfDay.sunAngle + 90) * Math.PI / 180)),
      moonPosition: this.timeOfDay.moonPosition,
      moonSize: 3,
      moonColor: this.timeOfDay.moonColor,
      moonIntensity: this.timeOfDay.starIntensity * 0.5,
      stars: this.generateStars(),
      nebula: [],
      cloudLayers: this.generateCloudLayers(),
    };
  }

  private generateStars(): Star[] {
    const stars: Star[] = [];
    const starCount = Math.floor(500 * this.timeOfDay.starIntensity);

    for (let i = 0; i < starCount; i++) {
      stars.push({
        x: Math.random() * 360,
        y: Math.random() * 90,
        z: 100,
        brightness: 0.5 + Math.random() * 0.5,
        size: 0.5 + Math.random() * 1.5,
        color: { r: 255, g: 255, b: 255, a: 255 },
        twinkle: Math.random() < 0.3,
        twinkleSpeed: 1 + Math.random() * 3,
      });
    }

    return stars;
  }

  private generateCloudLayers(): CloudLayer[] {
    return [
      { altitude: 100, density: 0.2, coverage: 0.3, velocity: { x: 1, y: 0 }, turbulence: 0.1, color: { r: 255, g: 255, b: 255, a: 200 } },
      { altitude: 200, density: 0.3, coverage: 0.4, velocity: { x: 1.2, y: 0 }, turbulence: 0.15, color: { r: 220, g: 220, b: 220, a: 180 } },
      { altitude: 400, density: 0.4, coverage: 0.5, velocity: { x: 0.8, y: 0 }, turbulence: 0.2, color: { r: 180, g: 180, b: 180, a: 150 } },
    ];
  }

  public isNight(): boolean {
    return this.timeOfDay.hour < 6 || this.timeOfDay.hour >= 20;
  }

  public isDay(): boolean {
    return this.timeOfDay.hour >= 6 && this.timeOfDay.hour < 20;
  }

  public isRaining(): boolean {
    return this.state.currentWeather === WeatherType.RAIN ||
           this.state.currentWeather === WeatherType.HEAVY_RAIN ||
           this.state.currentWeather === WeatherType.THUNDERSTORM;
  }

  public isSnowing(): boolean {
    return this.state.currentWeather === WeatherType.SNOW ||
           this.state.currentWeather === WeatherType.HEAVY_SNOW;
  }

  public isFoggy(): boolean {
    return this.state.fogDensity > 0.1 ||
           this.state.currentWeather === WeatherType.FOG ||
           this.state.currentWeather === WeatherType.DENSE_FOG;
  }

  public destroy(): void {
    this.particles.clear();
    this.clouds.clear();
    this.fogLayers.clear();
    this.zones.clear();
    this.windZones.clear();
    this.pollutionSources.clear();
    this.particlePool = [];
  }
}

export const createWeatherSystem = (config?: Partial<EnvironmentConfig>): WeatherSystem => {
  return new WeatherSystem(config);
};

export default WeatherSystem;
