import { GAME_CONFIG } from './config.js';

export const CONVAR_TYPE = {
    BOOL: 0,
    INT: 1,
    FLOAT: 2,
    STRING: 3,
    COLOR: 4,
    VECTOR2: 5,
    VECTOR3: 6
};

export const CONVAR_FLAG = {
    NONE: 0,
    ARCHIVE: 1 << 0,
    SPONLY: 1 << 1,
    NOTIFY: 1 << 2,
    RECHECK: 1 << 3,
    LATCH: 1 << 4,
    STEALTH: 1 << 5,
    HAS_MIN: 1 << 6,
    HAS_MAX: 1 << 7,
    EXPERT: 1 << 8,
    REGISTERED: 1 << 9,
    CHEAT: 1 << 10,
    RELOADED: 1 << 11,
    NOT_CONNECTED: 1 << 12,
    SERVER: 1 << 13,
    CLIENT: 1 << 14,
    PROTECTED: 1 << 15
};

export const CONSOLE_COLOR = {
    DEFAULT: '#FFFFFF',
    WHITE: '#FFFFFF',
    BLACK: '#000000',
    RED: '#FF0000',
    GREEN: '#00FF00',
    BLUE: '#0000FF',
    YELLOW: '#FFFF00',
    CYAN: '#00FFFF',
    MAGENTA: '#FF00FF',
    GRAY: '#808080',
    LIGHT_RED: '#FF6666',
    LIGHT_GREEN: '#66FF66',
    LIGHT_BLUE: '#6666FF',
    GOLD: '#FFD700'
};

export class ConCommand {
    constructor(name, callback, helpText = '', flags = 0) {
        this.name = name;
        this.callback = callback;
        this.helpText = helpText;
        this.flags = flags;
        this.aliases = [];
        this.argDescriptions = [];
        this.completionCallback = null;
        this.usage = '';
    }
    
    addAlias(alias) {
        this.aliases.push(alias);
        return this;
    }
    
    addArgDescription(description) {
        this.argDescriptions.push(description);
        return this;
    }
    
    setCompletionCallback(callback) {
        this.completionCallback = callback;
        return this;
    }
    
    setUsage(usage) {
        this.usage = usage;
        return this;
    }
    
    execute(args = []) {
        if (this.callback) {
            try {
                return this.callback(args);
            } catch (error) {
                console.error(`Error executing command ${this.name}:`, error);
                return { success: false, error: error.message };
            }
        }
        return { success: false, error: 'No callback defined' };
    }
    
    getHelpText() {
        let text = this.helpText;
        
        if (this.argDescriptions.length > 0) {
            text += '\nUsage: ' + this.name + ' ';
            for (const argDesc of this.argDescriptions) {
                text += `<${argDesc}> `;
            }
        }
        
        if (this.aliases.length > 0) {
            text += '\nAliases: ' + this.aliases.join(', ');
        }
        
        return text;
    }
    
    getCompletionSuggestions(input) {
        if (this.completionCallback) {
            return this.completionCallback(input);
        }
        return [];
    }
}

export class ConVar {
    constructor(name, defaultValue, flags = 0, helpText = '') {
        this.name = name;
        this.defaultValue = defaultValue;
        this.currentValue = defaultValue;
        this.flags = flags;
        this.helpText = helpText;
        this.minValue = null;
        this.maxValue = null;
        this.onChangeCallback = null;
        this.modified = false;
        this.isArray = Array.isArray(defaultValue);
    }
    
    getType() {
        if (typeof this.currentValue === 'boolean') {
            return CONVAR_TYPE.BOOL;
        } else if (typeof this.currentValue === 'number') {
            if (Number.isInteger(this.currentValue)) {
                return CONVAR_TYPE.INT;
            }
            return CONVAR_TYPE.FLOAT;
        } else if (typeof this.currentValue === 'string') {
            return CONVAR_TYPE.STRING;
        }
        return CONVAR_TYPE.STRING;
    }
    
    setValue(value) {
        const oldValue = this.currentValue;
        
        if (this.minValue !== null && value < this.minValue) {
            value = this.minValue;
        }
        
        if (this.maxValue !== null && value > this.maxValue) {
            value = this.maxValue;
        }
        
        this.currentValue = value;
        this.modified = true;
        
        if (this.onChangeCallback) {
            this.onChangeCallback(this.currentValue, oldValue);
        }
        
        if (this.flags & CONVAR_FLAG.NOTIFY) {
            console.log(`CVar ${this.name} changed from ${oldValue} to ${this.currentValue}`);
        }
        
        return this;
    }
    
    reset() {
        this.currentValue = this.defaultValue;
        this.modified = false;
    }
    
    revert() {
        this.currentValue = this.defaultValue;
        this.modified = false;
    }
    
    hasMin() {
        return this.minValue !== null;
    }
    
    hasMax() {
        return this.maxValue !== null;
    }
    
    setMin(minValue) {
        this.minValue = minValue;
        this.flags |= CONVAR_FLAG.HAS_MIN;
        return this;
    }
    
    setMax(maxValue) {
        this.maxValue = maxValue;
        this.flags |= CONVAR_FLAG.HAS_MAX;
        return this;
    }
    
    setOnChange(callback) {
        this.onChangeCallback = callback;
        return this;
    }
    
    getBool() {
        if (typeof this.currentValue === 'boolean') {
            return this.currentValue;
        }
        return !!parseInt(this.currentValue);
    }
    
    getInt() {
        return parseInt(this.currentValue);
    }
    
    getFloat() {
        return parseFloat(this.currentValue);
    }
    
    getString() {
        return String(this.currentValue);
    }
    
    getColor() {
        if (this.currentValue instanceof Array && this.currentValue.length >= 4) {
            return {
                r: this.currentValue[0],
                g: this.currentValue[1],
                b: this.currentValue[2],
                a: this.currentValue[3]
            };
        }
        return { r: 255, g: 255, b: 255, a: 255 };
    }
    
    getVector2() {
        if (this.currentValue instanceof Array && this.currentValue.length >= 2) {
            return { x: this.currentValue[0], y: this.currentValue[1] };
        }
        return { x: 0, y: 0 };
    }
    
    getVector3() {
        if (this.currentValue instanceof Array && this.currentValue.length >= 3) {
            return { x: this.currentValue[0], y: this.currentValue[1], z: this.currentValue[2] };
        }
        return { x: 0, y: 0, z: 0 };
    }
    
