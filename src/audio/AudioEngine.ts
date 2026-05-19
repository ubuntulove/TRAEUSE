export interface SoundConfig {
  src: string;
  volume?: number;
  loop?: boolean;
  playbackRate?: number;
  spatial?: boolean;
  refDistance?: number;
  maxDistance?: number;
  rolloffFactor?: number;
}

export interface MusicTrack {
  src: string;
  volume?: number;
  fadeIn?: number;
  fadeOut?: number;
}

export class AudioEngine {
  private context: AudioContext | null = null;
  private masterVolume: number = 1.0;
  private musicVolume: number = 0.7;
  private sfxVolume: number = 1.0;
  private sounds: Map<string, AudioBuffer> = new Map();
  private activeSounds: Map<string, {
    source: AudioBufferSourceNode;
    gain: GainNode;
    panner?: PannerNode;
  }> = new Map();
  private musicSource: AudioBufferSourceNode | null = null;
  private musicGain: GainNode | null = null;
  private initialized: boolean = false;
  private listenerPosition: { x: number; y: number; z: number } = { x: 0, y: 0, z: 0 };

  constructor() {
    this.initContext();
  }

  private initContext(): void {
    try {
      this.context = new (window.AudioContext || (window as any).webkitAudioContext)();
      this.initialized = true;
    } catch (e) {
      console.warn('Web Audio API not supported:', e);
      this.initialized = false;
    }
  }

  public async initialize(): Promise<void> {
    if (!this.initialized) {
      this.initContext();
    }

    if (this.context && this.context.state === 'suspended') {
      await this.context.resume();
    }
  }

  public async loadSound(id: string, src: string): Promise<void> {
    if (!this.context) return;

    try {
      const response = await fetch(src);
      const arrayBuffer = await response.arrayBuffer();
      const audioBuffer = await this.context.decodeAudioData(arrayBuffer);
      this.sounds.set(id, audioBuffer);
    } catch (e) {
      console.error(`Failed to load sound ${id} from ${src}:`, e);
    }
  }

  public async loadSounds(soundConfigs: Array<{ id: string; src: string }>): Promise<void> {
    const promises = soundConfigs.map(config => this.loadSound(config.id, config.src));
    await Promise.all(promises);
  }

  public playSound(
    id: string,
    options: {
      volume?: number;
      loop?: boolean;
      playbackRate?: number;
      position?: { x: number; y: number; z?: number };
    } = {}
  ): string | null {
    if (!this.context) return null;

    const buffer = this.sounds.get(id);
    if (!buffer) {
      console.warn(`Sound ${id} not found`);
      return null;
    }

    const source = this.context.createBufferSource();
    source.buffer = buffer;
    source.loop = options.loop || false;
    source.playbackRate.value = options.playbackRate || 1.0;

    const gainNode = this.context.createGain();
    gainNode.gain.value = (options.volume || 1.0) * this.sfxVolume * this.masterVolume;

    let panner: PannerNode | undefined;
    if (options.position) {
      panner = this.context.createPanner();
      panner.panningModel = 'HRTF';
      panner.distanceModel = 'inverse';
      panner.refDistance = 1;
      panner.maxDistance = 10000;
      panner.rolloffFactor = 1;
      panner.positionX.value = options.position.x;
      panner.positionY.value = options.position.y;
      panner.positionZ.value = options.position.z || 0;

      source.connect(panner);
      panner.connect(gainNode);
    } else {
      source.connect(gainNode);
    }

    gainNode.connect(this.context.destination);

    const soundId = `${id}_${Date.now()}_${Math.random()}`;
    this.activeSounds.set(soundId, { source, gain: gainNode, panner });

    source.onended = () => {
      this.activeSounds.delete(soundId);
    };

    source.start();

    return soundId;
  }

