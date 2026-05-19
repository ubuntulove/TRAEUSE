import { GameState } from './config.js';

export const SupportedLanguages = {
    ENGLISH: 'en',
    SPANISH: 'es',
    FRENCH: 'fr',
    GERMAN: 'de',
    ITALIAN: 'it',
    PORTUGUESE: 'pt',
    RUSSIAN: 'ru',
    POLISH: 'pl',
    TURKISH: 'tr',
    KOREAN: 'ko',
    JAPANESE: 'ja',
    CHINESE_SIMPLIFIED: 'zh-CN',
    CHINESE_TRADITIONAL: 'zh-TW',
    ARABIC: 'ar',
    HINDI: 'hi',
    THAI: 'th',
    VIETNAMESE: 'vi',
    INDONESIAN: 'id',
    DUTCH: 'nl',
    SWEDISH: 'sv',
    NORWEGIAN: 'no',
    DANISH: 'da',
    FINNISH: 'fi',
    CZECH: 'cs',
    HUNGARIAN: 'hu',
    ROMANIAN: 'ro',
    GREEK: 'el',
    UKRAINIAN: 'uk',
    HEBREW: 'he'
};

export const LanguageDirection = {
    LTR: 'ltr',
    RTL: 'rtl'
};

export const FontFamily = {
    ENGLISH: 'Counter-Strike, Arial, sans-serif',
    CJK: 'Counter-Strike, Microsoft YaHei, sans-serif',
    ARABIC: 'Counter-Strike, Tahoma, Arial, sans-serif',
    RUSSIAN: 'Counter-Strike, Arial, sans-serif'
};

export const TextCase = {
    UPPER: 'uppercase',
    LOWER: 'lowercase',
    TITLE: 'titlecase',
    SENTENCE: 'sentencecase',
    NONE: 'none'
};

export const NumberFormat = {
    DECIMAL: 'decimal',
    INTEGER: 'integer',
    CURRENCY: 'currency',
    PERCENT: 'percent'
};

export const DateFormat = {
    SHORT: 'short',
    LONG: 'long',
    FULL: 'full',
    NUMERIC: 'numeric',
    TIME: 'time'
};

export const PluralForm = {
    ZERO: 'zero',
    ONE: 'one',
    TWO: 'two',
    FEW: 'few',
    MANY: 'many',
    OTHER: 'other'
};

export class LocaleInfo {
    constructor(code, name, nativeName, direction, region, dateFormat, numberFormat) {
        this.code = code;
        this.name = name;
        this.nativeName = nativeName;
        this.direction = direction;
        this.region = region;
        this.dateFormat = dateFormat;
        this.numberFormat = numberFormat;
        this.pluralRules = this.getPluralRules();
    }

    getPluralRules() {
        const rules = {
            en: (n) => n === 1 ? PluralForm.ONE : PluralForm.OTHER,
            es: (n) => n === 1 ? PluralForm.ONE : PluralForm.OTHER,
            fr: (n) => n === 1 ? PluralForm.ONE : PluralForm.OTHER,
            de: (n) => n === 1 ? PluralForm.ONE : PluralForm.OTHER,
            ru: (n) => {
                const mod10 = n % 10;
                const mod100 = n % 100;
                if (mod10 === 1 && mod100 !== 11) return PluralForm.ONE;
                if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return PluralForm.FEW;
                return PluralForm.MANY;
            },
            ar: (n) => {
                if (n === 0) return PluralForm.ZERO;
                if (n === 1) return PluralForm.ONE;
                if (n === 2) return PluralForm.TWO;
                if (n % 100 >= 3 && n % 100 <= 10) return PluralForm.FEW;
                if (n % 100 >= 11) return PluralForm.MANY;
                return PluralForm.OTHER;
            },
            ja: () => PluralForm.OTHER,
            ko: () => PluralForm.OTHER,
            zh: () => PluralForm.OTHER,
            default: () => PluralForm.OTHER
        };
        return rules[this.code] || rules.default;
    }

    getPlural(n) {
        return this.pluralRules(n);
    }

    formatNumber(n, options = {}) {
        const formatter = new Intl.NumberFormat(this.code, {
            style: options.style || 'decimal',
            minimumFractionDigits: options.minDigits || 0,
            maximumFractionDigits: options.maxDigits || 2
        });
        return formatter.format(n);
    }

    formatCurrency(amount, currency = 'USD') {
        const formatter = new Intl.NumberFormat(this.code, {
            style: 'currency',
            currency: currency
        });
        return formatter.format(amount);
    }

    formatDate(date, format = DateFormat.SHORT) {
        const options = {
            [DateFormat.SHORT]: { month: 'numeric', day: 'numeric' },
            [DateFormat.LONG]: { month: 'long', day: 'numeric', year: 'numeric' },
            [DateFormat.FULL]: { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' },
            [DateFormat.NUMERIC]: { year: 'numeric', month: 'numeric', day: 'numeric' }
        };
        return new Intl.DateTimeFormat(this.code, options[format]).format(date);
    }

    formatTime(date) {
        return new Intl.DateTimeFormat(this.code, { hour: 'numeric', minute: 'numeric' }).format(date);
    }

    formatRelativeTime(date) {
        const now = new Date();
        const diff = now - date;
        const seconds = Math.floor(diff / 1000);
        const minutes = Math.floor(seconds / 60);
        const hours = Math.floor(minutes / 60);
        const days = Math.floor(hours / 24);

        if (seconds < 60) return this.translate('time.just_now');
        if (minutes < 60) return this.translate('time.minutes_ago', { count: minutes });
        if (hours < 24) return this.translate('time.hours_ago', { count: hours });
        if (days < 7) return this.translate('time.days_ago', { count: days });
        return this.formatDate(date);
    }

    translate(key, params = {}) {
        return LocalizationManager.getInstance().translate(key, params, this.code);
    }
}

export class LocaleRegistry {
    constructor() {
        this.locales = new Map();
        this.initDefaultLocales();
    }

    initDefaultLocales() {
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.ENGLISH,
            'English',
            'English',
            LanguageDirection.LTR,
            'US',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.SPANISH,
            'Spanish',
            'Español',
            LanguageDirection.LTR,
            'ES',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.FRENCH,
            'French',
            'Français',
            LanguageDirection.LTR,
            'FR',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.GERMAN,
            'German',
            'Deutsch',
            LanguageDirection.LTR,
            'DE',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.ITALIAN,
            'Italian',
            'Italiano',
            LanguageDirection.LTR,
            'IT',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.PORTUGUESE,
            'Portuguese',
            'Português',
            LanguageDirection.LTR,
            'BR',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.RUSSIAN,
            'Russian',
            'Русский',
            LanguageDirection.LTR,
            'RU',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.POLISH,
            'Polish',
            'Polski',
            LanguageDirection.LTR,
            'PL',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.TURKISH,
            'Turkish',
            'Türkçe',
            LanguageDirection.LTR,
            'TR',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.KOREAN,
            'Korean',
            '한국어',
            LanguageDirection.LTR,
            'KR',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.JAPANESE,
            'Japanese',
            '日本語',
            LanguageDirection.LTR,
            'JP',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.CHINESE_SIMPLIFIED,
            'Chinese (Simplified)',
            '简体中文',
            LanguageDirection.LTR,
            'CN',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.CHINESE_TRADITIONAL,
            'Chinese (Traditional)',
            '繁體中文',
            LanguageDirection.LTR,
            'TW',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.ARABIC,
            'Arabic',
            'العربية',
            LanguageDirection.RTL,
            'SA',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.HINDI,
            'Hindi',
            'हिन्दी',
            LanguageDirection.LTR,
            'IN',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.THAI,
            'Thai',
            'ไทย',
            LanguageDirection.LTR,
            'TH',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.VIETNAMESE,
            'Vietnamese',
            'Tiếng Việt',
            LanguageDirection.LTR,
            'VN',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.INDONESIAN,
            'Indonesian',
            'Bahasa Indonesia',
            LanguageDirection.LTR,
            'ID',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.DUTCH,
            'Dutch',
            'Nederlands',
            LanguageDirection.LTR,
            'NL',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.SWEDISH,
            'Swedish',
            'Svenska',
            LanguageDirection.LTR,
            'SE',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.NORWEGIAN,
            'Norwegian',
            'Norsk',
            LanguageDirection.LTR,
            'NO',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.DANISH,
            'Danish',
            'Dansk',
            LanguageDirection.LTR,
            'DK',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.FINNISH,
            'Finnish',
            'Suomi',
            LanguageDirection.LTR,
            'FI',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.CZECH,
            'Czech',
            'Čeština',
            LanguageDirection.LTR,
            'CZ',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.HUNGARIAN,
            'Hungarian',
            'Magyar',
            LanguageDirection.LTR,
            'HU',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.ROMANIAN,
            'Romanian',
            'Română',
            LanguageDirection.LTR,
            'RO',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.GREEK,
            'Greek',
            'Ελληνικά',
            LanguageDirection.LTR,
            'GR',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.UKRAINIAN,
            'Ukrainian',
            'Українська',
            LanguageDirection.LTR,
            'UA',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
        this.registerLocale(new LocaleInfo(
            SupportedLanguages.HEBREW,
            'Hebrew',
            'עברית',
            LanguageDirection.RTL,
            'IL',
            DateFormat.SHORT,
            NumberFormat.DECIMAL
        ));
    }