    toString() {
        if (this.currentValue instanceof Array) {
            return this.currentValue.join(' ');
        }
        return String(this.currentValue);
    }
    
    isDefault() {
        return this.currentValue === this.defaultValue;
    }
    
    isModified() {
        return this.modified;
    }
    
    toJSON() {
        return {
            name: this.name,
            value: this.currentValue,
            defaultValue: this.defaultValue,
            flags: this.flags,
            modified: this.modified
        };
    }
}

export class ConCommandManager {
    constructor() {
        this.commands = new Map();
        this.aliases = new Map();
        this.commandHistory = [];
        this.maxHistorySize = 100;
        this.autoCompleteEnabled = true;
        this.caseSensitive = false;
    }
    
    register(name, callback, helpText = '', flags = 0) {
        const command = new ConCommand(name, callback, helpText, flags);
        this.commands.set(name.toLowerCase(), command);
        return command;
    }
    
    unregister(name) {
        const command = this.commands.get(name.toLowerCase());
        if (command) {
            for (const alias of command.aliases) {
                this.aliases.delete(alias.toLowerCase());
            }
        }
        return this.commands.delete(name.toLowerCase());
    }
    
    registerAlias(alias, commandName) {
        this.aliases.set(alias.toLowerCase(), commandName.toLowerCase());
    }
    
    removeAlias(alias) {
        return this.aliases.delete(alias.toLowerCase());
    }
    
    findCommand(name) {
        const lowerName = name.toLowerCase();
        
        let command = this.commands.get(lowerName);
        if (command) return command;
        
        const aliasTarget = this.aliases.get(lowerName);
        if (aliasTarget) {
            command = this.commands.get(aliasTarget);
        }
        
        return command || null;
    }
    
    execute(input) {
        const trimmed = input.trim();
        if (!trimmed) {
            return { success: false, error: 'Empty command' };
        }
        
        const parts = this.parseCommand(trimmed);
        if (parts.length === 0) {
            return { success: false, error: 'Invalid command format' };
        }
        
        const commandName = parts[0];
        const args = parts.slice(1);
        
        const command = this.findCommand(commandName);
        if (!command) {
            return { success: false, error: `Unknown command: ${commandName}` };
        }
        
        this.addToHistory(trimmed);
        
        return command.execute(args);
    }
    
    parseCommand(input) {
        const parts = [];
        let current = '';
        let inQuotes = false;
        let quoteChar = '';
        
        for (let i = 0; i < input.length; i++) {
            const char = input[i];
            
            if ((char === '"' || char === "'") && !inQuotes) {
                inQuotes = true;
                quoteChar = char;
            } else if (char === quoteChar && inQuotes) {
                inQuotes = false;
                quoteChar = '';
            } else if (char === ' ' && !inQuotes) {
                if (current.length > 0) {
                    parts.push(current);
                    current = '';
                }
            } else {
                current += char;
            }
        }
        
        if (current.length > 0) {
            parts.push(current);
        }
        
        return parts;
    }
    
    addToHistory(command) {
        if (this.commandHistory.length > 0 && this.commandHistory[0] === command) {
            return;
        }
        
        this.commandHistory.unshift(command);
        
        if (this.commandHistory.length > this.maxHistorySize) {
            this.commandHistory.pop();
        }
    }
    
    getHistory(index = -1) {
        if (index < 0) {
            return [...this.commandHistory];
        }
        return this.commandHistory[index] || null;
    }
    
    clearHistory() {
        this.commandHistory = [];
    }
    
    getCommands() {
        return Array.from(this.commands.values());
    }
    
    searchCommands(query) {
        const lowerQuery = query.toLowerCase();
        const results = [];
        
        for (const command of this.commands.values()) {
            if (command.name.toLowerCase().includes(lowerQuery)) {
                results.push(command);
                continue;
            }
            
            for (const alias of command.aliases) {
                if (alias.toLowerCase().includes(lowerQuery)) {
                    results.push(command);
                    break;
                }
            }
        }
        
        return results;
    }
    
    getAutoCompleteSuggestions(input) {
        if (!this.autoCompleteEnabled || !input) {
            return [];
        }
        
        const lowerInput = input.toLowerCase();
        const suggestions = [];
        
        for (const command of this.commands.values()) {
            if (command.name.toLowerCase().startsWith(lowerInput)) {
                suggestions.push({
                    command: command,
                    display: command.name,
                    type: 'command'
                });
            }
            
            for (const alias of command.aliases) {
                if (alias.toLowerCase().startsWith(lowerInput)) {
                    suggestions.push({
                        command: command,
                        display: alias,
                        type: 'alias'
                    });
                }
            }
        }
        
        return suggestions.slice(0, 20);
    }
}

export class ConsoleVariableManager {
    constructor() {
        this.variables = new Map();
        this.modifiedVars = [];
        this.callbacks = new Map();
    }
    
    register(name, defaultValue, flags = 0, helpText = '') {
        const convar = new ConVar(name, defaultValue, flags, helpText);
        this.variables.set(name.toLowerCase(), convar);
        return convar;
    }
    
    unregister(name) {
        return this.variables.delete(name.toLowerCase());
    }
    
    find(name) {
        return this.variables.get(name.toLowerCase()) || null;
    }
    
    set(name, value) {
        const convar = this.find(name);
        if (!convar) {
            console.warn(`ConVar ${name} not found`);
            return null;
        }
        
        if (typeof value === 'string') {
            const type = convar.getType();
            
            switch (type) {
                case CONVAR_TYPE.BOOL:
                    value = value === '1' || value === 'true' || value === 'on';
                    break;
                case CONVAR_TYPE.INT:
                    value = parseInt(value);
                    if (isNaN(value)) value = 0;
                    break;
                case CONVAR_TYPE.FLOAT:
                    value = parseFloat(value);
                    if (isNaN(value)) value = 0;
                    break;
            }
        }
        
        convar.setValue(value);
        
        if (convar.isModified() && !this.modifiedVars.includes(name)) {
            this.modifiedVars.push(name);
        }
        
        return convar;
    }
    
    get(name) {
        const convar = this.find(name);
        return convar ? convar.currentValue : null;
    }
    
    getBool(name) {
        const convar = this.find(name);
        return convar ? convar.getBool() : false;
    }
    
    getInt(name) {
        const convar = this.find(name);
        return convar ? convar.getInt() : 0;
    }
    
    getFloat(name) {
        const convar = this.find(name);
        return convar ? convar.getFloat() : 0;
    }
    