  public stopSound(soundId: string): void {
    const sound = this.activeSounds.get(soundId);
    if (sound) {
      try {
        sound.source.stop();
        sound.source.disconnect();
        sound.gain.disconnect();
        sound.panner?.disconnect();
      } catch (e) {
      }
      this.activeSounds.delete(soundId);
    }
  }

  public stopAllSounds(): void {
    for (const soundId of this.activeSounds.keys()) {
      this.stopSound(soundId);
    }
  }

  public setSoundVolume(soundId: string, volume: number): void {
    const sound = this.activeSounds.get(soundId);
    if (sound) {
      sound.gain.gain.value = volume * this.sfxVolume * this.masterVolume;
    }
  }

  public updateSoundPosition(soundId: string, position: { x: number; y: number; z?: number }): void {
    const sound = this.activeSounds.get(soundId);
    if (sound?.panner) {
      sound.panner.positionX.value = position.x;
      sound.panner.positionY.value = position.y;
      sound.panner.positionZ.value = position.z || 0;
    }
  }

  public async playMusic(trackSrc: string, options: {
    volume?: number;
    fadeIn?: number;
    loop?: boolean;
  } = {}): Promise<void> {
    if (!this.context) return;

    if (this.musicSource) {
      await this.fadeOut(this.musicGain!, options.fadeIn || 1.0);
      this.musicSource.stop();
      this.musicSource.disconnect();
      this.musicGain!.disconnect();
    }

    try {
      const response = await fetch(trackSrc);
      const arrayBuffer = await response.arrayBuffer();
      const audioBuffer = await this.context.decodeAudioData(arrayBuffer);

      this.musicSource = this.context.createBufferSource();
      this.musicSource.buffer = audioBuffer;
      this.musicSource.loop = options.loop !== undefined ? options.loop : true;

      this.musicGain = this.context.createGain();
      const volume = (options.volume || 1.0) * this.musicVolume * this.masterVolume;

      if (options.fadeIn) {
        this.musicGain.gain.value = 0;
        this.musicSource.connect(this.musicGain);
        this.musicGain.connect(this.context.destination);
        this.musicSource.start();
        await this.fadeIn(this.musicGain, options.fadeIn, volume);
      } else {
        this.musicGain.gain.value = volume;
        this.musicSource.connect(this.musicGain);
        this.musicGain.connect(this.context.destination);
        this.musicSource.start();
      }
    } catch (e) {
      console.error('Failed to play music:', e);
    }
  }

  public stopMusic(fadeOut?: number): void {
    if (!this.musicSource || !this.context) return;

    if (fadeOut) {
      this.fadeOut(this.musicGain!, fadeOut).then(() => {
        if (this.musicSource) {
          this.musicSource.stop();
          this.musicSource.disconnect();
          this.musicGain!.disconnect();
          this.musicSource = null;
          this.musicGain = null;
        }
      });
    } else {
      this.musicSource.stop();
      this.musicSource.disconnect();
      this.musicGain!.disconnect();
      this.musicSource = null;
      this.musicGain = null;
    }
  }

  public setMusicVolume(volume: number, fadeTime?: number): void {
    this.musicVolume = Math.max(0, Math.min(1, volume));

    if (this.musicGain) {
      if (fadeTime) {
        const targetVolume = this.musicVolume * this.masterVolume;
        this.fadeTo(this.musicGain, fadeTime, targetVolume);
      } else {
        this.musicGain.gain.value = this.musicVolume * this.masterVolume;
      }
    }
  }

  public setMasterVolume(volume: number): void {
    this.masterVolume = Math.max(0, Math.min(1, volume));

    for (const sound of this.activeSounds.values()) {
      sound.gain.gain.value = this.masterVolume;
    }

    if (this.musicGain) {
      this.musicGain.gain.value = this.musicVolume * this.masterVolume;
    }
  }