    registerLocale(locale) {
        this.locales.set(locale.code, locale);
    }

    getLocale(code) {
        return this.locales.get(code) || this.locales.get(SupportedLanguages.ENGLISH);
    }

    getAllLocales() {
        return Array.from(this.locales.values());
    }

    getLocalesByRegion(region) {
        return Array.from(this.locales.values()).filter(l => l.region === region);
    }

    searchLocales(query) {
        const lowerQuery = query.toLowerCase();
        return Array.from(this.locales.values()).filter(l =>
            l.name.toLowerCase().includes(lowerQuery) ||
            l.nativeName.toLowerCase().includes(lowerQuery) ||
            l.code.toLowerCase().includes(lowerQuery)
        );
    }
}

export class TranslationEntry {
    constructor(key, values, description, context = {}) {
        this.key = key;
        this.values = values;
        this.description = description;
        this.context = context;
        this.metadata = {
            created: new Date(),
            modified: new Date(),
            version: '1.0',
            author: 'Unknown',
            status: 'pending',
            approved: false
        };
    }

    getValue(locale, count = 1, gender = null) {
        const localeValues = this.values[locale];
        if (!localeValues) {
            const fallback = this.values[SupportedLanguages.ENGLISH];
            if (!fallback) return this.key;
            return this.selectVariant(fallback, count, gender);
        }
        return this.selectVariant(localeValues, count, gender);
    }

    selectVariant(value, count, gender) {
        if (typeof value === 'string') return value;
        if (typeof value === 'object' && value !== null) {
            if (count !== null && value.plural) {
                const pluralForm = this.getPluralForm(count);
                return value.plural[pluralForm] || value.plural[PluralForm.OTHER] || this.key;
            }
            if (gender !== null && value.gender && value.gender[gender]) {
                return value.gender[gender];
            }
            return value.other || this.key;
        }
        return this.key;
    }

    getPluralForm(count) {
        const rules = {
            en: (n) => n === 1 ? PluralForm.ONE : PluralForm.OTHER,
            es: (n) => n === 1 ? PluralForm.ONE : PluralForm.OTHER,
            fr: (n) => n === 1 ? PluralForm.ONE : PluralForm.OTHER,
            ru: (n) => {
                const mod10 = n % 10;
                const mod100 = n % 100;
                if (mod10 === 1 && mod100 !== 11) return PluralForm.ONE;
                if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return PluralForm.FEW;
                return PluralForm.MANY;
            },
            ar: (n) => {
                if (n === 0) return PluralForm.ZERO;
                if (n === 1) return PluralForm.ONE;
                if (n === 2) return PluralForm.TWO;
                return PluralForm.OTHER;
            }
        };
        const rule = rules[Object.keys(this.values)[0]] || rules.en;
        return rule(count);
    }

    setValue(locale, value) {
        this.values[locale] = value;
        this.metadata.modified = new Date();
    }

    addPluralForm(locale, form, value) {
        if (!this.values[locale]) {
            this.values[locale] = { plural: {} };
        }
        if (!this.values[locale].plural) {
            this.values[locale] = { plural: {} };
        }
        this.values[locale].plural[form] = value;
        this.metadata.modified = new Date();
    }

    getUntranslatedLocales() {
        const englishValues = this.values[SupportedLanguages.ENGLISH];
        const untranslated = [];
        for (const code of Object.values(SupportedLanguages)) {
            if (!this.values[code]) {
                untranslated.push(code);
            }
        }
        return untranslated;
    }

    isComplete() {
        for (const code of Object.values(SupportedLanguages)) {
            if (!this.values[code]) return false;
        }
        return true;
    }

    export() {
        return {
            key: this.key,
            values: this.values,
            description: this.description,
            context: this.context,
            metadata: this.metadata
        };
    }

    static import(data) {
        const entry = new TranslationEntry(data.key, data.values, data.description, data.context);
        if (data.metadata) {
            entry.metadata = { ...entry.metadata, ...data.metadata };
        }
        return entry;
    }
}

export class TranslationCategory {
    constructor(name, description) {
        this.name = name;
        this.description = description;
        this.entries = new Map();
        this.subcategories = new Map();
        this.metadata = {
            totalEntries: 0,
            translatedEntries: 0,
            lastUpdate: new Date()
        };
    }

    addEntry(entry) {
        this.entries.set(entry.key, entry);
        this.metadata.totalEntries++;
        this.metadata.lastUpdate = new Date();
    }

    getEntry(key) {
        return this.entries.get(key);
    }

    getAllEntries() {
        return Array.from(this.entries.values());
    }

    getTranslationProgress() {
        let translated = 0;
        for (const entry of this.entries.values()) {
            if (entry.isComplete()) translated++;
        }
        return {
            total: this.metadata.totalEntries,
            translated,
            percentage: this.metadata.totalEntries > 0
                ? (translated / this.metadata.totalEntries) * 100
                : 0
        };
    }

    searchEntries(query) {
        const lowerQuery = query.toLowerCase();
        return Array.from(this.entries.values()).filter(e =>
            e.key.toLowerCase().includes(lowerQuery) ||
            e.description.toLowerCase().includes(lowerQuery)
        );
    }

    addSubcategory(name, description) {
        const subcategory = new TranslationCategory(name, description);
        this.subcategories.set(name, subcategory);
        return subcategory;
    }

    getSubcategory(name) {
        return this.subcategories.get(name);
    }

    export() {
        return {
            name: this.name,
            description: this.description,
            entries: Array.from(this.entries.values()).map(e => e.export()),
            subcategories: Array.from(this.subcategories.values()).map(s => s.export())
        };
    }

    static import(data) {
        const category = new TranslationCategory(data.name, data.description);
        for (const entryData of data.entries || []) {
            category.addEntry(TranslationEntry.import(entryData));
        }
        for (const subData of data.subcategories || []) {
            const subcategory = TranslationCategory.import(subData);
            category.subcategories.set(subcategory.name, subcategory);
        }
        return category;
    }
}

export class LocalizationDatabase {
    constructor() {
        this.categories = new Map();
        this.fallbackChain = [SupportedLanguages.ENGLISH];
        this.initDefaultCategories();
    }

    initDefaultCategories() {
        this.addCategory(this.createUICategory());
        this.addCategory(this.createGameplayCategory());
        this.addCategory(this.createWeaponsCategory());
        this.addCategory(this.createMapsCategory());
        this.addCategory(this.createUICommandsCategory());
        this.addCategory(this.createAchievementsCategory());
        this.addCategory(this.createTutorialCategory());
        this.addCategory(this.createMenusCategory());
        this.addCategory(this.createHUDCategory());
        this.addCategory(this.createChatCategory());
        this.addCategory(this.createScoresCategory());
        this.addCategory(this.createInventoryCategory());
        this.addCategory(this.createStoreCategory());
        this.addCategory(this.createSettingsCategory());
        this.addCategory(this.createMatchmakingCategory());
        this.addCategory(this.createTournamentCategory());
        this.addCategory(this.createSpectatorCategory());
        this.addCategory(this.createSystemMessagesCategory());
        this.addCategory(this.createErrorsCategory());
        this.addCategory(this.createFeedbackCategory());
        this.addCategory(this.createSupportCategory());
    }