    getString(name) {
        const convar = this.find(name);
        return convar ? convar.getString() : '';
    }
    
    reset(name) {
        const convar = this.find(name);
        if (convar) {
            convar.reset();
            const index = this.modifiedVars.indexOf(name);
            if (index !== -1) {
                this.modifiedVars.splice(index, 1);
            }
        }
    }
    
    resetAll() {
        for (const convar of this.variables.values()) {
            convar.reset();
        }
        this.modifiedVars = [];
    }
    
    revert(name) {
        const convar = this.find(name);
        if (convar) {
            convar.revert();
            const index = this.modifiedVars.indexOf(name);
            if (index !== -1) {
                this.modifiedVars.splice(index, 1);
            }
        }
    }
    
    revertAll() {
        for (const convar of this.variables.values()) {
            convar.revert();
        }
        this.modifiedVars = [];
    }
    
    getVariables() {
        return Array.from(this.variables.values());
    }
    
    getModifiedVariables() {
        return this.modifiedVars.map(name => this.find(name)).filter(c => c !== null);
    }
    
    searchVariables(query, includeValues = false) {
        const lowerQuery = query.toLowerCase();
        const results = [];
        
        for (const convar of this.variables.values()) {
            if (convar.name.toLowerCase().includes(lowerQuery)) {
                results.push(convar);
                continue;
            }
            
            if (includeValues && String(convar.currentValue).toLowerCase().includes(lowerQuery)) {
                results.push(convar);
            }
        }
        
        return results;
    }
    
    addCallback(name, callback) {
        if (!this.callbacks.has(name)) {
            this.callbacks.set(name, []);
        }
        this.callbacks.get(name).push(callback);
    }
    
    removeCallback(name, callback) {
        const callbacks = this.callbacks.get(name);
        if (callbacks) {
            const index = callbacks.indexOf(callback);
            if (index !== -1) {
                callbacks.splice(index, 1);
            }
        }
    }
    
    triggerCallback(name, oldValue, newValue) {
        const callbacks = this.callbacks.get(name);
        if (callbacks) {
            for (const callback of callbacks) {
                try {
                    callback(newValue, oldValue);
                } catch (error) {
                    console.error(`Error in ConVar callback for ${name}:`, error);
                }
            }
        }
    }
    
    exportVariables(includeNonModified = false) {
        const exported = {};
        
        for (const convar of this.variables.values()) {
            if (!includeNonModified && !convar.isModified()) {
                continue;
            }
            
            if (!(convar.flags & CONVAR_FLAG.ARCHIVE)) {
                continue;
            }
            
            exported[convar.name] = convar.currentValue;
        }
        
        return exported;
    }
    
    importVariables(data) {
        for (const [name, value] of Object.entries(data)) {
            this.set(name, value);
        }
    }
}

export class InputBinding {
    constructor(key, command, modifiers = {}) {
        this.key = key;
        this.command = command;
        this.modifiers = modifiers;
        this.active = false;
        this.holdTime = 0;
        this.lastPressTime = 0;
        this.toggleState = false;
        this.isToggle = false;
    }
    
    matches(event) {
        if (event.key !== this.key) {
            return false;
        }
        
        if (this.modifiers.ctrl && !event.ctrlKey) {
            return false;
        }
        if (this.modifiers.alt && !event.altKey) {
            return false;
        }
        if (this.modifiers.shift && !event.shiftKey) {
            return false;
        }
        
        return true;
    }
    
    setActive(active) {
        this.active = active;
        if (active) {
            this.lastPressTime = Date.now();
            this.holdTime = 0;
            
            if (this.isToggle) {
                this.toggleState = !this.toggleState;
                return this.toggleState;
            }
        } else {
            this.holdTime = Date.now() - this.lastPressTime;
        }
        
        return true;
    }
    
    getHoldDuration() {
        if (this.active) {
            return Date.now() - this.lastPressTime;
        }
        return this.holdTime;
    }
}

export class InputBinder {
    constructor() {
        this.bindings = new Map();
        this.keyStates = new Map();
        this.actionStates = new Map();
        this.onBindingPressed = null;
        this.onBindingReleased = null;
        this.onBindingHeld = null;
        this.blockedKeys = new Set();
        this.heldBindings = new Set();
    }
    
    bind(key, command, modifiers = {}) {
        const binding = new InputBinding(key, command, modifiers);
        this.bindings.set(key, binding);
        return binding;
    }
    
    unbind(key) {
        return this.bindings.delete(key);
    }
    
    unbindAll() {
        this.bindings.clear();
    }
    
    findBinding(key) {
        return this.bindings.get(key) || null;
    }
    
    findBindingsByCommand(command) {
        const results = [];
        for (const binding of this.bindings.values()) {
            if (binding.command === command) {
                results.push(binding);
            }
        }
        return results;
    }
    
    handleKeyDown(event) {
        const key = event.key;
        
        if (this.blockedKeys.has(key)) {
            event.preventDefault();
            return false;
        }
        
        const binding = this.findBinding(key);
        if (!binding) {
            return false;
        }
        
        if (!binding.matches(event)) {
            return false;
        }
        
        this.keyStates.set(key, true);
        
        const shouldActivate = binding.setActive(true);
        
        if (shouldActivate) {
            this.heldBindings.add(key);
            
            if (this.onBindingPressed) {
                this.onBindingPressed(binding);
            }
        }
        
        event.preventDefault();
        return true;
    }
    
    handleKeyUp(event) {
        const key = event.key;
        
        const binding = this.findBinding(key);
        if (!binding) {
            return false;
        }
        
        this.keyStates.set(key, false);
        
        binding.setActive(false);
        this.heldBindings.delete(key);
        
        if (this.onBindingReleased) {
            this.onBindingReleased(binding);
        }
        
        event.preventDefault();
        return true;
    }
    
    isKeyDown(key) {
        return this.keyStates.get(key) || false;
    }
    
    isBindingDown(key) {
        const binding = this.findBinding(key);
        return binding && binding.active;
    }
    
    getHeldBindings() {
        return Array.from(this.heldBindings).map(key => this.bindings.get(key));
    }
    
    blockKey(key) {
        this.blockedKeys.add(key);
    }
    
    unblockKey(key) {
        this.blockedKeys.delete(key);
    }
    
    blockAll() {
        for (const key of this.bindings.keys()) {
            this.blockedKeys.add(key);
        }
    }
    
    unblockAll() {
        this.blockedKeys.clear();
    }
    
    setOnBindingPressed(callback) {
        this.onBindingPressed = callback;
    }
    