  public setListenerPosition(x: number, y: number, z: number = 0): void {
    this.listenerPosition = { x, y, z };

    if (this.context) {
      const listener = this.context.listener;
      if (listener.positionX) {
        listener.positionX.value = x;
        listener.positionY.value = y;
        listener.positionZ.value = z;
      } else {
        listener.setPosition(x, y, z);
      }
    }
  }

  public setListenerOrientation(forwardX: number, forwardY: number, forwardZ: number, upX: number, upY: number, upZ: number): void {
    if (this.context) {
      const listener = this.context.listener;
      if (listener.forwardX) {
        listener.forwardX.value = forwardX;
        listener.forwardY.value = forwardY;
        listener.forwardZ.value = forwardZ;
        listener.upX.value = upX;
        listener.upY.value = upY;
        listener.upZ.value = upZ;
      } else {
        listener.setOrientation(forwardX, forwardY, forwardZ, upX, upY, upZ);
      }
    }
  }

  private async fadeIn(gainNode: GainNode, duration: number, targetVolume: number): Promise<void> {
    return new Promise((resolve) => {
      const now = this.context!.currentTime;
      gainNode.gain.setValueAtTime(0, now);
      gainNode.gain.linearRampToValueAtTime(targetVolume, now + duration);
      setTimeout(resolve, duration * 1000);
    });
  }

  private async fadeOut(gainNode: GainNode, duration: number): Promise<void> {
    return new Promise((resolve) => {
      const now = this.context!.currentTime;
      const currentVolume = gainNode.gain.value;
      gainNode.gain.setValueAtTime(currentVolume, now);
      gainNode.gain.linearRampToValueAtTime(0, now + duration);
      setTimeout(resolve, duration * 1000);
    });
  }

  private async fadeTo(gainNode: GainNode, duration: number, targetVolume: number): Promise<void> {
    return new Promise((resolve) => {
      const now = this.context!.currentTime;
      const currentVolume = gainNode.gain.value;
      gainNode.gain.setValueAtTime(currentVolume, now);
      gainNode.gain.linearRampToValueAtTime(targetVolume, now + duration);
      setTimeout(resolve, duration * 1000);
    });
  }

  public generateTone(frequency: number, duration: number, type: OscillatorType = 'sine', volume: number = 0.3): string | null {
    if (!this.context) return null;

    const oscillator = this.context.createOscillator();
    const gainNode = this.context.createGain();

    oscillator.type = type;
    oscillator.frequency.value = frequency;

    gainNode.gain.value = volume * this.sfxVolume * this.masterVolume;
    gainNode.gain.setValueAtTime(volume * this.sfxVolume * this.masterVolume, this.context.currentTime);
    gainNode.gain.exponentialRampToValueAtTime(0.01, this.context.currentTime + duration);

    oscillator.connect(gainNode);
    gainNode.connect(this.context.destination);

    const soundId = `tone_${Date.now()}`;
    const source = this.context.createBufferSource();
    source.buffer = this.createBufferFromOscillator(oscillator, duration);
    source.connect(gainNode);
    source.start();

    return soundId;
  }

  private createBufferFromOscillator(oscillator: OscillatorNode, duration: number): AudioBuffer {
    const sampleRate = this.context!.sampleRate;
    const length = sampleRate * duration;
    const buffer = this.context!.createBuffer(1, length, sampleRate);
    const data = buffer.getChannelData(0);

    for (let i = 0; i < length; i++) {
      const t = i / sampleRate;
      data[i] = Math.sin(2 * Math.PI * oscillator.frequency.value * t);
    }

    return buffer;
  }

  public playUISound(type: 'click' | 'hover' | 'select' | 'error' | 'success'): void {
    switch (type) {
      case 'click':
        this.generateTone(800, 0.05, 'square', 0.1);
        break;
      case 'hover':
        this.generateTone(600, 0.02, 'sine', 0.05);
        break;
      case 'select':
        this.generateTone(1000, 0.1, 'sine', 0.1);
        break;
      case 'error':
        this.generateTone(200, 0.2, 'sawtooth', 0.1);
        break;
      case 'success':
        this.generateTone(800, 0.1, 'sine', 0.1);
        setTimeout(() => this.generateTone(1200, 0.1, 'sine', 0.1), 100);
        break;
    }
  }