    createUICategory() {
        const category = new TranslationCategory('ui', 'User interface elements');
        category.addEntry(new TranslationEntry('ui.ok', { en: 'OK' }, 'OK button'));
        category.addEntry(new TranslationEntry('ui.cancel', { en: 'Cancel' }, 'Cancel button'));
        category.addEntry(new TranslationEntry('ui.apply', { en: 'Apply' }, 'Apply button'));
        category.addEntry(new TranslationEntry('ui.save', { en: 'Save' }, 'Save button'));
        category.addEntry(new TranslationEntry('ui.close', { en: 'Close' }, 'Close button'));
        category.addEntry(new TranslationEntry('ui.back', { en: 'Back' }, 'Back button'));
        category.addEntry(new TranslationEntry('ui.next', { en: 'Next' }, 'Next button'));
        category.addEntry(new TranslationEntry('ui.done', { en: 'Done' }, 'Done button'));
        category.addEntry(new TranslationEntry('ui.yes', { en: 'Yes' }, 'Yes button'));
        category.addEntry(new TranslationEntry('ui.no', { en: 'No' }, 'No button'));
        category.addEntry(new TranslationEntry('ui.loading', { en: 'Loading...' }, 'Loading message'));
        category.addEntry(new TranslationEntry('ui.search', { en: 'Search' }, 'Search placeholder'));
        category.addEntry(new TranslationEntry('ui.filter', { en: 'Filter' }, 'Filter button'));
        category.addEntry(new TranslationEntry('ui.sort', { en: 'Sort' }, 'Sort button'));
        category.addEntry(new TranslationEntry('ui.refresh', { en: 'Refresh' }, 'Refresh button'));
        category.addEntry(new TranslationEntry('ui.delete', { en: 'Delete' }, 'Delete button'));
        category.addEntry(new TranslationEntry('ui.edit', { en: 'Edit' }, 'Edit button'));
        category.addEntry(new TranslationEntry('ui.create', { en: 'Create' }, 'Create button'));
        category.addEntry(new TranslationEntry('ui.join', { en: 'Join' }, 'Join button'));
        category.addEntry(new TranslationEntry('ui.leave', { en: 'Leave' }, 'Leave button'));
        category.addEntry(new TranslationEntry('ui.start', { en: 'Start' }, 'Start button'));
        category.addEntry(new TranslationEntry('ui.stop', { en: 'Stop' }, 'Stop button'));
        category.addEntry(new TranslationEntry('ui.pause', { en: 'Pause' }, 'Pause button'));
        category.addEntry(new TranslationEntry('ui.resume', { en: 'Resume' }, 'Resume button'));
        category.addEntry(new TranslationEntry('ui.retry', { en: 'Retry' }, 'Retry button'));
        category.addEntry(new TranslationEntry('ui.confirm', { en: 'Confirm' }, 'Confirm button'));
        category.addEntry(new TranslationEntry('ui.skip', { en: 'Skip' }, 'Skip button'));
        category.addEntry(new TranslationEntry('ui.select', { en: 'Select' }, 'Select button'));
        category.addEntry(new TranslationEntry('ui.options', { en: 'Options' }, 'Options button'));
        category.addEntry(new TranslationEntry('ui.help', { en: 'Help' }, 'Help button'));
        category.addEntry(new TranslationEntry('ui.feedback', { en: 'Feedback' }, 'Feedback button'));
        return category;
    }

    createGameplayCategory() {
        const category = new TranslationCategory('gameplay', 'Gameplay terms');
        category.addEntry(new TranslationEntry('gameplay.kill', { en: 'Kill' }, 'Kill'));
        category.addEntry(new TranslationEntry('gameplay.death', { en: 'Death' }, 'Death'));
        category.addEntry(new TranslationEntry('gameplay.assist', { en: 'Assist' }, 'Assist'));
        category.addEntry(new TranslationEntry('gameplay.headshot', { en: 'Headshot' }, 'Headshot'));
        category.addEntry(new TranslationEntry('gameplay.mvp', { en: 'MVP' }, 'MVP'));
        category.addEntry(new TranslationEntry('gameplay.round', { en: 'Round' }, 'Round'));
        category.addEntry(new TranslationEntry('gameplay.match', { en: 'Match' }, 'Match'));
        category.addEntry(new TranslationEntry('gameplay.team', { en: 'Team' }, 'Team'));
        category.addEntry(new TranslationEntry('gameplay.player', { en: 'Player' }, 'Player'));
        category.addEntry(new TranslationEntry('gameplay.score', { en: 'Score' }, 'Score'));
        category.addEntry(new TranslationEntry('gameplay.win', { en: 'Win' }, 'Win'));
        category.addEntry(new TranslationEntry('gameplay.loss', { en: 'Loss' }, 'Loss'));
        category.addEntry(new TranslationEntry('gameplay.draw', { en: 'Draw' }, 'Draw'));
        category.addEntry(new TranslationEntry('gameplay.overtime', { en: 'Overtime' }, 'Overtime'));
        category.addEntry(new TranslationEntry('gameplay.sudden_death', { en: 'Sudden Death' }, 'Sudden Death'));
        category.addEntry(new TranslationEntry('gameplay.bomb', { en: 'Bomb' }, 'Bomb'));
        category.addEntry(new TranslationEntry('gameplay.hostage', { en: 'Hostage' }, 'Hostage'));
        category.addEntry(new TranslationEntry('gameplay.defuse', { en: 'Defuse' }, 'Defuse'));
        category.addEntry(new TranslationEntry('gameplay.plant', { en: 'Plant' }, 'Plant bomb'));
        category.addEntry(new TranslationEntry('gameplay.rescue', { en: 'Rescue' }, 'Rescue hostage'));
        category.addEntry(new TranslationEntry('gameplay.alive', { en: 'Alive' }, 'Alive'));
        category.addEntry(new TranslationEntry('gameplay.dead', { en: 'Dead' }, 'Dead'));
        category.addEntry(new TranslationEntry('gameplay.spectating', { en: 'Spectating' }, 'Spectating'));
        category.addEntry(new TranslationEntry('gameplay.spawning', { en: 'Spawning' }, 'Spawning'));
        category.addEntry(new TranslationEntry('gameplay.buying', { en: 'Buying' }, 'Buying phase'));
        return category;
    }

    createWeaponsCategory() {
        const category = new TranslationCategory('weapons', 'Weapon names and terms');
        category.addEntry(new TranslationEntry('weapons.knife', { en: 'Knife' }, 'Knife'));
        category.addEntry(new TranslationEntry('weapons.pistol', { en: 'Pistol' }, 'Pistol'));
        category.addEntry(new TranslationEntry('weapons.rifle', { en: 'Rifle' }, 'Rifle'));
        category.addEntry(new TranslationEntry('weapons.smg', { en: 'SMG' }, 'Submachine Gun'));
        category.addEntry(new TranslationEntry('weapons.shotgun', { en: 'Shotgun' }, 'Shotgun'));
        category.addEntry(new TranslationEntry('weapons.sniper', { en: 'Sniper Rifle' }, 'Sniper Rifle'));
        category.addEntry(new TranslationEntry('weapons.machinegun', { en: 'Machine Gun' }, 'Machine Gun'));
        category.addEntry(new TranslationEntry('weapons.grenade', { en: 'Grenade' }, 'Grenade'));
        category.addEntry(new TranslationEntry('weapons.flashbang', { en: 'Flashbang' }, 'Flashbang'));
        category.addEntry(new TranslationEntry('weapons.he_grenade', { en: 'HE Grenade' }, 'HE Grenade'));
        category.addEntry(new TranslationEntry('weapons.smoke', { en: 'Smoke Grenade' }, 'Smoke Grenade'));
        category.addEntry(new TranslationEntry('weapons.incendiary', { en: 'Incendiary Grenade' }, 'Incendiary Grenade'));
        category.addEntry(new TranslationEntry('weapons.decoy', { en: 'Decoy Grenade' }, 'Decoy Grenade'));
        category.addEntry(new TranslationEntry('weapons.zeus', { en: 'Zeus' }, 'Zeus taser'));
        category.addEntry(new TranslationEntry('weapons.kevlar', { en: 'Kevlar' }, 'Kevlar vest'));
        category.addEntry(new TranslationEntry('weapons.helmet', { en: 'Helmet' }, 'Helmet'));
        category.addEntry(new TranslationEntry('weapons.defuse_kit', { en: 'Defuse Kit' }, 'Defuse kit'));
        category.addEntry(new TranslationEntry('weapons.ammo', { en: 'Ammo' }, 'Ammunition'));
        category.addEntry(new TranslationEntry('weapons.reloading', { en: 'Reloading' }, 'Reloading'));
        category.addEntry(new TranslationEntry('weapons.firing', { en: 'Firing' }, 'Firing'));
        return category;
    }

    createMapsCategory() {
        const category = new TranslationCategory('maps', 'Map names and terms');
        category.addEntry(new TranslationEntry('maps.dust2', { en: 'Dust II' }, 'Dust II map'));
        category.addEntry(new TranslationEntry('maps.mirage', { en: 'Mirage' }, 'Mirage map'));
        category.addEntry(new TranslationEntry('maps.inferno', { en: 'Inferno' }, 'Inferno map'));
        category.addEntry(new TranslationEntry('maps.nuke', { en: 'Nuke' }, 'Nuke map'));
        category.addEntry(new TranslationEntry('maps.overpass', { en: 'Overpass' }, 'Overpass map'));
        category.addEntry(new TranslationEntry('maps.train', { en: 'Train' }, 'Train map'));
        category.addEntry(new TranslationEntry('maps.vertigo', { en: 'Vertigo' }, 'Vertigo map'));
        category.addEntry(new TranslationEntry('maps.ancient', { en: 'Ancient' }, 'Ancient map'));
        category.addEntry(new TranslationEntry('maps.office', { en: 'Office' }, 'Office map'));
        category.addEntry(new TranslationEntry('maps.italy', { en: 'Italy' }, 'Italy map'));
        category.addEntry(new TranslationEntry('maps.aztec', { en: 'Aztec' }, 'Aztec map'));
        category.addEntry(new TranslationEntry('maps.bank', { en: 'Bank' }, 'Bank map'));
        category.addEntry(new TranslationEntry('maps.bureau', { en: 'Bureau' }, 'Bureau map'));
        category.addEntry(new TranslationEntry('maps.cliimax', { en: 'Cliimax' }, 'Cliimax map'));
        category.addEntry(new TranslationEntry('maps.rescue', { en: 'Rescue' }, 'Rescue map'));
        category.addEntry(new TranslationEntry('maps.thrifty', { en: 'Thrifty' }, 'Thrifty map'));
        category.addEntry(new TranslationEntry('maps.militia', { en: 'Militia' }, 'Militia map'));
        category.addEntry(new TranslationEntry('maps.stmarc', { en: 'St. Marc' }, 'St. Marc map'));
        category.addEntry(new TranslationEntry('maps.agency', { en: 'Agency' }, 'Agency map'));
        category.addEntry(new TranslationEntry('maps.overview', { en: 'Overview' }, 'Map overview'));
        return category;
    }