    setOnBindingReleased(callback) {
        this.onBindingReleased = callback;
    }
    
    setOnBindingHeld(callback) {
        this.onBindingHeld = callback;
    }
    
    update(deltaTime) {
        for (const key of this.heldBindings) {
            const binding = this.bindings.get(key);
            if (binding && this.onBindingHeld) {
                this.onBindingHeld(binding, deltaTime);
            }
        }
    }
    
    exportBindings() {
        const exported = {};
        
        for (const [key, binding] of this.bindings) {
            exported[key] = {
                command: binding.command,
                modifiers: binding.modifiers,
                isToggle: binding.isToggle
            };
        }
        
        return exported;
    }
    
    importBindings(data) {
        this.unbindAll();
        
        for (const [key, bindingData] of Object.entries(data)) {
            const binding = this.bind(key, bindingData.command, bindingData.modifiers || {});
            binding.isToggle = bindingData.isToggle || false;
        }
    }
}

export class Console {
    constructor() {
        this.commandManager = new ConCommandManager();
        this.variableManager = new ConsoleVariableManager();
        this.inputBinder = new InputBinder();
        this.lines = [];
        this.maxLines = 500;
        this.visible = false;
        this.inputBuffer = '';
        this.inputHistory = [];
        this.historyIndex = -1;
        this.scrollPosition = 0;
        this.autoCompleteIndex = 0;
        this.autoCompleteSuggestions = [];
        this.filterText = '';
        this.showTimestamps = true;
        this.textScale = 1.0;
        this.consoleColors = new Map();
        this.messageQueue = [];
        this.processingMessage = false;
    }
    
    initialize() {
        this.registerDefaultCommands();
        this.registerDefaultVariables();
        this.registerDefaultBindings();
    }
    
    registerDefaultCommands() {
        const cmds = this.commandManager;
        
        cmds.register('help', (args) => {
            if (args.length === 0) {
                this.print('Available commands:');
                this.print('Type "help <command>" for more info');
                return;
            }
            
            const command = this.commandManager.findCommand(args[0]);
            if (command) {
                this.print(command.getHelpText());
            } else {
                this.print(`Unknown command: ${args[0]}`);
            }
        }, 'Display help information', CONVAR_FLAG.NONE)
            .addArgDescription('command');
        
        cmds.register('echo', (args) => {
            this.print(args.join(' '));
        }, 'Print text to console', CONVAR_FLAG.NONE)
            .addArgDescription('text');
        
        cmds.register('clear', () => {
            this.clear();
        }, 'Clear the console', CONVAR_FLAG.NONE);
        
        cmds.register('quit', () => {
            this.print('Goodbye!');
            if (typeof window !== 'undefined') {
                window.close();
            }
        }, 'Quit the game', CONVAR_FLAG.NONE);
        
        cmds.register('exit', () => {
            this.print('Goodbye!');
        }, 'Exit the game', CONVAR_FLAG.NONE);
        
        cmds.register('exec', (args) => {
            if (args.length === 0) {
                this.print('Usage: exec <filename>');
                return;
            }
            this.print(`Executing config: ${args[0]}`);
        }, 'Execute a config file', CONVAR_FLAG.NONE)
            .addArgDescription('config');
        
        cmds.register('bind', (args) => {
            if (args.length < 2) {
                this.print('Usage: bind <key> <command> [modifiers]');
                return;
            }
            const key = args[0];
            const command = args.slice(1).join(' ');
            this.inputBinder.bind(key, command);
            this.print(`Bound ${key} to "${command}"`);
        }, 'Bind a key to a command', CONVAR_FLAG.NONE)
            .addArgDescription('key')
            .addArgDescription('command');
        
        cmds.register('unbind', (args) => {
            if (args.length === 0) {
                this.print('Usage: unbind <key>');
                return;
            }
            this.inputBinder.unbind(args[0]);
            this.print(`Unbound ${args[0]}`);
        }, 'Unbind a key', CONVAR_FLAG.NONE)
            .addArgDescription('key');
        
        cmds.register('unbindall', () => {
            this.inputBinder.unbindAll();
            this.print('All bindings cleared');
        }, 'Unbind all keys', CONVAR_FLAG.NONE);
        
        cmds.register('bindlist', () => {
            const bindings = this.inputBinder.exportBindings();
            this.print('Current bindings:');
            for (const [key, data] of Object.entries(bindings)) {
                this.print(`  ${key}: ${data.command}`);
            }
        }, 'List all bindings', CONVAR_FLAG.NONE);
        
        cmds.register('toggle', (args) => {
            if (args.length === 0) {
                this.print('Usage: toggle <cvar>');
                return;
            }
            
            const cvar = this.variableManager.find(args[0]);
            if (!cvar) {
                this.print(`Unknown cvar: ${args[0]}`);
                return;
            }
            
            if (typeof cvar.currentValue === 'boolean') {
                cvar.setValue(!cvar.currentValue);
                this.print(`${args[0]} = ${cvar.currentValue}`);
            } else {
                this.print(`${args[0]} is not a boolean cvar`);
            }
        }, 'Toggle a boolean cvar', CONVAR_FLAG.NONE)
            .addArgDescription('cvar');
        
        cmds.register('set', (args) => {
            if (args.length < 2) {
                this.print('Usage: set <cvar> <value>');
                return;
            }
            
            const cvar = this.variableManager.set(args[0], args[1]);
            if (cvar) {
                this.print(`${args[0]} = ${cvar.currentValue}`);
            }
        }, 'Set a cvar value', CONVAR_FLAG.NONE)
            .addArgDescription('cvar')
            .addArgDescription('value');
        
        cmds.register('setraw', (args) => {
            if (args.length < 2) {
                this.print('Usage: setraw <cvar> <value>');
                return;
            }
            
            const cvar = this.variableManager.find(args[0]);
            if (cvar) {
                cvar.currentValue = args[1];
                this.print(`${args[0]} = ${cvar.currentValue}`);
            }
        }, 'Set a cvar value without parsing', CONVAR_FLAG.CHEAT)
            .addArgDescription('cvar')
            .addArgDescription('value');
        
        cmds.register('cvarlist', () => {
            const cvars = this.variableManager.getVariables();
            this.print(`CVar list (${cvars.length} variables):`);
            for (const cvar of cvars) {
                const flagStr = this.getFlagString(cvar.flags);
                this.print(`  ${cvar.name} = "${cvar.currentValue}" ${flagStr}`);
            }
        }, 'List all cvars', CONVAR_FLAG.NONE);
        
        cmds.register('find', (args) => {
            if (args.length === 0) {
                this.print('Usage: find <string>');
                return;
            }
            
            const results = this.variableManager.searchVariables(args[0], true);
            this.print(`Found ${results.length} matches:`);
            for (const cvar of results.slice(0, 20)) {
                this.print(`  ${cvar.name} = "${cvar.currentValue}"`);
            }
        }, 'Search for cvars', CONVAR_FLAG.NONE)
            .addArgDescription('string');
        
        cmds.register('findcmd', (args) => {
            if (args.length === 0) {
                this.print('Usage: findcmd <string>');
                return;
            }
            
            const results = this.commandManager.searchCommands(args[0]);
            this.print(`Found ${results.length} matches:`);
            for (const cmd of results.slice(0, 20)) {
                this.print(`  ${cmd.name}`);
            }
        }, 'Search for commands', CONVAR_FLAG.NONE)
            .addArgDescription('string');
        
        cmds.register('con_enable', (args) => {
            this.visible = !this.visible;
            this.print(`Console ${this.visible ? 'enabled' : 'disabled'}`);
        }, 'Toggle console visibility', CONVAR_FLAG.NONE);
        
        cmds.register('say', (args) => {
            this.print(`[You]: ${args.join(' ')}`);
        }, 'Send a chat message', CONVAR_FLAG.NONE)
            .addArgDescription('message');
        
        cmds.register('say_team', (args) => {
            this.print(`[Team]: ${args.join(' ')}`);
        }, 'Send a team chat message', CONVAR_FLAG.NONE)
            .addArgDescription('message');
        
        cmds.register('version', () => {
            this.print('Counter-Strike Game Engine v1.0.0');
        }, 'Display version information', CONVAR_FLAG.NONE);
        
        cmds.register('cmdlist', () => {
            const commands = this.commandManager.getCommands();
            this.print(`Command list (${commands.length} commands):`);
            for (const cmd of commands) {
                this.print(`  ${cmd.name}`);
            }
        }, 'List all commands', CONVAR_FLAG.NONE);
        
        cmds.register('wait', () => {
            this.print('Wait command executed');
        }, 'Wait for a number of frames', CONVAR_FLAG.NONE);
        
        cmds.register('incrementvar', (args) => {
            if (args.length < 4) {
                this.print('Usage: incrementvar <cvar> <min> <max> <delta>');
                return;
            }
            
            const cvar = this.variableManager.find(args[0]);
            if (!cvar || typeof cvar.currentValue !== 'number') {
                this.print(`Invalid numeric cvar: ${args[0]}`);
                return;
            }
            
            const min = parseFloat(args[1]);
            const max = parseFloat(args[2]);
            const delta = parseFloat(args[3]);
            
            let newValue = cvar.currentValue + delta;
            if (newValue > max) newValue = min;
            if (newValue < min) newValue = max;
            
            cvar.setValue(newValue);
            this.print(`${args[0]} = ${newValue}`);
        }, 'Increment a cvar value', CONVAR_FLAG.NONE)
            .addArgDescription('cvar')
            .addArgDescription('min')
            .addArgDescription('max')
            .addArgDescription('delta');
    }
    