  public isInitialized(): boolean {
    return this.initialized;
  }

  public getActiveSoundsCount(): number {
    return this.activeSounds.size;
  }

  public dispose(): void {
    this.stopAllSounds();
    this.stopMusic();

    if (this.context) {
      this.context.close();
      this.context = null;
    }

    this.initialized = false;
  }
}

export class SoundPool {
  private audio: AudioEngine;
  private pools: Map<string, string[]> = new Map();
  private maxPoolSize: number = 10;

  constructor(audio: AudioEngine, maxPoolSize: number = 10) {
    this.audio = audio;
    this.maxPoolSize = maxPoolSize;
  }

  public play(id: string, options: any = {}): string | null {
    let pool = this.pools.get(id);

    if (!pool) {
      pool = [];
      this.pools.set(id, pool);
    }

    if (pool.length > 0) {
      const soundId = pool.pop()!;
      this.audio.setSoundVolume(soundId, options.volume || 1.0);
      return soundId;
    }

    return this.audio.playSound(id, options);
  }

  public returnToPool(id: string, soundId: string): void {
    let pool = this.pools.get(id);

    if (!pool) {
      pool = [];
      this.pools.set(id, pool);
    }

    if (pool.length < this.maxPoolSize) {
      pool.push(soundId);
    } else {
      this.audio.stopSound(soundId);
    }
  }

  public clearPool(id: string): void {
    const pool = this.pools.get(id);
    if (pool) {
      for (const soundId of pool) {
        this.audio.stopSound(soundId);
      }
      pool.length = 0;
    }
  }

  public clearAllPools(): void {
    for (const pool of this.pools.values()) {
      for (const soundId of pool) {
        this.audio.stopSound(soundId);
      }
      pool.length = 0;
    }
  }
}

export class AmbientSoundManager {
  private audio: AudioEngine;
  private ambientSounds: Map<string, {
    id: string;
    position: { x: number; y: number };
    volume: number;
    soundId: string | null;
    radius: number;
  }> = new Map();

  constructor(audio: AudioEngine) {
    this.audio = audio;
  }

  public addAmbientSound(
    name: string,
    soundId: string,
    position: { x: number; y: number },
    volume: number = 1.0,
    radius: number = 100
  ): void {
    this.ambientSounds.set(name, {
      id: soundId,
      position,
      volume,
      soundId: null,
      radius,
    });
  }

  public removeAmbientSound(name: string): void {
    const ambient = this.ambientSounds.get(name);
    if (ambient?.soundId) {
      this.audio.stopSound(ambient.soundId);
    }
    this.ambientSounds.delete(name);
  }

  public updateListenerPosition(x: number, y: number): void {
    this.audio.setListenerPosition(x, y);

    for (const ambient of this.ambientSounds.values()) {
      const dx = ambient.position.x - x;
      const dy = ambient.position.y - y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance < ambient.radius) {
        const volumeMultiplier = 1 - (distance / ambient.radius);

        if (!ambient.soundId) {
          ambient.soundId = this.audio.playSound(ambient.id, {
            loop: true,
            volume: ambient.volume * volumeMultiplier,
            position: ambient.position,
          });
        } else {
          this.audio.setSoundVolume(ambient.soundId, ambient.volume * volumeMultiplier);
        }
      } else if (ambient.soundId) {
        this.audio.stopSound(ambient.soundId);
        ambient.soundId = null;
      }
    }
  }

  public stopAll(): void {
    for (const ambient of this.ambientSounds.values()) {
      if (ambient.soundId) {
        this.audio.stopSound(ambient.soundId);
        ambient.soundId = null;
      }
    }
  }
}
