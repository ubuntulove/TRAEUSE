/* =========================================================
 * events.js — 生命事件系统 + 换位思考生成器
 *  - 定时产生随机事件
 *  - 对参数变化的语义化描述
 *  - 换位思考：12 种视角人物
 * ========================================================= */
class EventSystem {
  constructor(game, renderer, audio) {
    this.game = game;
    this.renderer = renderer;
    this.audio = audio;
    this.timer = null;
    this.streamEl = null;
    this.perspEl = null;
    this.prevMeaning = 0;
  }
  init() {
    this.streamEl = document.getElementById('event-stream');
    this.perspEl = document.getElementById('perspective-out');
    this.game.onChange = (ev) => this._onGameChange(ev);

    // 启动时钟：每 6-12s 产生一次生命小事件
    this._scheduleNextEvent();

    // 绑定顶部按钮
    document.getElementById('btn-meditate').onclick = () => this._meditate();
    document.getElementById('btn-insight').onclick = () => this._epiphany();
    document.getElementById('btn-reset').onclick = () => this._reset();

    this.prevMeaning = this.game.meaning;
  }

  /* === 调度 === */
  _scheduleNextEvent() {
    const delay = 6000 + Math.random()*8000;
    this.timer = setTimeout(() => {
      this._randomLifeEvent();
      this._scheduleNextEvent();
    }, delay);
  }

  _randomLifeEvent() {
    const kinds = [
      { dim:'health',    type:'good', p:0.10, text:'清晨慢跑 3 公里，心肺舒畅', delta: 2 },
      { dim:'health',    type:'bad',  p:0.07, text:'熬夜刷手机，肩颈酸痛',   delta:-3 },
      { dim:'growth',    type:'good', p:0.11, text:'读完一章书，学到新技能', delta: 3 },
      { dim:'growth',    type:'warn', p:0.06, text:'看了一天短视频，感觉空虚', delta:-2 },
      { dim:'freedom',   type:'good', p:0.08, text:'请了一天假，无所事事漫游', delta: 4 },
      { dim:'freedom',   type:'bad',  p:0.07, text:'开会被安排了额外任务',  delta:-3 },
      { dim:'love',      type:'good', p:0.12, text:'和老友深度交谈，眼眶湿润', delta: 5 },
      { dim:'love',      type:'warn', p:0.05, text:'回避了一个重要对话',    delta:-2 },
      { dim:'purpose',   type:'good', p:0.08, text:'写下了明年的三个目标',  delta: 4 },
      { dim:'purpose',   type:'warn', p:0.06, text:'刷到了别人的"成功"，心生焦虑', delta:-3 },
      { dim:'wisdom',    type:'good', p:0.07, text:'静坐 15 分钟，看到了自己的模式', delta: 3 },
      { dim:'wisdom',    type:'bad',  p:0.05, text:'又一次陷入同样的争吵',    delta:-2 },
      { dim:'contribution',type:'good', p:0.08, text:'帮了陌生人一个小忙',   delta: 3 },
      { dim:'contribution',type:'warn', p:0.05, text:'想做的慈善计划一拖再拖', delta:-2 },
      { dim:'presence',  type:'good', p:0.09, text:'此刻停下，认真看了一朵云', delta: 4 },
      { dim:'presence',  type:'bad',  p:0.07, text:'整天在"想"而不是"在"',   delta:-3 },
      // 神秘事件
      { dim:null, type:'mystic', p:0.05, text:'🌟 一只黑猫从面前走过，时间似乎慢了下来', effect:()=>this._mysticEffect() },
      { dim:null, type:'mystic', p:0.04, text:'🎵 听到一首歌，旋律击中了你', effect:()=>this._mysticEffect() },
      { dim:null, type:'mystic', p:0.04, text:'🌙 做了一个记不清但深刻的梦', effect:()=>this._mysticEffect() },
    ];
    // 加权随机
    const sum = kinds.reduce((a,b)=>a+b.p,0);
    let r = Math.random()*sum;
    let picked = kinds[0];
    for (const k of kinds){ r -= k.p; if (r <= 0){ picked = k; break; } }

    if (picked.dim) {
      const d = this.game.dimensions.find(x => x.id === picked.dim);
      const delta = this.game.setDimension(picked.dim, d.value + picked.delta);
      this._pushEvent(picked.type, picked.text, delta, delta > 0 ? 'pos' : delta < 0 ? 'neg' : '');
    } else if (picked.effect) {
      picked.effect();
      const mPrev = this.prevMeaning;
      this.prevMeaning = this.game.meaning;
      const delta = this.game.meaning - mPrev;
      this._pushEvent('mystic', picked.text, delta, delta !== 0 ? (delta>0?'pos':'neg') : 'mystic');
    }
  }