    createUICommandsCategory() {
        const category = new TranslationCategory('ui_commands', 'In-game commands and UI labels');
        category.addEntry(new TranslationEntry('ui_commands.quit', { en: 'Quit Game' }, 'Quit game'));
        category.addEntry(new TranslationEntry('ui_commands.settings', { en: 'Settings' }, 'Settings menu'));
        category.addEntry(new TranslationEntry('ui_commands.play', { en: 'Play' }, 'Play menu'));
        category.addEntry(new TranslationEntry('ui_commands.watch', { en: 'Watch' }, 'Watch menu'));
        category.addEntry(new TranslationEntry('ui_commands.inventory', { en: 'Inventory' }, 'Inventory'));
        category.addEntry(new TranslationEntry('ui_commands.store', { en: 'Store' }, 'Store'));
        category.addEntry(new TranslationEntry('ui_commands.career', { en: 'Career' }, 'Career stats'));
        category.addEntry(new TranslationEntry('ui_commands.options', { en: 'Options' }, 'Options'));
        category.addEntry(new TranslationEntry('ui_commands.video', { en: 'Video' }, 'Video settings'));
        category.addEntry(new TranslationEntry('ui_commands.audio', { en: 'Audio' }, 'Audio settings'));
        category.addEntry(new TranslationEntry('ui_commands.controls', { en: 'Controls' }, 'Control settings'));
        category.addEntry(new TranslationEntry('ui_commands.keyboard', { en: 'Keyboard' }, 'Keyboard settings'));
        category.addEntry(new TranslationEntry('ui_commands.mouse', { en: 'Mouse' }, 'Mouse settings'));
        category.addEntry(new TranslationEntry('ui_commands.game', { en: 'Game' }, 'Game settings'));
        category.addEntry(new TranslationEntry('ui_commands.multiplayer', { en: 'Multiplayer' }, 'Multiplayer settings'));
        category.addEntry(new TranslationEntry('ui_commands.language', { en: 'Language' }, 'Language setting'));
        category.addEntry(new TranslationEntry('ui_commands.resolution', { en: 'Resolution' }, 'Resolution setting'));
        category.addEntry(new TranslationEntry('ui_commands.volume', { en: 'Volume' }, 'Volume setting'));
        category.addEntry(new TranslationEntry('ui_commands.sensitivity', { en: 'Sensitivity' }, 'Mouse sensitivity'));
        category.addEntry(new TranslationEntry('ui_commands.crosshair', { en: 'Crosshair' }, 'Crosshair settings'));
        category.addEntry(new TranslationEntry('ui_commands.radar', { en: 'Radar' }, 'Radar settings'));
        category.addEntry(new TranslationEntry('ui_commands.minimap', { en: 'Minimap' }, 'Minimap settings'));
        category.addEntry(new TranslationEntry('ui_commands.hud', { en: 'HUD' }, 'HUD settings'));
        return category;
    }

    createAchievementsCategory() {
        const category = new TranslationCategory('achievements', 'Achievement names and descriptions');
        category.addEntry(new TranslationEntry('achievements.first_blood', { en: 'First Blood' }, 'First kill of the match'));
        category.addEntry(new TranslationEntry('achievements.first_blood_desc', { en: 'Get the first kill of the match' }, 'First blood description'));
        category.addEntry(new TranslationEntry('achievements.nemesis', { en: 'Nemesis' }, 'Killed by same enemy multiple times'));
        category.addEntry(new TranslationEntry('achievements.nemesis_desc', { en: 'Kill the player who killed you' }, 'Nemesis description'));
        category.addEntry(new TranslationEntry('achievements.headhunter', { en: 'Headhunter' }, 'Headshot achievement'));
        category.addEntry(new TranslationEntry('achievements.headhunter_desc', { en: 'Kill 10 enemies with headshots' }, 'Headhunter description'));
        category.addEntry(new TranslationEntry('achievements.ace', { en: 'ACE' }, 'Kill all enemies in a round'));
        category.addEntry(new TranslationEntry('achievements.ace_desc', { en: 'Kill all 5 enemies in a single round' }, 'ACE description'));
        category.addEntry(new TranslationEntry('achievements.clutch', { en: 'Clutch' }, 'Win round alone'));
        category.addEntry(new TranslationEntry('achievements.clutch_desc', { en: 'Win a round while being the last player alive' }, 'Clutch description'));
        category.addEntry(new TranslationEntry('achievements.bomb_planted', { en: 'Bomb Planted' }, 'Plant the bomb'));
        category.addEntry(new TranslationEntry('achievements.bomb_planted_desc', { en: 'Plant the bomb in a competitive match' }, 'Bomb planted description'));
        category.addEntry(new TranslationEntry('achievements.bomb_defused', { en: 'Bomb Defused' }, 'Defuse the bomb'));
        category.addEntry(new TranslationEntry('achievements.bomb_defused_desc', { en: 'Defuse the bomb in a competitive match' }, 'Bomb defused description'));
        category.addEntry(new TranslationEntry('achievements.hostage_rescued', { en: 'Hostage Rescued' }, 'Rescue a hostage'));
        category.addEntry(new TranslationEntry('achievements.hostage_rescued_desc', { en: 'Rescue a hostage in a competitive match' }, 'Hostage rescued description'));
        category.addEntry(new TranslationEntry('achievements.flash_spree', { en: 'Flash Spree' }, 'Flash multiple enemies'));
        category.addEntry(new TranslationEntry('achievements.flash_spree_desc', { en: 'Flash 5 enemies in a single round' }, 'Flash spree description'));
        category.addEntry(new TranslationEntry('achievements.smoke_kill', { en: 'Through the Smoke' }, 'Kill through smoke'));
        category.addEntry(new TranslationEntry('achievements.smoke_kill_desc', { en: 'Kill an enemy through a smoke grenade' }, 'Through the smoke description'));
        return category;
    }

    createTutorialCategory() {
        const category = new TranslationCategory('tutorial', 'Tutorial messages');
        category.addEntry(new TranslationEntry('tutorial.welcome', { en: 'Welcome to Counter-Strike!' }, 'Welcome message'));
        category.addEntry(new TranslationEntry('tutorial.wasd', { en: 'Use WASD to move' }, 'Movement instruction'));
        category.addEntry(new TranslationEntry('tutorial.mouse', { en: 'Use the mouse to look around' }, 'Look instruction'));
        category.addEntry(new TranslationEntry('tutorial.jump', { en: 'Press SPACE to jump' }, 'Jump instruction'));
        category.addEntry(new TranslationEntry('tutorial.crouch', { en: 'Press CTRL to crouch' }, 'Crouch instruction'));
        category.addEntry(new TranslationEntry('tutorial.sprint', { en: 'Hold SHIFT to sprint' }, 'Sprint instruction'));
        category.addEntry(new TranslationEntry('tutorial.shoot', { en: 'Click to shoot' }, 'Shoot instruction'));
        category.addEntry(new TranslationEntry('tutorial.reload', { en: 'Press R to reload' }, 'Reload instruction'));
        category.addEntry(new TranslationEntry('tutorial.buy_menu', { en: 'Press B to open the buy menu' }, 'Buy menu instruction'));
        category.addEntry(new TranslationEntry('tutorial.drop_weapon', { en: 'Press G to drop your weapon' }, 'Drop weapon instruction'));
        category.addEntry(new TranslationEntry('tutorial.use', { en: 'Press E to interact with objects' }, 'Use instruction'));
        category.addEntry(new TranslationEntry('tutorial.knife_attack', { en: 'Click to attack with knife' }, 'Knife instruction'));
        category.addEntry(new TranslationEntry('tutorial.zoom', { en: 'Right-click to zoom' }, 'Zoom instruction'));
        category.addEntry(new TranslationEntry('tutorial.switch_weapon', { en: 'Press 1-5 to switch weapons' }, 'Weapon switch instruction'));
        category.addEntry(new TranslationEntry('tutorial.throw_grenade', { en: 'Press G then click to throw grenades' }, 'Grenade instruction'));
        category.addEntry(new TranslationEntry('tutorial.plant_bomb', { en: 'Press E at bomb site to plant' }, 'Plant instruction'));
        category.addEntry(new TranslationEntry('tutorial.defuse_bomb', { en: 'Press E at bomb to defuse' }, 'Defuse instruction'));
        category.addEntry(new TranslationEntry('tutorial.rescue_hostage', { en: 'Press E near hostage to rescue' }, 'Rescue instruction'));
        category.addEntry(new TranslationEntry('tutorial.teamwork', { en: 'Work with your team!' }, 'Teamwork message'));
        category.addEntry(new TranslationEntry('tutorial.communication', { en: 'Use voice or text chat to communicate' }, 'Communication message'));
        return category;
    }

