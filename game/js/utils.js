// utils.js — 数学/随机/碰撞/对象池等通用工具

export const TAU = Math.PI * 2;

/** 角度转弧度 */
export const deg = (d) => (d * Math.PI) / 180;

/** 区间随机浮点 */
export const rand = (min, max) => Math.random() * (max - min) + min;

/** 区间随机整数 */
export const randInt = (min, max) => Math.floor(rand(min, max + 1));

/** 从数组随机取一项 */
export const pick = (arr) => arr[Math.floor(Math.random() * arr.length)];

/** 钳制到 [min, max] */
export const clamp = (v, min, max) => (v < min ? min : v > max ? max : v);

/** 线性插值 */
export const lerp = (a, b, t) => a + (b - a) * t;

/** 两点距离平方（避免开方，性能更好） */
export const dist2 = (ax, ay, bx, by) => {
  const dx = ax - bx, dy = ay - by;
  return dx * dx + dy * dy;
};

/** 圆形碰撞检测 */
export const hitCircle = (a, b) =>
  dist2(a.x, a.y, b.x, b.y) < (a.r + b.r) * (a.r + b.r);

/** 矩形与圆形碰撞 */
export const hitRectCircle = (rect, c) => {
  const cx = clamp(c.x, rect.x, rect.x + rect.w);
  const cy = clamp(c.y, rect.y, rect.y + rect.h);
  return dist2(cx, cy, c.x, c.y) < c.r * c.r;
};

/**
 * 简易对象池：复用对象，减少 GC 压力。
 * 子类需要实现 reset() 方法。
 */
export class Pool {
  constructor(factory) {
    this.factory = factory;
    this.free = [];
    this.active = [];
  }
  acquire() {
    const obj = this.free.pop() || this.factory();
    this.active.push(obj);
    return obj;
  }
  release(obj) {
    const i = this.active.indexOf(obj);
    if (i >= 0) {
      this.active.splice(i, 1);
      this.free.push(obj);
    }
  }
  /** 遍历激活对象，回调返回 true 则回收 */
  sweep(cb) {
    for (let i = this.active.length - 1; i >= 0; i--) {
      const obj = this.active[i];
      if (cb(obj, i)) {
        this.active.splice(i, 1);
        this.free.push(obj);
      }
    }
  }
  clear() {
    while (this.active.length) this.free.push(this.active.pop());
  }
}

/** 颜色 HSL 助手 */
export const hsl = (h, s, l, a = 1) => `hsla(${h},${s}%,${l}%,${a})`;