  _mysticEffect() {
    // 随机给 2-3 个维度轻微扰动，并触发小视角
    const shuffled = [...this.game.dimensions].sort(()=>Math.random()-0.5);
    for (let i=0;i<2 + Math.floor(Math.random()*2);i++){
      const d = shuffled[i];
      const delta = (Math.random() > 0.4 ? 1 : -1) * Math.floor(2 + Math.random()*4);
      this.game.setDimension(d.id, d.value + delta);
    }
    // 触发一个小的换位思考
    this._pushPerspective(this._genPerspective('mystery'));
  }

  _pushEvent(type, text, delta, deltaClass) {
    const el = document.createElement('div');
    el.className = `event-item ${type}`;
    const now = new Date();
    const hh = String(now.getHours()).padStart(2,'0');
    const mm = String(now.getMinutes()).padStart(2,'0');
    const ss = String(now.getSeconds()).padStart(2,'0');
    const dStr = delta > 0 ? `+${delta}` : delta < 0 ? `${delta}` : '·';
    el.innerHTML = `
      <span class="event-time">${hh}:${mm}:${ss}</span>
      <span class="event-text">${text}</span>
      <span class="event-delta ${deltaClass}">${dStr}</span>`;
    this.streamEl.prepend(el);
    while (this.streamEl.children.length > 30) this.streamEl.lastChild.remove();
  }

  _onGameChange(ev) {
    const mNow = this.game.meaning;
    const delta = mNow - this.prevMeaning;
    if (Math.abs(delta) >= 3) this.audio?.triggerMeaningDelta(delta, mNow);
    if (ev.type === 'dim') {
      const d = this.game.dimensions.find(x=>x.id===ev.id);
      const texts = {
        health: ['身体状态变化', '健康波动', '生命体征更新'],
        growth: ['成长曲线变化', '学习更新', '发展中...'],
        freedom: ['自由度调整', '掌控感变化'],
        love: ['情感联结变化', '关系更新'],
        purpose: ['目标清晰度调整', '意义感变化'],
        wisdom: ['智慧积累', '认知更新'],
        contribution: ['贡献变化', '给予更新'],
        presence: ['觉察度调整', '当下感变化'],
      };
      const list = texts[ev.id] || ['参数更新'];
      const t = list[Math.floor(Math.random()*list.length)];
      const desc = ev.delta > 0
        ? `${t}：向上 +${ev.delta} 点`
        : ev.delta < 0 ? `${t}：向下 ${ev.delta} 点`
        : `${t}：保持不变`;
      // 只对大的手动改动记录
      if (Math.abs(ev.delta) >= 5) {
        this._pushEvent(ev.delta > 0 ? 'good' : ev.delta < 0 ? 'warn' : '',
          `🎚 手动调节 · ${d?.emoji||''}${d?.name||ev.id} · ${desc}`,
          ev.delta, ev.delta>0?'pos':ev.delta<0?'neg':'');
      }
    }
    if (ev.type === 'epiphany') {
      this._pushEvent('mystic', '💡 顿悟 · 权重重新排列！', 0, 'mystic');
    }
    if (ev.type === 'reset') {
      this.streamEl.innerHTML = '';
      this._pushEvent('', '⟳ 系统重置 · 一切重新开始', 0, '');
    }
    this.prevMeaning = mNow;
  }