    createMenusCategory() {
        const category = new TranslationCategory('menus', 'Menu screens');
        category.addEntry(new TranslationEntry('menus.main', { en: 'Main Menu' }, 'Main menu'));
        category.addEntry(new TranslationEntry('menus.play_menu', { en: 'Play Menu' }, 'Play menu'));
        category.addEntry(new TranslationEntry('menus.casual', { en: 'Casual' }, 'Casual mode'));
        category.addEntry(new TranslationEntry('menus.competitive', { en: 'Competitive' }, 'Competitive mode'));
        category.addEntry(new TranslationEntry('menus.deathmatch', { en: 'Deathmatch' }, 'Deathmatch mode'));
        category.addEntry(new TranslationEntry('menus.training', { en: 'Training' }, 'Training mode'));
        category.addEntry(new TranslationEntry('menus.custom_game', { en: 'Custom Game' }, 'Custom game'));
        category.addEntry(new TranslationEntry('menus.arms_race', { en: 'Arms Race' }, 'Arms race mode'));
        category.addEntry(new TranslationEntry('menus.demolition', { en: 'Demolition' }, 'Demolition mode'));
        category.addEntry(new TranslationEntry('menus.flying_scoutman', { en: 'Flying Scoutsman' }, 'Flying scoutsman mode'));
        category.addEntry(new TranslationEntry('menus.wingman', { en: 'Wingman' }, 'Wingman mode'));
        category.addEntry(new TranslationEntry('menus.guardian', { en: 'Guardian' }, 'Guardian mode'));
        category.addEntry(new TranslationEntry('menus.coop_strike', { en: 'Coop Strike' }, 'Coop strike mode'));
        category.addEntry(new TranslationEntry('menus.danger_zone', { en: 'Danger Zone' }, 'Danger zone mode'));
        category.addEntry(new TranslationEntry('menus.server_browser', { en: 'Server Browser' }, 'Server browser'));
        category.addEntry(new TranslationEntry('menus.create_server', { en: 'Create Server' }, 'Create server'));
        category.addEntry(new TranslationEntry('menus.join_server', { en: 'Join Server' }, 'Join server'));
        category.addEntry(new TranslationEntry('menus.favorites', { en: 'Favorites' }, 'Favorites list'));
        category.addEntry(new TranslationEntry('menus.history', { en: 'History' }, 'Server history'));
        category.addEntry(new TranslationEntry('menus.lan', { en: 'LAN' }, 'LAN servers'));
        return category;
    }

    createHUDCategory() {
        const category = new TranslationCategory('hud', 'HUD elements');
        category.addEntry(new TranslationEntry('hud.health', { en: 'Health' }, 'Health display'));
        category.addEntry(new TranslationEntry('hud.armor', { en: 'Armor' }, 'Armor display'));
        category.addEntry(new TranslationEntry('hud.money', { en: 'Money' }, 'Money display'));
        category.addEntry(new TranslationEntry('hud.kills', { en: 'Kills' }, 'Kills display'));
        category.addEntry(new TranslationEntry('hud.deaths', { en: 'Deaths' }, 'Deaths display'));
        category.addEntry(new TranslationEntry('hud.assists', { en: 'Assists' }, 'Assists display'));
        category.addEntry(new TranslationEntry('hud.score', { en: 'Score' }, 'Score display'));
        category.addEntry(new TranslationEntry('hud.ammo', { en: 'Ammo' }, 'Ammo display'));
        category.addEntry(new TranslationEntry('hud.round_time', { en: 'Round Time' }, 'Round time display'));
        category.addEntry(new TranslationEntry('hud.freeze_time', { en: 'Freeze Time' }, 'Freeze time display'));
        category.addEntry(new TranslationEntry('hud.buy_time', { en: 'Buy Time' }, 'Buy time display'));
        category.addEntry(new TranslationEntry('hud.team_score', { en: 'Team Score' }, 'Team score display'));
        category.addEntry(new TranslationEntry('hud.enemies_remaining', { en: 'Enemies Remaining' }, 'Enemies remaining'));
        category.addEntry(new TranslationEntry('hud.hostages_alive', { en: 'Hostages Alive' }, 'Hostages alive'));
        category.addEntry(new TranslationEntry('hud.bomb_carrier', { en: 'Bomb Carrier' }, 'Bomb carrier indicator'));
        category.addEntry(new TranslationEntry('hud.defuse_kit', { en: 'Defuse Kit' }, 'Defuse kit indicator'));
        category.addEntry(new TranslationEntry('hud.kill_feed', { en: 'Kill Feed' }, 'Kill feed'));
        category.addEntry(new TranslationEntry('hud.minimap', { en: 'Minimap' }, 'Minimap'));
        category.addEntry(new TranslationEntry('hud.chat', { en: 'Chat' }, 'Chat box'));
        category.addEntry(new TranslationEntry('hud.voice_chat', { en: 'Voice Chat' }, 'Voice chat indicator'));
        return category;
    }

    createChatCategory() {
        const category = new TranslationCategory('chat', 'Chat messages');
        category.addEntry(new TranslationEntry('chat.team_say', { en: 'Team' }, 'Team chat prefix'));
        category.addEntry(new TranslationEntry('chat.all_say', { en: 'All' }, 'All chat prefix'));
        category.addEntry(new TranslationEntry('chat.radio', { en: 'Radio' }, 'Radio chat'));
        category.addEntry(new TranslationEntry('chat.voice_chat', { en: 'Voice Chat' }, 'Voice chat'));
        category.addEntry(new TranslationEntry('chat.connected', { en: '{player} has connected' }, 'Player connected'));
        category.addEntry(new TranslationEntry('chat.disconnected', { en: '{player} has disconnected' }, 'Player disconnected'));
        category.addEntry(new TranslationEntry('chat.joined_team', { en: '{player} joined {team}' }, 'Player joined team'));
        category.addEntry(new TranslationEntry('chat.ready', { en: '{player} is ready' }, 'Player ready'));
        category.addEntry(new TranslationEntry('chat.not_ready', { en: '{player} is not ready' }, 'Player not ready'));
        category.addEntry(new TranslationEntry('chat.kicked', { en: '{player} was kicked' }, 'Player kicked'));
        category.addEntry(new TranslationEntry('chat.banned', { en: '{player} was banned' }, 'Player banned'));
        category.addEntry(new TranslationEntry('chat.unmuted', { en: '{player} was unmuted' }, 'Player unmuted'));
        category.addEntry(new TranslationEntry('chat.muted', { en: '{player} is muted' }, 'Player muted'));
        return category;
    }

    createScoresCategory() {
        const category = new TranslationCategory('scores', 'Scoreboard');
        category.addEntry(new TranslationEntry('scores.player', { en: 'Player' }, 'Player column'));
        category.addEntry(new TranslationEntry('scores.kills', { en: 'K' }, 'Kills column'));
        category.addEntry(new TranslationEntry('scores.deaths', { en: 'D' }, 'Deaths column'));
        category.addEntry(new TranslationEntry('scores.assists', { en: 'A' }, 'Assists column'));
        category.addEntry(new TranslationEntry('scores.score', { en: 'Score' }, 'Score column'));
        category.addEntry(new TranslationEntry('scores.ping', { en: 'Ping' }, 'Ping column'));
        category.addEntry(new TranslationEntry('scores.mvps', { en: 'MVP' }, 'MVP column'));
        category.addEntry(new TranslationEntry('scores.terrorists', { en: 'Terrorists' }, 'Terrorist team'));
        category.addEntry(new TranslationEntry('scores.counter_terrorists', { en: 'Counter-Terrorists' }, 'Counter-terrorist team'));
        category.addEntry(new TranslationEntry('scores.spectators', { en: 'Spectators' }, 'Spectators'));
        return category;
    }

    createInventoryCategory() {
        const category = new TranslationCategory('inventory', 'Inventory system');
        category.addEntry(new TranslationEntry('inventory.title', { en: 'Inventory' }, 'Inventory title'));
        category.addEntry(new TranslationEntry('inventory.weapons', { en: 'Weapons' }, 'Weapons tab'));
        category.addEntry(new TranslationEntry('inventory.cases', { en: 'Cases' }, 'Cases tab'));
        category.addEntry(new TranslationEntry('inventory.items', { en: 'Items' }, 'Items tab'));
        category.addEntry(new TranslationEntry('inventory.souvenirs', { en: 'Souvenirs' }, 'Souvenirs tab'));
        category.addEntry(new TranslationEntry('inventory.trophy', { en: 'Trophies' }, 'Trophies tab'));
        category.addEntry(new TranslationEntry('inventory.equip', { en: 'Equip' }, 'Equip button'));
        category.addEntry(new TranslationEntry('inventory.drop', { en: 'Drop' }, 'Drop button'));
        category.addEntry(new TranslationEntry('inventory.trade', { en: 'Trade' }, 'Trade button'));
        category.addEntry(new TranslationEntry('inventory.sell', { en: 'Sell' }, 'Sell button'));
        category.addEntry(new TranslationEntry('inventory.buy', { en: 'Buy' }, 'Buy button'));
        category.addEntry(new TranslationEntry('inventory.open', { en: 'Open' }, 'Open button'));
        category.addEntry(new TranslationEntry('inventory.empty', { en: 'Your inventory is empty' }, 'Empty inventory'));
        category.addEntry(new TranslationEntry('inventory.search', { en: 'Search inventory...' }, 'Search placeholder'));
        return category;
    }

