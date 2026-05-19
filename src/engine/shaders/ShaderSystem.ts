export interface ShaderConfig {
  version: string;
  vertexShader: string;
  fragmentShader: string;
  uniforms: ShaderUniform[];
  attributes: ShaderAttribute[];
  renderStates: RenderState[];
  defines: Record<string, string | number | boolean>;
}

export interface ShaderUniform {
  name: string;
  type: UniformType;
  location: WebGLUniformLocation | null;
  value: UniformValue;
  arraySize?: number;
}

export enum UniformType {
  FLOAT = 'float',
  INT = 'int',
  BOOL = 'bool',
  VEC2 = 'vec2',
  VEC3 = 'vec3',
  VEC4 = 'vec4',
  MAT2 = 'mat2',
  MAT3 = 'mat3',
  MAT4 = 'mat4',
  SAMPLER2D = 'sampler2D',
  SAMPLER3D = 'sampler3D',
  SAMPLER_CUBE = 'samplerCube',
}

export type UniformValue = number | boolean | number[] | boolean[] | Float32Array | Int32Array | Uint32Array;

export interface ShaderAttribute {
  name: string;
  type: AttributeType;
  location: number;
  size: number;
}

export enum AttributeType {
  FLOAT = 'float',
  VEC2 = 'vec2',
  VEC3 = 'vec3',
  VEC4 = 'vec4',
  MAT2 = 'mat2',
  MAT3 = 'mat3',
  MAT4 = 'mat4',
}

export interface RenderState {
  enable: boolean;
  func: RenderStateFunction;
  value: unknown;
}

export enum RenderStateFunction {
  BLEND = 'blend',
  DEPTH_TEST = 'depth_test',
  CULL_FACE = 'cull_face',
  STENCIL_TEST = 'stencil_test',
  SCISSOR_TEST = 'scissor_test',
  POLYGON_OFFSET_FILL = 'polygon_offset_fill',
  SAMPLE_ALPHA_TO_COVERAGE = 'sample_alpha_to_coverage',
}

export interface ShaderProgram {
  id: string;
  name: string;
  program: WebGLProgram;
  vertexShader: WebGLShader;
  fragmentShader: WebGLShader;
  uniforms: Map<string, ShaderUniform>;
  attributes: Map<string, ShaderAttribute>;
  sources: { vertex: string; fragment: string };
  compiled: boolean;
  compilationTime: number;
  lastUsed: number;
  useCount: number;
}

export interface ShaderMaterial {
  id: string;
  name: string;
  shaderId: string;
  uniforms: Record<string, UniformValue>;
  textures: Map<string, ShaderTexture>;
  renderQueue: RenderQueue;
  properties: MaterialProperties;
  keywords: Set<string>;
}

export interface ShaderTexture {
  unit: number;
  texture: WebGLTexture | null;
  target: TextureTarget;
  width: number;
  height: number;
  format: TextureFormat;
  type: TextureDataType;
  wrapS: TextureWrap;
  wrapT: TextureWrap;
  minFilter: TextureFilter;
  magFilter: TextureFilter;
  mipmap: boolean;
  anisotropy: number;
}

export enum TextureTarget {
  TEXTURE_2D = 'texture_2d',
  TEXTURE_CUBE_MAP = 'texture_cube_map',
  TEXTURE_3D = 'texture_3d',
}

export enum TextureFormat {
  ALPHA = 'alpha',
  RGB = 'rgb',
  RGBA = 'rgba',
  LUMINANCE = 'luminance',
  LUMINANCE_ALPHA = 'luminance_alpha',
  RGB16F = 'rgb16f',
  RGBA16F = 'rgba16f',
  RGB32F = 'rgb32f',
  RGBA32F = 'rgba32f',
  DEPTH = 'depth',
  STENCIL = 'stencil',
  DEPTH_STENCIL = 'depth_stencil',
  SRGB = 'srgb',
  SRGB_ALPHA = 'srgb_alpha',
}

export enum TextureDataType {
  UNSIGNED_BYTE = 'unsigned_byte',
  BYTE = 'byte',
  UNSIGNED_SHORT = 'unsigned_short',
  SHORT = 'short',
  UNSIGNED_INT = 'unsigned_int',
  INT = 'int',
  FLOAT = 'float',
  HALF_FLOAT = 'half_float',
}

export enum TextureWrap {
  CLAMP_TO_EDGE = 'clamp_to_edge',
  REPEAT = 'repeat',
  MIRRORED_REPEAT = 'mirrored_repeat',
  CLAMP_TO_BORDER = 'clamp_to_border',
}

export enum TextureFilter {
  NEAREST = 'nearest',
  LINEAR = 'linear',
  NEAREST_MIPMAP_NEAREST = 'nearest_mipmap_nearest',
  LINEAR_MIPMAP_NEAREST = 'linear_mipmap_nearest',
  NEAREST_MIPMAP_LINEAR = 'nearest_mipmap_linear',
  LINEAR_MIPMAP_LINEAR = 'linear_mipmap_linear',
}

export interface RenderQueue {
  type: QueueType;
  priority: number;
}

export enum QueueType {
  BACKGROUND = 'background',
  GEOMETRY = 'geometry',
  ALPHA_TEST = 'alpha_test',
  TRANSPARENT = 'transparent',
  OVERLAY = 'overlay',
  UI = 'ui',
}

export interface MaterialProperties {
  transparent: boolean;
  doubleSided: boolean;
  depthWrite: boolean;
  depthTest: boolean;
  colorWrite: boolean;
  blendMode: BlendMode;
  roughness: number;
  metalness: number;
  emission: number;
  opacity: number;
}

export enum BlendMode {
  NONE = 'none',
  ALPHA = 'alpha',
  ADD = 'add',
  MULTIPLY = 'multiply',
  SCREEN = 'screen',
  OVERLAY = 'overlay',
  SOFT_LIGHT = 'soft_light',
  HARD_LIGHT = 'hard_light',
  DIFFERENCE = 'difference',
  INVERT = 'invert',
}

export interface ShaderEffect {
  id: string;
  name: string;
  materials: ShaderMaterial[];
  parameters: Record<string, unknown>;
  duration: number;
  loop: boolean;
  speed: number;
  intensity: number;
  enabled: boolean;
  blendWithScene: boolean;
  onUpdate?: (time: number, deltaTime: number) => void;
  onComplete?: () => void;
}