  /* === 冥想 === */
  _meditate() {
    this.audio?.triggerMeditate(4);
    // 冥想：presence 增加，熵降低，love 和 wisdom 轻微增加
    const pres = this.game.getDimension('presence');
    const wis = this.game.getDimension('wisdom');
    const love = this.game.getDimension('love');
    this.game.setDimension('presence', pres + 10);
    this.game.setDimension('wisdom', wis + 4);
    this.game.setDimension('love', love + 3);
    this.renderer.showEpiphany('🧘 深呼吸四次...<br/>此刻，<b>你在</b>。', '432Hz 冥想');
    this._pushEvent('good', '🧘 冥想 4 分钟，回到当下', +17, 'pos');
    // 生成一个"从未来回望"视角
    this._pushPerspective(this._genPerspective('meditate'));
  }

  /* === 顿悟 === */
  _epiphany() {
    this.audio?.triggerEpiphany();
    const changes = this.game.epiphany();
    const lines = changes.map(c => {
      const d = this.game.dimensions.find(x=>x.id===c.id);
      return `${d?.emoji||''}${d?.name||c.id}: ${c.prev} → ${c.next} ${c.dir}`;
    });
    this.renderer.showEpiphany('💡 ' + this._epiphanySaying() + '<br/><small>' + lines.join('　') + '</small>', '视角跃迁');
    // 生成换位思考
    this._pushPerspective(this._genPerspective('epiphany'));
  }

  _epiphanySaying() {
    const arr = [
      '"你不是在寻找人生的意义，你是在<b>活出</b>它。"',
      '"当下是唯一真实的时刻。过去已去，未来未至。"',
      '"你所追求的，正在同样的强度上<b>限制</b>着你。"',
      '"痛苦 + 反思 = 智慧。 痛苦 - 反思 = 苦难。"',
      '"自由，不是增加选项，而是<b>减少</b>想要的选项。"',
      '"你已经足够好了。这不是终点，这是<b>起点</b>。"',
      '"世界不会因为你<b>拥有</b>什么而记住你，只会因为你<b>给予</b>什么而记住你。"',
      '"你以为的"我"，其实是一个由记忆和习惯编织的故事。<br/>你可以<b>重写</b>它。"',
    ];
    return arr[Math.floor(Math.random()*arr.length)];
  }

  /* === 重置 === */
  _reset() {
    if (!confirm('确定重置全部数值？历史不会被撤销。')) return;
    this.game.reset();
    this.perspEl.innerHTML = '<div class="persp-placeholder">点击 "💡 顿悟" 或在星海中选择一个点，触发一次视角跃迁。</div>';
  }