    registerDefaultVariables() {
        const vars = this.variableManager;
        
        vars.register('name', 'Player', CONVAR_FLAG.ARCHIVE, 'Player name')
            .setOnChange((newVal, oldVal) => {
                this.print(`Name changed from "${oldVal}" to "${newVal}"`);
            });
        
        vars.register('clan', '', CONVAR_FLAG.ARCHIVE, 'Clan tag');
        
        vars.register('rate', 786432, CONVAR_FLAG.ARCHIVE, 'Connection rate');
        
        vars.register('cl_updaterate', 128, CONVAR_FLAG.ARCHIVE, 'Client update rate')
            .setMin(10)
            .setMax(128);
        
        vars.register('cl_cmdrate', 128, CONVAR_FLAG.ARCHIVE, 'Client command rate')
            .setMin(10)
            .setMax(128);
        
        vars.register('fps_max', 400, CONVAR_FLAG.ARCHIVE, 'Maximum FPS')
            .setMin(10)
            .setMax(1000);
        
        vars.register('sensitivity', 3.0, CONVAR_FLAG.ARCHIVE, 'Mouse sensitivity')
            .setMin(0.1)
            .setMax(100);
        
        vars.register('zoom_sensitivity_ratio', 1.0, CONVAR_FLAG.ARCHIVE, 'Zoom sensitivity multiplier')
            .setMin(0.1)
            .setMax(10);
        
        vars.register('m_pitch', 0.022, CONVAR_FLAG.ARCHIVE, 'Mouse pitch')
            .setMin(-2)
            .setMax(2);
        
        vars.register('m_yaw', 0.022, CONVAR_FLAG.ARCHIVE, 'Mouse yaw')
            .setMin(-2)
            .setMax(2);
        
        vars.register('volume', 0.5, CONVAR_FLAG.ARCHIVE, 'Audio volume')
            .setMin(0)
            .setMax(1);
        
        vars.register('voice_scale', 1.0, CONVAR_FLAG.ARCHIVE, 'Voice volume scale')
            .setMin(0)
            .setMax(1);
        
        vars.register('voice_enable', true, CONVAR_FLAG.ARCHIVE, 'Enable voice chat');
        
        vars.register('cl_showfps', false, CONVAR_FLAG.ARCHIVE, 'Show FPS counter');
        
        vars.register('cl_showpos', false, CONVAR_FLAG.ARCHIVE, 'Show position');
        
        vars.register('cl_showdemoicons', true, CONVAR_FLAG.ARCHIVE, 'Show demo icons');
        
        vars.register('cl_radar_scale', 0.7, CONVAR_FLAG.ARCHIVE, 'Radar scale')
            .setMin(0.25)
            .setMax(1);
        
        vars.register('cl_radar_rotate', true, CONVAR_FLAG.ARCHIVE, 'Rotate radar');
        
        vars.register('cl_hud_color', [6, 174, 244, 255], CONVAR_FLAG.ARCHIVE, 'HUD color');
        
        vars.register('cl_righthand', true, CONVAR_FLAG.ARCHIVE, 'Right-hand viewmodel');
        
        vars.register('viewmodel_offset_x', 0, CONVAR_FLAG.ARCHIVE, 'Viewmodel X offset')
            .setMin(-10)
            .setMax(10);
        
        vars.register('viewmodel_offset_y', 0, CONVAR_FLAG.ARCHIVE, 'Viewmodel Y offset')
            .setMin(-10)
            .setMax(10);
        
        vars.register('viewmodel_offset_z', 0, CONVAR_FLAG.ARCHIVE, 'Viewmodel Z offset')
            .setMin(-10)
            .setMax(10);
        
        vars.register('viewmodel_fov', 54, CONVAR_FLAG.ARCHIVE, 'Viewmodel FOV')
            .setMin(30)
            .setMax(90);
        
        vars.register('fov_cs_debug', 0, CONVAR_FLAG.CHEAT, 'Debug FOV');
        
        vars.register('mat_show_texture_memory', false, CONVAR_FLAG.CHEAT, 'Show texture memory');
        
        vars.register('mat_fillrate', false, CONVAR_FLAG.CHEAT, 'Show fillrate');
        
        vars.register('cl_globallight_shadowdirection', [0.5, 0.5, -1], CONVAR_FLAG.CHEAT, 'Global light shadow direction');
        
        vars.register('cl_cam_crosshair', false, CONVAR_FLAG.CHEAT, 'Show crosshair in camera mode');
        
        vars.register('sv_cheats', false, CONVAR_FLAG.SERVER | CONVAR_FLAG.NOTIFY, 'Enable server cheats');
        
        vars.register('host_timescale', 1.0, CONVAR_FLAG.CHEAT, 'Host timescale')
            .setMin(0.1)
            .setMax(10);
        
        vars.register('god', false, CONVAR_FLAG.CHEAT, 'God mode');
        
        vars.register('notarget', false, CONVAR_FLAG.CHEAT, 'Notarget mode');
        
        vars.register('noclip', false, CONVAR_FLAG.CHEAT, 'Noclip mode');
        
        vars.register('thirdperson', false, CONVAR_FLAG.CHEAT, 'Third person mode');
        
        vars.register('mat_wireframe', false, CONVAR_FLAG.CHEAT, 'Wireframe rendering');
        
        vars.register('r_drawothermodels', 1, CONVAR_FLAG.CHEAT, 'Draw other models')
            .setMin(1)
            .setMax(2);
        
        vars.register('net_graph', 0, CONVAR_FLAG.ARCHIVE, 'Net graph display')
            .setMin(0)
            .setMax(3);
        
        vars.register('net_graphpos', 1, CONVAR_FLAG.ARCHIVE, 'Net graph position')
            .setMin(1)
            .setMax(3);
        
        vars.register('con_color', [255, 255, 255, 255], CONVAR_FLAG.ARCHIVE, 'Console text color');
        
        vars.register('con_alpha', 0.6, CONVAR_FLAG.ARCHIVE, 'Console alpha')
            .setMin(0)
            .setMax(1);
        
        vars.register('con_scale', 1.0, CONVAR_FLAG.ARCHIVE, 'Console scale')
            .setMin(0.5)
            .setMax(2);
        
        vars.register('developer', 0, CONVAR_FLAG.NONE, 'Developer mode')
            .setMin(0)
            .setMax(2);
        
        vars.register('con_notifytime', 3, CONVAR_FLAG.ARCHIVE, 'Notification display time')
            .setMin(0)
            .setMax(10);
        
        vars.register('con_nprint_bgalpha', 0.5, CONVAR_FLAG.NONE, 'Notification background alpha')
            .setMin(0)
            .setMax(1);
        
        vars.register('con_times', 4, CONVAR_FLAG.NONE, 'Notification columns')
            .setMin(1)
            .setMax(8);
        
        vars.register('closecaption', true, CONVAR_FLAG.ARCHIVE, 'Show closed captions');
        
        vars.register('cc_linger_time', 1, CONVAR_FLAG.NONE, 'Caption linger time')
            .setMin(0)
            .setMax(10);
        
        vars.register('snd_musicvolume', 0.5, CONVAR_FLAG.ARCHIVE, 'Music volume')
            .setMin(0)
            .setMax(1);
        
        vars.register('snd_gamevolume', 0.7, CONVAR_FLAG.ARCHIVE, 'Game sound volume')
            .setMin(0)
            .setMax(1);
        
        vars.register('snd_mvpvolume', 0.7, CONVAR_FLAG.ARCHIVE, 'MVP sound volume')
            .setMin(0)
            .setMax(1);
        
        vars.register('voice_modenable', true, CONVAR_FLAG.ARCHIVE, 'Enable voice modulation');
        
        vars.register('voice_boost', false, CONVAR_FLAG.ARCHIVE, 'Enable voice boost');
        
        vars.register('dsp_enhance_stereo', true, CONVAR_FLAG.ARCHIVE, 'Enhance stereo sound');
        
        vars.register('dsp_spatial', 60, CONVAR_FLAG.ARCHIVE, 'DSP spatial blend')
            .setMin(0)
            .setMax(100);
        
        vars.register('dsp_volume', 1.0, CONVAR_FLAG.ARCHIVE, 'DSP volume')
            .setMin(0)
            .setMax(1);
        
        vars.register('hud_scaling', 0.85, CONVAR_FLAG.ARCHIVE, 'HUD scaling')
            .setMin(0.5)
            .setMax(1);
        
        vars.register('hud_showtargetid', true, CONVAR_FLAG.ARCHIVE, 'Show target ID');
        
        vars.register('hud_takesshots', false, CONVAR_FLAG.ARCHIVE, 'Take HUD screenshots');
        
        vars.register('cl_draw_only_deathnotices', false, CONVAR_FLAG.NONE, 'Draw only death notices');
        
        vars.register('cl_spec_showbindkeys', false, CONVAR_FLAG.NONE, 'Show spectator bind keys');
        
        vars.register('cl_autohelp', true, CONVAR_FLAG.ARCHIVE, 'Show auto help');
        
        vars.register('cl_showloadout', true, CONVAR_FLAG.ARCHIVE, 'Show loadout');
        
        vars.register('cl_teamfeed_toponly', false, CONVAR_FLAG.ARCHIVE, 'Show team feed only');
        
        vars.register('cl_deathnotices_left', false, CONVAR_FLAG.ARCHIVE, 'Death notices on left');
        
        vars.register('cl_deathnotices_righthand', true, CONVAR_FLAG.ARCHIVE, 'Death notices on right side');
        
        vars.register('cl_deathnotices_alpha', 1.0, CONVAR_FLAG.ARCHIVE, 'Death notices alpha')
            .setMin(0)
            .setMax(1);
        
        vars.register('cl_obsolete', false, CONVAR_FLAG.NONE, 'Obsolete cvar');
    }
    