    createStoreCategory() {
        const category = new TranslationCategory('store', 'Store system');
        category.addEntry(new TranslationEntry('store.title', { en: 'Store' }, 'Store title'));
        category.addEntry(new TranslationEntry('store.featured', { en: 'Featured' }, 'Featured tab'));
        category.addEntry(new TranslationEntry('store.weapons', { en: 'Weapons' }, 'Weapons tab'));
        category.addEntry(new TranslationEntry('store.cases', { en: 'Cases' }, 'Cases tab'));
        category.addEntry(new TranslationEntry('store.stickers', { en: 'Stickers' }, 'Stickers tab'));
        category.addEntry(new TranslationEntry('store.skins', { en: 'Skins' }, 'Skins tab'));
        category.addEntry(new TranslationEntry('store.graphics', { en: 'Graphics' }, 'Graphics tab'));
        category.addEntry(new TranslationEntry('store.keys', { en: 'Keys' }, 'Keys tab'));
        category.addEntry(new TranslationEntry('store.cart', { en: 'Cart' }, 'Cart'));
        category.addEntry(new TranslationEntry('store.checkout', { en: 'Checkout' }, 'Checkout'));
        category.addEntry(new TranslationEntry('store.balance', { en: 'Balance' }, 'Account balance'));
        category.addEntry(new TranslationEntry('store.purchase', { en: 'Purchase' }, 'Purchase button'));
        category.addEntry(new TranslationEntry('store.sale', { en: 'On Sale!' }, 'Sale indicator'));
        category.addEntry(new TranslationEntry('store.new', { en: 'New!' }, 'New indicator'));
        category.addEntry(new TranslationEntry('store.popular', { en: 'Popular' }, 'Popular indicator'));
        return category;
    }

    createSettingsCategory() {
        const category = new TranslationCategory('settings', 'Settings');
        category.addEntry(new TranslationEntry('settings.general', { en: 'General' }, 'General settings'));
        category.addEntry(new TranslationEntry('settings.video', { en: 'Video' }, 'Video settings'));
        category.addEntry(new TranslationEntry('settings.audio', { en: 'Audio' }, 'Audio settings'));
        category.addEntry(new TranslationEntry('settings.controls', { en: 'Controls' }, 'Control settings'));
        category.addEntry(new TranslationEntry('settings.game', { en: 'Game' }, 'Game settings'));
        category.addEntry(new TranslationEntry('settings.keybinds', { en: 'Keybinds' }, 'Keybind settings'));
        category.addEntry(new TranslationEntry('settings.crosshair', { en: 'Crosshair' }, 'Crosshair settings'));
        category.addEntry(new TranslationEntry('settings.radar', { en: 'Radar' }, 'Radar settings'));
        category.addEntry(new TranslationEntry('settings.voice', { en: 'Voice' }, 'Voice settings'));
        category.addEntry(new TranslationEntry('settings.advanced', { en: 'Advanced' }, 'Advanced settings'));
        category.addEntry(new TranslationEntry('settings.revert', { en: 'Revert' }, 'Revert button'));
        category.addEntry(new TranslationEntry('settings.apply', { en: 'Apply' }, 'Apply button'));
        category.addEntry(new TranslationEntry('settings.reset', { en: 'Reset to Default' }, 'Reset button'));
        return category;
    }

    createMatchmakingCategory() {
        const category = new TranslationCategory('matchmaking', 'Matchmaking');
        category.addEntry(new TranslationEntry('matchmaking.searching', { en: 'Searching for match...' }, 'Searching'));
        category.addEntry(new TranslationEntry('matchmaking.found', { en: 'Match found!' }, 'Match found'));
        category.addEntry(new TranslationEntry('matchmaking.cancelled', { en: 'Search cancelled' }, 'Search cancelled'));
        category.addEntry(new TranslationEntry('matchmaking.failed', { en: 'Search failed' }, 'Search failed'));
        category.addEntry(new TranslationEntry('matchmaking.accept', { en: 'Accept' }, 'Accept button'));
        category.addEntry(new TranslationEntry('matchmaking.decline', { en: 'Decline' }, 'Decline button'));
        category.addEntry(new TranslationEntry('matchmaking.leave_queue', { en: 'Leave Queue' }, 'Leave queue'));
        category.addEntry(new TranslationEntry('matchmaking.queue_time', { en: 'Queue time: {time}' }, 'Queue time'));
        category.addEntry(new TranslationEntry('matchmaking.players_found', { en: '{count} players found' }, 'Players found'));
        category.addEntry(new TranslationEntry('matchmaking.estimated_wait', { en: 'Estimated wait: {time}' }, 'Estimated wait'));
        return category;
    }

    createTournamentCategory() {
        const category = new TranslationCategory('tournament', 'Tournament');
        category.addEntry(new TranslationEntry('tournament.bracket', { en: 'Bracket' }, 'Bracket view'));
        category.addEntry(new TranslationEntry('tournament.schedule', { en: 'Schedule' }, 'Schedule view'));
        category.addEntry(new TranslationEntry('tournament.teams', { en: 'Teams' }, 'Teams view'));
        category.addEntry(new TranslationEntry('tournament.live', { en: 'LIVE' }, 'Live indicator'));
        category.addEntry(new TranslationEntry('tournament.upcoming', { en: 'Upcoming' }, 'Upcoming matches'));
        category.addEntry(new TranslationEntry('tournament.completed', { en: 'Completed' }, 'Completed matches'));
        category.addEntry(new TranslationEntry('tournament.group_stage', { en: 'Group Stage' }, 'Group stage'));
        category.addEntry(new TranslationEntry('tournament.playoffs', { en: 'Playoffs' }, 'Playoffs'));
        category.addEntry(new TranslationEntry('tournament.finals', { en: 'Finals' }, 'Finals'));
        category.addEntry(new TranslationEntry('tournament.champion', { en: 'Champion' }, 'Champion'));
        category.addEntry(new TranslationEntry('tournament.register', { en: 'Register' }, 'Register button'));
        category.addEntry(new TranslationEntry('tournament.withdraw', { en: 'Withdraw' }, 'Withdraw button'));
        return category;
    }

    createSpectatorCategory() {
        const category = new TranslationCategory('spectator', 'Spectator');
        category.addEntry(new TranslationEntry('spectator.watching', { en: 'Watching' }, 'Watching indicator'));
        category.addEntry(new TranslationEntry('spectator.live', { en: 'LIVE' }, 'Live indicator'));
        category.addEntry(new TranslationEntry('spectator.delay', { en: 'Delay' }, 'Stream delay'));
        category.addEntry(new TranslationEntry('spectator.follow', { en: 'Follow Player' }, 'Follow player'));
        category.addEntry(new TranslationEntry('spectator.freeroam', { en: 'Free Camera' }, 'Free camera'));
        category.addEntry(new TranslationEntry('spectator.drone', { en: 'Drone Camera' }, 'Drone camera'));
        category.addEntry(new TranslationEntry('spectator.player_list', { en: 'Player List' }, 'Player list'));
        category.addEntry(new TranslationEntry('spectator.broadcast', { en: 'Broadcast' }, 'Broadcast'));
        category.addEntry(new TranslationEntry('spectator.record', { en: 'Record' }, 'Record demo'));
        category.addEntry(new TranslationEntry('spectator.stream', { en: 'Stream' }, 'Stream match'));
        return category;
    }