export interface PostProcessingEffect extends ShaderEffect {
  inputTexture: ShaderTexture;
  outputTexture: ShaderTexture;
  passes: RenderPass[];
}

export interface RenderPass {
  id: string;
  shader: ShaderProgram;
  inputs: Map<string, ShaderTexture>;
  output: RenderTarget;
  enabled: boolean;
  clear: boolean;
  clearColor?: Color;
  viewport?: Vec4;
}

export interface RenderTarget {
  id: string;
  width: number;
  height: number;
  framebuffer: WebGLFramebuffer;
  depthBuffer: WebGLRenderbuffer | null;
  textures: Map<string, ShaderTexture>;
  multisampled: boolean;
  samples: number;
}

export interface Color {
  r: number;
  g: number;
  b: number;
  a: number;
}

export interface Vec4 {
  x: number;
  y: number;
  z: number;
  w: number;
}

export interface ShaderLibrary {
  shaders: Map<string, ShaderProgram>;
  materials: Map<string, ShaderMaterial>;
  effects: Map<string, ShaderEffect>;
  postProcessing: Map<string, PostProcessingEffect>;
  renderTargets: Map<string, RenderTarget>;
  globalUniforms: Map<string, ShaderUniform>;
}

export class ShaderManager {
  private gl: WebGLRenderingContext | WebGL2RenderingContext | null = null;
  private shaders: Map<string, ShaderProgram> = new Map();
  private materials: Map<string, ShaderMaterial> = new Map();
  private effects: Map<string, ShaderEffect> = new Map();
  private postProcessing: Map<string, PostProcessingEffect> = new Map();
  private renderTargets: Map<string, RenderTarget> = new Map();
  private globalUniforms: Map<string, ShaderUniform> = new Map();
  private activeShader: ShaderProgram | null = null;
  private cacheEnabled: boolean = true;
  private compilationCache: Map<string, string> = new Map();

  constructor() {}

  public initialize(gl: WebGLRenderingContext | WebGL2RenderingContext): void {
    this.gl = gl;
    this.registerBuiltInShaders();
  }

  private registerBuiltInShaders(): void {
    this.registerShader({
      id: 'basic',
      name: 'Basic Shader',
      vertexShader: `
        attribute vec3 position;
        attribute vec2 uv;
        uniform mat4 modelMatrix;
        uniform mat4 viewMatrix;
        uniform mat4 projectionMatrix;
        varying vec2 vUv;
        void main() {
          vUv = uv;
          gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(position, 1.0);
        }
      `,
      fragmentShader: `
        uniform vec4 color;
        uniform sampler2D map;
        varying vec2 vUv;
        void main() {
          gl_FragColor = color * texture2D(map, vUv);
        }
      `,
      uniforms: [
        { name: 'modelMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'viewMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'projectionMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'color', type: UniformType.VEC4, value: [1, 1, 1, 1] },
        { name: 'map', type: UniformType.SAMPLER2D, value: null },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC3, size: 3 },
        { name: 'uv', type: AttributeType.VEC2, size: 2 },
      ],
      renderStates: [],
      defines: {},
    });

    this.registerShader({
      id: 'pbr',
      name: 'PBR Shader',
      vertexShader: `
        attribute vec3 position;
        attribute vec3 normal;
        attribute vec2 uv;
        uniform mat4 modelMatrix;
        uniform mat4 viewMatrix;
        uniform mat4 projectionMatrix;
        uniform mat4 normalMatrix;
        varying vec3 vPosition;
        varying vec3 vNormal;
        varying vec2 vUv;
        varying vec2 vClipPosition;
        void main() {
          vPosition = (modelMatrix * vec4(position, 1.0)).xyz;
          vNormal = normalize(normalMatrix * normal);
          vUv = uv;
          vec4 clipPosition = projectionMatrix * viewMatrix * vec4(vPosition, 1.0);
          vClipPosition = clipPosition.xy / clipPosition.w;
          gl_Position = clipPosition;
        }
      `,
      fragmentShader: `
        uniform vec3 cameraPosition;
        uniform vec3 albedo;
        uniform float metalness;
        uniform float roughness;
        uniform float opacity;
        uniform vec3 emissive;
        uniform vec3 lightPosition;
        uniform vec3 lightColor;
        uniform float lightIntensity;
        uniform sampler2D albedoMap;
        uniform sampler2D normalMap;
        uniform sampler2D metallicMap;
        uniform sampler2D roughnessMap;
        uniform sampler2D aoMap;
        uniform sampler2D emissiveMap;
        uniform vec2 texelSize;
        varying vec3 vPosition;
        varying vec3 vNormal;
        varying vec2 vUv;
        varying vec2 vClipPosition;
        const float PI = 3.14159265359;
        float DistributionGGX(vec3 N, vec3 H, float roughness) {
          float a = roughness * roughness;
          float a2 = a * a;
          float NdotH = max(dot(N, H), 0.0);
          float NdotH2 = NdotH * NdotH;
          float nom = a2;
          float denom = (NdotH2 * (a2 - 1.0) + 1.0);
          denom = PI * denom * denom;
          return nom / denom;
        }
        float GeometrySchlickGGX(float NdotV, float roughness) {
          float r = roughness + 1.0;
          float k = (r * r) / 8.0;
          float nom = NdotV;
          float denom = NdotV * (1.0 - k) + k;
          return nom / denom;
        }
        float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness) {
          float NdotV = max(dot(N, V), 0.0);
          float NdotL = max(dot(N, L), 0.0);
          float ggx2 = GeometrySchlickGGX(NdotV, roughness);
          float ggx1 = GeometrySchlickGGX(NdotL, roughness);
          return ggx1 * ggx2;
        }
        vec3 fresnelSchlick(float cosTheta, vec3 F0) {
          return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
        }
        vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness) {
          return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
        }
        void main() {
          vec4 albedoTex = texture2D(albedoMap, vUv);
          vec3 albedoColor = albedoTex.rgb * albedo;
          float metallic = texture2D(metallicMap, vUv).r * metalness;
          float roughness = texture2D(roughnessMap, vUv).r * roughness;
          float ao = texture2D(aoMap, vUv).r;
          vec3 emissiveColor = texture2D(emissiveMap, vUv).rgb * emissive;
          vec3 N = normalize(vNormal);
          vec3 V = normalize(cameraPosition - vPosition);
          vec3 F0 = vec3(0.04);
          F0 = mix(F0, albedoColor, metallic);
          vec3 Lo = vec3(0.0);
          vec3 L = normalize(lightPosition - vPosition);
          vec3 H = normalize(V + L);
          float distance = length(lightPosition - vPosition);
          float attenuation = 1.0 / (distance * distance);
          vec3 radiance = lightColor * lightIntensity * attenuation;
          float NDF = DistributionGGX(N, H, roughness);
          float G = GeometrySmith(N, V, L, roughness);
          vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);
          vec3 numerator = NDF * G * F;
          float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
          vec3 specular = numerator / denominator;
          vec3 kS = F;
          vec3 kD = vec3(1.0) - kS;
          kD *= 1.0 - metallic;
          float NdotL = max(dot(N, L), 0.0);
          Lo += (kD * albedoColor / PI + specular) * radiance * NdotL;
          vec3 ambient = vec3(0.03) * albedoColor * ao;
          vec3 color = ambient + Lo + emissiveColor;
          color = color / (color + vec3(1.0));
          color = pow(color, vec3(1.0/2.2));
          gl_FragColor = vec4(color, opacity * albedoTex.a);
        }
      `,
      uniforms: [
        { name: 'modelMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'viewMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'projectionMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'normalMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'cameraPosition', type: UniformType.VEC3, value: [0, 0, 0] },
        { name: 'albedo', type: UniformType.VEC3, value: [1, 1, 1] },
        { name: 'metalness', type: UniformType.FLOAT, value: 0 },
        { name: 'roughness', type: UniformType.FLOAT, value: 0.5 },
        { name: 'opacity', type: UniformType.FLOAT, value: 1 },
        { name: 'emissive', type: UniformType.VEC3, value: [0, 0, 0] },
        { name: 'lightPosition', type: UniformType.VEC3, value: [0, 10, 0] },
        { name: 'lightColor', type: UniformType.VEC3, value: [1, 1, 1] },
        { name: 'lightIntensity', type: UniformType.FLOAT, value: 1 },
        { name: 'albedoMap', type: UniformType.SAMPLER2D, value: null },
        { name: 'normalMap', type: UniformType.SAMPLER2D, value: null },
        { name: 'metallicMap', type: UniformType.SAMPLER2D, value: null },
        { name: 'roughnessMap', type: UniformType.SAMPLER2D, value: null },
        { name: 'aoMap', type: UniformType.SAMPLER2D, value: null },
        { name: 'emissiveMap', type: UniformType.SAMPLER2D, value: null },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC3, size: 3 },
        { name: 'normal', type: AttributeType.VEC3, size: 3 },
        { name: 'uv', type: AttributeType.VEC2, size: 2 },
      ],
      renderStates: [],
      defines: {},
    });