    registerDefaultBindings() {
        const binder = this.inputBinder;
        
        binder.bind('Tab', '+showscores');
        binder.bind('Escape', 'cancelselect');
        binder.bind('Space', '+jump');
        binder.bind('W', '+forward');
        binder.bind('S', '+back');
        binder.bind('A', '+moveleft');
        binder.bind('D', '+moveright');
        binder.bind('Shift', '+speed');
        binder.bind('Ctrl', '+duck');
        binder.bind('Mouse1', '+attack');
        binder.bind('Mouse2', '+attack2');
        binder.bind('Mouse3', 'voice_toggle');
        binder.bind('Mouse4', 'buymenu');
        binder.bind('Mouse5', 'rebuy');
        binder.bind('R', '+reload');
        binder.bind('E', '+use');
        binder.bind('F', 'impulse 100');
        binder.bind('G', 'drop');
        binder.bind('T', 'drop');
        binder.bind('Q', 'lastinv');
        binder.bind('Z', 'radio1');
        binder.bind('X', 'radio2');
        binder.bind('C', 'radio3');
        binder.bind('B', 'buymenu');
        binder.bind('M', '+voicerecord');
        binder.bind('N', 'nightvision');
        binder.bind('V', '+voicerecord');
        binder.bind('1', 'slot1');
        binder.bind('2', 'slot2');
        binder.bind('3', 'slot3');
        binder.bind('4', 'slot4');
        binder.bind('5', 'slot5');
        binder.bind('6', 'slot6');
        binder.bind('7', 'slot7');
        binder.bind('8', 'slot8');
        binder.bind('9', 'slot9');
        binder.bind('0', 'slot10');
        binder.bind('`', 'toggleconsole');
        binder.bind('~', 'toggleconsole');
        binder.bind('-', 'sizeup');
        binder.bind('=', 'sizeup');
        binder.bind('[', 'prevslot');
        binder.bind(']', 'nextslot');
        binder.bind('\\', 'showbudget');
        binder.bind(',', '+left');
        binder.bind('.', '+right');
        binder.bind('/', 'zoomout');
        binder.bind('.', 'zoomin');
    }
    