  /* =====================================================
   *  换位思考 · 12 种叙事视角
   * ===================================================== */
  _genPerspective(kind) {
    const roles = [
      { who:'80 岁的你',       tag:'时间维度',
        voice:(s)=>`嗨，年轻的我。此刻的你正在为 <b>${s.dims[0]?.name||'某件事'}</b> 焦虑吗？从这里回头看，那只是我故事里<b>最精彩的转折点</b>。你根本不知道自己有多强大。`},
      { who:'5 岁的你',       tag:'初心维度',
        voice:(s)=>`哇！你有好多好多事情要做！可是...你还记得吗？我们以前蹲在地上看蚂蚁搬家就能开心一下午。<b>你现在还会蹲下来吗？</b>`},
      { who:'你最崇拜的人',   tag:'英雄维度',
        voice:(s)=>`嘿。关于 <b>${s.dims[2]?.name||'自由'}</b>，我想告诉你：<b>我也害怕</b>。勇气不是没有恐惧，而是恐惧存在的同时<b>依然向前</b>。`},
      { who:'对你最失望的人', tag:'阴影维度',
        voice:(s)=>`我曾经以为你不行。现在我知道：<b>不是你不行，是我当时不够理解你</b>。继续吧，让我继续被打脸。`},
      { who:'一个路过的陌生人', tag:'陌生视角',
        voice:(s)=>`我不认识你，但我看到：<b>你正在努力</b>。仅此一点，就已经值得尊重了。`},
      { who:'你的身体',       tag:'肉身维度',
        voice:(s)=>`主人啊，我陪了你这么多年。你给了我 ${s.vals[0]}% 的关注。我不抱怨。但能不能，<b>今晚 11 点前让我睡一次？</b>我想多陪你几十年。`},
      { who:'你的「目标」',   tag:'意义维度',
        voice:(s)=>`你一直在追我。其实，<b>我一直就在你追我的那个动作里</b>。你不需要"到达"我，你只需要继续朝向我 — 这本身就是我。`},
      { who:'一只流浪猫',     tag:'生命维度',
        voice:(s)=>`喵~ 人类你好复杂哦。我们猫猫的哲学：<b>该吃就吃，该睡就睡，该撒娇就撒娇，该打架就打架。</b>一切刚刚好。`},
      { who:'一百亿光年外的外星文明观察员', tag:'宇宙维度',
        voice:(s)=>`已观测 14000 个人类个体。你的编号 #${Math.floor(Math.random()*9999)}。记录：<b>在"短暂"的 80 年里，用 7% 的生命体验了 84% 的情绪光谱。评定：⭐⭐⭐⭐ 精彩样本。</b>`},
      { who:'地球盖亚',       tag:'自然维度',
        voice:(s)=>`孩子，46 亿年了。你是我的梦。去玩，去爱，去犯错，<b>然后回家</b>。家永远在。`},
      { who:'死去的某个朋友', tag:'彼岸维度',
        voice:(s)=>`嘿，从这里看过去，你那点事儿<b>真的都没事</b>。活着本身就是最大的奇迹。替我多晒几次太阳，好吗？`},
      { who:'十年后的 AI 伙伴', tag:'未来维度',
        voice:(s)=>`分析完成。当前 <b>贡献值 ${s.vals[6]}</b> 是你最稀缺的维度。逻辑建议：<b>去帮助一个具体的人，越具体越有效。</b>明天就找一个。`},
      { who:'你的"恐惧"',     tag:'深度心理学',
        voice:(s)=>`我知道你不喜欢我。但我的存在是为了<b>保护</b>你。如果你能坐下来和我聊聊，而不是赶走我，我会变得小很多...谢谢你看到我。`},
      { who:'河流',           tag:'道家视角',
        voice:(s)=>`我不与谁争，也不与谁抢。我只是一直<b>向下流</b>，自然形成了最有力的路径。你也在流吗？还是在筑坝？`},
    ];
    const pick = () => roles[Math.floor(Math.random()*roles.length)];
    const role = kind==='mystery' ? (roles[9]||pick()) : pick();
    const dims = [...this.game.dimensions].sort(()=>Math.random()-0.5).slice(0,3);
    const vals = this.game.dimensions.map(d=>Math.round(d.value));
    const text = role.voice({ dims, vals });
    return { who:role.who, tag:role.tag, text };
  }

  _pushPerspective({ who, tag, text }) {
    // 移除 placeholder
    const ph = this.perspEl.querySelector('.persp-placeholder');
    if (ph) ph.remove();
    const card = document.createElement('div');
    card.className = 'perspective-card';
    card.innerHTML = `
      <div class="who">FROM: <b>${who}</b> ・LENS: ${tag}</div>
      <div class="text">${text}</div>
      <span class="tag">#换位思考</span>
    `;
    this.perspEl.prepend(card);
    while (this.perspEl.children.length > 8) this.perspEl.lastChild.remove();
  }
}