    this.registerShader({
      id: 'skybox',
      name: 'Skybox Shader',
      vertexShader: `
        attribute vec3 position;
        uniform mat4 projectionMatrix;
        uniform mat4 viewMatrix;
        varying vec3 vPosition;
        void main() {
          vPosition = position;
          mat4 viewNoTranslation = mat4(mat3(viewMatrix));
          vec4 pos = projectionMatrix * viewNoTranslation * vec4(position, 1.0);
          gl_Position = pos.xyww;
        }
      `,
      fragmentShader: `
        uniform samplerCube skybox;
        uniform float intensity;
        varying vec3 vPosition;
        void main() {
          vec4 color = textureCube(skybox, vPosition);
          gl_FragColor = color * intensity;
        }
      `,
      uniforms: [
        { name: 'projectionMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'viewMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'skybox', type: UniformType.SAMPLER_CUBE, value: null },
        { name: 'intensity', type: UniformType.FLOAT, value: 1 },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC3, size: 3 },
      ],
      renderStates: [
        { enable: false, func: RenderStateFunction.DEPTH_TEST, value: true },
      ],
      defines: {},
    });

    this.registerShader({
      id: 'outline',
      name: 'Outline Shader',
      vertexShader: `
        attribute vec3 position;
        attribute vec3 normal;
        uniform mat4 modelMatrix;
        uniform mat4 viewMatrix;
        uniform mat4 projectionMatrix;
        uniform float outlineWidth;
        uniform float outlineColor;
        void main() {
          vec3 pos = position + normal * outlineWidth;
          gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(pos, 1.0);
        }
      `,
      fragmentShader: `
        uniform float outlineColor;
        void main() {
          gl_FragColor = vec4(outlineColor, outlineColor, outlineColor, 1.0);
        }
      `,
      uniforms: [
        { name: 'modelMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'viewMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'projectionMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'outlineWidth', type: UniformType.FLOAT, value: 0.05 },
        { name: 'outlineColor', type: UniformType.FLOAT, value: 0 },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC3, size: 3 },
        { name: 'normal', type: AttributeType.VEC3, size: 3 },
      ],
      renderStates: [],
      defines: {},
    });

    this.registerShader({
      id: 'water',
      name: 'Water Shader',
      vertexShader: `
        attribute vec3 position;
        attribute vec3 normal;
        attribute vec2 uv;
        uniform mat4 modelMatrix;
        uniform mat4 viewMatrix;
        uniform mat4 projectionMatrix;
        uniform float time;
        varying vec3 vWorldPosition;
        varying vec3 vNormal;
        varying vec2 vUv;
        varying vec2 vClipPosition;
        void main() {
          vec3 pos = position;
          pos.y += sin(pos.x * 2.0 + time) * 0.1 + sin(pos.z * 2.0 + time * 0.8) * 0.1;
          vWorldPosition = (modelMatrix * vec4(pos, 1.0)).xyz;
          vNormal = normalize(normalMatrix * normal);
          vUv = uv;
          vec4 clipPosition = projectionMatrix * viewMatrix * vec4(vWorldPosition, 1.0);
          vClipPosition = clipPosition.xy / clipPosition.w;
          gl_Position = clipPosition;
        }
      `,
      fragmentShader: `
        uniform vec3 cameraPosition;
        uniform vec3 waterColor;
        uniform vec3 deepColor;
        uniform float opacity;
        uniform vec3 lightDirection;
        uniform vec3 lightColor;
        uniform float time;
        uniform sampler2D reflectionMap;
        uniform sampler2D refractionMap;
        uniform sampler2D normalMap;
        varying vec3 vWorldPosition;
        varying vec3 vNormal;
        varying vec2 vUv;
        varying vec2 vClipPosition;
        const float PI = 3.14159265359;
        vec3 fresnelSchlick(float cosTheta, vec3 F0) {
          return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
        }
        void main() {
          vec3 N = normalize(vNormal);
          vec3 V = normalize(cameraPosition - vWorldPosition);
          vec3 L = normalize(lightDirection);
          vec3 H = normalize(V + L);
          vec2 normalOffset = vec2(sin(time), cos(time)) * 0.02;
          vec3 normalTex = texture2D(normalMap, vUv + normalOffset).rgb * 2.0 - 1.0;
          N = normalize(N + normalTex * 0.1);
          float fresnel = fresnelSchlick(max(dot(N, V), 0.0), vec3(0.02)).r;
          vec3 reflection = texture2D(reflectionMap, vClipPosition + fresnel * 0.1).rgb;
          vec3 refraction = texture2D(refractionMap, vClipPosition - fresnel * 0.1).rgb;
          float depth = length(vWorldPosition - cameraPosition) / 50.0;
          vec3 color = mix(refraction, reflection, fresnel);
          float NdotL = max(dot(N, L), 0.0);
          vec3 diffuse = waterColor * NdotL * lightColor;
          float specular = pow(max(dot(N, H), 0.0), 64.0);
          vec3 specularColor = specular * lightColor;
          color = color + diffuse + specularColor;
          float alpha = mix(opacity, 1.0, fresnel);
          gl_FragColor = vec4(color, alpha);
        }
      `,
      uniforms: [
        { name: 'modelMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'viewMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'projectionMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'cameraPosition', type: UniformType.VEC3, value: [0, 0, 0] },
        { name: 'waterColor', type: UniformType.VEC3, value: [0.1, 0.3, 0.6] },
        { name: 'deepColor', type: UniformType.VEC3, value: [0.0, 0.1, 0.3] },
        { name: 'opacity', type: UniformType.FLOAT, value: 0.8 },
        { name: 'lightDirection', type: UniformType.VEC3, value: [0.5, 1, 0.3] },
        { name: 'lightColor', type: UniformType.VEC3, value: [1, 1, 0.9] },
        { name: 'time', type: UniformType.FLOAT, value: 0 },
        { name: 'reflectionMap', type: UniformType.SAMPLER2D, value: null },
        { name: 'refractionMap', type: UniformType.SAMPLER2D, value: null },
        { name: 'normalMap', type: UniformType.SAMPLER2D, value: null },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC3, size: 3 },
        { name: 'normal', type: AttributeType.VEC3, size: 3 },
        { name: 'uv', type: AttributeType.VEC2, size: 2 },
      ],
      renderStates: [
        { enable: true, func: RenderStateFunction.BLEND, value: true },
      ],
      defines: {},
    });

    this.registerShader({
      id: 'particles',
      name: 'Particle Shader',
      vertexShader: `
        attribute vec3 position;
        attribute vec4 color;
        attribute float size;
        attribute float alpha;
        uniform mat4 modelMatrix;
        uniform mat4 viewMatrix;
        uniform mat4 projectionMatrix;
        uniform float pixelRatio;
        varying vec4 vColor;
        varying float vAlpha;
        varying float vSize;
        void main() {
          vColor = color;
          vAlpha = alpha;
          vSize = size;
          vec4 mvPosition = modelMatrix * vec4(position, 1.0);
          gl_Position = projectionMatrix * viewMatrix * mvPosition;
          gl_PointSize = size * pixelRatio * (300.0 / -mvPosition.z);
        }
      `,
      fragmentShader: `
        uniform sampler2D texture;
        varying vec4 vColor;
        varying float vAlpha;
        varying float vSize;
        void main() {
          vec2 center = gl_PointCoord - vec2(0.5);
          float dist = length(center);
          if (dist > 0.5) discard;
          vec4 texColor = texture2D(texture, gl_PointCoord);
          gl_FragColor = texColor * vColor * vAlpha;
        }
      `,
      uniforms: [
        { name: 'modelMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'viewMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'projectionMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'pixelRatio', type: UniformType.FLOAT, value: 1 },
        { name: 'texture', type: UniformType.SAMPLER2D, value: null },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC3, size: 3 },
        { name: 'color', type: AttributeType.VEC4, size: 4 },
        { name: 'size', type: AttributeType.FLOAT, size: 1 },
        { name: 'alpha', type: UniformType.FLOAT, size: 1 },
      ],
      renderStates: [
        { enable: true, func: RenderStateFunction.BLEND, value: true },
        { enable: false, func: RenderStateFunction.DEPTH_TEST, value: false },
      ],
      defines: {},
    });

    this.registerShader({
      id: 'bloom',
      name: 'Bloom Post-Processing',
      vertexShader: `
        attribute vec2 position;
        attribute vec2 uv;
        varying vec2 vUv;
        void main() {
          vUv = uv;
          gl_Position = vec4(position, 0.0, 1.0);
        }
      `,
      fragmentShader: `
        uniform sampler2D sceneTexture;
        uniform vec2 resolution;
        uniform float threshold;
        uniform float intensity;
        uniform float radius;
        varying vec2 vUv;
        void main() {
          vec4 color = texture2D(sceneTexture, vUv);
          float brightness = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));
          vec4 bloom = vec4(0.0);
          float total = 0.0;
          for (float x = -4.0; x <= 4.0; x++) {
            for (float y = -4.0; y <= 4.0; y++) {
              vec2 offset = vec2(x, y) * radius / resolution;
              vec4 sample = texture2D(sceneTexture, vUv + offset);
              float b = dot(sample.rgb, vec3(0.2126, 0.7152, 0.0722));
              if (b > threshold) {
                float weight = 1.0 / (1.0 + length(vec2(x, y)));
                bloom += sample * weight;
                total += weight;
              }
            }
          }
          bloom /= total;
          gl_FragColor = color + bloom * intensity;
        }
      `,
      uniforms: [
        { name: 'sceneTexture', type: UniformType.SAMPLER2D, value: null },
        { name: 'resolution', type: UniformType.VEC2, value: [1920, 1080] },
        { name: 'threshold', type: UniformType.FLOAT, value: 0.8 },
        { name: 'intensity', type: UniformType.FLOAT, value: 1.5 },
        { name: 'radius', type: UniformType.FLOAT, value: 4 },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC2, size: 2 },
        { name: 'uv', type: AttributeType.VEC2, size: 2 },
      ],
      renderStates: [],
      defines: {},
    });

    this.registerShader({
      id: 'blur',
      name: 'Gaussian Blur',
      vertexShader: `
        attribute vec2 position;
        attribute vec2 uv;
        varying vec2 vUv;
        void main() {
          vUv = uv;
          gl_Position = vec4(position, 0.0, 1.0);
        }
      `,
      fragmentShader: `
        uniform sampler2D sceneTexture;
        uniform vec2 resolution;
        uniform vec2 direction;
        uniform float radius;
        varying vec2 vUv;
        void main() {
          vec4 color = vec4(0.0);
          float total = 0.0;
          float offset = radius / min(resolution.x, resolution.y);
          for (float x = -4.0; x <= 4.0; x++) {
            float weight = exp(-0.5 * pow(x / 2.0, 2.0));
            color += texture2D(sceneTexture, vUv + direction * x * offset) * weight;
            total += weight;
          }
          gl_FragColor = color / total;
        }
      `,
      uniforms: [
        { name: 'sceneTexture', type: UniformType.SAMPLER2D, value: null },
        { name: 'resolution', type: UniformType.VEC2, value: [1920, 1080] },
        { name: 'direction', type: UniformType.VEC2, value: [1, 0] },
        { name: 'radius', type: UniformType.FLOAT, value: 4 },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC2, size: 2 },
        { name: 'uv', type: AttributeType.VEC2, size: 2 },
      ],
      renderStates: [],
      defines: {},
    });

    this.registerShader({
      id: 'depth_of_field',
      name: 'Depth of Field',
      vertexShader: `
        attribute vec2 position;
        attribute vec2 uv;
        varying vec2 vUv;
        void main() {
          vUv = uv;
          gl_Position = vec4(position, 0.0, 1.0);
        }
      `,
      fragmentShader: `
        uniform sampler2D sceneTexture;
        uniform sampler2D depthTexture;
        uniform float focusDistance;
        uniform float focalLength;
        uniform float aperture;
        uniform float maxBlur;
        uniform vec2 resolution;
        varying vec2 vUv;
        float getDepth(vec2 uv) {
          float depth = texture2D(depthTexture, uv).r;
          float z = focalLength * focusDistance / (focusDistance - depth * (focalLength - focusDistance));
          return clamp(z, 0.0, 1000.0);
        }
        void main() {
          float depth = getDepth(vUv);
          float coc = abs(focalLength * aperture * (focusDistance - depth) / (depth * (focusDistance - focalLength)));
          vec4 color = vec4(0.0);
          float total = 0.0;
          float blur = min(coc, maxBlur);
          for (float x = -4.0; x <= 4.0; x++) {
            for (float y = -4.0; y <= 4.0; y++) {
              vec2 offset = vec2(x, y) * blur / resolution;
              float weight = 1.0 - length(vec2(x, y)) / 5.66;
              color += texture2D(sceneTexture, vUv + offset) * weight;
              total += weight;
            }
          }
          gl_FragColor = color / total;
        }
      `,
      uniforms: [
        { name: 'sceneTexture', type: UniformType.SAMPLER2D, value: null },
        { name: 'depthTexture', type: UniformType.SAMPLER2D, value: null },
        { name: 'focusDistance', type: UniformType.FLOAT, value: 10 },
        { name: 'focalLength', type: UniformType.FLOAT, value: 0.05 },
        { name: 'aperture', type: UniformType.FLOAT, value: 0.025 },
        { name: 'maxBlur', type: UniformType.FLOAT, value: 10 },
        { name: 'resolution', type: UniformType.VEC2, value: [1920, 1080] },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC2, size: 2 },
        { name: 'uv', type: AttributeType.VEC2, size: 2 },
      ],
      renderStates: [],
      defines: {},
    });

    this.registerShader({
      id: 'vignette',
      name: 'Vignette Effect',
      vertexShader: `
        attribute vec2 position;
        attribute vec2 uv;
        varying vec2 vUv;
        void main() {
          vUv = uv;
          gl_Position = vec4(position, 0.0, 1.0);
        }
      `,
      fragmentShader: `
        uniform sampler2D sceneTexture;
        uniform float intensity;
        uniform float smoothness;
        uniform vec2 offset;
        varying vec2 vUv;
        void main() {
          vec4 color = texture2D(sceneTexture, vUv);
          vec2 center = vUv - 0.5 + offset;
          float dist = length(center);
          float vignette = smoothstep(0.8 - smoothness, 0.2 - smoothness, dist * (intensity + smoothness));
          gl_FragColor = vec4(color.rgb * vignette, color.a);
        }
      `,
      uniforms: [
        { name: 'sceneTexture', type: UniformType.SAMPLER2D, value: null },
        { name: 'intensity', type: UniformType.FLOAT, value: 1 },
        { name: 'smoothness', type: UniformType.FLOAT, value: 0.5 },
        { name: 'offset', type: UniformType.VEC2, value: [0, 0] },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC2, size: 2 },
        { name: 'uv', type: AttributeType.VEC2, size: 2 },
      ],
      renderStates: [],
      defines: {},
    });

    this.registerShader({
      id: 'color_correction',
      name: 'Color Correction',
      vertexShader: `
        attribute vec2 position;
        attribute vec2 uv;
        varying vec2 vUv;
        void main() {
          vUv = uv;
          gl_Position = vec4(position, 0.0, 1.0);
        }
      `,
      fragmentShader: `
        uniform sampler2D sceneTexture;
        uniform float brightness;
        uniform float contrast;
        uniform float saturation;
        uniform vec3 tint;
        uniform float exposure;
        uniform float gamma;
        varying vec2 vUv;
        vec3 rgb2hsv(vec3 c) {
          vec4 K = vec4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
          vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
          vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
          float d = q.x - min(q.w, q.y);
          float e = 1.0e-10;
          return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
        }
        vec3 hsv2rgb(vec3 c) {
          vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
          vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
          return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
        }
        void main() {
          vec4 color = texture2D(sceneTexture, vUv);
          color.rgb *= exposure;
          color.rgb = max(vec3(0.0), color.rgb - vec3(0.5)) * contrast + vec3(0.5) + brightness;
          vec3 hsv = rgb2hsv(color.rgb);
          hsv.y *= saturation;
          color.rgb = hsv2rgb(hsv);
          color.rgb *= tint;
          color.rgb = pow(color.rgb, vec3(1.0 / gamma));
          gl_FragColor = color;
        }
      `,
      uniforms: [
        { name: 'sceneTexture', type: UniformType.SAMPLER2D, value: null },
        { name: 'brightness', type: UniformType.FLOAT, value: 0 },
        { name: 'contrast', type: UniformType.FLOAT, value: 1 },
        { name: 'saturation', type: UniformType.FLOAT, value: 1 },
        { name: 'tint', type: UniformType.VEC3, value: [1, 1, 1] },
        { name: 'exposure', type: UniformType.FLOAT, value: 1 },
        { name: 'gamma', type: UniformType.FLOAT, value: 1 },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC2, size: 2 },
        { name: 'uv', type: AttributeType.VEC2, size: 2 },
      ],
      renderStates: [],
      defines: {},
    });

    this.registerShader({
      id: 'fog',
      name: 'Fog Effect',
      vertexShader: `
        attribute vec3 position;
        attribute vec3 normal;
        attribute float depth;
        uniform mat4 modelMatrix;
        uniform mat4 viewMatrix;
        uniform mat4 projectionMatrix;
        varying float vDepth;
        varying float vDistance;
        void main() {
          vec4 mvPosition = viewMatrix * modelMatrix * vec4(position, 1.0);
          vDepth = -mvPosition.z;
          vDistance = length(position);
          gl_Position = projectionMatrix * mvPosition;
        }
      `,
      fragmentShader: `
        uniform vec3 fogColor;
        uniform float fogDensity;
        uniform float fogStart;
        uniform float fogEnd;
        uniform float fogAmount;
        uniform vec3 lightDirection;
        varying float vDepth;
        varying float vDistance;
        void main() {
          float fogFactor = 1.0 - exp(-fogDensity * vDepth);
          fogFactor = clamp((fogEnd - vDepth) / (fogEnd - fogStart), 0.0, 1.0);
          vec3 color = mix(fogColor, vec3(0.0), fogFactor);
          gl_FragColor = vec4(color, fogAmount * fogFactor);
        }
      `,
      uniforms: [
        { name: 'modelMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'viewMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'projectionMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'fogColor', type: UniformType.VEC3, value: [0.5, 0.6, 0.7] },
        { name: 'fogDensity', type: UniformType.FLOAT, value: 0.1 },
        { name: 'fogStart', type: UniformType.FLOAT, value: 10 },
        { name: 'fogEnd', type: UniformType.FLOAT, value: 100 },
        { name: 'fogAmount', type: UniformType.FLOAT, value: 1 },
        { name: 'lightDirection', type: UniformType.VEC3, value: [0.5, 1, 0.3] },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC3, size: 3 },
        { name: 'normal', type: AttributeType.VEC3, size: 3 },
        { name: 'depth', type: UniformType.FLOAT, size: 1 },
      ],
      renderStates: [],
      defines: {},
    });

    this.registerShader({
      id: 'grass',
      name: 'Grass Shader',
      vertexShader: `
        attribute vec3 position;
        attribute vec2 uv;
        uniform mat4 modelMatrix;
        uniform mat4 viewMatrix;
        uniform mat4 projectionMatrix;
        uniform float time;
        uniform float windStrength;
        uniform vec2 windDirection;
        varying vec2 vUv;
        varying vec3 vWorldPosition;
        void main() {
          vUv = uv;
          vec3 pos = position;
          float wind = sin(pos.x * 0.5 + time) * cos(pos.z * 0.5 + time) * windStrength;
          pos.x += wind * windDirection.x * (1.0 - uv.y);
          pos.z += wind * windDirection.y * (1.0 - uv.y);
          vWorldPosition = (modelMatrix * vec4(pos, 1.0)).xyz;
          gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(pos, 1.0);
        }
      `,
      fragmentShader: `
        uniform vec3 grassColor;
        uniform vec3 tipColor;
        uniform vec3 lightDirection;
        uniform float time;
        varying vec2 vUv;
        varying vec3 vWorldPosition;
        void main() {
          vec3 color = mix(grassColor, tipColor, vUv.y);
          float NdotL = dot(vec3(0.0, 1.0, 0.0), lightDirection);
          color *= 0.5 + 0.5 * NdotL;
          float ambient = 0.3;
          color *= ambient + (1.0 - ambient) * NdotL;
          gl_FragColor = vec4(color, 1.0);
        }
      `,
      uniforms: [
        { name: 'modelMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'viewMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'projectionMatrix', type: UniformType.MAT4, value: new Float32Array(16) },
        { name: 'time', type: UniformType.FLOAT, value: 0 },
        { name: 'windStrength', type: UniformType.FLOAT, value: 0.5 },
        { name: 'windDirection', type: UniformType.VEC2, value: [1, 0] },
        { name: 'grassColor', type: UniformType.VEC3, value: [0.2, 0.4, 0.1] },
        { name: 'tipColor', type: UniformType.VEC3, value: [0.4, 0.6, 0.2] },
        { name: 'lightDirection', type: UniformType.VEC3, value: [0.5, 1, 0.3] },
      ],
      attributes: [
        { name: 'position', type: AttributeType.VEC3, size: 3 },
        { name: 'uv', type: AttributeType.VEC2, size: 2 },
      ],
      renderStates: [],
      defines: {},
    });
  }

  public registerShader(config: Omit<ShaderConfig, 'version'>): ShaderProgram | null {
    if (!this.gl) return null;

    const vertexShader = this.createShader(this.gl.VERTEX_SHADER, config.vertexShader);
    if (!vertexShader) return null;

    const fragmentShader = this.createShader(this.gl.FRAGMENT_SHADER, config.fragmentShader);
    if (!fragmentShader) {
      this.gl.deleteShader(vertexShader);
      return null;
    }

    const program = this.gl.createProgram();
    if (!program) return null;

    this.gl.attachShader(program, vertexShader);
    this.gl.attachShader(program, fragmentShader);

    const attributes = new Map<string, ShaderAttribute>();
    config.attributes.forEach(attr => {
      this.gl!.bindAttribLocation(program, attributes.size, attr.name);
      attributes.set(attr.name, { ...attr, location: attributes.size });
    });

    const linkStart = performance.now();
    this.gl.linkProgram(program);
    const compilationTime = performance.now() - linkStart;

    if (!this.gl.getProgramParameter(program, this.gl.LINK_STATUS)) {
      console.error('Shader linking error:', this.gl.getProgramInfoLog(program));
      this.gl.deleteProgram(program);
      this.gl.deleteShader(vertexShader);
      this.gl.deleteShader(fragmentShader);
      return null;
    }

    const uniforms = new Map<string, ShaderUniform>();
    config.uniforms.forEach(uniform => {
      const location = this.gl!.getUniformLocation(program, uniform.name);
      uniforms.set(uniform.name, { ...uniform, location });
    });

    const shaderProgram: ShaderProgram = {
      id: config.uniforms[0]?.value?.[0] as string || `shader_${Date.now()}`,
      name: config.uniforms[0]?.value?.[0] as string || 'Unnamed Shader',
      program,
      vertexShader,
      fragmentShader,
      uniforms,
      attributes,
      sources: { vertex: config.vertexShader, fragment: config.fragmentShader },
      compiled: true,
      compilationTime,
      lastUsed: Date.now(),
      useCount: 0,
    };

    const shaderId = this.getShaderId(config);
    this.shaders.set(shaderId, shaderProgram);
    this.compilationCache.set(shaderId, shaderId);

    return shaderProgram;
  }

  private createShader(type: number, source: string): WebGLShader | null {
    if (!this.gl) return null;

    const shader = this.gl.createShader(type);
    if (!shader) return null;

    this.gl.shaderSource(shader, source);
    this.gl.compileShader(shader);

    if (!this.gl.getShaderParameter(shader, this.gl.COMPILE_STATUS)) {
      console.error('Shader compilation error:', this.gl.getShaderInfoLog(shader));
      this.gl.deleteShader(shader);
      return null;
    }

    return shader;
  }

  private getShaderId(config: Partial<ShaderConfig>): string {
    return `${config.id || 'shader'}_${Date.now()}`;
  }

  public useShader(shaderId: string): boolean {
    const shader = this.shaders.get(shaderId);
    if (!shader) return false;

    if (this.activeShader !== shader) {
      this.gl!.useProgram(shader.program);
      this.activeShader = shader;
      shader.lastUsed = Date.now();
      shader.useCount++;
    }

    return true;
  }

  public setUniform(name: string, value: UniformValue): void {
    if (!this.activeShader) return;

    const uniform = this.activeShader.uniforms.get(name);
    if (!uniform || uniform.location === null) return;

    this.setUniformValue(uniform, value);
  }

  private setUniformValue(uniform: ShaderUniform, value: UniformValue): void {
    if (!this.gl || uniform.location === null) return;

    switch (uniform.type) {
      case UniformType.FLOAT:
        this.gl.uniform1f(uniform.location, value as number);
        break;
      case UniformType.INT:
        this.gl.uniform1i(uniform.location, value as number);
        break;
      case UniformType.BOOL:
        this.gl.uniform1i(uniform.location, value as number ? 1 : 0);
        break;
      case UniformType.VEC2:
        const v2 = value as number[];
        this.gl.uniform2fv(uniform.location, v2);
        break;
      case UniformType.VEC3:
        const v3 = value as number[];
        this.gl.uniform3fv(uniform.location, v3);
        break;
      case UniformType.VEC4:
        const v4 = value as number[];
        this.gl.uniform4fv(uniform.location, v4);
        break;
      case UniformType.MAT2:
        this.gl.uniformMatrix2fv(uniform.location, false, value as Float32Array);
        break;
      case UniformType.MAT3:
        this.gl.uniformMatrix3fv(uniform.location, false, value as Float32Array);
        break;
      case UniformType.MAT4:
        this.gl.uniformMatrix4fv(uniform.location, false, value as Float32Array);
        break;
      case UniformType.SAMPLER2D:
        this.gl.uniform1i(uniform.location, value as number);
        break;
      case UniformType.SAMPLER_CUBE:
        this.gl.uniform1i(uniform.location, value as number);
        break;
    }

    uniform.value = value;
  }

  public createMaterial(shaderId: string, properties?: Partial<ShaderMaterial>): ShaderMaterial | null {
    const shader = this.shaders.get(shaderId);
    if (!shader) return null;

    const material: ShaderMaterial = {
      id: `material_${Date.now()}`,
      name: properties?.name || `Material ${this.materials.size + 1}`,
      shaderId,
      uniforms: {},
      textures: new Map(),
      renderQueue: properties?.renderQueue || { type: QueueType.GEOMETRY, priority: 0 },
      properties: properties?.properties || {
        transparent: false,
        doubleSided: false,
        depthWrite: true,
        depthTest: true,
        colorWrite: true,
        blendMode: BlendMode.NONE,
        roughness: 0.5,
        metalness: 0,
        emission: 0,
        opacity: 1,
      },
      keywords: new Set(),
    };

    shader.uniforms.forEach((uniform, name) => {
      material.uniforms[name] = uniform.value;
    });

    this.materials.set(material.id, material);
    return material;
  }

  public getMaterial(materialId: string): ShaderMaterial | undefined {
    return this.materials.get(materialId);
  }

  public applyMaterial(materialId: string): boolean {
    const material = this.materials.get(materialId);
    if (!material) return false;

    if (!this.useShader(material.shaderId)) return false;

    Object.entries(material.uniforms).forEach(([name, value]) => {
      this.setUniform(name, value);
    });

    return true;
  }

  public createRenderTarget(width: number, height: number, options?: Partial<RenderTargetOptions>): RenderTarget | null {
    if (!this.gl) return null;

    const framebuffer = this.gl.createFramebuffer();
    if (!framebuffer) return null;

    this.gl.bindFramebuffer(this.gl.FRAMEBUFFER, framebuffer);

    const colorTexture = this.createTexture(width, height, options?.format || TextureFormat.RGBA16F);
    if (!colorTexture) return null;

    this.gl.framebufferTexture2D(
      this.gl.FRAMEBUFFER,
      this.gl.COLOR_ATTACHMENT0,
      this.gl.TEXTURE_2D,
      colorTexture.texture,
      0
    );

    let depthBuffer: WebGLRenderbuffer | null = null;
    if (options?.depthBuffer) {
      depthBuffer = this.gl.createRenderbuffer()!;
      this.gl.bindRenderbuffer(this.gl.RENDERBUFFER, depthBuffer);
      this.gl.renderbufferStorage(
        this.gl.RENDERBUFFER,
        this.gl.DEPTH_COMPONENT16,
        width,
        height
      );
      this.gl.framebufferRenderbuffer(
        this.gl.FRAMEBUFFER,
        this.gl.DEPTH_ATTACHMENT,
        this.gl.RENDERBUFFER,
        depthBuffer
      );
    }

    this.gl.bindFramebuffer(this.gl.FRAMEBUFFER, null);

    const textures = new Map<string, ShaderTexture>();
    textures.set('color', colorTexture);

    const renderTarget: RenderTarget = {
      id: `rt_${Date.now()}`,
      width,
      height,
      framebuffer,
      depthBuffer,
      textures,
      multisampled: options?.multisampled || false,
      samples: options?.samples || 1,
    };

    this.renderTargets.set(renderTarget.id, renderTarget);
    return renderTarget;
  }

  private createTexture(
    width: number,
    height: number,
    format: TextureFormat
  ): ShaderTexture | null {
    if (!this.gl) return null;

    const texture = this.gl.createTexture();
    if (!texture) return null;

    this.gl.bindTexture(this.gl.TEXTURE_2D, texture);
    this.gl.texImage2D(
      this.gl.TEXTURE_2D,
      0,
      this.getTextureInternalFormat(format),
      width,
      height,
      0,
      this.getTextureFormat(format),
      this.getTextureType(format),
      null
    );

    this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_WRAP_S, this.gl.CLAMP_TO_EDGE);
    this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_WRAP_T, this.gl.CLAMP_TO_EDGE);
    this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_MIN_FILTER, this.gl.LINEAR);
    this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_MAG_FILTER, this.gl.LINEAR);

    const tex = {
      unit: 0,
      texture,
      target: TextureTarget.TEXTURE_2D,
      width,
      height,
      format,
      type: TextureDataType.FLOAT,
      wrapS: TextureWrap.CLAMP_TO_EDGE,
      wrapT: TextureWrap.CLAMP_TO_EDGE,
      minFilter: TextureFilter.LINEAR,
      magFilter: TextureFilter.LINEAR,
      mipmap: false,
      anisotropy: 1,
    };

    return tex;
  }

  private getTextureInternalFormat(format: TextureFormat): number {
    if (!this.gl) return 0;
    if (this.gl instanceof WebGL2RenderingContext) {
      switch (format) {
        case TextureFormat.RGBA16F: return (this.gl as WebGL2RenderingContext).RGBA16F;
        case TextureFormat.RGBA32F: return (this.gl as WebGL2RenderingContext).RGBA32F;
        case TextureFormat.DEPTH: return (this.gl as WebGL2RenderingContext).DEPTH_COMPONENT32F;
        default: return this.gl.RGBA;
      }
    }
    return this.gl.RGBA;
  }

  private getTextureFormat(format: TextureFormat): number {
    if (!this.gl) return 0;
    switch (format) {
      case TextureFormat.ALPHA: return this.gl.ALPHA;
      case TextureFormat.RGB: return this.gl.RGB;
      case TextureFormat.RGBA: return this.gl.RGBA;
      case TextureFormat.LUMINANCE: return this.gl.LUMINANCE;
      case TextureFormat.LUMINANCE_ALPHA: return this.gl.LUMUMINANCE_ALPHA;
      case TextureFormat.DEPTH: return this.gl.DEPTH_COMPONENT;
      default: return this.gl.RGBA;
    }
  }

  private getTextureType(format: TextureFormat): number {
    if (!this.gl) return 0;
    if (this.gl instanceof WebGL2RenderingContext) {
      switch (format) {
        case TextureFormat.RGBA16F:
        case TextureFormat.RGBA32F:
          return (this.gl as WebGL2RenderingContext).FLOAT;
        default: return this.gl.UNSIGNED_BYTE;
      }
    }
    return this.gl.UNSIGNED_BYTE;
  }

  public bindRenderTarget(renderTarget: RenderTarget): void {
    if (!this.gl) return;

    this.gl.bindFramebuffer(this.gl.FRAMEBUFFER, renderTarget.framebuffer);
    this.gl.viewport(0, 0, renderTarget.width, renderTarget.height);
  }

  public unbindRenderTarget(): void {
    if (!this.gl) return;

    this.gl.bindFramebuffer(this.gl.FRAMEBUFFER, null);
    this.gl.viewport(0, 0, this.gl.canvas.width, this.gl.canvas.height);
  }

  public deleteShader(shaderId: string): boolean {
    const shader = this.shaders.get(shaderId);
    if (!shader) return false;

    this.gl!.deleteProgram(shader.program);
    this.gl!.deleteShader(shader.vertexShader);
    this.gl!.deleteShader(shader.fragmentShader);

    this.shaders.delete(shaderId);
    return true;
  }

  public deleteMaterial(materialId: string): boolean {
    return this.materials.delete(materialId);
  }

  public deleteRenderTarget(renderTargetId: string): boolean {
    const renderTarget = this.renderTargets.get(renderTargetId);
    if (!renderTarget) return false;

    this.gl!.deleteFramebuffer(renderTarget.framebuffer);
    if (renderTarget.depthBuffer) {
      this.gl!.deleteRenderbuffer(renderTarget.depthBuffer);
    }
    renderTarget.textures.forEach(tex => {
      if (tex.texture) {
        this.gl!.deleteTexture(tex.texture);
      }
    });

    this.renderTargets.delete(renderTargetId);
    return true;
  }

  public getShader(shaderId: string): ShaderProgram | undefined {
    return this.shaders.get(shaderId);
  }

  public getAllShaders(): ShaderProgram[] {
    return Array.from(this.shaders.values());
  }

  public getAllMaterials(): ShaderMaterial[] {
    return Array.from(this.materials.values());
  }

  public getAllRenderTargets(): RenderTarget[] {
    return Array.from(this.renderTargets.values());
  }

  public getActiveShader(): ShaderProgram | null {
    return this.activeShader;
  }

  public clearCache(): void {
    this.compilationCache.clear();
  }

  public destroy(): void {
    this.shaders.forEach((_, id) => this.deleteShader(id));
    this.materials.clear();
    this.renderTargets.forEach((_, id) => this.deleteRenderTarget(id));
    this.globalUniforms.clear();
    this.activeShader = null;
    this.gl = null;
  }
}

export interface RenderTargetOptions {
  format: TextureFormat;
  depthBuffer: boolean;
  multisampled: boolean;
  samples: number;
}

export const createShaderManager = (): ShaderManager => {
  return new ShaderManager();
};

export default ShaderManager;