    getFlagString(flags) {
        const flagStrings = [];
        
        if (flags & CONVAR_FLAG.ARCHIVE) flagStrings.push('ARCHIVE');
        if (flags & CONVAR_FLAG.SPONLY) flagStrings.push('SPONLY');
        if (flags & CONVAR_FLAG.NOTIFY) flagStrings.push('NOTIFY');
        if (flags & CONVAR_FLAG.CHEAT) flagStrings.push('CHEAT');
        if (flags & CONVAR_FLAG.SERVER) flagStrings.push('SERVER');
        if (flags & CONVAR_FLAG.CLIENT) flagStrings.push('CLIENT');
        if (flags & CONVAR_FLAG.HAS_MIN) flagStrings.push('HAS_MIN');
        if (flags & CONVAR_FLAG.HAS_MAX) flagStrings.push('HAS_MAX');
        
        return flagStrings.length > 0 ? `[${flagStrings.join('|')}]` : '';
    }
    
    show() {
        this.visible = true;
    }
    
    hide() {
        this.visible = false;
    }
    
    toggle() {
        this.visible = !this.visible;
    }
    
    print(text, color = CONSOLE_COLOR.DEFAULT) {
        const timestamp = this.showTimestamps ? this.getTimestamp() : '';
        
        this.lines.push({
            text: String(text),
            color: color,
            timestamp: timestamp,
            time: Date.now()
        });
        
        while (this.lines.length > this.maxLines) {
            this.lines.shift();
        }
    }
    
    printError(text) {
        this.print(`ERROR: ${text}`, CONSOLE_COLOR.RED);
    }
    
    printWarning(text) {
        this.print(`WARNING: ${text}`, CONSOLE_COLOR.YELLOW);
    }
    
    printSuccess(text) {
        this.print(`SUCCESS: ${text}`, CONSOLE_COLOR.GREEN);
    }
    
    printInfo(text) {
        this.print(`INFO: ${text}`, CONSOLE_COLOR.CYAN);
    }
    
    clear() {
        this.lines = [];
    }
    
    getTimestamp() {
        const date = new Date();
        const hours = String(date.getHours()).padStart(2, '0');
        const mins = String(date.getMinutes()).padStart(2, '0');
        const secs = String(date.getSeconds()).padStart(2, '0');
        return `${hours}:${mins}:${secs}`;
    }
    
    execute(input) {
        if (!input || input.trim().length === 0) {
            return;
        }
        
        this.inputHistory.unshift(input);
        if (this.inputHistory.length > 100) {
            this.inputHistory.pop();
        }
        
        this.historyIndex = -1;
        
        this.print(`> ${input}`, CONSOLE_COLOR.GRAY);
        
        const result = this.commandManager.execute(input);
        
        if (!result.success && result.error) {
            this.printError(result.error);
        }
    }
    
    handleInput(event) {
        switch (event.key) {
            case 'Enter':
            case 'Return':
                if (this.inputBuffer.length > 0) {
                    this.execute(this.inputBuffer);
                    this.inputBuffer = '';
                }
                break;
                
            case 'Backspace':
                this.inputBuffer = this.inputBuffer.slice(0, -1);
                break;
                
            case 'Delete':
                break;
                
            case 'ArrowUp':
                if (this.historyIndex < this.inputHistory.length - 1) {
                    this.historyIndex++;
                    this.inputBuffer = this.inputHistory[this.historyIndex] || '';
                }
                break;
                
            case 'ArrowDown':
                if (this.historyIndex > 0) {
                    this.historyIndex--;
                    this.inputBuffer = this.inputHistory[this.historyIndex] || '';
                } else if (this.historyIndex === 0) {
                    this.historyIndex = -1;
                    this.inputBuffer = '';
                }
                break;
                
            case 'ArrowLeft':
                break;
                
            case 'ArrowRight':
                this.handleAutoComplete();
                break;
                
            case 'Tab':
                this.autoComplete();
                event.preventDefault();
                break;
                
            case 'PageUp':
                this.scrollPosition = Math.min(this.scrollPosition + 10, this.lines.length);
                break;
                
            case 'PageDown':
                this.scrollPosition = Math.max(this.scrollPosition - 10, 0);
                break;
                
            case 'Home':
                this.scrollPosition = 0;
                break;
                
            case 'End':
                this.scrollPosition = Math.max(0, this.lines.length - 1);
                break;
                
            case 'Escape':
                if (this.visible) {
                    this.hide();
                }
                break;
                
            default:
                if (event.key.length === 1 && !event.ctrlKey && !event.metaKey) {
                    this.inputBuffer += event.key;
                }
                break;
        }
        
        this.updateAutoComplete();
    }
    