    createSystemMessagesCategory() {
        const category = new TranslationCategory('system', 'System messages');
        category.addEntry(new TranslationEntry('system.connection_lost', { en: 'Connection to server lost' }, 'Connection lost'));
        category.addEntry(new TranslationEntry('system.connection_failed', { en: 'Failed to connect to server' }, 'Connection failed'));
        category.addEntry(new TranslationEntry('system.kicked', { en: 'You were kicked from the server' }, 'Kicked'));
        category.addEntry(new TranslationEntry('system.banned', { en: 'You were banned from the server' }, 'Banned'));
        category.addEntry(new TranslationEntry('system.timeout', { en: 'Connection timed out' }, 'Timeout'));
        category.addEntry(new TranslationEntry('system.server_full', { en: 'Server is full' }, 'Server full'));
        category.addEntry(new TranslationEntry('system.wrong_password', { en: 'Incorrect password' }, 'Wrong password'));
        category.addEntry(new TranslationEntry('system.version_mismatch', { en: 'Version mismatch' }, 'Version mismatch'));
        category.addEntry(new TranslationEntry('system VAC_banned', { en: ' VAC banned players cannot join secure servers' }, 'VAC ban'));
        category.addEntry(new TranslationEntry('system.connecting', { en: 'Connecting to server...' }, 'Connecting'));
        category.addEntry(new TranslationEntry('system.connected', { en: 'Connected to server' }, 'Connected'));
        category.addEntry(new TranslationEntry('system.disconnected', { en: 'Disconnected from server' }, 'Disconnected'));
        category.addEntry(new TranslationEntry('system.reconnecting', { en: 'Reconnecting...' }, 'Reconnecting'));
        return category;
    }

    createErrorsCategory() {
        const category = new TranslationCategory('errors', 'Error messages');
        category.addEntry(new TranslationEntry('errors.generic', { en: 'An error occurred' }, 'Generic error'));
        category.addEntry(new TranslationEntry('errors.network', { en: 'Network error' }, 'Network error'));
        category.addEntry(new TranslationEntry('errors.game', { en: 'Game error' }, 'Game error'));
        category.addEntry(new TranslationEntry('errors.file', { en: 'File error' }, 'File error'));
        category.addEntry(new TranslationEntry('errors.permissions', { en: 'Permission denied' }, 'Permission error'));
        category.addEntry(new TranslationEntry('errors.storage', { en: 'Storage error' }, 'Storage error'));
        category.addEntry(new TranslationEntry('errors.authentication', { en: 'Authentication failed' }, 'Auth error'));
        category.addEntry(new TranslationEntry('errors.server', { en: 'Server error' }, 'Server error'));
        category.addEntry(new TranslationEntry('errors.timeout', { en: 'Request timed out' }, 'Timeout error'));
        category.addEntry(new TranslationEntry('errors.invalid', { en: 'Invalid data' }, 'Invalid error'));
        return category;
    }

    createFeedbackCategory() {
        const category = new TranslationCategory('feedback', 'Feedback messages');
        category.addEntry(new TranslationEntry('feedback.thanks', { en: 'Thank you for your feedback!' }, 'Thanks message'));
        category.addEntry(new TranslationEntry('feedback.reported', { en: 'Player reported' }, 'Report confirmation'));
        category.addEntry(new TranslationEntry('feedback.submitted', { en: 'Feedback submitted' }, 'Feedback confirmation'));
        category.addEntry(new TranslationEntry('feedback.saved', { en: 'Settings saved' }, 'Settings saved'));
        category.addEntry(new TranslationEntry('feedback.applied', { en: 'Settings applied' }, 'Settings applied'));
        category.addEntry(new TranslationEntry('feedback.reset', { en: 'Settings reset' }, 'Settings reset'));
        category.addEntry(new TranslationEntry('feedback.copied', { en: 'Copied to clipboard' }, 'Copied'));
        category.addEntry(new TranslationEntry('feedback.exported', { en: 'Exported successfully' }, 'Exported'));
        category.addEntry(new TranslationEntry('feedback.imported', { en: 'Imported successfully' }, 'Imported'));
        category.addEntry(new TranslationEntry('feedback.purchased', { en: 'Purchase successful' }, 'Purchased'));
        return category;
    }

    createSupportCategory() {
        const category = new TranslationCategory('support', 'Support');
        category.addEntry(new TranslationEntry('support.help', { en: 'Help' }, 'Help'));
        category.addEntry(new TranslationEntry('support.faq', { en: 'FAQ' }, 'FAQ'));
        category.addEntry(new TranslationEntry('support.tickets', { en: 'Support Tickets' }, 'Tickets'));
        category.addEntry(new TranslationEntry('support.contact', { en: 'Contact Us' }, 'Contact'));
        category.addEntry(new TranslationEntry('support.terms', { en: 'Terms of Service' }, 'Terms'));
        category.addEntry(new TranslationEntry('support.privacy', { en: 'Privacy Policy' }, 'Privacy'));
        category.addEntry(new TranslationEntry('support.refund', { en: 'Refund Policy' }, 'Refund'));
        category.addEntry(new TranslationEntry('support.report', { en: 'Report Issue' }, 'Report'));
        category.addEntry(new TranslationEntry('support.feedback', { en: 'Send Feedback' }, 'Feedback'));
        category.addEntry(new TranslationEntry('support.bug', { en: 'Report Bug' }, 'Bug report'));
        return category;
    }

    addCategory(category) {
        this.categories.set(category.name, category);
    }

    getCategory(name) {
        return this.categories.get(name);
    }

    getAllCategories() {
        return Array.from(this.categories.values());
    }

    getEntry(key) {
        const parts = key.split('.');
        if (parts.length < 2) return null;
        
        const category = this.getCategory(parts[0]);
        if (!category) return null;
        
        return category.getEntry(key);
    }

    setFallbackChain(chain) {
        this.fallbackChain = chain;
    }

    getFallbackChain() {
        return [...this.fallbackChain];
    }

    export() {
        const data = {};
        for (const [name, category] of this.categories) {
            data[name] = category.export();
        }
        return data;
    }

    import(data) {
        for (const [name, categoryData] of Object.entries(data)) {
            const category = TranslationCategory.import(categoryData);
            this.categories.set(name, category);
        }
    }
}

export class TextProcessor {
    constructor(localization) {
        this.localization = localization;
        this.variablePattern = /\{(\w+)\}/g;
        this.pluralPattern = /\{#(\w+)\s+(\w+)\s+(\w+)\}/g;
        this.genderPattern = /\{(\w+)\|(\w+)\}/g;
    }

    process(text, locale, params = {}, count = null, gender = null) {
        let result = text;
        result = this.processVariables(result, params);
        result = this.processPlurals(result, locale, count);
        result = this.processGender(result, gender);
        return result;
    }

    processVariables(text, params) {
        return text.replace(this.variablePattern, (match, key) => {
            return params[key] !== undefined ? params[key] : match;
        });
    }

    processPlurals(text, locale, count) {
        if (count === null) return text;
        const pluralForm = this.localization.getPluralForm(count);
        const regex = /\{#(\w+)\s+(\w+)\}/g;
        return text.replace(regex, (match, form, word) => {
            const forms = {
                zero: word,
                one: word,
                two: word,
                few: word,
                many: word,
                other: word
            };
            return forms[pluralForm] || forms.other;
        });
    }

    processGender(text, gender) {
        if (!gender) return text;
        const regex = /\{(\w+)\|(\w+)\}/g;
        return text.replace(regex, (match, male, female) => {
            return gender === 'male' ? male : female;
        });
    }

    sanitize(text) {
        return text
            .replace(/[<>]/g, '')
            .replace(/javascript:/gi, '')
            .replace(/on\w+=/gi, '')
            .trim();
    }

    truncate(text, maxLength, ellipsis = '...') {
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength - ellipsis.length) + ellipsis;
    }

    capitalize(text) {
        return text.charAt(0).toUpperCase() + text.slice(1).toLowerCase();
    }

    titleCase(text) {
        return text.split(' ').map(word => this.capitalize(word)).join(' ');
    }

    toUpperCase(text, locale) {
        return text.toUpperCase();
    }

    toLowerCase(text, locale) {
        return text.toLowerCase();
    }
}

export class PluralRules {
    static getRule(locale) {
        const rules = {
            en: (n) => n === 1 ? 1 : 0,
            es: (n) => n === 1 ? 1 : 0,
            fr: (n) => n === 1 ? 1 : 0,
            de: (n) => n === 1 ? 1 : 0,
            it: (n) => n === 1 ? 1 : 0,
            pt: (n) => n === 1 ? 1 : 0,
            ru: (n) => {
                const mod10 = n % 10;
                const mod100 = n % 100;
                if (mod10 === 1 && mod100 !== 11) return 1;
                if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return 2;
                return 0;
            },
            pl: (n) => {
                const mod10 = n % 10;
                const mod100 = n % 100;
                if (n === 1) return 1;
                if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return 2;
                return 0;
            },
            ar: (n) => {
                if (n === 0) return 0;
                if (n === 1) return 1;
                if (n === 2) return 2;
                if (n % 100 >= 3 && n % 100 <= 10) return 3;
                return 0;
            },
            ja: () => 0,
            ko: () => 0,
            zh: () => 0,
            default: () => 0
        };
        return rules[locale] || rules.default;
    }

    static select(locale, forms, count) {
        const rule = this.getRule(locale);
        const formIndex = rule(count);
        return forms[formIndex] || forms[forms.length - 1] || '';
    }
}

export class LocalizationManager {
    static instance = null;

    constructor() {
        this.currentLocale = SupportedLanguages.ENGLISH;
        this.database = new LocalizationDatabase();
        this.localeRegistry = new LocaleRegistry();
        this.textProcessor = new TextProcessor(this);
        this.cache = new Map();
        this.cacheEnabled = true;
        this.maxCacheSize = 10000;
        this.missingKeys = new Set();
        this.listeners = new Map();
    }