    autoComplete() {
        if (this.inputBuffer.length === 0) {
            return;
        }
        
        const suggestions = this.commandManager.getAutoCompleteSuggestions(this.inputBuffer);
        
        if (suggestions.length === 0) {
            return;
        }
        
        if (suggestions.length === 1) {
            this.inputBuffer = suggestions[0].display + ' ';
        } else {
            this.print('Multiple matches:');
            for (const suggestion of suggestions.slice(0, 10)) {
                this.print(`  ${suggestion.display}`, CONSOLE_COLOR.GRAY);
            }
        }
    }
    
    updateAutoComplete() {
        if (this.inputBuffer.length > 0) {
            this.autoCompleteSuggestions = this.commandManager.getAutoCompleteSuggestions(this.inputBuffer);
        } else {
            this.autoCompleteSuggestions = [];
        }
        this.autoCompleteIndex = 0;
    }
    
    handleAutoComplete() {
    }
    
    scrollUp() {
        this.scrollPosition = Math.min(this.scrollPosition + 3, this.lines.length);
    }
    
    scrollDown() {
        this.scrollPosition = Math.max(this.scrollPosition - 3, 0);
    }
    
    setFilter(text) {
        this.filterText = text;
    }
    
    getFilteredLines() {
        if (!this.filterText) {
            return this.lines;
        }
        
        const filterLower = this.filterText.toLowerCase();
        return this.lines.filter(line => 
            line.text.toLowerCase().includes(filterLower)
        );
    }
    
    registerCommand(name, callback, helpText = '', flags = 0) {
        return this.commandManager.register(name, callback, helpText, flags);
    }
    
    registerVariable(name, defaultValue, flags = 0, helpText = '') {
        return this.variableManager.register(name, defaultValue, flags, helpText);
    }
    
    registerAlias(alias, command) {
        this.commandManager.registerAlias(alias, command);
    }
    
    bindKey(key, command, modifiers = {}) {
        return this.inputBinder.bind(key, command, modifiers);
    }
    
    getCommandManager() {
        return this.commandManager;
    }
    
    getVariableManager() {
        return this.variableManager;
    }
    
    getInputBinder() {
        return this.inputBinder;
    }
    
    saveConfig(filename) {
        const config = {
            cvars: this.variableManager.exportVariables(true),
            bindings: this.inputBinder.exportBindings(),
            name: this.variableManager.get('name'),
            binds: []
        };
        
        return config;
    }
    
    loadConfig(config) {
        if (config.cvars) {
            this.variableManager.importVariables(config.cvars);
        }
        
        if (config.bindings) {
            this.inputBinder.importBindings(config.bindings);
        }
    }
    
    executeConfig(config) {
        for (const [name, value] of Object.entries(config.cvars || {})) {
            this.variableManager.set(name, value);
        }
        
        this.inputBinder.importBindings(config.bindings || {});
    }
}

export class ConsoleUI {
    constructor(console) {
        this.console = console;
        this.fontFamily = 'Consolas, monospace';
        this.fontSize = 14;
        this.lineHeight = 20;
        this.padding = 10;
        this.maxVisibleLines = 30;
        this.canvas = null;
        this.ctx = null;
        this.animationFrame = null;
        this.blinkTimer = 0;
        this.showCursor = true;
    }
    
    attachToCanvas(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
    }
    
    render() {
        if (!this.ctx) return;
        
        const ctx = this.ctx;
        const width = this.canvas.width;
        const height = this.canvas.height;
        
        ctx.fillStyle = 'rgba(0, 0, 0, 0.9)';
        ctx.fillRect(0, 0, width, height);
        
        ctx.strokeStyle = 'rgba(255, 255, 255, 0.3)';
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.moveTo(0, height * 0.7);
        ctx.lineTo(width, height * 0.7);
        ctx.stroke();
        
        this.renderLines(ctx);
        this.renderInput(ctx, width, height);
        
        this.blinkTimer += 16;
        if (this.blinkTimer > 500) {
            this.showCursor = !this.showCursor;
            this.blinkTimer = 0;
        }
    }
    
    renderLines(ctx) {
        ctx.font = `${this.fontSize}px ${this.fontFamily}`;
        const lines = this.console.getFilteredLines();
        
        const startIndex = Math.max(0, lines.length - this.maxVisibleLines + this.console.scrollPosition);
        const visibleLines = lines.slice(startIndex, startIndex + this.maxVisibleLines);
        
        for (let i = 0; i < visibleLines.length; i++) {
            const line = visibleLines[i];
            const y = this.padding + (i + 1) * this.lineHeight;
            
            if (line.timestamp) {
                ctx.fillStyle = 'rgba(100, 100, 100, 0.8)';
                ctx.fillText(line.timestamp, this.padding, y);
            }
            
            ctx.fillStyle = line.color;
            ctx.fillText(line.text, this.padding + 80, y);
        }
    }
    
    renderInput(ctx, width, height) {
        const inputY = height * 0.7 + this.padding;
        
        ctx.fillStyle = 'rgba(255, 255, 255, 0.8)';
        ctx.fillText('>', this.padding, inputY);
        
        let inputText = this.console.inputBuffer;
        if (this.showCursor) {
            inputText += '_';
        }
        
        ctx.fillStyle = '#FFFFFF';
        ctx.fillText(inputText, this.padding + 20, inputY);
    }
    
    startAnimation() {
        if (this.animationFrame) return;
        
        const animate = () => {
            if (this.console.visible) {
                this.render();
            }
            this.animationFrame = requestAnimationFrame(animate);
        };
        
        animate();
    }
    
    stopAnimation() {
        if (this.animationFrame) {
            cancelAnimationFrame(this.animationFrame);
            this.animationFrame = null;
        }
    }
}

export default {
    CONVAR_TYPE,
    CONVAR_FLAG,
    CONSOLE_COLOR,
    ConCommand,
    ConVar,
    ConCommandManager,
    ConsoleVariableManager,
    InputBinding,
    InputBinder,
    Console,
    ConsoleUI
};