    static getInstance() {
        if (!LocalizationManager.instance) {
            LocalizationManager.instance = new LocalizationManager();
        }
        return LocalizationManager.instance;
    }

    setLocale(localeCode) {
        const locale = this.localeRegistry.getLocale(localeCode);
        if (!locale) {
            console.warn(`Locale ${localeCode} not found, falling back to English`);
            this.currentLocale = SupportedLanguages.ENGLISH;
        } else {
            this.currentLocale = localeCode;
        }
        this.clearCache();
        this.emit('localeChanged', { locale: this.currentLocale });
        return this.currentLocale;
    }

    getLocale() {
        return this.localeRegistry.getLocale(this.currentLocale);
    }

    getCurrentLocaleCode() {
        return this.currentLocale;
    }

    translate(key, params = {}, localeCode = null) {
        const locale = localeCode || this.currentLocale;
        const cacheKey = `${locale}:${key}:${JSON.stringify(params)}`;
        
        if (this.cacheEnabled && this.cache.has(cacheKey)) {
            return this.cache.get(cacheKey);
        }

        const entry = this.database.getEntry(key);
        let result;

        if (entry) {
            result = entry.getValue(locale, params.count, params.gender);
        } else {
            if (!this.missingKeys.has(key)) {
                this.missingKeys.add(key);
                console.warn(`Missing translation key: ${key}`);
            }
            result = key;
        }

        result = this.textProcessor.process(result, locale, params, params.count, params.gender);
        result = this.textProcessor.sanitize(result);

        if (this.cacheEnabled) {
            if (this.cache.size >= this.maxCacheSize) {
                const firstKey = this.cache.keys().next().value;
                this.cache.delete(firstKey);
            }
            this.cache.set(cacheKey, result);
        }

        return result;
    }

    translatePlural(key, count, forms, localeCode = null) {
        const locale = localeCode || this.currentLocale;
        const result = PluralRules.select(locale, forms, count);
        return this.textProcessor.process(result, locale, { count });
    }

    formatNumber(n, options = {}, localeCode = null) {
        const locale = localeCode || this.currentLocale;
        const localeInfo = this.localeRegistry.getLocale(locale);
        return localeInfo.formatNumber(n, options);
    }

    formatCurrency(amount, currency = 'USD', localeCode = null) {
        const locale = localeCode || this.currentLocale;
        const localeInfo = this.localeRegistry.getLocale(locale);
        return localeInfo.formatCurrency(amount, currency);
    }

    formatDate(date, format = DateFormat.SHORT, localeCode = null) {
        const locale = localeCode || this.currentLocale;
        const localeInfo = this.localeRegistry.getLocale(locale);
        return localeInfo.formatDate(date, format);
    }

    formatTime(date, localeCode = null) {
        const locale = localeCode || this.currentLocale;
        const localeInfo = this.localeRegistry.getLocale(locale);
        return localeInfo.formatTime(date);
    }

    formatRelativeTime(date, localeCode = null) {
        const locale = localeCode || this.currentLocale;
        const localeInfo = this.localeRegistry.getLocale(locale);
        return localeInfo.formatRelativeTime(date);
    }

    getPluralForm(count, localeCode = null) {
        const locale = localeCode || this.currentLocale;
        const localeInfo = this.localeRegistry.getLocale(locale);
        return localeInfo.getPluralForm(count);
    }

    isRTL(localeCode = null) {
        const locale = localeCode || this.currentLocale;
        const localeInfo = this.localeRegistry.getLocale(locale);
        return localeInfo.direction === LanguageDirection.RTL;
    }

    getTextDirection(localeCode = null) {
        const locale = localeCode || this.currentLocale;
        const localeInfo = this.localeRegistry.getLocale(locale);
        return localeInfo.direction;
    }

    getAllLocales() {
        return this.localeRegistry.getAllLocales();
    }

    searchLocales(query) {
        return this.localeRegistry.searchLocales(query);
    }

    getCategory(name) {
        return this.database.getCategory(name);
    }

    getAllCategories() {
        return this.database.getAllCategories();
    }

    addCategory(category) {
        this.database.addCategory(category);
        this.clearCache();
    }

    clearCache() {
        this.cache.clear();
    }

    disableCache() {
        this.cacheEnabled = false;
        this.clearCache();
    }

    enableCache() {
        this.cacheEnabled = true;
    }

    getMissingKeys() {
        return Array.from(this.missingKeys);
    }

    exportDatabase() {
        return this.database.export();
    }

    importDatabase(data) {
        this.database.import(data);
        this.clearCache();
    }

    on(event, callback) {
        if (!this.listeners.has(event)) {
            this.listeners.set(event, []);
        }
        this.listeners.get(event).push(callback);
    }

    off(event, callback) {
        if (!this.listeners.has(event)) return;
        const callbacks = this.listeners.get(event);
        const index = callbacks.indexOf(callback);
        if (index > -1) {
            callbacks.splice(index, 1);
        }
    }

    emit(event, data) {
        if (!this.listeners.has(event)) return;
        for (const callback of this.listeners.get(event)) {
            try {
                callback(data);
            } catch (e) {
                console.error(`Error in locale listener: ${e}`);
            }
        }
    }

    detectSystemLanguage() {
        const systemLang = navigator.language || navigator.userLanguage;
        const localeMap = {
            'en': SupportedLanguages.ENGLISH,
            'en-US': SupportedLanguages.ENGLISH,
            'en-GB': SupportedLanguages.ENGLISH,
            'es': SupportedLanguages.SPANISH,
            'es-ES': SupportedLanguages.SPANISH,
            'es-MX': SupportedLanguages.SPANISH,
            'fr': SupportedLanguages.FRENCH,
            'fr-FR': SupportedLanguages.FRENCH,
            'de': SupportedLanguages.GERMAN,
            'de-DE': SupportedLanguages.GERMAN,
            'it': SupportedLanguages.ITALIAN,
            'it-IT': SupportedLanguages.ITALIAN,
            'pt': SupportedLanguages.PORTUGUESE,
            'pt-BR': SupportedLanguages.PORTUGUESE,
            'pt-PT': SupportedLanguages.PORTUGUESE,
            'ru': SupportedLanguages.RUSSIAN,
            'ru-RU': SupportedLanguages.RUSSIAN,
            'pl': SupportedLanguages.POLISH,
            'pl-PL': SupportedLanguages.POLISH,
            'tr': SupportedLanguages.TURKISH,
            'tr-TR': SupportedLanguages.TURKISH,
            'ko': SupportedLanguages.KOREAN,
            'ko-KR': SupportedLanguages.KOREAN,
            'ja': SupportedLanguages.JAPANESE,
            'ja-JP': SupportedLanguages.JAPANESE,
            'zh': SupportedLanguages.CHINESE_SIMPLIFIED,
            'zh-CN': SupportedLanguages.CHINESE_SIMPLIFIED,
            'zh-TW': SupportedLanguages.CHINESE_TRADITIONAL,
            'ar': SupportedLanguages.ARABIC,
            'ar-SA': SupportedLanguages.ARABIC,
            'hi': SupportedLanguages.HINDI,
            'hi-IN': SupportedLanguages.HINDI,
            'th': SupportedLanguages.THAI,
            'th-TH': SupportedLanguages.THAI,
            'vi': SupportedLanguages.VIETNAMESE,
            'vi-VN': SupportedLanguages.VIETNAMESE,
            'id': SupportedLanguages.INDONESIAN,
            'id-ID': SupportedLanguages.INDONESIAN,
            'nl': SupportedLanguages.DUTCH,
            'nl-NL': SupportedLanguages.DUTCH,
            'sv': SupportedLanguages.SWEDISH,
            'sv-SE': SupportedLanguages.SWEDISH,
            'no': SupportedLanguages.NORWEGIAN,
            'no-NO': SupportedLanguages.NORWEGIAN,
            'da': SupportedLanguages.DANISH,
            'da-DK': SupportedLanguages.DANISH,
            'fi': SupportedLanguages.FINNISH,
            'fi-FI': SupportedLanguages.FINNISH,
            'cs': SupportedLanguages.CZECH,
            'cs-CZ': SupportedLanguages.CZECH,
            'hu': SupportedLanguages.HUNGARIAN,
            'hu-HU': SupportedLanguages.HUNGARIAN,
            'ro': SupportedLanguages.ROMANIAN,
            'ro-RO': SupportedLanguages.ROMANIAN,
            'el': SupportedLanguages.GREEK,
            'el-GR': SupportedLanguages.GREEK,
            'uk': SupportedLanguages.UKRAINIAN,
            'uk-UA': SupportedLanguages.UKRAINIAN,
            'he': SupportedLanguages.HEBREW,
            'he-IL': SupportedLanguages.HEBREW
        };
        return localeMap[systemLang] || SupportedLanguages.ENGLISH;
    }
}

export const Localization = LocalizationManager.getInstance();
